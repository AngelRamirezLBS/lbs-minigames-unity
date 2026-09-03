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

namespace Lbs.MiniGames.Games.TrianglesShapeLogic
{
    public sealed class TrianglesShapeLogicGame : MonoBehaviour, IAppScene, ILevelTransitionParticipant
    {
        private static readonly Color Background = new(0.969f, 0.961f, 0.98f);
        private static readonly Color SlotBaseColor = Color.clear;
        private static readonly Color Error = new(0.702f, 0.149f, 0.118f);

        // Principal table art (Principal.png, 1708x921) stretched exactly onto this
        // same-aspect panel, so normalized art coordinates map 1:1 to board space.
        private static readonly Vector2 PrincipalPanelCenter = new(1010, 440);
        private static readonly Vector2 PrincipalPanelSize = new(1320, 712);

        // Drop zones measured from the baked dashed boxes in Principal.png:
        // blue box x490-789/y484-771, red box x1249-1547/y484-771.
        // Slot rects match those boxes exactly; accepted cards snap to these centers.
        private static readonly Vector2 BlueSlotCenter = new(844, 569);
        private static readonly Vector2 RedSlotCenter = new(1430, 569);
        private static readonly Vector2 SlotSize = new(231, 222);

        // Two draggable triangles on the bottom row (red left, blue right).
        private static readonly Vector2 RedPieceCenter = new(700, 900);
        private static readonly Vector2 BluePieceCenter = new(1150, 900);
        private static readonly Vector2 PieceSize = new(220, 210);

        [SerializeField] private Sprite principalArtwork;
        [SerializeField] private Sprite redTriangleArtwork;
        [SerializeField] private Sprite blueTriangleArtwork;
        [SerializeField] private Sprite exitIcon, hongNeutral, hong1, hong2, hong3, finalStar;
        [SerializeField] private Sprite celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3;
        [SerializeField] private AudioClip instruction, successSfx, failSfx;
        [SerializeField] private AudioClip[] compliments, encouragements;
        [SerializeField] private SharedAudioLibrary sharedLibrary;
        [SerializeField] private Font font, scoreFont;
        [SerializeField] private FinalCelebrationConfiguration celebrationConfiguration;

        private readonly TrianglesShapeLogicState state = new();
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

            board = new GameObject("TrianglesShapeLogicBoard", typeof(RectTransform)).GetComponent<RectTransform>();
            board.SetParent(canvas.transform, false);
            UiFactory.Stretch(board, 0);
            UiFactory.Stretch(UiFactory.CreateImage(board, "Background", Background).rectTransform, 0);
            levelChrome = LevelChromeFactory.Build(board, font, exitIcon, hongNeutral, ReturnToLobby, ToggleInstruction);
            hongImage = levelChrome.HongImage;

            CreatePrincipalPanel();
            CreateSlot(TrianglesShapeLogicRule.BlueSplashSlot, BlueSlotCenter);
            CreateSlot(TrianglesShapeLogicRule.RedSplashSlot, RedSlotCenter);
            CreatePiece("RedPiece", TrianglesShapeLogicRule.RedPiece, redTriangleArtwork, RedPieceCenter);
            CreatePiece("BluePiece", TrianglesShapeLogicRule.BluePiece, blueTriangleArtwork, BluePieceCenter);
            EnsureScoreFont();
        }

