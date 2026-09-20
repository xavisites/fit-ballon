using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FitBallon.EditorTools
{
    /// <summary>
    /// Monta la sala entera con formas básicas para poder jugar sin haber importado
    /// todavía ningún modelo. Menú: Fit Ballon ▸ Crear escena jugable.
    /// Después se sustituyen las cajas por los assets de KayKit sin tocar los scripts.
    /// </summary>
    public static class FitBallonSceneBuilder
    {
        private const float RoomHalf = 9f;
        private const float WallHeight = 9f;

        [MenuItem("Fit Ballon/Crear escena jugable", false, 0)]
        public static void CreateScene()
        {
            if (!EditorUtility.DisplayDialog("Fit Ballon",
                    "Se va a crear una escena nueva con la sala montada.\n\n" +
                    "Si tienes cambios sin guardar en la escena actual, guárdalos antes.",
                    "Crear", "Cancelar"))
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildRoom();

            string dir = "Assets/FitBallon/Scenes";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();

            string path = dir + "/Sala.unity";
            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.Refresh();
            Debug.Log("Fit Ballon: escena creada en " + path + ". Dale a Play.");
        }

        [MenuItem("Fit Ballon/Añadir sala a la escena actual", false, 1)]
        public static void AddToCurrentScene()
        {
            BuildRoom();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        // ------------------------------------------------------------------ sala
        private static void BuildRoom()
        {
            GameObject root = new GameObject("SALA");
            GameObject center = new GameObject("CentroSala");
            center.transform.SetParent(root.transform);
            center.transform.position = Vector3.zero;

            BuildShell(root.transform);
            BuildObstacles(root.transform);

            BalloonController balloon = BuildBalloon(root.transform);
            PlayerController player = BuildPlayer(root.transform);
            GameObject drunkTemplate = BuildDrunkTemplate(root.transform);
            Camera cam = BuildCamera(root.transform, player.transform, balloon.transform);
            BuildLights(root.transform);

            player.cameraPivot = cam.transform;

            GameObject gmGo = new GameObject("GameManager");
            gmGo.transform.SetParent(root.transform);
            GameManager gm = gmGo.AddComponent<GameManager>();
            gm.player = player;
            gm.balloon = balloon;
            gm.roomCenter = center.transform;
            gm.drunkTemplate = drunkTemplate;
            gm.startInMenu = true;

            HudOnGui hud = gmGo.AddComponent<HudOnGui>();
            hud.game = gm;
            hud.cam = cam;

            // Un primer parroquiano ya en la sala.
            GameObject firstDrunk = Object.Instantiate(drunkTemplate, new Vector3(4f, 0f, 3f),
                Quaternion.identity);
            firstDrunk.name = "Borracho";
            firstDrunk.SetActive(true);
            firstDrunk.transform.SetParent(root.transform);

            Selection.activeGameObject = root;
        }

        private static void BuildShell(Transform parent)
        {
            Color stone = new Color(0.36f, 0.31f, 0.40f);
            Color floorCol = new Color(0.26f, 0.22f, 0.30f);

            Box(parent, "Suelo", new Vector3(0f, -0.5f, 0f),
                new Vector3(RoomHalf * 2f, 1f, RoomHalf * 2f), floorCol);
            Box(parent, "Techo", new Vector3(0f, WallHeight + 0.5f, 0f),
                new Vector3(RoomHalf * 2f, 1f, RoomHalf * 2f), stone);

            Box(parent, "Muro_N", new Vector3(0f, WallHeight * 0.5f, RoomHalf + 0.5f),
                new Vector3(RoomHalf * 2f + 2f, WallHeight, 1f), stone);
            Box(parent, "Muro_S", new Vector3(0f, WallHeight * 0.5f, -RoomHalf - 0.5f),
                new Vector3(RoomHalf * 2f + 2f, WallHeight, 1f), stone);
            Box(parent, "Muro_E", new Vector3(RoomHalf + 0.5f, WallHeight * 0.5f, 0f),
                new Vector3(1f, WallHeight, RoomHalf * 2f + 2f), stone);
            Box(parent, "Muro_O", new Vector3(-RoomHalf - 0.5f, WallHeight * 0.5f, 0f),
                new Vector3(1f, WallHeight, RoomHalf * 2f + 2f), stone);
        }

        private static void BuildObstacles(Transform parent)
        {
            GameObject group = new GameObject("Obstaculos");
            group.transform.SetParent(parent);
            Color pillarCol = new Color(0.48f, 0.42f, 0.52f);
            Color woodCol = new Color(0.42f, 0.30f, 0.19f);

            Box(group.transform, "Columna_1", new Vector3(6f, 2.5f, 6f), new Vector3(1.3f, 5f, 1.3f), pillarCol);
            Box(group.transform, "Columna_2", new Vector3(-6f, 2.5f, 6f), new Vector3(1.3f, 5f, 1.3f), pillarCol);
            Box(group.transform, "Columna_3", new Vector3(6f, 2.5f, -6f), new Vector3(1.3f, 5f, 1.3f), pillarCol);
            Box(group.transform, "Columna_4", new Vector3(-6f, 2.5f, -6f), new Vector3(1.3f, 5f, 1.3f), pillarCol);

            Box(group.transform, "Viga", new Vector3(0f, 5.6f, -4.5f), new Vector3(9f, 0.5f, 0.8f), woodCol);

            BuildChandelier(group.transform, new Vector3(0f, WallHeight, 1.5f), 3.2f);
            BuildBrazier(group.transform, new Vector3(5.5f, 3.4f, 0f));
            BuildBrazier(group.transform, new Vector3(-5.5f, 3.4f, 0f));

            BuildRune(group.transform, new Vector3(3.5f, 4.5f, -2.5f));
            BuildRune(group.transform, new Vector3(-3.5f, 5.5f, 2.5f));
        }

        private static void BuildChandelier(Transform parent, Vector3 pivotPos, float length)
        {
            GameObject pivot = new GameObject("Candelabro_pivote");
            pivot.transform.SetParent(parent);
            pivot.transform.position = pivotPos;
            SwingingObstacle swing = pivot.AddComponent<SwingingObstacle>();
            swing.amplitude = 32f;
            swing.period = 3.6f;
            swing.axis = Vector3.forward;

            GameObject chain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chain.name = "Cadena";
            chain.transform.SetParent(pivot.transform);
            chain.transform.localPosition = new Vector3(0f, -length * 0.5f, 0f);
            chain.transform.localScale = new Vector3(0.08f, length * 0.5f, 0.08f);
            Object.DestroyImmediate(chain.GetComponent<Collider>());
            Paint(chain, new Color(0.35f, 0.33f, 0.28f), false);

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ring.name = "Aro";
            ring.transform.SetParent(pivot.transform);
            ring.transform.localPosition = new Vector3(0f, -length, 0f);
            ring.transform.localScale = new Vector3(1.8f, 0.5f, 1.8f);
            Paint(ring, new Color(0.60f, 0.52f, 0.34f), false);
        }

        private static void BuildBrazier(Transform parent, Vector3 pos)
        {
            GameObject go = new GameObject("Brasero");
            go.transform.SetParent(parent);
            go.transform.position = pos;

            GameObject bowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bowl.name = "Cuenco";
            bowl.transform.SetParent(go.transform);
            bowl.transform.localPosition = Vector3.zero;
            bowl.transform.localScale = new Vector3(0.9f, 0.25f, 0.9f);
            Paint(bowl, new Color(0.25f, 0.20f, 0.16f), false);

            GameObject fire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fire.name = "Fuego";
            fire.transform.SetParent(go.transform);
            fire.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            fire.transform.localScale = new Vector3(0.9f, 1.2f, 0.9f);
            Object.DestroyImmediate(fire.GetComponent<Collider>());
            Paint(fire, new Color(1f, 0.55f, 0.15f), true);

            SphereCollider trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.1f;
            trigger.center = new Vector3(0f, 0.5f, 0f);
            go.AddComponent<HazardZone>();

            GameObject lightGo = new GameObject("Luz");
            lightGo.transform.SetParent(go.transform);
            lightGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            Light l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.65f, 0.3f);
            l.range = 11f;
            l.intensity = 2.2f;
        }

        private static void BuildRune(Transform parent, Vector3 pos)
        {
            GameObject go = new GameObject("Runa");
            go.transform.SetParent(parent);
            go.transform.position = pos;

            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            disc.name = "Disco";
            disc.transform.SetParent(go.transform);
            disc.transform.localPosition = Vector3.zero;
            disc.transform.localScale = new Vector3(1.1f, 0.12f, 1.1f);
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            Paint(disc, new Color(0.45f, 0.85f, 1f), true);

            SphereCollider trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.75f;
            go.AddComponent<RuneBonus>();
        }

        // -------------------------------------------------------------- actores
        private static BalloonController BuildBalloon(Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Globo";
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(0f, 6.5f, 0f);
            go.transform.localScale = Vector3.one * (FBConfig.BalloonRadius * 2f);
            Paint(go, new Color(0.90f, 0.18f, 0.18f), false);

            go.AddComponent<Rigidbody>();
            return go.AddComponent<BalloonController>();
        }

        private static PlayerController BuildPlayer(Transform parent)
        {
            GameObject go = new GameObject("Jugador");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(0f, 0f, -3f);

            CharacterController cc = go.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 1f, 0f);
            cc.slopeLimit = 50f;
            cc.stepOffset = 0.35f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Cuerpo_placeholder";
            body.transform.SetParent(go.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            Paint(body, new Color(0.35f, 0.25f, 0.48f), false);

            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Frente";
            nose.transform.SetParent(go.transform);
            nose.transform.localPosition = new Vector3(0f, 1.75f, 0.32f);
            nose.transform.localScale = new Vector3(0.18f, 0.18f, 0.25f);
            Object.DestroyImmediate(nose.GetComponent<Collider>());
            Paint(nose, new Color(0.91f, 0.76f, 0.60f), false);

            PlayerController pc = go.AddComponent<PlayerController>();

            BodyPartHitbox[] boxes = new BodyPartHitbox[5];
            boxes[0] = Hitbox(go.transform, BodyPartId.Foot, new Vector3(0f, 0.45f, 0.80f), 0.60f);
            boxes[1] = Hitbox(go.transform, BodyPartId.Knee, new Vector3(0f, 0.95f, 0.62f), 0.50f);
            boxes[2] = Hitbox(go.transform, BodyPartId.Head, new Vector3(0f, 1.95f, 0.35f), 0.50f);
            boxes[3] = Hitbox(go.transform, BodyPartId.Chest, new Vector3(0f, 1.35f, 0.55f), 0.58f);
            boxes[4] = Hitbox(go.transform, BodyPartId.Hand, new Vector3(0f, 2.25f, 0.50f), 0.50f);
            pc.hitboxes = boxes;

            return pc;
        }

        private static BodyPartHitbox Hitbox(Transform parent, BodyPartId part, Vector3 localPos, float radius)
        {
            GameObject go = new GameObject("Golpeo_" + part);
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;
            SphereCollider col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = radius;
            BodyPartHitbox hb = go.AddComponent<BodyPartHitbox>();
            hb.part = part;
            return hb;
        }

        private static GameObject BuildDrunkTemplate(Transform parent)
        {
            GameObject go = new GameObject("BorrachoPlantilla");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(0f, 0f, 20f);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Cuerpo_placeholder";
            body.transform.SetParent(go.transform);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.95f, 0.9f, 0.95f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            Paint(body, new Color(0.30f, 0.42f, 0.36f), false);

            GameObject mug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mug.name = "Jarra";
            mug.transform.SetParent(go.transform);
            mug.transform.localPosition = new Vector3(0.55f, 1.15f, 0.2f);
            mug.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            Object.DestroyImmediate(mug.GetComponent<Collider>());
            Paint(mug, new Color(0.72f, 0.66f, 0.54f), false);

            CapsuleCollider col = go.AddComponent<CapsuleCollider>();
            col.height = 1.9f;
            col.radius = 0.55f;
            col.center = new Vector3(0f, 0.95f, 0f);

            go.AddComponent<DrunkNpc>();
            go.SetActive(false);
            return go;
        }

        private static Camera BuildCamera(Transform parent, Transform player, Transform balloon)
        {
            GameObject go = new GameObject("Camara");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(0f, 7.5f, -15.5f);
            go.tag = "MainCamera";

            Camera cam = go.AddComponent<Camera>();
            cam.fieldOfView = 55f;
            cam.nearClipPlane = 0.2f;
            cam.farClipPlane = 120f;
            cam.backgroundColor = new Color(0.05f, 0.04f, 0.07f);
            go.AddComponent<AudioListener>();

            RoomCamera rc = go.AddComponent<RoomCamera>();
            rc.player = player;
            rc.balloon = balloon;
            rc.roomCenter = Vector3.zero;
            return cam;
        }

        private static void BuildLights(Transform parent)
        {
            GameObject go = new GameObject("Luz_direccional");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(0f, 12f, -4f);
            go.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            Light l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(0.85f, 0.82f, 0.95f);
            l.intensity = 0.9f;
            l.shadows = LightShadows.Soft;
        }

        // ------------------------------------------------------------- utilidad
        private static GameObject Box(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = scale;
            Paint(go, color, false);
            return go;
        }

        private static Shader LitShader()
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Diffuse");
            return s;
        }

        private static void Paint(GameObject go, Color color, bool emissive)
        {
            Renderer r = go.GetComponent<Renderer>();
            if (r == null) return;

            Material mat = new Material(LitShader());
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * 2.2f);
            }
            r.sharedMaterial = mat;
        }
    }
}
