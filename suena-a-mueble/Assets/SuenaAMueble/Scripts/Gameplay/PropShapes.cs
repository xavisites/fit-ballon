using System.Collections.Generic;
using UnityEngine;

namespace SuenaAMueble
{
    /// <summary>
    /// Modelos provisionales hechos con cubos, cilindros y esferas.
    /// Cuando lleguen los modelos de verdad, se sustituyen aquí (o se cargan desde Resources/Modelos).
    /// El objeto mira hacia -Z (hacia la cámara) y se apoya en y = 0.
    /// </summary>
    public static class PropShapes
    {
        public struct Part
        {
            public PrimitiveType Type;
            public Vector3 Pos;
            public Vector3 Scale;
            public Vector3 Euler;
            public Color Color;

            public Part(PrimitiveType type, Vector3 pos, Vector3 scale, Color color, Vector3 euler = default(Vector3))
            {
                Type = type;
                Pos = pos;
                Scale = scale;
                Color = color;
                Euler = euler;
            }
        }

        private static Color C(string hex)
        {
            Color c;
            ColorUtility.TryParseHtmlString(hex, out c);
            return c;
        }

        private static readonly PrimitiveType Box = PrimitiveType.Cube;
        private static readonly PrimitiveType Cyl = PrimitiveType.Cylinder;
        private static readonly PrimitiveType Ball = PrimitiveType.Sphere;
        private static readonly PrimitiveType Caps = PrimitiveType.Capsule;
        private static readonly Vector3 Lying = new Vector3(90f, 0f, 0f);
        private static readonly Vector3 Sideways = new Vector3(0f, 0f, 90f);

