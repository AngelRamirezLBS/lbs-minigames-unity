using System;
using System.Collections.Generic;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Games.NumberPull.Domain;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.NumberPull
{
    public sealed class NumberPullGame : MonoBehaviour, IMiniGame, IAppScene
    {
        public const string StableGameId = "math.number-pull";

        private const float CountdownDuration = 3.6f;
        private const float ResultEntranceDuration = 0.42f;
        private const int MaximumOwnedContacts = 32;
        private const int ParticleCount = 28;
        private const int ResultBurstParticleCount = 18;
        private const int GameplayParticleSortingOrder = 2;
        private const int ResultParticleSortingOrder = 4;
        private const string AudioMutedPreferenceKey = "math.number-pull.audio-muted";
        private const string PurpleCharacterResourcePath = "Characters/PurpleCrewCharacter";
        private const string PurplePullingCharacterResourcePath = "Characters/PurpleCrewCharacterPulling";
        private const string OrangeCharacterResourcePath = "Characters/OrangeCrewCharacter";
        private const string OrangePullingCharacterResourcePath = "Characters/OrangeCrewCharacterPulling";
        private const string PurpleWinnerResourcePath = "Characters/PurpleWinnerCelebration";
        private const string PurpleLoserResourcePath = "Characters/PurpleLoserResult";
        private const string OrangeWinnerResourcePath = "Characters/OrangeWinnerCelebration";
        private const string OrangeLoserResourcePath = "Characters/OrangeLoserResult";
        private const string ResultConfettiStarResourcePath = "Particles/kenney-star-01";

        private static readonly Color Purple = Hex(0x9448F4);
        private static readonly Color PurpleSecondaryKeyFill = Hex(0x63349D);
        private static readonly Color Orange = Hex(0xFFB740);
        private static readonly Color OrangeSecondaryKeyFill = Hex(0xD48616);
        private static readonly Color Ink = Hex(0x241A35);
        private static readonly Color CanvasColor = Hex(0x0D0920);
        private static readonly Color Surface = Hex(0x241A3D);
        private static readonly Color SurfaceRaised = Hex(0x30224E);
        private static readonly Color SurfaceDeep = Hex(0x17102B);
        private static readonly Color TextLight = Hex(0xF0EBFF);
        private static readonly Color TextMuted = Hex(0xC9BDDD);
        private static readonly Color RopeColor = Hex(0xFFD588);
        private static readonly Color Success = Hex(0x167A4A);
        private static readonly Color Error = Hex(0xB3261E);

        [SerializeField] private int deterministicSeed;

        private readonly List<TouchTarget> touchTargets = new(32);
        private readonly ContactOwnership[] contacts = new ContactOwnership[MaximumOwnedContacts];
        private readonly Image[] particles = new Image[ParticleCount];
        private readonly Vector2[] particleVelocity = new Vector2[ParticleCount];
        private readonly float[] particleLife = new float[ParticleCount];
        private readonly List<RectTransform> safeAreaRoots = new(2);
        private readonly SafeAreaLayoutState safeAreaLayout = new();

        private AppServices services;
        private NumberPullMatch match;
        private Font font;
        private Sprite roundedSprite;
        private Sprite circleSprite;
        private Sprite leftNormalCharacterSprite;
        private Sprite leftPullingCharacterSprite;
        private Sprite rightNormalCharacterSprite;
        private Sprite rightPullingCharacterSprite;
        private Sprite leftWinnerResultSprite;
        private Sprite leftLoserResultSprite;
        private Sprite rightWinnerResultSprite;
        private Sprite rightLoserResultSprite;
        private Sprite resultConfettiStarSprite;
        private Texture2D roundedTexture;
        private Texture2D circleTexture;
        private RuntimeAudio audio;
        private System.Random effectsRandom;

        private RectTransform leftCard;
        private RectTransform rightCard;
        private RectTransform leftAvatar;
        private RectTransform rightAvatar;
        private Image leftCharacterImage;
        private Image rightCharacterImage;
        private RectTransform ropeKnot;
        private Text leftProblem;
        private Text rightProblem;
        private Text leftAnswer;
        private Text rightAnswer;
        private Text leftFeedback;
        private Text rightFeedback;
        private Text timerText;
        private Text countdownText;
        private Text resultTitle;
        private Text resultStats;
        private Image leftResultImage;
        private Image rightResultImage;
        private Image leftResultHalo;
        private Image rightResultHalo;
        private RectTransform resultCard;
        private CanvasGroup resultCanvasGroup;
        private CanvasGroup leftResultCanvasGroup;
        private CanvasGroup rightResultCanvasGroup;
        private Canvas particleCanvas;
        private Text soundLabel;
        private Image soundControl;
        private Text motionLabel;
        private GameObject resultOverlay;
        private GameObject difficultyOverlay;
        private GameObject pauseOverlay;
        private GameObject restartPauseAction;
        private GameObject changeDifficultyPauseAction;

        private int leftEntry;
        private int rightEntry;
        private bool leftHasEntry;
        private bool rightHasEntry;
        private bool leftEntryIsNegative;
        private bool rightEntryIsNegative;
        private int? pendingLeftAnswer;
        private int? pendingRightAnswer;
        private int matchIndex;
        private int lastDisplayedSecond = -1;
        private int lastCountdownWarningSecond = int.MaxValue;
        private float countdownRemaining;
        private float leftFeedbackRemaining;
        private float rightFeedbackRemaining;
        private float leftWrongRemaining;
        private float rightWrongRemaining;
        private float pullAnimationRemaining;
        private float resultEntranceRemaining;
        private Image winnerResultImage;
        private Vector2 winnerResultStartOffset;
        private int pullDirection;
        private bool interfaceBuilt;
        private bool matchStarted;
        private bool resultReported;
        private bool muted;
        private bool reducedMotion;
        private bool isPaused;
        private NumberPullDifficulty? selectedDifficulty;

        public string GameId => StableGameId;
        public bool IsCompleted => match != null && match.IsComplete;
        public bool HasSelectedDifficulty => selectedDifficulty.HasValue;
        public event Action<MiniGameResult> Completed;

        public void Configure(AppServices appServices)
        {
            services = appServices ?? throw new ArgumentNullException(nameof(appServices));
            if (!interfaceBuilt)
            {
                BuildInterface();
                interfaceBuilt = true;
            }

            ShowDifficultySelector();
        }

        private void Update()
        {
            if (!interfaceBuilt)
            {
                return;
            }

            float delta = Time.unscaledDeltaTime;
            UpdateSafeArea();
            UpdateResultEntrance(delta);
            ProcessContacts();
            if (isPaused)
            {
                return;
            }

            UpdateFeedback(delta);
            UpdateAnimation(delta);
            UpdateParticles(delta);

            if (match == null || match.IsComplete)
            {
                return;
            }

            if (!matchStarted)
            {
                UpdateCountdown(delta);
                return;
            }

            if (pendingLeftAnswer.HasValue || pendingRightAnswer.HasValue)
            {
                SubmissionResult submission = match.Submit(pendingLeftAnswer, pendingRightAnswer);
                pendingLeftAnswer = null;
                pendingRightAnswer = null;
                PresentSubmission(submission);
            }

            match.Tick(delta);
            UpdateTimer();
            ConsumeResultIfReady();
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(HideCountdown));
            ResetContactOwnership();
            pendingLeftAnswer = null;
            pendingRightAnswer = null;
            ResetTransientPresentation();
            ResetResultPresentation();
            if (audio != null)
            {
                audio.Stop();
            }
        }

        private void OnEnable()
        {
            if (interfaceBuilt)
            {
                UpdateSafeArea();
            }

            if (interfaceBuilt && matchStarted && countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                ResetContactOwnership();
                pendingLeftAnswer = null;
                pendingRightAnswer = null;
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
            {
                ResetContactOwnership();
                pendingLeftAnswer = null;
                pendingRightAnswer = null;
            }
        }

        private void OnDestroy()
        {
            if (audio != null)
            {
                audio.Dispose();
                audio = null;
            }

            if (roundedSprite != null)
            {
                Destroy(roundedSprite);
            }

            if (circleSprite != null)
            {
                Destroy(circleSprite);
            }

            if (roundedTexture != null)
            {
                Destroy(roundedTexture);
            }

            if (circleTexture != null)
            {
                Destroy(circleTexture);
            }
        }

        private void BuildInterface()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                throw new InvalidOperationException("Number Pull requires a parent Canvas.");
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            font = Resources.Load<Font>("Volte-Regular");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                Debug.LogWarning("Volte Regular was not imported; using the editor fallback font.", this);
            }

            CreateProceduralSprites();
            resultConfettiStarSprite = Resources.Load<Sprite>(ResultConfettiStarResourcePath);
            effectsRandom = new System.Random(deterministicSeed == 0 ? 72491 : deterministicSeed);
            if (!TryGetComponent(out AudioListener _))
            {
                gameObject.AddComponent<AudioListener>();
            }

            audio = new RuntimeAudio(gameObject);

            RectTransform root = canvas.GetComponent<RectTransform>();
            CreateSpaceBackdrop(root);

            RectTransform safeRoot = CreateSafeAreaRoot(root, "SafeAreaRoot");

            Text title = CreateText(safeRoot, "Title", 52, TextAnchor.MiddleCenter, TextLight);
            title.text = "NUMBER PULL";
            Anchor(title.rectTransform, new Vector2(0.31f, 0.905f), new Vector2(0.69f, 0.985f));

            Text instruction = CreateText(safeRoot, "Instruction", 24, TextAnchor.MiddleCenter, TextMuted);
            instruction.text = "Solve. Submit. Shift the balance five steps.";
            Anchor(instruction.rectTransform, new Vector2(0.30f, 0.855f), new Vector2(0.70f, 0.91f));

            soundLabel = CreateUtilityControl(safeRoot, "SoundControl", "SOUND ON ♪", new Vector2(0.025f, 0.91f), new Vector2(0.17f, 0.975f), TouchAction.ToggleSound, out soundControl);
            motionLabel = CreateUtilityControl(safeRoot, "MotionControl", "MOTION ON", new Vector2(0.83f, 0.91f), new Vector2(0.975f, 0.975f), TouchAction.ToggleMotion);
            ApplyMuted(PlayerPrefs.GetInt(AudioMutedPreferenceKey, 0) == 1, false);

            leftCard = BuildPlayerCard(safeRoot, MatchSide.Left, new Vector2(0.025f, 0.075f), new Vector2(NumberPullBoardLayout.LeftInputMaximum, 0.84f));
            rightCard = BuildPlayerCard(safeRoot, MatchSide.Right, new Vector2(NumberPullBoardLayout.RightInputMinimum, 0.075f), new Vector2(0.975f, 0.84f));

            BuildCenterStage(safeRoot);
            BuildResultOverlay(root);
            BuildDifficultySelector(root);
            BuildPauseOverlay(root);
            UpdateSafeArea();
        }

        private RectTransform BuildPlayerCard(RectTransform root, MatchSide side, Vector2 min, Vector2 max)
        {
            bool isLeft = side == MatchSide.Left;
            Image shadow = CreateImage(root, isLeft ? "LeftShadow" : "RightShadow", new Color(0f, 0f, 0f, 0.38f), roundedSprite);
            Anchor(shadow.rectTransform, min + new Vector2(0.006f, -0.010f), max + new Vector2(0.006f, -0.010f));
            shadow.raycastTarget = false;

            Image card = CreateImage(root, isLeft ? "LeftCard" : "RightCard", Surface, roundedSprite);
            Anchor(card.rectTransform, min, max);
            card.raycastTarget = false;

            Image innerGlow = CreateImage(card.rectTransform, "InnerGlow", new Color(isLeft ? Purple.r : Orange.r, isLeft ? Purple.g : Orange.g, isLeft ? Purple.b : Orange.b, 0.12f), roundedSprite);
            Anchor(innerGlow.rectTransform, new Vector2(0.018f, 0.018f), new Vector2(0.982f, 0.982f));
            innerGlow.raycastTarget = false;

            Image accent = CreateImage(card.rectTransform, "Accent", isLeft ? Purple : Orange, roundedSprite);
            Anchor(accent.rectTransform, new Vector2(0.035f, 0.91f), new Vector2(0.965f, 0.98f));
            accent.raycastTarget = false;

            Text sideName = CreateText(card.rectTransform, "SideName", 28, TextAnchor.MiddleCenter, Ink);
            sideName.text = isLeft ? "PURPLE CREW" : "ORANGE CREW";
            Anchor(sideName.rectTransform, new Vector2(0.07f, 0.90f), new Vector2(0.93f, 0.985f));

            Text problem = CreateText(card.rectTransform, "Problem", 70, TextAnchor.MiddleCenter, TextLight);
            Anchor(problem.rectTransform, new Vector2(0.07f, 0.72f), new Vector2(0.93f, 0.89f));

            Image answerSurface = CreateImage(card.rectTransform, "AnswerSurface", SurfaceDeep, roundedSprite);
            Anchor(answerSurface.rectTransform, new Vector2(0.18f, 0.61f), new Vector2(0.82f, 0.72f));
            answerSurface.raycastTarget = false;

            Text answer = CreateText(answerSurface.rectTransform, "Answer", 48, TextAnchor.MiddleCenter, isLeft ? Tint(Purple, 0.58f) : RopeColor);
            answer.text = "?";
            Stretch(answer.rectTransform, 8f);

            Text feedback = CreateText(card.rectTransform, "Feedback", 25, TextAnchor.MiddleCenter, TextMuted);
            feedback.text = "WAIT FOR THE SIGNAL";
            Anchor(feedback.rectTransform, new Vector2(0.08f, 0.545f), new Vector2(0.92f, 0.61f));

            if (isLeft)
            {
                leftProblem = problem;
                leftAnswer = answer;
                leftFeedback = feedback;
            }
            else
            {
                rightProblem = problem;
                rightAnswer = answer;
                rightFeedback = feedback;
            }

            BuildKeypad(card.rectTransform, side);
            return card.rectTransform;
        }

        private void BuildKeypad(RectTransform card, MatchSide side)
        {
            Color secondaryKeyFill = side == MatchSide.Left ? PurpleSecondaryKeyFill : OrangeSecondaryKeyFill;
            Color secondaryLabelColor = side == MatchSide.Left ? TextLight : Ink;
            int[] digits = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int index = 0; index < digits.Length; index++)
            {
                int row = index / 3;
                int column = index % 3;
                float minX = 0.07f + column * 0.30f;
                float maxX = minX + 0.26f;
                float maxY = 0.525f - row * 0.10f;
                float minY = maxY - 0.08f;
                CreateKey(card, side, digits[index] + "Key", digits[index].ToString(), new Vector2(minX, minY), new Vector2(maxX, maxY), SurfaceRaised, TextLight, TouchAction.Digit, digits[index]);
            }

            CreateKey(card, side, "SignKey", "±", new Vector2(0.07f, 0.145f), new Vector2(0.33f, 0.225f), secondaryKeyFill, secondaryLabelColor, TouchAction.ToggleSign, 0);
            CreateKey(card, side, "0Key", "0", new Vector2(0.37f, 0.145f), new Vector2(0.63f, 0.225f), SurfaceRaised, TextLight, TouchAction.Digit, 0);
            CreateKey(card, side, "ClearKey", "CLEAR", new Vector2(0.67f, 0.145f), new Vector2(0.93f, 0.225f), secondaryKeyFill, secondaryLabelColor, TouchAction.Clear, 0);
            CreateKey(card, side, "SubmitKey", "SUBMIT", new Vector2(0.07f, 0.045f), new Vector2(0.93f, 0.125f), Orange, Ink, TouchAction.Submit, 0);
        }

        private void CreateKey(RectTransform card, MatchSide side, string name, string label, Vector2 min, Vector2 max, Color color, Color labelColor, TouchAction action, int value)
        {
            Image button = CreateImage(card, name, color, roundedSprite);
            Anchor(button.rectTransform, min, max);
            button.raycastTarget = false;

            Text text = CreateText(button.rectTransform, "Label", action == TouchAction.Clear || action == TouchAction.Submit ? 24 : 34, TextAnchor.MiddleCenter, labelColor);
            text.text = label;
            Stretch(text.rectTransform, 5f);
            touchTargets.Add(new TouchTarget(button.rectTransform, side, action, value));
        }

        private void BuildCenterStage(RectTransform root)
        {
            Image stageShadow = CreateImage(root, "CenterStageShadow", new Color(0f, 0f, 0f, 0.42f), roundedSprite);
            Anchor(stageShadow.rectTransform, new Vector2(NumberPullBoardLayout.CentralStageLeft + 0.005f, 0.067f), new Vector2(NumberPullBoardLayout.CentralStageRight + 0.005f, 0.832f));
            stageShadow.raycastTarget = false;

            Image stage = CreateImage(root, "CenterStage", SurfaceDeep, roundedSprite);
            Anchor(stage.rectTransform, new Vector2(NumberPullBoardLayout.CentralStageLeft, 0.075f), new Vector2(NumberPullBoardLayout.CentralStageRight, 0.84f));
            stage.raycastTarget = false;

            Image stageAura = CreateImage(stage.rectTransform, "StageAura", new Color(Purple.r, Purple.g, Purple.b, 0.19f), circleSprite);
            Anchor(stageAura.rectTransform, new Vector2(0.05f, 0.19f), new Vector2(0.95f, 0.81f));
            stageAura.raycastTarget = false;

            timerText = CreateText(stage.rectTransform, "Timer", 50, TextAnchor.MiddleCenter, TextLight);
            timerText.text = "1:30";
            Anchor(timerText.rectTransform, new Vector2(0.10f, 0.865f), new Vector2(0.90f, 0.97f));

            Text goal = CreateText(stage.rectTransform, "Goal", 22, TextAnchor.MiddleCenter, TextMuted);
            goal.text = "BALANCE THE ROPE";
            Anchor(goal.rectTransform, new Vector2(0.10f, 0.81f), new Vector2(0.90f, 0.87f));

            GameObject animationLayerObject = new("AnimationCanvas", typeof(RectTransform), typeof(Canvas));
            animationLayerObject.transform.SetParent(root, false);
            RectTransform animationLayer = animationLayerObject.GetComponent<RectTransform>();
            Stretch(animationLayer, 0f);
            Canvas animationCanvas = animationLayerObject.GetComponent<Canvas>();
            animationCanvas.overrideSorting = true;
            animationCanvas.sortingOrder = GameplayParticleSortingOrder;

            Image ropeGlow = CreateImage(animationLayer, "RopeGlow", new Color(Orange.r, Orange.g, Orange.b, 0.25f), roundedSprite);
            Anchor(ropeGlow.rectTransform, new Vector2(NumberPullBoardLayout.RopeLeftAnchor, 0.399f), new Vector2(NumberPullBoardLayout.RopeRightAnchor, 0.428f));
            ropeGlow.raycastTarget = false;

            Image rope = CreateImage(animationLayer, "Rope", RopeColor, roundedSprite);
            Anchor(
                rope.rectTransform,
                new Vector2(NumberPullBoardLayout.RopeLeftAnchor, NumberPullBoardLayout.RopeBottomAnchor),
                new Vector2(NumberPullBoardLayout.RopeRightAnchor, NumberPullBoardLayout.RopeTopAnchor));
            rope.raycastTarget = false;

            for (int step = -5; step <= 5; step++)
            {
                Image tick = CreateImage(animationLayer, "Step" + step, step == 0 ? Orange : new Color(TextMuted.r, TextMuted.g, TextMuted.b, 0.48f), circleSprite);
                tick.rectTransform.anchorMin = new Vector2(0.5f, NumberPullBoardLayout.RopeCenterAnchor);
                tick.rectTransform.anchorMax = new Vector2(0.5f, NumberPullBoardLayout.RopeCenterAnchor);
                tick.rectTransform.sizeDelta = step == 0 ? new Vector2(24f, 24f) : new Vector2(13f, 13f);
                tick.rectTransform.anchoredPosition = new Vector2(step * NumberPullBoardLayout.KnotStep, 0f);
                tick.raycastTarget = false;
            }

            Image knotGlow = CreateImage(animationLayer, "RopeMarkerGlow", new Color(Orange.r, Orange.g, Orange.b, 0.28f), circleSprite);
            knotGlow.rectTransform.anchorMin = new Vector2(0.5f, NumberPullBoardLayout.RopeCenterAnchor);
            knotGlow.rectTransform.anchorMax = new Vector2(0.5f, NumberPullBoardLayout.RopeCenterAnchor);
            knotGlow.rectTransform.sizeDelta = new Vector2(NumberPullBoardLayout.KnotDiameter + 24f, NumberPullBoardLayout.KnotDiameter + 24f);
            knotGlow.raycastTarget = false;

            Image knot = CreateImage(animationLayer, "RopeMarker", Orange, circleSprite);
            ropeKnot = knot.rectTransform;
            ropeKnot.anchorMin = new Vector2(0.5f, NumberPullBoardLayout.RopeCenterAnchor);
            ropeKnot.anchorMax = new Vector2(0.5f, NumberPullBoardLayout.RopeCenterAnchor);
            ropeKnot.sizeDelta = new Vector2(NumberPullBoardLayout.KnotDiameter, NumberPullBoardLayout.KnotDiameter);
            ropeKnot.anchoredPosition = Vector2.zero;
            knot.raycastTarget = false;
            Text marker = CreateText(ropeKnot, "MarkerIcon", 28, TextAnchor.MiddleCenter, Ink);
            marker.text = "↔";
            Stretch(marker.rectTransform, 4f);

            leftNormalCharacterSprite = Resources.Load<Sprite>(PurpleCharacterResourcePath);
            leftPullingCharacterSprite = Resources.Load<Sprite>(PurplePullingCharacterResourcePath);
            rightNormalCharacterSprite = Resources.Load<Sprite>(OrangeCharacterResourcePath);
            rightPullingCharacterSprite = Resources.Load<Sprite>(OrangePullingCharacterResourcePath);

            leftAvatar = CreateAvatar(
                animationLayer,
                "PurplePuller",
                Purple,
                new Vector2(NumberPullBoardLayout.LeftCharacterAnchor, NumberPullBoardLayout.CharacterVerticalAnchor),
                false,
                leftNormalCharacterSprite);
            rightAvatar = CreateAvatar(
                animationLayer,
                "OrangePuller",
                Orange,
                new Vector2(NumberPullBoardLayout.RightCharacterAnchor, NumberPullBoardLayout.CharacterVerticalAnchor),
                true,
                rightNormalCharacterSprite);
            leftCharacterImage = leftAvatar.Find("CharacterVisual")?.GetComponent<Image>();
            rightCharacterImage = rightAvatar.Find("CharacterVisual")?.GetComponent<Image>();

            countdownText = CreateText(animationLayer, "Countdown", 112, TextAnchor.MiddleCenter, TextLight);
            Anchor(countdownText.rectTransform, new Vector2(0.39f, 0.48f), new Vector2(0.61f, 0.69f));

            GameObject particleLayerObject = new("ParticleCanvas", typeof(RectTransform), typeof(Canvas));
            particleLayerObject.transform.SetParent(root, false);
            RectTransform particleLayer = particleLayerObject.GetComponent<RectTransform>();
            Stretch(particleLayer, 0f);
            particleCanvas = particleLayerObject.GetComponent<Canvas>();
            particleCanvas.overrideSorting = true;
            particleCanvas.sortingOrder = GameplayParticleSortingOrder;

            for (int index = 0; index < ParticleCount; index++)
            {
                Image particle = CreateImage(particleLayer, "Confetti" + index, index % 2 == 0 ? Purple : Orange, index % 3 == 0 ? circleSprite : roundedSprite);
                particle.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                particle.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                particle.rectTransform.sizeDelta = new Vector2(18f, 18f);
                particle.type = Image.Type.Simple;
                particle.gameObject.SetActive(false);
                particle.raycastTarget = false;
                particles[index] = particle;
            }
        }

        private RectTransform CreateAvatar(RectTransform parent, string name, Color color, Vector2 anchor, bool facesLeft, Sprite characterSprite)
        {
            GameObject avatarObject = new(name, typeof(RectTransform));
            avatarObject.transform.SetParent(parent, false);
            RectTransform avatar = avatarObject.GetComponent<RectTransform>();
            avatar.anchorMin = anchor;
            avatar.anchorMax = anchor;
            avatar.sizeDelta = new Vector2(NumberPullBoardLayout.CharacterWidth, NumberPullBoardLayout.CharacterHeight);
            avatar.anchoredPosition = Vector2.zero;

            Image aura = CreateImage(avatar, "Aura", new Color(color.r, color.g, color.b, 0.28f), circleSprite);
            Anchor(aura.rectTransform, new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.88f));
            aura.raycastTarget = false;

            if (characterSprite != null)
            {
                Image character = CreateImage(avatar, "CharacterVisual", Color.white, characterSprite);
                Stretch(character.rectTransform, 0f);
                character.preserveAspect = true;
                character.raycastTarget = false;
                character.rectTransform.localScale = new Vector3(facesLeft ? -1f : 1f, 1f, 1f);
                return avatar;
            }

            Image body = CreateImage(avatar, "Body", color, roundedSprite);
            Anchor(body.rectTransform, new Vector2(0.20f, 0.10f), new Vector2(0.80f, 0.57f));
            body.raycastTarget = false;

            Image head = CreateImage(avatar, "Helmet", SurfaceRaised, circleSprite);
            Anchor(head.rectTransform, new Vector2(0.23f, 0.50f), new Vector2(0.77f, 0.94f));
            head.raycastTarget = false;

            Image visor = CreateImage(head.rectTransform, "Visor", new Color(color.r, color.g, color.b, 0.82f), roundedSprite);
            Anchor(visor.rectTransform, new Vector2(0.18f, 0.45f), new Vector2(0.82f, 0.68f));
            visor.raycastTarget = false;

            float eyeX = facesLeft ? 0.31f : 0.58f;
            Image eye = CreateImage(visor.rectTransform, "Eye", TextLight, circleSprite);
            Anchor(eye.rectTransform, new Vector2(eyeX, 0.31f), new Vector2(eyeX + 0.14f, 0.70f));
            eye.raycastTarget = false;

            Image arm = CreateImage(avatar, "RopeArm", Tint(color, 0.20f), roundedSprite);
            Anchor(arm.rectTransform, facesLeft ? new Vector2(-0.16f, 0.31f) : new Vector2(0.62f, 0.31f), facesLeft ? new Vector2(0.38f, 0.45f) : new Vector2(1.16f, 0.45f));
            arm.rectTransform.localRotation = Quaternion.Euler(0f, 0f, facesLeft ? 16f : -16f);
            arm.raycastTarget = false;

            Image badge = CreateImage(body.rectTransform, "Badge", Orange, circleSprite);
            Anchor(badge.rectTransform, new Vector2(0.34f, 0.35f), new Vector2(0.66f, 0.67f));
            badge.raycastTarget = false;
            Text plus = CreateText(badge.rectTransform, "Plus", 26, TextAnchor.MiddleCenter, Ink);
            plus.text = "✦";
            Stretch(plus.rectTransform, 2f);

            Image leftBoot = CreateImage(avatar, "LeftBoot", SurfaceDeep, roundedSprite);
            Anchor(leftBoot.rectTransform, new Vector2(0.14f, 0.01f), new Vector2(0.45f, 0.15f));
            leftBoot.raycastTarget = false;
            Image rightBoot = CreateImage(avatar, "RightBoot", SurfaceDeep, roundedSprite);
            Anchor(rightBoot.rectTransform, new Vector2(0.55f, 0.01f), new Vector2(0.86f, 0.15f));
            rightBoot.raycastTarget = false;

            return avatar;
        }

        private void BuildResultOverlay(RectTransform root)
        {
            leftWinnerResultSprite = Resources.Load<Sprite>(PurpleWinnerResourcePath);
            leftLoserResultSprite = Resources.Load<Sprite>(PurpleLoserResourcePath);
            rightWinnerResultSprite = Resources.Load<Sprite>(OrangeWinnerResourcePath);
            rightLoserResultSprite = Resources.Load<Sprite>(OrangeLoserResourcePath);

            Image dim = CreateImage(root, "ResultOverlay", new Color(Ink.r, Ink.g, Ink.b, 0.82f), null);
            Stretch(dim.rectTransform, 0f);
            Canvas resultCanvas = dim.gameObject.AddComponent<Canvas>();
            resultCanvas.overrideSorting = true;
            resultCanvas.sortingOrder = 3;
            resultOverlay = dim.gameObject;

            RectTransform safeRoot = CreateSafeAreaRoot(dim.rectTransform, "ResultSafeArea");
            leftResultHalo = CreateImage(safeRoot, "PurpleResultHalo", new Color(Purple.r, Purple.g, Purple.b, 0.22f), circleSprite);
            Anchor(leftResultHalo.rectTransform, new Vector2(0.0f, 0.12f), new Vector2(0.29f, 0.94f));
            leftResultHalo.raycastTarget = false;

            rightResultHalo = CreateImage(safeRoot, "OrangeResultHalo", new Color(Orange.r, Orange.g, Orange.b, 0.20f), circleSprite);
            Anchor(rightResultHalo.rectTransform, new Vector2(0.71f, 0.12f), new Vector2(1f, 0.94f));
            rightResultHalo.raycastTarget = false;

            Image card = CreateImage(safeRoot, "ResultCard", Surface, roundedSprite);
            Anchor(card.rectTransform, new Vector2(0.30f, 0.18f), new Vector2(0.70f, 0.82f));
            card.raycastTarget = false;
            resultCard = card.rectTransform;
            resultCanvasGroup = card.gameObject.AddComponent<CanvasGroup>();
            resultCanvasGroup.interactable = false;
            resultCanvasGroup.blocksRaycasts = false;

            Image resultGlow = CreateImage(card.rectTransform, "ResultGlow", new Color(Purple.r, Purple.g, Purple.b, 0.16f), circleSprite);
            Anchor(resultGlow.rectTransform, new Vector2(0.12f, 0.24f), new Vector2(0.88f, 0.92f));
            resultGlow.raycastTarget = false;

            leftResultImage = CreateImage(safeRoot, "PurpleResultCharacter", Color.white, null);
            leftResultImage.preserveAspect = true;
            leftResultImage.raycastTarget = false;
            leftResultCanvasGroup = leftResultImage.gameObject.AddComponent<CanvasGroup>();
            leftResultCanvasGroup.interactable = false;
            leftResultCanvasGroup.blocksRaycasts = false;
            rightResultImage = CreateImage(safeRoot, "OrangeResultCharacter", Color.white, null);
            rightResultImage.preserveAspect = true;
            rightResultImage.raycastTarget = false;
            rightResultCanvasGroup = rightResultImage.gameObject.AddComponent<CanvasGroup>();
            rightResultCanvasGroup.interactable = false;
            rightResultCanvasGroup.blocksRaycasts = false;

            resultTitle = CreateText(card.rectTransform, "ResultTitle", 56, TextAnchor.MiddleCenter, TextLight);
            Anchor(resultTitle.rectTransform, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.93f));

            resultStats = CreateText(card.rectTransform, "ResultStats", 26, TextAnchor.MiddleCenter, TextMuted);
            Anchor(resultStats.rectTransform, new Vector2(0.10f, 0.30f), new Vector2(0.90f, 0.77f));

            Image rematch = CreateImage(card.rectTransform, "Rematch", Purple, roundedSprite);
            Anchor(rematch.rectTransform, new Vector2(0.06f, 0.06f), new Vector2(0.35f, 0.25f));
            rematch.raycastTarget = false;
            Text rematchText = CreateText(rematch.rectTransform, "Label", 20, TextAnchor.MiddleCenter, Color.white);
            rematchText.text = "REMATCH ↻";
            Stretch(rematchText.rectTransform, 8f);
            touchTargets.Add(new TouchTarget(rematch.rectTransform, null, TouchAction.Rematch, 0));

            Image changeDifficulty = CreateImage(card.rectTransform, "ChangeDifficulty", SurfaceRaised, roundedSprite);
            Anchor(changeDifficulty.rectTransform, new Vector2(0.37f, 0.06f), new Vector2(0.63f, 0.25f));
            changeDifficulty.raycastTarget = false;
            Text changeDifficultyText = CreateText(changeDifficulty.rectTransform, "Label", 18, TextAnchor.MiddleCenter, TextLight);
            changeDifficultyText.text = "CHANGE\nLEVEL";
            Stretch(changeDifficultyText.rectTransform, 6f);
            touchTargets.Add(new TouchTarget(changeDifficulty.rectTransform, null, TouchAction.ChangeDifficulty, 0));

            Image hub = CreateImage(card.rectTransform, "BackToHub", Orange, roundedSprite);
            Anchor(hub.rectTransform, new Vector2(0.65f, 0.06f), new Vector2(0.94f, 0.25f));
            hub.raycastTarget = false;
            Text hubText = CreateText(hub.rectTransform, "Label", 20, TextAnchor.MiddleCenter, Ink);
            hubText.text = "HUB ⌂";
            Stretch(hubText.rectTransform, 8f);
            touchTargets.Add(new TouchTarget(hub.rectTransform, null, TouchAction.Hub, 0));

            ResetResultPresentation();
        }

        private void BuildDifficultySelector(RectTransform root)
        {
            Image dim = CreateImage(root, "DifficultySelector", new Color(Ink.r, Ink.g, Ink.b, 0.90f), null);
            Stretch(dim.rectTransform, 0f);
            Canvas selectorCanvas = dim.gameObject.AddComponent<Canvas>();
            selectorCanvas.overrideSorting = true;
            selectorCanvas.sortingOrder = 4;
            difficultyOverlay = dim.gameObject;

            RectTransform safeRoot = CreateSafeAreaRoot(dim.rectTransform, "DifficultySafeArea");
            Image panel = CreateImage(safeRoot, "DifficultyPanel", Surface, roundedSprite);
            Anchor(panel.rectTransform, new Vector2(0.055f, 0.12f), new Vector2(0.945f, 0.88f));
            panel.raycastTarget = false;

            Text title = CreateText(panel.rectTransform, "DifficultyTitle", 58, TextAnchor.MiddleCenter, TextLight);
            title.text = "CHOOSE A LEVEL";
            Anchor(title.rectTransform, new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.94f));
            Text instruction = CreateText(panel.rectTransform, "DifficultyInstruction", 27, TextAnchor.MiddleCenter, TextMuted);
            instruction.text = "Both teams play by the same rules.";
            Anchor(instruction.rectTransform, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.82f));

            CreateDifficultyCard(panel.rectTransform, "DifficultyLowerPrimary", "LEVEL 1", "Primary School · Grades 1–3", "Addition and subtraction up to 20\nNo negative numbers · 4 pulls · 1:45", new Vector2(0.055f, 0.13f), new Vector2(0.345f, 0.68f), Purple, NumberPullDifficultyTier.LowerPrimary);
            CreateDifficultyCard(panel.rectTransform, "DifficultyUpperPrimary", "LEVEL 2", "Upper Primary & Secondary\nGrades 4–9", "Addition and subtraction up to 100\nTimes tables 2–10 and exact division · 5 pulls · 1:30", new Vector2(0.355f, 0.13f), new Vector2(0.645f, 0.68f), Orange, NumberPullDifficultyTier.UpperPrimaryAndSecondary);
            CreateDifficultyCard(panel.rectTransform, "DifficultyPreparatory", "LEVEL 3", "High School · Grades 10–12", "Signed integers\nMultiplication and exact division up to 12 · 6 pulls · 1:15", new Vector2(0.655f, 0.13f), new Vector2(0.945f, 0.68f), Purple, NumberPullDifficultyTier.PreparatoryHighSchool);

            difficultyOverlay.SetActive(false);
        }

        private void CreateDifficultyCard(RectTransform parent, string name, string level, string range, string rules, Vector2 min, Vector2 max, Color accent, NumberPullDifficultyTier tier)
        {
            Image card = CreateImage(parent, name, SurfaceRaised, roundedSprite);
            Anchor(card.rectTransform, min, max);
            card.raycastTarget = false;
            Image badge = CreateImage(card.rectTransform, "Badge", accent, roundedSprite);
            Anchor(badge.rectTransform, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.93f));
            badge.raycastTarget = false;
            Text levelText = CreateText(badge.rectTransform, "Level", 28, TextAnchor.MiddleCenter, accent == Orange ? Ink : Color.white);
            levelText.text = level;
            Stretch(levelText.rectTransform, 5f);
            Text rangeText = CreateText(card.rectTransform, "AgeRange", 27, TextAnchor.MiddleCenter, TextLight);
            rangeText.text = range;
            Anchor(rangeText.rectTransform, new Vector2(0.07f, 0.48f), new Vector2(0.93f, 0.76f));
            Text rulesText = CreateText(card.rectTransform, "MathRules", 22, TextAnchor.MiddleCenter, TextMuted);
            rulesText.text = rules;
            Anchor(rulesText.rectTransform, new Vector2(0.07f, 0.13f), new Vector2(0.93f, 0.47f));
            touchTargets.Add(new TouchTarget(card.rectTransform, null, TouchAction.SelectDifficulty, (int)tier));
        }

        private Text CreateUtilityControl(RectTransform root, string name, string text, Vector2 min, Vector2 max, TouchAction action, out Image controlImage)
        {
            Image control = CreateImage(root, name, SurfaceDeep, roundedSprite);
            Anchor(control.rectTransform, min, max);
            control.raycastTarget = false;
            Text label = CreateText(control.rectTransform, "Label", 20, TextAnchor.MiddleCenter, TextMuted);
            label.text = text;
            Stretch(label.rectTransform, 7f);
            touchTargets.Add(new TouchTarget(control.rectTransform, null, action, 0));
            controlImage = control;
            return label;
        }

        private Text CreateUtilityControl(RectTransform root, string name, string text, Vector2 min, Vector2 max, TouchAction action)
        {
            return CreateUtilityControl(root, name, text, min, max, action, out _);
        }

        private void BuildPauseOverlay(RectTransform root)
        {
            Image dim = CreateImage(root, "PauseOverlay", new Color(Ink.r, Ink.g, Ink.b, 0.88f), null);
            Stretch(dim.rectTransform, 0f);
            Canvas pauseCanvas = dim.gameObject.AddComponent<Canvas>();
            pauseCanvas.overrideSorting = true;
            pauseCanvas.sortingOrder = 6;
            pauseOverlay = dim.gameObject;

            RectTransform safeRoot = CreateSafeAreaRoot(dim.rectTransform, "PauseSafeArea");
            Image card = CreateImage(safeRoot, "PauseCard", Surface, roundedSprite);
            Anchor(card.rectTransform, new Vector2(0.30f, 0.13f), new Vector2(0.70f, 0.87f));
            card.raycastTarget = false;

            Text title = CreateText(card.rectTransform, "PauseTitle", 62, TextAnchor.MiddleCenter, TextLight);
            title.text = "PAUSED";
            Anchor(title.rectTransform, new Vector2(0.10f, 0.78f), new Vector2(0.90f, 0.93f));
            Text instruction = CreateText(card.rectTransform, "PauseInstruction", 25, TextAnchor.MiddleCenter, TextMuted);
            instruction.text = "Game is paused.";
            Anchor(instruction.rectTransform, new Vector2(0.10f, 0.69f), new Vector2(0.90f, 0.78f));

            restartPauseAction = CreatePauseAction(card.rectTransform, "RestartMatch", "RESTART MATCH", Purple, TextLight, new Vector2(0.12f, 0.49f), new Vector2(0.88f, 0.62f), TouchAction.Restart);
            changeDifficultyPauseAction = CreatePauseAction(card.rectTransform, "ChangeLevel", "CHANGE LEVEL", SurfaceRaised, TextLight, new Vector2(0.12f, 0.34f), new Vector2(0.88f, 0.47f), TouchAction.ChangeDifficulty);
            CreatePauseAction(card.rectTransform, "ExitToHub", "EXIT TO HUB", Orange, Ink, new Vector2(0.12f, 0.19f), new Vector2(0.88f, 0.32f), TouchAction.Hub);
            CreatePauseAction(card.rectTransform, "ContinueMatch", "CONTINUE", TextMuted, Ink, new Vector2(0.25f, 0.05f), new Vector2(0.75f, 0.15f), TouchAction.Continue);

            pauseOverlay.SetActive(false);
        }

        private GameObject CreatePauseAction(RectTransform parent, string name, string text, Color color, Color textColor, Vector2 min, Vector2 max, TouchAction action)
        {
            Image control = CreateImage(parent, name, color, roundedSprite);
            Anchor(control.rectTransform, min, max);
            control.raycastTarget = false;
            Text label = CreateText(control.rectTransform, "Label", 29, TextAnchor.MiddleCenter, textColor);
            label.text = text;
            Stretch(label.rectTransform, 8f);
            touchTargets.Add(new TouchTarget(control.rectTransform, null, action, 0));
            return control.gameObject;
        }

        private void CreateSpaceBackdrop(RectTransform root)
        {
            Image background = CreateImage(root, "NeutralCanvas", CanvasColor, null);
            Stretch(background.rectTransform, 0f);
            background.raycastTarget = false;

            Image violetNebula = CreateImage(root, "VioletNebula", new Color(Purple.r, Purple.g, Purple.b, 0.18f), circleSprite);
            Anchor(violetNebula.rectTransform, new Vector2(-0.16f, 0.28f), new Vector2(0.48f, 1.20f));
            violetNebula.raycastTarget = false;

            Image amberNebula = CreateImage(root, "AmberNebula", new Color(Orange.r, Orange.g, Orange.b, 0.13f), circleSprite);
            Anchor(amberNebula.rectTransform, new Vector2(0.56f, -0.18f), new Vector2(1.18f, 0.62f));
            amberNebula.raycastTarget = false;

            Image horizon = CreateImage(root, "ArenaHorizon", new Color(Purple.r, Purple.g, Purple.b, 0.19f), roundedSprite);
            Anchor(horizon.rectTransform, new Vector2(0.015f, 0.365f), new Vector2(0.985f, 0.385f));
            horizon.raycastTarget = false;

            for (int index = 0; index < 38; index++)
            {
                Color color = index % 7 == 0 ? new Color(Orange.r, Orange.g, Orange.b, 0.72f) : new Color(TextLight.r, TextLight.g, TextLight.b, 0.56f);
                Image dot = CreateImage(root, "Star" + index, color, circleSprite);
                float x = Mathf.Repeat(index * 0.173f + 0.071f, 0.94f) + 0.03f;
                float y = Mathf.Repeat(index * 0.271f + 0.113f, 0.82f) + 0.08f;
                dot.rectTransform.anchorMin = new Vector2(x, y);
                dot.rectTransform.anchorMax = new Vector2(x, y);
                float size = 4f + index % 4 * 3f;
                dot.rectTransform.sizeDelta = new Vector2(size, size);
                dot.raycastTarget = false;
            }
        }

        private void StartMatch()
        {
            if (!selectedDifficulty.HasValue)
            {
                return;
            }

            CancelInvoke(nameof(HideCountdown));
            audio.StopMusic();
            ResetTransientPresentation();
            int seed = deterministicSeed == 0 ? Environment.TickCount : deterministicSeed;
            seed += matchIndex * 101;
            matchIndex++;
            NumberPullDifficulty difficulty = selectedDifficulty.Value;
            match = new NumberPullMatch(
                new MathProblemGenerator(seed, difficulty.Tier),
                new MathProblemGenerator(seed + 1, difficulty.Tier),
                difficulty.TargetPulls,
                difficulty.DurationSeconds);
            leftEntry = 0;
            rightEntry = 0;
            leftHasEntry = false;
            rightHasEntry = false;
            leftEntryIsNegative = false;
            rightEntryIsNegative = false;
            pendingLeftAnswer = null;
            pendingRightAnswer = null;
            resultReported = false;
            isPaused = false;
            matchStarted = false;
            countdownRemaining = CountdownDuration;
            lastDisplayedSecond = -1;
            lastCountdownWarningSecond = int.MaxValue;
            pullAnimationRemaining = 0f;
            ropeKnot.anchoredPosition = Vector2.zero;
            ResetResultPresentation();
            if (pauseOverlay != null)
            {
                pauseOverlay.SetActive(false);
            }

            countdownText.gameObject.SetActive(true);
            leftFeedback.text = "READY";
            rightFeedback.text = "READY";
            leftFeedback.color = TextMuted;
            rightFeedback.color = TextMuted;
            RefreshProblems();
            RefreshEntries();
            UpdateTimer();
            ResetContactOwnership();
        }

        private void ShowDifficultySelector()
        {
            CancelInvoke(nameof(HideCountdown));
            audio.StopMusic();
            ResetTransientPresentation();
            isPaused = false;
            match = null;
            matchStarted = false;
            pendingLeftAnswer = null;
            pendingRightAnswer = null;
            leftEntry = 0;
            rightEntry = 0;
            leftHasEntry = false;
            rightHasEntry = false;
            leftEntryIsNegative = false;
            rightEntryIsNegative = false;
            ResetContactOwnership();
            if (resultOverlay != null)
            {
                ResetResultPresentation();
            }

            if (pauseOverlay != null)
            {
                pauseOverlay.SetActive(false);
            }

            if (difficultyOverlay != null)
            {
                difficultyOverlay.SetActive(true);
            }
        }

        private void SelectDifficulty(NumberPullDifficultyTier tier)
        {
            selectedDifficulty = NumberPullDifficulty.For(tier);
            difficultyOverlay.SetActive(false);
            StartMatch();
            audio.Play(AudioCue.Tap, 0.3f);
        }

        private void UpdateCountdown(float delta)
        {
            int previous = Mathf.CeilToInt(countdownRemaining);
            countdownRemaining = Mathf.Max(0f, countdownRemaining - delta);
            int current = Mathf.CeilToInt(countdownRemaining);
            if (current != previous || countdownText.text.Length == 0)
            {
                if (current >= 4)
                {
                    countdownText.text = "GET READY";
                }
                else if (current > 0)
                {
                    countdownText.text = current.ToString();
                    audio.Play(AudioCue.Tap, 0.32f);
                }
            }

            if (countdownRemaining > 0f)
            {
                return;
            }

            matchStarted = true;
            countdownText.text = "PULL!";
            audio.Play(AudioCue.Pull, 0.45f);
            audio.StartMusic();
            Invoke(nameof(HideCountdown), 0.55f);
        }

        private void HideCountdown()
        {
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }
        }

        private void ProcessContacts()
        {
            int touchCount = Input.touchCount;
            for (int index = 0; index < touchCount; index++)
            {
                Touch touch = Input.GetTouch(index);
                if (touch.phase == TouchPhase.Began)
                {
                    TryBeginContact(touch.fingerId, touch.position);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    ReleaseContact(touch.fingerId);
                }
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (touchCount == 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    TryBeginContact(-1, Input.mousePosition);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    ReleaseContact(-1);
                }
            }
#endif
        }

        private void TryBeginContact(int fingerId, Vector2 screenPosition)
        {
            if (FindContact(fingerId) >= 0)
            {
                return;
            }

            int targetIndex = FindTarget(screenPosition);
            if (targetIndex < 0)
            {
                return;
            }

            for (int index = 0; index < contacts.Length; index++)
            {
                if (contacts[index].Active)
                {
                    continue;
                }

                TouchTarget target = touchTargets[targetIndex];
                contacts[index] = new ContactOwnership(true, fingerId, target.Side, target.Action);
                HandleTarget(target);
                return;
            }
        }

        private int FindTarget(Vector2 screenPosition)
        {
            if (pauseOverlay != null && pauseOverlay.activeSelf)
            {
                return FindActivePauseMenuTarget(screenPosition);
            }

            for (int index = touchTargets.Count - 1; index >= 0; index--)
            {
                TouchTarget target = touchTargets[index];
                if (difficultyOverlay != null && difficultyOverlay.activeSelf && target.Action != TouchAction.SelectDifficulty)
                {
                    continue;
                }

                if (resultOverlay != null && resultOverlay.activeSelf && target.Action != TouchAction.Rematch && target.Action != TouchAction.ChangeDifficulty && target.Action != TouchAction.Hub)
                {
                    continue;
                }

                if (!target.Rect.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (target.Side == MatchSide.Left && screenPosition.x >= Screen.width * 0.46f)
                {
                    continue;
                }

                if (target.Side == MatchSide.Right && screenPosition.x <= Screen.width * 0.54f)
                {
                    continue;
                }

                if (RectTransformUtility.RectangleContainsScreenPoint(target.Rect, screenPosition, null))
                {
                    return index;
                }
            }

            return -1;
        }

        private int FindActivePauseMenuTarget(Vector2 screenPosition)
        {
            for (int index = touchTargets.Count - 1; index >= 0; index--)
            {
                TouchTarget target = touchTargets[index];
                if (!IsPauseMenuAction(target.Action)
                    || !target.Rect.gameObject.activeInHierarchy
                    || !RectTransformUtility.RectangleContainsScreenPoint(target.Rect, screenPosition, null))
                {
                    continue;
                }

                return index;
            }

            return -1;
        }

        private static bool IsPauseMenuAction(TouchAction action)
        {
            return action == TouchAction.Restart
                || action == TouchAction.ChangeDifficulty
                || action == TouchAction.Hub
                || action == TouchAction.Continue;
        }

        private void HandleTarget(TouchTarget target)
        {
            if (target.Action == TouchAction.Continue)
            {
                ClosePauseMenu();
                return;
            }

            if (target.Action == TouchAction.Restart)
            {
                StartMatch();
                audio.Play(AudioCue.Tap, 0.3f);
                return;
            }

            if (target.Action == TouchAction.ToggleSound)
            {
                SetMuted(!muted);
                return;
            }

            if (target.Action == TouchAction.ToggleMotion)
            {
                SetReducedMotion(!reducedMotion);
                return;
            }

            if (target.Action == TouchAction.Rematch)
            {
                StartMatch();
                audio.Play(AudioCue.Tap, 0.3f);
                return;
            }

            if (target.Action == TouchAction.ChangeDifficulty)
            {
                ShowDifficultySelector();
                audio.Play(AudioCue.Tap, 0.3f);
                return;
            }

            if (target.Action == TouchAction.SelectDifficulty)
            {
                SelectDifficulty((NumberPullDifficultyTier)target.Value);
                return;
            }

            if (target.Action == TouchAction.Hub)
            {
                ReturnToHub();
                return;
            }

            if (!matchStarted || match == null || match.IsComplete || !target.Side.HasValue)
            {
                return;
            }

            MatchSide side = target.Side.Value;
            switch (target.Action)
            {
                case TouchAction.Digit:
                    AppendDigit(side, target.Value);
                    audio.Play(AudioCue.Tap, 0.22f);
                    break;
                case TouchAction.Clear:
                    ClearEntry(side);
                    audio.Play(AudioCue.Tap, 0.22f);
                    break;
                case TouchAction.ToggleSign:
                    ToggleEntrySign(side);
                    audio.Play(AudioCue.Tap, 0.22f);
                    break;
                case TouchAction.Submit:
                    QueueSubmission(side);
                    break;
            }
        }

        private void AppendDigit(MatchSide side, int digit)
        {
            if (side == MatchSide.Left)
            {
                if (!leftHasEntry)
                {
                    leftEntry = digit;
                    leftHasEntry = true;
                }
                else if (leftEntry < 100)
                {
                    leftEntry = leftEntry * 10 + digit;
                }
            }
            else
            {
                if (!rightHasEntry)
                {
                    rightEntry = digit;
                    rightHasEntry = true;
                }
                else if (rightEntry < 100)
                {
                    rightEntry = rightEntry * 10 + digit;
                }
            }

            RefreshEntries();
        }

        private void ClearEntry(MatchSide side)
        {
            if (side == MatchSide.Left)
            {
                leftEntry = 0;
                leftHasEntry = false;
                leftEntryIsNegative = false;
            }
            else
            {
                rightEntry = 0;
                rightHasEntry = false;
                rightEntryIsNegative = false;
            }

            RefreshEntries();
        }

        private void ToggleEntrySign(MatchSide side)
        {
            if (side == MatchSide.Left)
            {
                leftEntryIsNegative = !leftEntryIsNegative;
            }
            else
            {
                rightEntryIsNegative = !rightEntryIsNegative;
            }

            RefreshEntries();
        }

        private void QueueSubmission(MatchSide side)
        {
            if (side == MatchSide.Left && leftHasEntry && !pendingLeftAnswer.HasValue)
            {
                pendingLeftAnswer = leftEntryIsNegative ? -leftEntry : leftEntry;
                leftHasEntry = false;
                leftEntry = 0;
                leftEntryIsNegative = false;
            }
            else if (side == MatchSide.Right && rightHasEntry && !pendingRightAnswer.HasValue)
            {
                pendingRightAnswer = rightEntryIsNegative ? -rightEntry : rightEntry;
                rightHasEntry = false;
                rightEntry = 0;
                rightEntryIsNegative = false;
            }

            RefreshEntries();
        }

        private void PresentSubmission(SubmissionResult submission)
        {
            PresentSideFeedback(MatchSide.Left, submission.Left);
            PresentSideFeedback(MatchSide.Right, submission.Right);
            RefreshProblems();
            SetPullingPose(null);

            if (submission.BalanceChanged)
            {
                pullDirection = match.Balance < Mathf.RoundToInt(ropeKnot.anchoredPosition.x / NumberPullBoardLayout.KnotStep) ? -1 : 1;
                pullAnimationRemaining = reducedMotion ? 0.18f : 0.65f;
                SetPullingPose(ResolvePullingSide(submission));
                ropeKnot.anchoredPosition = new Vector2(match.Balance * NumberPullBoardLayout.KnotStep, 0f);
                audio.Play(AudioCue.Pull, 0.46f);
            }
        }

        private static MatchSide? ResolvePullingSide(SubmissionResult submission)
        {
            if (submission.Left == SubmissionFeedback.Correct && submission.Right != SubmissionFeedback.Correct)
            {
                return MatchSide.Left;
            }

            if (submission.Right == SubmissionFeedback.Correct && submission.Left != SubmissionFeedback.Correct)
            {
                return MatchSide.Right;
            }

            return null;
        }

        private void SetPullingPose(MatchSide? pullingSide)
        {
            if (leftCharacterImage != null)
            {
                leftCharacterImage.sprite = pullingSide == MatchSide.Left
                    ? leftPullingCharacterSprite ?? leftNormalCharacterSprite
                    : leftNormalCharacterSprite;
            }

            if (rightCharacterImage != null)
            {
                rightCharacterImage.sprite = pullingSide == MatchSide.Right
                    ? rightPullingCharacterSprite ?? rightNormalCharacterSprite
                    : rightNormalCharacterSprite;
            }
        }

        private void PresentSideFeedback(MatchSide side, SubmissionFeedback feedback)
        {
            if (feedback == SubmissionFeedback.None)
            {
                return;
            }

            Text label = side == MatchSide.Left ? leftFeedback : rightFeedback;
            if (feedback == SubmissionFeedback.Correct)
            {
                label.text = "✓ CORRECT — PULL!";
                label.color = Success;
                audio.Play(AudioCue.Correct, 0.34f);
                SpawnParticles(side == MatchSide.Left ? -360f : 360f, false);
            }
            else if (feedback == SubmissionFeedback.Neutralized)
            {
                label.text = "◇ EVEN PULL";
                label.color = Purple;
                audio.Play(AudioCue.Correct, 0.26f);
            }
            else
            {
                label.text = "× TRY AGAIN";
                label.color = Error;
                audio.Play(AudioCue.Incorrect, 0.28f);
                if (side == MatchSide.Left)
                {
                    leftWrongRemaining = reducedMotion ? 0f : 0.35f;
                }
                else
                {
                    rightWrongRemaining = reducedMotion ? 0f : 0.35f;
                }
            }

            if (side == MatchSide.Left)
            {
                leftFeedbackRemaining = 1.15f;
            }
            else
            {
                rightFeedbackRemaining = 1.15f;
            }
        }

        private void UpdateFeedback(float delta)
        {
            if (leftFeedbackRemaining > 0f)
            {
                leftFeedbackRemaining -= delta;
                if (leftFeedbackRemaining <= 0f)
                {
                    leftFeedback.text = "SOLVE YOUR SIDE";
                    leftFeedback.color = TextMuted;
                }
            }

            if (rightFeedbackRemaining > 0f)
            {
                rightFeedbackRemaining -= delta;
                if (rightFeedbackRemaining <= 0f)
                {
                    rightFeedback.text = "SOLVE YOUR SIDE";
                    rightFeedback.color = TextMuted;
                }
            }

            leftWrongRemaining = Mathf.Max(0f, leftWrongRemaining - delta);
            rightWrongRemaining = Mathf.Max(0f, rightWrongRemaining - delta);
        }

        private void UpdateAnimation(float delta)
        {
            float time = Time.unscaledTime;
            float idle = reducedMotion ? 0f : Mathf.Sin(time * 2.2f) * 4f;
            float leftPull = 0f;
            float rightPull = 0f;
            if (pullAnimationRemaining > 0f)
            {
                pullAnimationRemaining = Mathf.Max(0f, pullAnimationRemaining - delta);
                float progress = 1f - pullAnimationRemaining / (reducedMotion ? 0.18f : 0.65f);
                float wave = Mathf.Sin(progress * Mathf.PI);
                float amount = (reducedMotion ? 5f : NumberPullBoardLayout.MaximumHorizontalMotion) * wave * pullDirection;
                leftPull = amount;
                rightPull = amount;
                if (pullAnimationRemaining <= 0f)
                {
                    SetPullingPose(null);
                }
            }

            leftAvatar.anchoredPosition = new Vector2(leftPull, idle);
            rightAvatar.anchoredPosition = new Vector2(rightPull, -idle);
            leftCard.anchoredPosition = new Vector2(
                leftWrongRemaining > 0f ? Mathf.Sin(leftWrongRemaining * 75f) * NumberPullBoardLayout.MaximumInputMotion : 0f,
                0f);
            rightCard.anchoredPosition = new Vector2(
                rightWrongRemaining > 0f ? Mathf.Sin(rightWrongRemaining * 75f) * NumberPullBoardLayout.MaximumInputMotion : 0f,
                0f);
        }

        private void SpawnParticles(float x, bool celebration)
        {
            SpawnParticles(new Vector2(x, celebration ? 50f : 160f), celebration);
        }

        private void SpawnParticles(Vector2 origin, bool celebration)
        {
            if (reducedMotion)
            {
                return;
            }

            int activeCount = celebration ? ParticleCount : 8;
            particleCanvas.sortingOrder = celebration ? ResultParticleSortingOrder : GameplayParticleSortingOrder;
            for (int index = 0; index < activeCount; index++)
            {
                Image particle = particles[index];
                bool cascade = celebration && index >= ResultBurstParticleCount;
                particle.gameObject.SetActive(true);
                particle.rectTransform.anchoredPosition = cascade ? GetResultCascadeOrigin(index) : origin;
                float horizontal = cascade
                    ? (float)(effectsRandom.NextDouble() * 160.0 - 80.0)
                    : (float)(effectsRandom.NextDouble() * 320.0 - 160.0);
                float vertical = cascade
                    ? -(float)(effectsRandom.NextDouble() * 150.0 + 130.0)
                    : (float)(effectsRandom.NextDouble() * 250.0 + 180.0);
                particleVelocity[index] = new Vector2(horizontal, vertical);
                particleLife[index] = celebration
                    ? (float)(effectsRandom.NextDouble() * 0.8 + 1.8)
                    : 0.8f;
                particle.color = index % 2 == 0 ? Purple : Orange;
                particle.sprite = celebration && resultConfettiStarSprite != null && index % 5 == 0
                    ? resultConfettiStarSprite
                    : index % 3 == 0 ? circleSprite : roundedSprite;
                float size = celebration
                    ? (float)(effectsRandom.NextDouble() * 18.0 + 14.0)
                    : 18f;
                particle.rectTransform.sizeDelta = new Vector2(size, size);
                particle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, (float)(effectsRandom.NextDouble() * 360.0));
            }
        }

        private Vector2 GetResultCascadeOrigin(int index)
        {
            RectTransform particleRect = particleCanvas.transform as RectTransform;
            if (particleRect == null)
            {
                return new Vector2(0f, 400f);
            }

            float cascadeProgress = (index - ResultBurstParticleCount + 0.5f) / (ParticleCount - ResultBurstParticleCount);
            float x = Mathf.Lerp(particleRect.rect.xMin * 0.92f, particleRect.rect.xMax * 0.92f, cascadeProgress);
            float y = particleRect.rect.yMax - (float)(effectsRandom.NextDouble() * particleRect.rect.height * 0.12);
            return new Vector2(x, y);
        }

        private void SpawnWinnerResultParticles(Image winnerImage, float fallbackX)
        {
            SpawnParticles(GetResultParticleOrigin(winnerImage, new Vector2(fallbackX, 50f)), true);
        }

        private Vector2 GetResultParticleOrigin(Image resultImage, Vector2 fallbackOrigin)
        {
            if (resultImage == null || !resultImage.isActiveAndEnabled || particleCanvas == null)
            {
                return fallbackOrigin;
            }

            RectTransform particleRect = particleCanvas.transform as RectTransform;
            if (particleRect == null)
            {
                return fallbackOrigin;
            }

            Canvas.ForceUpdateCanvases();
            Rect imageBounds = resultImage.rectTransform.rect;
            Vector2 upperTorsoPoint = new(
                imageBounds.center.x,
                Mathf.Lerp(imageBounds.yMin, imageBounds.yMax, 0.62f));
            Vector3 worldPoint = resultImage.rectTransform.TransformPoint(upperTorsoPoint);
            Camera uiCamera = particleCanvas.worldCamera;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPoint);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                particleRect,
                screenPoint,
                uiCamera,
                out Vector2 localPoint)
                ? localPoint
                : fallbackOrigin;
        }

        private void UpdateParticles(float delta)
        {
            if (reducedMotion)
            {
                return;
            }

            for (int index = 0; index < particles.Length; index++)
            {
                if (particleLife[index] <= 0f)
                {
                    continue;
                }

                particleLife[index] -= delta;
                if (particleLife[index] <= 0f)
                {
                    particles[index].gameObject.SetActive(false);
                    continue;
                }

                particleVelocity[index].y -= particleLife[index] > 1f ? 360f * delta : 480f * delta;
                particles[index].rectTransform.anchoredPosition += particleVelocity[index] * delta;
                particles[index].rectTransform.Rotate(0f, 0f, (index % 2 == 0 ? 180f : -180f) * delta);
            }
        }

        private void SetMuted(bool value)
        {
            ApplyMuted(value, true);
        }

        private void ApplyMuted(bool value, bool persist)
        {
            muted = value;
            audio.Muted = value;
            if (persist)
            {
                PlayerPrefs.SetInt(AudioMutedPreferenceKey, muted ? 1 : 0);
                PlayerPrefs.Save();
            }

            soundLabel.text = muted ? "SOUND OFF ×" : "SOUND ON ♪";
            if (soundControl != null)
            {
                soundControl.color = muted ? Error : SurfaceDeep;
            }

            if (!muted && IsActiveGameplay())
            {
                audio.StartMusic();
            }
        }

        private bool IsActiveGameplay()
        {
            return isActiveAndEnabled
                && matchStarted
                && !isPaused
                && match != null
                && !match.IsComplete
                && (difficultyOverlay == null || !difficultyOverlay.activeSelf)
                && (resultOverlay == null || !resultOverlay.activeSelf);
        }

        private void SetReducedMotion(bool value)
        {
            reducedMotion = value;
            motionLabel.text = reducedMotion ? "MOTION LOW −" : "MOTION ON ~";
            if (reducedMotion)
            {
                ResetTransientPresentation();
                CompleteResultEntrance();
            }
        }

        private void ResetTransientPresentation()
        {
            pullAnimationRemaining = 0f;
            SetPullingPose(null);
            leftWrongRemaining = 0f;
            rightWrongRemaining = 0f;
            ClearParticles();

            if (leftAvatar != null)
            {
                leftAvatar.anchoredPosition = Vector2.zero;
            }

            if (rightAvatar != null)
            {
                rightAvatar.anchoredPosition = Vector2.zero;
            }

            if (leftCard != null)
            {
                leftCard.anchoredPosition = Vector2.zero;
            }

            if (rightCard != null)
            {
                rightCard.anchoredPosition = Vector2.zero;
            }
        }

        private void ClearParticles()
        {
            if (particleCanvas != null)
            {
                particleCanvas.sortingOrder = GameplayParticleSortingOrder;
            }

            for (int index = 0; index < particles.Length; index++)
            {
                particleLife[index] = 0f;
                particleVelocity[index] = Vector2.zero;
                if (particles[index] != null)
                {
                    particles[index].rectTransform.anchoredPosition = Vector2.zero;
                    particles[index].rectTransform.localRotation = Quaternion.identity;
                    particles[index].gameObject.SetActive(false);
                }
            }
        }

        private RectTransform CreateSafeAreaRoot(RectTransform parent, string name)
        {
            GameObject item = new(name, typeof(RectTransform));
            item.transform.SetParent(parent, false);
            RectTransform rect = item.GetComponent<RectTransform>();
            safeAreaRoots.Add(rect);
            return rect;
        }

        private void UpdateSafeArea()
        {
            Rect area = Screen.safeArea;
            if (!safeAreaLayout.TryUpdate(
                    Screen.width,
                    Screen.height,
                    new LayoutRect(area.x, area.y, area.width, area.height),
                    out LayoutRect normalized))
            {
                return;
            }

            Vector2 minimum = new(normalized.X, normalized.Y);
            Vector2 maximum = new(normalized.X + normalized.Width, normalized.Y + normalized.Height);
            for (int index = 0; index < safeAreaRoots.Count; index++)
            {
                Anchor(safeAreaRoots[index], minimum, maximum);
            }
        }

        private void ConsumeResultIfReady()
        {
            if (!match.TryConsumeResult(out NumberPullResult result))
            {
                return;
            }

            ShowResult(result);
            ReportCompletedResult(result);
        }

        private void ShowResult(NumberPullResult result)
        {
            audio.StopMusic();
            ClearParticles();
            resultOverlay.SetActive(true);
            PresentResultCharacters(result.Outcome);
            StartResultEntrance();
            if (result.Outcome == MatchOutcome.LeftWins)
            {
                resultTitle.text = "PURPLE WINS!";
                resultTitle.color = Purple;
                audio.Play(AudioCue.Win, 0.52f);
                SpawnWinnerResultParticles(leftResultImage, -220f);
            }
            else if (result.Outcome == MatchOutcome.RightWins)
            {
                resultTitle.text = "ORANGE WINS!";
                resultTitle.color = Orange;
                audio.Play(AudioCue.Win, 0.52f);
                SpawnWinnerResultParticles(rightResultImage, 220f);
            }
            else
            {
                resultTitle.text = "BALANCED DRAW";
                resultTitle.color = TextLight;
                audio.Play(AudioCue.Draw, 0.46f);
                SpawnParticles(0f, true);
            }

            resultStats.text =
                $"PURPLE  ✓ {result.LeftStats.Correct} / {result.LeftStats.Attempts}\n" +
                $"ORANGE  ✓ {result.RightStats.Correct} / {result.RightStats.Attempts}\n\n" +
                $"FINAL BALANCE  {FormatBalance(result.Balance)}";
        }

        private void PresentResultCharacters(MatchOutcome outcome)
        {
            if (outcome == MatchOutcome.LeftWins)
            {
                ConfigureResultImage(leftResultImage, leftResultHalo, leftWinnerResultSprite ?? leftNormalCharacterSprite, new Vector2(0.01f, 0.16f), new Vector2(0.24f, 0.90f), false);
                ConfigureResultImage(rightResultImage, rightResultHalo, rightLoserResultSprite ?? rightNormalCharacterSprite, new Vector2(0.76f, 0.27f), new Vector2(0.96f, 0.76f), false);
                winnerResultImage = leftResultImage;
                winnerResultStartOffset = new Vector2(-32f, 0f);
                return;
            }

            if (outcome == MatchOutcome.RightWins)
            {
                ConfigureResultImage(leftResultImage, leftResultHalo, leftLoserResultSprite ?? leftNormalCharacterSprite, new Vector2(0.04f, 0.27f), new Vector2(0.24f, 0.76f), false);
                ConfigureResultImage(rightResultImage, rightResultHalo, rightWinnerResultSprite ?? rightNormalCharacterSprite, new Vector2(0.73f, 0.16f), new Vector2(0.99f, 0.90f), false);
                winnerResultImage = rightResultImage;
                winnerResultStartOffset = new Vector2(32f, 0f);
                return;
            }

            ConfigureResultImage(leftResultImage, leftResultHalo, leftNormalCharacterSprite, new Vector2(0.03f, 0.24f), new Vector2(0.24f, 0.82f), false);
            ConfigureResultImage(rightResultImage, rightResultHalo, rightNormalCharacterSprite, new Vector2(0.75f, 0.24f), new Vector2(0.97f, 0.82f), true);
            winnerResultImage = null;
            winnerResultStartOffset = Vector2.zero;
        }

        private static void ConfigureResultImage(Image image, Image halo, Sprite sprite, Vector2 minimum, Vector2 maximum, bool flipHorizontally)
        {
            image.sprite = sprite;
            Anchor(image.rectTransform, minimum, maximum);
            image.rectTransform.localScale = new Vector3(flipHorizontally ? -1f : 1f, 1f, 1f);
            image.gameObject.SetActive(sprite != null);
            halo.gameObject.SetActive(sprite != null);
        }

        private void StartResultEntrance()
        {
            if (reducedMotion)
            {
                CompleteResultEntrance();
                return;
            }

            resultEntranceRemaining = ResultEntranceDuration;
            resultCanvasGroup.alpha = 0f;
            resultCard.localScale = new Vector3(0.96f, 0.96f, 1f);
            SetResultCharacterEntrance(leftResultImage, leftResultCanvasGroup);
            SetResultCharacterEntrance(rightResultImage, rightResultCanvasGroup);
        }

        private void UpdateResultEntrance(float delta)
        {
            if (resultEntranceRemaining <= 0f || resultCanvasGroup == null)
            {
                return;
            }

            resultEntranceRemaining = Mathf.Max(0f, resultEntranceRemaining - delta);
            float progress = 1f - resultEntranceRemaining / ResultEntranceDuration;
            float eased = 1f - (1f - progress) * (1f - progress);
            resultCanvasGroup.alpha = eased;
            float scale = Mathf.Lerp(0.96f, 1f, eased);
            resultCard.localScale = new Vector3(scale, scale, 1f);
            UpdateResultCharacterEntrance(leftResultImage, leftResultCanvasGroup, eased);
            UpdateResultCharacterEntrance(rightResultImage, rightResultCanvasGroup, eased);
        }

        private void SetResultCharacterEntrance(Image image, CanvasGroup canvasGroup)
        {
            if (image == null || canvasGroup == null)
            {
                return;
            }

            bool isWinner = image == winnerResultImage;
            canvasGroup.alpha = isWinner ? 0f : 0.76f;
            image.rectTransform.anchoredPosition = isWinner ? winnerResultStartOffset : Vector2.zero;
            SetResultImageScale(image, isWinner ? 0.90f : 1f);
        }

        private void UpdateResultCharacterEntrance(Image image, CanvasGroup canvasGroup, float eased)
        {
            if (image == null || canvasGroup == null)
            {
                return;
            }

            bool isWinner = image == winnerResultImage;
            canvasGroup.alpha = isWinner ? eased : Mathf.Lerp(0.76f, 1f, eased);
            if (isWinner)
            {
                image.rectTransform.anchoredPosition = Vector2.Lerp(winnerResultStartOffset, Vector2.zero, eased);
                SetResultImageScale(image, Mathf.Lerp(0.90f, 1f, eased));
            }
        }

        private static void SetResultImageScale(Image image, float scale)
        {
            float direction = image.rectTransform.localScale.x < 0f ? -1f : 1f;
            image.rectTransform.localScale = new Vector3(direction * scale, scale, 1f);
        }

        private void CompleteResultEntrance()
        {
            resultEntranceRemaining = 0f;
            if (resultCanvasGroup != null)
            {
                resultCanvasGroup.alpha = 1f;
            }

            if (resultCard != null)
            {
                resultCard.localScale = Vector3.one;
            }

            CompleteResultCharacterEntrance(leftResultImage, leftResultCanvasGroup);
            CompleteResultCharacterEntrance(rightResultImage, rightResultCanvasGroup);
        }

        private static void CompleteResultCharacterEntrance(Image image, CanvasGroup canvasGroup)
        {
            if (image == null || canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = 1f;
            image.rectTransform.anchoredPosition = Vector2.zero;
            SetResultImageScale(image, 1f);
        }

        private void ResetResultPresentation()
        {
            CompleteResultEntrance();
            ClearParticles();
            ResetResultImage(leftResultImage);
            ResetResultImage(rightResultImage);
            ResetResultHalo(leftResultHalo);
            ResetResultHalo(rightResultHalo);
            winnerResultImage = null;
            winnerResultStartOffset = Vector2.zero;
            if (resultOverlay != null)
            {
                resultOverlay.SetActive(false);
            }
        }

        private static void ResetResultImage(Image image)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = null;
            Anchor(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            image.rectTransform.localScale = Vector3.one;
            image.rectTransform.localRotation = Quaternion.identity;
            image.gameObject.SetActive(false);
        }

        private static void ResetResultHalo(Image halo)
        {
            if (halo != null)
            {
                halo.gameObject.SetActive(false);
            }
        }

        private void ReportCompletedResult(NumberPullResult result)
        {
            if (resultReported)
            {
                return;
            }

            resultReported = true;
            int correct = result.LeftStats.Correct + result.RightStats.Correct;
            int attempts = result.LeftStats.Attempts + result.RightStats.Attempts;
            int accuracy = attempts == 0 ? 0 : Mathf.RoundToInt(correct * 100f / attempts);
            MiniGameResult aggregate = new(StableGameId, MiniGameCompletionState.Completed, accuracy, correct, attempts);
            services.GameLauncher.Complete(aggregate);
            Completed?.Invoke(aggregate);
        }

        private void ReturnToHub()
        {
            audio.Stop();
            ResetContactOwnership();
            pendingLeftAnswer = null;
            pendingRightAnswer = null;
            if (!resultReported && match != null)
            {
                PlayerStats left = match == null ? default : match.LeftStats;
                PlayerStats right = match == null ? default : match.RightStats;
                int correct = left.Correct + right.Correct;
                int attempts = left.Attempts + right.Attempts;
                MiniGameResult abandoned = new(StableGameId, MiniGameCompletionState.Abandoned, 0, correct, attempts);
                resultReported = true;
                services.GameLauncher.Complete(abandoned);
                Completed?.Invoke(abandoned);
            }

            services.GameLauncher.ShowLobby();
        }

        private void RefreshProblems()
        {
            leftProblem.text = match.LeftProblem.Format();
            rightProblem.text = match.RightProblem.Format();
        }

        private void RefreshEntries()
        {
            leftAnswer.text = FormatEntry(leftEntry, leftHasEntry, leftEntryIsNegative);
            rightAnswer.text = FormatEntry(rightEntry, rightHasEntry, rightEntryIsNegative);
        }

        private static string FormatEntry(int value, bool hasEntry, bool isNegative)
        {
            if (!hasEntry)
            {
                return isNegative ? "−" : "?";
            }

            return isNegative && value != 0 ? $"−{value}" : value.ToString();
        }

        private void UpdateTimer()
        {
            int seconds = Mathf.CeilToInt(match.RemainingSeconds);
            if (seconds == lastDisplayedSecond)
            {
                return;
            }

            lastDisplayedSecond = seconds;
            timerText.text = $"{seconds / 60}:{seconds % 60:00}";
            timerText.color = seconds <= 10 ? Error : TextLight;
            PlayCountdownWarning(seconds);
        }

        private void PlayCountdownWarning(int seconds)
        {
            if (!matchStarted || seconds < 1 || seconds > 5 || seconds == lastCountdownWarningSecond)
            {
                return;
            }

            lastCountdownWarningSecond = seconds;
            audio.Play(seconds == 1 ? AudioCue.Pull : AudioCue.Correct, seconds == 1 ? 0.30f : 0.22f);
        }

        private int FindContact(int fingerId)
        {
            for (int index = 0; index < contacts.Length; index++)
            {
                if (contacts[index].Active && contacts[index].FingerId == fingerId)
                {
                    return index;
                }
            }

            return -1;
        }

        private void ReleaseContact(int fingerId)
        {
            int index = FindContact(fingerId);
            if (index >= 0)
            {
                contacts[index] = default;
            }
        }

        private void ResetContactOwnership()
        {
            Array.Clear(contacts, 0, contacts.Length);
        }

        private void OpenPauseMenu()
        {
            if (pauseOverlay == null || pauseOverlay.activeSelf)
            {
                return;
            }

            ResetContactOwnership();
            pendingLeftAnswer = null;
            pendingRightAnswer = null;
            bool isPreMatchDifficultySelection = difficultyOverlay != null && difficultyOverlay.activeSelf;
            restartPauseAction.SetActive(!isPreMatchDifficultySelection);
            changeDifficultyPauseAction.SetActive(!isPreMatchDifficultySelection);
            isPaused = true;
            pauseOverlay.SetActive(true);
            audio.StopMusic();
            audio.Play(AudioCue.Tap, 0.3f);
        }

        private void ClosePauseMenu()
        {
            if (pauseOverlay == null || !pauseOverlay.activeSelf)
            {
                return;
            }

            ResetContactOwnership();
            pendingLeftAnswer = null;
            pendingRightAnswer = null;
            pauseOverlay.SetActive(false);
            isPaused = false;
            audio.Play(AudioCue.Tap, 0.3f);
            if (IsActiveGameplay())
            {
                audio.StartMusic();
            }
        }

        private void CreateProceduralSprites()
        {
            roundedTexture = CreateShapeTexture(false);
            circleTexture = CreateShapeTexture(true);
            roundedSprite = Sprite.Create(roundedTexture, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(18f, 18f, 18f, 18f));
            circleSprite = Sprite.Create(circleTexture, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 100f);
            roundedSprite.name = "NumberPullRounded";
            circleSprite.name = "NumberPullCircle";
        }

        private static Texture2D CreateShapeTexture(bool circle)
        {
            const int size = 64;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                name = circle ? "NumberPullCircleTexture" : "NumberPullRoundedTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            Color32[] pixels = new Color32[size * size];
            float radius = circle ? 31f : 14f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = circle ? x - 31.5f : Mathf.Max(0f, Mathf.Abs(x - 31.5f) - (31.5f - radius));
                    float dy = circle ? y - 31.5f : Mathf.Max(0f, Mathf.Abs(y - 31.5f) - (31.5f - radius));
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt((radius + 0.5f - distance) * 255f), 0, 255);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private Image CreateImage(RectTransform parent, string name, Color color, Sprite sprite)
        {
            GameObject item = new(name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            Image image = item.GetComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.type = sprite == roundedSprite ? Image.Type.Sliced : Image.Type.Simple;
            return image;
        }

        private Text CreateText(RectTransform parent, string name, int size, TextAnchor alignment, Color color)
        {
            GameObject item = new(name, typeof(RectTransform), typeof(Text));
            item.transform.SetParent(parent, false);
            Text text = item.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16;
            text.resizeTextMaxSize = size;
            text.raycastTarget = false;
            return text;
        }

        private static void Anchor(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Stretch(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        private static string FormatBalance(int balance)
        {
            if (balance == 0)
            {
                return "CENTER";
            }

            return balance < 0 ? $"PURPLE {Math.Abs(balance)}" : $"ORANGE {balance}";
        }

        private static Color Hex(uint rgb)
        {
            return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
        }

        private static Color Tint(Color color, float whiteAmount)
        {
            return Color.Lerp(color, Color.white, whiteAmount);
        }

        private enum TouchAction
        {
            Digit,
            Clear,
            Submit,
            ToggleSign,
            ToggleSound,
            ToggleMotion,
            Rematch,
            ChangeDifficulty,
            SelectDifficulty,
            Hub,
            Restart,
            Continue
        }

        private readonly struct TouchTarget
        {
            public TouchTarget(RectTransform rect, MatchSide? side, TouchAction action, int value)
            {
                Rect = rect;
                Side = side;
                Action = action;
                Value = value;
            }

            public RectTransform Rect { get; }
            public MatchSide? Side { get; }
            public TouchAction Action { get; }
            public int Value { get; }
        }

        private readonly struct ContactOwnership
        {
            public ContactOwnership(bool active, int fingerId, MatchSide? side, TouchAction action)
            {
                Active = active;
                FingerId = fingerId;
                Side = side;
                Action = action;
            }

            public bool Active { get; }
            public int FingerId { get; }
            public MatchSide? Side { get; }
            public TouchAction Action { get; }
        }

        private enum AudioCue
        {
            Tap,
            Correct,
            Incorrect,
            Pull,
            Win,
            Draw
        }

        private sealed class RuntimeAudio : IDisposable
        {
            private const int SampleRate = 22050;
            private const string CorrectResourcePath = "Audio/number-pull-correct";
            private const string PullResourcePath = "Audio/number-pull-rope-pull";
            private const string WinResourcePath = "Audio/number-pull-win";
            private const string MusicResourcePath = "Audio/number-pull-background";
            private readonly AudioSource[] sources = new AudioSource[4];
            private readonly AudioClip[] clips = new AudioClip[6];
            private readonly bool[] ownsClips = new bool[6];
            private readonly float[] lastPlayed = new float[6];
            private readonly Func<string, AudioClip> resourceLoader;
            private readonly AudioSource musicSource;
            private readonly AudioClip musicClip;
            private int sourceIndex;

            public RuntimeAudio(GameObject owner, Func<string, AudioClip> resourceLoader = null)
            {
                this.resourceLoader = resourceLoader ?? Resources.Load<AudioClip>;
                for (int index = 0; index < sources.Length; index++)
                {
                    sources[index] = owner.AddComponent<AudioSource>();
                    sources[index].playOnAwake = false;
                    sources[index].loop = false;
                    sources[index].mute = false;
                    sources[index].volume = 1f;
                    sources[index].spatialBlend = 0f;
                }

                musicSource = owner.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
                musicSource.mute = false;
                musicSource.volume = 0.12f;
                musicSource.spatialBlend = 0f;
                musicClip = this.resourceLoader(MusicResourcePath);
                musicSource.clip = musicClip;

                clips[(int)AudioCue.Tap] = CreateTone("NP Tap", 0.055f, 520f, 760f, 0.25f);
                ownsClips[(int)AudioCue.Tap] = true;
                clips[(int)AudioCue.Correct] = LoadCue(
                    AudioCue.Correct,
                    CorrectResourcePath,
                    () => CreateTone("NP Correct", 0.16f, 520f, 920f, 0.28f));
                clips[(int)AudioCue.Incorrect] = CreateTone("NP Incorrect", 0.14f, 210f, 135f, 0.24f);
                ownsClips[(int)AudioCue.Incorrect] = true;
                clips[(int)AudioCue.Pull] = LoadCue(
                    AudioCue.Pull,
                    PullResourcePath,
                    () => CreateTone("NP Pull", 0.20f, 120f, 230f, 0.30f));
                clips[(int)AudioCue.Win] = LoadCue(AudioCue.Win, WinResourcePath, CreateWinFanfare);
                clips[(int)AudioCue.Draw] = CreateTone("NP Draw", 0.38f, 420f, 420f, 0.24f);
                ownsClips[(int)AudioCue.Draw] = true;
            }

            private bool muted;

            public bool Muted
            {
                get => muted;
                set
                {
                    muted = value;
                    for (int index = 0; index < sources.Length; index++)
                    {
                        sources[index].mute = muted;
                    }

                    musicSource.mute = muted;
                    if (muted)
                    {
                        Stop();
                    }
                }
            }

            public void Play(AudioCue cue, float volume)
            {
                if (Muted)
                {
                    return;
                }

                int cueIndex = (int)cue;
                float now = Time.unscaledTime;
                float limit = cue == AudioCue.Tap ? 0.035f : 0.075f;
                if (now - lastPlayed[cueIndex] < limit)
                {
                    return;
                }

                lastPlayed[cueIndex] = now;
                AudioSource source = sources[sourceIndex++ % sources.Length];
                source.PlayOneShot(clips[cueIndex], volume);
            }

            public void Stop()
            {
                for (int index = 0; index < sources.Length; index++)
                {
                    sources[index].Stop();
                }

                StopMusic();
            }

            public void StartMusic()
            {
                if (!Muted && musicClip != null && !musicSource.isPlaying)
                {
                    musicSource.Play();
                }
            }

            public void StopMusic()
            {
                musicSource.Stop();
            }

            public void Dispose()
            {
                Stop();
                for (int index = 0; index < clips.Length; index++)
                {
                    if (ownsClips[index] && clips[index] != null)
                    {
                        UnityEngine.Object.Destroy(clips[index]);
                    }
                }
            }

            private AudioClip LoadCue(AudioCue cue, string resourcePath, Func<AudioClip> fallback)
            {
                AudioClip importedClip = resourceLoader(resourcePath);
                if (importedClip != null)
                {
                    return importedClip;
                }

                ownsClips[(int)cue] = true;
                return fallback();
            }

            private static AudioClip CreateTone(string name, float duration, float startFrequency, float endFrequency, float amplitude)
            {
                int sampleCount = Mathf.CeilToInt(duration * SampleRate);
                float[] samples = new float[sampleCount];
                float phase = 0f;
                for (int index = 0; index < sampleCount; index++)
                {
                    float progress = index / (float)Math.Max(1, sampleCount - 1);
                    float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
                    phase += frequency * Mathf.PI * 2f / SampleRate;
                    float attack = Mathf.Clamp01(progress / 0.08f);
                    float release = Mathf.Clamp01((1f - progress) / 0.25f);
                    samples[index] = Mathf.Sin(phase) * amplitude * attack * release;
                }

                AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
                clip.SetData(samples, 0);
                return clip;
            }

            private static AudioClip CreateWinFanfare()
            {
                const float duration = 0.78f;
                int sampleCount = Mathf.CeilToInt(duration * SampleRate);
                float[] samples = new float[sampleCount];
                for (int index = 0; index < sampleCount; index++)
                {
                    float time = index / (float)SampleRate;
                    samples[index] =
                        FanfareVoice(time, 0f, 0.58f, 523.25f, 0.20f) +
                        FanfareVoice(time, 0.14f, 0.54f, 659.25f, 0.17f) +
                        FanfareVoice(time, 0.28f, 0.50f, 783.99f, 0.15f) +
                        FanfareVoice(time, 0.40f, 0.38f, 1046.50f, 0.08f);
                }

                AudioClip clip = AudioClip.Create("NP Warm Win Fanfare", sampleCount, 1, SampleRate, false);
                clip.SetData(samples, 0);
                return clip;
            }

            private static float FanfareVoice(float time, float start, float duration, float frequency, float amplitude)
            {
                float localTime = time - start;
                if (localTime < 0f || localTime >= duration)
                {
                    return 0f;
                }

                float attack = Mathf.Clamp01(localTime / 0.035f);
                float release = Mathf.Clamp01((duration - localTime) / 0.18f);
                float phase = localTime * frequency * Mathf.PI * 2f;
                float warmTone = Mathf.Sin(phase) + Mathf.Sin(phase * 2f) * 0.16f;
                return warmTone * amplitude * attack * release;
            }
        }
    }
}
