using SuenaAMueble.Audio;
using UnityEngine;

namespace SuenaAMueble
{
    /// <summary>
    /// Monta el laboratorio entero al darle a Play: cámara, luz, salón, objeto, micro e interfaz.
    /// La escena solo necesita un GameObject con este componente.
    /// </summary>
    public class LabBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            cam.transform.position = new Vector3(-0.9f, 1.55f, -3.4f);
            cam.transform.LookAt(new Vector3(-0.9f, 0.75f, 0f));
            cam.fieldOfView = 50f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.16f, 0.13f, 0.12f);

            if (FindObjectOfType<Light>() == null)
            {
                GameObject lightGo = new GameObject("Luz");
                Light l = lightGo.AddComponent<Light>();
                l.type = LightType.Directional;
                l.intensity = 1.1f;
                l.shadows = LightShadows.Soft;
                lightGo.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            }
            RenderSettings.ambientLight = new Color(0.45f, 0.42f, 0.4f);

            BuildRoom();

            GameObject propGo = new GameObject("ObjetoEscondido");
            propGo.transform.position = new Vector3(0.6f, 0f, 0f);
            propGo.transform.rotation = Quaternion.Euler(0f, -12f, 0f);
            PropDisplay prop = propGo.AddComponent<PropDisplay>();

            GameObject labGo = new GameObject("Laboratorio");
            MicCapture mic = labGo.AddComponent<MicCapture>();
            ImitationLab lab = labGo.AddComponent<ImitationLab>();
            lab.prop = prop;
            lab.mic = mic;
            LabHud hud = labGo.AddComponent<LabHud>();
            hud.lab = lab;
            hud.mic = mic;

            prop.Show(PropCatalog.Get("nevera"));
        }

        /// <summary>Un rincón de salón con muebles de relleno, para que se vea dónde te escondes.</summary>
        private static void BuildRoom()
        {
            Transform root = new GameObject("Salon").transform;
            Block(root, "Suelo", new Vector3(0, -0.05f, 1f), new Vector3(9f, 0.1f, 7f), "#8b6a4f");
            Block(root, "ParedFondo", new Vector3(0, 1.6f, 2.6f), new Vector3(9f, 3.2f, 0.1f), "#e6d5b8");
            Block(root, "ParedLateral", new Vector3(-3.4f, 1.6f, 1f), new Vector3(0.1f, 3.2f, 7f), "#d9c4a3");
            Block(root, "Zocalo", new Vector3(0, 0.08f, 2.53f), new Vector3(9f, 0.16f, 0.04f), "#6d4c35");

            GameObject rug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(rug.GetComponent<Collider>());
            rug.name = "Alfombra";
            rug.transform.SetParent(root, false);
            rug.transform.localPosition = new Vector3(0.2f, 0.005f, 0.3f);
            rug.transform.localScale = new Vector3(2.6f, 0.005f, 1.8f);
            PropDisplay.Paint(rug, Hex("#a63d40"));

            // Sofá y mueble del fondo: decorado, no hacen nada.
            Block(root, "Sofa_Base", new Vector3(-1.9f, 0.25f, 1.9f), new Vector3(1.9f, 0.5f, 0.8f), "#4f6d7a");
            Block(root, "Sofa_Respaldo", new Vector3(-1.9f, 0.7f, 2.25f), new Vector3(1.9f, 0.6f, 0.2f), "#4f6d7a");
            Block(root, "Sofa_BrazoI", new Vector3(-2.8f, 0.45f, 1.9f), new Vector3(0.15f, 0.4f, 0.8f), "#3f5864");
            Block(root, "Sofa_BrazoD", new Vector3(-1.0f, 0.45f, 1.9f), new Vector3(0.15f, 0.4f, 0.8f), "#3f5864");
            Block(root, "Estanteria", new Vector3(2.4f, 0.9f, 2.3f), new Vector3(1.2f, 1.8f, 0.4f), "#7a5236");
            Block(root, "Cuadro", new Vector3(-1.9f, 1.9f, 2.54f), new Vector3(0.9f, 0.6f, 0.02f), "#3d5a40");
            Block(root, "Lampara_Pie", new Vector3(-3.0f, 0.75f, 2.2f), new Vector3(0.05f, 1.5f, 0.05f), "#333333");
            Block(root, "Lampara_Pantalla", new Vector3(-3.0f, 1.55f, 2.2f), new Vector3(0.4f, 0.3f, 0.4f), "#f3e3b5");
        }

        private static void Block(Transform parent, string name, Vector3 pos, Vector3 size, string hex)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            PropDisplay.Paint(go, Hex(hex));
        }

        private static Color Hex(string hex)
        {
            Color c;
            ColorUtility.TryParseHtmlString(hex, out c);
            return c;
        }
    }
}
