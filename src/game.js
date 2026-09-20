/* Bucle principal, estados, HUD y pegamento con el DOM. */
(function (FB) {
  'use strict';
  var U = FB.U;
  var W = FB.VIEW.W, H = FB.VIEW.H, F = FB.FLOOR_Y;

  function Game(canvas) {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d');
    this.room = new FB.Room();
    this.fx = new FB.FX();
    this.balloon = new FB.Balloon();
    this.player = new FB.Player(0);
    this.drunks = [];
    this.state = 'menu';
    this.t = 0;
    this.debugHitboxes = false;
    this.input = { left: false, right: false, up: false };
    this.best = loadBest();
    this.banner = null;
    this.reset(0);
  }

  /* ------------------------------------------------------------- arranque */
  Game.prototype.reset = function (drunkLevel) {
    this.level = 1;
    this.lives = 3;
    this.score = 0;
    this.touches = 0;
    this.levelTouches = 0;
    this.combo = 0;
    this.comboTimer = 0;
    this.penaltyTimer = 0;
    this.freeze = 90;
    this.lastPart = null;
    this.baseDrunk = drunkLevel;
    this.player.reset(drunkLevel);
    this.balloon.reset(1);
    this.room.build(1);
    this.drunks = [];
    this.fx.clear();
    this.banner = null;
  };

  Game.prototype.start = function (drunkLevel) {
    this.reset(drunkLevel);
    this.state = 'playing';
  };

  /* --------------------------------------------------------------- puntos */
  Game.prototype.addScore = function (pts, x, y, text, color) {
    this.score += Math.round(pts);
    if (text) this.fx.text(x, y, text, color, 17);
  };

  Game.prototype.multiplier = function () {
    return Math.min(4, 1 + Math.floor(this.combo / 4) * 0.5);
  };

  Game.prototype.onTouch = function (hb) {
    var part = hb.part;
    this.combo++;
    this.comboTimer = 200;
    this.touches++;
    this.levelTouches++;
    this.lastPart = part.id;

    var mult = this.multiplier();
    var pts = part.points * mult;
    // Bonus por variar de parte.
    var label = '+' + Math.round(pts);
    if (mult > 1) label += ' x' + mult;
    this.addScore(pts, hb.x, hb.y - 24, label, part.color);
    this.fx.spark(hb.x, hb.y, part.color, 10, 3.2);
    this.fx.shake(2);

    if (this.levelTouches >= FB.goalFor(this.level)) this.levelUp();
  };

  /* ---------------------------------------------------------- progresión */
  Game.prototype.levelUp = function () {
    this.level++;
    this.levelTouches = 0;
    this.room.build(this.level);
    if (this.level === 2 || this.level === 4 || this.level === 6) this.addDrunk();

    var unlocked = null;
    for (var i = 0; i < FB.PARTS.length; i++) {
      if (FB.PARTS[i].level === this.level) unlocked = FB.PARTS[i];
    }

    var roll = this.rollD20();
    this.banner = {
      t: 0, dur: 230, level: this.level,
      unlocked: unlocked, roll: roll.n, rollText: roll.text
    };
    this.state = 'levelup';
    this.fx.shake(5);
  };

  Game.prototype.addDrunk = function () {
    this.drunks.push(new FB.Drunk());
  };

  Game.prototype.rollD20 = function () {
    var n = U.randInt(1, 20);
    var text;
    if (n === 20) {
      this.lives = Math.min(5, this.lives + 1);
      text = '¡CRÍTICO! El tabernero te regala una vida.';
    } else if (n >= 15) {
      this.score += 200;
      text = 'Buena tirada: +200 puntos.';
    } else if (n >= 10) {
      text = 'Nada digno de mención.';
    } else if (n >= 6) {
      this.addDrunk();
      text = 'Entra otro parroquiano dando tumbos.';
    } else if (n >= 2) {
      this.player.drunk = Math.min(1, this.player.drunk + 0.25);
      text = 'Te invitan a una ronda. No puedes decir que no.';
    } else {
      this.penaltyTimer = 900;
      text = '¡PIFIA! Alguien ha hechizado el globo: pesa más (15 s).';
    }
    return { n: n, text: text };
  };

  Game.prototype.loseLife = function (reason) {
    this.lives--;
    this.combo = 0;
    this.fx.pop(this.balloon.x, this.balloon.y, 'hsl(' + this.balloon.hue + ',85%,62%)');
    this.fx.text(this.balloon.x, this.balloon.y - 40, reason, '#ff8f7a', 19);
    if (this.lives <= 0) {
      this.state = 'gameover';
      this.lastReason = reason;
      if (this.score > this.best) { this.best = this.score; saveBest(this.best); }
      if (this.onGameOver) this.onGameOver();
    } else {
      this.balloon.reset(this.level);
      this.freeze = 80;
    }
  };

  /* ------------------------------------------------------------ actualizar */
  Game.prototype.update = function (dt) {
    this.t += dt;
    this.fx.update(dt);

    if (this.state === 'levelup') {
      this.banner.t += dt;
      this.room.update(dt);
      if (this.banner.t >= this.banner.dur) {
        this.banner = null;
        this.state = 'playing';
        this.freeze = 50;
      }
      return;
    }
    if (this.state !== 'playing') return;

    if (this.penaltyTimer > 0) this.penaltyTimer -= dt;
    if (this.comboTimer > 0) {
      this.comboTimer -= dt;
      if (this.comboTimer <= 0) this.combo = 0;
    }

    this.player.update(dt, this.input, this);
    this.room.update(dt);

    var i;
    for (i = 0; i < this.drunks.length; i++) {
      this.drunks[i].update(dt, this);
      this.drunks[i].collidePlayer(this.player, this);
    }

    if (this.freeze > 0) {
      this.freeze -= dt;
      this.balloon.wobble += 0.03 * dt;
      return;
    }

    this.balloon.update(dt, this);

    // Golpe del jugador.
    var hb = this.player.hitbox();
    if (hb && U.circleHit(hb.x, hb.y, hb.r, this.balloon.x, this.balloon.y, this.balloon.r)) {
      this.player.action.used = true;
      this.player.applyKick(hb.part, this.balloon);
      // Separa el globo para que no se quede pegado.
      var dx = this.balloon.x - hb.x, dy = this.balloon.y - hb.y;
      var d = Math.sqrt(dx * dx + dy * dy) || 0.001;
      this.balloon.x = hb.x + (dx / d) * (hb.r + this.balloon.r);
      this.balloon.y = hb.y + (dy / d) * (hb.r + this.balloon.r);
      this.onTouch(hb);
    }

    // Obstáculos de la sala.
    if (this.room.collide(this.balloon, this) === 'pop') {
      this.loseLife('¡El globo ha ardido!');
      return;
    }

    // Borrachos.
    for (i = 0; i < this.drunks.length; i++) {
      this.drunks[i].collideBalloon(this.balloon, this);
    }

    // Suelo.
    if (this.balloon.y + this.balloon.r >= F) {
      this.balloon.y = F - this.balloon.r;
      this.loseLife('¡El globo ha tocado el suelo!');
    }
  };

  /* ---------------------------------------------------------------- pintar */
  Game.prototype.draw = function () {
    var ctx = this.ctx;
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.clearRect(0, 0, W, H);

    ctx.save();
    // Temblor + mareo.
    var tilt = this.player.drunk * Math.sin(this.t * 0.012) * 0.018;
    ctx.translate(W / 2 + this.fx.shakeX, H / 2 + this.fx.shakeY);
    ctx.rotate(tilt);
    ctx.scale(1.02, 1.02);
    ctx.translate(-W / 2, -H / 2);

    this.room.drawBack(ctx, this.t);
    this.room.drawFront(ctx, this.t);

    for (var i = 0; i < this.drunks.length; i++) this.drunks[i].draw(ctx, this.t);

    if (this.freeze > 0 && this.state === 'playing') {
      ctx.save();
      ctx.globalAlpha = 0.5 + Math.sin(this.t * 0.2) * 0.3;
      ctx.strokeStyle = '#ffd479';
      ctx.lineWidth = 3;
      U.circle(ctx, this.balloon.x, this.balloon.y, this.balloon.r + 10);
      ctx.stroke();
      ctx.restore();
    }

    this.balloon.draw(ctx, this.t);
    this.player.draw(ctx, this.t, this);
    this.fx.draw(ctx);
    ctx.restore();

    this.drawHud(ctx);
    if (this.state === 'levelup') this.drawBanner(ctx);
  };

  Game.prototype.drawHud = function (ctx) {
    var i;
    ctx.save();
    ctx.fillStyle = 'rgba(10,8,14,.72)';
    ctx.fillRect(0, 0, W, 32);
    ctx.strokeStyle = 'rgba(231,194,106,.25)';
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.moveTo(0, 32.5); ctx.lineTo(W, 32.5); ctx.stroke();

    // Vidas.
    for (i = 0; i < this.lives; i++) heart(ctx, 22 + i * 26, 16, 9, '#c94a5a');

    // Nivel y progreso.
    ctx.textAlign = 'center';
    ctx.font = 'bold 15px Georgia, serif';
    ctx.fillStyle = '#e7c26a';
    ctx.fillText('NIVEL ' + this.level, W / 2, 21);

    var goal = FB.goalFor(this.level);
    var bw = 190, bx = W / 2 - bw / 2, by = 25;
    ctx.fillStyle = 'rgba(255,255,255,.12)';
    U.roundRect(ctx, bx, by, bw, 5, 2.5); ctx.fill();
    ctx.fillStyle = '#8be9a8';
    U.roundRect(ctx, bx, by, bw * U.clamp(this.levelTouches / goal, 0, 1), 5, 2.5); ctx.fill();

    // Puntuación.
    ctx.textAlign = 'right';
    ctx.font = 'bold 18px Georgia, serif';
    ctx.fillStyle = '#f2e6d2';
    ctx.fillText(this.score.toLocaleString('es-ES'), W - 16, 22);
    ctx.textAlign = 'left';
    ctx.font = '11px Georgia, serif';
    ctx.fillStyle = '#8b7a63';
    ctx.fillText('récord ' + this.best.toLocaleString('es-ES'), 30 + this.lives * 26, 21);

    // Combo.
    if (this.combo >= 4) {
      ctx.textAlign = 'center';
      ctx.font = 'bold 15px Georgia, serif';
      ctx.fillStyle = '#ffd479';
      ctx.globalAlpha = U.clamp(this.comboTimer / 60, 0.35, 1);
      ctx.fillText('COMBO ' + this.combo + '  ×' + this.multiplier(), W / 2, 50);
      ctx.globalAlpha = 1;
    }

    // Castigo del d20.
    if (this.penaltyTimer > 0) {
      ctx.textAlign = 'left';
      ctx.font = '12px Georgia, serif';
      ctx.fillStyle = '#ff8f7a';
      ctx.fillText('globo hechizado ' + Math.ceil(this.penaltyTimer / 60) + 's', 16, 50);
    }

    // Tira de partes del cuerpo.
    var n = FB.PARTS.length, cw = 82, gap = 8;
    var totalW = n * cw + (n - 1) * gap;
    var sx = W / 2 - totalW / 2, sy = H - 44;
    for (i = 0; i < n; i++) {
      var p = FB.PARTS[i];
      var unlocked = this.level >= p.level;
      var cx = sx + i * (cw + gap);
      var cdLeft = this.player.cd[p.id] || 0;
      var ready = unlocked && cdLeft <= 0;

      ctx.fillStyle = unlocked ? 'rgba(20,16,26,.85)' : 'rgba(12,10,16,.7)';
      U.roundRect(ctx, cx, sy, cw, 34, 5); ctx.fill();
      ctx.strokeStyle = ready ? FB.hexA(p.color, 0.85) : 'rgba(255,255,255,.12)';
      ctx.lineWidth = ready ? 2 : 1;
      U.roundRect(ctx, cx, sy, cw, 34, 5); ctx.stroke();

      if (!ready && unlocked) {
        var frac = U.clamp(1 - cdLeft / (p.cd + p.dur), 0, 1);
        ctx.fillStyle = FB.hexA(p.color, 0.22);
        U.roundRect(ctx, cx, sy, cw * frac, 34, 5); ctx.fill();
      }

      ctx.textAlign = 'left';
      ctx.font = 'bold 13px Georgia, serif';
      ctx.fillStyle = unlocked ? '#f2e6d2' : '#5a5060';
      ctx.fillText(p.name, cx + 26, sy + 15);
      ctx.font = '10px Georgia, serif';
      ctx.fillStyle = unlocked ? '#8b7a63' : '#4a4250';
      ctx.fillText(unlocked ? p.points + ' pts' : 'nivel ' + p.level, cx + 26, sy + 27);

      // Tecla o candado.
      ctx.textAlign = 'center';
      if (unlocked) {
        ctx.fillStyle = FB.hexA(p.color, ready ? 0.95 : 0.35);
        U.roundRect(ctx, cx + 6, sy + 8, 17, 18, 3); ctx.fill();
        ctx.fillStyle = '#1a1020';
        ctx.font = 'bold 12px Georgia, serif';
        ctx.fillText(p.keyLabel, cx + 14.5, sy + 21);
      } else {
        padlock(ctx, cx + 14.5, sy + 17);
      }
    }

    // Aviso de los primeros segundos.
    if (this.level === 1 && this.touches === 0 && this.state === 'playing') {
      ctx.textAlign = 'center';
      ctx.font = 'italic 17px Georgia, serif';
      ctx.fillStyle = 'rgba(242,230,210,' + (0.45 + Math.sin(this.t * 0.06) * 0.25) + ')';
      ctx.fillText('Solo con los pies: coloca la bota debajo y pulsa Z', W / 2, H - 62);
    }

    // Jarras de borrachera.
    if (this.player.drunk > 0) {
      var jars = Math.round(this.player.drunk * 4);
      ctx.textAlign = 'left';
      ctx.font = '11px Georgia, serif';
      ctx.fillStyle = '#8b7a63';
      ctx.fillText('borrachera', 16, H - 30);
      for (i = 0; i < jars; i++) mug(ctx, 22 + i * 16, H - 16);
    }
    ctx.restore();
  };

  Game.prototype.drawBanner = function (ctx) {
    var b = this.banner;
    var a = U.clamp(Math.min(b.t / 18, (b.dur - b.t) / 24), 0, 1);
    ctx.save();
    ctx.globalAlpha = a;
    ctx.fillStyle = 'rgba(8,6,12,.78)';
    ctx.fillRect(0, 150, W, 300);
    ctx.strokeStyle = 'rgba(231,194,106,.4)';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(0, 150.5); ctx.lineTo(W, 150.5);
    ctx.moveTo(0, 449.5); ctx.lineTo(W, 449.5);
    ctx.stroke();

    ctx.textAlign = 'center';
    ctx.font = 'bold 44px Georgia, serif';
    ctx.fillStyle = '#e7c26a';
    ctx.fillText('NIVEL ' + b.level, W / 2, 210);

    if (b.unlocked) {
      ctx.font = 'bold 26px Georgia, serif';
      ctx.fillStyle = b.unlocked.color;
      ctx.fillText('¡' + b.unlocked.name.toUpperCase() + ' DESBLOQUEADA!  [' + b.unlocked.keyLabel + ']',
        W / 2, 254);
    } else {
      ctx.font = 'italic 20px Georgia, serif';
      ctx.fillStyle = '#b9a88f';
      ctx.fillText('La sala se complica…', W / 2, 254);
    }

    // Tirada de d20.
    d20(ctx, W / 2, 330, 46, b.roll);
    ctx.font = 'italic 17px Georgia, serif';
    ctx.fillStyle = '#c9bba4';
    ctx.fillText(b.rollText, W / 2, 412);
    ctx.font = '12px Georgia, serif';
    ctx.fillStyle = '#7a6b58';
    ctx.fillText('pulsa cualquier tecla para seguir', W / 2, 436);
    ctx.restore();
  };

  /* -------------------------------------------------------------- iconos */
  function heart(ctx, x, y, r, color) {
    ctx.save();
    ctx.fillStyle = color;
    ctx.beginPath();
    ctx.moveTo(x, y + r * 0.85);
    ctx.bezierCurveTo(x - r * 1.4, y - r * 0.25, x - r * 0.5, y - r * 1.2, x, y - r * 0.35);
    ctx.bezierCurveTo(x + r * 0.5, y - r * 1.2, x + r * 1.4, y - r * 0.25, x, y + r * 0.85);
    ctx.closePath();
    ctx.fill();
    ctx.restore();
  }

  function padlock(ctx, x, y) {
    ctx.save();
    ctx.strokeStyle = '#5a5060';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.arc(x, y - 3, 4, Math.PI, 0);
    ctx.stroke();
    ctx.fillStyle = '#5a5060';
    U.roundRect(ctx, x - 6, y - 3, 12, 10, 2);
    ctx.fill();
    ctx.restore();
  }

  function mug(ctx, x, y) {
    ctx.save();
    ctx.fillStyle = '#b9a88a';
    U.roundRect(ctx, x - 5, y - 8, 10, 13, 2);
    ctx.fill();
    ctx.strokeStyle = '#b9a88a';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.arc(x + 7, y - 2, 3.5, -1.2, 1.2);
    ctx.stroke();
    ctx.fillStyle = '#f0e6c8';
    ctx.beginPath();
    ctx.ellipse(x, y - 8, 5, 2.2, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();
  }

  function d20(ctx, x, y, r, n) {
    ctx.save();
    ctx.translate(x, y);
    var g = ctx.createLinearGradient(0, -r, 0, r);
    g.addColorStop(0, '#6f4fa0');
    g.addColorStop(1, '#2e1f45');
    ctx.fillStyle = g;
    ctx.strokeStyle = '#e7c26a';
    ctx.lineWidth = 2.5;
    ctx.beginPath();
    for (var i = 0; i < 6; i++) {
      var a = (i / 6) * Math.PI * 2 - Math.PI / 2;
      var px = Math.cos(a) * r, py = Math.sin(a) * r;
      if (i === 0) ctx.moveTo(px, py); else ctx.lineTo(px, py);
    }
    ctx.closePath();
    ctx.fill();
    ctx.stroke();

    ctx.strokeStyle = 'rgba(231,194,106,.3)';
    ctx.lineWidth = 1.2;
    ctx.beginPath();
    for (var k = 0; k < 3; k++) {
      var b = (k / 3) * Math.PI * 2 - Math.PI / 2;
      ctx.moveTo(Math.cos(b) * r, Math.sin(b) * r);
      ctx.lineTo(Math.cos(b + Math.PI) * r, Math.sin(b + Math.PI) * r);
    }
    ctx.stroke();

    ctx.textAlign = 'center';
    ctx.font = 'bold 30px Georgia, serif';
    ctx.fillStyle = n === 20 ? '#8be9a8' : (n === 1 ? '#ff7a7a' : '#f2e6d2');
    ctx.fillText(String(n), 0, 11);
    ctx.restore();
  }

  /* -------------------------------------------------- récord persistente */
  function loadBest() {
    try {
      return parseInt(localStorage.getItem(FB.STORAGE_KEY) || '0', 10) || 0;
    } catch (e) { return 0; }
  }
  function saveBest(v) {
    try { localStorage.setItem(FB.STORAGE_KEY, String(v)); } catch (e) { /* sin almacenamiento */ }
  }

  FB.Game = Game;

  /* ====================================================================== */
  /*                              ARRANQUE                                  */
  /* ====================================================================== */
  function boot() {
    var canvas = document.getElementById('game');
    var game = new Game(canvas);
    FB.game = game;

    var el = {
      menu: document.getElementById('menu'),
      pause: document.getElementById('pause'),
      over: document.getElementById('over'),
      touch: document.getElementById('touch'),
      touchParts: document.getElementById('touchParts'),
      drunkOpts: document.getElementById('drunkOpts'),
      drunkHint: document.getElementById('drunkHint')
    };
    var drunkChoice = 0;

    function show(node) {
      [el.menu, el.pause, el.over].forEach(function (n) { n.classList.add('hidden'); });
      if (node) node.classList.remove('hidden');
      el.touch.classList.toggle('hidden', node !== null || !isTouch());
    }
    function isTouch() {
      return ('ontouchstart' in window) || navigator.maxTouchPoints > 0;
    }

    // Botones de borrachera.
    Array.prototype.forEach.call(el.drunkOpts.children, function (btn) {
      btn.addEventListener('click', function () {
        Array.prototype.forEach.call(el.drunkOpts.children, function (b) {
          b.classList.remove('selected');
        });
        btn.classList.add('selected');
        drunkChoice = parseFloat(btn.dataset.drunk);
        el.drunkHint.textContent = FB.DRUNK_HINTS[btn.dataset.drunk] || '';
      });
    });

    document.getElementById('btnStart').addEventListener('click', function () {
      game.start(drunkChoice); show(null);
    });
    document.getElementById('btnRetry').addEventListener('click', function () {
      game.start(drunkChoice); show(null);
    });
    document.getElementById('btnMenu').addEventListener('click', function () {
      game.state = 'menu'; show(el.menu);
    });
    document.getElementById('btnResume').addEventListener('click', function () {
      game.state = 'playing'; show(null);
    });
    document.getElementById('btnQuit').addEventListener('click', function () {
      game.state = 'menu'; show(el.menu);
    });

    game.onGameOver = function () {
      document.getElementById('overReason').textContent = game.lastReason || '';
      document.getElementById('ovScore').textContent = game.score.toLocaleString('es-ES');
      document.getElementById('ovLevel').textContent = game.level;
      document.getElementById('ovTouches').textContent = game.touches;
      document.getElementById('ovBest').textContent = game.best.toLocaleString('es-ES');
      show(el.over);
    };

    /* ------------------------------------------------------------ teclado */
    var MOVE = {
      ArrowLeft: 'left', KeyA: 'left',
      ArrowRight: 'right', KeyD: 'right',
      ArrowUp: 'up', KeyW: 'up', Space: 'up'
    };
    var ACTION = {};
    FB.PARTS.forEach(function (p) { ACTION[p.key] = p.id; });

    function togglePause() {
      if (game.state === 'playing') { game.state = 'paused'; show(el.pause); }
      else if (game.state === 'paused') { game.state = 'playing'; show(null); }
    }

    window.addEventListener('keydown', function (e) {
      if (e.code === 'Space' || e.code.indexOf('Arrow') === 0) e.preventDefault();

      if (game.state === 'levelup') { game.banner.t = game.banner.dur; return; }
      if (e.code === 'KeyP' || e.code === 'Escape') { togglePause(); return; }
      if (e.code === 'KeyH') { game.debugHitboxes = !game.debugHitboxes; return; }
      if (game.state !== 'playing') {
        if (e.code === 'Enter' || e.code === 'Space') {
          if (game.state === 'menu' || game.state === 'gameover') {
            game.start(drunkChoice); show(null);
          }
        }
        return;
      }
      if (MOVE[e.code]) { game.input[MOVE[e.code]] = true; return; }
      if (ACTION[e.code] && !e.repeat) game.player.tryAction(ACTION[e.code], game);
    });

    window.addEventListener('keyup', function (e) {
      if (MOVE[e.code]) game.input[MOVE[e.code]] = false;
    });

    window.addEventListener('blur', function () {
      game.input.left = game.input.right = game.input.up = false;
    });

    /* -------------------------------------------------------------- táctil */
    FB.PARTS.forEach(function (p) {
      var b = document.createElement('button');
      b.className = 'tbtn';
      b.textContent = p.name;
      b.dataset.part = p.id;
      el.touchParts.appendChild(b);
    });

    function bindTouch(btn) {
      var down = function (ev) {
        ev.preventDefault();
        if (btn.dataset.part) {
          if (game.state === 'playing') game.player.tryAction(btn.dataset.part, game);
        } else if (MOVE[btn.dataset.key]) {
          game.input[MOVE[btn.dataset.key]] = true;
        }
      };
      var up = function (ev) {
        ev.preventDefault();
        if (MOVE[btn.dataset.key]) game.input[MOVE[btn.dataset.key]] = false;
      };
      btn.addEventListener('pointerdown', down);
      btn.addEventListener('pointerup', up);
      btn.addEventListener('pointercancel', up);
      btn.addEventListener('pointerleave', up);
    }
    Array.prototype.forEach.call(document.querySelectorAll('#touch .tbtn'), bindTouch);

    function refreshTouchButtons() {
      Array.prototype.forEach.call(el.touchParts.children, function (b) {
        if (!b.dataset.part) return;
        var p = FB.partById(b.dataset.part);
        b.disabled = game.level < p.level;
      });
    }

    /* ---------------------------------------------------------- bucle 60Hz */
    var last = performance.now(), acc = 0;
    var STEP = 1000 / 60;

    function frame(now) {
      var elapsed = Math.min(now - last, 250);
      last = now;
      acc += elapsed;
      var guard = 0;
      while (acc >= STEP && guard < 6) {
        game.update(1);
        acc -= STEP;
        guard++;
      }
      if (guard >= 6) acc = 0;
      game.draw();
      refreshTouchButtons();
      requestAnimationFrame(frame);
    }

    show(el.menu);
    requestAnimationFrame(frame);
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', boot);
  } else {
    boot();
  }
})(window.FB = window.FB || {});
