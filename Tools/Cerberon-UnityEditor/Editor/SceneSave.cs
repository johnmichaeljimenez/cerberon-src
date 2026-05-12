using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using CerberonEditor.Main;
using UnityEngine;

namespace CerberonEditor.Editor
{
    [InitializeOnLoad]
    public class SceneSave
    {
        static SceneSave()
        {
            EditorSceneManager.sceneSaving += OnSceneSaving;
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                var exp = root.GetComponent<CerberonExporter>();
                if (exp != null)
                {
                    exp.Export();
                }
            }
        }
    }
}