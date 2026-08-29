#if UNITY_EDITOR
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.ShapeAnalogy.Editor
{
    public static class ShapeAnalogyRuntimeAudioCheck
    {
        private static float deadline;
        private static AudioSource source;
        private static Button hong;
        private static bool replayRequested;

        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/App/Games/ShapeAnalogy/ShapeAnalogy.unity", OpenSceneMode.Single);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            ShapeAnalogyGame game = Object.FindFirstObjectByType<ShapeAnalogyGame>();
            if (!game) { Finish(false, "game missing"); return; }
            GameSession session = new();
            game.Configure(new AppServices(session, new GameLauncher(session, new NoOpSceneLoader(), "Lobby")));
            source = game.GetComponent<AudioSource>();
            hong = GameObject.Find("Hong")?.GetComponent<Button>();
            if (!source || !hong || !Object.FindFirstObjectByType<AudioListener>() || !source.clip || source.clip.name != "Instruction" || source.mute || source.volume != 1f)
            {
                Finish(false, "required runtime audio wiring missing");
                return;
            }

            deadline = Time.realtimeSinceStartup + 5f;
            EditorApplication.update += Verify;
        }

        private static void Verify()
        {
            if (!source || Time.realtimeSinceStartup > deadline) { Finish(false, replayRequested ? "Hong replay timeout" : "startup playback timeout"); return; }
            if (!source.isPlaying) return;
            if (!replayRequested)
            {
                hong.onClick.Invoke();
                if (source.isPlaying) { Finish(false, "Hong did not stop playback"); return; }
                hong.onClick.Invoke();
                replayRequested = true;
                deadline = Time.realtimeSinceStartup + 5f;
                return;
            }

            Finish(true, "clip=Instruction startup=playing replay=playing listener=present");
        }

        private static void Finish(bool passed, string detail)
        {
            EditorApplication.update -= Verify;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Debug.Log($"SHAPE_ANALOGY_RUNTIME_AUDIO_SUMMARY passed={passed} {detail}");
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(passed ? 0 : 1);
        }

        private sealed class NoOpSceneLoader : ISceneLoader
        {
            public void Load(string sceneName) { }
        }
    }
}
#endif
