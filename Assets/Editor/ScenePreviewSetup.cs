using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExamenNavesU1
{
    public static class ScenePreviewSetup
    {
        [MenuItem("ExamenNavesU1/Preparar previa en escena")]
        public static void Create()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/ExamenNavesU1.unity");
            var game = UnityEngine.Object.FindFirstObjectByType<Game>();
            var camera = Camera.main;
            camera.orthographic = true; camera.orthographicSize = 5;
            camera.backgroundColor = new Color(.035f, .045f, .09f);
            if (GameObject.Find("Fondo") == null)
                Game.CreateSprite("Fondo", "background", Vector2.zero, new Vector2(18, 10), -10);
            if (game.player == null)
            {
                game.player = Game.CreateSprite("Nave", "player", Vector2.zero, new Vector2(.7f, .9f)).AddComponent<Player>();
                Game.Body(game.player.gameObject, new Vector2(.6f, .7f));
            }
            var hud = game.GetComponent<Hud>();
            if (hud == null) hud = game.gameObject.AddComponent<Hud>();
            if (hud.pointsText == null) CreateHud(hud, game, camera);
            var savedCanvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            savedCanvas.GetComponent<CanvasScaler>().matchWidthOrHeight = 0;
            var controlsRect = savedCanvas.transform.Find("Controles").GetComponent<RectTransform>();
            controlsRect.anchorMin = controlsRect.anchorMax = Vector2.zero;
            controlsRect.pivot = Vector2.zero;
            controlsRect.anchoredPosition = new Vector2(18, 6);
            var resultRect = hud.resultPanel.GetComponent<RectTransform>();
            resultRect.anchorMin = resultRect.anchorMax = new Vector2(.5f, .5f);
            resultRect.pivot = new Vector2(.5f, .5f);
            resultRect.anchoredPosition = new Vector2(0, -10);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Validate();
            Debug.Log("EXAMEN_PREVIEW_OK");
        }

        static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
        static Text Label(string name, Transform parent, string value, float x, float y, float width, float height, int size = 19)
        {
            var text = Rect(name, parent, x, y, width, height).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size; text.text = value; text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false;
            return text;
        }
        static Image Panel(string name, Transform parent, float x, float y, float width, float height)
        {
            var image = Rect(name, parent, x, y, width, height).gameObject.AddComponent<Image>();
            image.color = new Color(.035f, .045f, .07f, .92f);
            return image;
        }
        static void CreateHud(Hud hud, Game game, Camera camera)
        {
            var obj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = obj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.sortingOrder = 10;
            var scaler = obj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 540); scaler.matchWidthOrHeight = 0;
            var top = Panel("Barra de indicadores", obj.transform, 8, 8, 944, 48);
            hud.pointsText = Label("Puntaje", top.transform, "PUNTOS: 0 / 200", 12, 9, 190, 30);
            hud.recordText = Label("Record", top.transform, "RECORD: 0", 212, 9, 170, 30);
            hud.weaponText = Label("Arma", top.transform, "ARMA: NORMAL", 402, 9, 270, 30);
            hud.hearts = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                var heart = Rect("Vida " + (i + 1), top.transform, 737 + i * 37, 10, 28, 28).gameObject.AddComponent<Image>();
                heart.sprite = Resources.Load<Sprite>("Art/life"); heart.preserveAspect = true; heart.raycastTarget = false;
                hud.hearts[i] = heart;
            }
            var controls = Label("Controles", obj.transform, "Flechas / WASD: mover   |   Mouse: apuntar   |   Clic / Espacio: disparar", 18, 506, 924, 28);
            controls.rectTransform.anchorMin = controls.rectTransform.anchorMax = Vector2.zero;
            controls.rectTransform.pivot = Vector2.zero;
            controls.rectTransform.anchoredPosition = new Vector2(18, 6);
            var result = Panel("Fin de partida", obj.transform, 265, 170, 430, 220);
            result.rectTransform.anchorMin = result.rectTransform.anchorMax = new Vector2(.5f, .5f);
            result.rectTransform.pivot = new Vector2(.5f, .5f);
            result.rectTransform.anchoredPosition = new Vector2(0, -10);
            hud.resultPanel = result.gameObject;
            hud.resultTitle = Label("Titulo", result.transform, "FIN DE PARTIDA", 10, 15, 410, 50, 30);
            hud.resultTitle.alignment = TextAnchor.MiddleCenter;
            hud.resultText = Label("Resultado", result.transform, "Puntos obtenidos: 0", 30, 75, 390, 45);
            var buttonImage = Panel("Volver a jugar", result.transform, 85, 145, 260, 45);
            buttonImage.color = new Color(.23f, .29f, .39f);
            var button = buttonImage.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            var buttonLabel = Label("Texto", button.transform, "VOLVER A JUGAR", 0, 0, 260, 45);
            buttonLabel.alignment = TextAnchor.MiddleCenter;
            UnityEventTools.AddPersistentListener(button.onClick, game.Restart);
            result.gameObject.SetActive(false);
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public static void Validate()
        {
            var game = UnityEngine.Object.FindFirstObjectByType<Game>();
            var hud = game.GetComponent<Hud>();
            if (GameObject.Find("Fondo") == null || game.player == null || hud == null || hud.pointsText == null || hud.hearts.Length != 5)
                throw new Exception("Faltan objetos guardados para la previa.");
            Canvas.ForceUpdateCanvases();
            var camera = Camera.main;
            var target = new RenderTexture(1280, 720, 24);
            camera.targetTexture = target; camera.Render();
            var previous = RenderTexture.active; RenderTexture.active = target;
            var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            image.ReadPixels(new UnityEngine.Rect(0, 0, 1280, 720), 0, 0); image.Apply();
            Directory.CreateDirectory("Validation");
            File.WriteAllBytes("Validation/scene-preview.png", image.EncodeToPNG());
            RenderTexture.active = previous; camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
