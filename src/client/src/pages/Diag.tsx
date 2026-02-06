import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { checkHealth } from '../services/apiClient';
import styles from './Diag.module.css';

interface HealthDetails {
  healthy: boolean;
  details?: Record<string, unknown>;
  lastChecked: string;
}

export default function Diag() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [health, setHealth] = useState<HealthDetails | null>(null);

  const check = useCallback(async () => {
    setIsLoading(true);
    try {
      const result = await checkHealth();
      setHealth({
        healthy: result.healthy,
        details: result.details as Record<string, unknown> | undefined,
        lastChecked: new Date().toLocaleString(),
      });
    } catch {
      setHealth({ healthy: false, lastChecked: new Date().toLocaleString() });
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Auto-check on mount
  useEffect(() => { check(); }, [check]);

  return (
    <div className={styles.container}>
      <h1>System Status</h1>

      {isLoading && (
        <div className={styles.status}>
          <div className={styles.spinner} />
          <p>Checking API status...</p>
        </div>
      )}

      {!isLoading && health?.healthy && (
        <div className={`${styles.status} ${styles.success}`}>
          <h4>Healthy</h4>
          <p>API Status: OK</p>
          <small>Last checked: {health.lastChecked}</small>
        </div>
      )}

      {!isLoading && health && !health.healthy && (
        <div className={`${styles.status} ${styles.error}`}>
          <h4>API unavailable</h4>
          <p>The API could not be reached. The app runs in offline mode.</p>
          <small>Last checked: {health.lastChecked}</small>
        </div>
      )}

      <div className={styles.actions}>
        <button onClick={check} disabled={isLoading}>
          {isLoading ? 'Checking...' : 'Refresh Status'}
        </button>
        <button onClick={() => navigate('/')}>Return to Home</button>
      </div>
    </div>
  );
}
