using System.Collections;
using System.Collections.Generic;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.GameKits.DragDrop;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using Lbs.MiniGames.Shared.Audio;
using Lbs.MiniGames.Shared.Results;
using Lbs.MiniGames.Shared.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.FunnyFaceDrag
{
    public sealed class FunnyFaceDragGame : MonoBehaviour, IAppScene, ILevelTransitionParticipant
    {
        private static readonly Color SlotBaseColor = Color.clear;
        private static readonly Color Error = new(0.702f, 0.149f, 0.118f);

        // Layout - consistent square size for pieces and slots
        private static readonly Vector2 PieceSize = new(280, 280);
        private static readonly Vector2 SlotSize = new(280, 280);

        // Principal1 reference paper top-left (as in Image2)
        private static readonly Vector2 Principal1Center = new(380, 200);
        private static readonly Vector2 Principal1Size = new(380, 380);

        // Destination panel containing Principal2 artwork (left assembled + right dashed squares)
        private static readonly Vector2 DestinationPanelCenter = new(750, 500);
        private static readonly Vector2 DestinationPanelSize = new(960, 560);

        // Slots overlaying dashed squares inside Principal2
        private static readonly Vector2 TopSlotCenter = new(920, 600);
        private static readonly Vector2 BottomSlotCenter = new(920, 420);

        // Three draggable pieces at bottom (as in Image2)
        private static readonly Vector2 YellowPieceCenter = new(620, 900);
        private static readonly Vector2 GreenPieceCenter = new(960, 900);
        private static readonly Vector2 PurplePieceCenter = new(1300, 900);

        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite principal1Sprite;
        [SerializeField] private Sprite principal2Sprite;
        [SerializeField] private Sprite drag1Sprite; // yellow distractor
        [SerializeField] private Sprite drag2Sprite; // green correct bottom
        [SerializeField] private Sprite drag3Sprite; // purple correct top
        [SerializeField] private Sprite exitIcon, hongNeutral, hong1, hong2, hong3, finalStar;
        [SerializeField] private Sprite celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3;
        [SerializeField] private AudioClip instruction, successSfx, failSfx;
        [SerializeField] private AudioClip[] compliments, encouragements;
        [SerializeField] private SharedAudioLibrary sharedLibrary;
        [SerializeField] private Font font, scoreFont;
        [SerializeField] private FinalCelebrationConfiguration celebrationConfiguration;

        private readonly FunnyFaceState state = new();
        private readonly List<DragDropCard> pieces = new();
        private readonly Dictionary<string, RectTransform> slots = new();
        private readonly Dictionary<string, RoundedSurface> slotSurfaces = new();
        private readonly Dictionary<string, ProximityHighlighter> slotHighlighters = new();
        private AppServices services;
        private IAppAudioService audio;
        private RectTransform board;
        private LevelChrome levelChrome;
        private Image hongImage;
        private Coroutine hongPlayback;
        private Coroutine resolutionSequence;
        private FinalCelebrationPresenter celebrationPresenter;
        private int activePointer = int.MinValue;
        private bool transitionHandoffPending;

        public RectTransform TransitionRoot => board;

        public void Configure(AppServices appServices)
        {
            services = appServices;
            audio = appServices?.Audio;
            celebrationPresenter ??= new FinalCelebrationPresenter(this, celebrationConfiguration);
            Build();
            EnsureSharedAudio();
            EnsureScoreFont();
            if (appServices?.LevelSequence?.IsTransitioning == true) transitionHandoffPending = true;
            else StartInstructionPlayback();
        }

        public void CompleteTransitionHandoff()
        {
            if (!transitionHandoffPending) return;
            transitionHandoffPending = false;
            StartInstructionPlayback();
        }

        private void Build()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (!canvas || board != null) return;

            board = new GameObject("FunnyFaceDragBoard", typeof(RectTransform)).GetComponent<RectTransform>();
            board.SetParent(canvas.transform, false);
            UiFactory.Stretch(board, 0);

            // Full-screen background with pencils
            Image bgImage = UiFactory.CreateImage(board, "Background", Color.white);
            bgImage.sprite = backgroundSprite;
            bgImage.preserveAspect = false;
            bgImage.raycastTarget = false;
            UiFactory.Stretch(bgImage.rectTransform, 0);
            if (backgroundSprite == null)
            {
                bgImage.color = new Color(0.992f, 0.894f, 0.722f, 1f);
            }

            levelChrome = LevelChromeFactory.Build(board, font, exitIcon, hongNeutral, ReturnToLobby, ToggleInstruction);
            hongImage = levelChrome.HongImage;

            CreatePrincipal1();
            CreateDestinationWithSlots();
            CreatePiece("YellowPiece", FunnyFaceRule.YellowPiece, drag1Sprite, YellowPieceCenter);
            CreatePiece("GreenPiece", FunnyFaceRule.GreenPiece, drag2Sprite, GreenPieceCenter);
            CreatePiece("PurplePiece", FunnyFaceRule.PurplePiece, drag3Sprite, PurplePieceCenter);
        }

        private void CreatePrincipal1()
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, "Principal1", Color.clear, 24f);
            Pixel(surface.rectTransform, Principal1Center, Principal1Size);
            surface.OutlineThickness = 0f;
            surface.raycastTarget = false;
            Image img = UiFactory.CreateImage(surface.rectTransform, "Artwork", Color.white);
            img.sprite = principal1Sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            UiFactory.Stretch(img.rectTransform, 0);
        }

        private void CreateDestinationWithSlots()
        {
            // Destination panel with Principal2 artwork stretched inside
            RoundedSurface bgSurface = UiFactory.CreateRoundedSurface(board, "DestinationPanel", Color.clear, 28f);
            Pixel(bgSurface.rectTransform, DestinationPanelCenter, DestinationPanelSize);
            bgSurface.OutlineThickness = 0f;
            bgSurface.raycastTarget = false;
            Image bgImage = UiFactory.CreateImage(bgSurface.rectTransform, "Principal2", Color.white);
            bgImage.sprite = principal2Sprite;
            bgImage.preserveAspect = true;
            bgImage.raycastTarget = false;
            UiFactory.Stretch(bgImage.rectTransform, 8f);

            CreateSlot(FunnyFaceRule.TopSlot, TopSlotCenter);
            CreateSlot(FunnyFaceRule.BottomSlot, BottomSlotCenter);
        }

        private void CreateSlot(string id, Vector2 center)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, id, SlotBaseColor, 28f);
            Pixel(surface.rectTransform, center, SlotSize);
            surface.OutlineThickness = 7f;
            surface.raycastTarget = false;

            // ProximityHighlighter for ShapeAnalogy-style feedback
            CanvasGroup cg = surface.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
            ProximityHighlighter highlighter = surface.gameObject.AddComponent<ProximityHighlighter>();
            highlighter.HideImmediate();

            slots.Add(id, surface.rectTransform);
            slotSurfaces.Add(id, surface);
            slotHighlighters.Add(id, highlighter);
        }

        private void CreatePiece(string name, string id, Sprite artwork, Vector2 center)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, name, Color.clear, 28f);
            Pixel(surface.rectTransform, center, PieceSize);
            surface.OutlineThickness = 0f;
            CanvasGroup group = surface.gameObject.AddComponent<CanvasGroup>();
            Image image = UiFactory.CreateImage(surface.rectTransform, "Artwork", Color.white);
            image.sprite = artwork;
            image.preserveAspect = true;
            UiFactory.Stretch(image.rectTransform, 0);
            DragDropCard card = surface.gameObject.AddComponent<DragDropCard>();
            card.Setup(id, group, surface.rectTransform.anchoredPosition);
            card.DragStarted += HandleDragStarted;
            card.DragMoved += HandleDragMoved;
            card.DragEnded += HandleDragEnded;
            pieces.Add(card);
        }

        private void HandleDragStarted(DragDropCard card, PointerEventData eventData)
        {
            if (state.Phase != FunnyFacePhase.Ready || activePointer != int.MinValue) return;
            activePointer = eventData.pointerId;
            StopInstruction();
            card.Lift();
        }

        private void HandleDragMoved(DragDropCard card, PointerEventData eventData)
        {
            if (eventData.pointerId != activePointer) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(board, eventData.position, eventData.pressEventCamera, out Vector2 point);
            ((RectTransform)card.transform).anchoredPosition = point;
            UpdateProximityHighlight(card);
        }

        private void HandleDragEnded(DragDropCard card, PointerEventData eventData)
        {
            if (eventData.pointerId != activePointer) return;
            activePointer = int.MinValue;
            ClearProximityHighlight();
            string slotId = FindSlot(eventData);
            FunnyFaceDropOutcome outcome = state.Drop(card.TokenId, slotId, slotId != null);
            if (outcome == FunnyFaceDropOutcome.Correct)
            {
                resolutionSequence = StartCoroutine(ResolveCorrect(card, slotId));
            }
            else if (outcome == FunnyFaceDropOutcome.Incorrect)
            {
                resolutionSequence = StartCoroutine(ResolveIncorrect(card, slotId));
            }
            else
            {
                card.Restore();
            }
        }

        private void UpdateProximityHighlight(DragDropCard card)
        {
            RectTransform cardRect = (RectTransform)card.transform;
            Vector2 cardPos = cardRect.anchoredPosition;
            foreach (KeyValuePair<string, RectTransform> kv in slots)
            {
                string id = kv.Key;
                RectTransform slotRect = kv.Value;
                float dist = Vector2.Distance(cardPos, slotRect.anchoredPosition);
                if (slotHighlighters.TryGetValue(id, out ProximityHighlighter hl))
                {
                    hl.ShowForDistance(dist);
                }
            }
        }

        private void ClearProximityHighlight()
        {
            foreach (ProximityHighlighter hl in slotHighlighters.Values)
            {
                if (hl) hl.HideImmediate();
            }
        }

        private string FindSlot(PointerEventData eventData)
        {
            foreach (KeyValuePair<string, RectTransform> slot in slots)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(slot.Value, eventData.position, eventData.pressEventCamera)) return slot.Key;
            }
            return null;
        }

        private IEnumerator ResolveCorrect(DragDropCard card, string slotId)
        {
            card.Accept(slots[slotId]);
            // No offset - sizes identical
            yield return CardAnimator.PunchPlace((RectTransform)card.transform);
            if (successSfx) audio?.PlaySfx(successSfx);
            PlayRandom(compliments);
            if (state.Phase == FunnyFacePhase.Celebrating) yield return Celebrate();
            resolutionSequence = null;
        }

        private IEnumerator ResolveIncorrect(DragDropCard card, string slotId)
        {
            if (slotId != null && slotSurfaces.TryGetValue(slotId, out RoundedSurface surface))
            {
                surface.color = Error;
            }
            if (failSfx) audio?.PlaySfx(failSfx);
            PlayRandom(encouragements);
            yield return CardAnimator.ShakeBoard(board);
            if (slotId != null && slotSurfaces.TryGetValue(slotId, out RoundedSurface s2))
            {
                s2.color = Color.clear;
            }
            ClearProximityHighlight();
            card.Restore();
            state.FinishIncorrect();
            resolutionSequence = null;
        }

        private IEnumerator Celebrate()
        {
            CreateCelebration();
            yield return new WaitForSecondsRealtime(celebrationPresenter.PresentationDelay);
            celebrationPresenter.ShowFinal(CelebrationInput());
            state.FinishCelebration();
            string resultGameId = services?.Session?.CurrentRequest?.Game?.GameId ?? "funnyface.drag";
            string difficultyId = services?.Session?.SelectedDifficultyId;
            services?.GameLauncher.Complete(new MiniGameResult(resultGameId, MiniGameCompletionState.Completed, state.Score, 1, 1, difficultyId));
            // Terminal game: 0.35s then enable final tap to lobby (no auto-advance)
            yield return new WaitForSecondsRealtime(0.35f);
            state.EnableFinalInput();
        }

        private void StartInstructionPlayback()
        {
            PlayInstruction();
            if (hongPlayback == null) hongPlayback = StartCoroutine(AnimateHong());
        }

        private void EnsureScoreFont()
        {
            if (scoreFont) return;
            scoreFont = Resources.Load<Font>("Fonts/Nunito-Black");
            if (!scoreFont) scoreFont = Resources.Load<Font>("Nunito-Black");
            if (!scoreFont) scoreFont = font;
        }

        private void EnsureSharedAudio()
        {
            if (successSfx == null) successSfx = sharedLibrary != null ? sharedLibrary.SuccessClip : Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_success_true_answer");
            if (failSfx == null) failSfx = sharedLibrary != null ? sharedLibrary.FailClip : Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_fail_incorrect_answer");
            if (compliments == null || compliments.Length == 0) compliments = sharedLibrary != null && sharedLibrary.Compliments.Count > 0 ? CopyClips(sharedLibrary.Compliments) : Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Compliments/en");
            if (encouragements == null || encouragements.Length == 0) encouragements = sharedLibrary != null && sharedLibrary.Encouragements.Count > 0 ? CopyClips(sharedLibrary.Encouragements) : Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Encouragement/en");
        }

        private FinalCelebrationInput CelebrationInput() => new(state.Score, state.StarCount, scoreFont ? scoreFont : font, finalStar, celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3);
        private void CreateCelebration() => celebrationPresenter.ShowCelebration(board, CelebrationInput());
        private void PlayInstruction() { if (instruction) audio?.PlayVoice(instruction); }
        private void StopInstruction() { audio?.StopVoiceIfPlaying(instruction); }
        private void ToggleInstruction() { if (state.Phase != FunnyFacePhase.Ready) return; if (audio != null && audio.IsVoicePlaying(instruction)) audio.StopVoiceIfPlaying(instruction); else PlayInstruction(); }
        private void PlayRandom(AudioClip[] clips) { if (clips != null && clips.Length > 0) audio?.PlayVoice(clips[Random.Range(0, clips.Length)]); }
        private static AudioClip[] CopyClips(System.Collections.Generic.IReadOnlyList<AudioClip> clips) { AudioClip[] copy = new AudioClip[clips.Count]; for (int i = 0; i < copy.Length; i++) copy[i] = clips[i]; return copy; }
        private IEnumerator AnimateHong() { int[] frames = { 1, 2, 3, 2, 1 }; int index = 0; while (true) { bool playing = audio != null && audio.IsVoicePlaying(instruction); if (hongImage) hongImage.sprite = playing ? (frames[index++ % frames.Length] == 1 ? hong1 : frames[(index - 1 + frames.Length) % frames.Length] == 2 ? hong2 : hong3) : hongNeutral; yield return new WaitForSecondsRealtime(.18f); } }
        private void Update() { if (state.AcceptFinalInput() && (Input.GetMouseButtonDown(0) || Input.touchCount > 0)) ReturnToLobby(); }
        private void ReturnToLobby() { if (state.Phase == FunnyFacePhase.ResolvingIncorrect || state.Phase == FunnyFacePhase.Celebrating || (state.Phase == FunnyFacePhase.Final && !state.FinalInputEnabled)) return; audio?.StopMusic(); services?.GameLauncher.ShowLobby(); }
        private static void Pixel(RectTransform rect, Vector2 topOriginCenter, Vector2 size) { rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = LevelChromeLayout.ToAnchoredPosition(topOriginCenter); rect.sizeDelta = size; }

        private void OnDisable()
        {
            if (resolutionSequence != null) StopCoroutine(resolutionSequence);
            if (hongPlayback != null) StopCoroutine(hongPlayback);
            hongPlayback = null;
            activePointer = int.MinValue;
            ClearProximityHighlight();
        }

        private void OnDestroy()
        {
            foreach (DragDropCard piece in pieces)
            {
                if (!piece) continue;
                piece.DragStarted -= HandleDragStarted;
                piece.DragMoved -= HandleDragMoved;
                piece.DragEnded -= HandleDragEnded;
            }
        }
    }
}
