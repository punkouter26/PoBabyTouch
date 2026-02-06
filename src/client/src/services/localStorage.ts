/**
 * Client-side storage service for offline / API-down resilience.
 * Stores high scores, stats, and last player initials locally.
 */

import type { HighScore, GameStats } from '../types.ts';

const KEYS = {
  HIGH_SCORES: 'pobabytouch_highscores',
  PLAYER_INITIALS: 'pobabytouch_initials',
  GAME_STATS: 'pobabytouch_stats',
} as const;

function read<T>(key: string, fallback: T): T {
  try {
    const raw = localStorage.getItem(key);
    return raw ? (JSON.parse(raw) as T) : fallback;
  } catch {
    return fallback;
  }
}

function write<T>(key: string, value: T) {
  try {
    localStorage.setItem(key, JSON.stringify(value));
  } catch {
    console.warn('localStorage write failed');
  }
}

/* ── High Scores (local) ──────────────────────────────────── */

export function getLocalHighScores(): HighScore[] {
  return read<HighScore[]>(KEYS.HIGH_SCORES, []);
}

export function saveLocalHighScore(initials: string, score: number, gameMode = 'Default') {
  const scores = getLocalHighScores();
  scores.push({
    playerInitials: initials,
    score,
    gameMode,
    scoreDate: new Date().toISOString(),
    partitionKey: gameMode,
    rowKey: `${(999999 - score).toString().padStart(6, '0')}_${Date.now()}`,
    timestamp: new Date().toISOString(),
  });
  scores.sort((a, b) => b.score - a.score);
  write(KEYS.HIGH_SCORES, scores.slice(0, 20)); // keep top 20
}

/* ── Player Initials ──────────────────────────────────────── */

export function getStoredInitials(): string | null {
  return localStorage.getItem(KEYS.PLAYER_INITIALS);
}

export function storeInitials(initials: string) {
  localStorage.setItem(KEYS.PLAYER_INITIALS, initials.toUpperCase());
}

/* ── Local Stats ──────────────────────────────────────────── */

export function getLocalStats(): GameStats | null {
  return read<GameStats | null>(KEYS.GAME_STATS, null);
}

export function recordLocalGameSession(
  initials: string,
  score: number,
  circlesTapped: number,
  playtimeSeconds: number,
) {
  const existing = getLocalStats() ?? {
    initials: initials.toUpperCase(),
    totalGames: 0,
    totalCirclesTapped: 0,
    averageScore: 0,
    highestScore: 0,
    longestStreak: 0,
    totalPlaytimeSeconds: 0,
    lastPlayed: new Date().toISOString(),
    firstPlayed: new Date().toISOString(),
    percentileRank: 0,
    scoreDistribution: '',
  };

  existing.totalGames++;
  existing.totalCirclesTapped += circlesTapped;
  existing.totalPlaytimeSeconds += playtimeSeconds;
  existing.lastPlayed = new Date().toISOString();
  if (score > existing.highestScore) existing.highestScore = score;
  existing.averageScore =
    (existing.averageScore * (existing.totalGames - 1) + score) / existing.totalGames;

  write(KEYS.GAME_STATS, existing);
}
