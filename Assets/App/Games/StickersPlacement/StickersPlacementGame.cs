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

namespace Lbs.MiniGames.Games.StickersPlacement
{
    public sealed class StickersPlacementGame : MonoBehaviour, IAppScene, ILevelTransitionParticipant
    {
        private static readonly Color Background = new(0.969f, 0.961f, 0.98f);
        private static readonly Color SlotBaseColor = Color.clear;
        private static readonly Color HoverOrange = new(1f, 0.7176f, 0.251f); // #FFB740
        private static readonly Color ConfirmGreen = new(0.23f, 0.68f, 0.32f);
        private static readonly Color ConfirmDisabled = new(0.75f, 0.75f, 0.75f);
        private static readonly Color ConfirmShadowGreen = new(0.13f, 0.45f, 0.20f);
        private static readonly Color ConfirmShadowDisabled = new(0.55f, 0.55f, 0.55f);
        private static readonly Color ConfirmInk = Color.white;

        // Board (1254x1254 incl. mosaic + dashed slots) top-center.
        private static readonly Vector2 BoardCenter = new(960, 400);
        private static readonly Vector2 BoardSize = new(800, 800);

        // Dashed slots measured on the 1254px board (slot2 purple is pre-placed).
        private static readonly Vector2 Slot1Center = new(681, 657);
        private static readonly Vector2 Slot3Center = new(1058, 657);
        private static readonly Vector2 Slot4Center = new(1249, 657);
        private static readonly Vector2 SlotSize = new(190, 190);

        // Sticker tray below the board.
        private static readonly Vector2 YellowHome = new(620, 930);
        private static readonly Vector2 PinkHome = new(960, 930);
        private static readonly Vector2 BlueHome = new(1300, 930);
        private static readonly Vector2 PieceSize = new(160, 160);

        private static readonly Vector2 ConfirmCenter = new(1720, 880);
        private static readonly Vector2 ConfirmSize = new(170, 250);

        [SerializeField] private Sprite boardSprite;
        [SerializeField] private Sprite stickerYellow;
        [SerializeField] private Sprite stickerPink;
        [SerializeField] private Sprite stickerBlue;
        [SerializeField] private Sprite exitIcon, hongNeutral, hong1, hong2, hong3, finalStar;
        [SerializeField] private Sprite celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3;
        [SerializeField] private AudioClip instruction, successSfx, failSfx;
        [SerializeField] private AudioClip[] compliments, encouragements;
        [SerializeField] private SharedAudioLibrary sharedLibrary;
        [SerializeField] private Font font, scoreFont;
        [SerializeField] private FinalCelebrationConfiguration celebrationConfiguration;

        private readonly StickersPlacementState state = new();
        private readonly List<DragDropCard> pieces = new();
        private readonly Dictionary<string, RectTransform> slots = new();
        private readonly Dictionary<string, RoundedSurface> slotSurfaces = new();
        private readonly Dictionary<string, Vector2> trayHomes = new();
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
        private string draggedFromSlot;
        private Button confirmButton;
        private RoundedSurface confirmSurface;
        private RoundedSurface confirmShadow;
        private bool confirmReady;

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

            board = new GameObject("StickersPlacementBoard", typeof(RectTransform)).GetComponent<RectTransform>();
            board.SetParent(canvas.transform, false);
            UiFactory.Stretch(board, 0);
            UiFactory.Stretch(UiFactory.CreateImage(board, "Background", Background).rectTransform, 0);
            levelChrome = LevelChromeFactory.Build(board, font, exitIcon, hongNeutral, ReturnToLobby, ToggleInstruction);
            hongImage = levelChrome.HongImage;

            CreateBoardBackground();
            CreateSlot(StickersPlacementRule.Slot1, Slot1Center);
            CreateSlot(StickersPlacementRule.Slot3, Slot3Center);
            CreateSlot(StickersPlacementRule.Slot4, Slot4Center);
            CreatePiece(StickersPlacementRule.YellowSticker, stickerYellow, YellowHome);
            CreatePiece(StickersPlacementRule.PinkSticker, stickerPink, PinkHome);
            CreatePiece(StickersPlacementRule.BlueSticker, stickerBlue, BlueHome);
            CreateConfirmButton();
            EnsureScoreFont();
        }

        private void CreateBoardBackground()
        {
            RoundedSurface bgSurface = UiFactory.CreateRoundedSurface(board, "BoardBackground", Color.clear, 34f);
            Pixel(bgSurface.rectTransform, BoardCenter, BoardSize);
            bgSurface.OutlineThickness = 0f;
            bgSurface.raycastTarget = false;
            Image bgImage = UiFactory.CreateImage(bgSurface.rectTransform, "Board", Color.white);
            bgImage.sprite = boardSprite;
            bgImage.preserveAspect = true;
            UiFactory.Stretch(bgImage.rectTransform, 0);
            bgImage.raycastTarget = false;
        }

