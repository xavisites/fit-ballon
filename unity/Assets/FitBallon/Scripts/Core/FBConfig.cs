using System.Collections.Generic;
using UnityEngine;

namespace FitBallon
{
    /// <summary>Identificadores de las partes del cuerpo con las que se toca el globo.</summary>
    public enum BodyPartId { Foot = 0, Knee = 1, Head = 2, Chest = 3, Hand = 4 }

    /// <summary>Datos de una parte: cuándo se desbloquea, cuánto impulsa y cuánto puntúa.</summary>
    public class BodyPartData
    {
        public BodyPartId Id;
        public string Name;
        public KeyCode Key;
        public string KeyLabel;
        public int UnlockLevel;
        public float Power;        // velocidad vertical que imprime al globo (m/s)
        public float Forward;      // componente hacia delante
        public int Points;
        public float Cooldown;     // segundos
        public float Duration;     // duración de la animación de golpeo
        public float ActiveFrom;   // ventana en la que el trigger está activo
        public float ActiveTo;
        public Color Color;

        public BodyPartData(BodyPartId id, string name, KeyCode key, string keyLabel, int unlockLevel,
                            float power, float forward, int points, float cooldown,
                            float duration, float activeFrom, float activeTo, Color color)
        {
            Id = id; Name = name; Key = key; KeyLabel = keyLabel; UnlockLevel = unlockLevel;
            Power = power; Forward = forward; Points = points; Cooldown = cooldown;
            Duration = duration; ActiveFrom = activeFrom; ActiveTo = activeTo; Color = color;
        }
    }

    /// <summary>Tabla de partes y constantes de ajuste del juego.</summary>
    public static class FBConfig
    {
        public const float BalloonRadius = 0.7f;
        public const float BalloonFallAccel = 3.0f;     // gravedad propia del globo (flota)
        public const float BalloonFallPerLevel = 0.22f;
        public const float BalloonFallMax = 5.5f;
        public const float BalloonDrag = 1.5f;   // con esta pareja cae a ~2 m/s y un toque de pie sube 2,6 m
        public const float BalloonMaxSpeed = 12f;
        public const float BalloonBounce = 0.82f;
        public const float BalloonPenaltyAccel = 1.2f;  // castigo de la pifia del d20
        public const float FloorDeathY = 0.75f;         // altura a la que se considera "tocó el suelo"

        public const float PlayerSpeed = 6.2f;
        public const float PlayerAccel = 28f;
        public const float PlayerJump = 6.2f;
        public const float PlayerGravity = -20f;
        public const float PlayerTurnSpeed = 14f;

        public const int StartLives = 3;
        public const int MaxLives = 5;

        public static int GoalFor(int level) { return 8 + (level - 1) * 5; }

        private static List<BodyPartData> _parts;

        public static List<BodyPartData> Parts
        {
            get
            {
                if (_parts == null)
                {
                    _parts = new List<BodyPartData>();
                    _parts.Add(new BodyPartData(BodyPartId.Foot, "Pie", KeyCode.Z, "Z", 1,
                        4.6f, 1.5f, 10, 0.32f, 0.38f, 0.08f, 0.24f, new Color(0.55f, 0.91f, 0.66f)));
                    _parts.Add(new BodyPartData(BodyPartId.Knee, "Rodilla", KeyCode.X, "X", 2,
                        3.9f, 0.9f, 20, 0.28f, 0.34f, 0.07f, 0.22f, new Color(0.54f, 0.83f, 1f)));
                    _parts.Add(new BodyPartData(BodyPartId.Head, "Cabeza", KeyCode.C, "C", 3,
                        4.3f, 1.2f, 35, 0.42f, 0.40f, 0.10f, 0.28f, new Color(1f, 0.83f, 0.47f)));
                    _parts.Add(new BodyPartData(BodyPartId.Chest, "Pecho", KeyCode.V, "V", 4,
                        2.8f, 0.5f, 25, 0.26f, 0.36f, 0.08f, 0.26f, new Color(1f, 0.62f, 0.80f)));
                    _parts.Add(new BodyPartData(BodyPartId.Hand, "Mano", KeyCode.B, "B", 5,
                        4.4f, 1.1f, 5, 0.60f, 0.32f, 0.06f, 0.20f, new Color(0.79f, 0.63f, 1f)));
                }
                return _parts;
            }
        }

        public static BodyPartData Get(BodyPartId id)
        {
            List<BodyPartData> list = Parts;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == id) return list[i];
            }
            return null;
        }
    }
}
