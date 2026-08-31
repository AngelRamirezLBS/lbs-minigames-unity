using System;
using System.Collections;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.GameKits.DragDrop;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using Lbs.MiniGames.Shared.Audio;
using Lbs.MiniGames.Shared.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.ShapeAnalogy
{
    public sealed class ShapeAnalogyGame : MonoBehaviour, IAppScene
    {
        private static readonly Color Background = new(1f, .886f, .678f);
        private static readonly Color Orange = new(1f, .38f, .08f);
        private static readonly Color Purple = new(.58f, .28f, .96f);
        private static readonly Color SuccessGreen = new(0.09f, 0.48f, 0.29f);
        private static readonly Color Dark = new(.14f, .10f, .21f);
        [SerializeField] private Sprite starEmpty, starFull, heartEmpty, heartFull, missing, finalStar, exitIcon, hongNeutral, hong1, hong2, hong3;
        [SerializeField] private Sprite celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina;
        [SerializeField] private Sprite serpentina2, serpentina3;
        [SerializeField] private AudioClip instruction, tryAgain;
        [SerializeField] private AudioClip bgMusic; // ShapeAnalogy level music routed through the shared audio service
        [SerializeField] private AudioClip successSfx;
        [SerializeField] private AudioClip failSfx;
        [SerializeField] private AudioClip[] compliments;
        [SerializeField] private AudioClip[] encouragements;
        [SerializeField] private SharedAudioLibrary sharedLibrary; // optional immutable library; fallback to serialized arrays
        [SerializeField] private Font font;
        [SerializeField] private Font scoreFont;
        private AppServices services;
        private IAppAudioService appAudio;
        private AudioSource audioSource; // level-scoped voice for instruction (facade separate from SFX)
        private AudioSource sfxSource; // dedicated SFX source (PlayOneShot) separate from voice
        private AudioSource voiceSource; // for compliments/encouragements
        private RectTransform target;
        private ProximityHighlighter proximityHighlighter;
        private RoundedSurface proximity;
        private Image hongImage;
        private Image missingImage;
        private Image resultBackdropDim;
        private RectTransform board;
        private LevelChrome levelChrome;
        private readonly ShapeAnalogyState state = new();
        private Coroutine playback;
        private Coroutine resultBackdropFade;
        private int activePointer = int.MinValue;
        private bool wasPausedByFocus;
        private readonly System.Collections.Generic.List<DragDropCard> draggables = new();
        private const float CardReferenceSize = 295f;
        private const float ProximityFrameSize = 315f;
        private static readonly Vector2 RelationshipCenter = new(960, 550);
        private static readonly Vector2[] FixedCardCenters = { new(910, 235), new(1150, 235), new(910, 475), new(1150, 475) };
        private readonly Vector2[] origins = { new(750, 855), new(1020, 855), new(1290, 855) };
        private readonly System.Collections.Generic.List<GameObject> celebrationObjects = new();

        public void Configure(AppServices appServices)
        {
            services = appServices;
            appAudio = appServices != null ? appServices.Audio : null;
            Build();
            // ShapeAnalogy owns this music while the level is active; the shared service provides playback and volume handling.
            if (appAudio != null && bgMusic != null)
            {
                appAudio.PlayMusic(bgMusic, true, 0.25f);
            }
            else if (appAudio == null && bgMusic != null)
            {
                // Transitional fallback when injected service absent (e.g., editor tests without bootstrap)
                Debug.LogWarning("[ShapeAnalogy] IAppAudioService not injected — level music will not play.", this);
            }
            PlayInstruction();
            playback = StartCoroutine(HongPlayback());
        }

        private void Build()
        {
            Canvas canvas = GetComponentInParent<Canvas>(); if (!canvas) return; RectTransform root = canvas.GetComponent<RectTransform>(); board = GetOrCreateBoard(root);
            Image bg = UiFactory.CreateImage(board, "WarmYellowBackground", Background); UiFactory.Stretch(bg.rectTransform, 0);
            // Reusable level chrome centralizes the approved Exit and Hong coordinates.
            levelChrome = LevelChromeFactory.Build(board, font, exitIcon, hongNeutral, ReturnToLobby, ToggleInstruction);
            // Preserve reference to Hong image for frame animation (factory owns button, we keep image)
            if (levelChrome != null) hongImage = levelChrome.HongImage;
            CreateCard(board, "GivenStar", starFull, FixedCardCenters[0], Vector2.one * CardReferenceSize, false); CreateCard(board, "GivenHeart", heartFull, FixedCardCenters[1], Vector2.one * CardReferenceSize, false);
            CreateCard(board, "PatternStar", starEmpty, FixedCardCenters[2], Vector2.one * CardReferenceSize, false);
            RoundedSurface slot = UiFactory.CreateRoundedSurface(board, "MissingSlot", Color.clear, 28); PixelRect(slot.rectTransform, FixedCardCenters[3], Vector2.one * CardReferenceSize);
            missingImage = UiFactory.CreateImage(slot.rectTransform, "DottedPlaceholder", Color.white); missingImage.sprite = missing; missingImage.preserveAspect = true; AddArtworkShadow(missingImage, 3f, .18f); UiFactory.Stretch(missingImage.rectTransform, 0);
            target = slot.rectTransform;
            // Proximity highlighter kit component preserving orange interpolation
            proximity = UiFactory.CreateRoundedSurface(board, "OrangeProximityFrame", new Color(1f, .38f, .08f, 0f), 28f, false); proximity.OutlineThickness = 8f; proximity.raycastTarget = false; PixelRect(proximity.rectTransform, FixedCardCenters[3], Vector2.one * ProximityFrameSize);
            CanvasGroup proximityCg = proximity.gameObject.AddComponent<CanvasGroup>(); proximityCg.alpha = 0f; proximityCg.blocksRaycasts = false; proximityCg.interactable = false;
            proximityHighlighter = proximity.gameObject.AddComponent<ProximityHighlighter>();
            proximity.gameObject.SetActive(false);
            CreateDraggable(board, "HeartAnswer", heartFull, "filled-heart", origins[0]); CreateDraggable(board, "StarAnswer", starFull, "filled-star", origins[1]); CreateDraggable(board, "CorrectAnswer", heartEmpty, ShapeAnalogyRule.CorrectAnswer, origins[2]);
            if (!FindAnyObjectByType<AudioListener>()) gameObject.AddComponent<AudioListener>();
            // Level-scoped voice source for instruction (separate from SFX)
            audioSource = GetComponent<AudioSource>(); if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>(); audioSource.playOnAwake = false; audioSource.spatialBlend = 0; audioSource.mute = false; audioSource.volume = 1;
            if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>(); sfxSource.loop = false; sfxSource.playOnAwake = false; sfxSource.spatialBlend = 0; sfxSource.mute = false; sfxSource.volume = 1f;
            if (!voiceSource) voiceSource = gameObject.AddComponent<AudioSource>(); voiceSource.loop = false; voiceSource.playOnAwake = false; voiceSource.spatialBlend = 0; voiceSource.volume = 1f; voiceSource.mute = false;
            EnsureAudioFallbacks();
            EnsureFontFallbacks();
        }

        private void EnsureFontFallbacks()
        {
            if (scoreFont) return;
            scoreFont = Resources.Load<Font>("Fonts/Nunito-Black");
            if (!scoreFont) scoreFont = Resources.Load<Font>("Nunito-Black");
            if (!scoreFont) scoreFont = font;
        }

        private void EnsureAudioFallbacks()
        {
            if (!bgMusic)
            {
                bgMusic = Resources.Load<AudioClip>("ShapeAnalogy/Music/bg_cabinet_menu");
                if (!bgMusic) Debug.LogWarning("[ShapeAnalogy] Audio fallback not found: ShapeAnalogy/Music/bg_cabinet_menu");
            }
            if (!successSfx)
            {
                if (sharedLibrary != null && sharedLibrary.SuccessClip != null) successSfx = sharedLibrary.SuccessClip;
                else successSfx = Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_success_true_answer");
                if (!successSfx) Debug.LogWarning("[ShapeAnalogy] Audio fallback not found: ShapeAnalogy/SFX/sfx_success_true_answer");
            }
            if (!failSfx)
            {
                if (sharedLibrary != null && sharedLibrary.FailClip != null) failSfx = sharedLibrary.FailClip;
                else failSfx = Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_fail_incorrect_answer");
                if (!failSfx) Debug.LogWarning("[ShapeAnalogy] Audio fallback not found: ShapeAnalogy/SFX/sfx_fail_incorrect_answer");
            }
            if (compliments == null || compliments.Length == 0)
            {
                if (sharedLibrary != null && sharedLibrary.Compliments.Count > 0)
                {
                    compliments = new AudioClip[sharedLibrary.Compliments.Count];
                    for (int i = 0; i < compliments.Length; i++) compliments[i] = sharedLibrary.Compliments[i];
                }
                else
                {
                    compliments = Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Compliments/en");
                    if (compliments.Length == 0) compliments = Resources.LoadAll<AudioClip>("ShapeAnalogy/Compliments/en");
                    if (compliments == null || compliments.Length == 0) Debug.LogWarning("[ShapeAnalogy] Audio fallback not found: ShapeAnalogy/Voice/Compliments/en");
                }
            }
            if (encouragements == null || encouragements.Length == 0)
            {
                if (sharedLibrary != null && sharedLibrary.Encouragements.Count > 0)
                {
                    encouragements = new AudioClip[sharedLibrary.Encouragements.Count];
                    for (int i = 0; i < encouragements.Length; i++) encouragements[i] = sharedLibrary.Encouragements[i];
                }
                else
                {
                    encouragements = Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Encouragement/en");
                    if (encouragements.Length == 0) encouragements = Resources.LoadAll<AudioClip>("ShapeAnalogy/Encouragement/en");
                    if (encouragements == null || encouragements.Length == 0) Debug.LogWarning("[ShapeAnalogy] Audio fallback not found: ShapeAnalogy/Voice/Encouragement/en");
                }
            }
        }

        private RectTransform GetOrCreateBoard(RectTransform root)
        {
            Transform existing = root.Find("ShapeAnalogyBoard");
            if (existing) return (RectTransform)existing;
            GameObject boardObject = new("ShapeAnalogyBoard", typeof(RectTransform));
            RectTransform boardRoot = boardObject.GetComponent<RectTransform>();
            boardRoot.SetParent(root, false);
            UiFactory.Stretch(boardRoot, 0);
            return boardRoot;
        }

        private void PixelRect(RectTransform rect, Vector2 topOriginCenter, Vector2 size) { rect.anchorMin=rect.anchorMax=new(.5f,.5f); rect.pivot=new(.5f,.5f); rect.anchoredPosition=LevelChromeLayout.ToAnchoredPosition(topOriginCenter); rect.sizeDelta=size; }
        private static void AddArtworkShadow(Image image, float offset, float alpha) { Shadow shadow=image.gameObject.AddComponent<Shadow>(); shadow.effectColor=new Color(0f,0f,0f,alpha); shadow.effectDistance=new Vector2(offset,-offset); shadow.useGraphicAlpha=true; }
        private DragDropCard CreateDraggable(RectTransform root, string name, Sprite sprite, string id, Vector2 pos)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(root, name, Color.clear, 28); PixelRect(surface.rectTransform, pos, Vector2.one * CardReferenceSize);
            CanvasGroup group = surface.gameObject.AddComponent<CanvasGroup>();
            Image image = UiFactory.CreateImage(surface.rectTransform,"Artwork",Color.white); image.sprite=sprite; image.preserveAspect=true; AddArtworkShadow(image, 3f, .18f); UiFactory.Stretch(image.rectTransform,0);
            DragDropCard card = surface.gameObject.AddComponent<DragDropCard>(); card.SetTokenId(id); card.Setup(id, group, surface.rectTransform.anchoredPosition);
            // Wire kit card events to owner with one-pointer policy
            card.DragStarted += HandleDragStarted;
            card.DragMoved += HandleDragMoved;
            card.DragEnded += HandleDragEnded;
            draggables.Add(card); return card;
        }
        private void CreateCard(RectTransform root,string name,Sprite sprite,Vector2 center,Vector2 size,bool drag) { RoundedSurface surface=UiFactory.CreateRoundedSurface(root,name,Color.clear,28); PixelRect(surface.rectTransform,center,size); Image image=UiFactory.CreateImage(surface.rectTransform,"Artwork",Color.white); image.sprite=sprite; image.preserveAspect=true; AddArtworkShadow(image, 3f, .18f); UiFactory.Stretch(image.rectTransform,0); }

        private void HandleDragStarted(DragDropCard card, PointerEventData e) => Begin(card, e);
        private void HandleDragMoved(DragDropCard card, PointerEventData e) => Move(card, e);
        private void HandleDragEnded(DragDropCard card, PointerEventData e) => End(card, e);

        private void Begin(DragDropCard card, PointerEventData e) { if (activePointer != int.MinValue) return; state.StartDrag(); if (state.Phase != ShapeAnalogyPhase.Dragging) return; activePointer=e.pointerId; card.Lift(); }
        private void Move(DragDropCard card, PointerEventData e)
        {
            if (e.pointerId != activePointer) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(board, e.position, e.pressEventCamera, out Vector2 p);
            ((RectTransform)card.transform).anchoredPosition = p;
            float dist = Vector2.Distance(((RectTransform)card.transform).anchoredPosition, target.anchoredPosition);
            if (proximityHighlighter != null) proximityHighlighter.ShowForDistance(dist);
            else
            {
                // Fallback if highlighter missing (should not happen)
                const float maxDist = 350f; float t = 1f - Mathf.Clamp01(dist / maxDist); CanvasGroup cg = proximity.GetComponent<CanvasGroup>(); bool show = t > 0.02f; proximity.gameObject.SetActive(show);
                if (show) { if (cg) cg.alpha = Mathf.Lerp(0.35f, 1f, t); Color c = proximity.color; c.r = Orange.r; c.g = Orange.g; c.b = Orange.b; c.a = Mathf.Lerp(0.45f, 1f, t); proximity.color = c; proximity.OutlineThickness = Mathf.Lerp(8f, 3f, t); proximity.rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.02f, t); }
            }
        }
        private void End(DragDropCard card, PointerEventData e)
        {
            if (e.pointerId != activePointer) return;
            activePointer = int.MinValue;
            bool over = RectTransformUtility.RectangleContainsScreenPoint(target, e.position, e.pressEventCamera);
            if (proximityHighlighter != null) proximityHighlighter.HideImmediate();
            else if (proximity)
            {
                CanvasGroup cgHide = proximity.GetComponent<CanvasGroup>(); if (cgHide) cgHide.alpha = 0f; Color pc = proximity.color; pc.a = 0f; proximity.color = pc; proximity.OutlineThickness = 8f; proximity.rectTransform.localScale = Vector3.one; proximity.gameObject.SetActive(false);
            }
            ShapeAnalogyDropOutcome outcome = state.Drop(card.TokenId, over);
            if (outcome == ShapeAnalogyDropOutcome.Correct)
            {
                StartCoroutine(CorrectPlaceSequence(card));
            }
            else if (outcome == ShapeAnalogyDropOutcome.Incorrect)
            {
                // SFX via dedicated source or global service (does not duck music)
                if (appAudio != null && failSfx != null) appAudio.PlaySfx(failSfx);
                else if (failSfx && sfxSource) sfxSource.PlayOneShot(failSfx);
                if (failSfx) { if (encouragements != null && encouragements.Length > 0) StartCoroutine(PlayRandomEncouragement()); else if (tryAgain) StartCoroutine(PlayTryAgainDelayed()); }
                else if (tryAgain) { if (appAudio != null) appAudio.PlayVoice(tryAgain); else if (sfxSource) sfxSource.PlayOneShot(tryAgain); }
                StartCoroutine(ResolveIncorrect(card));
            }
            else card.Restore();
        }

        private IEnumerator CorrectPlaceSequence(DragDropCard card)
        {
            RectTransform rect = (RectTransform)card.transform;
            rect.anchoredPosition = target.anchoredPosition;
            HideMissingArtwork();
            yield return CardAnimator.PunchPlace(rect);
            CanvasGroup group = card.GetComponent<CanvasGroup>();
            if (group) { group.alpha = 1f; group.blocksRaycasts = true; }
            rect.localScale = Vector3.one;
            rect.anchoredPosition = target.anchoredPosition;
            yield return new WaitForSecondsRealtime(0.5f);
            if (successSfx)
            {
                if (appAudio != null) appAudio.PlaySfx(successSfx);
                else if (sfxSource) sfxSource.PlayOneShot(successSfx);
                else PlaySuccess();
            }
            else PlaySuccess();
            StartCoroutine(PlayRandomCompliment());
            StartCoroutine(Celebrate());
        }

        private void HideMissingArtwork() { if (missingImage) missingImage.gameObject.SetActive(false); }
        private IEnumerator ResolveIncorrect(DragDropCard card){ yield return CardAnimator.ShakeBoard(board); foreach(var c in draggables) c.Restore(); state.FinishResolve(); }
        private IEnumerator Celebrate(){CreateCelebration(); yield return new WaitForSecondsRealtime(1f); CreateFinal(); state.FinishCelebration(); yield return new WaitForSecondsRealtime(1f); state.ArmFinal();}
        private void CreateCelebration()
        {
            ClearCelebrationVisuals();
            resultBackdropDim = UiFactory.CreateImage(board, "ResultBackdropDim", new Color(.15f, .08f, .28f, 0f));
            resultBackdropDim.raycastTarget = false;
            UiFactory.Stretch(resultBackdropDim.rectTransform, 0);
            resultBackdropDim.transform.SetAsLastSibling();

            GameObject rootObject = new("ResultCelebration", typeof(RectTransform), typeof(ShapeAnalogyCelebrationParticles));
            RectTransform root = rootObject.GetComponent<RectTransform>();
            root.SetParent(board, false);
            UiFactory.Stretch(root, 0);
            rootObject.GetComponent<ShapeAnalogyCelebrationParticles>().Initialize(celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3);
            celebrationObjects.Add(rootObject);
            root.transform.SetAsLastSibling();
            StartResultBackdropFade();
        }
        private void CreateFinal()
        {
            Vector2 groupCenter = new Vector2(965f, 550f);
            GameObject haloBlurObject = new GameObject("FinalHaloBlur", typeof(RectTransform), typeof(CanvasRenderer), typeof(EllipseSurface));
            EllipseSurface haloBlur = haloBlurObject.GetComponent<EllipseSurface>();
            haloBlur.color = new Color(0.22f, 0.70f, 0.45f, 0.06f);
            haloBlur.raycastTarget = false;
            haloBlur.transform.SetParent(board, false);
            PixelRect(haloBlur.rectTransform, groupCenter, new Vector2(600, 220));
            GameObject haloObject = new GameObject("FinalHalo", typeof(RectTransform), typeof(CanvasRenderer), typeof(EllipseSurface));
            EllipseSurface halo = haloObject.GetComponent<EllipseSurface>();
            halo.color = new Color(0.22f, 0.70f, 0.45f, 0.12f);
            halo.raycastTarget = false;
            halo.transform.SetParent(board, false);
            PixelRect(halo.rectTransform, groupCenter, new Vector2(520, 180));
            Shadow haloShadow = halo.gameObject.AddComponent<Shadow>();
            haloShadow.effectColor = new Color(0.18f, 0.62f, 0.40f, 0.14f);
            haloShadow.effectDistance = new Vector2(0f, 0f);
            haloShadow.useGraphicAlpha = true;
            Shadow haloShadow2 = halo.gameObject.AddComponent<Shadow>();
            haloShadow2.effectColor = new Color(0.18f, 0.62f, 0.40f, 0.08f);
            haloShadow2.effectDistance = new Vector2(2f, -2f);
            haloShadow2.useGraphicAlpha = true;
            Font sFont = scoreFont ? scoreFont : font;
            Text score = UiFactory.CreateText(board, "FinalScore", sFont, 165, TextAnchor.MiddleCenter, Color.white);
            score.text = "+8";
            PixelRect(score.rectTransform, groupCenter + new Vector2(-125f, 3f), new Vector2(200, 200));
            Shadow scoreShadow = score.gameObject.AddComponent<Shadow>();
            scoreShadow.effectColor = new Color(0, 0, 0, 0.22f);
            scoreShadow.effectDistance = new Vector2(3f, -3f);
            scoreShadow.useGraphicAlpha = true;
            CreateCard(board, "FinalStarA", finalStar, groupCenter + new Vector2(78f, -22f), new Vector2(175, 175), false);
            CreateCard(board, "FinalStarB", finalStar, groupCenter + new Vector2(128f, 28f), new Vector2(195, 195), false);
            Transform starA = board.Find("FinalStarA");
            if (starA)
            {
                if (!starA.GetComponent<Shadow>())
                {
                    Shadow sh = starA.gameObject.AddComponent<Shadow>();
                    sh.effectColor = new Color(0, 0, 0, 0.18f);
                    sh.effectDistance = new Vector2(4f, -4f);
                    sh.useGraphicAlpha = true;
                }
                Transform artworkA = starA.Find("Artwork");
                Image imgA = artworkA ? artworkA.GetComponent<Image>() : null;
                Shadow sA = imgA ? imgA.GetComponent<Shadow>() : null;
                if (sA) sA.effectDistance = new Vector2(3f, -3f);
            }
            Transform starB = board.Find("FinalStarB");
            if (starB)
            {
                if (!starB.GetComponent<Shadow>())
                {
                    Shadow sh = starB.gameObject.AddComponent<Shadow>();
                    sh.effectColor = new Color(0, 0, 0, 0.18f);
                    sh.effectDistance = new Vector2(4f, -4f);
                    sh.useGraphicAlpha = true;
                }
                Transform artworkB = starB.Find("Artwork");
                Image imgB = artworkB ? artworkB.GetComponent<Image>() : null;
                Shadow sB = imgB ? imgB.GetComponent<Shadow>() : null;
                if (sB) sB.effectDistance = new Vector2(3f, -3f);
            }
            score.canvasRenderer.SetAlpha(0f);
            score.rectTransform.localScale = Vector3.one * 0.85f;
            StartCoroutine(CardAnimator.FadeScaleIn(score.rectTransform, 0.35f));
            if (starA)
            {
                CanvasGroup cgA = starA.gameObject.GetComponent<CanvasGroup>();
                if (!cgA) cgA = starA.gameObject.AddComponent<CanvasGroup>();
                cgA.alpha = 0f;
                starA.GetComponent<RectTransform>().localScale = Vector3.one * 0.85f;
                StartCoroutine(CardAnimator.FadeScaleIn(starA.GetComponent<RectTransform>(), 0.35f));
            }
            if (starB)
            {
                CanvasGroup cgB = starB.gameObject.GetComponent<CanvasGroup>();
                if (!cgB) cgB = starB.gameObject.AddComponent<CanvasGroup>();
                cgB.alpha = 0f;
                starB.GetComponent<RectTransform>().localScale = Vector3.one * 0.85f;
                StartCoroutine(CardAnimator.FadeScaleIn(starB.GetComponent<RectTransform>(), 0.35f));
            }
            Transform dim = resultBackdropDim ? resultBackdropDim.transform : null;
            Transform celebration = board.Find("ResultCelebration");
            Transform haloBlurTr = board.Find("FinalHaloBlur");
            Transform haloTr = board.Find("FinalHalo");
            Transform scoreTr = board.Find("FinalScore");
            Transform starATr = board.Find("FinalStarA");
            Transform starBTr = board.Find("FinalStarB");
            if (dim && haloBlurTr && haloTr && celebration && scoreTr && starATr && starBTr)
            {
                if (dim.GetSiblingIndex() > haloBlurTr.GetSiblingIndex()) dim.SetSiblingIndex(haloBlurTr.GetSiblingIndex());
                int baseIndex = Mathf.Min(dim.GetSiblingIndex(), haloBlurTr.GetSiblingIndex(), haloTr.GetSiblingIndex(), celebration.GetSiblingIndex(), scoreTr.GetSiblingIndex(), starATr.GetSiblingIndex(), starBTr.GetSiblingIndex());
                dim.SetSiblingIndex(baseIndex);
                haloBlurTr.SetSiblingIndex(baseIndex + 1);
                haloTr.SetSiblingIndex(baseIndex + 2);
                celebration.SetSiblingIndex(baseIndex + 3);
                scoreTr.SetSiblingIndex(baseIndex + 4);
                starATr.SetSiblingIndex(baseIndex + 5);
                starBTr.SetSiblingIndex(baseIndex + 6);
            }
            else if (dim && celebration && dim.GetSiblingIndex() > celebration.GetSiblingIndex())
            {
                dim.SetSiblingIndex(celebration.GetSiblingIndex());
            }
        }
        private void AddStarNumber(RectTransform starRoot, string number, int fontSize)
        {
            if (!starRoot || !font) return;
            if (starRoot.Find("StarNumber") != null) return;
            Text t = UiFactory.CreateText(starRoot, "StarNumber", font, fontSize, TextAnchor.MiddleCenter, new Color(0.545f, 0.353f, 0.169f, 1f));
            t.text = number;
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            t.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            t.rectTransform.anchoredPosition = Vector2.zero;
            t.rectTransform.sizeDelta = Vector2.zero;
            UiFactory.Stretch(t.rectTransform, 0);
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            t.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            t.rectTransform.anchoredPosition = Vector2.zero;
            t.rectTransform.sizeDelta = new Vector2(80, 80);
            t.raycastTarget = false;
        }
        private void StartResultBackdropFade(){if(!resultBackdropDim)return;if(resultBackdropFade!=null)StopCoroutine(resultBackdropFade);if(!Application.isPlaying){SetResultBackdropDim(.13f);return;}resultBackdropFade=StartCoroutine(FadeResultBackdrop());}
        private IEnumerator FadeResultBackdrop(){const float duration=.18f;float elapsed=0f;while(elapsed<duration){elapsed+=Time.unscaledDeltaTime;SetResultBackdropDim(Mathf.Lerp(0f,.13f,Mathf.Clamp01(elapsed/duration)));yield return null;}SetResultBackdropDim(.13f);resultBackdropFade=null;}
        private void SetResultBackdropDim(float alpha){if(!resultBackdropDim)return;Color color=resultBackdropDim.color;color.a=alpha;resultBackdropDim.color=color;}
        private Coroutine instructionPlayback; // local fallback only (when appAudio == null)
        private void PlayInstruction()
        {
            if (!instruction) return;
            if (appAudio != null)
            {
                appAudio.PlayVoice(instruction);
                return;
            }
            if (!audioSource) return;
            audioSource.Stop(); audioSource.clip=instruction; audioSource.mute=false; audioSource.volume=1;
            if(instruction.loadState!=AudioDataLoadState.Loaded)instruction.LoadAudioData();
            if(instructionPlayback!=null)StopCoroutine(instructionPlayback); instructionPlayback=StartCoroutine(PlayInstructionWhenReady());
        }
        private IEnumerator PlayInstructionWhenReady(){while(instruction && instruction.loadState==AudioDataLoadState.Loading)yield return null; if(instruction && instruction.loadState==AudioDataLoadState.Loaded)audioSource.Play(); instructionPlayback=null;}
        private void ToggleInstruction()
        {
            if (appAudio != null)
            {
                if (appAudio.IsVoicePlaying(instruction)) appAudio.StopVoice();
                else PlayInstruction();
                return;
            }
            if(audioSource.isPlaying || instructionPlayback!=null){audioSource.Stop(); if(instructionPlayback!=null)StopCoroutine(instructionPlayback);instructionPlayback=null;}else PlayInstruction();
        }
        private IEnumerator HongPlayback(){int[] sequence={1,2,3,2,1};int index=0;float next=0;while(true){
            bool isPlaying;
            if (appAudio != null) isPlaying = appAudio.IsVoicePlaying(instruction);
            else isPlaying = audioSource != null && (audioSource.isPlaying || instructionPlayback != null);
            if(!isPlaying){index=0;state.SetHongFrame(0);if(hongImage)hongImage.sprite=hongNeutral;}else if(Time.unscaledTime>=next){int frame=sequence[index++%sequence.Length];state.SetHongFrame(frame);if(hongImage)hongImage.sprite=frame==1?hong1:frame==2?hong2:hong3;next=Time.unscaledTime+.18f;}yield return null;}}
        private void PlaySuccess(){const int rate=44100; const int samples=rate/10; var clip=AudioClip.Create("SuccessChime",samples,1,rate,false); var data=new float[samples]; for(int i=0;i<samples;i++){float t=i/(float)rate;data[i]=Mathf.Sin(2f*Mathf.PI*Mathf.Lerp(520f,1040f,t/.1f)*t)*Mathf.Exp(-8f*t)*.32f;} clip.SetData(data,0); if(appAudio!=null) appAudio.PlaySfx(clip); else if(sfxSource) sfxSource.PlayOneShot(clip); else audioSource.PlayOneShot(clip);}
        private IEnumerator PlayRandomCompliment(){ yield return new WaitForSecondsRealtime(0.45f); AudioClip clip = null; if(compliments!=null && compliments.Length>0) clip = compliments[UnityEngine.Random.Range(0, compliments.Length)]; if(clip){ if(clip.loadState==AudioDataLoadState.Loading) yield return new WaitUntil(()=>clip.loadState!=AudioDataLoadState.Loading); if(appAudio!=null) appAudio.PlayVoice(clip); else if(voiceSource) voiceSource.PlayOneShot(clip); } }
        private IEnumerator PlayRandomEncouragement(){ yield return new WaitForSecondsRealtime(0.35f); AudioClip clip = null; if(encouragements!=null && encouragements.Length>0) clip = encouragements[UnityEngine.Random.Range(0, encouragements.Length)]; if(clip){ if(clip.loadState==AudioDataLoadState.Loading) yield return new WaitUntil(()=>clip.loadState!=AudioDataLoadState.Loading); if(appAudio!=null) appAudio.PlayVoice(clip); else if(voiceSource) voiceSource.PlayOneShot(clip); yield break; } if(tryAgain){ if(tryAgain.loadState==AudioDataLoadState.Loading) yield return new WaitUntil(()=>tryAgain.loadState!=AudioDataLoadState.Loading); if(appAudio!=null) appAudio.PlayVoice(tryAgain); else if(voiceSource) voiceSource.PlayOneShot(tryAgain); } }
        private IEnumerator PlayTryAgainDelayed(){ yield return new WaitForSecondsRealtime(0.35f); if(tryAgain){ if(tryAgain.loadState==AudioDataLoadState.Loading) yield return new WaitUntil(()=>tryAgain.loadState!=AudioDataLoadState.Loading); if(appAudio!=null) appAudio.PlayVoice(tryAgain); else if(voiceSource) voiceSource.PlayOneShot(tryAgain); } }
        private void Update(){if(state.AcceptFinalTap() && (Input.GetMouseButtonDown(0) || Input.touchCount>0))ReturnToLobby();}
        private void OnApplicationFocus(bool hasFocus)
        {
            if (appAudio != null)
            {
                // Global AppAudioService handles its own OnApplicationFocus/OnApplicationPause; avoid duplicate pause.
                return;
            }
            if (!hasFocus)
            {
                wasPausedByFocus = true;
                if (audioSource && audioSource.isPlaying) audioSource.Pause();
                if (voiceSource && voiceSource.isPlaying) voiceSource.Pause();
                if (sfxSource && sfxSource.isPlaying) sfxSource.Pause();
            }
            else if (wasPausedByFocus)
            {
                wasPausedByFocus = false;
                if (audioSource && audioSource.clip != null) audioSource.UnPause();
                if (voiceSource && voiceSource.clip != null) voiceSource.UnPause();
            }
        }
        private void OnDisable(){Cleanup();}
        private void OnDestroy(){Cleanup();}
        private void Cleanup()
        {
            if(playback!=null)StopCoroutine(playback); playback=null;
            if(resultBackdropFade!=null)StopCoroutine(resultBackdropFade); resultBackdropFade=null;
            if(instructionPlayback!=null)StopCoroutine(instructionPlayback); instructionPlayback=null;
            if(audioSource)audioSource.Stop();
            if(sfxSource) sfxSource.Stop();
            if(voiceSource)voiceSource.Stop();
            if(appAudio != null)
            {
                appAudio.StopVoice();
                appAudio.StopMusic();
            }
            if(proximityHighlighter) proximityHighlighter.HideImmediate(); else proximity?.gameObject.SetActive(false);
            foreach(var card in draggables) if(card) card.Restore();
            if(board){board.anchoredPosition=Vector2.zero;ClearCelebrationVisuals();foreach(Transform child in board)if(child.name.StartsWith("Final"))RemoveTransient(child.gameObject);}
            celebrationObjects.Clear(); activePointer=int.MinValue; state.Reset();
        }
        private void ClearCelebrationVisuals(){if(!board)return;for(int i=board.childCount-1;i>=0;i--){Transform child=board.GetChild(i);if(child.name=="ResultCelebration"||child.name=="ResultBackdropDim"||child.name.StartsWith("GreenGlow")||child.name.StartsWith("StarBurst")||child.name.StartsWith("CelebrationStar")||child.name.StartsWith("CurvedStreamer"))RemoveTransient(child.gameObject);}resultBackdropDim=null;celebrationObjects.Clear();}
        private static void RemoveTransient(GameObject gameObject){gameObject.SetActive(false);if(Application.isPlaying)Destroy(gameObject);else DestroyImmediate(gameObject);}
        private void ReturnToLobby(){if(services!=null)services.GameLauncher.ShowLobby();}
