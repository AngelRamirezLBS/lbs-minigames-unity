using System;
using System.Collections.Generic;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Games.Common;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Games.WildWhiz
{
    public sealed class WildWhizGame : MonoBehaviour, IMiniGame, IAppScene
    {
        private const string Id = "wild-whiz.logic";

        [SerializeField]
        private WildWhizLevelSet levelSet;

        [SerializeField]
        private Font interfaceFont;

        [SerializeField]
        private AudioClip instructionClip;

        [SerializeField]
        private Sprite speakerSprite;

        private AppServices services;
        private WildWhizCoordinator coordinator;
        private WildWhizScreen screen;
        private WildWhizAudioPresenter audioPresenter;
        private Canvas canvas;
        private bool resultReported;
        private Coroutine celebrationCoroutine;
        private bool finalCompletionActive;
        private bool celebrationActive;
        private readonly List<DragDropToken> boundTokens = new();

        public string GameId => Id;

        public bool IsCompleted => coordinator != null && coordinator.IsAllCompleted;

        public WildWhizCoordinator Coordinator => coordinator;

        public WildWhizScreen Screen => screen;

        public WildWhizAudioPresenter AudioPresenter => audioPresenter;

        public event Action<MiniGameResult> Completed;

        public void Configure(AppServices appServices)
        {
            StopCelebrationCoroutine();
            TearDownGeneratedInterface();
            services = appServices ?? throw new ArgumentNullException(nameof(appServices));

            WildWhizLevelSet set = levelSet != null ? levelSet : WildWhizLevelSet.CreateDefault();
            coordinator = new WildWhizCoordinator(set);
            resultReported = false;
            finalCompletionActive = false;

            BuildInterface();
        }

        public void SetLevelSet(WildWhizLevelSet set)
        {
            levelSet = set;
        }

        public void SetInterfaceFont(Font font)
        {
            interfaceFont = font;
        }

        public void SetInstructionClip(AudioClip clip)
        {
            instructionClip = clip;
            if (audioPresenter != null)
            {
                audioPresenter.SetInstructionClip(clip);
            }
        }

        private void BuildInterface()
        {
            canvas = ResolveSceneRootCanvas();
            if (canvas == null)
            {
                throw new InvalidOperationException("WildWhiz requires a root Canvas in the WildWhiz scene; no foreign or persistent Canvas is allowed.");
            }

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.localScale = Vector3.one;
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            // Ensure StandaloneInputModule for drag
            if (FindObjectOfType<StandaloneInputModule>() == null)
            {
                GameObject esGo = new("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                esGo.transform.SetParent(canvas.transform, false);
            }

            EnsureAudioPresenter();

            Font font = interfaceFont != null ? interfaceFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            TearDownGeneratedInterface();

            screen = new WildWhizScreen();
            screen.Build(canvas, font, coordinator.CurrentLevel, HandleSpeak, HandleClose, speakerSprite);

            WireCurrentLevel();

            RefreshProgress();
            // Ensure instruction text remains English even after audio fallback
            screen.SetInstruction(coordinator.CurrentLevel.Instruction);
        }

        private Canvas ResolveSceneRootCanvas()
        {
            Scene scene = gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded) return null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Canvas candidate = root.GetComponent<Canvas>();
                if (candidate != null && candidate.isRootCanvas) return candidate;
            }
            return null;
        }

        private void EnsureAudioPresenter()
        {
            audioPresenter = GetComponent<WildWhizAudioPresenter>();
            if (audioPresenter == null)
            {
                audioPresenter = gameObject.AddComponent<WildWhizAudioPresenter>();
            }

            audioPresenter.SetInstructionClip(coordinator.CurrentLevel.InstructionClip != null ? coordinator.CurrentLevel.InstructionClip : instructionClip);
            audioPresenter.EnsureAudio();
        }

        private void WireCurrentLevel()
        {
            boundTokens.Clear();
            foreach (KeyValuePair<DropTarget, WildWhizScreen.TargetArea> kv in screen.TargetAreas)
            {
                kv.Key.TokenDropped -= HandleDrop;
                kv.Key.TokenDropped += HandleDrop;
            }

            // Wire pickup sound via DragDropToken.DragStarted for pointer-safe feedback
            DragDropToken[] tokens = canvas.GetComponentsInChildren<DragDropToken>(true);
            foreach (DragDropToken t in tokens)
            {
                t.DragStarted -= HandlePickup;
                t.DragStarted += HandlePickup;
                boundTokens.Add(t);
            }
        }

        private void HandlePickup(DragDropToken token)
        {
            if (token == null || (coordinator != null && coordinator.IsLevelCompleted))
            {
                return;
            }

            // Light sfx handled via audioPresenter if needed; keep non-blocking
            audioPresenter?.PlaySuccess();
        }

        private void HandleDrop(DropTarget target, DragDropToken token)
        {
            if (coordinator == null || screen == null || target == null || token == null)
            {
                return;
            }

            if (celebrationActive || coordinator.IsLevelCompleted || coordinator.IsAllCompleted)
            {
                return;
            }

            if (!screen.TargetAreas.TryGetValue(target, out WildWhizScreen.TargetArea area))
            {
                return;
            }

            bool correct = coordinator.TryClassify(token.TokenId, target.ClassificationId);
            if (!correct)
            {
                screen.SetFeedbackTryAgain();
                audioPresenter?.PlayError();
                RefreshProgress();
                return;
            }

            // Pointer-safe Accept parents to ResolvedTokensRoot and blocks re-drag via blocksRaycasts=false
            token.Accept(area.ResolvedTokensRoot, area.ResolvedCount, area.SlotCount);
            area.ResolvedCount++;
            screen.SetFeedbackCorrect(area.Label);
            audioPresenter?.PlaySuccess();
            RefreshProgress();

            if (coordinator.IsLevelCompleted)
            {
                celebrationCoroutine = StartCoroutine(CelebrateThenAdvance());
            }
        }

        private System.Collections.IEnumerator CelebrateThenAdvance()
        {
            celebrationActive = true;
            screen.SetFeedbackComplete();
            screen.ShowCelebration();
            float end = Time.unscaledTime + 0.9f;
            while (Time.unscaledTime < end) yield return null;
            screen.HideCelebration();
            celebrationActive = false;
            celebrationCoroutine = null;
            if (coordinator.IsAllCompleted)
            {
                finalCompletionActive = true;
                screen.ShowFinalCompletion(HandleContinue);
            }
            else if (coordinator.TryAdvance()) RebuildForNextLevel();
        }

        private void HandleContinue()
        {
            if (!finalCompletionActive || resultReported) return;
            finalCompletionActive = false;
            screen?.HideCelebration();
            ReportCompleted();
            services?.GameLauncher.ShowLobby();
        }

        private void RebuildForNextLevel()
        {
            TearDownGeneratedInterface();

            Font font = interfaceFont != null ? interfaceFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            screen = new WildWhizScreen();
            screen.Build(canvas, font, coordinator.CurrentLevel, HandleSpeak, HandleClose, speakerSprite);
            audioPresenter?.SetInstructionClip(coordinator.CurrentLevel.InstructionClip != null ? coordinator.CurrentLevel.InstructionClip : instructionClip);
            WireCurrentLevel();
            RefreshProgress();
            screen.SetInstruction(coordinator.CurrentLevel.Instruction);
        }

        private void TearDownGeneratedInterface()
        {
            StopCelebrationCoroutine();
            finalCompletionActive = false;
            celebrationActive = false;
            screen?.HideCelebration();
            foreach (DragDropToken token in boundTokens)
            {
                if (token != null)
                {
                    token.DragStarted -= HandlePickup;
                }
            }

            boundTokens.Clear();
            if (screen?.GeneratedRoot != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(screen.GeneratedRoot);
                }
                else
#endif
                {
                    Destroy(screen.GeneratedRoot);
                }
            }

            screen = null;
        }

        private void RefreshProgress()
        {
            if (screen != null && coordinator != null)
            {
                screen.RefreshProgress(coordinator.CurrentLevel, coordinator.ResolvedCount, coordinator.TotalCount);
            }
        }

        private void HandleSpeak()
        {
            // Replay instruction audio; English text must remain visible even if clip missing
            audioPresenter?.Replay();
            if (screen != null && coordinator != null)
            {
                screen.SetInstruction(coordinator.CurrentLevel.Instruction);
            }
        }

        private void HandleClose()
        {
            if (celebrationActive || finalCompletionActive || coordinator?.IsLevelCompleted == true)
            {
                return;
            }

            audioPresenter?.StopAll();
            ReportAbandoned();
            services?.GameLauncher.ShowLobby();
        }

        private void OnDisable()
        {
            audioPresenter?.StopAll();
            TearDownGeneratedInterface();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                audioPresenter?.StopAll();
            }
        }

        private void OnDestroy()
        {
            audioPresenter?.StopAll();
            TearDownGeneratedInterface();
        }

        private void StopCelebrationCoroutine()
        {
            if (celebrationCoroutine != null)
            {
                StopCoroutine(celebrationCoroutine);
                celebrationCoroutine = null;
            }
        }

        private void ReportCompleted()
        {
            if (resultReported || coordinator == null)
            {
                return;
            }

            resultReported = true;
            audioPresenter?.StopAll();
            MiniGameResult result = new(Id, MiniGameCompletionState.Completed, 100, coordinator.TotalCount, coordinator.Attempts, services.Session.SelectedDifficultyId);
            services?.GameLauncher.Complete(result);
            Completed?.Invoke(result);
        }

        private void ReportAbandoned()
        {
            if (resultReported)
            {
                return;
            }

            resultReported = true;
            audioPresenter?.StopAll();
            int correct = coordinator != null ? coordinator.ResolvedCount : 0;
            int attempts = coordinator != null ? coordinator.Attempts : 0;
            MiniGameResult result = new(Id, MiniGameCompletionState.Abandoned, 0, correct, attempts, services.Session.SelectedDifficultyId);
            services?.GameLauncher.Complete(result);
            Completed?.Invoke(result);
        }
    }
}
