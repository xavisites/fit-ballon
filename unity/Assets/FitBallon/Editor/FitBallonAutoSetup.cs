using System.IO;
using UnityEditor;
using UnityEngine;

namespace FitBallon.EditorTools
{
    /// <summary>
    /// Monta la sala sola la primera vez que Unity compila el proyecto,
    /// para que no haya que tocar ni el menú: copias la carpeta y le das a Play.
    /// Si la escena ya existe no hace nada.
    /// </summary>
    [InitializeOnLoad]
    public static class FitBallonAutoSetup
    {
        static FitBallonAutoSetup()
        {
            EditorApplication.delayCall += TrySetup;
        }

        private static void TrySetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (File.Exists(FitBallonSceneBuilder.ScenePath)) return;

            FitBallonSceneBuilder.CreateSceneHeadless();
            Debug.Log("Fit Ballon: sala montada automáticamente. Dale a Play y elige tu estado de embriaguez.");
        }
    }
}