#if UNITY_EDITOR
        public void CaptureInitial() { Cleanup(); ClearCaptureVisuals(); draggables.Clear(); Build(); }
        private DragDropCard CorrectDraggable() { foreach (var d in draggables) if (d.TokenId == ShapeAnalogyRule.CorrectAnswer) return d; return null; }
        public void CaptureDragOver() { ResetCaptureScene(); DragDropCard correct = CorrectDraggable(); if (correct != null) { correct.Lift(); ((RectTransform)correct.transform).anchoredPosition = target.anchoredPosition + new Vector2(0, 20); } if(proximityHighlighter) proximityHighlighter.ShowForDistance(0f); else { proximity.gameObject.SetActive(true); if (proximity.TryGetComponent<CanvasGroup>(out var cg)) cg.alpha = 0.85f; proximity.color = new Color(1f, .38f, .08f, 1f); proximity.OutlineThickness = 3f; proximity.rectTransform.localScale = Vector3.one * 1.04f; } }
        public void CaptureSuccess() { ResetCaptureScene(); DragDropCard correct = CorrectDraggable(); if (correct != null) { ((RectTransform)correct.transform).anchoredPosition = target.anchoredPosition; var cg = correct.GetComponent<CanvasGroup>(); if(cg) cg.blocksRaycasts = false; } HideMissingArtwork(); CreateCelebration(); }
        public void CaptureFinal() { ResetCaptureScene(); DragDropCard correct = CorrectDraggable(); if (correct != null) { ((RectTransform)correct.transform).anchoredPosition = target.anchoredPosition; var cg = correct.GetComponent<CanvasGroup>(); if(cg) cg.blocksRaycasts = false; } HideMissingArtwork(); CreateCelebration(); CreateFinal(); }
        private void ResetCaptureScene() { Cleanup(); ClearCaptureVisuals(); draggables.Clear(); Build(); }
        private void ClearCaptureVisuals() { if (!board) return; for (int i = board.childCount - 1; i >= 0; i--) DestroyImmediate(board.GetChild(i).gameObject); celebrationObjects.Clear(); }
#endif
    }
}
