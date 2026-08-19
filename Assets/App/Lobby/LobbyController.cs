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
        [SerializeField] private GameCatalog catalog;

        private AppServices services;
        private RectTransform contentRoot;
        private Text heading;
        private Text result;

        public void SetCatalog(GameCatalog gameCatalog)
        {
            catalog = gameCatalog;
        }

        public void Configure(AppServices appServices)
        {
            services = appServices;
            BuildInterface();
            ShowCategories();
        }

        private void BuildInterface()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            RectTransform root = canvas.GetComponent<RectTransform>();

            Image background = UiFactory.CreateImage(root, "Background", new Color(0.08f, 0.11f, 0.2f));
            UiFactory.Stretch(background.rectTransform, 0f);

            Text title = UiFactory.CreateText(root, "Title", font, 48, TextAnchor.UpperCenter, Color.white);
            title.text = "LBS Mini Games";
            UiFactory.Anchor(title.rectTransform, new Vector2(0.1f, 0.84f), new Vector2(0.9f, 0.97f));

            heading = UiFactory.CreateText(root, "Heading", font, 32, TextAnchor.MiddleCenter, new Color(0.72f, 0.87f, 1f));
            UiFactory.Anchor(heading.rectTransform, new Vector2(0.1f, 0.72f), new Vector2(0.9f, 0.83f));

            result = UiFactory.CreateText(root, "LastResult", font, 22, TextAnchor.MiddleCenter, Color.white);
            UiFactory.Anchor(result.rectTransform, new Vector2(0.12f, 0.06f), new Vector2(0.88f, 0.15f));

            GameObject content = new("CatalogContent", typeof(RectTransform));
            content.transform.SetParent(root, false);
            contentRoot = content.GetComponent<RectTransform>();
            UiFactory.Anchor(contentRoot, new Vector2(0.13f, 0.2f), new Vector2(0.87f, 0.7f));
        }

        private void ShowCategories()
        {
            ClearContent();
            heading.text = "Choose a category";
            result.text = FormatLastResult(services.Session.LastResult);

            int count = catalog == null ? 0 : catalog.Categories.Count;
            for (int index = 0; index < count; index++)
            {
                GameCategory category = catalog.Categories[index];
                CreateCatalogButton(category.DisplayName, category.Description, index, count, () => ShowGames(category));
            }
        }

        private void ShowGames(GameCategory category)
        {
            ClearContent();
            heading.text = category.DisplayName;
            result.text = category.Description;

            int count = 0;
            foreach (GameDefinition _ in catalog.GetGames(category))
            {
                count++;
            }

            int index = 0;
            foreach (GameDefinition game in catalog.GetGames(category))
            {
                GameDefinition selectedGame = game;
                CreateCatalogButton(selectedGame.VisibleName, selectedGame.Description, index++, count, () => services.GameLauncher.Launch(selectedGame));
            }

            Button back = UiFactory.CreateButton(contentRoot, "BackButton", Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), "Categories", new Color(0.28f, 0.36f, 0.56f));
            UiFactory.Anchor(back.GetComponent<RectTransform>(), new Vector2(0.35f, 0.01f), new Vector2(0.65f, 0.13f));
            back.onClick.AddListener(ShowCategories);
        }

        private void CreateCatalogButton(string title, string description, int index, int count, UnityEngine.Events.UnityAction onClick)
        {
            float slotHeight = 0.78f / Mathf.Max(count, 1);
            float maxY = 0.94f - index * slotHeight;
            Button button = UiFactory.CreateButton(contentRoot, "CatalogButton", Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), title + "\n" + description, new Color(0.16f, 0.45f, 0.68f));
            UiFactory.Anchor(button.GetComponent<RectTransform>(), new Vector2(0.06f, maxY - slotHeight + 0.03f), new Vector2(0.94f, maxY));
            button.onClick.AddListener(onClick);
        }

        private void ClearContent()
        {
            for (int index = contentRoot.childCount - 1; index >= 0; index--)
            {
                Destroy(contentRoot.GetChild(index).gameObject);
            }
        }

        private static string FormatLastResult(MiniGameResult? lastResult)
        {
            if (!lastResult.HasValue)
            {
                return "Complete a game to see your latest result here.";
            }

            MiniGameResult value = lastResult.Value;
            return value.CompletionState == MiniGameCompletionState.Completed
                ? $"Last result: {value.GameId} — {value.Score}% ({value.CorrectActions}/{value.TotalActions} correct)"
                : $"Last result: {value.GameId} — not completed.";
        }
    }
}
