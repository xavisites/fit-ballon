/* La sala de mazmorra: decorado, obstáculos y sus colisiones con el globo. */
(function (FB) {
  'use strict';
  var U = FB.U;
  var W = FB.VIEW.W, H = FB.VIEW.H, F = FB.FLOOR_Y;

  function Room() {
    this.obstacles = [];
    this.runes = [];
    this.torches = [
      { x: 108, y: 190 }, { x: W - 108, y: 190 },
      { x: 330, y: 150 }, { x: W - 330, y: 150 }
    ];
    this.bg = null;
  }

  /* --------------------------------------------------------- construcción */
  Room.prototype.build = function (level) {
    this.obstacles = [];
    this.runes = [];

    // Columnas laterales: siempre presentes.
    this.obstacles.push({ type: 'pillar', x: 34, y: F - 168, w: 48, h: 168 });
    this.obstacles.push({ type: 'pillar', x: W - 82, y: F - 168, w: 48, h: 168 });

    if (level >= 2) {
      this.obstacles.push(this.chandelier(W / 2, 54, 132, 0.7));
    }
    if (level >= 3) {
      this.obstacles.push({ type: 'flame', x: 150, y: F - 214, r: 17 });
      this.obstacles.push({ type: 'flame', x: W - 150, y: F - 214, r: 17 });
    }
    if (level >= 4) {
      this.obstacles.push({ type: 'beam', x: W / 2 - 136, y: F - 268, w: 272, h: 20 });
    }
    if (level >= 5) {
      this.obstacles.push(this.chandelier(W * 0.24, 48, 186, -0.6));
    }
    if (level >= 6) {
      this.obstacles.push(this.chandelier(W * 0.76, 48, 150, 0.55));
    }
    if (level >= 7) {
      this.obstacles.push({ type: 'flame', x: W / 2, y: F - 330, r: 19 });
    }
    if (level >= 8) {
      this.obstacles.push({ type: 'beam', x: 120, y: F - 120, w: 150, h: 18 });
      this.obstacles.push({ type: 'beam', x: W - 270, y: F - 120, w: 150, h: 18 });
    }

    this.spawnRune();
    if (level >= 3) this.spawnRune();
  };

  Room.prototype.chandelier = function (px, py, len, ang) {
    return {
      type: 'chandelier', px: px, py: py, len: len,
      ang: ang, angVel: 0, r: 33, x: px, y: py + len
    };
  };

  Room.prototype.spawnRune = function () {
    this.runes.push({
      x: U.rand(160, W - 160),
      y: U.rand(FB.CEIL_Y + 70, F - 160),
      r: 17, active: true, cooldown: 0, phase: U.rand(0, 6.28)
    });
  };

  /* --------------------------------------------------------------- lógica */
  Room.prototype.update = function (dt) {
    var i, o;
    for (i = 0; i < this.obstacles.length; i++) {
      o = this.obstacles[i];
      if (o.type === 'chandelier') {
        // Péndulo simple.
        o.angVel += -0.0026 * Math.sin(o.ang) * dt;
        o.angVel *= Math.pow(0.9995, dt);
        o.ang += o.angVel * dt;
        var nx = o.px + Math.sin(o.ang) * o.len;
        var ny = o.py + Math.cos(o.ang) * o.len;
        o.vx = nx - o.x;
        o.vy = ny - o.y;
        o.x = nx;
        o.y = ny;
      }
    }
    for (i = 0; i < this.runes.length; i++) {
      var rn = this.runes[i];
      if (!rn.active) {
        rn.cooldown -= dt;
        if (rn.cooldown <= 0) {
          rn.active = true;
          rn.x = U.rand(160, W - 160);
          rn.y = U.rand(FB.CEIL_Y + 70, F - 160);
        }
      }
    }
  };

  /* Devuelve 'pop' si el globo ha tocado fuego. */
  Room.prototype.collide = function (b, game) {
    var i, o, res = null;
    for (i = 0; i < this.obstacles.length; i++) {
      o = this.obstacles[i];

      if (o.type === 'pillar' || o.type === 'beam') {
        var c = U.closestOnRect(b.x, b.y, o);
        var dx = b.x - c.x, dy = b.y - c.y;
        var d2 = dx * dx + dy * dy;
        if (d2 < b.r * b.r) {
          var d = Math.sqrt(d2) || 0.001;
          var nx = dx / d, ny = dy / d;
          if (d2 < 0.01) { nx = 0; ny = -1; d = 0.01; }
          b.x = c.x + nx * b.r;
          b.y = c.y + ny * b.r;
          var dot = b.vx * nx + b.vy * ny;
          b.vx = (b.vx - 2 * dot * nx) * FB.PHYS.bounce;
          b.vy = (b.vy - 2 * dot * ny) * FB.PHYS.bounce;
          game.fx.spark(b.x, b.y, '#c9b28a', 5, 2.4);
        }

      } else if (o.type === 'chandelier') {
        if (U.circleHit(b.x, b.y, b.r, o.x, o.y, o.r)) {
          var ax = b.x - o.x, ay = b.y - o.y;
          var ad = Math.sqrt(ax * ax + ay * ay) || 0.001;
          ax /= ad; ay /= ad;
          b.x = o.x + ax * (b.r + o.r);
          b.y = o.y + ay * (b.r + o.r);
          var adot = b.vx * ax + b.vy * ay;
          b.vx = (b.vx - 2 * adot * ax) * 0.9 + (o.vx || 0) * 2.2;
          b.vy = (b.vy - 2 * adot * ay) * 0.9 + (o.vy || 0) * 2.2;
          o.angVel -= (b.vx * 0.00012);
          game.fx.spark(b.x, b.y, '#ffce7a', 7, 3);
          game.fx.shake(3);
        }

      } else if (o.type === 'flame') {
        if (U.circleHit(b.x, b.y, b.r * 0.82, o.x, o.y, o.r * 1.15)) {
          res = 'pop';
        }
      }
    }

    for (i = 0; i < this.runes.length; i++) {
      var rn = this.runes[i];
      if (rn.active && U.circleHit(b.x, b.y, b.r, rn.x, rn.y, rn.r)) {
        rn.active = false;
        rn.cooldown = 420;
        game.addScore(60, rn.x, rn.y, '+60 RUNA', '#9fe8ff');
        game.fx.spark(rn.x, rn.y, '#9fe8ff', 18, 4);
      }
    }
    return res;
  };

  /* --------------------------------------------------------------- dibujo */
  Room.prototype.makeBackground = function () {
    var c = document.createElement('canvas');
    c.width = W; c.height = H;
    var g = c.getContext('2d');

    var wall = g.createLinearGradient(0, 0, 0, F);
    wall.addColorStop(0, '#1b1420');
    wall.addColorStop(0.55, '#241a28');
    wall.addColorStop(1, '#171120');
    g.fillStyle = wall;
    g.fillRect(0, 0, W, F);

    // Sillería de piedra.
    var bh = 34, bw = 84;
    for (var y = 0, row = 0; y < F; y += bh, row++) {
      var off = (row % 2) * (bw / 2);
      for (var x = -bw; x < W + bw; x += bw) {
        var shade = 12 + ((row * 7 + Math.floor(x / bw) * 13) % 11);
        g.fillStyle = 'rgba(' + (52 + shade) + ',' + (42 + shade) + ',' + (58 + shade) + ',.55)';
        g.fillRect(x + off + 1.5, y + 1.5, bw - 3, bh - 3);
        g.strokeStyle = 'rgba(0,0,0,.35)';
        g.lineWidth = 1;
        g.strokeRect(x + off + 1.5, y + 1.5, bw - 3, bh - 3);
      }
    }

    // Arco gótico al fondo.
    g.save();
    g.fillStyle = 'rgba(8,6,12,.72)';
    g.beginPath();
    g.moveTo(W / 2 - 110, F);
    g.lineTo(W / 2 - 110, 230);
    g.quadraticCurveTo(W / 2, 96, W / 2 + 110, 230);
    g.lineTo(W / 2 + 110, F);
    g.closePath();
    g.fill();
    g.strokeStyle = 'rgba(231,194,106,.12)';
    g.lineWidth = 4;
    g.stroke();
    g.restore();

    // Viñeteado.
    var vg = g.createRadialGradient(W / 2, H / 2, H * 0.34, W / 2, H / 2, H * 0.95);
    vg.addColorStop(0, 'rgba(0,0,0,0)');
    vg.addColorStop(1, 'rgba(0,0,0,.85)');
    g.fillStyle = vg;
    g.fillRect(0, 0, W, H);

    this.bg = c;
  };

  Room.prototype.drawBack = function (ctx, t) {
    if (!this.bg) this.makeBackground();
    ctx.drawImage(this.bg, 0, 0);

    // Antorchas de pared.
    for (var i = 0; i < this.torches.length; i++) {
      var tt = this.torches[i];
      ctx.fillStyle = '#3b2c22';
      U.roundRect(ctx, tt.x - 4, tt.y, 8, 28, 3);
      ctx.fill();
      ctx.fillStyle = '#6b563f';
      U.roundRect(ctx, tt.x - 9, tt.y - 6, 18, 9, 3);
      ctx.fill();
      U.flame(ctx, tt.x, tt.y - 6, 9, t + i * 40, 0.85);
    }

    // Suelo.
    var floor = ctx.createLinearGradient(0, F, 0, H);
    floor.addColorStop(0, '#2a2130');
    floor.addColorStop(1, '#100c16');
    ctx.fillStyle = floor;
    ctx.fillRect(0, F, W, H - F);

    ctx.strokeStyle = 'rgba(0,0,0,.5)';
    ctx.lineWidth = 2;
    for (var x = 0; x < W; x += 60) {
      ctx.beginPath();
      ctx.moveTo(x, F);
      ctx.lineTo(x - 26, H);
      ctx.stroke();
    }
    ctx.beginPath();
    ctx.moveTo(0, F + 0.5);
    ctx.lineTo(W, F + 0.5);
    ctx.strokeStyle = 'rgba(231,194,106,.30)';
    ctx.lineWidth = 2;
    ctx.stroke();
  };

  Room.prototype.drawFront = function (ctx, t) {
    var i, o;

    // Runas mágicas.
    for (i = 0; i < this.runes.length; i++) {
      var rn = this.runes[i];
      if (!rn.active) continue;
      var pulse = 0.65 + Math.sin(t * 0.08 + rn.phase) * 0.35;
      ctx.save();
      ctx.globalCompositeOperation = 'lighter';
      var rg = ctx.createRadialGradient(rn.x, rn.y, 0, rn.x, rn.y, rn.r * 2.6);
      rg.addColorStop(0, 'rgba(120,220,255,' + (0.5 * pulse) + ')');
      rg.addColorStop(1, 'rgba(60,150,255,0)');
      ctx.fillStyle = rg;
      U.circle(ctx, rn.x, rn.y, rn.r * 2.6);
      ctx.fill();
      ctx.restore();

      ctx.save();
      ctx.translate(rn.x, rn.y);
      ctx.rotate(t * 0.012);
      ctx.strokeStyle = 'rgba(180,240,255,' + (0.5 + 0.4 * pulse) + ')';
      ctx.lineWidth = 2.2;
      ctx.beginPath();
      for (var k = 0; k < 6; k++) {
        var a = (k / 6) * Math.PI * 2;
        var px = Math.cos(a) * rn.r, py = Math.sin(a) * rn.r;
        if (k === 0) ctx.moveTo(px, py); else ctx.lineTo(px, py);
      }
      ctx.closePath();
      ctx.stroke();
      ctx.beginPath();
      ctx.moveTo(-rn.r * 0.5, 0); ctx.lineTo(rn.r * 0.5, 0);
      ctx.moveTo(0, -rn.r * 0.6); ctx.lineTo(0, rn.r * 0.6);
      ctx.stroke();
      ctx.restore();
    }

    for (i = 0; i < this.obstacles.length; i++) {
      o = this.obstacles[i];

      if (o.type === 'pillar') {
        var pg = ctx.createLinearGradient(o.x, 0, o.x + o.w, 0);
        pg.addColorStop(0, '#3a2f40');
        pg.addColorStop(0.35, '#6a5a70');
        pg.addColorStop(1, '#2c2433');
        ctx.fillStyle = pg;
        ctx.fillRect(o.x, o.y, o.w, o.h);
        ctx.fillStyle = '#7b6a82';
        ctx.fillRect(o.x - 7, o.y - 12, o.w + 14, 14);
        ctx.fillRect(o.x - 7, o.y + o.h - 10, o.w + 14, 12);
        ctx.strokeStyle = 'rgba(0,0,0,.45)';
        ctx.lineWidth = 1.5;
        for (var yy = o.y + 16; yy < o.y + o.h - 10; yy += 22) {
          ctx.beginPath();
          ctx.moveTo(o.x + 4, yy);
          ctx.lineTo(o.x + o.w - 4, yy);
          ctx.stroke();
        }

      } else if (o.type === 'beam') {
        var bg = ctx.createLinearGradient(0, o.y, 0, o.y + o.h);
        bg.addColorStop(0, '#6d4d31');
        bg.addColorStop(1, '#3b2919');
        ctx.fillStyle = bg;
        U.roundRect(ctx, o.x, o.y, o.w, o.h, 4);
        ctx.fill();
        ctx.strokeStyle = 'rgba(0,0,0,.5)';
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fillStyle = '#8a7a52';
        ctx.fillRect(o.x + 12, o.y - 2, 5, o.h + 4);
        ctx.fillRect(o.x + o.w - 17, o.y - 2, 5, o.h + 4);

      } else if (o.type === 'chandelier') {
        U.line(ctx, o.px, o.py, o.x, o.y, 2.5, 'rgba(180,170,150,.55)');
        ctx.save();
        ctx.translate(o.x, o.y);
        ctx.strokeStyle = '#9a8a6a';
        ctx.lineWidth = 5;
        U.circle(ctx, 0, 0, o.r);
        ctx.stroke();
        ctx.strokeStyle = '#6a5c44';
        ctx.lineWidth = 2;
        U.circle(ctx, 0, 0, o.r * 0.55);
        ctx.stroke();
        ctx.restore();
        for (var cnd = 0; cnd < 4; cnd++) {
          var ca = (cnd / 4) * Math.PI * 2 + 0.4;
          var cx = o.x + Math.cos(ca) * o.r;
          var cy = o.y + Math.sin(ca) * o.r;
          ctx.fillStyle = '#e8dcc0';
          ctx.fillRect(cx - 2.5, cy - 12, 5, 12);
          U.flame(ctx, cx, cy - 12, 6, t + cnd * 25, 0.9);
        }

      } else if (o.type === 'flame') {
        ctx.fillStyle = '#4a3b2c';
        U.roundRect(ctx, o.x - o.r, o.y - 4, o.r * 2, 14, 5);
        ctx.fill();
        ctx.fillStyle = '#2e2419';
        ctx.fillRect(o.x - 4, o.y + 9, 8, 16);
        U.flame(ctx, o.x, o.y - 2, o.r * 0.85, t + o.x, 1);
      }
    }
  };

  FB.Room = Room;
})(window.FB = window.FB || {});
