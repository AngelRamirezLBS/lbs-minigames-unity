using System;
using System.Collections;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
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
        [SerializeField] private AudioClip bgMusic; // Assets/App/Games/ShapeAnalogy/Sounds/Music/bg_cabinet_menu.mp3 (bg_menu, loop volume 0.25 durante gameplay) — alternativa: bg_puzzle_shell.mp3
        [SerializeField] private AudioClip successSfx; // SFX/Feedback/sfx_success_true_answer.mp3
        [SerializeField] private AudioClip failSfx; // SFX/Feedback/sfx_fail_incorrect_answer.mp3
        [SerializeField] private AudioClip[] compliments; // Voice/Compliments/es-MX/*.mp3 (15 es-MX, opcional en/ 27)
        [SerializeField] private AudioClip[] encouragements; // Voice/Encouragement/es-MX/*.mp3 (5 es-MX, opcional en/ 16)
        [SerializeField] private Font font;
        [SerializeField] private Font scoreFont; // Nunito-Black para +8 (si null usa font fallback)
        private AppServices services; private AudioSource audioSource; private AudioSource musicSource; private AudioSource voiceSource; private RectTransform target; private RoundedSurface proximity; private RoundedSurface proximityInner; private Image hongImage; private Image missingImage; private Image resultBackdropDim; private RectTransform board;
        private readonly ShapeAnalogyState state = new(); private Coroutine playback; private Coroutine resultBackdropFade; private bool finalVisuals; private int activePointer = int.MinValue; private readonly System.Collections.Generic.List<Draggable> draggables = new();
        private const float CardReferenceSize = 295f;
        private const float ProximityFrameSize = 315f;
        private static readonly Vector2 RelationshipCenter = new(960, 550);
        private static readonly Vector2[] FixedCardCenters = { new(910, 235), new(1150, 235), new(910, 475), new(1150, 475) };
        private readonly Vector2[] origins = { new(750, 855), new(1020, 855), new(1290, 855) };
        private readonly System.Collections.Generic.List<GameObject> celebrationObjects = new();

        public void Configure(AppServices appServices) { services = appServices; Build(); if (bgMusic && musicSource) { musicSource.clip = bgMusic; musicSource.loop = true; musicSource.volume = 0.25f; musicSource.playOnAwake = false; musicSource.spatialBlend = 0; musicSource.mute = false; musicSource.Play(); } PlayInstruction(); playback = StartCoroutine(HongPlayback()); }

        private void Build()
        {
            Canvas canvas = GetComponentInParent<Canvas>(); if (!canvas) return; RectTransform root = canvas.GetComponent<RectTransform>(); board = GetOrCreateBoard(root);
            Image bg = UiFactory.CreateImage(board, "WarmYellowBackground", Background); UiFactory.Stretch(bg.rectTransform, 0);
            Button exit = UiFactory.CreateButton(board, "Exit", font, "", Color.clear); PixelRect(exit.GetComponent<RectTransform>(), new(170,150), new(170,170)); exit.GetComponentInChildren<Text>().gameObject.SetActive(false); Image exitImage = UiFactory.CreateImage(exit.transform as RectTransform, "ExitArtwork", Color.white); exitImage.sprite = exitIcon; exitImage.preserveAspect = true; AddArtworkShadow(exitImage, 2f, .14f); UiFactory.Stretch(exitImage.rectTransform, 0); exit.onClick.AddListener(ReturnToLobby);
            CreateCard(board, "GivenStar", starFull, FixedCardCenters[0], Vector2.one * CardReferenceSize, false); CreateCard(board, "GivenHeart", heartFull, FixedCardCenters[1], Vector2.one * CardReferenceSize, false);
            CreateCard(board, "PatternStar", starEmpty, FixedCardCenters[2], Vector2.one * CardReferenceSize, false);
            RoundedSurface slot = UiFactory.CreateRoundedSurface(board, "MissingSlot", Color.clear, 28); PixelRect(slot.rectTransform, FixedCardCenters[3], Vector2.one * CardReferenceSize);
            missingImage = UiFactory.CreateImage(slot.rectTransform, "DottedPlaceholder", Color.white); missingImage.sprite = missing; missingImage.preserveAspect = true; AddArtworkShadow(missingImage, 3f, .18f); UiFactory.Stretch(missingImage.rectTransform, 0);
            target = slot.rectTransform; proximity = UiFactory.CreateRoundedSurface(board, "OrangeProximityFrame", new Color(1f, .38f, .08f, 0f), 28f, false); proximity.raycastTarget = false; PixelRect(proximity.rectTransform, FixedCardCenters[3], Vector2.one * ProximityFrameSize); proximityInner = UiFactory.CreateRoundedSurface(proximity.rectTransform, "InnerCutout", Color.clear, 20f, false); proximityInner.raycastTarget = false; UiFactory.Stretch(proximityInner.rectTransform, 8f); CanvasGroup proximityCg = proximity.gameObject.AddComponent<CanvasGroup>(); proximityCg.alpha = 0f; proximityCg.blocksRaycasts = false; proximityCg.interactable = false; proximity.gameObject.SetActive(false);
            CreateDraggable(board, "HeartAnswer", heartFull, "filled-heart", origins[0]); CreateDraggable(board, "StarAnswer", starFull, "filled-star", origins[1]); CreateDraggable(board, "CorrectAnswer", heartEmpty, ShapeAnalogyRule.CorrectAnswer, origins[2]);
            RoundedSurface hongSurface = UiFactory.CreateRoundedSurface(board, "Hong", Color.clear, 28); PixelRect(hongSurface.rectTransform, new(175,930), new(220,220)); hongImage = UiFactory.CreateImage(hongSurface.rectTransform, "HongArtwork", Color.white); hongImage.sprite = hongNeutral; hongImage.preserveAspect = true; AddArtworkShadow(hongImage, 2f, .14f); UiFactory.Stretch(hongImage.rectTransform, 0); Button hong = hongSurface.gameObject.AddComponent<Button>(); hong.onClick.AddListener(ToggleInstruction);
            if (!FindFirstObjectByType<AudioListener>()) gameObject.AddComponent<AudioListener>();
            audioSource = GetComponent<AudioSource>(); if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>(); audioSource.playOnAwake = false; audioSource.spatialBlend = 0; audioSource.mute = false; audioSource.volume = 1;
            if (!musicSource) musicSource = gameObject.AddComponent<AudioSource>(); musicSource.loop = true; musicSource.volume = 0.25f; musicSource.playOnAwake = false; musicSource.spatialBlend = 0; musicSource.mute = false;
            if (!voiceSource) voiceSource = gameObject.AddComponent<AudioSource>(); voiceSource.loop = false; voiceSource.playOnAwake = false; voiceSource.spatialBlend = 0; voiceSource.volume = 1f; voiceSource.mute = false;
            EnsureAudioFallbacks();
            EnsureFontFallbacks();
        }

        private void EnsureFontFallbacks()
        {
            if (scoreFont) return;
            scoreFont = Resources.Load<Font>("Fonts/Nunito-Black");
            if (!scoreFont) scoreFont = Resources.Load<Font>("Nunito-Black");
            if (!scoreFont) scoreFont = font; // fallback a Volte si Nunito no asignado/cargado — asignar Nunito-Black en inspector para +8
        }

        private void EnsureAudioFallbacks()
        {
            // Híbrido: inspector primario, Resources fallback. Estructura: Resources/ShapeAnalogy replica Sounds.
            if (!bgMusic)
            {
                bgMusic = Resources.Load<AudioClip>("ShapeAnalogy/Music/bg_cabinet_menu");
                if (!bgMusic) Debug.LogWarning("[ShapeAnalogy] Audio fallback no encontrado: ShapeAnalogy/Music/bg_cabinet_menu");
            }
            if (!successSfx)
            {
                successSfx = Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_success_true_answer");
                if (!successSfx) Debug.LogWarning("[ShapeAnalogy] Audio fallback no encontrado: ShapeAnalogy/SFX/sfx_success_true_answer");
            }
            if (!failSfx)
            {
                failSfx = Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_fail_incorrect_answer");
                if (!failSfx) Debug.LogWarning("[ShapeAnalogy] Audio fallback no encontrado: ShapeAnalogy/SFX/sfx_fail_incorrect_answer");
            }
            if (compliments == null || compliments.Length == 0)
            {
                compliments = Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Compliments/en");
                if (compliments.Length == 0) compliments = Resources.LoadAll<AudioClip>("ShapeAnalogy/Compliments/en");
                if (compliments == null || compliments.Length == 0) Debug.LogWarning("[ShapeAnalogy] Audio fallback no encontrado: ShapeAnalogy/Voice/Compliments/en (fallback ShapeAnalogy/Compliments/en)");
            }
            if (encouragements == null || encouragements.Length == 0)
            {
                encouragements = Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Encouragement/en");
                if (encouragements.Length == 0) encouragements = Resources.LoadAll<AudioClip>("ShapeAnalogy/Encouragement/en");
                if (encouragements == null || encouragements.Length == 0) Debug.LogWarning("[ShapeAnalogy] Audio fallback no encontrado: ShapeAnalogy/Voice/Encouragement/en (fallback ShapeAnalogy/Encouragement/en)");
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

        private void PixelRect(RectTransform rect, Vector2 topOriginCenter, Vector2 size) { rect.anchorMin=rect.anchorMax=new(.5f,.5f); rect.pivot=new(.5f,.5f); rect.anchoredPosition=new(topOriginCenter.x-960,540-topOriginCenter.y); rect.sizeDelta=size; }
        private static void AddArtworkShadow(Image image, float offset, float alpha) { Shadow shadow=image.gameObject.AddComponent<Shadow>(); shadow.effectColor=new Color(0f,0f,0f,alpha); shadow.effectDistance=new Vector2(offset,-offset); shadow.useGraphicAlpha=true; }
        private Draggable CreateDraggable(RectTransform root, string name, Sprite sprite, string id, Vector2 pos)
        { RoundedSurface surface = UiFactory.CreateRoundedSurface(root, name, Color.clear, 28); PixelRect(surface.rectTransform, pos, Vector2.one * CardReferenceSize); CanvasGroup group = surface.gameObject.AddComponent<CanvasGroup>(); Image image = UiFactory.CreateImage(surface.rectTransform,"Artwork",Color.white); image.sprite=sprite; image.preserveAspect=true; AddArtworkShadow(image, 3f, .18f); UiFactory.Stretch(image.rectTransform,0); Draggable d=surface.gameObject.AddComponent<Draggable>(); d.Setup(this,id,group,surface.rectTransform.anchoredPosition); draggables.Add(d); return d; }
        private void CreateCard(RectTransform root,string name,Sprite sprite,Vector2 center,Vector2 size,bool drag) { RoundedSurface surface=UiFactory.CreateRoundedSurface(root,name,Color.clear,28); PixelRect(surface.rectTransform,center,size); Image image=UiFactory.CreateImage(surface.rectTransform,"Artwork",Color.white); image.sprite=sprite; image.preserveAspect=true; AddArtworkShadow(image, 3f, .18f); UiFactory.Stretch(image.rectTransform,0); }
        private void Begin(Draggable d, PointerEventData e) { if (activePointer != int.MinValue) return; state.StartDrag(); if (state.Phase != ShapeAnalogyPhase.Dragging) return; activePointer=e.pointerId; d.Lift(); }
        private void Move(Draggable d, PointerEventData e)
        {
            if (e.pointerId != activePointer) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(board, e.position, e.pressEventCamera, out Vector2 p);
            d.rect.anchoredPosition = p;
            float dist = Vector2.Distance(d.rect.anchoredPosition, target.anchoredPosition);
            const float maxDist = 350f;
            float t = 1f - Mathf.Clamp01(dist / maxDist);
            CanvasGroup cg = proximity.GetComponent<CanvasGroup>();
            bool show = t > 0.02f;
            proximity.gameObject.SetActive(show);
            if (show)
            {
                if (cg) cg.alpha = Mathf.Lerp(0.35f, 1f, t);
                Color c = proximity.color;
                c.r = Orange.r; c.g = Orange.g; c.b = Orange.b;
                c.a = Mathf.Lerp(0.45f, 1f, t);
                proximity.color = c;
                if (proximityInner)
                {
                    float border = Mathf.Lerp(8f, 3f, t);
                    proximityInner.rectTransform.offsetMin = new Vector2(border, border);
                    proximityInner.rectTransform.offsetMax = new Vector2(-border, -border);
                }
                proximity.rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.02f, t);
            }
        }
        private void End(Draggable d, PointerEventData e)
        {
            if (e.pointerId != activePointer) return;
            activePointer = int.MinValue;
            bool over = RectTransformUtility.RectangleContainsScreenPoint(target, e.position, e.pressEventCamera);
            if (proximity)
            {
                CanvasGroup cgHide = proximity.GetComponent<CanvasGroup>();
                if (cgHide) cgHide.alpha = 0f;
                Color pc = proximity.color; pc.a = 0f; proximity.color = pc;
                if (proximityInner)
                {
                    proximityInner.rectTransform.offsetMin = new Vector2(8f, 8f);
                    proximityInner.rectTransform.offsetMax = new Vector2(-8f, -8f);
                }
                proximity.rectTransform.localScale = Vector3.one;
                proximity.gameObject.SetActive(false);
            }
            ShapeAnalogyDropOutcome outcome = state.Drop(d.Id, over);
            if (outcome == ShapeAnalogyDropOutcome.Correct)
            {
                StartCoroutine(CorrectPlaceSequence(d));
            }
            else if (outcome == ShapeAnalogyDropOutcome.Incorrect)
            {
                if (failSfx) audioSource.PlayOneShot(failSfx);
                if (failSfx) { if (encouragements != null && encouragements.Length > 0) StartCoroutine(PlayRandomEncouragement()); else if (tryAgain) StartCoroutine(PlayTryAgainDelayed()); }
                else if (tryAgain) audioSource.PlayOneShot(tryAgain);
                StartCoroutine(ResolveIncorrect(d));
            }
            else d.Restore();
        }

        private IEnumerator CorrectPlaceSequence(Draggable d)
        {
            d.rect.anchoredPosition = target.anchoredPosition;
            HideMissingArtwork();
            yield return PunchPlace(d.rect);
            d.group.alpha = 1f;
            d.group.blocksRaycasts = true;
            d.rect.localScale = Vector3.one; // quitar transparencia de selección
            d.rect.anchoredPosition = target.anchoredPosition;
            yield return new WaitForSecondsRealtime(0.5f);
            if (successSfx) audioSource.PlayOneShot(successSfx); else PlaySuccess();
            StartCoroutine(PlayRandomCompliment());
            StartCoroutine(Celebrate());
        }

        private IEnumerator PunchPlace(RectTransform cardRect)
        {
            if (!cardRect) yield break;
            Vector2 basePos = cardRect.anchoredPosition;
            Vector3 baseScale = Vector3.one;
            const float duration = 0.22f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(elapsed / duration);
                // scale 1 -> 1.08 -> 1 : parabola peak at 0.5
                float scaleT = 1f - 4f * (p - 0.5f) * (p - 0.5f); // 0..1..0
                float scale = Mathf.Lerp(1f, 1.08f, Mathf.Clamp01(scaleT));
                // y +10 -> -4 -> 0 with easeOutBack on second segment
                float yOffset;
                if (p < 0.4f)
                {
                    float lp = p / 0.4f;
                    yOffset = Mathf.LerpUnclamped(10f, -4f, lp);
                }
                else
                {
                    float lp = (p - 0.4f) / 0.6f;
                    float c1 = 1.70158f; float c3 = c1 + 1f;
                    float eased = 1f + c3 * Mathf.Pow(lp - 1f, 3f) + c1 * Mathf.Pow(lp - 1f, 2f);
                    yOffset = Mathf.LerpUnclamped(-4f, 0f, eased);
                }
                cardRect.localScale = baseScale * scale;
                cardRect.anchoredPosition = basePos + new Vector2(0f, yOffset);
                yield return null;
            }
            cardRect.localScale = baseScale;
            cardRect.anchoredPosition = basePos;
        }
        private void HideMissingArtwork() { if (missingImage) missingImage.gameObject.SetActive(false); }
        private IEnumerator ResolveIncorrect(Draggable d){const float duration=.48f,amplitude=18f;Vector2 origin=board.anchoredPosition;float t=0;while(t<duration){t+=Time.unscaledDeltaTime;float progress=Mathf.Min(t/duration,1f);float offset=Mathf.Sin(progress*Mathf.PI*4f)*amplitude*(1f-progress);board.anchoredPosition=origin+new Vector2(offset,0);yield return null;}board.anchoredPosition=origin;foreach(var card in draggables)card.Restore();state.FinishResolve();}
        private IEnumerator Celebrate(){CreateCelebration(); yield return new WaitForSecondsRealtime(1f); CreateFinal(); state.FinishCelebration(); yield return new WaitForSecondsRealtime(1f); state.ArmFinal();}
        private void CreateCelebration(){ClearCelebrationVisuals(); resultBackdropDim=UiFactory.CreateImage(board,"ResultBackdropDim",new Color(.15f,.08f,.28f,0f));resultBackdropDim.raycastTarget=false;UiFactory.Stretch(resultBackdropDim.rectTransform,0); GameObject rootObject=new("ResultCelebration",typeof(RectTransform),typeof(ShapeAnalogyCelebrationParticles)); RectTransform root=rootObject.GetComponent<RectTransform>();root.SetParent(board,false);UiFactory.Stretch(root,0);rootObject.GetComponent<ShapeAnalogyCelebrationParticles>().Initialize(celebration4Star,celebration5Star,circleConfetti,rectangularConfetti,serpentina,serpentina2,serpentina3);celebrationObjects.Add(rootObject); resultBackdropDim.transform.SetSiblingIndex(board.childCount - 2); root.transform.SetAsLastSibling();}
        private void CreateFinal()
        {
            StartResultBackdropFade();

            Vector2 groupCenter = new Vector2(965f, 550f);

            // SOLO HALO iluminación detrás del +8 y estrellas — sutil blured sprite (chico, no tapa).
            // HaloBlur exterior sutil: 600x220 alpha 0.06
            GameObject haloBlurObject = new GameObject("FinalHaloBlur", typeof(RectTransform), typeof(CanvasRenderer), typeof(EllipseSurface));
            EllipseSurface haloBlur = haloBlurObject.GetComponent<EllipseSurface>();
            haloBlur.color = new Color(0.22f, 0.70f, 0.45f, 0.06f);
            haloBlur.raycastTarget = false;
            haloBlur.transform.SetParent(board, false);
            PixelRect(haloBlur.rectTransform, groupCenter, new Vector2(600, 220));

            // Halo principal sutil: 520x180 alpha 0.12 con Shadows suaves para blur
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

            // Premium: estrellas amarillas tamaño +8 — 175 y 195 separadas del +8 con gap ~15px, contenidas en halo 600x220/520x180
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

            // Aparecer poco a poco: fade+scale in frame-accurate (0.35s easeOutCubic 0.85→1)
            score.canvasRenderer.SetAlpha(0f);
            score.rectTransform.localScale = Vector3.one * 0.85f;
            StartCoroutine(FadeScaleIn(score.rectTransform, 0.35f));

            if (starA)
            {
                CanvasGroup cgA = starA.gameObject.GetComponent<CanvasGroup>();
                if (!cgA) cgA = starA.gameObject.AddComponent<CanvasGroup>();
                cgA.alpha = 0f;
                starA.GetComponent<RectTransform>().localScale = Vector3.one * 0.85f;
                StartCoroutine(FadeScaleIn(starA.GetComponent<RectTransform>(), 0.35f));
            }
            if (starB)
            {
                CanvasGroup cgB = starB.gameObject.GetComponent<CanvasGroup>();
                if (!cgB) cgB = starB.gameObject.AddComponent<CanvasGroup>();
                cgB.alpha = 0f;
                starB.GetComponent<RectTransform>().localScale = Vector3.one * 0.85f;
                StartCoroutine(FadeScaleIn(starB.GetComponent<RectTransform>(), 0.35f));
            }

            // Ordering: dim < haloBlur < halo < celebration < score/estrellas
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
            Text t = UiFactory.CreateText(starRoot, "StarNumber", font, fontSize, TextAnchor.MiddleCenter, new Color(0.545f, 0.353f, 0.169f, 1f)); // #8B5A2B
            t.text = number;
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            t.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            t.rectTransform.anchoredPosition = Vector2.zero;
            t.rectTransform.sizeDelta = Vector2.zero;
            UiFactory.Stretch(t.rectTransform, 0);
            // Re-centrar tras Stretch para overlay centrado perfecto
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            t.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            t.rectTransform.anchoredPosition = Vector2.zero;
            t.rectTransform.sizeDelta = new Vector2(80, 80);
            t.raycastTarget = false;
        }
        private IEnumerator FadeScaleIn(RectTransform tr, float duration)
        {
            if (!tr) yield break;
            CanvasGroup cg = tr.GetComponent<CanvasGroup>();
            Graphic graphic = tr.GetComponent<Graphic>();
            Text txt = tr.GetComponent<Text>();
            float elapsed = 0f;
            Vector3 startScale = Vector3.one * 0.85f;
            Vector3 endScale = Vector3.one;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - p, 3f); // easeOutCubic
                tr.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);
                float alpha = Mathf.Lerp(0f, 1f, eased);
                if (cg) cg.alpha = alpha;
                else if (txt) txt.canvasRenderer.SetAlpha(alpha);
                else if (graphic) graphic.canvasRenderer.SetAlpha(alpha);
                yield return null;
            }
            tr.localScale = endScale;
            if (cg) cg.alpha = 1f;
            else if (txt) txt.canvasRenderer.SetAlpha(1f);
            else if (graphic) graphic.canvasRenderer.SetAlpha(1f);
        }
        private void StartResultBackdropFade(){if(!resultBackdropDim)return;if(resultBackdropFade!=null)StopCoroutine(resultBackdropFade);if(!Application.isPlaying){SetResultBackdropDim(.13f);return;}resultBackdropFade=StartCoroutine(FadeResultBackdrop());}
        private IEnumerator FadeResultBackdrop(){const float duration=.18f;float elapsed=0f;while(elapsed<duration){elapsed+=Time.unscaledDeltaTime;SetResultBackdropDim(Mathf.Lerp(0f,.13f,Mathf.Clamp01(elapsed/duration)));yield return null;}SetResultBackdropDim(.13f);resultBackdropFade=null;}
        private void SetResultBackdropDim(float alpha){if(!resultBackdropDim)return;Color color=resultBackdropDim.color;color.a=alpha;resultBackdropDim.color=color;}
        private Coroutine instructionPlayback;
        private void PlayInstruction(){if(!instruction || !audioSource)return; audioSource.Stop(); audioSource.clip=instruction; audioSource.mute=false; audioSource.volume=1; if(instruction.loadState!=AudioDataLoadState.Loaded)instruction.LoadAudioData(); if(instructionPlayback!=null)StopCoroutine(instructionPlayback); instructionPlayback=StartCoroutine(PlayInstructionWhenReady());}
        private IEnumerator PlayInstructionWhenReady(){while(instruction && instruction.loadState==AudioDataLoadState.Loading)yield return null; if(instruction && instruction.loadState==AudioDataLoadState.Loaded)audioSource.Play(); instructionPlayback=null;}
        private void ToggleInstruction(){if(audioSource.isPlaying || instructionPlayback!=null){audioSource.Stop();if(instructionPlayback!=null)StopCoroutine(instructionPlayback);instructionPlayback=null;}else PlayInstruction();}
        private IEnumerator HongPlayback(){int[] sequence={1,2,3,2,1};int index=0;float next=0;while(true){if(!audioSource.isPlaying){index=0;state.SetHongFrame(0);if(hongImage)hongImage.sprite=hongNeutral;}else if(Time.unscaledTime>=next){int frame=sequence[index++%sequence.Length];state.SetHongFrame(frame);if(hongImage)hongImage.sprite=frame==1?hong1:frame==2?hong2:hong3;next=Time.unscaledTime+.18f;}yield return null;}}
        private void PlaySuccess(){const int rate=44100; const int samples=rate/10; var clip=AudioClip.Create("SuccessChime",samples,1,rate,false); var data=new float[samples]; for(int i=0;i<samples;i++){float t=i/(float)rate;data[i]=Mathf.Sin(2f*Mathf.PI*Mathf.Lerp(520f,1040f,t/.1f)*t)*Mathf.Exp(-8f*t)*.32f;} clip.SetData(data,0);audioSource.PlayOneShot(clip);}
        private IEnumerator PlayRandomCompliment(){ yield return new WaitForSecondsRealtime(0.45f); if(compliments!=null && compliments.Length>0 && voiceSource){ var c=compliments[UnityEngine.Random.Range(0, compliments.Length)]; if(c){ if(c.loadState==AudioDataLoadState.Loading) yield return new WaitUntil(()=>c.loadState!=AudioDataLoadState.Loading); if(c.loadState==AudioDataLoadState.Loaded || c.loadState==AudioDataLoadState.Unloaded) voiceSource.PlayOneShot(c); } } }
        private IEnumerator PlayRandomEncouragement(){ yield return new WaitForSecondsRealtime(0.35f); if(encouragements!=null && encouragements.Length>0 && voiceSource){ var e=encouragements[UnityEngine.Random.Range(0, encouragements.Length)]; if(e){ if(e.loadState==AudioDataLoadState.Loading) yield return new WaitUntil(()=>e.loadState!=AudioDataLoadState.Loading); if(e.loadState==AudioDataLoadState.Loaded || e.loadState==AudioDataLoadState.Unloaded) voiceSource.PlayOneShot(e); } } else if(tryAgain && voiceSource){ if(tryAgain.loadState==AudioDataLoadState.Loading) yield return new WaitUntil(()=>tryAgain.loadState!=AudioDataLoadState.Loading); voiceSource.PlayOneShot(tryAgain); } }
        private IEnumerator PlayTryAgainDelayed(){ yield return new WaitForSecondsRealtime(0.35f); if(tryAgain && voiceSource){ if(tryAgain.loadState==AudioDataLoadState.Loading) yield return new WaitUntil(()=>tryAgain.loadState!=AudioDataLoadState.Loading); voiceSource.PlayOneShot(tryAgain); } }
        private void Update(){if(state.AcceptFinalTap() && (Input.GetMouseButtonDown(0) || Input.touchCount>0))ReturnToLobby();}
        private void OnApplicationFocus(bool focus){if(!focus) Cleanup();}
        private void OnDisable(){Cleanup();}
        private void OnDestroy(){Cleanup();}
        private void Cleanup(){if(playback!=null)StopCoroutine(playback); playback=null; if(resultBackdropFade!=null)StopCoroutine(resultBackdropFade); resultBackdropFade=null; if(instructionPlayback!=null)StopCoroutine(instructionPlayback); instructionPlayback=null; if(audioSource)audioSource.Stop(); if(musicSource)musicSource.Stop(); if(voiceSource)voiceSource.Stop(); proximity?.gameObject.SetActive(false); foreach(var card in draggables) if(card) card.Restore(); if(board){board.anchoredPosition=Vector2.zero;ClearCelebrationVisuals();foreach(Transform child in board)if(child.name.StartsWith("Final"))RemoveTransient(child.gameObject);} celebrationObjects.Clear(); activePointer=int.MinValue; state.Reset();}
        private void ClearCelebrationVisuals(){if(!board)return;for(int i=board.childCount-1;i>=0;i--){Transform child=board.GetChild(i);if(child.name=="ResultCelebration"||child.name=="ResultBackdropDim"||child.name.StartsWith("GreenGlow")||child.name.StartsWith("StarBurst")||child.name.StartsWith("CelebrationStar")||child.name.StartsWith("CurvedStreamer"))RemoveTransient(child.gameObject);}resultBackdropDim=null;celebrationObjects.Clear();}
        private static void RemoveTransient(GameObject gameObject){gameObject.SetActive(false);if(Application.isPlaying)Destroy(gameObject);else DestroyImmediate(gameObject);}
        private void ReturnToLobby(){if(services!=null)services.GameLauncher.ShowLobby();}
