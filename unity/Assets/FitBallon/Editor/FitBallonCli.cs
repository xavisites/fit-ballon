using System.IO;
using UnityEditor;
using UnityEngine;

namespace FitBallon.EditorTools
{
    /// <summary>
    /// Puntos de entrada para usar Unity desde la terminal, sin abrir el editor:
    ///
    ///   Unity -batchmode -quit -projectPath RUTA \
    ///         -executeMethod FitBallon.EditorTools.FitBallonCli.Scene -logFile -
    ///
    ///   Unity -batchmode -quit -projectPath RUTA \
    ///         -executeMethod FitBallon.EditorTools.FitBallonCli.Build -logFile -
    /// </summary>
    public static class FitBallonCli
    {
        /// <summary>Monta y guarda la escena de la sala.</summary>
        public static void Scene()
        {
            FitBallonSceneBuilder.CreateSceneHeadless();
        }

        /// <summary>Compila un ejecutable para la plataforma activa del proyecto.</summary>
        public static void Build()
        {
            if (!File.Exists(FitBallonSceneBuilder.ScenePath))
            {
                FitBallonSceneBuilder.CreateSceneHeadless();
            }

            string dir = "Builds";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildPlayerOptions opts = new BuildPlayerOptions();
            opts.scenes = new string[] { FitBallonSceneBuilder.ScenePath };
            opts.target = target;
            opts.locationPathName = dir + "/FitBallon" + ExtensionFor(target);
            opts.options = BuildOptions.None;

            BuildPipeline.BuildPlayer(opts);
            Debug.Log("Fit Ballon: compilado en " + opts.locationPathName);
        }

        private static string ExtensionFor(BuildTarget target)
        {
            if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {
                return ".exe";
            }
            if (target == BuildTarget.StandaloneOSX) return ".app";
            return "";   // WebGL y Linux generan carpeta
        }
    }
}
