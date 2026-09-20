/* El aventurero: se mueve, salta y golpea el globo con las partes desbloqueadas. */
(function (FB) {
  'use strict';
  var U = FB.U, P = FB.PHYS;
  var W = FB.VIEW.W, F = FB.FLOOR_Y;

  function Player(drunk) {
    this.reset(drunk);
  }

  Player.prototype.reset = function (drunk) {
    this.x = W / 2;
    this.y = F;
    this.vx = 0;
    this.vy = 0;
    this.onGround = true;
    this.facing = 1;
    this.action = null;       // { part, t, used }
    this.cd = {};             // recargas por parte
    this.stun = 0;
    this.drunk = drunk || 0;
    this.swayPhase = U.rand(0, 6.28);
    this.invert = 0;          // fotogramas con los controles cambiados
    this.invertCd = 180;
    this.hiccup = 0;
    this.step = 0;
    this.lockMsgCd = 0;
  };

  /* ------------------------------------------------------------ acciones */
  Player.prototype.canUse = function (part, game) {
    if (this.stun > 0) return false;
    if (game.level < part.level) return false;
    if ((this.cd[part.id] || 0) > 0) return false;
    if (this.action) return false;
    return true;
  };

  Player.prototype.tryAction = function (partId, game) {
    var part = FB.partById(partId);
    if (!part) return false;
    if (!this.canUse(part, game)) {
      if (game.level < part.level && this.lockMsgCd <= 0) {
        this.lockMsgCd = 50;
        game.fx.text(this.x, this.y - 130,
          part.name.toUpperCase() + ' — nivel ' + part.level, '#c96a6a', 15);
      }
      return false;
    }
    this.action = { part: part, t: 0, used: false };
    this.cd[part.id] = part.cd + part.dur;
    return true;
  };

  /* Punto de golpeo activo, o null. */
  Player.prototype.hitbox = function () {
    if (!this.action) return null;
    var a = this.action, p = a.part;
    if (a.t < p.active[0] || a.t > p.active[1] || a.used) return null;
    var f = this.facing;
    var air = this.onGround ? 0 : -10;
    switch (p.id) {
      case 'foot':  return { x: this.x + f * 36, y: this.y - 18 + air, r: 23, part: p };
      case 'knee':  return { x: this.x + f * 22, y: this.y - 46, r: 18, part: p };
      case 'head':  return { x: this.x + f * 6,  y: this.y - 100, r: 19, part: p };
      case 'chest': return { x: this.x + f * 14, y: this.y - 64, r: 23, part: p };
      case 'hand':  return { x: this.x + f * 24, y: this.y - 116, r: 19, part: p };
    }
    return null;
  };

  /* Impulso que cada parte imprime al globo. */
  Player.prototype.applyKick = function (part, b) {
    var f = this.facing;
    var pv = this.vx * 0.45;
    switch (part.id) {
      case 'foot':
        b.kick(f * 3.1 + pv + U.rand(-0.4, 0.4), -part.power);
        break;
      case 'knee':
        b.kick(f * 1.5 + pv * 0.7 + U.rand(-0.3, 0.3), -part.power);
        break;
      case 'head':
        b.kick(f * 2.3 + U.rand(-0.4, 0.4), -part.power);
        break;
      case 'chest':
        b.kick(b.vx * 0.25 + f * 0.6, -part.power);
        break;
      case 'hand':
        b.kick(f * 2.0 + pv * 0.5, -part.power);
        break;
    }
    // La borrachera desvía el golpe.
    if (this.drunk > 0) {
      b.vx += U.rand(-2.4, 2.4) * this.drunk;
      b.vy += U.rand(-0.8, 0.8) * this.drunk;
    }
  };

  /* ------------------------------------------------------------ física */
  Player.prototype.update = function (dt, input, game) {
    var k;
    for (k in this.cd) {
      if (this.cd[k] > 0) this.cd[k] -= dt;
    }

    if (this.stun > 0) this.stun -= dt;
    if (this.lockMsgCd > 0) this.lockMsgCd -= dt;
    this.swayPhase += 0.035 * dt;

    // Controles cambiados por el alcohol.
    if (this.drunk > 0) {
      this.invertCd -= dt;
      if (this.invert > 0) {
        this.invert -= dt;
      } else if (this.invertCd <= 0) {
        this.invertCd = U.rand(200, 460) / (0.4 + this.drunk);
        if (Math.random() < this.drunk * 0.75) {
          this.invert = U.rand(50, 110);
          this.hiccup = 45;
          game.fx.text(this.x, this.y - 140, '¡hip!', '#ffd479', 16);
        }
      }
      if (this.hiccup > 0) this.hiccup -= dt;
    }

    var left = input.left, right = input.right;
    if (this.invert > 0) { var tmp = left; left = right; right = tmp; }

    if (this.stun <= 0) {
      if (left && !right) { this.vx -= P.playerAccel * dt; this.facing = -1; }
      else if (right && !left) { this.vx += P.playerAccel * dt; this.facing = 1; }
      else { this.vx *= Math.pow(P.playerFriction, dt); }

      if (input.up && this.onGround) {
        this.vy = -P.playerJump;
        this.onGround = false;
        game.fx.spark(this.x, this.y, '#6a5a70', 5, 1.8);
      }
    } else {
      this.vx *= Math.pow(0.9, dt);
    }

    // Vaivén de borracho.
    if (this.drunk > 0) {
      this.vx += Math.sin(this.swayPhase * 1.3) * 0.42 * this.drunk * dt;
    }

    this.vx = U.clamp(this.vx, -P.playerMaxSpeed, P.playerMaxSpeed);
    this.x += this.vx * dt;
    this.x = U.clamp(this.x, 28, W - 28);

    this.vy += P.playerGravity * dt;
    this.y += this.vy * dt;
    if (this.y >= F) {
      if (!this.onGround && this.vy > 6) game.fx.spark(this.x, F, '#4a3f50', 6, 2.4);
      this.y = F;
      this.vy = 0;
      this.onGround = true;
    }

    if (this.onGround && Math.abs(this.vx) > 0.4) this.step += Math.abs(this.vx) * 0.05 * dt;

    if (this.action) {
      this.action.t += dt;
      if (this.action.t >= this.action.part.dur) this.action = null;
    }
  };

  Player.prototype.knock = function (dir, game) {
    this.stun = 46;
    this.vx = dir * 5.5;
    this.vy = -5;
    this.onGround = false;
    this.action = null;
    game.fx.shake(6);
    game.fx.spark(this.x, this.y - 60, '#ffd479', 12, 3.4);
  };

  /* ------------------------------------------------------------- dibujo */
  Player.prototype.draw = function (ctx, t, game) {
    var x = this.x, y = this.y, f = this.facing;
    var sway = Math.sin(this.swayPhase * 1.3) * this.drunk * 7;
    var tilt = (this.stun > 0 ? Math.sin(t * 0.5) * 0.25 : 0) +
               Math.sin(this.swayPhase * 1.3) * this.drunk * 0.13;

    // Sombra.
    ctx.save();
    ctx.fillStyle = 'rgba(0,0,0,.4)';
    var sh = U.clamp(1 - (F - y) / 260, 0.25, 1);
    ctx.beginPath();
    ctx.ellipse(x, F + 4, 26 * sh, 7 * sh, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();

    var hb = this.hitbox();
    var a = this.action;
    var prog = a ? U.clamp(Math.sin(Math.PI * (a.t / a.part.dur)), 0, 1) : 0;
    var act = a ? a.part.id : null;

    // Posiciones base del cuerpo.
    var hipX = x + sway * 0.3, hipY = y - 48;
    var shX = x + sway * 0.7, shY = y - 80;
    var headX = x + sway, headY = y - 98;

    // Piernas.
    var walk = Math.sin(this.step) * (this.onGround ? 9 : 0);
    var footF = { x: x + f * 8 + walk, y: y };
    var footB = { x: x - f * 9 - walk, y: y };
    var kneeF = { x: (hipX + footF.x) / 2 + f * 3, y: (hipY + y) / 2 };
    var kneeB = { x: (hipX + footB.x) / 2 - f * 2, y: (hipY + y) / 2 };
    if (!this.onGround) {
      footF = { x: x + f * 12, y: y - 8 };
      footB = { x: x - f * 8, y: y - 2 };
    }

    if (act === 'foot' && hbTarget(this, 'foot')) {
      var tg = hbTarget(this, 'foot');
      footF = { x: U.lerp(x + f * 8, tg.x + f * 6, prog), y: U.lerp(y, tg.y, prog) };
      kneeF = { x: U.lerp(hipX, (hipX + footF.x) / 2 + f * 6, prog), y: U.lerp(hipY + 24, hipY + 16, prog) };
    } else if (act === 'knee') {
      var tk = hbTarget(this, 'knee');
      kneeF = { x: U.lerp(hipX, tk.x, prog), y: U.lerp(hipY + 24, tk.y, prog) };
      footF = { x: U.lerp(x + f * 8, kneeF.x - f * 6, prog), y: U.lerp(y, kneeF.y + 20, prog) };
    }

    drawLeg(ctx, hipX - f * 5, hipY, kneeB.x, kneeB.y, footB.x, footB.y, '#3a2c44', '#241a2c');
    // Torso.
    ctx.save();
    ctx.translate(hipX, hipY);
    ctx.rotate(tilt + (act === 'head' ? f * 0.3 * prog : 0) + (act === 'chest' ? -f * 0.18 * prog : 0));
    ctx.translate(-hipX, -hipY);

    var tg2 = ctx.createLinearGradient(0, shY, 0, hipY + 6);
    tg2.addColorStop(0, '#5b3f7a');
    tg2.addColorStop(1, '#3a2750');
    ctx.fillStyle = tg2;
    ctx.beginPath();
    ctx.moveTo(shX - 17, shY);
    ctx.lineTo(shX + 17, shY);
    ctx.lineTo(hipX + 13, hipY + 6);
    ctx.lineTo(hipX - 13, hipY + 6);
    ctx.closePath();
    ctx.fill();

    // Cinturón.
    ctx.fillStyle = '#6b4a2a';
    ctx.fillRect(hipX - 14, hipY - 4, 28, 7);
    ctx.fillStyle = '#e7c26a';
    ctx.fillRect(hipX - 4, hipY - 5, 8, 9);

    // Brazos.
    var handF = { x: shX + f * 14, y: shY + 26 };
    var handB = { x: shX - f * 12, y: shY + 24 };
    if (act === 'hand') {
      var th = hbTarget(this, 'hand');
      handF = { x: U.lerp(shX + f * 14, th.x, prog), y: U.lerp(shY + 26, th.y, prog) };
    } else if (act === 'chest') {
      handF = { x: shX + f * 22, y: shY + 8 };
      handB = { x: shX - f * 18, y: shY + 10 };
    }
    drawArm(ctx, shX - f * 6, shY + 4, handB.x, handB.y, '#3a2750');

    // Cabeza.
    var hx = headX, hy = headY;
    if (act === 'head') {
      var thd = hbTarget(this, 'head');
      hx = U.lerp(headX, thd.x, prog);
      hy = U.lerp(headY, thd.y, prog);
    }
    ctx.fillStyle = '#e8c39a';
    U.circle(ctx, hx, hy, 13);
    ctx.fill();
    // Nariz.
    ctx.fillStyle = '#d9ad84';
    U.circle(ctx, hx + f * 12, hy + 2, 3.2);
    ctx.fill();
    // Capucha: solo tapa la coronilla y la nuca, la cara queda a la vista.
    ctx.fillStyle = '#4a3366';
    ctx.beginPath();
    ctx.arc(hx, hy - 3, 14.5, Math.PI * 1.02, Math.PI * 2.02);
    ctx.closePath();
    ctx.fill();
    ctx.beginPath();
    ctx.moveTo(hx - f * 13, hy - 8);
    ctx.quadraticCurveTo(hx - f * 25, hy + 4, hx - f * 10, hy + 13);
    ctx.quadraticCurveTo(hx - f * 5, hy + 2, hx - f * 13, hy - 8);
    ctx.fill();
    // Cuello de la capa.
    ctx.fillStyle = '#3a2750';
    U.roundRect(ctx, hx - 10, hy + 11, 20, 6, 3);
    ctx.fill();
    // Ojos.
    ctx.fillStyle = '#1a1020';
    if (this.stun > 0) {
      U.line(ctx, hx + f * 2, hy - 3, hx + f * 8, hy + 2, 2, '#1a1020');
      U.line(ctx, hx + f * 8, hy - 3, hx + f * 2, hy + 2, 2, '#1a1020');
    } else {
      ctx.fillRect(hx + f * 2, hy - 2, 3, 4);
      ctx.fillRect(hx + f * 8, hy - 2, 3, 4);
    }
    // Mofletes de tabernero.
    if (this.drunk > 0) {
      ctx.fillStyle = 'rgba(220,90,90,' + (0.25 + this.drunk * 0.4) + ')';
      U.circle(ctx, hx + f * 11, hy + 4, 4);
      ctx.fill();
      U.circle(ctx, hx - f * 4, hy + 5, 3.4);
      ctx.fill();
    }

    drawArm(ctx, shX + f * 6, shY + 4, handF.x, handF.y, '#4a3366');
    ctx.restore();

    drawLeg(ctx, hipX + f * 4, hipY, kneeF.x, kneeF.y, footF.x, footF.y, '#4a3a58', '#2c2038');

    // Estela de la parte activa.
    if (hb) {
      ctx.save();
      ctx.globalCompositeOperation = 'lighter';
      var gg = ctx.createRadialGradient(hb.x, hb.y, 0, hb.x, hb.y, hb.r * 1.9);
      gg.addColorStop(0, hexA(hb.part.color, 0.55));
      gg.addColorStop(1, hexA(hb.part.color, 0));
      ctx.fillStyle = gg;
      U.circle(ctx, hb.x, hb.y, hb.r * 1.9);
      ctx.fill();
      ctx.restore();
    }

    if (game && game.debugHitboxes && hb) {
      ctx.save();
      ctx.strokeStyle = hb.part.color;
      ctx.lineWidth = 2;
      U.circle(ctx, hb.x, hb.y, hb.r);
      ctx.stroke();
      ctx.restore();
    }

    if (this.invert > 0) {
      ctx.save();
      ctx.textAlign = 'center';
      ctx.font = 'bold 13px Georgia, serif';
      ctx.fillStyle = '#ff9c6a';
      ctx.fillText('¡controles al revés!', x, y - 150);
      ctx.restore();
    }
  };

  /* Destino de la parte aunque la ventana activa haya pasado (para animar). */
  function hbTarget(pl, id) {
    var f = pl.facing;
    var air = pl.onGround ? 0 : -10;
    switch (id) {
      case 'foot':  return { x: pl.x + f * 36, y: pl.y - 18 + air };
      case 'knee':  return { x: pl.x + f * 22, y: pl.y - 46 };
      case 'head':  return { x: pl.x + f * 6,  y: pl.y - 100 };
      case 'chest': return { x: pl.x + f * 14, y: pl.y - 64 };
      case 'hand':  return { x: pl.x + f * 24, y: pl.y - 116 };
    }
    return null;
  }

  function drawLeg(ctx, hx, hy, kx, ky, fx, fy, color, boot) {
    U.line(ctx, hx, hy, kx, ky, 11, color);
    U.line(ctx, kx, ky, fx, fy, 9.5, color);
    ctx.save();
    ctx.fillStyle = boot;
    ctx.beginPath();
    ctx.ellipse(fx, fy - 2, 10, 6, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();
  }

  function drawArm(ctx, sx, sy, hx, hy, color) {
    var ex = (sx + hx) / 2 + 3, ey = (sy + hy) / 2;
    U.line(ctx, sx, sy, ex, ey, 8, color);
    U.line(ctx, ex, ey, hx, hy, 7, color);
    ctx.save();
    ctx.fillStyle = '#e8c39a';
    U.circle(ctx, hx, hy, 5);
    ctx.fill();
    ctx.restore();
  }

  function hexA(hex, a) {
    var v = hex.replace('#', '');
    var r = parseInt(v.substring(0, 2), 16);
    var g = parseInt(v.substring(2, 4), 16);
    var b = parseInt(v.substring(4, 6), 16);
    return 'rgba(' + r + ',' + g + ',' + b + ',' + a + ')';
  }

  FB.hexA = hexA;
  FB.Player = Player;
})(window.FB = window.FB || {});
