using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuenaAMueble.EditorTools
{
    /// <summary>
    /// Crea la escena del laboratorio la primera vez que se abre el proyecto y la deja abierta,
    /// para que solo haya que darle a Play. Menú: Suena a Mueble.
    /// También sirve desde la terminal:
    ///   Unity -batchmode -quit -projectPath RUTA -executeMethod SuenaAMueble.EditorTools.SuenaAMuebleSetup.Build
    /// </summary>
    [InitializeOnLoad]
    public static class SuenaAMuebleSetup
    {
        public const string ScenePath = "Assets/SuenaAMueble/Escenas/Laboratorio.unity";

        static SuenaAMuebleSetup()
        {
            EditorApplication.delayCall += AutoSetup;
        }

        private static void AutoSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(ScenePath))
            {
                CreateScene();
                Debug.Log("Suena a Mueble: laboratorio creado. Dale a Play.");
                return;
            }
            Scene active = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(active.path) && !active.isDirty)
            {
                EditorSceneManager.OpenScene(ScenePath);
            }
        }

        [MenuItem("Suena a Mueble/Abrir laboratorio", false, 0)]
        public static void OpenLab()
        {
            if (!File.Exists(ScenePath)) CreateScene();
            else if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Suena a Mueble/Rehacer escena del laboratorio", false, 1)]
        public static void CreateScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("SuenaAMueble").AddComponent<LabBootstrap>();

            string dir = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        /// <summary>Compila un ejecutable para la plataforma activa en la carpeta Builds/.</summary>
        public static void Build()
        {
            if (!File.Exists(ScenePath)) CreateScene();
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            string ext = target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64 ? ".exe"
                       : target == BuildTarget.StandaloneOSX ? ".app" : "";
            BuildPlayerOptions opts = new BuildPlayerOptions();
            opts.scenes = new[] { ScenePath };
            opts.target = target;
            opts.locationPathName = "Builds/SuenaAMueble" + ext;
            BuildPipeline.BuildPlayer(opts);
        }
    }
}
