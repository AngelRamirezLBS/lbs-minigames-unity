using System;
using System.Collections.Generic;
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
        private const string SeaGroup = "sea";
        private const string LandGroup = "land";

        private static readonly ClassificationRound.Animal[] Animals =
        {
            new("dolphin", SeaGroup),
            new("octopus", SeaGroup),
            new("lion", LandGroup),
            new("giraffe", LandGroup)
        };

        private static readonly Dictionary<string, string> AnimalNames = new()
        {
            { "dolphin", "El delfín" },
            { "octopus", "El pulpo" },
            { "lion", "El león" },
            { "giraffe", "La jirafa" }
        };

        private static readonly Color Purple = new(0.580f, 0.282f, 0.957f);
        private static readonly Color Orange = new(1f, 0.718f, 0.251f);
        private static readonly Color DarkInk = new(0.141f, 0.102f, 0.208f);
        private static readonly Color NeutralCanvas = new(0.969f, 0.961f, 0.980f);
        private static readonly Color White = Color.white;
        private static readonly Color PalePurple = new(0.927f, 0.875f, 0.992f);
        private static readonly Color SeaSurface = new(0.760f, 0.910f, 0.890f);
        private static readonly Color LandSurface = new(1f, 0.900f, 0.700f);
        private static readonly Color Success = new(0.086f, 0.478f, 0.290f);
        private static readonly Color Error = new(0.702f, 0.149f, 0.118f);

        [SerializeField] private Font interfaceFont;
        [SerializeField] private Sprite dolphinSprite;
        [SerializeField] private Sprite octopusSprite;
        [SerializeField] private Sprite lionSprite;
        [SerializeField] private Sprite giraffeSprite;
        [SerializeField, Range(0f, 1f)] private float pickupVolume = 0.16f;
        [SerializeField, Range(0f, 1f)] private float dropVolume = 0.14f;
        [SerializeField, Range(0f, 1f)] private float completionVolume = 0.20f;

        private readonly ClassificationRound round = new(Animals);
        private readonly Dictionary<DropTarget, TargetArea> targetAreas = new();

        private AppServices services;
        private AudioSource audioSource;
        private AudioClip pickupClip;
        private AudioClip dropClip;
        private AudioClip completionClip;
        private Text feedbackText;
        private Text progressText;
        private GameObject completionPanel;
        private bool resultReported;
        private bool completionPlayed;

        public string GameId => Id;
        public bool IsCompleted => round.IsCompleted;
        public event Action<MiniGameResult> Completed;

        public void Configure(AppServices appServices)
        {
            services = appServices;
            ConfigureAudio();
            BuildInterface();
        }

        private void BuildInterface()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Classification requires a parent Canvas.", this);
                return;
            }

            Font font = interfaceFont == null
                ? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                : interfaceFont;
            RectTransform root = canvas.GetComponent<RectTransform>();

            Image background = UiFactory.CreateImage(root, "Background", NeutralCanvas);
            UiFactory.Stretch(background.rectTransform, 0f);

            Image header = UiFactory.CreateImage(root, "Header", Purple);
            UiFactory.Anchor(header.rectTransform, new Vector2(0f, 0.865f), new Vector2(1f, 1f));

            Button backButton = UiFactory.CreateRoundedButton(root, "BackButton", font, "← Volver", White, DarkInk, 24f);
            UiFactory.Anchor(backButton.GetComponent<RectTransform>(), new Vector2(0.045f, 0.885f), new Vector2(0.175f, 0.975f));
            backButton.onClick.AddListener(ReturnToLobby);

            Text title = UiFactory.CreateText(root, "Title", font, 50, TextAnchor.MiddleLeft, White);
            title.text = "Clasifica los animales";
            UiFactory.ApplySyntheticHeaderStroke(title, White);
            title.raycastTarget = false;
            UiFactory.Anchor(title.rectTransform, new Vector2(0.205f, 0.890f), new Vector2(0.66f, 0.975f));

            progressText = UiFactory.CreateText(root, "Progress", font, 30, TextAnchor.MiddleRight, White);
            progressText.raycastTarget = false;
            UiFactory.Anchor(progressText.rectTransform, new Vector2(0.68f, 0.890f), new Vector2(0.94f, 0.975f));
            RefreshProgress();

            Text instruction = UiFactory.CreateText(root, "Instruction", font, 30, TextAnchor.MiddleCenter, DarkInk);
            instruction.text = "Arrastra cada animal a su hábitat.";
            instruction.raycastTarget = false;
            UiFactory.Anchor(instruction.rectTransform, new Vector2(0.10f, 0.785f), new Vector2(0.90f, 0.850f));

            feedbackText = UiFactory.CreateText(root, "Feedback", font, 26, TextAnchor.MiddleCenter, DarkInk);
            feedbackText.text = "Suelta cada animal en Mar o Tierra.";
            feedbackText.raycastTarget = false;
            UiFactory.Anchor(feedbackText.rectTransform, new Vector2(0.12f, 0.710f), new Vector2(0.88f, 0.770f));

            CreateTarget(root, font, "Mar", SeaGroup, new Vector2(0.055f, 0.340f), new Vector2(0.485f, 0.670f), SeaSurface, 2);
            CreateTarget(root, font, "Tierra", LandGroup, new Vector2(0.515f, 0.340f), new Vector2(0.945f, 0.670f), LandSurface, 2);

            RoundedSurface tray = UiFactory.CreateRoundedSurface(root, "AnimalTray", White, 36f, false);
            UiFactory.Anchor(tray.rectTransform, new Vector2(0.055f, 0.055f), new Vector2(0.945f, 0.295f));

            Text trayLabel = UiFactory.CreateText(tray.rectTransform, "Label", font, 24, TextAnchor.MiddleCenter, DarkInk);
            trayLabel.text = "Animales por clasificar";
            trayLabel.raycastTarget = false;
            UiFactory.Anchor(trayLabel.rectTransform, new Vector2(0.06f, 0.705f), new Vector2(0.94f, 0.945f));

            CreateAnimalToken(tray.rectTransform, "DolphinToken", "dolphin", dolphinSprite, new Vector2(0.055f, 0.075f), new Vector2(0.225f, 0.675f));
            CreateAnimalToken(tray.rectTransform, "OctopusToken", "octopus", octopusSprite, new Vector2(0.285f, 0.075f), new Vector2(0.455f, 0.675f));
            CreateAnimalToken(tray.rectTransform, "LionToken", "lion", lionSprite, new Vector2(0.545f, 0.075f), new Vector2(0.715f, 0.675f));
            CreateAnimalToken(tray.rectTransform, "GiraffeToken", "giraffe", giraffeSprite, new Vector2(0.775f, 0.075f), new Vector2(0.945f, 0.675f));

            CreateCompletionPanel(root, font);
        }

        private void CreateTarget(
            RectTransform root,
            Font font,
            string label,
            string classification,
            Vector2 min,
            Vector2 max,
            Color color,
            int slotCount)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(root, label + "Target", color, 36f);
            UiFactory.Anchor(surface.rectTransform, min, max);

            DropTarget target = surface.gameObject.AddComponent<DropTarget>();
            target.SetClassificationId(classification);
            target.TokenDropped += HandleDrop;

            Text targetLabel = UiFactory.CreateText(surface.rectTransform, "Label", font, 42, TextAnchor.MiddleCenter, DarkInk);
            targetLabel.text = label;
            targetLabel.raycastTarget = false;
            UiFactory.Anchor(targetLabel.rectTransform, new Vector2(0.08f, 0.590f), new Vector2(0.92f, 0.940f));

            Text targetHint = UiFactory.CreateText(surface.rectTransform, "Hint", font, 21, TextAnchor.MiddleCenter, DarkInk);
            targetHint.text = "Suelta aquí";
            targetHint.raycastTarget = false;
            UiFactory.Anchor(targetHint.rectTransform, new Vector2(0.08f, 0.465f), new Vector2(0.92f, 0.625f));

            GameObject resolvedTokens = new("ResolvedTokens", typeof(RectTransform));
            resolvedTokens.transform.SetParent(surface.rectTransform, false);
            RectTransform resolvedTokensTransform = resolvedTokens.GetComponent<RectTransform>();
            UiFactory.Anchor(resolvedTokensTransform, new Vector2(0.08f, 0.075f), new Vector2(0.92f, 0.465f));

            targetAreas.Add(target, new TargetArea(label, resolvedTokensTransform, slotCount));
        }

        private void CreateAnimalToken(
            RectTransform tray,
            string objectName,
            string animalId,
            Sprite sprite,
            Vector2 min,
            Vector2 max)
        {
            RoundedSurface tokenSurface = UiFactory.CreateRoundedSurface(tray, objectName, Color.clear, 24f);
            UiFactory.Anchor(tokenSurface.rectTransform, min, max);
            tokenSurface.gameObject.AddComponent<CanvasGroup>();

            Image tokenImage = UiFactory.CreateImage(tokenSurface.rectTransform, "AnimalImage", White);
            tokenImage.sprite = sprite;
            tokenImage.preserveAspect = true;
            tokenImage.raycastTarget = false;
            UiFactory.Stretch(tokenImage.rectTransform, 10f);

            DragDropToken token = tokenSurface.gameObject.AddComponent<DragDropToken>();
            token.SetTokenId(animalId);
            token.DragStarted += HandlePickup;
        }

        private void HandleDrop(DropTarget target, DragDropToken token)
        {
            if (round.IsCompleted || token == null || !targetAreas.TryGetValue(target, out TargetArea targetArea))
            {
                return;
            }

            PlaySound(dropClip, dropVolume);
            if (!round.TryClassify(token.TokenId, target.ClassificationId))
            {
                feedbackText.text = "Intenta otra vez.";
                feedbackText.color = Error;
                return;
            }

            token.Accept(targetArea.ResolvedTokensRoot, targetArea.ResolvedCount, targetArea.SlotCount);
            targetArea.ResolvedCount++;
            feedbackText.text = $"¡Correcto! {AnimalNames[token.TokenId]} va en {targetArea.Label}.";
            feedbackText.color = Success;
            RefreshProgress();

            if (round.IsCompleted)
            {
                PlayCompletionSound();
                ReportResult();
                completionPanel.SetActive(true);
            }
        }

        private void HandlePickup(DragDropToken token)
        {
            if (token == null || round.IsCompleted)
            {
                return;
            }

            PlaySound(pickupClip, pickupVolume);
        }

        private void ConfigureAudio()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;
            audioSource.loop = false;

            pickupClip ??= CreatePickupClip();
            dropClip ??= CreateDropClip();
            completionClip ??= CreateCompletionClip();
        }

        private void PlayCompletionSound()
        {
            if (completionPlayed)
            {
                return;
            }

            completionPlayed = true;
            PlaySound(completionClip, completionVolume);
        }

        private void PlaySound(AudioClip clip, float volume)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }

        private static AudioClip CreatePickupClip()
        {
            const float duration = 0.12f;
            return CreateRuntimeClip("ClassificationPickup", duration, time =>
            {
                float progress = time / duration;
                float phase = 2f * Mathf.PI * (440f * time + 140f * time * progress);
                return Mathf.Sin(phase) * Mathf.Sin(Mathf.PI * progress) * 0.30f;
            });
        }

        private static AudioClip CreateDropClip()
        {
            const float duration = 0.10f;
            return CreateRuntimeClip("ClassificationDrop", duration, time =>
            {
                float progress = time / duration;
                float phase = 2f * Mathf.PI * (190f * time - 40f * time * progress);
                return Mathf.Sin(phase) * Mathf.Pow(1f - progress, 2f) * 0.34f;
            });
        }

        private static AudioClip CreateCompletionClip()
        {
            const float duration = 0.32f;
            return CreateRuntimeClip("ClassificationCompletion", duration, time =>
            {
                float progress = time / duration;
                float noteProgress;
                float frequency;

                if (progress < 0.30f)
                {
                    noteProgress = progress / 0.30f;
                    frequency = 523.25f;
                }
                else if (progress < 0.65f)
                {
                    noteProgress = (progress - 0.35f) / 0.30f;
                    frequency = 659.25f;
                }
                else if (progress >= 0.70f)
                {
                    noteProgress = (progress - 0.70f) / 0.30f;
                    frequency = 783.99f;
                }
                else
                {
                    return 0f;
                }

                return Mathf.Sin(2f * Mathf.PI * frequency * time)
                    * Mathf.Sin(Mathf.PI * noteProgress)
                    * 0.28f;
            });
        }

        private static AudioClip CreateRuntimeClip(string clipName, float duration, Func<float, float> sampleAtTime)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(duration * sampleRate);
            float[] samples = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                samples[index] = sampleAtTime(index / (float)sampleRate);
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void RefreshProgress()
        {
            progressText.text = $"Clasificados: {round.ResolvedCount} de {round.TotalCount}";
        }

        private void CreateCompletionPanel(RectTransform root, Font font)
        {
            Image overlay = UiFactory.CreateImage(root, "CompletionOverlay", new Color(DarkInk.r, DarkInk.g, DarkInk.b, 0.72f));
            UiFactory.Stretch(overlay.rectTransform, 0f);
            completionPanel = overlay.gameObject;

            RoundedSurface panel = UiFactory.CreateRoundedSurface(overlay.rectTransform, "CompletionPanel", White, 44f);
            UiFactory.Anchor(panel.rectTransform, new Vector2(0.300f, 0.270f), new Vector2(0.700f, 0.730f));

            Text title = UiFactory.CreateText(panel.rectTransform, "Title", font, 48, TextAnchor.MiddleCenter, Success);
            title.text = "¡Clasificación completa!";
            title.raycastTarget = false;
            UiFactory.Anchor(title.rectTransform, new Vector2(0.08f, 0.610f), new Vector2(0.92f, 0.875f));

            Text message = UiFactory.CreateText(panel.rectTransform, "Message", font, 28, TextAnchor.MiddleCenter, DarkInk);
            message.text = "Clasificaste los cuatro animales.";
            message.raycastTarget = false;
            UiFactory.Anchor(message.rectTransform, new Vector2(0.10f, 0.415f), new Vector2(0.90f, 0.615f));

            Button backButton = UiFactory.CreateRoundedButton(panel.rectTransform, "BackToLobby", font, "Volver al inicio", Orange, DarkInk, 26f);
            backButton.GetComponentInChildren<Text>().fontSize = 30;
            UiFactory.Anchor(backButton.GetComponent<RectTransform>(), new Vector2(0.150f, 0.125f), new Vector2(0.850f, 0.355f));
            backButton.onClick.AddListener(ReturnToLobby);

            completionPanel.SetActive(false);
        }

        private void ReportResult()
        {
            if (resultReported)
            {
                return;
            }

            resultReported = true;
            MiniGameResult result = new(Id, MiniGameCompletionState.Completed, 100, round.TotalCount, round.Attempts);
            services.GameLauncher.Complete(result);
            Completed?.Invoke(result);
        }

        private void ReturnToLobby()
        {
            if (!resultReported)
            {
                MiniGameResult result = new(Id, MiniGameCompletionState.Abandoned, 0, round.ResolvedCount, round.Attempts);
                services.GameLauncher.Complete(result);
                Completed?.Invoke(result);
            }

            services.GameLauncher.ShowLobby();
        }

        private sealed class TargetArea
        {
            public TargetArea(string label, RectTransform resolvedTokensRoot, int slotCount)
            {
                Label = label;
                ResolvedTokensRoot = resolvedTokensRoot;
                SlotCount = slotCount;
            }

            public string Label { get; }
            public RectTransform ResolvedTokensRoot { get; }
            public int SlotCount { get; }
            public int ResolvedCount { get; set; }
        }
    }
}
