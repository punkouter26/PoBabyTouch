/**
 * Audio manager — thin wrapper around the Web Audio API.
 * Gracefully degrades if audio is unavailable.
 */

let volume = 0.7;
let isMuted = false;

const soundMap: Record<string, string> = {
  tap: '/sounds/tap.mp3',
  highscore: '/sounds/highscore.mp3',
  gameover: '/sounds/gameover.mp3',
  tick: '/sounds/tick.mp3',
  matt: '/sounds/matt/Recording.m4a',
  kim: '/sounds/kim/Recording.m4a',
  nick: '/sounds/nick/Recording.m4a',
};

export function playSound(type: string) {
  if (isMuted) return;
  try {
    const src = soundMap[type.toLowerCase()] ?? soundMap.tap;
    const audio = new Audio(src);
    audio.volume = volume;
    audio.play().catch(() => { /* silent fail */ });
  } catch { /* silent fail */ }
}

export function setVolume(v: number) {
  volume = Math.max(0, Math.min(1, v));
  localStorage.setItem('gameVolume', String(volume));
}

export function getVolume() {
  const s = localStorage.getItem('gameVolume');
  if (s) volume = parseFloat(s);
  return volume;
}

export function toggleMute() {
  isMuted = !isMuted;
  localStorage.setItem('gameMuted', String(isMuted));
  return isMuted;
}

export function vibrate(ms = 50) {
  if ('vibrate' in navigator) navigator.vibrate(ms);
}
