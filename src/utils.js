/* Utilidades matemáticas y de dibujo. */
(function (FB) {
  'use strict';

  var U = FB.U = {};

  U.clamp = function (v, a, b) { return v < a ? a : (v > b ? b : v); };
  U.lerp = function (a, b, t) { return a + (b - a) * t; };
  U.rand = function (a, b) { return a + Math.random() * (b - a); };
  U.randInt = function (a, b) { return Math.floor(U.rand(a, b + 1)); };
  U.pick = function (arr) { return arr[Math.floor(Math.random() * arr.length)]; };
  U.sign = function (v) { return v < 0 ? -1 : 1; };

  U.dist2 = function (x1, y1, x2, y2) {
    var dx = x2 - x1, dy = y2 - y1;
    return dx * dx + dy * dy;
  };

  U.circleHit = function (x1, y1, r1, x2, y2, r2) {
    var r = r1 + r2;
    return U.dist2(x1, y1, x2, y2) <= r * r;
  };

  /* Punto del rectángulo más cercano a (x, y). */
  U.closestOnRect = function (x, y, rect) {
    return {
      x: U.clamp(x, rect.x, rect.x + rect.w),
      y: U.clamp(y, rect.y, rect.y + rect.h)
    };
  };

  U.roundRect = function (ctx, x, y, w, h, r) {
    r = Math.min(r, w / 2, h / 2);
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.arcTo(x + w, y, x + w, y + h, r);
    ctx.arcTo(x + w, y + h, x, y + h, r);
    ctx.arcTo(x, y + h, x, y, r);
    ctx.arcTo(x, y, x + w, y, r);
    ctx.closePath();
  };

  U.circle = function (ctx, x, y, r) {
    ctx.beginPath();
    ctx.arc(x, y, r, 0, Math.PI * 2);
    ctx.closePath();
  };

  U.line = function (ctx, x1, y1, x2, y2, w, color, cap) {
    ctx.save();
    ctx.lineWidth = w;
    ctx.strokeStyle = color;
    ctx.lineCap = cap || 'round';
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();
    ctx.restore();
  };

  /* Llama animada reutilizable (antorchas, braseros, candelabros). */
  U.flame = function (ctx, x, y, size, t, alpha) {
    var flick = Math.sin(t * 0.21 + x) * 0.16 + Math.sin(t * 0.37 + y) * 0.1;
    var h = size * (1.6 + flick);
    ctx.save();
    ctx.globalAlpha = alpha === undefined ? 1 : alpha;
    ctx.globalCompositeOperation = 'lighter';

    var glow = ctx.createRadialGradient(x, y - h * 0.3, 0, x, y - h * 0.3, size * 4.2);
    glow.addColorStop(0, 'rgba(255,180,70,.55)');
    glow.addColorStop(0.45, 'rgba(255,110,30,.16)');
    glow.addColorStop(1, 'rgba(255,90,20,0)');
    ctx.fillStyle = glow;
    U.circle(ctx, x, y - h * 0.3, size * 4.2);
    ctx.fill();

    ctx.fillStyle = 'rgba(255,140,40,.92)';
    ctx.beginPath();
    ctx.moveTo(x - size * 0.72, y);
    ctx.quadraticCurveTo(x - size * 0.5, y - h * 0.6, x + flick * size, y - h);
    ctx.quadraticCurveTo(x + size * 0.55, y - h * 0.55, x + size * 0.72, y);
    ctx.closePath();
    ctx.fill();

    ctx.fillStyle = 'rgba(255,238,170,.95)';
    ctx.beginPath();
    ctx.moveTo(x - size * 0.34, y);
    ctx.quadraticCurveTo(x - size * 0.22, y - h * 0.42, x + flick * size * 0.6, y - h * 0.58);
    ctx.quadraticCurveTo(x + size * 0.26, y - h * 0.4, x + size * 0.34, y);
    ctx.closePath();
    ctx.fill();
    ctx.restore();
  };
})(window.FB = window.FB || {});
