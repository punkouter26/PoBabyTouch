import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { HighScore } from '../types';
import { getTopScores } from '../services/apiClient';
import { getLocalHighScores } from '../services/localStorage';
import styles from './Leader.module.css';

export default function Leader() {
  const navigate = useNavigate();
  const [entries, setEntries] = useState<HighScore[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage(null);
    try {
      const scores = await getTopScores('Default', 10);
      if (scores.length > 0) {
        setEntries(scores);
      } else {
        // Fallback to local storage
        const local = getLocalHighScores().slice(0, 10);
        setEntries(local);
        if (local.length === 0) setErrorMessage(null); // no error, just empty
      }
    } catch {
      const local = getLocalHighScores().slice(0, 10);
      setEntries(local);
      if (local.length === 0) setErrorMessage('Failed to load leaderboard data');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => { loadData(); }, [loadData]);

  return (
    <div className={styles.container}>
      <h1>Leaderboard</h1>

      <div className={styles.content}>
        {isLoading && (
          <div className={styles.loadingSpinner}>
            <div className={styles.spinner} />
            <p>Loading leaderboard data...</p>
          </div>
        )}

        {!isLoading && errorMessage && (
          <div className={styles.error}>
            <p>Error: {errorMessage}</p>
            <button onClick={loadData}>Try Again</button>
          </div>
        )}

        {!isLoading && !errorMessage && entries.length > 0 && (
          <table className={styles.table}>
            <thead>
              <tr>
                <th>Rank</th>
                <th>Initials</th>
                <th>Score</th>
                <th className={styles.dateCol}>Date</th>
              </tr>
            </thead>
            <tbody>
              {entries.map((entry, i) => (
                <tr key={entry.rowKey ?? i} className={styles.scoreRow}>
                  <td className={styles.rankCell}>#{i + 1}</td>
                  <td className={styles.initialsCell}>{entry.playerInitials}</td>
                  <td className={styles.scoreCell}>{entry.score}</td>
                  <td className={`${styles.dateCell} ${styles.dateCol}`}>
                    {entry.scoreDate
                      ? new Date(entry.scoreDate).toLocaleDateString()
                      : '-'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {!isLoading && !errorMessage && entries.length === 0 && (
          <div className={styles.empty}>
            <p>No scores have been recorded yet. Be the first!</p>
          </div>
        )}
      </div>

      <div className={styles.navButtons}>
        <button onClick={() => navigate('/game')}>Play Game</button>
        <button onClick={() => navigate('/')}>Back to Menu</button>
      </div>
    </div>
  );
}
