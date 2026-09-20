/* El globo: cae despacio, rebota y no debe tocar el suelo. */
(function (FB) {
  'use strict';
  var U = FB.U, P = FB.PHYS;
  var W = FB.VIEW.W;

  function Balloon() {
    this.r = 27;
    this.reset(1);
  }

  Balloon.prototype.reset = function (level) {
    this.x = W / 2 + U.rand(-90, 90);
    this.y = 130;
    this.vx = U.rand(-1.2, 1.2);
    this.vy = 0.4;
    this.spin = 0;
    this.squash = 0;
    this.wobble = U.rand(0, 6.28);
    this.alive = true;
    this.hue = (level * 47) % 360;
  };

  Balloon.prototype.gravity = function (game) {
    var g = P.gravityBase + (game.level - 1) * P.gravityPerLevel;
    g = Math.min(g, P.gravityMax);
    if (game.penaltyTimer > 0) g += P.gravityPenalty;
    return g;
  };

  Balloon.prototype.update = function (dt, game) {
    this.wobble += 0.035 * dt;

    this.vy += this.gravity(game) * dt;
    // Corriente de aire de la sala: más fuerte según el nivel.
    var wind = Math.sin(this.wobble * 0.5) * (P.windPerLevel * Math.min(game.level, 8));
    this.vx += (wind + Math.sin(this.wobble * 1.7) * 0.012) * dt;

    this.vx *= Math.pow(P.airDrag, dt);
    this.vy *= Math.pow(P.airDrag, dt);

    var sp = Math.hypot(this.vx, this.vy);
    if (sp > P.maxSpeed) {
      this.vx = this.vx / sp * P.maxSpeed;
      this.vy = this.vy / sp * P.maxSpeed;
    }

    this.x += this.vx * dt;
    this.y += this.vy * dt;

    // Muros y techo.
    if (this.x - this.r < 0) {
      this.x = this.r;
      this.vx = Math.abs(this.vx) * P.bounce;
      game.fx.spark(this.x, this.y, '#c9b28a', 5, 2);
    } else if (this.x + this.r > W) {
      this.x = W - this.r;
      this.vx = -Math.abs(this.vx) * P.bounce;
      game.fx.spark(this.x, this.y, '#c9b28a', 5, 2);
    }
    if (this.y - this.r < FB.CEIL_Y) {
      this.y = FB.CEIL_Y + this.r;
      this.vy = Math.abs(this.vy) * 0.55;
    }

    this.spin = U.clamp(this.vx * 0.02, -0.4, 0.4);
    this.squash *= Math.pow(0.88, dt);
  };

  /* Impulso recibido de una parte del cuerpo. */
  Balloon.prototype.kick = function (vx, vy) {
    this.vx = vx;
    this.vy = vy;
    this.squash = 1;
  };

  Balloon.prototype.draw = function (ctx, t) {
    var sq = 1 + this.squash * 0.28;
    var st = 1 - this.squash * 0.22;
    var x = this.x, y = this.y, r = this.r;
    var sway = Math.sin(this.wobble) * 0.06;

    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(this.spin + sway);

    // Cuerda.
    ctx.strokeStyle = 'rgba(240,220,180,.55)';
    ctx.lineWidth = 1.6;
    ctx.beginPath();
    ctx.moveTo(0, r * 1.02);
    ctx.quadraticCurveTo(Math.sin(t * 0.05) * 10, r + 18, Math.sin(t * 0.04) * 16, r + 34);
    ctx.stroke();

    ctx.scale(st, sq);

    var g = ctx.createRadialGradient(-r * 0.35, -r * 0.45, r * 0.1, 0, 0, r * 1.15);
    g.addColorStop(0, 'hsl(' + this.hue + ',92%,78%)');
    g.addColorStop(0.55, 'hsl(' + this.hue + ',80%,56%)');
    g.addColorStop(1, 'hsl(' + ((this.hue + 18) % 360) + ',72%,34%)');
    ctx.fillStyle = g;
    ctx.beginPath();
    ctx.ellipse(0, 0, r * 0.92, r, 0, 0, Math.PI * 2);
    ctx.fill();

    // Nudo.
    ctx.fillStyle = 'hsl(' + this.hue + ',70%,40%)';
    ctx.beginPath();
    ctx.moveTo(-5, r * 0.96);
    ctx.lineTo(5, r * 0.96);
    ctx.lineTo(0, r * 1.16);
    ctx.closePath();
    ctx.fill();

    // Brillo.
    ctx.fillStyle = 'rgba(255,255,255,.35)';
    ctx.beginPath();
    ctx.ellipse(-r * 0.32, -r * 0.4, r * 0.22, r * 0.32, -0.5, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    // Aura cálida de las antorchas.
    ctx.save();
    ctx.globalCompositeOperation = 'lighter';
    var ag = ctx.createRadialGradient(x, y, r * 0.6, x, y, r * 2.2);
    ag.addColorStop(0, 'rgba(255,190,120,.10)');
    ag.addColorStop(1, 'rgba(255,150,80,0)');
    ctx.fillStyle = ag;
    U.circle(ctx, x, y, r * 2.2);
    ctx.fill();
    ctx.restore();
  };

  FB.Balloon = Balloon;
})(window.FB = window.FB || {});
