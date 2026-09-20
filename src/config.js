/* Configuración global del juego. */
(function (FB) {
  'use strict';

  FB.VIEW = { W: 960, H: 600 };
  FB.FLOOR_Y = 542;      // línea del suelo (si el globo la toca, pierdes)
  FB.CEIL_Y = 34;        // techo de la sala

  /* Partes del cuerpo: se desbloquean por nivel.
     power  = impulso vertical que le da al globo
     points = puntos base por toque (cuanto más difícil, más puntos)
     cd     = fotogramas de recarga
     dur    = duración de la animación
     active = ventana de fotogramas en la que la parte golpea de verdad */
  FB.PARTS = [
    { id: 'foot',  name: 'Pie',     key: 'KeyZ', keyLabel: 'Z', level: 1,
      power: 8.6, points: 10, cd: 16, dur: 22, active: [4, 14], color: '#8be9a8' },
    { id: 'knee',  name: 'Rodilla', key: 'KeyX', keyLabel: 'X', level: 2,
      power: 7.0, points: 20, cd: 14, dur: 20, active: [4, 13], color: '#8ad4ff' },
    { id: 'head',  name: 'Cabeza',  key: 'KeyC', keyLabel: 'C', level: 3,
      power: 7.9, points: 35, cd: 24, dur: 24, active: [5, 16], color: '#ffd479' },
    { id: 'chest', name: 'Pecho',   key: 'KeyV', keyLabel: 'V', level: 4,
      power: 4.8, points: 25, cd: 14, dur: 22, active: [5, 16], color: '#ff9ecb' },
    { id: 'hand',  name: 'Mano',    key: 'KeyB', keyLabel: 'B', level: 5,
      power: 8.2, points: 5,  cd: 36, dur: 20, active: [3, 12], color: '#c9a0ff' }
  ];

  FB.partById = function (id) {
    for (var i = 0; i < FB.PARTS.length; i++) {
      if (FB.PARTS[i].id === id) return FB.PARTS[i];
    }
    return null;
  };

  /* Toques necesarios para pasar de nivel. */
  FB.goalFor = function (level) { return 8 + (level - 1) * 5; };

  /* Física */
  FB.PHYS = {
    gravityBase: 0.135,     // gravedad del globo (flota, cae despacio)
    gravityPerLevel: 0.011,
    gravityMax: 0.30,
    gravityPenalty: 0.10,   // castigo de la pifia del d20
    airDrag: 0.9935,
    maxSpeed: 15,
    bounce: 0.86,           // rebote contra muros y obstáculos
    windPerLevel: 0.004,
    playerAccel: 0.95,
    playerFriction: 0.80,
    playerMaxSpeed: 5.4,
    playerJump: 13.2,
    playerGravity: 0.72
  };

  FB.DRUNK_HINTS = {
    '0': 'Control limpio. Para aventureros aburridos.',
    '0.45': 'Dos jarras. Te vas un poco de lado y a veces te lías con las teclas.',
    '0.9': 'Barril y medio. La sala da vueltas y tus piernas no obedecen.'
  };

  FB.STORAGE_KEY = 'fitballon.best.v1';
})(window.FB = window.FB || {});