#if UNITY_EDITOR
        public void CaptureInitial() { Cleanup(); ClearCaptureVisuals(); draggables.Clear(); Build(); }
        private Draggable CorrectDraggable() { foreach (var d in draggables) if (d.Id == ShapeAnalogyRule.CorrectAnswer) return d; return null; }
        public void CaptureDragOver() { ResetCaptureScene(); Draggable correct = CorrectDraggable(); if (correct != null) { correct.Lift(); correct.rect.anchoredPosition = target.anchoredPosition + new Vector2(0, 20); } proximity.gameObject.SetActive(true); if (proximity.TryGetComponent<CanvasGroup>(out var cg)) cg.alpha = 0.85f; proximity.color = new Color(1f, .38f, .08f, 1f); if (proximityInner) { proximityInner.rectTransform.offsetMin = new Vector2(3f, 3f); proximityInner.rectTransform.offsetMax = new Vector2(-3f, -3f); } proximity.rectTransform.localScale = Vector3.one * 1.04f; }
        public void CaptureSuccess() { ResetCaptureScene(); Draggable correct = CorrectDraggable(); if (correct != null) { correct.rect.anchoredPosition = target.anchoredPosition; correct.group.blocksRaycasts = false; } HideMissingArtwork(); CreateCelebration(); }
        public void CaptureFinal() { ResetCaptureScene(); Draggable correct = CorrectDraggable(); if (correct != null) { correct.rect.anchoredPosition = target.anchoredPosition; correct.group.blocksRaycasts = false; } HideMissingArtwork(); CreateCelebration(); CreateFinal(); }
        private void ResetCaptureScene() { Cleanup(); ClearCaptureVisuals(); draggables.Clear(); Build(); }
        private void ClearCaptureVisuals() { if (!board) return; for (int i = board.childCount - 1; i >= 0; i--) DestroyImmediate(board.GetChild(i).gameObject); celebrationObjects.Clear(); }
#endif
        private sealed class Draggable:MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler { internal RectTransform rect; internal string Id; private ShapeAnalogyGame owner; internal CanvasGroup group; private Vector2 origin; internal void Setup(ShapeAnalogyGame o,string id,CanvasGroup g,Vector2 p){owner=o;Id=id;group=g;origin=p;rect=(RectTransform)transform;} public void OnBeginDrag(PointerEventData e)=>owner.Begin(this,e);public void OnDrag(PointerEventData e)=>owner.Move(this,e);public void OnEndDrag(PointerEventData e)=>owner.End(this,e);internal void Lift(){group.alpha=.75f;group.blocksRaycasts=false;transform.SetAsLastSibling();}internal void Restore(){rect.anchoredPosition=origin;group.alpha=1;group.blocksRaycasts=true;} }
    }
}
