/* Los borrachos de la taberna: estorban, tropiezan y descolocan el globo. */
(function (FB) {
  'use strict';
  var U = FB.U;
  var W = FB.VIEW.W, F = FB.FLOOR_Y;

  var NAMES = ['Grombak', 'Eldrin', 'Marta la Roja', 'Bofur', 'Tía Nerys', 'El Tuerto'];

  function Drunk(x) {
    this.x = x === undefined ? U.rand(140, W - 140) : x;
    this.y = F;
    this.dir = Math.random() < 0.5 ? -1 : 1;
    this.speed = U.rand(0.5, 1.25);
    this.phase = U.rand(0, 6.28);
    this.stumble = 0;
    this.hiccup = U.rand(90, 300);
    this.bubble = 0;
    this.name = U.pick(NAMES);
    this.hue = U.randInt(0, 359);
    this.h = U.rand(0.85, 1.12);   // escala
  }

  Drunk.prototype.update = function (dt, game) {
    this.phase += 0.045 * dt;

    if (this.stumble > 0) {
      this.stumble -= dt;
    } else {
      this.x += this.dir * this.speed * dt;
      // Tumbo lateral.
      this.x += Math.sin(this.phase * 1.7) * 0.6 * dt;
      if (this.x < 110) { this.x = 110; this.dir = 1; }
      if (this.x > W - 110) { this.x = W - 110; this.dir = -1; }
      if (Math.random() < 0.004 * dt) this.dir *= -1;
      if (Math.random() < 0.003 * dt) this.stumble = U.rand(40, 110);
    }

    this.hiccup -= dt;
    if (this.hiccup <= 0) {
      this.hiccup = U.rand(140, 420);
      this.bubble = 60;
      game.fx.text(this.x, F - 96 * this.h, '¡hip!', '#ffd479', 14);
    }
    if (this.bubble > 0) this.bubble -= dt;
  };

  Drunk.prototype.bodyCircles = function () {
    var s = this.h;
    var lean = Math.sin(this.phase * 1.7) * 6;
    return [
      { x: this.x + lean, y: F - 72 * s, r: 16 * s },  // cabeza
      { x: this.x + lean * 0.6, y: F - 38 * s, r: 22 * s }   // tripa
    ];
  };

  /* Choque con el globo: rebote aleatorio de taberna. */
  Drunk.prototype.collideBalloon = function (b, game) {
    var cs = this.bodyCircles();
    for (var i = 0; i < cs.length; i++) {
      var c = cs[i];
      if (U.circleHit(b.x, b.y, b.r, c.x, c.y, c.r)) {
        var dx = b.x - c.x, dy = b.y - c.y;
        var d = Math.sqrt(dx * dx + dy * dy) || 0.001;
        b.x = c.x + (dx / d) * (b.r + c.r);
        b.y = c.y + (dy / d) * (b.r + c.r);
        b.kick(U.rand(-4, 4) + this.dir * 1.5, -U.rand(4.5, 7.5));
        this.stumble = 50;
        this.bubble = 60;
        game.addScore(15, this.x, F - 110 * this.h, '+15 ' + this.name, '#ffc98a');
        game.fx.spark(b.x, b.y, '#ffb36a', 10, 3);
        game.fx.shake(3);
        return true;
      }
    }
    return false;
  };

  Drunk.prototype.collidePlayer = function (pl, game) {
    if (!pl.onGround || pl.stun > 0) return false;
    if (Math.abs(pl.x - this.x) < 26 && Math.abs(pl.y - F) < 30) {
      pl.knock(pl.x < this.x ? -1 : 1, game);
      this.stumble = 70;
      game.fx.text(this.x, F - 120 * this.h, '¡mira por dónde vas!', '#ff9c6a', 14);
      return true;
    }
    return false;
  };

  Drunk.prototype.draw = function (ctx, t) {
    var s = this.h;
    var lean = Math.sin(this.phase * 1.7) * 6 + (this.stumble > 0 ? Math.sin(t * 0.4) * 5 : 0);
    var x = this.x, y = F;

    ctx.save();
    ctx.fillStyle = 'rgba(0,0,0,.35)';
    ctx.beginPath();
    ctx.ellipse(x, y + 4, 24 * s, 6 * s, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    // Piernas cortas.
    var stepx = Math.sin(this.phase * 2.4) * (this.stumble > 0 ? 1 : 5);
    U.line(ctx, x - 7 * s, y - 34 * s, x - 8 * s + stepx, y, 8 * s, '#3b3040');
    U.line(ctx, x + 7 * s, y - 34 * s, x + 8 * s - stepx, y, 8 * s, '#3b3040');

    // Tripa.
    ctx.save();
    ctx.fillStyle = 'hsl(' + this.hue + ',26%,38%)';
    ctx.beginPath();
    ctx.ellipse(x + lean * 0.6, y - 38 * s, 22 * s, 25 * s, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.fillStyle = 'rgba(0,0,0,.25)';
    ctx.fillRect(x - 22 * s + lean * 0.6, y - 30 * s, 44 * s, 6 * s);
    ctx.restore();

    // Brazos y jarra.
    var mugX = x + lean + 24 * s, mugY = y - 52 * s + Math.sin(this.phase * 2) * 3;
    U.line(ctx, x + 16 * s + lean * 0.6, y - 46 * s, mugX, mugY, 7 * s, 'hsl(' + this.hue + ',24%,32%)');
    U.line(ctx, x - 16 * s + lean * 0.6, y - 46 * s, x - 24 * s + lean, y - 30 * s, 7 * s, 'hsl(' + this.hue + ',24%,32%)');
    ctx.save();
    ctx.fillStyle = '#b9a88a';
    U.roundRect(ctx, mugX - 7 * s, mugY - 9 * s, 14 * s, 18 * s, 3);
    ctx.fill();
    ctx.strokeStyle = '#b9a88a';
    ctx.lineWidth = 3 * s;
    ctx.beginPath();
    ctx.arc(mugX + 10 * s, mugY, 5 * s, -1.2, 1.2);
    ctx.stroke();
    ctx.fillStyle = '#f0e6c8';
    ctx.beginPath();
    ctx.ellipse(mugX, mugY - 9 * s, 7 * s, 3 * s, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    // Cabeza.
    var hx = x + lean, hy = y - 72 * s;
    ctx.fillStyle = '#dcb28c';
    U.circle(ctx, hx, hy, 16 * s);
    ctx.fill();
    ctx.fillStyle = 'rgba(200,70,70,.55)';
    U.circle(ctx, hx, hy + 3 * s, 5 * s);
    ctx.fill();
    ctx.fillStyle = '#1a1020';
    if (this.stumble > 0) {
      U.line(ctx, hx - 9 * s, hy - 4 * s, hx - 3 * s, hy + 1 * s, 2, '#1a1020');
      U.line(ctx, hx - 3 * s, hy - 4 * s, hx - 9 * s, hy + 1 * s, 2, '#1a1020');
      U.line(ctx, hx + 3 * s, hy - 4 * s, hx + 9 * s, hy + 1 * s, 2, '#1a1020');
      U.line(ctx, hx + 9 * s, hy - 4 * s, hx + 3 * s, hy + 1 * s, 2, '#1a1020');
    } else {
      ctx.fillRect(hx - 8 * s, hy - 4 * s, 3.5 * s, 3 * s);
      ctx.fillRect(hx + 5 * s, hy - 4 * s, 3.5 * s, 3 * s);
    }
    // Gorro.
    ctx.fillStyle = 'hsl(' + ((this.hue + 40) % 360) + ',34%,30%)';
    ctx.beginPath();
    ctx.arc(hx, hy - 4 * s, 16 * s, Math.PI * 1.05, Math.PI * 2.05);
    ctx.closePath();
    ctx.fill();

    if (this.bubble > 0) {
      ctx.save();
      ctx.globalAlpha = U.clamp(this.bubble / 60, 0, 1) * 0.8;
      ctx.fillStyle = '#ffd479';
      U.circle(ctx, hx + 18 * s, hy - 16 * s, 4);
      ctx.fill();
      U.circle(ctx, hx + 26 * s, hy - 26 * s, 2.5);
      ctx.fill();
      ctx.restore();
    }
  };

  FB.Drunk = Drunk;
})(window.FB = window.FB || {});
