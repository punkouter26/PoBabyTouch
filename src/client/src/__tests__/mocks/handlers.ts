import { http, HttpResponse } from 'msw';
import type { ApiResponse, HighScore, GameStats } from '../../types';

/* ── Fixtures ──────────────────────────────────────────────── */

const fakeHighScores: HighScore[] = [
  {
    playerInitials: 'ABC',
    score: 2500,
    gameMode: 'Default',
    scoreDate: '2026-01-15T10:30:00Z',
    partitionKey: 'Default',
    rowKey: 'abc-001',
    timestamp: '2026-01-15T10:30:00Z',
  },
  {
    playerInitials: 'XYZ',
    score: 2000,
    gameMode: 'Default',
    scoreDate: '2026-01-14T09:00:00Z',
    partitionKey: 'Default',
    rowKey: 'xyz-001',
    timestamp: '2026-01-14T09:00:00Z',
  },
];

const fakeStats: GameStats = {
  initials: 'ABC',
  totalGames: 10,
  totalCirclesTapped: 500,
  averageScore: 1800,
  highestScore: 2500,
  longestStreak: 12,
  totalPlaytimeSeconds: 1200,
  lastPlayed: '2026-01-15T10:30:00Z',
  firstPlayed: '2026-01-01T08:00:00Z',
  percentileRank: 85.5,
  scoreDistribution: '0-99:2,100-199:5,200-299:3',
};

/* ── Handlers ──────────────────────────────────────────────── */

export const handlers = [
  // GET /api/highscores
  http.get('/api/highscores', () =>
    HttpResponse.json<ApiResponse<HighScore[]>>({
      success: true,
      message: 'OK',
      data: fakeHighScores,
      timestamp: new Date().toISOString(),
    }),
  ),

  // POST /api/highscores
  http.post('/api/highscores', () =>
    HttpResponse.json<ApiResponse<unknown>>({
      success: true,
      message: 'High score saved successfully',
      timestamp: new Date().toISOString(),
    }),
  ),

  // GET /api/highscores/check/:score
  http.get('/api/highscores/check/:score', ({ params }) => {
    const score = Number(params.score);
    return HttpResponse.json<ApiResponse<boolean>>({
      success: true,
      message: 'OK',
      data: score > 1500,
      timestamp: new Date().toISOString(),
    });
  }),

  // GET /api/highscores/rank/:score
  http.get('/api/highscores/rank/:score', () =>
    HttpResponse.json<ApiResponse<number>>({
      success: true,
      message: 'OK',
      data: 3,
      timestamp: new Date().toISOString(),
    }),
  ),

  // GET /api/stats/:initials
  http.get('/api/stats/:initials', ({ params }) =>
    HttpResponse.json<ApiResponse<GameStats>>({
      success: true,
      message: 'OK',
      data: { ...fakeStats, initials: params.initials as string },
      timestamp: new Date().toISOString(),
    }),
  ),

  // GET /api/stats
  http.get('/api/stats', () =>
    HttpResponse.json<ApiResponse<GameStats[]>>({
      success: true,
      message: 'OK',
      data: [fakeStats],
      timestamp: new Date().toISOString(),
    }),
  ),

  // POST /api/stats/record
  http.post('/api/stats/record', () =>
    HttpResponse.json<ApiResponse<GameStats>>({
      success: true,
      message: 'OK',
      data: fakeStats,
      timestamp: new Date().toISOString(),
    }),
  ),

  // GET /api/health
  http.get('/api/health', () =>
    HttpResponse.json({ status: 'Healthy' }),
  ),
];
