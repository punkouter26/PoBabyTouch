/**
 * Centralised API client — all HTTP calls go through here.
 * If the API is unreachable the app stays functional
 * (progressive dev: client-only first, API optional).
 */
import type { ApiResponse, HighScore, SaveHighScoreRequest, GameStats, RecordGameRequest } from '../types.ts';

const API_BASE = import.meta.env.VITE_API_URL ?? '';

async function get<T>(url: string): Promise<ApiResponse<T>> {
  try {
    const res = await fetch(`${API_BASE}${url}`);
    if (!res.ok) return { success: false, message: `HTTP ${res.status}`, timestamp: new Date().toISOString() };
    return (await res.json()) as ApiResponse<T>;
  } catch {
    return { success: false, message: 'API unreachable', timestamp: new Date().toISOString() };
  }
}

async function post<TReq, TRes>(url: string, body: TReq): Promise<ApiResponse<TRes>> {
  try {
    const res = await fetch(`${API_BASE}${url}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    if (!res.ok) return { success: false, message: `HTTP ${res.status}`, timestamp: new Date().toISOString() };
    return (await res.json()) as ApiResponse<TRes>;
  } catch {
    return { success: false, message: 'API unreachable', timestamp: new Date().toISOString() };
  }
}

/* ── High Scores ──────────────────────────────────────────── */

export async function getTopScores(gameMode = 'Default', count = 10): Promise<HighScore[]> {
  const r = await get<HighScore[]>(`/api/highscores?gameMode=${gameMode}&count=${count}`);
  return r.success && r.data ? r.data : [];
}

export async function submitScore(request: SaveHighScoreRequest): Promise<boolean> {
  const r = await post<SaveHighScoreRequest, unknown>('/api/highscores', request);
  return r.success;
}

export async function isHighScore(score: number, gameMode = 'Default'): Promise<boolean> {
  const r = await get<boolean>(`/api/highscores/check/${score}?gameMode=${gameMode}`);
  return r.success && r.data === true;
}

export async function getPlayerRank(score: number, gameMode = 'Default'): Promise<number> {
  const r = await get<number>(`/api/highscores/rank/${score}?gameMode=${gameMode}`);
  return r.success && r.data !== undefined ? r.data : -1;
}

/* ── Stats ────────────────────────────────────────────────── */

export async function getPlayerStats(initials: string): Promise<GameStats | null> {
  const r = await get<GameStats>(`/api/stats/${initials}`);
  return r.success && r.data ? r.data : null;
}

export async function getAllStats(): Promise<GameStats[]> {
  const r = await get<GameStats[]>('/api/stats');
  return r.success && r.data ? r.data : [];
}

export async function recordGameSession(req: RecordGameRequest): Promise<GameStats | null> {
  const r = await post<RecordGameRequest, GameStats>('/api/stats/record', req);
  return r.success && r.data ? r.data : null;
}

/* ── Health ───────────────────────────────────────────────── */

export async function checkHealth(): Promise<{ healthy: boolean; details?: unknown }> {
  try {
    const res = await fetch(`${API_BASE}/api/health`);
    const data = await res.json();
    return { healthy: res.ok, details: data };
  } catch {
    return { healthy: false };
  }
}