        public static List<Part> For(string id)
        {
            List<Part> p = new List<Part>();
            switch (id)
            {
                case "reloj":
                    p.Add(new Part(Box, new Vector3(0, 0.9f, 0), new Vector3(0.5f, 1.8f, 0.35f), C("#8a5a3b")));
                    p.Add(new Part(Cyl, new Vector3(0, 1.45f, -0.18f), new Vector3(0.38f, 0.02f, 0.38f), C("#f4ecd8"), Lying));
                    p.Add(new Part(Box, new Vector3(0, 1.5f, -0.2f), new Vector3(0.025f, 0.14f, 0.01f), C("#222222")));
                    p.Add(new Part(Cyl, new Vector3(0, 0.75f, -0.18f), new Vector3(0.14f, 0.01f, 0.14f), C("#d9b44a"), Lying));
                    break;
                case "nevera":
                    p.Add(new Part(Box, new Vector3(0, 0.95f, 0), new Vector3(0.8f, 1.9f, 0.7f), C("#f2f4f5")));
                    p.Add(new Part(Box, new Vector3(0, 1.32f, -0.36f), new Vector3(0.78f, 0.015f, 0.02f), C("#b8c0c4")));
                    p.Add(new Part(Box, new Vector3(0.3f, 1.55f, -0.38f), new Vector3(0.04f, 0.3f, 0.04f), C("#9aa3a8")));
                    p.Add(new Part(Box, new Vector3(0.3f, 0.95f, -0.38f), new Vector3(0.04f, 0.4f, 0.04f), C("#9aa3a8")));
                    break;
                case "grifo":
                    p.Add(new Part(Box, new Vector3(0, 0.4f, 0), new Vector3(0.9f, 0.8f, 0.55f), C("#e8e8e8")));
                    p.Add(new Part(Box, new Vector3(0, 0.81f, -0.02f), new Vector3(0.6f, 0.02f, 0.35f), C("#9fb7c4")));
                    p.Add(new Part(Cyl, new Vector3(0, 0.95f, 0.18f), new Vector3(0.06f, 0.15f, 0.06f), C("#c0c6ca")));
                    p.Add(new Part(Cyl, new Vector3(0, 1.08f, 0.08f), new Vector3(0.05f, 0.12f, 0.05f), C("#c0c6ca"), Lying));
                    break;
                case "pato":
                    p.Add(new Part(Ball, new Vector3(0, 0.25f, 0.05f), new Vector3(0.6f, 0.45f, 0.7f), C("#ffd23f")));
                    p.Add(new Part(Ball, new Vector3(0, 0.6f, -0.15f), new Vector3(0.36f, 0.36f, 0.36f), C("#ffd23f")));
                    p.Add(new Part(Box, new Vector3(0, 0.57f, -0.36f), new Vector3(0.2f, 0.06f, 0.16f), C("#ff8c1a")));
                    break;
                case "despertador":
                    p.Add(new Part(Cyl, new Vector3(0, 0.32f, 0), new Vector3(0.55f, 0.1f, 0.55f), C("#d93b3b"), Lying));
                    p.Add(new Part(Cyl, new Vector3(0, 0.32f, -0.1f), new Vector3(0.44f, 0.01f, 0.44f), C("#fff8e7"), Lying));
                    p.Add(new Part(Ball, new Vector3(-0.17f, 0.62f, 0), new Vector3(0.18f, 0.18f, 0.18f), C("#c9a23a")));
                    p.Add(new Part(Ball, new Vector3(0.17f, 0.62f, 0), new Vector3(0.18f, 0.18f, 0.18f), C("#c9a23a")));
                    break;
                case "gato":
                    p.Add(new Part(Caps, new Vector3(0, 0.22f, 0.1f), new Vector3(0.4f, 0.35f, 0.4f), C("#8d8d8d"), Lying));
                    p.Add(new Part(Ball, new Vector3(0, 0.45f, -0.3f), new Vector3(0.36f, 0.32f, 0.32f), C("#8d8d8d")));
                    p.Add(new Part(Box, new Vector3(-0.1f, 0.63f, -0.3f), new Vector3(0.09f, 0.12f, 0.05f), C("#7a7a7a"), new Vector3(0, 0, 20)));
                    p.Add(new Part(Box, new Vector3(0.1f, 0.63f, -0.3f), new Vector3(0.09f, 0.12f, 0.05f), C("#7a7a7a"), new Vector3(0, 0, -20)));
                    p.Add(new Part(Caps, new Vector3(0.25f, 0.2f, 0.45f), new Vector3(0.08f, 0.25f, 0.08f), C("#7a7a7a"), new Vector3(30, 0, -40)));
                    break;
                case "microondas":
                    p.Add(new Part(Box, new Vector3(0, 0.45f, 0), new Vector3(1.4f, 0.9f, 0.6f), C("#c8c8c8")));
                    p.Add(new Part(Box, new Vector3(0, 0.45f, 0), new Vector3(0.6f, 0.4f, 0.4f), C("#5c5c5c")));
                    p.Add(new Part(Box, new Vector3(-0.15f, 0.45f, -0.3f), new Vector3(0.75f, 0.55f, 0.02f), C("#2b3a42")));
                    p.Add(new Part(Box, new Vector3(0.45f, 0.45f, -0.3f), new Vector3(0.25f, 0.6f, 0.02f), C("#3d3d3d")));
                    break;
                case "lavadora":
                    p.Add(new Part(Box, new Vector3(0, 0.45f, 0), new Vector3(0.8f, 0.9f, 0.7f), C("#f2f4f5")));
                    p.Add(new Part(Cyl, new Vector3(0, 0.42f, -0.36f), new Vector3(0.5f, 0.02f, 0.5f), C("#7f8c94"), Lying));
                    p.Add(new Part(Cyl, new Vector3(0, 0.42f, -0.37f), new Vector3(0.38f, 0.02f, 0.38f), C("#3b5566"), Lying));
                    p.Add(new Part(Box, new Vector3(0, 0.84f, -0.36f), new Vector3(0.78f, 0.1f, 0.02f), C("#d0d5d8")));
                    break;
                case "telefono":
                    p.Add(new Part(Box, new Vector3(0, 0.15f, 0), new Vector3(0.6f, 0.3f, 0.5f), C("#b3261e")));
                    p.Add(new Part(Caps, new Vector3(0, 0.38f, 0), new Vector3(0.14f, 0.32f, 0.14f), C("#8f1d17"), Sideways));
                    p.Add(new Part(Cyl, new Vector3(0, 0.2f, -0.26f), new Vector3(0.3f, 0.01f, 0.3f), C("#f0e6d2"), Lying));
                    break;
                case "olla":
                    p.Add(new Part(Cyl, new Vector3(0, 0.25f, 0), new Vector3(0.7f, 0.25f, 0.7f), C("#b8bec2")));
                    p.Add(new Part(Cyl, new Vector3(0, 0.52f, 0), new Vector3(0.72f, 0.03f, 0.72f), C("#8f969a")));
                    p.Add(new Part(Cyl, new Vector3(0, 0.62f, 0), new Vector3(0.08f, 0.08f, 0.08f), C("#3a3a3a")));
                    p.Add(new Part(Box, new Vector3(0.55f, 0.52f, 0), new Vector3(0.45f, 0.06f, 0.1f), C("#222222")));
                    break;
                case "aspiradora":
                    p.Add(new Part(Ball, new Vector3(0, 0.25f, 0.1f), new Vector3(0.6f, 0.5f, 0.75f), C("#e0412f")));
                    p.Add(new Part(Cyl, new Vector3(0.25f, 0.6f, -0.2f), new Vector3(0.06f, 0.45f, 0.06f), C("#9e9e9e"), new Vector3(0, 0, -25)));
                    p.Add(new Part(Box, new Vector3(0.45f, 0.05f, -0.35f), new Vector3(0.35f, 0.06f, 0.12f), C("#444444")));
                    break;
                case "perro":
                    p.Add(new Part(Caps, new Vector3(0, 0.35f, 0.1f), new Vector3(0.4f, 0.4f, 0.4f), C("#a8743f"), Lying));
                    p.Add(new Part(Ball, new Vector3(0, 0.62f, -0.3f), new Vector3(0.38f, 0.36f, 0.4f), C("#a8743f")));
                    p.Add(new Part(Box, new Vector3(0, 0.56f, -0.5f), new Vector3(0.16f, 0.12f, 0.16f), C("#7a5230")));
                    p.Add(new Part(Box, new Vector3(-0.2f, 0.62f, -0.28f), new Vector3(0.06f, 0.22f, 0.14f), C("#6b4528"), new Vector3(0, 0, 15)));
                    p.Add(new Part(Box, new Vector3(0.2f, 0.62f, -0.28f), new Vector3(0.06f, 0.22f, 0.14f), C("#6b4528"), new Vector3(0, 0, -15)));
                    p.Add(new Part(Cyl, new Vector3(-0.12f, 0.1f, -0.1f), new Vector3(0.1f, 0.1f, 0.1f), C("#a8743f")));
                    p.Add(new Part(Cyl, new Vector3(0.12f, 0.1f, -0.1f), new Vector3(0.1f, 0.1f, 0.1f), C("#a8743f")));
                    break;
                case "sarten":
                    p.Add(new Part(Cyl, new Vector3(0, 0.05f, 0), new Vector3(0.75f, 0.05f, 0.75f), C("#2b2b2b")));
                    p.Add(new Part(Box, new Vector3(0.65f, 0.07f, 0), new Vector3(0.6f, 0.05f, 0.09f), C("#4a3424")));
                    p.Add(new Part(Ball, new Vector3(-0.1f, 0.1f, -0.05f), new Vector3(0.22f, 0.04f, 0.22f), C("#ffe082")));
                    p.Add(new Part(Ball, new Vector3(-0.1f, 0.12f, -0.05f), new Vector3(0.09f, 0.04f, 0.09f), C("#ffb300")));
                    break;
                case "gallo":
                    p.Add(new Part(Ball, new Vector3(0, 0.4f, 0.1f), new Vector3(0.45f, 0.5f, 0.6f), C("#f5f0e6")));
                    p.Add(new Part(Ball, new Vector3(0, 0.78f, -0.15f), new Vector3(0.26f, 0.28f, 0.26f), C("#f5f0e6")));
                    p.Add(new Part(Box, new Vector3(0, 0.95f, -0.15f), new Vector3(0.05f, 0.14f, 0.2f), C("#d62828")));
                    p.Add(new Part(Box, new Vector3(0, 0.76f, -0.3f), new Vector3(0.08f, 0.06f, 0.12f), C("#f4a300")));
                    p.Add(new Part(Box, new Vector3(0, 0.6f, 0.4f), new Vector3(0.1f, 0.4f, 0.25f), C("#2e7d32"), new Vector3(-30, 0, 0)));
                    p.Add(new Part(Cyl, new Vector3(-0.1f, 0.08f, 0.1f), new Vector3(0.04f, 0.1f, 0.04f), C("#f4a300")));
                    p.Add(new Part(Cyl, new Vector3(0.1f, 0.08f, 0.1f), new Vector3(0.04f, 0.1f, 0.04f), C("#f4a300")));
                    break;
                case "motosierra":
                    p.Add(new Part(Box, new Vector3(-0.3f, 0.2f, 0), new Vector3(0.45f, 0.35f, 0.3f), C("#f57c00")));
                    p.Add(new Part(Box, new Vector3(0.3f, 0.2f, 0), new Vector3(0.8f, 0.12f, 0.03f), C("#9e9e9e")));
                    p.Add(new Part(Box, new Vector3(-0.35f, 0.45f, 0), new Vector3(0.3f, 0.05f, 0.05f), C("#222222")));
                    break;
                case "vater":
                    p.Add(new Part(Cyl, new Vector3(0, 0.22f, -0.05f), new Vector3(0.5f, 0.22f, 0.6f), C("#fbfbfb")));
                    p.Add(new Part(Cyl, new Vector3(0, 0.45f, -0.05f), new Vector3(0.52f, 0.02f, 0.62f), C("#e3e3e3")));
                    p.Add(new Part(Box, new Vector3(0, 0.65f, 0.3f), new Vector3(0.55f, 0.55f, 0.22f), C("#fbfbfb")));
                    p.Add(new Part(Box, new Vector3(0, 0.95f, 0.3f), new Vector3(0.12f, 0.03f, 0.08f), C("#c0c6ca")));
                    break;
                case "coche":
                    p.Add(new Part(Box, new Vector3(0, 0.35f, 0), new Vector3(1.6f, 0.4f, 0.8f), C("#1e88e5")));
                    p.Add(new Part(Box, new Vector3(0.1f, 0.7f, 0), new Vector3(0.9f, 0.35f, 0.75f), C("#1565c0")));
                    p.Add(new Part(Box, new Vector3(0.1f, 0.7f, -0.38f), new Vector3(0.8f, 0.25f, 0.02f), C("#b3e5fc")));
                    p.Add(new Part(Cyl, new Vector3(-0.5f, 0.15f, -0.4f), new Vector3(0.3f, 0.05f, 0.3f), C("#212121"), Lying));
                    p.Add(new Part(Cyl, new Vector3(0.5f, 0.15f, -0.4f), new Vector3(0.3f, 0.05f, 0.3f), C("#212121"), Lying));
                    p.Add(new Part(Cyl, new Vector3(-0.5f, 0.15f, 0.4f), new Vector3(0.3f, 0.05f, 0.3f), C("#212121"), Lying));
                    p.Add(new Part(Cyl, new Vector3(0.5f, 0.15f, 0.4f), new Vector3(0.3f, 0.05f, 0.3f), C("#212121"), Lying));
                    break;
                case "cortacesped":
                    p.Add(new Part(Box, new Vector3(0, 0.22f, 0), new Vector3(0.7f, 0.25f, 0.7f), C("#43a047")));
                    p.Add(new Part(Cyl, new Vector3(0, 0.38f, 0), new Vector3(0.3f, 0.08f, 0.3f), C("#2e7d32")));
                    p.Add(new Part(Cyl, new Vector3(0, 0.75f, 0.45f), new Vector3(0.04f, 0.45f, 0.04f), C("#424242"), new Vector3(-35, 0, 0)));
                    p.Add(new Part(Box, new Vector3(0, 1.1f, 0.7f), new Vector3(0.5f, 0.04f, 0.04f), C("#424242")));
                    p.Add(new Part(Cyl, new Vector3(-0.38f, 0.1f, -0.25f), new Vector3(0.2f, 0.03f, 0.2f), C("#212121"), Sideways));
                    p.Add(new Part(Cyl, new Vector3(0.38f, 0.1f, -0.25f), new Vector3(0.2f, 0.03f, 0.2f), C("#212121"), Sideways));
                    break;
                case "licuadora":
                    p.Add(new Part(Box, new Vector3(0, 0.12f, 0), new Vector3(0.4f, 0.24f, 0.4f), C("#263238")));
                    p.Add(new Part(Cyl, new Vector3(0, 0.5f, 0), new Vector3(0.3f, 0.26f, 0.3f), C("#b2dfdb")));
                    p.Add(new Part(Cyl, new Vector3(0, 0.78f, 0), new Vector3(0.32f, 0.02f, 0.32f), C("#263238")));
                    p.Add(new Part(Box, new Vector3(0, 0.12f, -0.21f), new Vector3(0.08f, 0.08f, 0.02f), C("#ff5252")));
                    break;
                default:
                    p.Add(new Part(Box, new Vector3(0, 0.4f, 0), new Vector3(0.8f, 0.8f, 0.8f), C("#9e9e9e")));
                    break;
            }
            return p;
        }
    }
}
