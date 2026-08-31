using System;
using System.Collections;
using System.Collections.Generic;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Lobby
{
    public sealed class LobbyController : MonoBehaviour, IAppScene
    {
        private static readonly Color Purple = new(0.580f, 0.282f, 0.957f);
        private static readonly Color Orange = new(1f, 0.718f, 0.251f);
        private static readonly Color DarkInk = new(0.141f, 0.102f, 0.208f);
        private static readonly Color NeutralCanvas = new(0.969f, 0.961f, 0.980f);
        private static readonly Color White = Color.white;
        private static readonly Color PalePurple = new(0.927f, 0.875f, 0.992f);
        private static readonly Color SoftPurple = new(0.780f, 0.643f, 0.980f);

        private const int MaximumCardsPerPage = 8;
        private const float OpeningDelaySeconds = 0.16f;
        private const float WolfieDialogueMascotWidthFraction = 0.30f;

        [SerializeField] private GameCatalog catalog;
        [SerializeField] private Font interfaceFont;
        [SerializeField] private Sprite brandLogo;
        [Header("Mascot Layout")]
        [Tooltip("Optional non-interactive Wolfie sprite shown in the Hub mascot area.")]
        [SerializeField] private Sprite mascotSprite;
        [Tooltip("Fraction of the main content width reserved for the mascot area.")]
        [SerializeField, Range(0.20f, 0.30f)] private float mascotAreaWidthFraction = 0.30f;
        [Tooltip("Inset from the mascot area's bottom-right edge in reference pixels.")]
        [SerializeField] private Vector2 mascotBottomRightInset = new(18f, 18f);

        private readonly List<GameDefinition> games = new();

        private AppServices services;
        private RectTransform gameGridRoot;
        private Text pageIndicator;
        private Button previousPageButton;
        private Button nextPageButton;
        private int pageIndex;
        private bool launchInProgress;
        private bool loggedFontFallback;

        public void SetCatalog(GameCatalog gameCatalog)
        {
            catalog = gameCatalog;
        }

        public void SetInterfaceFont(Font font)
        {
            interfaceFont = font;
        }

        public void SetBrandLogo(Sprite logo)
        {
            brandLogo = logo;
        }

        public void SetMascotSprite(Sprite sprite)
        {
            mascotSprite = sprite;
        }

        public void Configure(AppServices appServices)
        {
            services = appServices;
            BuildInterface();
        }

        private void BuildInterface()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Lobby requires a parent Canvas.", this);
                return;
            }

            RectTransform root = canvas.GetComponent<RectTransform>();
            Font font = ResolveInterfaceFont();

            Image background = UiFactory.CreateImage(root, "HubBackground", NeutralCanvas);
            UiFactory.Stretch(background.rectTransform, 0f);

            Image header = UiFactory.CreateImage(root, "HubHeader", Purple);
            UiFactory.Anchor(header.rectTransform, new Vector2(0f, 0.845f), new Vector2(1f, 1f));

            if (brandLogo != null)
            {
                Image logo = UiFactory.CreateImage(root, "LbsPlusLogo", White);
                logo.sprite = brandLogo;
                logo.preserveAspect = true;
                logo.raycastTarget = false;
                UiFactory.Anchor(logo.rectTransform, new Vector2(0.075f, 0.865f), new Vector2(0.130f, 0.975f));
            }

            Text title = UiFactory.CreateText(root, "HubTitle", font, 56, TextAnchor.MiddleLeft, White);
            title.text = "LBS+ Games";
            UiFactory.ApplySyntheticHeaderStroke(title, White);
            title.raycastTarget = false;
            float titleLeft = brandLogo != null ? 0.145f : 0.075f;
            UiFactory.Anchor(title.rectTransform, new Vector2(titleLeft, 0.865f), new Vector2(0.58f, 0.975f));

            GameObject mainArea = new("MainArea", typeof(RectTransform));
            mainArea.transform.SetParent(root, false);
            RectTransform mainAreaRoot = mainArea.GetComponent<RectTransform>();
            UiFactory.Anchor(mainAreaRoot, new Vector2(0.075f, 0.175f), new Vector2(0.925f, 0.765f));

            float mascotWidth = Mathf.Clamp(
                Mathf.Max(mascotAreaWidthFraction, WolfieDialogueMascotWidthFraction),
                0.20f,
                0.30f);
            GameObject gamesArea = new("GamesArea", typeof(RectTransform));
            gamesArea.transform.SetParent(mainAreaRoot, false);
            RectTransform gamesAreaRoot = gamesArea.GetComponent<RectTransform>();
            UiFactory.Anchor(gamesAreaRoot, Vector2.zero, new Vector2(1f - mascotWidth, 1f));

            GameObject gameGrid = new("GameGrid", typeof(RectTransform));
            gameGrid.transform.SetParent(gamesAreaRoot, false);
            gameGridRoot = gameGrid.GetComponent<RectTransform>();
            UiFactory.Stretch(gameGridRoot, 0f);

            GameObject mascotArea = new("MascotArea", typeof(RectTransform));
            mascotArea.transform.SetParent(mainAreaRoot, false);
            RectTransform mascotAreaRoot = mascotArea.GetComponent<RectTransform>();
            UiFactory.Anchor(mascotAreaRoot, new Vector2(1f - mascotWidth, 0f), Vector2.one);
            CreateMascotImage(mascotAreaRoot);
            CreateWolfieSpeechBubble(mascotAreaRoot, font);

            CreatePagingControls(root, font);
            CollectGames();
            ShowPage(0);
        }

        private void CollectGames()
        {
            games.Clear();
            if (catalog == null)
            {
                return;
            }

            foreach (GameCategory category in catalog.Categories)
            {
                if (category == null)
                {
                    continue;
                }

                foreach (GameDefinition game in catalog.GetGames(category))
                {
                    if (game != null)
                    {
                        games.Add(game);
                    }
                }
            }
        }

        private void ShowPage(int requestedPage)
        {
            int pageCount = Mathf.Max(1, Mathf.CeilToInt(games.Count / (float)MaximumCardsPerPage));
            pageIndex = Mathf.Clamp(requestedPage, 0, pageCount - 1);
            ClearGallery();

            if (games.Count == 0)
            {
                CreateEmptyGallery();
            }
            else
            {
                Canvas.ForceUpdateCanvases();
                CreatePageCards();
            }

            bool hasMultiplePages = pageCount > 1;
            pageIndicator.gameObject.SetActive(hasMultiplePages);
            previousPageButton.gameObject.SetActive(hasMultiplePages);
            nextPageButton.gameObject.SetActive(hasMultiplePages);
            if (hasMultiplePages)
            {
                pageIndicator.text = $"{pageIndex + 1} / {pageCount}";
                previousPageButton.interactable = pageIndex > 0;
                nextPageButton.interactable = pageIndex < pageCount - 1;
            }
        }

        private void CreatePageCards()
        {
            int firstGameIndex = pageIndex * MaximumCardsPerPage;
            int cardCount = Mathf.Min(MaximumCardsPerPage, games.Count - firstGameIndex);
            int rows = cardCount <= 4 ? 1 : 2;
            int columns = rows == 1 ? cardCount : Mathf.Min(4, Mathf.CeilToInt(cardCount / 2f));

            float horizontalPadding = 32f;
            float verticalPadding = 20f;
            float horizontalGap = 30f;
            float verticalGap = 28f;
            float availableWidth = gameGridRoot.rect.width - (horizontalPadding * 2f) - (horizontalGap * (columns - 1));
            float availableHeight = gameGridRoot.rect.height - (verticalPadding * 2f) - (verticalGap * (rows - 1));
            const float cardAspectRatio = 1.4f;
            float maximumCardWidth = (availableHeight / rows) * cardAspectRatio;
            float cardWidth = Mathf.Min(availableWidth / columns, maximumCardWidth);
            float cardHeight = cardWidth / cardAspectRatio;

            int cardOffset = 0;
            for (int row = 0; row < rows; row++)
            {
                int cardsInRow = Mathf.Min(columns, cardCount - cardOffset);
                float rowWidth = cardsInRow * cardWidth + (cardsInRow - 1) * horizontalGap;
                float rowStart = -rowWidth * 0.5f + cardWidth * 0.5f;
                float rowPosition = rows == 1
                    ? 0f
                    : (row == 0 ? (cardHeight + verticalGap) * 0.5f : -(cardHeight + verticalGap) * 0.5f);

                for (int column = 0; column < cardsInRow; column++)
                {
                    GameDefinition game = games[firstGameIndex + cardOffset++];
                    CreateGameCard(game, new Vector2(rowStart + column * (cardWidth + horizontalGap), rowPosition), new Vector2(cardWidth, cardHeight));
                }
            }
        }

        private void CreateGameCard(GameDefinition game, Vector2 position, Vector2 size)
        {
            RoundedSurface outline = UiFactory.CreateRoundedSurface(gameGridRoot, "GameCard", Purple, 40f);
            RectTransform cardTransform = outline.rectTransform;
            cardTransform.anchorMin = new Vector2(0.5f, 0.5f);
            cardTransform.anchorMax = new Vector2(0.5f, 0.5f);
            cardTransform.pivot = new Vector2(0.5f, 0.5f);
            cardTransform.anchoredPosition = position;
            cardTransform.sizeDelta = size;

            RoundedSurface cardSurface = UiFactory.CreateRoundedSurface(cardTransform, "CardSurface", White, 34f, false);
            UiFactory.Stretch(cardSurface.rectTransform, 6f);

            CreateCardArtwork(cardTransform, game);

            Text category = UiFactory.CreateText(cardTransform, "Category", ResolveInterfaceFont(), 22, TextAnchor.MiddleLeft, Purple);
            category.text = game.Category == null ? "Juego" : game.Category.DisplayName;
            category.raycastTarget = false;
            UiFactory.Anchor(category.rectTransform, new Vector2(0.06f, 0.155f), new Vector2(0.94f, 0.235f));

            Text title = UiFactory.CreateText(cardTransform, "Title", ResolveInterfaceFont(), 40, TextAnchor.UpperLeft, DarkInk);
            title.text = game.VisibleName;
            title.raycastTarget = false;
            UiFactory.Anchor(title.rectTransform, new Vector2(0.06f, 0.030f), new Vector2(0.94f, 0.150f));

            RoundedSurface openingCue = UiFactory.CreateRoundedSurface(cardTransform, "OpeningCue", Orange, 18f, false);
            UiFactory.Anchor(openingCue.rectTransform, new Vector2(0.68f, 0.765f), new Vector2(0.93f, 0.900f));
            Text openingLabel = UiFactory.CreateText(openingCue.rectTransform, "Label", ResolveInterfaceFont(), 20, TextAnchor.MiddleCenter, DarkInk);
            openingLabel.text = "Abriendo...";
            openingLabel.raycastTarget = false;
            UiFactory.Stretch(openingLabel.rectTransform, 6f);

            GameCardFeedback feedback = outline.gameObject.AddComponent<GameCardFeedback>();
            feedback.Configure(outline, openingCue.gameObject, Purple, Orange);
            feedback.SelectionRequested += card => RequestLaunch(game, card);
        }

        private void CreateCardArtwork(RectTransform cardTransform, GameDefinition game)
        {
            RoundedSurface artBackground = UiFactory.CreateRoundedSurface(cardTransform, "Artwork", PalePurple, 26f, false);
            UiFactory.Anchor(artBackground.rectTransform, new Vector2(0.03f, 0.23975f), new Vector2(0.97f, 0.98f));
            Mask artworkMask = artBackground.gameObject.AddComponent<Mask>();
            artworkMask.showMaskGraphic = false;

            if (game.Thumbnail != null)
            {
                Image thumbnail = UiFactory.CreateImage(artBackground.rectTransform, "Thumbnail", Color.white);
                thumbnail.sprite = game.Thumbnail;
                thumbnail.raycastTarget = false;
                AspectRatioFitter thumbnailFit = thumbnail.gameObject.AddComponent<AspectRatioFitter>();
                thumbnailFit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                thumbnailFit.aspectRatio = thumbnail.sprite.rect.width / thumbnail.sprite.rect.height;
                UiFactory.Stretch(thumbnail.rectTransform, 0f);
                return;
            }

            RoundedSurface orangePlanet = UiFactory.CreateRoundedSurface(artBackground.rectTransform, "OrangePlanet", Orange, 999f, false);
            UiFactory.Anchor(orangePlanet.rectTransform, new Vector2(0.12f, 0.20f), new Vector2(0.37f, 0.70f));

            RoundedSurface purpleNode = UiFactory.CreateRoundedSurface(artBackground.rectTransform, "PurpleNode", Purple, 999f, false);
            UiFactory.Anchor(purpleNode.rectTransform, new Vector2(0.63f, 0.54f), new Vector2(0.79f, 0.84f));

            RoundedSurface softNode = UiFactory.CreateRoundedSurface(artBackground.rectTransform, "SoftNode", SoftPurple, 999f, false);
            UiFactory.Anchor(softNode.rectTransform, new Vector2(0.66f, 0.17f), new Vector2(0.86f, 0.46f));
        }

        private void CreatePagingControls(RectTransform root, Font font)
        {
            previousPageButton = UiFactory.CreateRoundedButton(root, "PreviousPage", font, "Anterior", Purple, White, 22f);
            UiFactory.Anchor(previousPageButton.GetComponent<RectTransform>(), new Vector2(0.075f, 0.065f), new Vector2(0.205f, 0.130f));
            previousPageButton.onClick.AddListener(() => ShowPage(pageIndex - 1));

            nextPageButton = UiFactory.CreateRoundedButton(root, "NextPage", font, "Siguiente", Orange, DarkInk, 22f);
            UiFactory.Anchor(nextPageButton.GetComponent<RectTransform>(), new Vector2(0.795f, 0.065f), new Vector2(0.925f, 0.130f));
            nextPageButton.onClick.AddListener(() => ShowPage(pageIndex + 1));

            pageIndicator = UiFactory.CreateText(root, "PageIndicator", font, 28, TextAnchor.MiddleCenter, DarkInk);
            pageIndicator.raycastTarget = false;
            UiFactory.Anchor(pageIndicator.rectTransform, new Vector2(0.43f, 0.065f), new Vector2(0.57f, 0.130f));
        }

        private void CreateEmptyGallery()
        {
            RoundedSurface emptySurface = UiFactory.CreateRoundedSurface(gameGridRoot, "EmptyGallery", White, 32f, false);
            UiFactory.Stretch(emptySurface.rectTransform, 32f);
            Text message = UiFactory.CreateText(emptySurface.rectTransform, "Message", ResolveInterfaceFont(), 34, TextAnchor.MiddleCenter, DarkInk);
            message.text = "Aún no hay juegos disponibles.";
            message.raycastTarget = false;
            UiFactory.Stretch(message.rectTransform, 32f);
        }

        private void RequestLaunch(GameDefinition game, GameCardFeedback card)
        {
            if (launchInProgress || game == null || !game.IsValid())
            {
                return;
            }

            launchInProgress = true;
            card.MarkOpening();
            StartCoroutine(LaunchAfterFeedback(game, card));
        }

        private IEnumerator LaunchAfterFeedback(GameDefinition game, GameCardFeedback card)
        {
            yield return new WaitForSecondsRealtime(OpeningDelaySeconds);

            try
            {
                // Future-ready difficulty plumbing: lobby auto-launches default difficulty without visible selector.
                GameLaunchRequest request = LobbyLaunchModel.CreateRequest(game);
                services.GameLauncher.Launch(request);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                launchInProgress = false;
                if (card != null)
                {
                    card.ResetOpening();
                }
            }
        }

        private void ClearGallery()
        {
            for (int index = gameGridRoot.childCount - 1; index >= 0; index--)
            {
                Destroy(gameGridRoot.GetChild(index).gameObject);
            }
        }

        private void CreateMascotImage(RectTransform mascotAreaRoot)
        {
            if (mascotSprite == null)
            {
                return;
            }

            Image mascot = UiFactory.CreateImage(mascotAreaRoot, "WolfieImage", White);
            mascot.sprite = mascotSprite;
            mascot.preserveAspect = true;
            mascot.raycastTarget = false;
            UiFactory.Stretch(mascot.rectTransform, 0f);
            mascot.rectTransform.offsetMax = -Vector2.Max(Vector2.zero, mascotBottomRightInset);
        }

        private static void CreateWolfieSpeechBubble(RectTransform mascotAreaRoot, Font font)
        {
            RoundedSurface bubble = UiFactory.CreateRoundedSurface(
                mascotAreaRoot,
                "WolfieSpeechBubble",
                White,
                28f,
                false);
            UiFactory.Anchor(bubble.rectTransform, new Vector2(0f, 0.35f), new Vector2(0.31f, 0.96f));

            RoundedSurface tail = UiFactory.CreateRoundedSurface(
                mascotAreaRoot,
                "WolfieSpeechTail",
                White,
                999f,
                false);
            UiFactory.Anchor(tail.rectTransform, new Vector2(0.22f, 0.29f), new Vector2(0.31f, 0.40f));
            tail.rectTransform.localEulerAngles = new Vector3(0f, 0f, -45f);

            Text message = UiFactory.CreateText(bubble.rectTransform, "Message", font, 32, TextAnchor.MiddleCenter, DarkInk);
            message.text = "¡Hola! Elige un juego y aprendamos juntos.";
            message.raycastTarget = false;
            message.resizeTextForBestFit = false;
            UiFactory.Stretch(message.rectTransform, 28f);
        }

        private Font ResolveInterfaceFont()
        {
            if (interfaceFont != null)
            {
                return interfaceFont;
            }

            if (!loggedFontFallback)
            {
                Debug.LogWarning("Volte Regular is not imported or assigned yet. The Lobby is using Unity's built-in fallback font.", this);
                loggedFontFallback = true;
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
