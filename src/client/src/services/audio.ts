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

// Color sound mappings - each color has 4 random sounds
const colorSounds: Record<string, string[]> = {
  blue: [
    '/sounds/blue/sound1.mp3',
    '/sounds/blue/sound2.mp3',
    '/sounds/blue/sound3.mp3',
    '/sounds/blue/sound4.mp3',
  ],
  green: [
    '/sounds/green/sound1.mp3',
    '/sounds/green/sound2.mp3',
    '/sounds/green/sound3.mp3',
    '/sounds/green/sound4.mp3',
  ],
  red: [
    '/sounds/red/sound1.mp3',
    '/sounds/red/sound2.mp3',
    '/sounds/red/sound3.mp3',
    '/sounds/red/sound4.mp3',
  ],
  purple: [
    '/sounds/purple/sound1.mp3',
    '/sounds/purple/sound2.mp3',
    '/sounds/purple/sound3.mp3',
    '/sounds/purple/sound4.mp3',
  ],
};

function playAudio(src: string) {
  if (isMuted) return;
  try {
    const audio = new Audio(src);
    audio.volume = volume;
    audio.play().catch(() => { /* silent fail */ });
  } catch { /* silent fail */ }
}

export function playSound(type: string) {
  const src = soundMap[type.toLowerCase()] ?? soundMap.tap;
  playAudio(src);
}

export function playColorSound(color: string) {
  const sounds = colorSounds[color.toLowerCase()];
  if (!sounds || sounds.length === 0) {
    playSound('tap'); // fallback to default sound
    return;
  }
  // Pick a random sound from the color's sound array
  const randomIndex = Math.floor(Math.random() * sounds.length);
  const src = sounds[randomIndex];
  playAudio(src);
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
