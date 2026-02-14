import { useCallback, useEffect, useRef, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import type { GameCircle } from '../types';
import { standardEngine, babyEngine, type PhysicsEngine } from '../services/physics';
import { playColorSound, vibrate } from '../services/audio';
import { submitScore, recordGameSession } from '../services/apiClient';
import { saveLocalHighScore, storeInitials, recordLocalGameSession } from '../services/localStorage';
import styles from './Game.module.css';

/* ── Constants ────────────────────────────────────────────── */
const TOTAL_CIRCLES = 7;
const CIRCLE_RADIUS_PERCENT = 5;
const CIRCLE_REAPPEAR_DELAY_MS = 800;
const BASE_SPEED = 0.5;
const GAME_DURATION_SECONDS = 5;
const CIRCLE_COLORS = ['blue', 'green', 'red', 'purple'] as const;

/* ── Helper: build one circle ─────────────────────────────── */
function createCircle(id: number, w: number, h: number, radius: number): GameCircle {
  const color = CIRCLE_COLORS[Math.floor(Math.random() * CIRCLE_COLORS.length)];
  return {
    id,
    x: Math.random() * (w - 2 * radius) + radius,
    y: Math.random() * (h - 2 * radius) + radius,
    radius,
    velocityX: (Math.random() * 2 - 1) * BASE_SPEED,
    velocityY: (Math.random() * 2 - 1) * BASE_SPEED,
    isVisible: true,
    isHit: false,
    person: color,
    personClass: color,
    color,
  };
}

/* ── Helper: initialise non-overlapping circles ───────────── */
function initCircles(count: number, w: number, h: number, radius: number, engine: PhysicsEngine) {
  const circles: GameCircle[] = [];
  for (let i = 0; i < count; i++) {
    const c = createCircle(i, w, h, radius);
    let valid = false;
    let attempts = 0;
    while (!valid && attempts++ < 50) {
      c.x = Math.random() * (w - 2 * radius) + radius;
      c.y = Math.random() * (h - 2 * radius) + radius;
      valid = !circles.some(o => engine.isOverlapping(c, o));
    }
    if (valid) circles.push(c);
  }
  return circles;
}

/* ── Component ────────────────────────────────────────────── */
export default function Game() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const isBabyMode = searchParams.get('mode') === 'baby';
  const engine = isBabyMode ? babyEngine : standardEngine;

  /* state */
  const [score, setScore] = useState(0);
  const [timeRemaining, setTimeRemaining] = useState(GAME_DURATION_SECONDS);
  const [isGameOver, setIsGameOver] = useState(false);
  const [circles, setCircles] = useState<GameCircle[]>([]);
  const [showHighScoreModal, setShowHighScoreModal] = useState(false);
  const [showSuccessAnimation, setShowSuccessAnimation] = useState(false);
  const [playerInitials, setPlayerInitials] = useState('');

  /* refs */
  const gameAreaRef = useRef<HTMLDivElement>(null);
  const circlesRef = useRef<GameCircle[]>([]);
  const scoreRef = useRef(0);
  const timeRef = useRef(GAME_DURATION_SECONDS);
  const isActiveRef = useRef(false);
  const physicsRaf = useRef(0);
  const timerInterval = useRef(0);
  const dims = useRef({ w: 0, h: 0 });

  /* keep mutable refs in sync with state */
  circlesRef.current = circles;
  scoreRef.current = score;
  timeRef.current = timeRemaining;

  /* ── Physics loop (requestAnimationFrame) ───── */
  const physicsLoop = useCallback(() => {
    if (!isActiveRef.current) return;
    const { w, h } = dims.current;
    const speedMult = isBabyMode
      ? 1.0
      : 1.0 + ((GAME_DURATION_SECONDS - timeRef.current) / GAME_DURATION_SECONDS) * 0.5;

    engine.update(circlesRef.current, w, h, speedMult);

    // Force re-render with fresh array reference
    setCircles([...circlesRef.current]);
    physicsRaf.current = requestAnimationFrame(physicsLoop);
  }, [engine, isBabyMode]);

  /* ── Start game ─────────────────────────────── */
  const startGame = useCallback(() => {
    const el = gameAreaRef.current;
    if (!el) return;
    dims.current = { w: el.clientWidth, h: el.clientHeight };
    const radius = Math.max(15, Math.floor(dims.current.w * CIRCLE_RADIUS_PERCENT / 100));

    const newCircles = initCircles(TOTAL_CIRCLES, dims.current.w, dims.current.h, radius, engine);
    circlesRef.current = newCircles;
    setCircles(newCircles);
    setScore(0);
    scoreRef.current = 0;
    setTimeRemaining(GAME_DURATION_SECONDS);
    timeRef.current = GAME_DURATION_SECONDS;
    setIsGameOver(false);
    setShowHighScoreModal(false);
    setShowSuccessAnimation(false);
    setPlayerInitials('');
    isActiveRef.current = true;

    cancelAnimationFrame(physicsRaf.current);
    physicsRaf.current = requestAnimationFrame(physicsLoop);

    // Timer (skipped in baby mode)
    clearInterval(timerInterval.current);
    if (!isBabyMode) {
      timerInterval.current = window.setInterval(() => {
        timeRef.current--;
        setTimeRemaining(timeRef.current);
        if (timeRef.current <= 0) {
          endGame();
        }
      }, 1000);
    }
  }, [engine, isBabyMode, physicsLoop]);

  /* ── End game ───────────────────────────────── */
  const endGame = useCallback(() => {
    isActiveRef.current = false;
    cancelAnimationFrame(physicsRaf.current);
    clearInterval(timerInterval.current);
    setIsGameOver(true);
    setShowHighScoreModal(true);
  }, []);

  /* ── Init on mount ──────────────────────────── */
  useEffect(() => {
    // Small delay to let the DOM settle for measurements
    const tid = setTimeout(startGame, 100);
    return () => {
      clearTimeout(tid);
      isActiveRef.current = false;
      cancelAnimationFrame(physicsRaf.current);
      clearInterval(timerInterval.current);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  /* ── Handle circle click / touch ────────────── */
  const handleCircleClick = useCallback((e: React.MouseEvent | React.TouchEvent, circle: GameCircle) => {
    e.stopPropagation();
    if (!isActiveRef.current || !circle.isVisible) return;

    circle.isHit = true;
    circle.isVisible = false;

    if (!isBabyMode) {
      scoreRef.current++;
      setScore(scoreRef.current);
    }

    setCircles([...circlesRef.current]);

    playColorSound(circle.color);
    vibrate(50);

    // Respawn after delay
    setTimeout(() => {
      if (!isActiveRef.current) return;
      const { w, h } = dims.current;
      let valid = false;
      let attempts = 0;
      while (!valid && attempts++ < 50) {
        circle.x = Math.random() * (w - 2 * circle.radius) + circle.radius;
        circle.y = Math.random() * (h - 2 * circle.radius) + circle.radius;
        valid = !circlesRef.current.some(
          c => c.isVisible && c.id !== circle.id && engine.isOverlapping(circle, c),
        );
      }
      if (valid) {
        circle.velocityX = (Math.random() * 2 - 1) * BASE_SPEED;
        circle.velocityY = (Math.random() * 2 - 1) * BASE_SPEED;
        circle.isVisible = true;
        circle.isHit = false;
        setCircles([...circlesRef.current]);
      }
    }, CIRCLE_REAPPEAR_DELAY_MS);
  }, [engine, isBabyMode]);

  /* ── High-score initials input (auto-submit at 3 chars) ── */
  const handleInitialsInput = useCallback(async (raw: string) => {
    const filtered = raw
      .toUpperCase()
      .replace(/[^A-Z]/g, '')
      .slice(0, 3);
    setPlayerInitials(filtered);

    if (filtered.length === 3) {
      // Small delay for visual feedback before auto-submit
      await new Promise(r => setTimeout(r, 400));
      await submitHighScore(filtered);
    }
  }, []);

  /* ── Submit score to API + local fallback ──── */
  const submitHighScore = useCallback(async (initials: string) => {
    setShowSuccessAnimation(true);
    const finalScore = scoreRef.current;

    // Save locally for resilience
    saveLocalHighScore(initials, finalScore);
    storeInitials(initials);
    recordLocalGameSession(initials, finalScore, finalScore, GAME_DURATION_SECONDS);

    // Fire API calls (non-blocking, graceful degradation)
    submitScore({ playerInitials: initials, score: finalScore });
    recordGameSession({
      initials,
      score: finalScore,
      circlesTapped: finalScore,
      playtimeSeconds: GAME_DURATION_SECONDS,
    });

    await new Promise(r => setTimeout(r, 2000));
    navigate('/leader');
  }, [navigate]);

  const cancelHighScore = useCallback(() => {
    setShowHighScoreModal(false);
    setIsGameOver(true);
  }, []);

  const restartGame = useCallback(() => {
    setShowHighScoreModal(false);
    setShowSuccessAnimation(false);
    setPlayerInitials('');
    startGame();
  }, [startGame]);

  /* ── Render ─────────────────────────────────── */
  return (
    <div className={`${styles.gameContainer} ${isGameOver ? styles.gameOver : ''}`}>
      {/* Header */}
      <div className={styles.gameHeader}>
        {isBabyMode ? (
          <>
            <div className={styles.babyModeIndicator}>Baby Mode</div>
            <div className={styles.scoreDisplay}>Score: {score}</div>
            <div />
          </>
        ) : (
          <>
            <div className={styles.scoreDisplay}>Score: {score}</div>
            <div className={styles.timerDisplay}>Time: {timeRemaining}</div>
            <div />
          </>
        )}
      </div>

      {/* Game area */}
      <div ref={gameAreaRef} className={styles.gameArea}>
        {circles.map(c => (
          <div
            key={c.id}
            className={[
              styles.gameCircle,
              styles[`${c.personClass}Circle`],
              c.isVisible ? styles.appear : styles.disappear,
              c.isHit ? styles.hitEffect : '',
            ]
              .filter(Boolean)
              .join(' ')}
            style={{
              left: c.x,
              top: c.y,
              width: c.radius * 2,
              height: c.radius * 2,
            }}
            onClick={e => handleCircleClick(e, c)}
            onTouchEnd={e => { e.preventDefault(); handleCircleClick(e, c); }}
          />
        ))}
      </div>

      {/* Game-over overlay */}
      {isGameOver && !showHighScoreModal && (
        <div className={styles.gameOverOverlay}>
          <h2>Game Over!</h2>
          <p>Your final score: {score}</p>
          <button className={styles.restartButton} onClick={restartGame}>Play Again</button>
          <button className={styles.homeButton} onClick={() => navigate('/')}>Main Menu</button>
        </div>
      )}

      {/* High-score modal */}
      {showHighScoreModal && (
        <div className={styles.highScoreModal}>
          <div className={`${styles.highScoreContent} ${showSuccessAnimation ? styles.successAnimation : ''}`}>
            {!showSuccessAnimation ? (
              <>
                <h2>NEW HIGH SCORE!</h2>
                <p>Your score: <span className={styles.scoreValue}>{score}</span></p>
                <p>Enter your initials (3 letters):</p>
                <input
                  type="text"
                  maxLength={3}
                  value={playerInitials}
                  onChange={e => handleInitialsInput(e.target.value)}
                  placeholder="ABC"
                  className={styles.simpleInitialsInput}
                  autoFocus
                />
                <div className={styles.buttonGroup}>
                  <button className={styles.skipButton} onClick={cancelHighScore}>SKIP</button>
                </div>
              </>
            ) : (
              <div className={styles.successContent}>
                <div className={styles.successIcon}>🎉</div>
                <h2>Score Submitted!</h2>
                <p>Congratulations {playerInitials}!</p>
                <p>Your score of {score} has been saved.</p>
                <div className={styles.spinner} />
                <p className={styles.redirecting}>Redirecting to leaderboard...</p>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
