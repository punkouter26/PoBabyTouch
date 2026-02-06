/**
 * Physics engine implementations (Strategy Pattern).
 * Pure functions — no React dependency.
 */

import type { GameCircle } from '../types.ts';

/* ── Interface ────────────────────────────────────────────── */

export interface PhysicsEngine {
  maxSpeed: number;
  isOverlapping(a: GameCircle, b: GameCircle): boolean;
  update(circles: GameCircle[], w: number, h: number, speedMult: number): void;
}

/* ── Shared helpers ───────────────────────────────────────── */

function distance(a: GameCircle, b: GameCircle) {
  const dx = a.x - b.x;
  const dy = a.y - b.y;
  return Math.sqrt(dx * dx + dy * dy);
}

function isOverlapping(a: GameCircle, b: GameCircle) {
  return distance(a, b) < a.radius + b.radius;
}

function handleBoundary(c: GameCircle, w: number, h: number) {
  if (c.x - c.radius < 0) { c.x = c.radius; c.velocityX = Math.abs(c.velocityX); }
  else if (c.x + c.radius > w) { c.x = w - c.radius; c.velocityX = -Math.abs(c.velocityX); }
  if (c.y - c.radius < 0) { c.y = c.radius; c.velocityY = Math.abs(c.velocityY); }
  else if (c.y + c.radius > h) { c.y = h - c.radius; c.velocityY = -Math.abs(c.velocityY); }
}

function handleCollisions(circle: GameCircle, all: GameCircle[], bounce: number) {
  for (const other of all) {
    if (!other.isVisible || other.id === circle.id || !isOverlapping(circle, other)) continue;

    const dx = other.x - circle.x;
    const dy = other.y - circle.y;
    const d = Math.sqrt(dx * dx + dy * dy);
    if (d === 0) continue;

    const nx = dx / d;
    const ny = dy / d;
    const vx = circle.velocityX - other.velocityX;
    const vy = circle.velocityY - other.velocityY;
    const vn = vx * nx + vy * ny;
    if (vn > 0) continue;

    const impulse = -(1 + bounce) * vn;
    circle.velocityX -= impulse * nx;
    circle.velocityY -= impulse * ny;
    other.velocityX += impulse * nx;
    other.velocityY += impulse * ny;

    const overlap = (circle.radius + other.radius) - d;
    circle.x -= overlap * 0.5 * nx;
    circle.y -= overlap * 0.5 * ny;
    other.x += overlap * 0.5 * nx;
    other.y += overlap * 0.5 * ny;
  }
}

/* ── Standard (normal mode) ───────────────────────────────── */

export const standardEngine: PhysicsEngine = {
  maxSpeed: 5,
  isOverlapping,
  update(circles, w, h, speedMult) {
    for (const c of circles) {
      if (!c.isVisible) continue;
      c.x += c.velocityX * speedMult;
      c.y += c.velocityY * speedMult;
      handleBoundary(c, w, h);
      handleCollisions(c, circles, 1.0);
    }
  },
};

/* ── Baby mode (gentler) ──────────────────────────────────── */

export const babyEngine: PhysicsEngine = {
  maxSpeed: 2,
  isOverlapping,
  update(circles, w, h) {
    for (const c of circles) {
      if (!c.isVisible) continue;
      c.x += c.velocityX;
      c.y += c.velocityY;
      handleBoundary(c, w, h);
      handleCollisions(c, circles, 0.8);

      // Clamp speed
      const speed = Math.sqrt(c.velocityX ** 2 + c.velocityY ** 2);
      if (speed > 2) {
        const s = 2 / speed;
        c.velocityX *= s;
        c.velocityY *= s;
      }
    }
  },
};
