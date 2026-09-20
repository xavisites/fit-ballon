/* Partículas, textos flotantes y temblor de cámara. */
(function (FB) {
  'use strict';
  var U = FB.U;

  function FX() {
    this.parts = [];
    this.texts = [];
    this.shakeMag = 0;
    this.shakeX = 0;
    this.shakeY = 0;
  }

  FX.prototype.clear = function () {
    this.parts.length = 0;
    this.texts.length = 0;
    this.shakeMag = 0;
  };

  FX.prototype.shake = function (mag) {
    this.shakeMag = Math.max(this.shakeMag, mag);
  };

  FX.prototype.spark = function (x, y, color, n, spread) {
    spread = spread || 4;
    for (var i = 0; i < n; i++) {
      this.parts.push({
        x: x, y: y,
        vx: U.rand(-spread, spread),
        vy: U.rand(-spread, spread * 0.6),
        life: U.randInt(18, 38),
        max: 38,
        r: U.rand(1.5, 3.6),
        g: 0.08,
        color: color
      });
    }
  };

  FX.prototype.pop = function (x, y, color) {
    for (var i = 0; i < 26; i++) {
      var a = (i / 26) * Math.PI * 2 + U.rand(-0.2, 0.2);
      var s = U.rand(2, 7);
      this.parts.push({
        x: x, y: y,
        vx: Math.cos(a) * s,
        vy: Math.sin(a) * s,
        life: U.randInt(24, 52),
        max: 52,
        r: U.rand(2, 5),
        g: 0.16,
        color: color
      });
    }
    this.shake(9);
  };

  FX.prototype.text = function (x, y, str, color, size) {
    this.texts.push({
      x: x, y: y, str: str, color: color || '#ffe9b0',
      size: size || 18, life: 56, max: 56
    });
  };

  FX.prototype.update = function (dt) {
    var i, p;
    for (i = this.parts.length - 1; i >= 0; i--) {
      p = this.parts[i];
      p.x += p.vx * dt;
      p.y += p.vy * dt;
      p.vy += p.g * dt;
      p.vx *= 0.98;
      p.life -= dt;
      if (p.life <= 0) this.parts.splice(i, 1);
    }
    for (i = this.texts.length - 1; i >= 0; i--) {
      var t = this.texts[i];
      t.y -= 0.7 * dt;
      t.life -= dt;
      if (t.life <= 0) this.texts.splice(i, 1);
    }
    if (this.shakeMag > 0.1) {
      this.shakeX = U.rand(-this.shakeMag, this.shakeMag);
      this.shakeY = U.rand(-this.shakeMag, this.shakeMag);
      this.shakeMag *= Math.pow(0.86, dt);
    } else {
      this.shakeMag = this.shakeX = this.shakeY = 0;
    }
  };

  FX.prototype.draw = function (ctx) {
    var i, p;
    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    for (i = 0; i < this.parts.length; i++) {
      p = this.parts[i];
      ctx.globalAlpha = U.clamp(p.life / p.max, 0, 1);
      ctx.fillStyle = p.color;
      U.circle(ctx, p.x, p.y, p.r);
      ctx.fill();
    }
    ctx.restore();

    ctx.save();
    ctx.textAlign = 'center';
    for (i = 0; i < this.texts.length; i++) {
      var t = this.texts[i];
      ctx.globalAlpha = U.clamp(t.life / t.max, 0, 1);
      ctx.font = 'bold ' + t.size + 'px Georgia, serif';
      ctx.lineWidth = 4;
      ctx.strokeStyle = 'rgba(0,0,0,.75)';
      ctx.strokeText(t.str, t.x, t.y);
      ctx.fillStyle = t.color;
      ctx.fillText(t.str, t.x, t.y);
    }
    ctx.restore();
  };

  FB.FX = FX;
})(window.FB = window.FB || {});
