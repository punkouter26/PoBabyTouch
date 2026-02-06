/**
 * Contract tests for apiClient.ts — verifies the React client correctly
 * parses API responses and handles error/offline scenarios.
 * Uses MSW to mock the .NET API so no real server is needed.
 */
import { describe, it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import { server } from './mocks/server';
import {
  getTopScores,
  submitScore,
  isHighScore,
  getPlayerRank,
  getPlayerStats,
  getAllStats,
  recordGameSession,
  checkHealth,
} from '../services/apiClient';

/* ── High Scores ────────────────────────────────────────────── */

describe('apiClient – High Scores', () => {
  it('getTopScores returns typed HighScore array', async () => {
    const scores = await getTopScores('Default', 10);

    expect(scores).toHaveLength(2);
    expect(scores[0]).toMatchObject({
      playerInitials: 'ABC',
      score: 2500,
      gameMode: 'Default',
    });
    // verify shape keys match TypeScript interface
    for (const s of scores) {
      expect(s).toHaveProperty('playerInitials');
      expect(s).toHaveProperty('score');
      expect(s).toHaveProperty('gameMode');
      expect(s).toHaveProperty('scoreDate');
      expect(s).toHaveProperty('partitionKey');
      expect(s).toHaveProperty('rowKey');
      expect(s).toHaveProperty('timestamp');
    }
  });

  it('getTopScores returns empty array on API failure', async () => {
    server.use(
      http.get('/api/highscores', () => HttpResponse.json(null, { status: 500 })),
    );
    const scores = await getTopScores();
    expect(scores).toEqual([]);
  });

  it('submitScore returns true on success', async () => {
    const ok = await submitScore({ playerInitials: 'TST', score: 1000, gameMode: 'Default' });
    expect(ok).toBe(true);
  });

  it('submitScore returns false on server error', async () => {
    server.use(
      http.post('/api/highscores', () => new HttpResponse(null, { status: 500 })),
    );
    const ok = await submitScore({ playerInitials: 'TST', score: 1000 });
    expect(ok).toBe(false);
  });

  it('isHighScore correctly parses boolean response', async () => {
    expect(await isHighScore(2000)).toBe(true);
    expect(await isHighScore(500)).toBe(false);
  });

  it('getPlayerRank returns numeric rank', async () => {
    const rank = await getPlayerRank(2000);
    expect(rank).toBe(3);
  });

  it('getPlayerRank returns -1 on failure', async () => {
    server.use(
      http.get('/api/highscores/rank/:score', () => new HttpResponse(null, { status: 500 })),
    );
    const rank = await getPlayerRank(2000);
    expect(rank).toBe(-1);
  });
});

/* ── Stats ──────────────────────────────────────────────────── */

describe('apiClient – Stats', () => {
  it('getPlayerStats returns typed GameStats', async () => {
    const stats = await getPlayerStats('ABC');

    expect(stats).not.toBeNull();
    expect(stats!.initials).toBe('ABC');
    expect(stats!.totalGames).toBe(10);
    // verify all expected keys exist
    const keys = [
      'initials', 'totalGames', 'totalCirclesTapped', 'averageScore',
      'highestScore', 'longestStreak', 'totalPlaytimeSeconds',
      'lastPlayed', 'firstPlayed', 'percentileRank', 'scoreDistribution',
    ];
    for (const k of keys) {
      expect(stats).toHaveProperty(k);
    }
  });

  it('getPlayerStats returns null on failure', async () => {
    server.use(
      http.get('/api/stats/:initials', () => new HttpResponse(null, { status: 404 })),
    );
    const stats = await getPlayerStats('ZZZ');
    expect(stats).toBeNull();
  });

  it('getAllStats returns array of GameStats', async () => {
    const all = await getAllStats();
    expect(all).toHaveLength(1);
    expect(all[0]).toHaveProperty('initials');
    expect(all[0]).toHaveProperty('highestScore');
  });

  it('recordGameSession returns GameStats on success', async () => {
    const result = await recordGameSession({
      initials: 'TST',
      score: 1500,
      circlesTapped: 80,
      playtimeSeconds: 120,
    });
    expect(result).not.toBeNull();
    expect(result!.totalGames).toBe(10);
  });

  it('recordGameSession returns null on failure', async () => {
    server.use(
      http.post('/api/stats/record', () => new HttpResponse(null, { status: 500 })),
    );
    const result = await recordGameSession({
      initials: 'TST',
      score: 1500,
      circlesTapped: 80,
      playtimeSeconds: 120,
    });
    expect(result).toBeNull();
  });
});

/* ── Health ──────────────────────────────────────────────────── */

describe('apiClient – Health', () => {
  it('checkHealth returns healthy:true when API is up', async () => {
    const h = await checkHealth();
    expect(h.healthy).toBe(true);
    expect(h.details).toBeDefined();
  });

  it('checkHealth returns healthy:false on network error', async () => {
    server.use(
      http.get('/api/health', () => HttpResponse.error()),
    );
    const h = await checkHealth();
    expect(h.healthy).toBe(false);
  });
});

/* ── Response contract shape ────────────────────────────────── */

describe('ApiResponse contract', () => {
  it('successful response has exactly: success, message, data, timestamp', async () => {
    const res = await fetch('/api/highscores');
    const body = await res.json();

    expect(body).toHaveProperty('success', true);
    expect(body).toHaveProperty('message');
    expect(body).toHaveProperty('data');
    expect(body).toHaveProperty('timestamp');
  });

  it('error response preserves errorCode when present', async () => {
    server.use(
      http.get('/api/highscores', () =>
        HttpResponse.json({
          success: false,
          message: 'Table not found',
          errorCode: 'TABLE_NOT_FOUND',
          timestamp: new Date().toISOString(),
        }),
      ),
    );
    const res = await fetch('/api/highscores');
    const body = await res.json();

    expect(body.success).toBe(false);
    expect(body.errorCode).toBe('TABLE_NOT_FOUND');
  });
});
