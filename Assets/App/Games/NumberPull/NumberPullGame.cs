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
        private const int MaximumOwnedContacts = 32;
        private const int ParticleCount = 28;

        private static readonly Color Purple = Hex(0x9448F4);
        private static readonly Color Orange = Hex(0xFFB740);
        private static readonly Color Ink = Hex(0x241A35);
        private static readonly Color CanvasColor = Hex(0xF7F5FA);
        private static readonly Color Surface = Color.white;
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
        private Texture2D roundedTexture;
        private Texture2D circleTexture;
        private RuntimeAudio audio;
        private System.Random effectsRandom;

        private RectTransform leftCard;
        private RectTransform rightCard;
        private RectTransform leftAvatar;
        private RectTransform rightAvatar;
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
        private Text soundLabel;
        private Text motionLabel;
        private GameObject resultOverlay;

        private int leftEntry;
        private int rightEntry;
        private bool leftHasEntry;
        private bool rightHasEntry;
        private int? pendingLeftAnswer;
        private int? pendingRightAnswer;
        private int matchIndex;
        private int lastDisplayedSecond = -1;
        private float countdownRemaining;
        private float leftFeedbackRemaining;
        private float rightFeedbackRemaining;
        private float leftWrongRemaining;
        private float rightWrongRemaining;
        private float pullAnimationRemaining;
        private int pullDirection;
        private bool interfaceBuilt;
        private bool matchStarted;
        private bool resultReported;
        private bool muted;
        private bool reducedMotion;

        public string GameId => StableGameId;
        public bool IsCompleted => match != null && match.IsComplete;
        public event Action<MiniGameResult> Completed;

        public void Configure(AppServices appServices)
        {
            services = appServices ?? throw new ArgumentNullException(nameof(appServices));
            if (!interfaceBuilt)
            {
                BuildInterface();
                interfaceBuilt = true;
            }

            StartMatch();
        }

        private void Update()
        {
            if (!interfaceBuilt)
            {
                return;
            }

            float delta = Time.unscaledDeltaTime;
            UpdateSafeArea();
            ProcessContacts();
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
            effectsRandom = new System.Random(deterministicSeed == 0 ? 72491 : deterministicSeed);
            if (!TryGetComponent(out AudioListener _))
            {
                gameObject.AddComponent<AudioListener>();
            }

            audio = new RuntimeAudio(gameObject);

            RectTransform root = canvas.GetComponent<RectTransform>();
            Image background = CreateImage(root, "NeutralCanvas", CanvasColor, null);
            Stretch(background.rectTransform, 0f);
            background.raycastTarget = false;

            CreateDecorativeDots(root);

            RectTransform safeRoot = CreateSafeAreaRoot(root, "SafeAreaRoot");

            Text title = CreateText(safeRoot, "Title", 52, TextAnchor.MiddleCenter, Ink);
            title.text = "NUMBER PULL";
            Anchor(title.rectTransform, new Vector2(0.31f, 0.905f), new Vector2(0.69f, 0.985f));

            Text instruction = CreateText(safeRoot, "Instruction", 24, TextAnchor.MiddleCenter, new Color(Ink.r, Ink.g, Ink.b, 0.75f));
            instruction.text = "Solve. Submit. Pull the marker five steps.";
            Anchor(instruction.rectTransform, new Vector2(0.30f, 0.855f), new Vector2(0.70f, 0.91f));

            soundLabel = CreateUtilityControl(safeRoot, "SoundControl", "SOUND ON", new Vector2(0.025f, 0.91f), new Vector2(0.17f, 0.975f), TouchAction.ToggleSound);
            motionLabel = CreateUtilityControl(safeRoot, "MotionControl", "MOTION ON", new Vector2(0.83f, 0.91f), new Vector2(0.975f, 0.975f), TouchAction.ToggleMotion);

            leftCard = BuildPlayerCard(safeRoot, MatchSide.Left, new Vector2(0.025f, 0.075f), new Vector2(0.375f, 0.84f));
            rightCard = BuildPlayerCard(safeRoot, MatchSide.Right, new Vector2(0.625f, 0.075f), new Vector2(0.975f, 0.84f));

            BuildCenterStage(safeRoot);
            BuildResultOverlay(root);
            UpdateSafeArea();
        }

        private RectTransform BuildPlayerCard(RectTransform root, MatchSide side, Vector2 min, Vector2 max)
        {
            bool isLeft = side == MatchSide.Left;
            Image shadow = CreateImage(root, isLeft ? "LeftShadow" : "RightShadow", new Color(Ink.r, Ink.g, Ink.b, 0.10f), roundedSprite);
            Anchor(shadow.rectTransform, min + new Vector2(0.005f, -0.008f), max + new Vector2(0.005f, -0.008f));
            shadow.raycastTarget = false;

            Image card = CreateImage(root, isLeft ? "LeftCard" : "RightCard", Surface, roundedSprite);
            Anchor(card.rectTransform, min, max);
            card.raycastTarget = false;

            Image accent = CreateImage(card.rectTransform, "Accent", isLeft ? Purple : Orange, roundedSprite);
            Anchor(accent.rectTransform, new Vector2(0.035f, 0.91f), new Vector2(0.965f, 0.98f));
            accent.raycastTarget = false;

            Text sideName = CreateText(card.rectTransform, "SideName", 28, TextAnchor.MiddleCenter, isLeft ? Color.white : Ink);
            sideName.text = isLeft ? "PURPLE SIDE" : "ORANGE SIDE";
            Anchor(sideName.rectTransform, new Vector2(0.07f, 0.90f), new Vector2(0.93f, 0.985f));

            Text problem = CreateText(card.rectTransform, "Problem", 70, TextAnchor.MiddleCenter, Ink);
            Anchor(problem.rectTransform, new Vector2(0.07f, 0.72f), new Vector2(0.93f, 0.89f));

            Image answerSurface = CreateImage(card.rectTransform, "AnswerSurface", isLeft ? Tint(Purple, 0.91f) : Tint(Orange, 0.84f), roundedSprite);
            Anchor(answerSurface.rectTransform, new Vector2(0.18f, 0.61f), new Vector2(0.82f, 0.72f));
            answerSurface.raycastTarget = false;

            Text answer = CreateText(answerSurface.rectTransform, "Answer", 48, TextAnchor.MiddleCenter, Ink);
            answer.text = "?";
            Stretch(answer.rectTransform, 8f);

            Text feedback = CreateText(card.rectTransform, "Feedback", 25, TextAnchor.MiddleCenter, Ink);
            feedback.text = "READY";
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
            int[] values = { 1, 2, 3, 4, 5, 6, 7, 8, 9, -1, 0, -2 };
            string[] labels = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "CLEAR", "0", "GO" };
            for (int index = 0; index < values.Length; index++)
            {
                int row = index / 3;
                int column = index % 3;
                float minX = 0.07f + column * 0.30f;
                float maxX = minX + 0.26f;
                float maxY = 0.52f - row * 0.125f;
                float minY = maxY - 0.10f;
                bool submit = values[index] == -2;
                bool clear = values[index] == -1;
                Color color = submit ? Orange : clear ? Tint(Purple, 0.88f) : CanvasColor;

                Image button = CreateImage(card, labels[index] + "Key", color, roundedSprite);
                Anchor(button.rectTransform, new Vector2(minX, minY), new Vector2(maxX, maxY));
                button.raycastTarget = false;

                Text label = CreateText(button.rectTransform, "Label", submit || clear ? 24 : 34, TextAnchor.MiddleCenter, Ink);
                label.text = labels[index];
                Stretch(label.rectTransform, 5f);

                TouchAction action = submit ? TouchAction.Submit : clear ? TouchAction.Clear : TouchAction.Digit;
                touchTargets.Add(new TouchTarget(button.rectTransform, side, action, Math.Max(0, values[index])));
            }
        }

        private void BuildCenterStage(RectTransform root)
        {
            Image stage = CreateImage(root, "CenterStage", Tint(Purple, 0.94f), roundedSprite);
            Anchor(stage.rectTransform, new Vector2(NumberPullBoardLayout.CentralStageLeft, 0.075f), new Vector2(NumberPullBoardLayout.CentralStageRight, 0.84f));
            stage.raycastTarget = false;

            timerText = CreateText(stage.rectTransform, "Timer", 50, TextAnchor.MiddleCenter, Ink);
            timerText.text = "1:30";
            Anchor(timerText.rectTransform, new Vector2(0.10f, 0.865f), new Vector2(0.90f, 0.97f));

            Text goal = CreateText(stage.rectTransform, "Goal", 22, TextAnchor.MiddleCenter, Ink);
            goal.text = "FIRST TO 5";
            Anchor(goal.rectTransform, new Vector2(0.10f, 0.81f), new Vector2(0.90f, 0.87f));

            GameObject animationLayerObject = new("AnimationCanvas", typeof(RectTransform), typeof(Canvas));
            animationLayerObject.transform.SetParent(root, false);
            RectTransform animationLayer = animationLayerObject.GetComponent<RectTransform>();
            Stretch(animationLayer, 0f);
            Canvas animationCanvas = animationLayerObject.GetComponent<Canvas>();
            animationCanvas.overrideSorting = true;
            animationCanvas.sortingOrder = 2;

            Image rope = CreateImage(animationLayer, "Rope", Ink, roundedSprite);
            Anchor(rope.rectTransform, new Vector2(NumberPullBoardLayout.RopeLeftAnchor, 0.405f), new Vector2(NumberPullBoardLayout.RopeRightAnchor, 0.422f));
            rope.raycastTarget = false;

            for (int step = -5; step <= 5; step++)
            {
                Image tick = CreateImage(animationLayer, "Step" + step, step == 0 ? Orange : new Color(Ink.r, Ink.g, Ink.b, 0.25f), circleSprite);
                tick.rectTransform.anchorMin = new Vector2(0.5f, 0.4135f);
                tick.rectTransform.anchorMax = new Vector2(0.5f, 0.4135f);
                tick.rectTransform.sizeDelta = step == 0 ? new Vector2(24f, 24f) : new Vector2(13f, 13f);
                tick.rectTransform.anchoredPosition = new Vector2(step * NumberPullBoardLayout.KnotStep, 0f);
                tick.raycastTarget = false;
            }

            Image knot = CreateImage(animationLayer, "RopeMarker", Orange, circleSprite);
            ropeKnot = knot.rectTransform;
            ropeKnot.anchorMin = new Vector2(0.5f, 0.4135f);
            ropeKnot.anchorMax = new Vector2(0.5f, 0.4135f);
            ropeKnot.sizeDelta = new Vector2(NumberPullBoardLayout.KnotDiameter, NumberPullBoardLayout.KnotDiameter);
            ropeKnot.anchoredPosition = Vector2.zero;
            knot.raycastTarget = false;
            Text marker = CreateText(ropeKnot, "MarkerIcon", 28, TextAnchor.MiddleCenter, Ink);
            marker.text = "5";
            Stretch(marker.rectTransform, 4f);

            leftAvatar = CreateAvatar(
                animationLayer,
                "PurplePuller",
                Purple,
                new Vector2(NumberPullBoardLayout.LeftCharacterAnchor, 0.46f),
                false);
            rightAvatar = CreateAvatar(
                animationLayer,
                "OrangePuller",
                Orange,
                new Vector2(NumberPullBoardLayout.RightCharacterAnchor, 0.46f),
                true);

            countdownText = CreateText(animationLayer, "Countdown", 112, TextAnchor.MiddleCenter, Ink);
            Anchor(countdownText.rectTransform, new Vector2(0.39f, 0.48f), new Vector2(0.61f, 0.69f));

            for (int index = 0; index < ParticleCount; index++)
            {
                Image particle = CreateImage(animationLayer, "Confetti" + index, index % 2 == 0 ? Purple : Orange, index % 3 == 0 ? circleSprite : roundedSprite);
                particle.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                particle.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                particle.rectTransform.sizeDelta = new Vector2(18f, 18f);
                particle.gameObject.SetActive(false);
                particle.raycastTarget = false;
                particles[index] = particle;
            }
        }

        private RectTransform CreateAvatar(RectTransform parent, string name, Color color, Vector2 anchor, bool facesLeft)
        {
            GameObject avatarObject = new(name, typeof(RectTransform));
            avatarObject.transform.SetParent(parent, false);
            RectTransform avatar = avatarObject.GetComponent<RectTransform>();
            avatar.anchorMin = anchor;
            avatar.anchorMax = anchor;
            avatar.sizeDelta = new Vector2(NumberPullBoardLayout.CharacterWidth, NumberPullBoardLayout.CharacterHeight);
            avatar.anchoredPosition = Vector2.zero;

            Image body = CreateImage(avatar, "Body", color, roundedSprite);
            Anchor(body.rectTransform, new Vector2(0.20f, 0.02f), new Vector2(0.80f, 0.62f));
            body.raycastTarget = false;

            Image head = CreateImage(avatar, "Head", Tint(color, 0.64f), circleSprite);
            Anchor(head.rectTransform, new Vector2(0.25f, 0.53f), new Vector2(0.75f, 0.95f));
            head.raycastTarget = false;

            float eyeX = facesLeft ? 0.37f : 0.56f;
            Image eye = CreateImage(head.rectTransform, "Eye", Ink, circleSprite);
            Anchor(eye.rectTransform, new Vector2(eyeX, 0.50f), new Vector2(eyeX + 0.13f, 0.63f));
            eye.raycastTarget = false;

            Image arm = CreateImage(avatar, "RopeArm", color, roundedSprite);
            Anchor(arm.rectTransform, facesLeft ? new Vector2(-0.12f, 0.34f) : new Vector2(0.62f, 0.34f), facesLeft ? new Vector2(0.38f, 0.47f) : new Vector2(1.12f, 0.47f));
            arm.rectTransform.localRotation = Quaternion.Euler(0f, 0f, facesLeft ? 13f : -13f);
            arm.raycastTarget = false;

            Image badge = CreateImage(body.rectTransform, "Badge", Orange, circleSprite);
            Anchor(badge.rectTransform, new Vector2(0.34f, 0.40f), new Vector2(0.66f, 0.67f));
            badge.raycastTarget = false;
            Text plus = CreateText(badge.rectTransform, "Plus", 26, TextAnchor.MiddleCenter, Ink);
            plus.text = "+";
            Stretch(plus.rectTransform, 2f);

            return avatar;
        }

        private void BuildResultOverlay(RectTransform root)
        {
            Image dim = CreateImage(root, "ResultOverlay", new Color(Ink.r, Ink.g, Ink.b, 0.82f), null);
            Stretch(dim.rectTransform, 0f);
            Canvas resultCanvas = dim.gameObject.AddComponent<Canvas>();
            resultCanvas.overrideSorting = true;
            resultCanvas.sortingOrder = 3;
            resultOverlay = dim.gameObject;

            RectTransform safeRoot = CreateSafeAreaRoot(dim.rectTransform, "ResultSafeArea");
            Image card = CreateImage(safeRoot, "ResultCard", Surface, roundedSprite);
            Anchor(card.rectTransform, new Vector2(0.27f, 0.18f), new Vector2(0.73f, 0.82f));
            card.raycastTarget = false;

            resultTitle = CreateText(card.rectTransform, "ResultTitle", 64, TextAnchor.MiddleCenter, Ink);
            Anchor(resultTitle.rectTransform, new Vector2(0.08f, 0.74f), new Vector2(0.92f, 0.93f));

            resultStats = CreateText(card.rectTransform, "ResultStats", 31, TextAnchor.MiddleCenter, Ink);
            Anchor(resultStats.rectTransform, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.74f));

            Image rematch = CreateImage(card.rectTransform, "Rematch", Purple, roundedSprite);
            Anchor(rematch.rectTransform, new Vector2(0.08f, 0.12f), new Vector2(0.47f, 0.31f));
            rematch.raycastTarget = false;
            Text rematchText = CreateText(rematch.rectTransform, "Label", 30, TextAnchor.MiddleCenter, Color.white);
            rematchText.text = "REMATCH ↻";
            Stretch(rematchText.rectTransform, 8f);
            touchTargets.Add(new TouchTarget(rematch.rectTransform, null, TouchAction.Rematch, 0));

            Image hub = CreateImage(card.rectTransform, "BackToHub", Orange, roundedSprite);
            Anchor(hub.rectTransform, new Vector2(0.53f, 0.12f), new Vector2(0.92f, 0.31f));
            hub.raycastTarget = false;
            Text hubText = CreateText(hub.rectTransform, "Label", 30, TextAnchor.MiddleCenter, Ink);
            hubText.text = "HUB ⌂";
            Stretch(hubText.rectTransform, 8f);
            touchTargets.Add(new TouchTarget(hub.rectTransform, null, TouchAction.Hub, 0));

            resultOverlay.SetActive(false);
        }

        private Text CreateUtilityControl(RectTransform root, string name, string text, Vector2 min, Vector2 max, TouchAction action)
        {
            Image control = CreateImage(root, name, Surface, roundedSprite);
            Anchor(control.rectTransform, min, max);
            control.raycastTarget = false;
            Text label = CreateText(control.rectTransform, "Label", 20, TextAnchor.MiddleCenter, Ink);
            label.text = text;
            Stretch(label.rectTransform, 7f);
            touchTargets.Add(new TouchTarget(control.rectTransform, null, action, 0));
            return label;
        }

        private void CreateDecorativeDots(RectTransform root)
        {
            for (int index = 0; index < 12; index++)
            {
                Image dot = CreateImage(root, "BackgroundDot" + index, index % 2 == 0 ? new Color(Purple.r, Purple.g, Purple.b, 0.10f) : new Color(Orange.r, Orange.g, Orange.b, 0.14f), circleSprite);
                float x = 0.03f + (index * 0.083f) % 0.94f;
                float y = index % 3 == 0 ? 0.03f : 0.86f + (index % 2) * 0.04f;
                dot.rectTransform.anchorMin = new Vector2(x, y);
                dot.rectTransform.anchorMax = new Vector2(x, y);
                dot.rectTransform.sizeDelta = new Vector2(24f + index % 4 * 9f, 24f + index % 4 * 9f);
                dot.raycastTarget = false;
            }
        }

        private void StartMatch()
        {
            CancelInvoke(nameof(HideCountdown));
            ResetTransientPresentation();
            int seed = deterministicSeed == 0 ? Environment.TickCount : deterministicSeed;
            seed += matchIndex * 101;
            matchIndex++;
            match = new NumberPullMatch(new MathProblemGenerator(seed), new MathProblemGenerator(seed + 1));
            leftEntry = 0;
            rightEntry = 0;
            leftHasEntry = false;
            rightHasEntry = false;
            pendingLeftAnswer = null;
            pendingRightAnswer = null;
            resultReported = false;
            matchStarted = false;
            countdownRemaining = CountdownDuration;
            lastDisplayedSecond = -1;
            pullAnimationRemaining = 0f;
            ropeKnot.anchoredPosition = Vector2.zero;
            resultOverlay.SetActive(false);
            countdownText.gameObject.SetActive(true);
            leftFeedback.text = "READY";
            rightFeedback.text = "READY";
            leftFeedback.color = Ink;
            rightFeedback.color = Ink;
            RefreshProblems();
            RefreshEntries();
            UpdateTimer();
            ResetContactOwnership();
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
            for (int index = touchTargets.Count - 1; index >= 0; index--)
            {
                TouchTarget target = touchTargets[index];
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

        private void HandleTarget(TouchTarget target)
        {
            if (target.Action == TouchAction.ToggleSound)
            {
                SetMuted(!muted);
                if (!muted)
                {
                    audio.Play(AudioCue.Tap, 0.28f);
                }

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
                else if (leftEntry < 10)
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
                else if (rightEntry < 10)
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
            }
            else
            {
                rightEntry = 0;
                rightHasEntry = false;
            }

            RefreshEntries();
        }

        private void QueueSubmission(MatchSide side)
        {
            if (side == MatchSide.Left && leftHasEntry && !pendingLeftAnswer.HasValue)
            {
                pendingLeftAnswer = leftEntry;
                leftHasEntry = false;
                leftEntry = 0;
            }
            else if (side == MatchSide.Right && rightHasEntry && !pendingRightAnswer.HasValue)
            {
                pendingRightAnswer = rightEntry;
                rightHasEntry = false;
                rightEntry = 0;
            }

            RefreshEntries();
        }

        private void PresentSubmission(SubmissionResult submission)
        {
            PresentSideFeedback(MatchSide.Left, submission.Left);
            PresentSideFeedback(MatchSide.Right, submission.Right);
            RefreshProblems();

            if (submission.BalanceChanged)
            {
                pullDirection = match.Balance < Mathf.RoundToInt(ropeKnot.anchoredPosition.x / NumberPullBoardLayout.KnotStep) ? -1 : 1;
                pullAnimationRemaining = reducedMotion ? 0.18f : 0.65f;
                ropeKnot.anchoredPosition = new Vector2(match.Balance * NumberPullBoardLayout.KnotStep, 0f);
                audio.Play(AudioCue.Pull, 0.46f);
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
                    leftFeedback.color = Ink;
                }
            }

            if (rightFeedbackRemaining > 0f)
            {
                rightFeedbackRemaining -= delta;
                if (rightFeedbackRemaining <= 0f)
                {
                    rightFeedback.text = "SOLVE YOUR SIDE";
                    rightFeedback.color = Ink;
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
            if (reducedMotion)
            {
                return;
            }

            int activeCount = celebration ? ParticleCount : 8;
            for (int index = 0; index < activeCount; index++)
            {
                Image particle = particles[index];
                particle.gameObject.SetActive(true);
                particle.rectTransform.anchoredPosition = new Vector2(x, celebration ? 50f : 160f);
                float horizontal = (float)(effectsRandom.NextDouble() * 320.0 - 160.0);
                float vertical = (float)(effectsRandom.NextDouble() * 250.0 + 180.0);
                particleVelocity[index] = new Vector2(horizontal, vertical);
                particleLife[index] = celebration ? 2.2f : 0.8f;
                particle.color = index % 2 == 0 ? Purple : Orange;
            }
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

                particleVelocity[index].y -= 480f * delta;
                particles[index].rectTransform.anchoredPosition += particleVelocity[index] * delta;
                particles[index].rectTransform.Rotate(0f, 0f, (index % 2 == 0 ? 180f : -180f) * delta);
            }
        }

        private void SetMuted(bool value)
        {
            muted = value;
            audio.Muted = value;
            soundLabel.text = muted ? "SOUND OFF ×" : "SOUND ON ♪";
        }

        private void SetReducedMotion(bool value)
        {
            reducedMotion = value;
            motionLabel.text = reducedMotion ? "MOTION LOW −" : "MOTION ON ~";
            if (reducedMotion)
            {
                ResetTransientPresentation();
            }
        }

        private void ResetTransientPresentation()
        {
            pullAnimationRemaining = 0f;
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
            for (int index = 0; index < particles.Length; index++)
            {
                particleLife[index] = 0f;
                particleVelocity[index] = Vector2.zero;
                if (particles[index] != null)
                {
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
            resultOverlay.SetActive(true);
            if (result.Outcome == MatchOutcome.LeftWins)
            {
                resultTitle.text = "PURPLE WINS!";
                resultTitle.color = Purple;
                audio.Play(AudioCue.Win, 0.52f);
                SpawnParticles(-220f, true);
            }
            else if (result.Outcome == MatchOutcome.RightWins)
            {
                resultTitle.text = "ORANGE WINS!";
                resultTitle.color = Hex(0xB86600);
                audio.Play(AudioCue.Win, 0.52f);
                SpawnParticles(220f, true);
            }
            else
            {
                resultTitle.text = "BALANCED DRAW";
                resultTitle.color = Ink;
                audio.Play(AudioCue.Draw, 0.46f);
                SpawnParticles(0f, true);
            }

            resultStats.text =
                $"PURPLE  ✓ {result.LeftStats.Correct} / {result.LeftStats.Attempts}\n" +
                $"ORANGE  ✓ {result.RightStats.Correct} / {result.RightStats.Attempts}\n\n" +
                $"FINAL BALANCE  {FormatBalance(result.Balance)}";
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
            if (!resultReported)
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
            leftAnswer.text = leftHasEntry ? leftEntry.ToString() : "?";
            rightAnswer.text = rightHasEntry ? rightEntry.ToString() : "?";
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
            timerText.color = seconds <= 10 ? Error : Ink;
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
            ToggleSound,
            ToggleMotion,
            Rematch,
            Hub
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
            private readonly AudioSource[] sources = new AudioSource[4];
            private readonly AudioClip[] clips = new AudioClip[6];
            private readonly float[] lastPlayed = new float[6];
            private int sourceIndex;

            public RuntimeAudio(GameObject owner)
            {
                for (int index = 0; index < sources.Length; index++)
                {
                    sources[index] = owner.AddComponent<AudioSource>();
                    sources[index].playOnAwake = false;
                    sources[index].loop = false;
                    sources[index].mute = false;
                    sources[index].volume = 1f;
                    sources[index].spatialBlend = 0f;
                }

                clips[(int)AudioCue.Tap] = CreateTone("NP Tap", 0.055f, 520f, 760f, 0.25f);
                clips[(int)AudioCue.Correct] = CreateTone("NP Correct", 0.16f, 520f, 920f, 0.28f);
                clips[(int)AudioCue.Incorrect] = CreateTone("NP Incorrect", 0.14f, 210f, 135f, 0.24f);
                clips[(int)AudioCue.Pull] = CreateTone("NP Pull", 0.20f, 120f, 230f, 0.30f);
                clips[(int)AudioCue.Win] = CreateTone("NP Win", 0.52f, 360f, 980f, 0.27f);
                clips[(int)AudioCue.Draw] = CreateTone("NP Draw", 0.38f, 420f, 420f, 0.24f);
            }

            private bool muted;

            public bool Muted
            {
                get => muted;
                set
                {
                    muted = value;
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
            }

            public void Dispose()
            {
                Stop();
                for (int index = 0; index < clips.Length; index++)
                {
                    if (clips[index] != null)
                    {
                        UnityEngine.Object.Destroy(clips[index]);
                    }
                }
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
        }
    }
}
