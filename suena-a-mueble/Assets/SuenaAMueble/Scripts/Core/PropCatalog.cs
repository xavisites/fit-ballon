namespace SuenaAMueble
{
    public sealed class PropDef
    {
        /// <summary>Identificador. También es el nombre del .wav en Resources/Sonidos.</summary>
        public readonly string Id;
        public readonly string Nombre;
        /// <summary>"un" o "una", para las frases.</summary>
        public readonly string Articulo;
        public readonly string Onomatopeya;
        /// <summary>1 fácil, 2 medio, 3 difícil. Multiplica los puntos.</summary>
        public readonly int Dificultad;
        public readonly string Habitacion;
        /// <summary>Si la melodía (subir y bajar el tono) cuenta para la nota. Sobre todo animales.</summary>
        public readonly bool Melodia;

        public PropDef(string id, string nombre, string articulo, string onomatopeya, int dificultad, string habitacion,
                       bool melodia = false)
        {
            Id = id;
            Nombre = nombre;
            Articulo = articulo;
            Onomatopeya = onomatopeya;
            Dificultad = dificultad;
            Habitacion = habitacion;
            Melodia = melodia;
        }

        public string ConArticulo { get { return Articulo + " " + Nombre; } }
    }

    /// <summary>Los objetos de "Casa de la Abuela".</summary>
    public static class PropCatalog
    {
        public static readonly PropDef[] All =
        {
            // Fáciles (x1)
            new PropDef("reloj",       "reloj",               "un",  "tic-tac tic-tac",        1, "Salón"),
            new PropDef("nevera",      "nevera",              "una", "mmmmmmmm",               1, "Cocina"),
            new PropDef("grifo",       "grifo",               "un",  "plic... plic... plic",   1, "Baño"),
            new PropDef("pato",        "pato de goma",        "un",  "¡cuac! ¡cuac!",          1, "Baño", true),
            new PropDef("despertador", "despertador",         "un",  "pi-pi-pi-pi  pi-pi-pi-pi", 1, "Dormitorio"),
            new PropDef("gato",        "gato",                "un",  "miaaau",                 1, "Salón", true),
            // Medios (x2)
            new PropDef("microondas",  "microondas",          "un",  "mmmmmm... ¡PIP!",        2, "Cocina"),
            new PropDef("lavadora",    "lavadora",            "una", "brrrrrRRRRR",            2, "Baño"),
            new PropDef("telefono",    "teléfono antiguo",    "un",  "riiiing  riiiing",       2, "Salón"),
            new PropDef("olla",        "olla exprés",         "una", "psssssssss",             2, "Cocina"),
            new PropDef("aspiradora",  "aspiradora",          "una", "vvvvvvvvvv",             2, "Salón"),
            new PropDef("perro",       "perro",               "un",  "¡guau! ¡guau!",          2, "Salón", true),
            new PropDef("sarten",      "sartén",              "una", "chsssss chs chs",        2, "Cocina"),
            // Difíciles (x3)
            new PropDef("gallo",       "gallo",               "un",  "ki-ki-ri-kíííí",         3, "Garaje", true),
            new PropDef("motosierra",  "motosierra",          "una", "rin, rin... RRRRRRRR",   3, "Garaje"),
            new PropDef("vater",       "váter",               "un",  "shhhhhh glu-glu-glu-glu", 3, "Baño"),
            new PropDef("coche",       "coche que no arranca", "un", "rrr-rrr-rrr... ¡puf!",   3, "Garaje"),
            new PropDef("cortacesped", "cortacésped",         "un",  "brrrrrrrrr",             3, "Garaje"),
            new PropDef("licuadora",   "licuadora",           "una", "iiiiiIIIIIII",           3, "Cocina"),
        };

        public static PropDef Get(string id)
        {
            foreach (PropDef p in All) if (p.Id == id) return p;
            return null;
        }

        public static string Estrellas(int dificultad)
        {
            return dificultad == 1 ? "Fácil  x1" : (dificultad == 2 ? "Medio  x2" : "Difícil  x3");
        }
    }
}
