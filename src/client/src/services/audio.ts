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
  'blue-1': '/sounds/blue/wave-1.mp3',
  'blue-2': '/sounds/blue/wave-2.mp3',
  'blue-3': '/sounds/blue/wave-3.mp3',
  'blue-4': '/sounds/blue/wave-4.mp3',
  'blue-5': '/sounds/blue/wave-5.mp3',
  'green-1': '/sounds/green/wave-1.mp3',
  'green-2': '/sounds/green/wave-2.mp3',
  'green-3': '/sounds/green/wave-3.mp3',
  'green-4': '/sounds/green/wave-4.mp3',
  'green-5': '/sounds/green/wave-5.mp3',
  'red-1': '/sounds/red/wave-1.mp3',
  'red-2': '/sounds/red/wave-2.mp3',
  'red-3': '/sounds/red/wave-3.mp3',
  'red-4': '/sounds/red/wave-4.mp3',
  'red-5': '/sounds/red/wave-5.mp3',
  'purple-1': '/sounds/purple/wave-1.mp3',
  'purple-2': '/sounds/purple/wave-2.mp3',
  'purple-3': '/sounds/purple/wave-3.mp3',
  'purple-4': '/sounds/purple/wave-4.mp3',
  'purple-5': '/sounds/purple/wave-5.mp3',
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

export function playColorSound(color: string) {
  if (isMuted) return;
  try {
    const randomIndex = Math.floor(Math.random() * 5) + 1;
    const soundKey = `${color}-${randomIndex}`;
    const src = soundMap[soundKey] ?? soundMap.tap;
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
