using System.Collections.Generic;
using Lbs.MiniGames.Games.Common;
using Lbs.MiniGames.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.WildWhiz
{
    /// <summary>
    /// Runtime UiFactory layout for Wild Whiz (1920x1080 safe-area, large targets/tokens, Volte only).
    /// Builds header #9448F4, tray, targets with ResolvedTokensRoot, progress/instruction/feedback
    /// [Correct]/[Try again]/[Complete] (color+label, not color alone), top-right X, bottom-left SPEAK.
    /// </summary>
    public sealed class WildWhizScreen
    {
        private static readonly Color DarkInk = new(0.141f, 0.102f, 0.208f);
        private static readonly Color NeutralGray = new(0.42f, 0.40f, 0.45f);
        private static readonly Color Orange = new(1f, 0.718f, 0.251f);
        private static readonly Color NeutralCanvas = new(0.969f, 0.961f, 0.980f);
        private static readonly Color White = Color.white;
        private static readonly Color Success = new(0.086f, 0.478f, 0.290f);
        private static readonly Color Error = new(0.702f, 0.149f, 0.118f);

        private readonly Dictionary<DropTarget, TargetArea> targetAreas = new();

        private Text progressText;
        private Text instructionText;
        private Text feedbackText;
        private Button speakButton;
        private Button closeButton;
        private Sprite speakerSprite;
        private RectTransform trayRoot;
        private RectTransform safeRoot;
        private GameObject celebrationRoot;
        private Button continueButton;
        private GameObject generatedRoot;

        public IReadOnlyDictionary<DropTarget, TargetArea> TargetAreas => targetAreas;
        public Text InstructionText => instructionText;
        public Text FeedbackText => feedbackText;
        public Button SpeakButton => speakButton;
        public Button CloseButton => closeButton;
        public RectTransform TrayRoot => trayRoot;
        public RectTransform SafeRoot => safeRoot;
        public GameObject GeneratedRoot => generatedRoot;
        public Button ContinueButton => continueButton;

        public void Build(Canvas canvas, Font font, WildWhizLevel level, System.Action onSpeak, System.Action onClose, Sprite speaker = null)
        {
            if (canvas == null)
            {
                Debug.LogError("[WildWhizScreen] Canvas is required.");
                return;
            }

            DestroyGeneratedRoot();

            RectTransform root = canvas.GetComponent<RectTransform>();
            targetAreas.Clear();
            speakerSprite = speaker;

            // Ensure canvas scaler for 1920x1080 landscape
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // One owned root keeps the full-bleed background and safe-area content symmetric.
            generatedRoot = new GameObject("WildWhizGeneratedRoot", typeof(RectTransform));
            generatedRoot.transform.SetParent(root, false);
            RectTransform generatedRect = generatedRoot.GetComponent<RectTransform>();
            generatedRect.localScale = Vector3.one;
            generatedRect.anchorMin = Vector2.zero;
            generatedRect.anchorMax = Vector2.one;
            generatedRect.offsetMin = Vector2.zero;
            generatedRect.offsetMax = Vector2.zero;
            Image fullBleedBackground = UiFactory.CreateImage(generatedRoot.GetComponent<RectTransform>(), "FullBleedBackground", NeutralCanvas);
            UiFactory.Stretch(fullBleedBackground.rectTransform, 0f);
            fullBleedBackground.raycastTarget = false;

            // Safe-area container
            GameObject safeGo = new("WildWhizSafeArea", typeof(RectTransform), typeof(WildWhizSafeArea));
            safeGo.transform.SetParent(generatedRoot.transform, false);
            safeRoot = safeGo.GetComponent<RectTransform>();
            WildWhizSafeArea.ApplySafeAnchors(safeRoot);
            safeGo.GetComponent<WildWhizSafeArea>().Refresh();

            Text title = UiFactory.CreateText(safeRoot, "Title", font, 48, TextAnchor.MiddleLeft, DarkInk);
            title.text = "Wild Whiz";
            title.raycastTarget = false;
            UiFactory.Anchor(title.rectTransform, new Vector2(0.10f, 0.900f), new Vector2(0.48f, 0.975f));

            // Top-left X (close/abandon) — invisible 88dp minimum hit area.
            closeButton = UiFactory.CreateButton(safeRoot, "CloseButton", font, "X", Color.clear);
            Text closeLabel = closeButton.GetComponentInChildren<Text>();
            closeLabel.fontSize = 68;
            closeLabel.color = NeutralGray;
            closeLabel.raycastTarget = false;
            UiFactory.Anchor(closeButton.GetComponent<RectTransform>(), new Vector2(0.025f, 0.885f), new Vector2(0.095f, 0.985f));
            if (onClose != null)
            {
                closeButton.onClick.AddListener(() => onClose());
            }

            // Progress (right of header, non-color alone)
            progressText = UiFactory.CreateText(safeRoot, "Progress", font, 26, TextAnchor.MiddleRight, DarkInk);
            progressText.raycastTarget = false;
            UiFactory.Anchor(progressText.rectTransform, new Vector2(0.48f, 0.900f), new Vector2(0.885f, 0.975f));

            // Instruction (English, always visible)
            instructionText = UiFactory.CreateText(safeRoot, "Instruction", font, 28, TextAnchor.MiddleCenter, DarkInk);
            instructionText.text = level.Instruction;
            instructionText.raycastTarget = false;
            UiFactory.Anchor(instructionText.rectTransform, new Vector2(0.10f, 0.785f), new Vector2(0.90f, 0.850f));

            // Feedback — [Correct]/[Try again]/[Complete] with color+label (not color alone)
            feedbackText = UiFactory.CreateText(safeRoot, "Feedback", font, 26, TextAnchor.MiddleCenter, DarkInk);
            feedbackText.text = "Drag each item to its group.";
            feedbackText.raycastTarget = false;
            UiFactory.Anchor(feedbackText.rectTransform, new Vector2(0.12f, 0.715f), new Vector2(0.88f, 0.770f));

            // Icon-only replay control. The transparent root remains an 88dp hit target.
            speakButton = UiFactory.CreateButton(safeRoot, "SpeakButton", font, "", new Color(1f, 1f, 1f, 0.01f));
            Text speakerLabel = speakButton.GetComponentInChildren<Text>();
            speakerLabel.gameObject.SetActive(false);
            if (speakerSprite != null)
            {
                Image icon = UiFactory.CreateImage(speakButton.GetComponent<RectTransform>(), "SpeakerIcon", Color.white);
                icon.sprite = speakerSprite;
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                UiFactory.Anchor(icon.rectTransform, new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.82f));
            }
            UiFactory.Anchor(speakButton.GetComponent<RectTransform>(), new Vector2(0.025f, 0.040f), new Vector2(0.095f, 0.145f));
            if (onSpeak != null)
            {
                speakButton.onClick.AddListener(() => onSpeak());
            }

            BuildTargetsForLevel(safeRoot, font, level);

            GameObject tray = new("TokenTray", typeof(RectTransform));
            tray.transform.SetParent(safeRoot, false);
            trayRoot = tray.GetComponent<RectTransform>();
            UiFactory.Anchor(trayRoot, new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.29f));
            BuildTokens(trayRoot, font, level);

            RefreshProgress(level, 0, level.Items.Count);
        }

        private void DestroyGeneratedRoot()
        {
            if (generatedRoot == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(generatedRoot);
            }
            else
#endif
            { UnityEngine.Object.DestroyImmediate(generatedRoot); }

            generatedRoot = null;
            safeRoot = null;
            trayRoot = null;
            closeButton = null;
            speakButton = null;
            progressText = null;
            instructionText = null;
            feedbackText = null;
            celebrationRoot = null;
            continueButton = null;
            targetAreas.Clear();
        }

        private void BuildTargetsForLevel(RectTransform safeRoot, Font font, WildWhizLevel level)
        {
            int count = level.Targets.Count;
            for (int i = 0; i < count; i++)
            {
                string targetId = level.Targets[i];
                string label = ToDisplayLabel(targetId);

                // Distribute fractional anchors evenly for 2 or 3 targets
                Vector2 min;
                Vector2 max;
                if (count == 2)
                {
                    min = i == 0 ? new Vector2(0.035f, 0.340f) : new Vector2(0.515f, 0.340f);
                    max = i == 0 ? new Vector2(0.485f, 0.670f) : new Vector2(0.965f, 0.670f);
                }
                else if (count == 3)
                {
                    float w = 0.30f;
                    float gap = 0.025f;
                    float x0 = 0.035f + i * (w + gap);
                    min = new Vector2(x0, 0.340f);
                    max = new Vector2(x0 + w, 0.670f);
                }
                else
                {
                    float w = 0.90f / count;
                    float x0 = 0.05f + i * w;
                    min = new Vector2(x0, 0.340f);
                    max = new Vector2(x0 + w - 0.02f, 0.670f);
                }

                Image hitArea = UiFactory.CreateImage(safeRoot, label + "Target", new Color(1f, 1f, 1f, 0.01f));
                UiFactory.Anchor(hitArea.rectTransform, min, max);
                DropTarget dropTarget = hitArea.gameObject.AddComponent<DropTarget>();
                dropTarget.SetClassificationId(targetId);
                if (level.TargetSprites != null && i < level.TargetSprites.Count && level.TargetSprites[i] != null)
                {
                    Image zoneImage = UiFactory.CreateImage(hitArea.rectTransform, "ZoneIllustration", Color.white);
                    zoneImage.sprite = level.TargetSprites[i];
                    zoneImage.preserveAspect = true;
                    zoneImage.raycastTarget = false;
                    UiFactory.Anchor(zoneImage.rectTransform, new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.95f));
                }

                GameObject resolvedGo = new("ResolvedTokens", typeof(RectTransform));
                resolvedGo.transform.SetParent(hitArea.rectTransform, false);
                RectTransform resolvedRoot = resolvedGo.GetComponent<RectTransform>();
                UiFactory.Anchor(resolvedRoot, new Vector2(0.08f, 0.075f), new Vector2(0.92f, 0.465f));

                targetAreas.Add(dropTarget, new TargetArea(label, targetId, resolvedRoot, level.Items.Count));
            }
        }

        private void BuildTokens(RectTransform tray, Font font, WildWhizLevel level)
        {
            int count = level.Items.Count;
            for (int i = 0; i < count; i++)
            {
                WildWhizLevel.Item item = level.Items[i];
                float w = 0.90f / count;
                float x0 = 0.05f + i * w;
                Vector2 min = new(x0, 0.02f);
                Vector2 max = new(x0 + w - 0.02f, 0.98f);
                Image tokenSurface = UiFactory.CreateImage(tray, item.TokenId + "Token", new Color(1f, 1f, 1f, 0.01f));
                UiFactory.Anchor(tokenSurface.rectTransform, min, max);
                tokenSurface.gameObject.AddComponent<CanvasGroup>();
                if (item.Sprite != null)
                {
                    Image animalImage = UiFactory.CreateImage(tokenSurface.rectTransform, "AnimalIllustration", Color.white);
                    animalImage.sprite = item.Sprite;
                    animalImage.preserveAspect = true;
                    animalImage.raycastTarget = false;
                    UiFactory.Anchor(animalImage.rectTransform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f));
                }

                DragDropToken token = tokenSurface.gameObject.AddComponent<DragDropToken>();
                token.SetTokenId(item.TokenId);
            }
        }

        public void RefreshProgress(WildWhizLevel level, int resolved, int total)
        {
            if (progressText != null)
            {
                progressText.text = $"Level {level.Id} — {resolved}/{total}";
            }
        }

        public void SetInstruction(string text)
        {
            if (instructionText != null)
            {
                instructionText.text = text;
            }
        }

        public void SetFeedbackCorrect(string targetLabel)
        {
            if (feedbackText != null)
            {
                feedbackText.text = $"[Correct] {targetLabel}";
                feedbackText.color = Success;
            }
        }

        public void SetFeedbackTryAgain()
        {
            if (feedbackText != null)
            {
                feedbackText.text = "[Try again]";
                feedbackText.color = Error;
            }
        }

        public void SetFeedbackComplete()
        {
            if (feedbackText != null)
            {
                feedbackText.text = "[Complete]";
                feedbackText.color = Success;
            }
        }

        public bool IsCelebrating => celebrationRoot != null;

        public void ShowCelebration()
        {
            if (celebrationRoot != null || safeRoot == null) return;
            Image scrim = UiFactory.CreateImage(safeRoot, "CelebrationScrim", new Color(0.141f, 0.102f, 0.208f, 0.86f));
            UiFactory.Stretch(scrim.rectTransform, 0f);
            celebrationRoot = scrim.gameObject;
            Text message = UiFactory.CreateText(scrim.rectTransform, "Perfect", instructionText.font, 72, TextAnchor.MiddleCenter, Orange);
            message.text = "PERFECT!";
            UiFactory.Anchor(message.rectTransform, new Vector2(0.2f, 0.42f), new Vector2(0.8f, 0.62f));
            Text stars = UiFactory.CreateText(scrim.rectTransform, "Stars", instructionText.font, 44, TextAnchor.MiddleCenter, White);
            stars.text = "★  ✦  ★  ✦  ★";
            UiFactory.Anchor(stars.rectTransform, new Vector2(0.2f, 0.28f), new Vector2(0.8f, 0.42f));
            Text confetti = UiFactory.CreateText(scrim.rectTransform, "Confetti", instructionText.font, 36, TextAnchor.MiddleCenter, Orange);
            confetti.text = "✦  •  ✧  •  ✦  •  ✧  •  ✦";
            UiFactory.Anchor(confetti.rectTransform, new Vector2(0.12f, 0.68f), new Vector2(0.88f, 0.78f));
        }

        public void ShowFinalCompletion(System.Action onContinue)
        {
            if (celebrationRoot != null || safeRoot == null) return;
            Image scrim = UiFactory.CreateImage(safeRoot, "FinalCompletionScrim", new Color(0.141f, 0.102f, 0.208f, 0.86f));
            UiFactory.Stretch(scrim.rectTransform, 0f);
            celebrationRoot = scrim.gameObject;
            Text primary = UiFactory.CreateText(scrim.rectTransform, "FinalPrimary", instructionText.font, 72, TextAnchor.MiddleCenter, Orange);
            primary.text = "YOU DID IT!";
            UiFactory.Anchor(primary.rectTransform, new Vector2(0.16f, 0.58f), new Vector2(0.84f, 0.76f));
            Text secondary = UiFactory.CreateText(scrim.rectTransform, "FinalSecondary", instructionText.font, 32, TextAnchor.MiddleCenter, White);
            secondary.text = "All three levels complete!";
            UiFactory.Anchor(secondary.rectTransform, new Vector2(0.16f, 0.47f), new Vector2(0.84f, 0.58f));
            continueButton = UiFactory.CreateRoundedButton(scrim.rectTransform, "ContinueButton", instructionText.font, "CONTINUE", Orange, DarkInk, 20f);
            UiFactory.Anchor(continueButton.GetComponent<RectTransform>(), new Vector2(0.32f, 0.24f), new Vector2(0.68f, 0.36f));
            if (onContinue != null) continueButton.onClick.AddListener(() => onContinue());
        }

        public void HideCelebration()
        {
            if (celebrationRoot == null) return;
            Object.Destroy(celebrationRoot);
            celebrationRoot = null;
            continueButton = null;
        }

        private static string ToDisplayLabel(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return id;
            }

            // Title-case first letter, keep rest as-is (e.g., "forest" -> "Forest")
            return char.ToUpperInvariant(id[0]) + id.Substring(1);
        }

        public sealed class TargetArea
        {
            public TargetArea(string label, string targetId, RectTransform resolvedTokensRoot, int slotCount)
            {
                Label = label;
                TargetId = targetId;
                ResolvedTokensRoot = resolvedTokensRoot;
                SlotCount = slotCount;
            }

            public string Label { get; }
            public string TargetId { get; }
            public RectTransform ResolvedTokensRoot { get; }
            public int SlotCount { get; }
            public int ResolvedCount { get; set; }
        }
    }
}
