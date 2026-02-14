/* ── Shared model types mirroring the .NET Shared project ── */

/** Wraps every API response for consistent success/error handling */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errorCode?: string;
  timestamp: string;
}

export interface HighScore {
  playerInitials: string;
  score: number;
  gameMode: string;
  scoreDate: string;
  partitionKey: string;
  rowKey: string;
  timestamp: string;
}

export interface SaveHighScoreRequest {
  playerInitials: string;
  score: number;
  gameMode?: string;
}

export interface GameStats {
  initials: string;
  totalGames: number;
  totalCirclesTapped: number;
  averageScore: number;
  highestScore: number;
  longestStreak: number;
  totalPlaytimeSeconds: number;
  lastPlayed: string;
  firstPlayed: string;
  percentileRank: number;
  scoreDistribution: string;
}

export interface RecordGameRequest {
  initials: string;
  score: number;
  circlesTapped: number;
  playtimeSeconds: number;
}

/** Circle used by the physics engine at runtime */
export interface GameCircle {
  id: number;
  x: number;
  y: number;
  radius: number;
  velocityX: number;
  velocityY: number;
  isVisible: boolean;
  isHit: boolean;
  person: string;
  personClass: string;
  color: string;
}
