using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ExamenNavesU1
{
    public class ProjectSetup
    {
        [MenuItem("ExamenNavesU1/Crear escena principal")]
        public static void Create()
        {
            PlayerSettings.productName = "ExamenNavesU1";
            PlayerSettings.companyName = "ProyectoU1";
            foreach (string path in Directory.GetFiles("Assets/Resources/Art", "*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 0, -10);
            camera.AddComponent<Camera>().orthographic = true;
            camera.AddComponent<AudioListener>();
            new GameObject("GameManager").AddComponent<Game>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/ExamenNavesU1.unity");
            ScenePreviewSetup.Create();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/ExamenNavesU1.unity", true) };
            AssetDatabase.SaveAssets();
            Debug.Log("EXAMEN_SETUP_OK");
        }

        public static void Validate()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/ExamenNavesU1.unity");
            var game = UnityEngine.Object.FindFirstObjectByType<Game>();
            if (game == null || Camera.main == null) throw new Exception("Falta la escena principal.");
            foreach (string name in new[] { "player", "enemy0", "enemy1", "rock0", "rock1", "rock2", "bullet0", "bullet1", "enemybullet0", "enemybullet1", "power0", "power1", "power2", "life", "background" })
                if (Resources.Load<Sprite>("Art/" + name) == null) throw new Exception("Falta sprite: " + name);
            foreach (string name in new[] { "shot", "special", "hit", "pickup", "rock", "metal", "cancel", "explosion", "win", "lose", "music" })
                if (Resources.Load<AudioClip>("Audio/" + name) == null) throw new Exception("Falta audio: " + name);
            Debug.Log("EXAMEN_ASSETS_OK: 15 sprites, 11 audios, escena y camara.");
        }

        public static void Build()
        {
            Validate();
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/ExamenNavesU1.unity" },
                locationPathName = "Build/ExamenNavesU1.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("Fallo al crear el ejecutable: " + result.summary.result);
            Debug.Log("EXAMEN_BUILD_OK");
        }
    }
}
