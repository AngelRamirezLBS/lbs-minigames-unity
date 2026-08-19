using System;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Games.Common;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.Classification
{
    public sealed class ClassificationGame : MonoBehaviour, IMiniGame, IAppScene
    {
        private const string Id = "classification.animals";
        private readonly ClassificationRound round = new("mammal");

        private AppServices services;
        private Text feedbackText;
        private Button finishButton;
        private bool resultReported;

        public string GameId => Id;
        public bool IsCompleted => round.IsCompleted;
        public event Action<MiniGameResult> Completed;

        public void Configure(AppServices appServices)
        {
            services = appServices;
            BuildInterface();
        }

        private void BuildInterface()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            RectTransform root = canvas.GetComponent<RectTransform>();

            Image background = UiFactory.CreateImage(root, "Background", new Color(0.05f, 0.12f, 0.2f));
            UiFactory.Stretch(background.rectTransform, 0f);

            Text title = UiFactory.CreateText(root, "Title", font, 46, TextAnchor.UpperCenter, Color.white);
            title.text = "Classify the animal";
            UiFactory.Anchor(title.rectTransform, new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.96f));

            Text instruction = UiFactory.CreateText(root, "Instruction", font, 28, TextAnchor.MiddleCenter, new Color(0.85f, 0.93f, 1f));
            instruction.text = "Drag the dolphin to its correct group.";
            UiFactory.Anchor(instruction.rectTransform, new Vector2(0.15f, 0.72f), new Vector2(0.85f, 0.82f));

            CreateTarget(root, font, "Mammal", "mammal", new Vector2(0.08f, 0.2f), new Vector2(0.43f, 0.62f), new Color(0.12f, 0.42f, 0.34f));
            CreateTarget(root, font, "Bird", "bird", new Vector2(0.57f, 0.2f), new Vector2(0.92f, 0.62f), new Color(0.22f, 0.28f, 0.55f));

            Button token = UiFactory.CreateButton(root, "DolphinToken", font, "DOLPHIN", new Color(0.94f, 0.7f, 0.2f));
            UiFactory.Anchor(token.GetComponent<RectTransform>(), new Vector2(0.38f, 0.51f), new Vector2(0.62f, 0.64f));
            token.gameObject.AddComponent<CanvasGroup>();
            token.gameObject.AddComponent<DragDropToken>();

            feedbackText = UiFactory.CreateText(root, "Feedback", font, 26, TextAnchor.MiddleCenter, Color.white);
            feedbackText.text = "Choose a group.";
            UiFactory.Anchor(feedbackText.rectTransform, new Vector2(0.16f, 0.09f), new Vector2(0.64f, 0.18f));

            finishButton = UiFactory.CreateButton(root, "FinishButton", font, "Back to lobby", new Color(0.2f, 0.55f, 0.86f));
            UiFactory.Anchor(finishButton.GetComponent<RectTransform>(), new Vector2(0.7f, 0.07f), new Vector2(0.92f, 0.19f));
            finishButton.interactable = false;
            finishButton.onClick.AddListener(ReturnToLobby);
        }

        private void CreateTarget(
            RectTransform root,
            Font font,
            string label,
            string classification,
            Vector2 min,
            Vector2 max,
            Color color)
        {
            Image image = UiFactory.CreateImage(root, label + "Target", color);
            UiFactory.Anchor(image.rectTransform, min, max);
            DropTarget target = image.gameObject.AddComponent<DropTarget>();
            target.SetClassificationId(classification);
            target.TokenDropped += HandleDrop;

            Text text = UiFactory.CreateText(image.rectTransform, "Label", font, 38, TextAnchor.MiddleCenter, Color.white);
            text.text = label;
            UiFactory.Stretch(text.rectTransform, 12f);
        }

        private void HandleDrop(DropTarget target, DragDropToken token)
        {
            bool isCorrect = round.TryClassify(target.ClassificationId);
            if (!isCorrect)
            {
                feedbackText.text = "Not quite. Dolphins are not birds. Try again.";
                feedbackText.color = new Color(1f, 0.72f, 0.5f);
                return;
            }

            token.Accept();
            token.gameObject.SetActive(false);
            feedbackText.text = "Correct! A dolphin is a mammal.";
            feedbackText.color = new Color(0.55f, 1f, 0.7f);
            finishButton.interactable = true;
            ReportResult();
        }

        private void ReportResult()
        {
            if (resultReported)
            {
                return;
            }

            resultReported = true;
            MiniGameResult result = new(Id, MiniGameCompletionState.Completed, 100, 1, round.Attempts);
            services.GameLauncher.Complete(result);
            Completed?.Invoke(result);
        }

        private void ReturnToLobby()
        {
            if (!resultReported)
            {
                MiniGameResult result = new(Id, MiniGameCompletionState.Abandoned, 0, 0, round.Attempts);
                services.GameLauncher.Complete(result);
                Completed?.Invoke(result);
            }

            services.GameLauncher.ShowLobby();
        }
    }
}
