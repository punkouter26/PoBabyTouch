import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import type { GameStats } from '../types';
import { getPlayerStats } from '../services/apiClient';
import { getStoredInitials, getLocalStats } from '../services/localStorage';
import styles from './Stats.module.css';

interface ScoreDistEntry { range: string; count: number; }

function parseScoreDist(raw: string | undefined): ScoreDistEntry[] {
  if (!raw) return [];
  return raw.split(',').filter(Boolean).map(entry => {
    const [range, count] = entry.split(':');
    return { range, count: parseInt(count, 10) || 0 };
  });
}

function formatPlaytime(seconds: number): string {
  if (seconds >= 3600) return `${Math.floor(seconds / 3600)}h ${Math.floor((seconds % 3600) / 60)}m`;
  if (seconds >= 60) return `${Math.floor(seconds / 60)}m ${seconds % 60}s`;
  return `${seconds}s`;
}

export default function Stats() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [stats, setStats] = useState<GameStats | null>(null);
  const [scoreDist, setScoreDist] = useState<ScoreDistEntry[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const loadStats = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage(null);

    const initials = searchParams.get('initials') ?? getStoredInitials();
    if (!initials) {
      setErrorMessage('Please play a game first to track statistics');
      setIsLoading(false);
      return;
    }

    try {
      const remote = await getPlayerStats(initials);
      if (remote) {
        setStats(remote);
        setScoreDist(parseScoreDist(remote.scoreDistribution));
      } else {
        // Fallback to local stats
        const local = getLocalStats();
        if (local) {
          setStats(local);
          setScoreDist(parseScoreDist(local.scoreDistribution));
        } else {
          setErrorMessage('No statistics available yet. Play a game to start tracking!');
        }
      }
    } catch {
      const local = getLocalStats();
      if (local) {
        setStats(local);
        setScoreDist(parseScoreDist(local.scoreDistribution));
      } else {
        setErrorMessage('An error occurred while loading statistics');
      }
    } finally {
      setIsLoading(false);
    }
  }, [searchParams]);

  useEffect(() => { loadStats(); }, [loadStats]);

  const maxDistCount = Math.max(...scoreDist.map(d => d.count), 1);

  return (
    <div className={styles.container}>
      <h1>Game Statistics</h1>

      {isLoading && (
        <div className={styles.loading}>
          <div className={styles.spinner} />
          <p>Loading statistics...</p>
        </div>
      )}

      {!isLoading && errorMessage && (
        <div className={styles.message}>{errorMessage}</div>
      )}

      {!isLoading && stats && (
        <>
          {/* Summary cards */}
          <div className={styles.grid}>
            <div className={styles.statBox}>
              <div className={styles.statValue}>{stats.highestScore}</div>
              <div className={styles.statLabel}>Highest Score</div>
            </div>
            <div className={styles.statBox}>
              <div className={styles.statValue}>{stats.averageScore.toFixed(1)}</div>
              <div className={styles.statLabel}>Average Score</div>
            </div>
            <div className={styles.statBox}>
              <div className={styles.statValue}>{stats.totalGames}</div>
              <div className={styles.statLabel}>Games Played</div>
            </div>
            <div className={styles.statBox}>
              <div className={styles.statValue}>{formatPlaytime(stats.totalPlaytimeSeconds)}</div>
              <div className={styles.statLabel}>Total Playtime</div>
            </div>
          </div>

          {/* Percentile */}
          <div className={styles.percentile}>
            <h3>Your Rank</h3>
            <div className={styles.percentileRing}>
              <svg viewBox="0 0 36 36" className={styles.percentileSvg}>
                <path
                  className={styles.percentileBg}
                  d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
                  fill="none"
                  strokeWidth="3"
                />
                <path
                  className={styles.percentileFg}
                  d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
                  fill="none"
                  strokeWidth="3"
                  strokeDasharray={`${stats.percentileRank}, 100`}
                />
                <text x="18" y="20.5" className={styles.percentileText}>
                  {stats.percentileRank.toFixed(0)}%
                </text>
              </svg>
            </div>
            <p className={styles.percentileLabel}>
              Top {(100 - stats.percentileRank).toFixed(1)}% of players!
            </p>
          </div>

          {/* Score distribution */}
          {scoreDist.length > 0 && (
            <div className={styles.chartCard}>
              <h3>Score Distribution</h3>
              <div className={styles.barChart}>
                {scoreDist.map((d, i) => (
                  <div key={i} className={styles.barCol}>
                    <div
                      className={styles.bar}
                      style={{ height: `${(d.count / maxDistCount) * 100}%` }}
                    >
                      <span className={styles.barValue}>{d.count}</span>
                    </div>
                    <span className={styles.barLabel}>{d.range}</span>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Additional stats */}
          <div className={styles.additional}>
            <h3>Additional Statistics</h3>
            <div className={styles.statRow}>
              <span>Total Circles Tapped:</span>
              <span className={styles.statRowValue}>{stats.totalCirclesTapped}</span>
            </div>
            <div className={styles.statRow}>
              <span>First Played:</span>
              <span className={styles.statRowValue}>
                {new Date(stats.firstPlayed).toLocaleDateString()}
              </span>
            </div>
            <div className={styles.statRow}>
              <span>Last Played:</span>
              <span className={styles.statRowValue}>
                {new Date(stats.lastPlayed).toLocaleString()}
              </span>
            </div>
            <div className={styles.statRow}>
              <span>Avg Circles per Game:</span>
              <span className={styles.statRowValue}>
                {stats.totalGames > 0
                  ? (stats.totalCirclesTapped / stats.totalGames).toFixed(1)
                  : '0'}
              </span>
            </div>
          </div>
        </>
      )}

      {/* Navigation */}
      <div className={styles.navButtons}>
        <button onClick={() => navigate('/')}>Home</button>
        <button onClick={() => navigate('/leader')}>Leaderboard</button>
      </div>
    </div>
  );
}
