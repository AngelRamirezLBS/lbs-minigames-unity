using System.Collections;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.GameKits.DragDrop;
using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using Lbs.MiniGames.Shared.Audio;
using Lbs.MiniGames.Shared.Results;
using Lbs.MiniGames.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.TrianglesCount
{
    public sealed class TrianglesCountGame : MonoBehaviour, IAppScene, ILevelTransitionParticipant
    {
        private static readonly Color Background = new(0.9686f, 0.9608f, 0.9804f, 1f); // #F7F5FA
        private static readonly Color NormalBorder = new(.78f, .78f, .78f, 1f);
        private static readonly Color Error = new(.70f, .15f, .12f);
        private static readonly Color Success = new(.09f, .48f, .29f);
        private static readonly Color Ink = new(.14f, .10f, .21f);

        [SerializeField] private Sprite principalSprite;
        [SerializeField] private Sprite revealSprite;
        [SerializeField] private Sprite exitIcon, hongNeutral, hong1, hong2, hong3, finalStar;
        [SerializeField] private Sprite celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3;
        [SerializeField] private AudioClip instruction, successSfx, failSfx;
        [SerializeField] private AudioClip[] compliments, encouragements;
        [SerializeField] private SharedAudioLibrary sharedLibrary;
        [SerializeField] private Font font, scoreFont;
        [SerializeField] private FinalCelebrationConfiguration celebrationConfiguration;

        private readonly SelectionGameState state = new();
        private readonly System.Collections.Generic.List<Button> answers = new();
        private AppServices services;
        private IAppAudioService audio;
        private RectTransform board;
        private Image hongImage;
        private LevelChrome levelChrome;
        private Coroutine hongPlayback;
        private Coroutine selectionSequence;
        private FinalCelebrationPresenter celebrationPresenter;
        private bool transitionHandoffPending;

        private RectTransform principalRoot;
        private RectTransform optionsRoot;
        private Image revealImage;

        public RectTransform TransitionRoot => board;

        public void Configure(AppServices appServices)
        {
            services = appServices;
            audio = appServices?.Audio;
            celebrationPresenter ??= new FinalCelebrationPresenter(this, celebrationConfiguration);
            Build();
            EnsureSharedAudio();
            if (appServices?.LevelSequence?.IsTransitioning == true) transitionHandoffPending = true;
            else StartInstructionPlayback();
        }

        public void CompleteTransitionHandoff()
        {
            if (!transitionHandoffPending) return;
            transitionHandoffPending = false;
            StartInstructionPlayback();
        }

        private void StartInstructionPlayback()
        {
            PlayInstruction();
            if (hongPlayback == null) hongPlayback = StartCoroutine(AnimateHong());
        }

        private void Build()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (!canvas || board != null) return;

            board = new GameObject("TrianglesCountBoard", typeof(RectTransform)).GetComponent<RectTransform>();
            board.SetParent(canvas.transform, false);
            UiFactory.Stretch(board, 0);

            UiFactory.Stretch(UiFactory.CreateImage(board, "Background", Background).rectTransform, 0);

            levelChrome = LevelChromeFactory.Build(board, font, exitIcon, hongNeutral, ReturnToLobby, ToggleInstruction);
            hongImage = levelChrome.HongImage;

            // Principal image container centered large
            principalRoot = new GameObject("PrincipalRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            principalRoot.SetParent(board, false);
            Pixel(principalRoot, new Vector2(960, 430), new Vector2(980, 540));
            Image principal = UiFactory.CreateImage(principalRoot, "Principal", Color.white);
            principal.sprite = principalSprite;
            principal.preserveAspect = true;
            principal.raycastTarget = false;
            UiFactory.Stretch(principal.rectTransform, 0);

            // Reveal image initially inactive, same center but larger
            GameObject revealRootObj = new("RevealRoot", typeof(RectTransform));
            RectTransform revealRoot = revealRootObj.GetComponent<RectTransform>();
            revealRoot.SetParent(board, false);
            Pixel(revealRoot, new Vector2(960, 430), new Vector2(1180, 560));
            revealImage = UiFactory.CreateImage(revealRoot, "Reveal", Color.white);
            revealImage.sprite = revealSprite;
            revealImage.preserveAspect = true;
            revealImage.raycastTarget = false;
            UiFactory.Stretch(revealImage.rectTransform, 0);
            revealRootObj.SetActive(false);
            // keep reference to root via revealImage.transform.parent
            revealImage.gameObject.SetActive(false);

            // Options grid bottom row
            optionsRoot = new GameObject("OptionsRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            optionsRoot.SetParent(board, false);
            UiFactory.Stretch(optionsRoot, 0);
            // Create 4 option buttons: 4, 2, 3, 1  correct is 3 — enlarged for tablet/TV touch targets
            CreateOption("4", new Vector2(510, 860));
            CreateOption("2", new Vector2(850, 860));
            CreateOption("3", new Vector2(1190, 860));
            CreateOption("1", new Vector2(1530, 860));

            EnsureScoreFont();
        }

        private void EnsureScoreFont()
        {
            if (scoreFont) return;
            scoreFont = Resources.Load<Font>("Fonts/Nunito-Black");
            if (!scoreFont) scoreFont = Resources.Load<Font>("Nunito-Black");
            if (!scoreFont) scoreFont = font;
        }

        private void CreateOption(string id, Vector2 center)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(optionsRoot, id + "Card", Color.white, 22f);
            Pixel(surface.rectTransform, center, new Vector2(320, 145));
            surface.OutlineThickness = 4f;
            // border color is NormalBorder via surface.color? Actually RoundedSurface color is fill; outline is same color with thickness.
            // Use surface.color = white fill, but we need border. The pattern in ObjectSelection uses NormalBorder as color with outline thickness.
            // For white card with gray border, set color to NormalBorder and inner fill white. Simpler: set color = NormalBorder and add inner white.
            // However to keep consistent with spec (white rounded buttons), use white fill with gray outline via OutlineThickness.
            // RoundedSurface draws outline using same color; so to have gray border we set color = NormalBorder and create inner.
            // Let's create inner fill to mimic ObjectSelection pattern.
            surface.color = NormalBorder;
            RoundedSurface inner = UiFactory.CreateRoundedSurface(surface.rectTransform, "Fill", Color.white, 18f, false);
            UiFactory.Stretch(inner.rectTransform, surface.OutlineThickness);

            Text label = UiFactory.CreateText(surface.rectTransform, "Label", font, 62, TextAnchor.MiddleCenter, Ink);
            label.text = id;
            label.raycastTarget = false;
            label.fontStyle = FontStyle.Bold;
            UiFactory.Stretch(label.rectTransform, 0);

            // Ensure surface is raycast target
            surface.raycastTarget = true;
            inner.raycastTarget = false;

            Button button = surface.gameObject.AddComponent<Button>();
            button.targetGraphic = surface;
            button.onClick.AddListener(() => Select(id, surface));
            answers.Add(button);
        }

        private void Select(string id, RoundedSurface surface)
        {
            if (state.Phase != SelectionPhase.Ready) return;
            StopInstruction();
            SetInteractable(false);
            bool correct = state.Select(id, TrianglesCountRule.CorrectAnswer);
            selectionSequence = StartCoroutine(correct ? ResolveCorrect(surface) : ResolveIncorrect(surface));
        }

        private IEnumerator ResolveIncorrect(RoundedSurface surface)
        {
            surface.color = Error;
            // keep inner fill white, only border signals error
            if (failSfx) audio?.PlaySfx(failSfx);
            PlayRandom(encouragements);
            yield return CardAnimator.ShakeBoard(board);
            surface.color = NormalBorder;
            state.FinishIncorrect();
            SetInteractable(true);
        }

        private IEnumerator ResolveCorrect(RoundedSurface surface)
        {
            yield return CardAnimator.PunchPlace(surface.rectTransform);
            surface.color = Success;
            if (successSfx) audio?.PlaySfx(successSfx);
            PlayRandom(compliments);

            // Hide principal + options, show reveal, wait 0.5s, then celebration keeping reveal in bg
            if (principalRoot) principalRoot.gameObject.SetActive(false);
            if (optionsRoot) optionsRoot.gameObject.SetActive(false);
            if (revealImage)
            {
                revealImage.transform.parent.gameObject.SetActive(true);
                revealImage.gameObject.SetActive(true);
                // punch reveal slightly for delight
                yield return CardAnimator.PunchPlace(revealImage.rectTransform);
            }

            yield return new WaitForSecondsRealtime(0.5f);

            CreateCelebration();
            yield return new WaitForSecondsRealtime(celebrationPresenter.PresentationDelay);
            CreateFinal();
            state.FinishCelebration();
            // keep reveal visible during celebration - do not clear. FinalCelebrationPresenter overlays on top.
            yield return new WaitForSecondsRealtime(0.35f);
            state.EnableFinalInput();
            selectionSequence = null;
        }

        private FinalCelebrationInput CelebrationInput() => new(state.Score, state.StarCount, scoreFont ? scoreFont : font, finalStar, celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3);
        private void CreateCelebration() => celebrationPresenter.ShowCelebration(board, CelebrationInput());
        private void CreateFinal()
        {
            celebrationPresenter.ShowFinal(CelebrationInput());
            services?.GameLauncher.Complete(new MiniGameResult("triangles.count", MiniGameCompletionState.Completed, state.Score, 1, 1, services.Session.SelectedDifficultyId));
        }

        private void SetInteractable(bool value)
        {
            foreach (Button answer in answers) if (answer) answer.interactable = value;
            if (levelChrome == null) return;
            if (levelChrome.ExitButton != null) levelChrome.ExitButton.interactable = value;
            if (levelChrome.HongButton != null) levelChrome.HongButton.interactable = value;
        }

        private void EnsureSharedAudio()
        {
            if (successSfx == null) successSfx = sharedLibrary != null ? sharedLibrary.SuccessClip : Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_success_true_answer");
            if (failSfx == null) failSfx = sharedLibrary != null ? sharedLibrary.FailClip : Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_fail_incorrect_answer");
            if (compliments == null || compliments.Length == 0) compliments = sharedLibrary != null && sharedLibrary.Compliments.Count > 0 ? CopyClips(sharedLibrary.Compliments) : Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Compliments/en");
            if (encouragements == null || encouragements.Length == 0) encouragements = sharedLibrary != null && sharedLibrary.Encouragements.Count > 0 ? CopyClips(sharedLibrary.Encouragements) : Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Encouragement/en");
        }

        private static AudioClip[] CopyClips(System.Collections.Generic.IReadOnlyList<AudioClip> clips)
        {
            AudioClip[] copy = new AudioClip[clips.Count];
            for (int i = 0; i < copy.Length; i++) copy[i] = clips[i];
            return copy;
        }

        private void PlayInstruction() { if (instruction) audio?.PlayVoice(instruction); }
        private void StopInstruction() { audio?.StopVoiceIfPlaying(instruction); }
        private void ToggleInstruction() { if (state.Phase != SelectionPhase.Ready) return; if (audio != null && audio.IsVoicePlaying(instruction)) audio.StopVoiceIfPlaying(instruction); else PlayInstruction(); }
        private void PlayRandom(AudioClip[] clips) { if (clips != null && clips.Length > 0) audio?.PlayVoice(clips[Random.Range(0, clips.Length)]); }
        private IEnumerator AnimateHong() { int[] frames = { 1, 2, 3, 2, 1 }; int index = 0; while (true) { bool playing = audio != null && audio.IsVoicePlaying(instruction); if (hongImage) hongImage.sprite = playing ? (frames[index++ % frames.Length] == 1 ? hong1 : frames[(index - 1 + frames.Length) % frames.Length] == 2 ? hong2 : hong3) : hongNeutral; yield return new WaitForSecondsRealtime(.18f); } }
        private void Update() { if (state.AcceptFinalInput() && (Input.GetMouseButtonDown(0) || Input.touchCount > 0)) ReturnToLobby(); }
        private void ReturnToLobby() { if (state.Phase == SelectionPhase.ResolvingIncorrect || state.Phase == SelectionPhase.Celebrating || (state.Phase == SelectionPhase.Final && !state.IsFinalInputEnabled)) return; audio?.StopMusic(); services?.GameLauncher.ShowLobby(); }
        private void OnDisable()
        {
            if (selectionSequence != null) StopCoroutine(selectionSequence);
            if (hongPlayback != null) StopCoroutine(hongPlayback);
            selectionSequence = null;
            hongPlayback = null;
            celebrationPresenter?.Clear();
            audio?.StopVoiceIfPlaying(instruction);
        }

        private static void Pixel(RectTransform rect, Vector2 top, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = LevelChromeLayout.ToAnchoredPosition(top);
            rect.sizeDelta = size;
        }
    }
}
