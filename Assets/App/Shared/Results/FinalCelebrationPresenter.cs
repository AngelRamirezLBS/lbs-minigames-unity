using System.Collections;
using Lbs.MiniGames.GameKits.DragDrop;
using Lbs.MiniGames.Shared;
using Lbs.MiniGames.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Shared.Results
{
    public sealed class FinalCelebrationPresenter
    {
        private readonly MonoBehaviour coroutineOwner;
        private readonly FinalCelebrationConfiguration configuration;
        private Coroutine backdropFade;
        private Image backdrop;
        private FinalCelebrationParticles particles;
        private RectTransform board;

        public FinalCelebrationPresenter(MonoBehaviour coroutineOwner, FinalCelebrationConfiguration configuration)
        {
            this.coroutineOwner = coroutineOwner;
            this.configuration = configuration;
        }

        public float PresentationDelay => configuration.PresentationDelay;

        public void ShowCelebration(RectTransform presentationBoard, FinalCelebrationInput input)
        {
            Clear();
            board = presentationBoard;
            if (!board) return;

            backdrop = UiFactory.CreateImage(board, "ResultBackdropDim", configuration.BackdropColor);
            backdrop.raycastTarget = false;
            UiFactory.Stretch(backdrop.rectTransform, 0);
            backdrop.transform.SetAsLastSibling();

            GameObject rootObject = new("ResultCelebration", typeof(RectTransform), typeof(FinalCelebrationParticles));
            RectTransform root = rootObject.GetComponent<RectTransform>();
            root.SetParent(board, false);
            UiFactory.Stretch(root, 0);
            particles = rootObject.GetComponent<FinalCelebrationParticles>();
            particles.Initialize(input.FourStar, input.FiveStar, input.CircleConfetti, input.RectangularConfetti, input.Serpentina, input.Serpentina2, input.Serpentina3);
            root.SetAsLastSibling();
            StartBackdropFade();
        }

        public void ShowFinal(FinalCelebrationInput input)
        {
            if (!board) return;
            Vector2 center = configuration.GroupCenter;
            EllipseSurface haloBlur = CreateHalo("FinalHaloBlur", configuration.HaloBlurColor, center, configuration.HaloBlurSize);
            EllipseSurface halo = CreateHalo("FinalHalo", configuration.HaloColor, center, configuration.HaloSize);
            AddShadow(halo.gameObject, configuration.HaloPrimaryShadowColor, Vector2.zero);
            AddShadow(halo.gameObject, configuration.HaloSecondaryShadowColor, configuration.HaloSecondaryShadowOffset);

            Text score = UiFactory.CreateText(board, "FinalScore", input.ScoreFont, configuration.ScoreFontSize, TextAnchor.MiddleCenter, Color.white);
            score.text = "+" + input.Score;
            Pixel(score.rectTransform, center + configuration.ScoreOffset, configuration.ScoreSize);
            AddShadow(score.gameObject, configuration.ScoreShadowColor, configuration.ScoreShadowOffset);
            RectTransform starA = CreateFinalStar("FinalStarA", input.FinalStar, center + configuration.FirstStarOffset, configuration.FirstStarSize);
            RectTransform starB = input.StarCount == 2
                ? CreateFinalStar("FinalStarB", input.FinalStar, center + configuration.SecondStarOffset, configuration.SecondStarSize)
                : null;

            score.canvasRenderer.SetAlpha(0f);
            score.rectTransform.localScale = Vector3.one * configuration.EntranceStartScale;
            coroutineOwner.StartCoroutine(CardAnimator.FadeScaleIn(score.rectTransform, configuration.EntranceDuration));
            StartFinalEntrance(starA);
            StartFinalEntrance(starB);
            SetLayering(haloBlur.transform, halo.transform, score.transform, starA, starB);
        }

        public void Clear()
        {
            if (backdropFade != null) coroutineOwner.StopCoroutine(backdropFade);
            backdropFade = null;
            if (particles) particles.StopAndClear();
            if (board)
            {
                for (int index = board.childCount - 1; index >= 0; index--)
                {
                    Transform child = board.GetChild(index);
                    if (child.name == "ResultBackdropDim" || child.name == "ResultCelebration" || child.name == "FinalHaloBlur" || child.name == "FinalHalo" || child.name == "FinalScore" || child.name == "FinalStarA" || child.name == "FinalStarB" || child.name.StartsWith("GreenGlow") || child.name.StartsWith("StarBurst") || child.name.StartsWith("CelebrationStar") || child.name.StartsWith("CurvedStreamer"))
                    {
                        RemoveTransient(child.gameObject);
                    }
                }
            }
            backdrop = null;
            particles = null;
        }

        private EllipseSurface CreateHalo(string name, Color color, Vector2 center, Vector2 size)
        {
            GameObject haloObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(EllipseSurface));
            EllipseSurface halo = haloObject.GetComponent<EllipseSurface>();
            halo.color = color;
            halo.raycastTarget = false;
            halo.transform.SetParent(board, false);
            Pixel(halo.rectTransform, center, size);
            return halo;
        }

        private RectTransform CreateFinalStar(string name, Sprite sprite, Vector2 center, Vector2 size)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, name, Color.clear, configuration.StarCornerRadius);
            CanvasGroup canvasGroup = surface.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            Pixel(surface.rectTransform, center, size);
            Image artwork = UiFactory.CreateImage(surface.rectTransform, "Artwork", Color.white);
            artwork.sprite = sprite;
            artwork.preserveAspect = true;
            AddShadow(artwork.gameObject, configuration.StarArtworkShadowColor, configuration.StarArtworkShadowOffset);
            UiFactory.Stretch(artwork.rectTransform, 0);
            AddShadow(surface.gameObject, configuration.StarSurfaceShadowColor, configuration.StarSurfaceShadowOffset);
            return surface.rectTransform;
        }

        private void StartBackdropFade()
        {
            if (!backdrop) return;
            if (!Application.isPlaying)
            {
                SetBackdropAlpha(configuration.BackdropFinalAlpha);
                return;
            }
            backdropFade = coroutineOwner.StartCoroutine(FadeBackdrop());
        }

        private IEnumerator FadeBackdrop()
        {
            float elapsed = 0f;
            while (elapsed < configuration.BackdropFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetBackdropAlpha(Mathf.Lerp(0f, configuration.BackdropFinalAlpha, Mathf.Clamp01(elapsed / configuration.BackdropFadeDuration)));
                yield return null;
            }
            SetBackdropAlpha(configuration.BackdropFinalAlpha);
            backdropFade = null;
        }

        private void SetBackdropAlpha(float alpha)
        {
            if (!backdrop) return;
            Color color = backdrop.color;
            color.a = alpha;
            backdrop.color = color;
        }

        private void StartFinalEntrance(RectTransform target)
        {
            if (!target) return;
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>() ?? target.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            target.localScale = Vector3.one * configuration.EntranceStartScale;
            coroutineOwner.StartCoroutine(CardAnimator.FadeScaleIn(target, configuration.EntranceDuration));
        }

        private void SetLayering(Transform haloBlur, Transform halo, Transform score, Transform starA, Transform starB)
        {
            Transform celebration = board.Find("ResultCelebration");
            if (!backdrop || !celebration) return;
            if (!haloBlur || !halo || !score || !starA)
            {
                if (backdrop.transform.GetSiblingIndex() > celebration.GetSiblingIndex()) backdrop.transform.SetSiblingIndex(celebration.GetSiblingIndex());
                return;
            }
            if (backdrop.transform.GetSiblingIndex() > haloBlur.GetSiblingIndex()) backdrop.transform.SetSiblingIndex(haloBlur.GetSiblingIndex());
            int baseIndex = Mathf.Min(backdrop.transform.GetSiblingIndex(), haloBlur.GetSiblingIndex(), halo.GetSiblingIndex(), celebration.GetSiblingIndex(), score.GetSiblingIndex(), starA.GetSiblingIndex());
            backdrop.transform.SetSiblingIndex(baseIndex);
            haloBlur.SetSiblingIndex(baseIndex + 1);
            halo.SetSiblingIndex(baseIndex + 2);
            celebration.SetSiblingIndex(baseIndex + 3);
            score.SetSiblingIndex(baseIndex + 4);
            starA.SetSiblingIndex(baseIndex + 5);
            if (starB) starB.SetSiblingIndex(baseIndex + 6);
        }

        private static void Pixel(RectTransform rect, Vector2 top, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = LevelChromeLayout.ToAnchoredPosition(top);
            rect.sizeDelta = size;
        }

        private static void AddShadow(GameObject target, Color color, Vector2 distance)
        {
            Shadow shadow = target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static void RemoveTransient(GameObject gameObject)
        {
            gameObject.SetActive(false);
            if (Application.isPlaying) Object.Destroy(gameObject);
            else Object.DestroyImmediate(gameObject);
        }
    }
}