        private void CreatePrincipalPanel()
        {
            // Single table image with splashes, worked green example and dashed targets baked in.
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, "PrincipalPanel", Color.clear, 34f);
            Pixel(surface.rectTransform, PrincipalPanelCenter, PrincipalPanelSize);
            surface.OutlineThickness = 0f;
            surface.raycastTarget = false;
            Image image = UiFactory.CreateImage(surface.rectTransform, "Principal", Color.white);
            image.sprite = principalArtwork;
            image.preserveAspect = false;
            image.raycastTarget = false;
            UiFactory.Stretch(image.rectTransform, 0);
        }

        private void CreateSlot(string id, Vector2 center)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, id, SlotBaseColor, 28f);
            Pixel(surface.rectTransform, center, SlotSize);
            surface.OutlineThickness = 7f;
            surface.raycastTarget = false;

            // ShapeAnalogy-style orange proximity frame.
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
            image.raycastTarget = false;
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
            if (state.Phase != TrianglesShapeLogicPhase.Ready || activePointer != int.MinValue) return;
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
            TrianglesShapeLogicDropOutcome outcome = state.Drop(card.TokenId, slotId, slotId != null);
            if (outcome == TrianglesShapeLogicDropOutcome.Correct)
            {
                resolutionSequence = StartCoroutine(ResolveCorrect(card, slotId));
            }
            else if (outcome == TrianglesShapeLogicDropOutcome.Incorrect)
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
            // Exact snap: the slot rect matches the baked dashed box, no offset.
            card.Accept(slots[slotId]);
            yield return CardAnimator.PunchPlace((RectTransform)card.transform);
            if (successSfx) audio?.PlaySfx(successSfx);
            PlayRandom(compliments);
            if (state.Phase == TrianglesShapeLogicPhase.Celebrating) yield return Celebrate();
        }

        private IEnumerator ResolveIncorrect(DragDropCard card, string slotId)
        {
            if (slotId != null) slotSurfaces[slotId].color = Error;
            if (failSfx) audio?.PlaySfx(failSfx);
            PlayRandom(encouragements);
            yield return CardAnimator.ShakeBoard(board);
            if (slotId != null) slotSurfaces[slotId].color = Color.clear;
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
            string resultGameId = services?.Session?.CurrentRequest?.Game?.GameId ?? "triangles.shape.logic";
            string difficultyId = services?.Session?.SelectedDifficultyId;
            services?.GameLauncher.Complete(new MiniGameResult(resultGameId, MiniGameCompletionState.Completed, state.Score, 1, 1, difficultyId));
            // Standalone terminal: enable final tap to lobby (no sequence advance yet).
            yield return new WaitForSecondsRealtime(2f);
            state.EnableFinalInput();
            resolutionSequence = null;
        }

        private FinalCelebrationInput CelebrationInput() => new(state.Score, state.StarCount, scoreFont ? scoreFont : font, finalStar, celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3);
        private void CreateCelebration() => celebrationPresenter.ShowCelebration(board, CelebrationInput());

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

        private static AudioClip[] CopyClips(System.Collections.Generic.IReadOnlyList<AudioClip> clips) { AudioClip[] copy = new AudioClip[clips.Count]; for (int i = 0; i < copy.Length; i++) copy[i] = clips[i]; return copy; }
        private void PlayInstruction() { if (instruction) audio?.PlayVoice(instruction); }
        private void StopInstruction() { audio?.StopVoiceIfPlaying(instruction); }
        private void ToggleInstruction() { if (state.Phase != TrianglesShapeLogicPhase.Ready) return; if (audio != null && audio.IsVoicePlaying(instruction)) audio.StopVoiceIfPlaying(instruction); else PlayInstruction(); }
        private void PlayRandom(AudioClip[] clips) { if (clips != null && clips.Length > 0) audio?.PlayVoice(clips[Random.Range(0, clips.Length)]); }
        private IEnumerator AnimateHong() { int[] frames = { 1, 2, 3, 2, 1 }; int index = 0; while (true) { bool playing = audio != null && audio.IsVoicePlaying(instruction); if (hongImage) hongImage.sprite = playing ? (frames[index++ % frames.Length] == 1 ? hong1 : frames[(index - 1 + frames.Length) % frames.Length] == 2 ? hong2 : hong3) : hongNeutral; yield return new WaitForSecondsRealtime(.18f); } }
        private void Update() { if (state.AcceptFinalInput() && (Input.GetMouseButtonDown(0) || Input.touchCount > 0)) ReturnToLobby(); }
        private void ReturnToLobby() { if (state.Phase == TrianglesShapeLogicPhase.ResolvingIncorrect || state.Phase == TrianglesShapeLogicPhase.Celebrating || (state.Phase == TrianglesShapeLogicPhase.Final && !state.FinalInputEnabled)) return; audio?.StopMusic(); services?.GameLauncher.ShowLobby(); }
        private static void Pixel(RectTransform rect, Vector2 topOriginCenter, Vector2 size) { rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = LevelChromeLayout.ToAnchoredPosition(topOriginCenter); rect.sizeDelta = size; }

        private void OnDisable()
        {
            if (resolutionSequence != null) StopCoroutine(resolutionSequence);
            if (hongPlayback != null) StopCoroutine(hongPlayback);
            resolutionSequence = null;
            hongPlayback = null;
            celebrationPresenter?.Clear();
            audio?.StopVoiceIfPlaying(instruction);
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
