#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Lbs.MiniGames.Games.ShapeAnalogy;
using Lbs.MiniGames.Shared.Results;

namespace Lbs.MiniGames.Games.ShapeAnalogy.Editor
{
    public static class ShapeAnalogyVisualCapture
    {
        private const string Folder = "C:/Users/legio/AppData/Local/Temp/shape-analogy-visuals";
        public static void Run()
        {
            Directory.CreateDirectory(Folder);
            Scene scene = EditorSceneManager.OpenScene("Assets/App/Games/ShapeAnalogy/ShapeAnalogy.unity", OpenSceneMode.Single);
            var game = Object.FindFirstObjectByType<ShapeAnalogyGame>();
            if (!game) { Debug.LogError("ShapeAnalogy capture: production game missing"); EditorApplication.Exit(1); return; }
            var so = new SerializedObject(game);
            string[] names = { "starEmpty", "starFull", "heartEmpty", "heartFull", "missing", "finalStar", "hongNeutral", "hong1", "hong2", "hong3", "celebration4Star", "celebration5Star", "circleConfetti", "rectangularConfetti", "serpentina" };
            string[] paths = { "Star_UnFilled.png", "Star_FullFilled.png", "Heart_UnFilled.png", "Heart_FullFilled.png", "Missingitem.png", "FinalStar.png", "Hong_Neutral.png", "Hong_Listening1.png", "Hong_Listening2.png", "Hong_Listening3.png", "Celebration/4Star.png", "Celebration/5star.png", "Celebration/CircleConfetti.png", "Celebration/RectangularConfetti.png", "Celebration/Serpentina.png" };
            for (int i = 0; i < names.Length; i++) so.FindProperty(names[i]).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/" + paths[i]);
            so.ApplyModifiedPropertiesWithoutUndo();
            game.CaptureInitial();
            var canvas = Object.FindFirstObjectByType<Canvas>(); var canvasRect = canvas.GetComponent<RectTransform>(); canvas.renderMode=RenderMode.WorldSpace; canvas.worldCamera=null; canvasRect.sizeDelta=new Vector2(1920,1080); canvasRect.pivot=new Vector2(.5f,.5f); canvasRect.anchoredPosition=Vector2.zero; canvasRect.localPosition=Vector3.zero; canvasRect.localScale=Vector3.one;
            var cameraObject = new GameObject("ShapeAnalogyCaptureCamera"); var camera = cameraObject.AddComponent<Camera>(); camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=Color.black; camera.cullingMask=1 << canvas.gameObject.layer; camera.orthographic=true; camera.orthographicSize=540; camera.aspect=16f/9f; camera.nearClipPlane=.01f; camera.farClipPlane=100f; camera.transform.position=new Vector3(0,0,-10); camera.transform.rotation=Quaternion.identity;
            var rt= new RenderTexture(1920,1080,24); camera.targetTexture=rt; var image=new Texture2D(1920,1080,TextureFormat.RGBA32,false);
            string[] outputNames = { "initial.png", "drag-over.png", "success.png", "final.png" };
            for (int i = 0; i < outputNames.Length; i++) { if (i == 1) game.CaptureDragOver(); else if (i == 2) { game.CaptureSuccess(); SimulateCelebration(canvas, .7f); } else if (i == 3) { game.CaptureFinal(); SimulateCelebration(canvas, .9f); } Canvas.ForceUpdateCanvases(); LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRect); RenderTexture.active=rt; GL.Clear(true,true,Color.black); camera.Render(); GL.Flush(); RenderTexture.active=rt; image.ReadPixels(new Rect(0,0,1920,1080),0,0); image.Apply(); File.WriteAllBytes(Path.Combine(Folder,outputNames[i]),image.EncodeToPNG()); }
            Object.DestroyImmediate(image); Object.DestroyImmediate(rt); Object.DestroyImmediate(cameraObject); EditorSceneManager.CloseScene(scene,true); Debug.Log("SHAPE_ANALOGY_VISUAL_CAPTURE_SUMMARY files=4 dimensions=1920x1080"); EditorApplication.Exit(0);
        }
        private static void SimulateCelebration(Canvas canvas, float time) { foreach (ParticleSystem particles in canvas.GetComponentsInChildren<ParticleSystem>()) particles.Simulate(time, true, true, true); foreach (FinalCelebrationUIParticleRenderer bridge in canvas.GetComponentsInChildren<FinalCelebrationUIParticleRenderer>()) bridge.Refresh(); }
    }
}
#endif