        private void CreateSlot(string id, Vector2 center)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, id, SlotBaseColor, 30f);
            Pixel(surface.rectTransform, center, SlotSize);
            surface.OutlineThickness = 7f;
            surface.raycastTarget = false;
            slots.Add(id, surface.rectTransform);
            slotSurfaces.Add(id, surface);
        }

        private void CreatePiece(string id, Sprite artwork, Vector2 center)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, id + "Piece", Color.clear, 30f);
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
            trayHomes.Add(id, surface.rectTransform.anchoredPosition);
        }

        private void CreateConfirmButton()
        {
            confirmShadow = UiFactory.CreateRoundedSurface(board, "ConfirmShadow", ConfirmShadowDisabled, 40f);
            Pixel(confirmShadow.rectTransform, ConfirmCenter + new Vector2(0f, 12f), ConfirmSize);
            confirmShadow.raycastTarget = false;
            confirmSurface = UiFactory.CreateRoundedSurface(board, "ConfirmButton", ConfirmDisabled, 40f);
            Pixel(confirmSurface.rectTransform, ConfirmCenter, ConfirmSize);
            CreateArrowBar(new Vector2(0f, 0f), new Vector2(76f, 16f), 0f);
            CreateArrowBar(new Vector2(23f, 13f), new Vector2(44f, 16f), -45f);
            CreateArrowBar(new Vector2(23f, -13f), new Vector2(44f, 16f), 45f);
            confirmButton = confirmSurface.gameObject.AddComponent<Button>();
            confirmButton.targetGraphic = confirmSurface;
            confirmButton.interactable = false;
            confirmButton.onClick.AddListener(OnConfirmPressed);
        }

        private void CreateArrowBar(Vector2 offset, Vector2 size, float angle)
        {
            Image bar = UiFactory.CreateImage(confirmSurface.rectTransform, "Arrow", ConfirmInk);
            bar.raycastTarget = false;
            RectTransform rect = bar.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = offset;
            rect.sizeDelta = size;
            rect.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void HandleDragStarted(DragDropCard card, PointerEventData eventData)
        {
            if (state.Phase != StickersPlacementPhase.Ready || activePointer != int.MinValue) return;
            activePointer = eventData.pointerId;
            StopInstruction();
            draggedFromSlot = state.SlotOf(card.TokenId);
            if (draggedFromSlot != null) state.Remove(card.TokenId);
            UpdateConfirm();
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
            if (slotId == null)
            {
                card.Restore();
                if (draggedFromSlot != null) state.Place(card.TokenId, draggedFromSlot);
            }
            else
            {
                string occupant = state.OccupantOf(slotId);
                if (occupant != null && occupant != card.TokenId) SwapOccupant(occupant, draggedFromSlot);
                PlaceOnSlot(card, slotId);
            }
            draggedFromSlot = null;
            UpdateConfirm();
        }

        private void SwapOccupant(string occupantToken, string freedSpot)
        {
            DragDropCard occupant = pieces.Find(p => p && p.TokenId == occupantToken);
            if (occupant == null) return;
            Vector2 dest = freedSpot != null && slots.TryGetValue(freedSpot, out RectTransform slotRect)
                ? slotRect.anchoredPosition
                : trayHomes[occupantToken];
            ((RectTransform)occupant.transform).anchoredPosition = dest;
            occupant.UpdateOrigin(dest);
            ResetCardAlpha(occupant);
            if (freedSpot != null) state.Place(occupantToken, freedSpot);
        }

        private void PlaceOnSlot(DragDropCard card, string slotId)
        {
            ((RectTransform)card.transform).anchoredPosition = slots[slotId].anchoredPosition;
            card.UpdateOrigin(slots[slotId].anchoredPosition);
            ResetCardAlpha(card);
            if (slotSurfaces.TryGetValue(slotId, out RoundedSurface surface)) surface.color = SlotBaseColor;
            state.Place(card.TokenId, slotId);
        }

        private static void ResetCardAlpha(DragDropCard card)
        {
            CanvasGroup group = card ? card.GetComponent<CanvasGroup>() : null;
            if (group == null) return;
            group.alpha = 1f;
            group.blocksRaycasts = true;
        }

        private void ResetAllToTray()
        {
            foreach (DragDropCard piece in pieces)
            {
                if (!piece || !trayHomes.TryGetValue(piece.TokenId, out Vector2 home)) continue;
                ((RectTransform)piece.transform).anchoredPosition = home;
                piece.UpdateOrigin(home);
                ResetCardAlpha(piece);
            }
            state.ClearPlacements();
        }

        private void UpdateConfirm()
        {
            bool ready = state.Phase == StickersPlacementPhase.Ready && state.AllFilled;
            if (ready && !confirmReady)
            {
                confirmReady = true;
                confirmSurface.color = ConfirmGreen;
                confirmShadow.color = ConfirmShadowGreen;
                confirmButton.interactable = true;
                StartCoroutine(CardAnimator.PunchPlace(confirmSurface.rectTransform));
            }
            else if (!ready && confirmReady)
            {
                confirmReady = false;
                confirmSurface.color = ConfirmDisabled;
                confirmShadow.color = ConfirmShadowDisabled;
                confirmButton.interactable = false;
            }
        }

        private void OnConfirmPressed()
        {
            if (state.Phase != StickersPlacementPhase.Ready || !state.AllFilled) return;
            if (!state.Confirm()) return;
            StopInstruction();
            SetInteractable(false);
            confirmButton.interactable = false;
            resolutionSequence = StartCoroutine(ResolveConfirm());
        }

        private IEnumerator ResolveConfirm()
        {
            yield return CardAnimator.PunchPlace(confirmSurface.rectTransform);
            if (state.ResolveConfirm())
            {
                if (successSfx) audio?.PlaySfx(successSfx);
                PlayRandom(compliments);
                yield return Celebrate();
            }
            else
            {
                if (failSfx) audio?.PlaySfx(failSfx);
                PlayRandom(encouragements);
                yield return CardAnimator.ShakeBoard(board);
                ResetAllToTray();
                state.FinishIncorrect();
                SetInteractable(true);
                UpdateConfirm();
            }
            resolutionSequence = null;
        }

        private IEnumerator Celebrate()
        {
            state.FinishCorrect();
            CreateCelebration();
            yield return new WaitForSecondsRealtime(celebrationPresenter.PresentationDelay);
            celebrationPresenter.ShowFinal(CelebrationInput());
            state.FinishCelebration();
            string resultGameId = services?.Session?.CurrentRequest?.Game?.GameId ?? "stickers.placement";
            string difficultyId = services?.Session?.SelectedDifficultyId;
            services?.GameLauncher.Complete(new MiniGameResult(resultGameId, MiniGameCompletionState.Completed, state.Score, 1, 1, difficultyId));
            yield return new WaitForSecondsRealtime(2f);
            services?.LevelSequence?.Advance(LevelSequenceRoute.StickersPlacementSuccessTarget);
            state.EnableFinalInput();
        }

        private string FindSlot(PointerEventData eventData)
        {
            foreach (KeyValuePair<string, RectTransform> slot in slots)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(slot.Value, eventData.position, eventData.pressEventCamera)) return slot.Key;
            }
            return null;
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
                if (surface.color == HoverOrange)
                {
                    surface.color = SlotBaseColor;
                    surface.OutlineThickness = 7f;
                }
            }
            hoveredSlotId = null;
            foreach (KeyValuePair<string, RoundedSurface> kv in slotSurfaces)
            {
                if (kv.Key == hoveredSlotId) continue;
                if (kv.Value.color == HoverOrange) kv.Value.color = SlotBaseColor;
            }
        }

        private void SetInteractable(bool value)
        {
            foreach (DragDropCard piece in pieces)
            {
                if (!piece) continue;
                CanvasGroup group = piece.GetComponent<CanvasGroup>();
                if (group) group.blocksRaycasts = value;
            }
            if (levelChrome == null) return;
            if (levelChrome.ExitButton != null) levelChrome.ExitButton.interactable = value;
            if (levelChrome.HongButton != null) levelChrome.HongButton.interactable = value;
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
        private void ToggleInstruction() { if (state.Phase != StickersPlacementPhase.Ready) return; if (audio != null && audio.IsVoicePlaying(instruction)) audio.StopVoiceIfPlaying(instruction); else PlayInstruction(); }
        private void PlayRandom(AudioClip[] clips) { if (clips != null && clips.Length > 0) audio?.PlayVoice(clips[Random.Range(0, clips.Length)]); }
        private static AudioClip[] CopyClips(System.Collections.Generic.IReadOnlyList<AudioClip> clips) { AudioClip[] copy = new AudioClip[clips.Count]; for (int i = 0; i < copy.Length; i++) copy[i] = clips[i]; return copy; }
        private IEnumerator AnimateHong() { int[] frames = { 1, 2, 3, 2, 1 }; int index = 0; while (true) { bool playing = audio != null && audio.IsVoicePlaying(instruction); if (hongImage) hongImage.sprite = playing ? (frames[index++ % frames.Length] == 1 ? hong1 : frames[(index - 1 + frames.Length) % frames.Length] == 2 ? hong2 : hong3) : hongNeutral; yield return new WaitForSecondsRealtime(.18f); } }
        private void Update() { if (state.AcceptFinalInput() && (Input.GetMouseButtonDown(0) || Input.touchCount > 0)) ReturnToLobby(); }
        private void ReturnToLobby() { if (state.Phase == StickersPlacementPhase.Resolving || state.Phase == StickersPlacementPhase.Celebrating || (state.Phase == StickersPlacementPhase.Final && !state.IsFinalInputEnabled)) return; audio?.StopMusic(); services?.GameLauncher.ShowLobby(); }
        private static void Pixel(RectTransform rect, Vector2 topOriginCenter, Vector2 size) { rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = LevelChromeLayout.ToAnchoredPosition(topOriginCenter); rect.sizeDelta = size; }

        private void OnDisable()
        {
            if (resolutionSequence != null) StopCoroutine(resolutionSequence);
            if (hongPlayback != null) StopCoroutine(hongPlayback);
            hongPlayback = null;
            activePointer = int.MinValue;
            ClearHoverHighlight();
            audio?.StopVoiceIfPlaying(instruction);
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
            if (confirmButton) confirmButton.onClick.RemoveListener(OnConfirmPressed);
        }
    }
}
