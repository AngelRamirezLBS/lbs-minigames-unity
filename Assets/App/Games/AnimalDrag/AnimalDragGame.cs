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

namespace Lbs.MiniGames.Games.AnimalDrag
{
    public sealed class AnimalDragGame : MonoBehaviour, IAppScene, ILevelTransitionParticipant
    {
        private static readonly Color Background = new(0.969f, 0.961f, 0.98f);
        private static readonly Color SlotBaseColor = Color.clear;
        private static readonly Color HoverOrange = new(1f, 0.7176f, 0.251f); // #FFB740
        private static readonly Color Error = new(0.702f, 0.149f, 0.118f);
        private static readonly Color Success = new(0.09f, 0.48f, 0.29f);

        // Destination panel: background houses image fills the panel.
        private static readonly Vector2 DestinationPanelCenter = new(560, 540);
        private static readonly Vector2 DestinationPanelSize = new(880, 620);

        // Two hitboxes transparent over casitas.png — dotted interior only (left = green house, right = yellow house).
        private static readonly Vector2 GreenSlotCenter = new(447, 557);
        private static readonly Vector2 YellowSlotCenter = new(740, 509);
        private static readonly Vector2 SlotSize = new(300, 320);

        // Two pieces on the right side.
        private static readonly Vector2 CatPieceCenter = new(1320, 360);
        private static readonly Vector2 PigPieceCenter = new(1320, 720);
        private static readonly Vector2 PieceSize = new(260, 260);

        [SerializeField] private Sprite casitasBackground;
        [SerializeField] private Sprite catArtwork;
        [SerializeField] private Sprite pigArtwork;
        [SerializeField] private Sprite exitIcon, hongNeutral, hong1, hong2, hong3, finalStar;
        [SerializeField] private Sprite celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3;
        [SerializeField] private AudioClip instruction, successSfx, failSfx;
        [SerializeField] private AudioClip[] compliments, encouragements;
        [SerializeField] private SharedAudioLibrary sharedLibrary;
        [SerializeField] private Font font, scoreFont;
        [SerializeField] private FinalCelebrationConfiguration celebrationConfiguration;

        private readonly AnimalDragState state = new();
        private readonly List<DragDropCard> pieces = new();
        private readonly Dictionary<string, RectTransform> slots = new();
        private readonly Dictionary<string, RoundedSurface> slotSurfaces = new();
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
        private string hoveredSlotId;

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

        private void Build()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (!canvas || board != null) return;

            board = new GameObject("AnimalDragBoard", typeof(RectTransform)).GetComponent<RectTransform>();
            board.SetParent(canvas.transform, false);
            UiFactory.Stretch(board, 0);
            UiFactory.Stretch(UiFactory.CreateImage(board, "Background", Background).rectTransform, 0);
            levelChrome = LevelChromeFactory.Build(board, font, exitIcon, hongNeutral, ReturnToLobby, ToggleInstruction);
            hongImage = levelChrome.HongImage;

            CreateDestinationBackground();
            CreateSlot(AnimalDragRule.YellowHouseSlot, YellowSlotCenter);
            CreateSlot(AnimalDragRule.GreenHouseSlot, GreenSlotCenter);
            CreatePiece("CatPiece", AnimalDragRule.CatPiece, catArtwork, CatPieceCenter);
            CreatePiece("PigPiece", AnimalDragRule.PigPiece, pigArtwork, PigPieceCenter);
            EnsureScoreFont();
        }

        private void CreateDestinationBackground()
        {
            // Single background image with both houses (casitas.png). Transparent slots overlay on dotted interior.
            RoundedSurface bgSurface = UiFactory.CreateRoundedSurface(board, "DestinationBackground", Color.clear, 34f);
            Pixel(bgSurface.rectTransform, DestinationPanelCenter, DestinationPanelSize);
            bgSurface.OutlineThickness = 0f;
            bgSurface.raycastTarget = false;
            Image bgImage = UiFactory.CreateImage(bgSurface.rectTransform, "Casitas", Color.white);
            bgImage.sprite = casitasBackground;
            bgImage.preserveAspect = true;
            UiFactory.Stretch(bgImage.rectTransform, 6f);
            bgImage.raycastTarget = false;
        }

