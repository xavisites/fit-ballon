using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace FitBallon.EditorTools
{
    /// <summary>
    /// Monta solo el Animator del personaje: busca sus animaciones, crea un
    /// controlador con mezcla quieto → andar → correr y lo engancha.
    ///
    /// Uso: selecciona el personaje (en la escena o el .fbx en Project) y
    /// Fit Ballon ▸ Preparar animaciones del personaje.
    /// </summary>
    public static class FitBallonAnimatorSetup
    {
        private const string FolderPath = "Assets/FitBallon/Animation";
        private const string ControllerPath = FolderPath + "/Personaje.controller";

        private static readonly string[] IdleWords = { "idle", "reposo", "quieto", "parado", "espera" };
        private static readonly string[] WalkWords = { "walk", "andar", "caminar", "camina", "anda" };
        private static readonly string[] RunWords = { "run", "sprint", "correr", "corre", "carrera" };

        [MenuItem("Fit Ballon/Preparar animaciones del personaje", false, 20)]
        public static void Setup()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Tell("Selecciona primero el personaje: o el objeto en la jerarquía, " +
                     "o su archivo .fbx en la ventana Project.");
                return;
            }

            List<AnimationClip> clips = CollectClips(selected);
            if (clips.Count == 0)
            {
                Tell("No encuentro animaciones en '" + selected.name + "'.\n\n" +
                     "Comprueba que el .fbx trae las animaciones dentro (pestaña Animation del " +
                     "modelo) o descarga un pack de animaciones a la carpeta Assets.");
                return;
            }

            AnimationClip idle = Match(clips, IdleWords);
            AnimationClip walk = Match(clips, WalkWords);
            AnimationClip run = Match(clips, RunWords);

            if (idle == null) idle = clips[0];
            if (walk == null)
            {
                Tell("He encontrado " + clips.Count + " animaciones pero ninguna de andar.\n\n" +
                     "Renombra la de andar para que lleve 'walk' o 'andar' en el nombre, " +
                     "o descarga una (Mixamo o el pack gratuito KayKit Character Animations).");
                return;
            }

            // Sin animación de correr, se usa la de andar acelerada: no es lo ideal,
            // pero se nota el cambio y se puede sustituir luego sin tocar nada más.
            bool fakeRun = run == null;
            if (fakeRun) run = walk;

            AnimatorController controller = BuildController(idle, walk, run, fakeRun);
            Attach(selected, controller);

            string msg = "Animator listo.\n\n" +
                         "Quieto: " + idle.name + "\n" +
                         "Andar: " + walk.name + "\n" +
                         "Correr: " + (fakeRun ? walk.name + "  (acelerada x1,6)" : run.name);
            Debug.Log("Fit Ballon: " + msg.Replace("\n", " "));
            Tell(msg + (fakeRun
                ? "\n\nNo tenías animación de correr, así que he usado la de andar más rápida."
                : ""));
        }

        // ------------------------------------------------------------- clips
        private static List<AnimationClip> CollectClips(GameObject go)
        {
            List<AnimationClip> result = new List<AnimationClip>();

            string path = AssetDatabase.GetAssetPath(go);
            if (string.IsNullOrEmpty(path))
            {
                Object source = PrefabUtility.GetCorrespondingObjectFromSource(go);
                if (source != null) path = AssetDatabase.GetAssetPath(source);
            }

            if (!string.IsNullOrEmpty(path))
            {
                Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                for (int i = 0; i < subAssets.Length; i++)
                {
                    AnimationClip clip = subAssets[i] as AnimationClip;
                    if (clip != null && !clip.name.StartsWith("__preview__")) result.Add(clip);
                }
            }

            // Si el modelo no trae animaciones dentro, se buscan por todo el proyecto
            // (es lo típico al bajar clips sueltos de Mixamo).
            if (result.Count == 0)
            {
                string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new string[] { "Assets" });
                for (int i = 0; i < guids.Length; i++)
                {
                    string clipPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(clipPath);
                    for (int k = 0; k < subAssets.Length; k++)
                    {
                        AnimationClip clip = subAssets[k] as AnimationClip;
                        if (clip != null && !clip.name.StartsWith("__preview__")) result.Add(clip);
                    }
                    AnimationClip direct = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    if (direct != null && !result.Contains(direct)) result.Add(direct);
                }
            }
            return result;
        }

        private static AnimationClip Match(List<AnimationClip> clips, string[] words)
        {
            for (int i = 0; i < clips.Count; i++)
            {
                string n = clips[i].name.ToLower();
                for (int k = 0; k < words.Length; k++)
                {
                    if (n.Contains(words[k])) return clips[i];
                }
            }
            return null;
        }

        // -------------------------------------------------------- controlador
        private static AnimatorController BuildController(AnimationClip idle, AnimationClip walk,
                                                          AnimationClip run, bool fakeRun)
        {
            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
            AssetDatabase.Refresh();

            AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (existing != null) AssetDatabase.DeleteAsset(ControllerPath);

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Kick", AnimatorControllerParameterType.Trigger);

            BlendTree tree;
            AnimatorState state = controller.CreateBlendTreeInController("Locomocion", out tree, 0);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.useAutomaticThresholds = false;
            tree.AddChild(idle, 0f);
            tree.AddChild(walk, 0.5f);
            tree.AddChild(run, 1f);

            if (fakeRun)
            {
                ChildMotion[] children = tree.children;
                children[2].timeScale = 1.6f;
                tree.children = children;
            }

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            machine.defaultState = state;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return controller;
        }

        private static void Attach(GameObject selected, AnimatorController controller)
        {
            // Si lo seleccionado es el .fbx del Project, se busca su copia en la escena.
            GameObject target = selected;
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(selected)))
            {
                PlayerController inScene = Object.FindObjectOfType<PlayerController>();
                if (inScene != null)
                {
                    Animator anim = inScene.GetComponentInChildren<Animator>();
                    if (anim != null) target = anim.gameObject;
                    else target = inScene.gameObject;
                }
            }

            Animator animator = target.GetComponentInChildren<Animator>();
            if (animator == null) animator = target.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            PlayerController player = target.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                PlayerAnimator bridge = player.GetComponent<PlayerAnimator>();
                if (bridge == null) bridge = player.gameObject.AddComponent<PlayerAnimator>();
                bridge.player = player;
                bridge.animator = animator;
                EditorUtility.SetDirty(player.gameObject);
            }
            EditorUtility.SetDirty(target);
        }

        private static void Tell(string message)
        {
            EditorUtility.DisplayDialog("Fit Ballon · animaciones", message, "Vale");
        }
    }
}
