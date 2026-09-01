using System.Collections;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Catalog;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Navigation
{
    public sealed class LevelSequenceController : MonoBehaviour, ILevelSequence
    {
        public const float SlideDurationSeconds = 1f;
        private AppServices services;
        private GameCatalog catalog;
        private bool transitioning;
        public bool IsTransitioning => transitioning;
        public void Configure(AppServices appServices, GameCatalog gameCatalog) { services = appServices; catalog = gameCatalog; }
        public void Advance(string nextGameId) { if (!transitioning) StartCoroutine(SlideTo(nextGameId)); }
        private IEnumerator SlideTo(string nextGameId)
        {
            GameDefinition next = catalog.FindGameById(nextGameId);
            if (next == null) yield break; transitioning = true;
            Scene outgoing = SceneManager.GetActiveScene(); AsyncOperation load = SceneManager.LoadSceneAsync(next.SceneName, LoadSceneMode.Additive); yield return load;
            Scene incoming = SceneManager.GetSceneByName(next.SceneName); SceneManager.SetActiveScene(incoming);
            Canvas.ForceUpdateCanvases();
            ILevelTransitionParticipant outgoingParticipant = FindTransitionParticipant(outgoing);
            ILevelTransitionParticipant incomingParticipant = FindTransitionParticipant(incoming);
            RectTransform outgoingRoot = outgoingParticipant?.TransitionRoot;
            RectTransform incomingRoot = incomingParticipant?.TransitionRoot;
            float width = outgoingRoot != null && outgoingRoot.rect.width > 0f ? outgoingRoot.rect.width : Screen.width;
            if (incomingRoot != null) incomingRoot.anchoredPosition = LevelSlideMotion.IncomingPosition(width, 0f);
            float elapsed = 0f; while (elapsed < SlideDurationSeconds) { elapsed += Time.unscaledDeltaTime; float t = Mathf.SmoothStep(0f, 1f, elapsed / SlideDurationSeconds); if (outgoingRoot != null) outgoingRoot.anchoredPosition = LevelSlideMotion.OutgoingPosition(width, t); if (incomingRoot != null) incomingRoot.anchoredPosition = LevelSlideMotion.IncomingPosition(width, t); yield return null; }
            if (outgoingRoot != null) outgoingRoot.anchoredPosition = LevelSlideMotion.OutgoingPosition(width, 1f); if (incomingRoot != null) incomingRoot.anchoredPosition = LevelSlideMotion.IncomingPosition(width, 1f);
            incomingParticipant?.CompleteTransitionHandoff();
            yield return SceneManager.UnloadSceneAsync(outgoing);
            transitioning = false;
        }
        private static ILevelTransitionParticipant FindTransitionParticipant(Scene scene) { foreach (GameObject root in scene.GetRootGameObjects()) foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true)) if (behaviour is ILevelTransitionParticipant participant) return participant; return null; }
    }
}