        private void CreateSlot(string id, Vector2 center)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, id, SlotBaseColor, 34f);
            Pixel(surface.rectTransform, center, SlotSize);
            surface.OutlineThickness = 7f;
            surface.raycastTarget = false;
            slots.Add(id, surface.rectTransform);
            slotSurfaces.Add(id, surface);
        }

        private void CreatePiece(string name, string id, Sprite artwork, Vector2 center)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, name, Color.clear, 34f);
            Pixel(surface.rectTransform, center, PieceSize);
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
            if (state.Phase != AnimalDragPhase.Ready || activePointer != int.MinValue) return;
            activePointer = eventData.pointerId;
            StopInstruction();
            card.Lift();
        }

        private void HandleDragMoved(DragDropCard card, PointerEventData eventData)
        {
            if (eventData.pointerId != activePointer) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(board, eventData.position, eventData.pressEventCamera, out Vector2 point);
            ((RectTransform)card.transform).anchoredPosition = point;
            UpdateHoverHighlight(eventData);
        }

        private void HandleDragEnded(DragDropCard card, PointerEventData eventData)
        {
            if (eventData.pointerId != activePointer) return;
            activePointer = int.MinValue;
            ClearHoverHighlight();
            string slotId = FindSlot(eventData);
            AnimalDragDropOutcome outcome = state.Drop(card.TokenId, slotId, slotId != null);
            if (outcome == AnimalDragDropOutcome.Correct)
            {
                StartCoroutine(ResolveCorrect(card, slotId));
            }
            else if (outcome == AnimalDragDropOutcome.Incorrect)
            {
                resolutionSequence = StartCoroutine(ResolveIncorrect(card, slotId));
            }
            else
            {
                card.Restore();
            }
        }

        private void UpdateHoverHighlight(PointerEventData eventData)
        {
            string hovered = FindSlot(eventData);
            if (hovered == hoveredSlotId) return;
            ClearHoverHighlight();
            hoveredSlotId = hovered;
            if (hoveredSlotId != null && slotSurfaces.TryGetValue(hoveredSlotId, out RoundedSurface surface))
            {
                surface.color = HoverOrange;
                surface.OutlineThickness = 7f;
            }
        }

        private void ClearHoverHighlight()
        {
            if (hoveredSlotId != null && slotSurfaces.TryGetValue(hoveredSlotId, out RoundedSurface surface))
            {
                // Only clear if not already marked success/error.
                if (surface.color != Success && surface.color != Error)
                {
                    surface.color = SlotBaseColor;
                    surface.OutlineThickness = 7f;
                }
            }
            hoveredSlotId = null;
            // Also clear any hover orange that might remain on non-hovered slots.
            foreach (KeyValuePair<string, RoundedSurface> kv in slotSurfaces)
            {
                if (kv.Key == hoveredSlotId) continue;
                if (kv.Value.color == HoverOrange)
                {
                    kv.Value.color = SlotBaseColor;
                }
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
            slotSurfaces[slotId].color = Success;
            yield return CardAnimator.PunchPlace((RectTransform)card.transform);
            if (successSfx) audio?.PlaySfx(successSfx);
            PlayRandom(compliments);
            if (state.Phase == AnimalDragPhase.Celebrating) yield return Celebrate();
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
            services?.GameLauncher.Complete(new MiniGameResult("animal.drag", MiniGameCompletionState.Completed, state.Score, 1, 1, services.Session.SelectedDifficultyId));
            yield return new WaitForSecondsRealtime(.35f);
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
        private void ToggleInstruction() { if (state.Phase != AnimalDragPhase.Ready) return; if (audio != null && audio.IsVoicePlaying(instruction)) audio.StopVoiceIfPlaying(instruction); else PlayInstruction(); }
        private void PlayRandom(AudioClip[] clips) { if (clips != null && clips.Length > 0) audio?.PlayVoice(clips[Random.Range(0, clips.Length)]); }
        private static AudioClip[] CopyClips(System.Collections.Generic.IReadOnlyList<AudioClip> clips) { AudioClip[] copy = new AudioClip[clips.Count]; for (int i = 0; i < copy.Length; i++) copy[i] = clips[i]; return copy; }
        private IEnumerator AnimateHong() { int[] frames = { 1, 2, 3, 2, 1 }; int index = 0; while (true) { bool playing = audio != null && audio.IsVoicePlaying(instruction); if (hongImage) hongImage.sprite = playing ? (frames[index++ % frames.Length] == 1 ? hong1 : frames[(index - 1 + frames.Length) % frames.Length] == 2 ? hong2 : hong3) : hongNeutral; yield return new WaitForSecondsRealtime(.18f); } }
        private void Update() { if (state.AcceptFinalInput() && (Input.GetMouseButtonDown(0) || Input.touchCount > 0)) ReturnToLobby(); }
        private void ReturnToLobby() { if (state.Phase == AnimalDragPhase.ResolvingIncorrect || state.Phase == AnimalDragPhase.Celebrating || (state.Phase == AnimalDragPhase.Final && !state.FinalInputEnabled)) return; audio?.StopMusic(); services?.GameLauncher.ShowLobby(); }
        private static void Pixel(RectTransform rect, Vector2 topOriginCenter, Vector2 size) { rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = LevelChromeLayout.ToAnchoredPosition(topOriginCenter); rect.sizeDelta = size; }

        private void OnDisable()
        {
            if (resolutionSequence != null) StopCoroutine(resolutionSequence);
            if (hongPlayback != null) StopCoroutine(hongPlayback);
            hongPlayback = null;
            activePointer = int.MinValue;
            ClearHoverHighlight();
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
