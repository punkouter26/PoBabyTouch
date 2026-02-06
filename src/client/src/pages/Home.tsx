import { useNavigate } from 'react-router-dom';
import styles from './Home.module.css';

export default function Home() {
  const nav = useNavigate();

  return (
    <div className={styles.container}>
      <div className={styles.title}>
        <h1>PoBabyTouch</h1>
        <p>Welcome to Baby Touch Game</p>
      </div>

      <div className={styles.menu}>
        <button className={styles.btn} onClick={() => nav('/game')}>
          ▶ Play Game
        </button>
        <button className={`${styles.btn} ${styles.baby}`} onClick={() => nav('/game?mode=baby')}>
          ♥ Baby Mode
        </button>
        <button className={styles.btn} onClick={() => nav('/leader')}>
          🏆 Leaderboard
        </button>
        <button className={`${styles.btn} ${styles.stats}`} onClick={() => nav('/stats')}>
          📊 Statistics
        </button>
      </div>

      <div className={styles.footer}>
        <p>Version 2.0 &copy; 2026</p>
      </div>
    </div>
  );
}
