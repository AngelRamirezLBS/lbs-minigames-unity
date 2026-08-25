using System;
using System.Collections;
using System.Reflection;
using Lbs.MiniGames.Games.NumberPull.Domain;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lbs.MiniGames.Games.NumberPull.Tests
{
    public sealed class NumberPullPlayModeTests
    {
        [UnityTest]
        public IEnumerator BootstrapConfiguresBuiltNumberPullSceneAcrossLifecycle()
        {
            PlayerPrefs.DeleteKey("math.number-pull.audio-muted");
            yield return SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);

            for (int frame = 0; frame < 30 && SceneManager.GetActiveScene().name != "Lobby"; frame++)
            {
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Lobby"));
            yield return SceneManager.LoadSceneAsync("NumberPull", LoadSceneMode.Single);

            for (int frame = 0; frame < 30 && GameObject.Find("LeftCard") == null; frame++)
            {
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("NumberPull"));
            Assert.That(GameObject.Find("LeftCard"), Is.Not.Null);
            Assert.That(GameObject.Find("RightCard"), Is.Not.Null);
            Assert.That(GameObject.Find("RopeMarker"), Is.Not.Null);
            Assert.That(GameObject.Find("EventSystem"), Is.Null, "Gameplay input must not have a parallel EventSystem route.");

            GameObject gameObject = GameObject.Find("NumberPullGame");
            Assert.That(gameObject, Is.Not.Null);
            AssertSingleActiveListenerOwnedBy(gameObject);
            AudioSource[] sources = gameObject.GetComponents<AudioSource>();
            Assert.That(sources, Has.Length.EqualTo(5));
            for (int index = 0; index < 4; index++)
            {
                AudioSource source = sources[index];
                Assert.That(source.enabled, Is.True);
                Assert.That(source.playOnAwake, Is.False);
                Assert.That(source.loop, Is.False);
                Assert.That(source.mute, Is.False);
                Assert.That(source.volume, Is.EqualTo(1f));
                Assert.That(source.spatialBlend, Is.EqualTo(0f));
            }

            AudioSource music = sources[4];
            Assert.That(music.enabled, Is.True);
            Assert.That(music.playOnAwake, Is.False);
            Assert.That(music.loop, Is.True);
            Assert.That(music.mute, Is.False);
            Assert.That(music.volume, Is.EqualTo(0.12f));
            Assert.That(music.spatialBlend, Is.EqualTo(0f));
            Assert.That(music.clip, Is.SameAs(Resources.Load<AudioClip>("Audio/number-pull-background")));
            Assert.That(music.clip.length, Is.EqualTo(44.651f).Within(0.05f));

            Component game = gameObject.GetComponent("NumberPullGame");
            object runtimeAudio = GetField(game, "audio");
            Array clips = (Array)runtimeAudio.GetType()
                .GetField("clips", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(runtimeAudio);
            Assert.That(clips, Has.Length.EqualTo(6));
            foreach (AudioClip clip in clips)
            {
                Assert.That(clip, Is.Not.Null);
                Assert.That(clip.samples, Is.GreaterThan(0));
            }

            AssertImportedAudioCue(clips, 1, "Audio/number-pull-correct", 0.1f, 0.35f);
            AssertImportedAudioCue(clips, 3, "Audio/number-pull-rope-pull", 0.15f, 0.5f);
            AssertImportedAudioCue(clips, 4, "Audio/number-pull-win", 0.5f, 1.2f);

            Canvas animationCanvas = GameObject.Find("AnimationCanvas").GetComponent<Canvas>();
            Canvas particleCanvas = GameObject.Find("ParticleCanvas").GetComponent<Canvas>();
            Canvas resultCanvas = FindLoadedTransform("ResultOverlay").GetComponent<Canvas>();
            Assert.That(resultCanvas.overrideSorting, Is.True);
            Assert.That(resultCanvas.sortingOrder, Is.GreaterThan(animationCanvas.sortingOrder));
            Assert.That(particleCanvas.overrideSorting, Is.True);
            Assert.That(particleCanvas.sortingOrder, Is.EqualTo(animationCanvas.sortingOrder));
            Assert.That(particleCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>(), Is.Null);

            gameObject.SetActive(false);
            yield return null;
            Assert.That(ActiveAudioListeners(), Is.Empty);
            gameObject.SetActive(true);
            yield return null;

            AssertSingleActiveListenerOwnedBy(gameObject);
            Assert.That(GameObject.Find("SafeAreaRoot"), Is.Not.Null);
            Assert.That(GameObject.Find("SoundControl").transform.parent.name, Is.EqualTo("SafeAreaRoot"));
            Assert.That(FindLoadedTransform("ResultSafeArea"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator ReloadingNumberPullReplacesRatherThanDuplicatesItsAudioListener()
        {
            yield return LoadNumberPull();
            GameObject originalOwner = GameObject.Find("NumberPullGame");
            AudioListener originalListener = AssertSingleActiveListenerOwnedBy(originalOwner);

            yield return SceneManager.LoadSceneAsync("NumberPull", LoadSceneMode.Single);
            for (int frame = 0; frame < 30 && GameObject.Find("LeftCard") == null; frame++)
            {
                yield return null;
            }

            GameObject replacementOwner = GameObject.Find("NumberPullGame");
            AudioListener replacementListener = AssertSingleActiveListenerOwnedBy(replacementOwner);
            Assert.That(replacementOwner, Is.Not.SameAs(originalOwner));
            Assert.That(replacementListener, Is.Not.SameAs(originalListener));
        }

        [UnityTest]
        public IEnumerator ReducedMotionClearsParticlesAndPreventsNewParticles()
        {
            yield return LoadNumberPull();
            Component game = GetGame();

            Invoke(game, "SpawnParticles", 0f, true);
            Assert.That(ActiveConfettiCount(), Is.GreaterThan(0));

            Invoke(game, "SetReducedMotion", true);
            Assert.That(ActiveConfettiCount(), Is.Zero);

            Invoke(game, "SpawnParticles", 0f, true);
            Assert.That(ActiveConfettiCount(), Is.Zero);

            Invoke(game, "SetReducedMotion", false);
            Assert.That(ActiveConfettiCount(), Is.Zero);
        }

        [UnityTest]
        public IEnumerator MutingStopsCurrentlyPlayingAudio()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            AudioSource source = game.GetComponent<AudioSource>();
            AudioClip clip = AudioClip.Create("Number Pull mute test", 44100, 1, 44100, false);
            source.clip = clip;
            source.loop = true;
            source.Play();
            yield return null;
            Assert.That(source.isPlaying, Is.True);

            Invoke(game, "SetMuted", true);

            Assert.That(source.isPlaying, Is.False);
            Invoke(game, "SetMuted", false);
            PlayerPrefs.DeleteKey("math.number-pull.audio-muted");
            UnityEngine.Object.Destroy(clip);
        }

        [UnityTest]
        public IEnumerator BackgroundMusicFollowsGameplayLifecycleAndPersistsMutePreference()
        {
            const string preferenceKey = "math.number-pull.audio-muted";
            PlayerPrefs.DeleteKey(preferenceKey);
            yield return LoadNumberPull();
            Component game = GetGame();
            object runtimeAudio = GetField(game, "audio");
            AudioSource music = GetRuntimeMusicSource(runtimeAudio);

            Assert.That(music.isPlaying, Is.False, "Music must remain stopped in the difficulty lobby.");
            Invoke(game, "SetMuted", true);
            Invoke(game, "SelectDifficulty", NumberPullDifficultyTier.LowerPrimary);
            Assert.That(music.isPlaying, Is.False, "Music must remain stopped during the countdown.");
            Invoke(game, "SetMuted", false);
            Assert.That(PlayerPrefs.GetInt(preferenceKey), Is.Zero);
            Assert.That(music.isPlaying, Is.False, "Unmuting during the countdown must not start music.");

            Invoke(game, "UpdateCountdown", 5f);
            yield return null;
            Assert.That(music.isPlaying, Is.True, "Music must start only once gameplay begins.");

            Invoke(game, "OpenPauseMenu");
            Assert.That(music.isPlaying, Is.False, "Music must stop while gameplay is paused.");
            Invoke(game, "ClosePauseMenu");
            Assert.That(music.isPlaying, Is.True, "Music must resume after active gameplay resumes.");

            Invoke(game, "SetMuted", true);
            Assert.That(PlayerPrefs.GetInt(preferenceKey), Is.EqualTo(1));
            Assert.That(music.isPlaying, Is.False);
            Assert.That(GameObject.Find("SoundControl").transform.Find("Label").GetComponent<UnityEngine.UI.Text>().text, Is.EqualTo("SOUND OFF ×"));
            foreach (AudioSource source in game.GetComponents<AudioSource>())
            {
                Assert.That(source.mute, Is.True);
            }

            Invoke(game, "SetMuted", false);
            Assert.That(PlayerPrefs.GetInt(preferenceKey), Is.Zero);
            Assert.That(music.isPlaying, Is.True, "Unmuting during active gameplay must resume music.");

            Invoke(game, "ShowResult", CreateResult(MatchOutcome.Draw, 0));
            Assert.That(music.isPlaying, Is.False);
            Invoke(game, "SetMuted", true);
            Invoke(game, "SetMuted", false);
            Assert.That(music.isPlaying, Is.False, "Unmuting on the result screen must not resume music.");

            Invoke(game, "ShowDifficultySelector");
            Assert.That(music.isPlaying, Is.False);
            Assert.That(GameObject.Find("SoundControl").transform.Find("Label").GetComponent<UnityEngine.UI.Text>().text, Is.EqualTo("SOUND ON ♪"));
            PlayerPrefs.DeleteKey(preferenceKey);
        }

        [UnityTest]
        public IEnumerator BackgroundMusicStopsBeforeReturningToHub()
        {
            PlayerPrefs.DeleteKey("math.number-pull.audio-muted");
            yield return LoadNumberPull();
            Component game = GetGame();
            AudioSource music = GetRuntimeMusicSource(GetField(game, "audio"));

            Invoke(game, "SelectDifficulty", NumberPullDifficultyTier.LowerPrimary);
            Invoke(game, "UpdateCountdown", 5f);
            yield return null;
            Assert.That(music.isPlaying, Is.True);

            Invoke(game, "ReturnToHub");
            Assert.That(music.isPlaying, Is.False);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Lobby"));
        }

        [UnityTest]
        public IEnumerator CountdownWarningUsesFourRegularTicksThenOneDistinctFinalTick()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            ((Behaviour)game).enabled = false;
            NumberPullMatch match = ConfigureWarningTimer(game, 6f, 0f);
            object runtimeAudio = GetField(game, "audio");
            int initialPlays = GetRuntimeAudioSourceIndex(runtimeAudio);

            Invoke(game, "UpdateTimer");
            match.Tick(1.1f);
            Invoke(game, "UpdateTimer");
            Assert.That(GetRuntimeAudioSourceIndex(runtimeAudio), Is.EqualTo(initialPlays + 1));

            for (int seconds = 4; seconds >= 2; seconds--)
            {
                yield return new WaitForSecondsRealtime(0.1f);
                match.Tick(1f);
                Invoke(game, "UpdateTimer");
                Assert.That(GetRuntimeAudioSourceIndex(runtimeAudio), Is.EqualTo(initialPlays + (5 - seconds) + 1));
            }

            yield return new WaitForSecondsRealtime(0.1f);
            match.Tick(1f);
            Invoke(game, "UpdateTimer");

            Assert.That(GetRuntimeAudioSourceIndex(runtimeAudio), Is.EqualTo(initialPlays + 5));
            Assert.That(GetRuntimeAudioLastPlayed(runtimeAudio, 1), Is.GreaterThan(0f), "Seconds 5 through 2 must use the short regular warning tick.");
            Assert.That(GetRuntimeAudioLastPlayed(runtimeAudio, 3), Is.GreaterThan(0f), "Second 1 must use the distinct final warning tick.");
        }

        [UnityTest]
        public IEnumerator CountdownWarningDoesNotDuplicateWhilePausedAndResetsForRematch()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            ((Behaviour)game).enabled = false;
            NumberPullMatch match = ConfigureWarningTimer(game, 6f, 1.1f);
            object runtimeAudio = GetField(game, "audio");
            int initialPlays = GetRuntimeAudioSourceIndex(runtimeAudio);

            Invoke(game, "UpdateTimer");
            Invoke(game, "UpdateTimer");
            Assert.That(GetRuntimeAudioSourceIndex(runtimeAudio), Is.EqualTo(initialPlays + 1));

            SetField(game, "isPaused", true);
            Invoke(game, "Update");
            SetField(game, "isPaused", false);
            Invoke(game, "Update");
            Assert.That(GetRuntimeAudioSourceIndex(runtimeAudio), Is.EqualTo(initialPlays + 1), "Pause/resume must not replay an already announced second.");

            yield return new WaitForSecondsRealtime(0.1f);
            Invoke(game, "StartMatch");
            ConfigureWarningTimer(game, 6f, 1.1f);
            Invoke(game, "UpdateTimer");

            Assert.That(GetRuntimeAudioSourceIndex(runtimeAudio), Is.EqualTo(initialPlays + 2), "A rematch must have its own warning cadence.");
        }

        [UnityTest]
        public IEnumerator CountdownWarningHonorsMuteButNotReducedMotion()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            ((Behaviour)game).enabled = false;
            object runtimeAudio = GetField(game, "audio");
            int initialPlays = GetRuntimeAudioSourceIndex(runtimeAudio);

            ConfigureWarningTimer(game, 6f, 1.1f);
            Invoke(game, "SetMuted", true);
            Invoke(game, "UpdateTimer");
            Assert.That(GetRuntimeAudioSourceIndex(runtimeAudio), Is.EqualTo(initialPlays));
            Assert.That((int)GetField(game, "lastCountdownWarningSecond"), Is.EqualTo(5));

            Invoke(game, "SetMuted", false);
            Invoke(game, "UpdateTimer");
            Assert.That(GetRuntimeAudioSourceIndex(runtimeAudio), Is.EqualTo(initialPlays), "Unmuting must not replay an expired warning.");

            yield return new WaitForSecondsRealtime(0.1f);
            ConfigureWarningTimer(game, 6f, 2.1f);
            Invoke(game, "SetReducedMotion", true);
            Invoke(game, "UpdateTimer");

            Assert.That((bool)GetField(game, "muted"), Is.False);
            Assert.That(GetRuntimeAudioSourceIndex(runtimeAudio), Is.EqualTo(initialPlays + 1), "Reduced motion must preserve countdown audio.");
        }

        [UnityTest]
        public IEnumerator RuntimeAudioFallsBackToGeneratedCuesWhenResourcesAreUnavailable()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            object sceneRuntimeAudio = GetField(game, "audio");
            Type runtimeAudioType = sceneRuntimeAudio.GetType();
            ConstructorInfo constructor = runtimeAudioType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(GameObject), typeof(Func<string, AudioClip>) },
                null);
            Assert.That(constructor, Is.Not.Null);

            GameObject fallbackOwner = new("NumberPullGeneratedAudioFallback");
            object fallbackAudio = constructor.Invoke(new object[]
            {
                fallbackOwner,
                new Func<string, AudioClip>(_ => null)
            });
            Array fallbackClips = (Array)runtimeAudioType
                .GetField("clips", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(fallbackAudio);

            Assert.That(((AudioClip)fallbackClips.GetValue(1)).name, Is.EqualTo("NP Correct"));
            Assert.That(((AudioClip)fallbackClips.GetValue(3)).name, Is.EqualTo("NP Pull"));
            Assert.That(((AudioClip)fallbackClips.GetValue(4)).name, Is.EqualTo("NP Warm Win Fanfare"));

            runtimeAudioType.GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public).Invoke(fallbackAudio, null);
            UnityEngine.Object.Destroy(fallbackOwner);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DisableAndReenableClearsTransientPresentationAndContactOwnership()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            Transform leftCard = GameObject.Find("LeftCard").transform;
            RectTransform leftKey = FindChild(leftCard, "1Key").GetComponent<RectTransform>();
            Vector2 keyCenter = RectTransformUtility.WorldToScreenPoint(null, leftKey.TransformPoint(leftKey.rect.center));

            Invoke(game, "SpawnParticles", 0f, true);
            leftCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(30f, 15f);
            Invoke(game, "TryBeginContact", 73, keyCenter);
            Assert.That(HasOwnedContact(game), Is.True);
            Invoke(game, "ShowResult", CreateResult(MatchOutcome.Draw, 0));

            game.gameObject.SetActive(false);
            yield return null;
            game.gameObject.SetActive(true);
            yield return null;

            Assert.That(ActiveConfettiCount(), Is.Zero);
            Assert.That(leftCard.GetComponent<RectTransform>().anchoredPosition, Is.EqualTo(Vector2.zero));
            Assert.That(HasOwnedContact(game), Is.False);
            Assert.That(FindLoadedTransform("ResultOverlay").gameObject.activeSelf, Is.False);
            Assert.That(FindLoadedTransform("PurpleResultCharacter").gameObject.activeSelf, Is.False);
            Assert.That(FindLoadedTransform("OrangeResultCharacter").gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator ResultIsPresentedOnceAndRematchResetsPresentationState()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            object match = GetField(game, "match");
            Type matchType = match.GetType();

            for (int index = 0; index < 5; index++)
            {
                object problem = matchType.GetProperty("LeftProblem").GetValue(match);
                int answer = (int)problem.GetType().GetProperty("Answer").GetValue(problem);
                matchType.GetMethod("Submit").Invoke(match, new object[] { answer, null });
            }

            Invoke(game, "ConsumeResultIfReady");
            GameObject resultOverlay = GameObject.Find("ResultOverlay");
            Assert.That(resultOverlay, Is.Not.Null);
            Assert.That(resultOverlay.activeSelf, Is.True);
            string presentedTitle = GameObject.Find("ResultTitle").GetComponent<UnityEngine.UI.Text>().text;

            Invoke(game, "ConsumeResultIfReady");
            Assert.That(GameObject.Find("ResultTitle").GetComponent<UnityEngine.UI.Text>().text, Is.EqualTo(presentedTitle));

            Invoke(game, "StartMatch");
            Assert.That(resultOverlay.activeSelf, Is.False);
            Assert.That((bool)game.GetType().GetProperty("IsCompleted").GetValue(game), Is.False);
            Assert.That(GameObject.Find("Answer").GetComponent<UnityEngine.UI.Text>().text, Is.EqualTo("?"));
            Assert.That(ActiveConfettiCount(), Is.Zero);
            Assert.That(FindLoadedTransform("PurpleResultCharacter").gameObject.activeSelf, Is.False);
            Assert.That(FindLoadedTransform("OrangeResultCharacter").gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator ResultArtworkMapsBothWinnersAndDrawWithReadableNonInteractiveLayout()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            UnityEngine.UI.Image purple = FindLoadedTransform("PurpleResultCharacter").GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image orange = FindLoadedTransform("OrangeResultCharacter").GetComponent<UnityEngine.UI.Image>();
            RectTransform resultSafeArea = FindLoadedTransform("ResultSafeArea");
            RectTransform resultCard = FindLoadedTransform("ResultCard");

            Assert.That(purple.rectTransform.parent, Is.SameAs(resultSafeArea));
            Assert.That(orange.rectTransform.parent, Is.SameAs(resultSafeArea));
            Assert.That(resultCard.parent, Is.SameAs(resultSafeArea));
            Assert.That(resultCard.anchorMin, Is.EqualTo(new Vector2(0.30f, 0.18f)));
            Assert.That(resultCard.anchorMax, Is.EqualTo(new Vector2(0.70f, 0.82f)));

            Invoke(game, "ShowResult", CreateResult(MatchOutcome.LeftWins, -5));
            CanvasGroup purpleEntrance = purple.GetComponent<CanvasGroup>();
            CanvasGroup orangeEntrance = orange.GetComponent<CanvasGroup>();
            Assert.That(purpleEntrance.alpha, Is.Zero);
            Assert.That(purple.rectTransform.anchoredPosition.x, Is.LessThan(0f));
            Assert.That(Mathf.Abs(purple.rectTransform.localScale.x), Is.EqualTo(0.90f).Within(0.001f));
            Assert.That(orangeEntrance.alpha, Is.EqualTo(0.76f).Within(0.001f));
            Assert.That(orange.rectTransform.anchoredPosition, Is.EqualTo(Vector2.zero));
            Invoke(game, "CompleteResultEntrance");
            Canvas.ForceUpdateCanvases();
            Assert.That(purple.sprite, Is.SameAs(Resources.Load<Sprite>("Characters/PurpleWinnerCelebration")));
            Assert.That(orange.sprite, Is.SameAs(Resources.Load<Sprite>("Characters/OrangeLoserResult")));
            Assert.That(WorldRect(purple.rectTransform).width * WorldRect(purple.rectTransform).height,
                Is.GreaterThan(WorldRect(orange.rectTransform).width * WorldRect(orange.rectTransform).height));
            Assert.That(purple.rectTransform.anchorMax.x, Is.LessThanOrEqualTo(resultCard.anchorMin.x + 0.02f));
            Assert.That(orange.rectTransform.anchorMin.x, Is.GreaterThanOrEqualTo(resultCard.anchorMax.x));
            AssertResultArtworkAndCopyRemainSeparated(purple, orange);

            Invoke(game, "ShowResult", CreateResult(MatchOutcome.RightWins, 5));
            Invoke(game, "CompleteResultEntrance");
            Canvas.ForceUpdateCanvases();
            Assert.That(purple.sprite, Is.SameAs(Resources.Load<Sprite>("Characters/PurpleLoserResult")));
            Assert.That(orange.sprite, Is.SameAs(Resources.Load<Sprite>("Characters/OrangeWinnerCelebration")));
            Assert.That(WorldRect(orange.rectTransform).width * WorldRect(orange.rectTransform).height,
                Is.GreaterThan(WorldRect(purple.rectTransform).width * WorldRect(purple.rectTransform).height));
            AssertResultArtworkAndCopyRemainSeparated(purple, orange);

            Invoke(game, "ShowResult", CreateResult(MatchOutcome.Draw, 0));
            Invoke(game, "CompleteResultEntrance");
            Canvas.ForceUpdateCanvases();
            Assert.That(purple.sprite, Is.SameAs(Resources.Load<Sprite>("Characters/PurpleCrewCharacter")));
            Assert.That(orange.sprite, Is.SameAs(Resources.Load<Sprite>("Characters/OrangeCrewCharacter")));
            Assert.That(orange.rectTransform.localScale.x, Is.EqualTo(-1f));
            AssertResultArtworkAndCopyRemainSeparated(purple, orange);
        }

        [UnityTest]
        public IEnumerator CompactResultModalContainsInteractiveHubAndRematchActions()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            RectTransform resultCard = FindLoadedTransform("ResultCard");
            RectTransform rematch = FindLoadedTransform("Rematch");
            RectTransform hub = FindLoadedTransform("BackToHub");

            Assert.That(resultCard.anchorMax.x - resultCard.anchorMin.x, Is.EqualTo(0.40f).Within(0.0001f));
            Assert.That(resultCard.anchorMax.y - resultCard.anchorMin.y, Is.EqualTo(0.64f).Within(0.0001f));
            Assert.That(hub.IsChildOf(resultCard), Is.True);
            AssertResultCardHasHubAndRematchTargets(game, resultCard);

            Invoke(game, "ShowResult", CreateResult(MatchOutcome.Draw, 0));
            Canvas.ForceUpdateCanvases();
            Vector2 rematchCenter = RectTransformUtility.WorldToScreenPoint(null, rematch.TransformPoint(rematch.rect.center));
            Invoke(game, "TryBeginContact", 99, rematchCenter);

            Assert.That(FindLoadedTransform("ResultOverlay").gameObject.activeSelf, Is.False);
            Assert.That((bool)game.GetType().GetProperty("IsCompleted").GetValue(game), Is.False);

            Invoke(game, "ShowResult", CreateResult(MatchOutcome.Draw, 0));
            TapTarget(game, "BackToHub", 100);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Lobby"));
        }

        [UnityTest]
        public IEnumerator MissingDedicatedResultArtworkFallsBackAndResetClearsOutcomeState()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            UnityEngine.UI.Image purple = FindLoadedTransform("PurpleResultCharacter").GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image orange = FindLoadedTransform("OrangeResultCharacter").GetComponent<UnityEngine.UI.Image>();
            SetField(game, "leftWinnerResultSprite", null);

            Invoke(game, "PresentResultCharacters", MatchOutcome.LeftWins);

            Assert.That(purple.sprite, Is.SameAs(Resources.Load<Sprite>("Characters/PurpleCrewCharacter")));
            Assert.That(purple.gameObject.activeSelf, Is.True);
            Assert.That(orange.gameObject.activeSelf, Is.True);

            SetField(game, "leftNormalCharacterSprite", null);
            Invoke(game, "PresentResultCharacters", MatchOutcome.LeftWins);
            Assert.That(purple.gameObject.activeSelf, Is.False, "If dedicated and normal art are both missing, the slot must hide safely.");

            Invoke(game, "ResetResultPresentation");
            Assert.That(purple.sprite, Is.Null);
            Assert.That(orange.sprite, Is.Null);
            Assert.That(purple.gameObject.activeSelf, Is.False);
            Assert.That(orange.gameObject.activeSelf, Is.False);
            Assert.That(purple.rectTransform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(orange.rectTransform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(purple.rectTransform.sizeDelta, Is.EqualTo(Vector2.zero));
            Assert.That(orange.rectTransform.sizeDelta, Is.EqualTo(Vector2.zero));
        }

        [UnityTest]
        public IEnumerator CelebrationParticlesRenderAboveResultsAndResetToGameplayLayer()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            Canvas particles = GameObject.Find("ParticleCanvas").GetComponent<Canvas>();
            Canvas results = FindLoadedTransform("ResultOverlay").GetComponent<Canvas>();
            Canvas animation = GameObject.Find("AnimationCanvas").GetComponent<Canvas>();

            Invoke(game, "ShowResult", CreateResult(MatchOutcome.LeftWins, -5));

            Assert.That(ActiveConfettiCount(), Is.EqualTo(28));
            Assert.That(particles.sortingOrder, Is.GreaterThan(results.sortingOrder));
            Assert.That(particles.GetComponent<UnityEngine.UI.GraphicRaycaster>(), Is.Null);
            Assert.That(GameObject.Find("Confetti0").GetComponent<UnityEngine.UI.Image>().sprite,
                Is.SameAs(Resources.Load<Sprite>("Particles/kenney-star-01")));
            Assert.That(GameObject.Find("Confetti0").GetComponent<RectTransform>().sizeDelta.x,
                Is.InRange(14f, 32f));
            Assert.That(GameObject.Find("Confetti27").GetComponent<RectTransform>().anchoredPosition.y,
                Is.GreaterThan(0f), "Result celebration needs a top cascade in addition to the winner burst.");

            Invoke(game, "UpdateParticles", 0.1f);
            RectTransform firstParticle = GameObject.Find("Confetti0").GetComponent<RectTransform>();
            Assert.That(firstParticle.localRotation, Is.Not.EqualTo(Quaternion.identity));
            Invoke(game, "ResetResultPresentation");
            Assert.That(ActiveConfettiCount(), Is.Zero);
            Assert.That(particles.sortingOrder, Is.EqualTo(animation.sortingOrder));
            Assert.That(firstParticle.anchoredPosition, Is.EqualTo(Vector2.zero));
            Assert.That(firstParticle.localRotation, Is.EqualTo(Quaternion.identity));
        }

        [UnityTest]
        public IEnumerator ResultCelebrationsFollowWinnerArtworkAndDrawStaysNeutral()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            RectTransform particleCanvas = GameObject.Find("ParticleCanvas").GetComponent<RectTransform>();
            RectTransform firstParticle = GameObject.Find("Confetti0").GetComponent<RectTransform>();
            UnityEngine.UI.Image purple = FindLoadedTransform("PurpleResultCharacter").GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image orange = FindLoadedTransform("OrangeResultCharacter").GetComponent<UnityEngine.UI.Image>();

            Invoke(game, "ShowResult", CreateResult(MatchOutcome.LeftWins, -5));
            Canvas.ForceUpdateCanvases();
            Assert.That(firstParticle.parent, Is.SameAs(particleCanvas));
            AssertParticleOriginMatchesResultArtwork(firstParticle, purple.rectTransform);
            Vector2 purpleOrigin = firstParticle.anchoredPosition;

            particleCanvas.localScale = new Vector3(1.13f, 0.87f, 1f);
            Invoke(game, "ShowResult", CreateResult(MatchOutcome.RightWins, 5));
            Canvas.ForceUpdateCanvases();
            AssertParticleOriginMatchesResultArtwork(firstParticle, orange.rectTransform);
            Vector2 orangeOrigin = firstParticle.anchoredPosition;
            Assert.That(Mathf.Abs(purpleOrigin.x - orangeOrigin.x), Is.GreaterThan(1f));

            Invoke(game, "ShowResult", CreateResult(MatchOutcome.Draw, 0));
            Assert.That(firstParticle.anchoredPosition, Is.EqualTo(new Vector2(0f, 50f)));

            particleCanvas.localScale = Vector3.one;
            Invoke(game, "ResetResultPresentation");
            Assert.That(ActiveConfettiCount(), Is.Zero);
            Assert.That(firstParticle.anchoredPosition, Is.EqualTo(Vector2.zero));
        }

        [UnityTest]
        public IEnumerator ReducedMotionShowsFinalResultStateWithoutEntranceOrParticles()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            Invoke(game, "SetReducedMotion", true);

            Invoke(game, "ShowResult", CreateResult(MatchOutcome.RightWins, 5));

            CanvasGroup group = FindLoadedTransform("ResultCard").GetComponent<CanvasGroup>();
            UnityEngine.UI.Image winner = FindLoadedTransform("OrangeResultCharacter").GetComponent<UnityEngine.UI.Image>();
            CanvasGroup winnerGroup = winner.GetComponent<CanvasGroup>();
            Assert.That(group.alpha, Is.EqualTo(1f));
            Assert.That(group.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(winnerGroup.alpha, Is.EqualTo(1f));
            Assert.That(winner.rectTransform.anchoredPosition, Is.EqualTo(Vector2.zero));
            Assert.That(Mathf.Abs(winner.rectTransform.localScale.x), Is.EqualTo(1f));
            Assert.That(ActiveConfettiCount(), Is.Zero);
        }

        [UnityTest]
        public IEnumerator DifficultyMustBeSelectedBeforeAMatchCanStart()
        {
            yield return SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);
            for (int frame = 0; frame < 30 && SceneManager.GetActiveScene().name != "Lobby"; frame++)
            {
                yield return null;
            }

            yield return SceneManager.LoadSceneAsync("NumberPull", LoadSceneMode.Single);
            for (int frame = 0; frame < 30 && GameObject.Find("DifficultySelector") == null; frame++)
            {
                yield return null;
            }

            Component game = GetGame();
            Assert.That(GameObject.Find("DifficultySelector").activeSelf, Is.True);
            Assert.That(GameObject.Find("HomeButton"), Is.Null);
            Assert.That(GetField(game, "match"), Is.Null);

            Invoke(game, "SelectDifficulty", NumberPullDifficultyTier.PreparatoryHighSchool);

            Assert.That(GameObject.Find("DifficultySelector").activeSelf, Is.False);
            Assert.That(GetField(game, "match"), Is.Not.Null);
            Assert.That((bool)game.GetType().GetProperty("HasSelectedDifficulty").GetValue(game), Is.True);
        }

        [UnityTest]
        public IEnumerator ResultOverlayCanReturnToDifficultySelectionAndRestart()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            object match = GetField(game, "match");
            Type matchType = match.GetType();

            for (int index = 0; index < 4; index++)
            {
                object problem = matchType.GetProperty("LeftProblem").GetValue(match);
                int answer = (int)problem.GetType().GetProperty("Answer").GetValue(problem);
                matchType.GetMethod("Submit").Invoke(match, new object[] { answer, null });
            }

            Invoke(game, "ConsumeResultIfReady");
            Assert.That(GameObject.Find("ResultOverlay").activeSelf, Is.True);

            Invoke(game, "ShowDifficultySelector");
            Assert.That(GameObject.Find("ResultOverlay").activeSelf, Is.False);
            Assert.That(GameObject.Find("DifficultySelector").activeSelf, Is.True);
            Assert.That(GetField(game, "match"), Is.Null);

            Invoke(game, "SelectDifficulty", NumberPullDifficultyTier.UpperPrimaryAndSecondary);
            Assert.That(GameObject.Find("DifficultySelector").activeSelf, Is.False);
            Assert.That(GetField(game, "match"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator PauseMenuBlocksGameplayContacts()
        {
            yield return LoadNumberPull();
            Component game = GetGame();

            Invoke(game, "OpenPauseMenu");

            Assert.That(GameObject.Find("PauseOverlay").activeSelf, Is.True);
            Assert.That(GameObject.Find("PauseTitle").GetComponent<UnityEngine.UI.Text>().text, Is.EqualTo("PAUSED"));
            Assert.That(GameObject.Find("RestartMatch"), Is.Not.Null);
            Assert.That(GameObject.Find("ChangeLevel"), Is.Not.Null);
            Assert.That(GameObject.Find("ExitToHub"), Is.Not.Null);
            Assert.That(GameObject.Find("ContinueMatch"), Is.Not.Null);

            TapTarget(game, "1Key", 81);
            Assert.That((bool)GetField(game, "leftHasEntry"), Is.False, "The pause modal must own touches above gameplay controls.");
            Assert.That(HasOwnedContact(game), Is.False, "Opening the pause modal must clear active touch ownership.");
        }

        [UnityTest]
        public IEnumerator PauseMenuDuringDifficultySelectionShowsOnlyContinueAndExit()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            Invoke(game, "ShowDifficultySelector");

            Invoke(game, "OpenPauseMenu");

            Assert.That(GameObject.Find("PauseOverlay").activeSelf, Is.True);
            Assert.That(GameObject.Find("ContinueMatch"), Is.Not.Null);
            Assert.That(GameObject.Find("ExitToHub"), Is.Not.Null);
            Assert.That(FindLoadedTransform("RestartMatch").gameObject.activeSelf, Is.False);
            Assert.That(FindLoadedTransform("ChangeLevel").gameObject.activeSelf, Is.False);

            TapTarget(game, "1Key", 87);
            Assert.That(GetField(game, "match"), Is.Null, "The reduced pause menu must continue to block gameplay controls.");
            Assert.That(HasOwnedContact(game), Is.False);

            TapTarget(game, "ContinueMatch", 88);
            Assert.That(GameObject.Find("PauseOverlay").activeSelf, Is.False);
            Assert.That(GameObject.Find("DifficultySelector").activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator GameplayDoesNotCreateTopLeftPauseHomeControlOrInputTarget()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            Assert.That(GameObject.Find("HomeButton"), Is.Null);
            Assert.That(GameObject.Find("HomeIcon"), Is.Null);
            AssertGameplayHasNoPauseHomeTarget(game);
        }

        [UnityTest]
        public IEnumerator PauseFreezesCountdownAndMatchThenContinueResumes()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            float countdownBeforePause = (float)GetField(game, "countdownRemaining");

            Invoke(game, "OpenPauseMenu");
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.That((float)GetField(game, "countdownRemaining"), Is.EqualTo(countdownBeforePause));

            TapTarget(game, "ContinueMatch", 82);
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.That((float)GetField(game, "countdownRemaining"), Is.LessThan(countdownBeforePause));

            SetField(game, "matchStarted", true);
            object match = GetField(game, "match");
            float elapsedBeforePause = (float)match.GetType().GetProperty("ElapsedSeconds").GetValue(match);
            Invoke(game, "OpenPauseMenu");
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.That((float)match.GetType().GetProperty("ElapsedSeconds").GetValue(match), Is.EqualTo(elapsedBeforePause));

            TapTarget(game, "ContinueMatch", 83);
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.That((float)match.GetType().GetProperty("ElapsedSeconds").GetValue(match), Is.GreaterThan(elapsedBeforePause));
        }

        [UnityTest]
        public IEnumerator PauseRestartPreservesDifficultyAndChangeLevelResetsMatch()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            object selectedDifficulty = GetField(game, "selectedDifficulty");
            object originalMatch = GetField(game, "match");

            Invoke(game, "OpenPauseMenu");
            TapTarget(game, "RestartMatch", 84);
            Assert.That(GameObject.Find("PauseOverlay").activeSelf, Is.False);
            Assert.That(GetField(game, "match"), Is.Not.SameAs(originalMatch));
            Assert.That(GetField(game, "selectedDifficulty"), Is.EqualTo(selectedDifficulty));

            Invoke(game, "OpenPauseMenu");
            TapTarget(game, "ChangeLevel", 85);
            Assert.That(GameObject.Find("PauseOverlay").activeSelf, Is.False);
            Assert.That(GameObject.Find("DifficultySelector").activeSelf, Is.True);
            Assert.That(GetField(game, "match"), Is.Null);
        }

        [UnityTest]
        public IEnumerator PauseExitReportsAnAbandonedStartedMatch()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            SetField(game, "matchStarted", true);

            Invoke(game, "OpenPauseMenu");
            TapTarget(game, "ExitToHub", 86);

            object services = GetField(game, "services");
            object session = services.GetType().GetProperty("Session").GetValue(services);
            object reported = session.GetType().GetProperty("LastResult").GetValue(session);
            Assert.That(reported, Is.Not.Null);
            Assert.That(reported.GetType().GetProperty("CompletionState").GetValue(reported).ToString(), Is.EqualTo("Abandoned"));
        }

        [UnityTest]
        public IEnumerator KeypadsExposeEveryControlInSeparateRows()
        {
            yield return LoadNumberPull();

            AssertKeypadLayout(GameObject.Find("LeftCard").transform);
            AssertKeypadLayout(GameObject.Find("RightCard").transform);
        }

        [UnityTest]
        public IEnumerator KeypadsSeparateDigitSecondaryAndPrimaryVisualRoles()
        {
            yield return LoadNumberPull();

            AssertKeypadVisualRoles(GameObject.Find("LeftCard").transform, true);
            AssertKeypadVisualRoles(GameObject.Find("RightCard").transform, false);
        }

        [UnityTest]
        public IEnumerator CrewVisualsUseFeatureLocalSpritesWithoutBlockingInput()
        {
            yield return LoadNumberPull();

            AssertCrewVisual("PurplePuller", "Characters/PurpleCrewCharacter", 1f);
            AssertCrewVisual("OrangePuller", "Characters/OrangeCrewCharacter", -1f);
            AssertCharacterResource("Characters/PurpleCrewCharacterPulling");
            AssertCharacterResource("Characters/OrangeCrewCharacterPulling");
        }

        [UnityTest]
        public IEnumerator SuccessfulPullSelectsOnlyTheActiveCrewPoseAndResetsDeterministically()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            UnityEngine.UI.Image leftVisual = GetCharacterVisual("PurplePuller");
            UnityEngine.UI.Image rightVisual = GetCharacterVisual("OrangePuller");
            Sprite leftNormal = Resources.Load<Sprite>("Characters/PurpleCrewCharacter");
            Sprite leftPulling = Resources.Load<Sprite>("Characters/PurpleCrewCharacterPulling");
            Sprite rightNormal = Resources.Load<Sprite>("Characters/OrangeCrewCharacter");
            Sprite rightPulling = Resources.Load<Sprite>("Characters/OrangeCrewCharacterPulling");

            Invoke(game, "PresentSubmission", new SubmissionResult(SubmissionFeedback.Correct, SubmissionFeedback.None, true));
            Assert.That(leftVisual.sprite, Is.SameAs(leftPulling));
            Assert.That(rightVisual.sprite, Is.SameAs(rightNormal), "The opposing crew must remain in its normal pose.");

            Invoke(game, "UpdateAnimation", 1f);
            Assert.That(leftVisual.sprite, Is.SameAs(leftNormal), "The pulling pose must end with the animation window.");
            Assert.That(rightVisual.sprite, Is.SameAs(rightNormal));

            Invoke(game, "PresentSubmission", new SubmissionResult(SubmissionFeedback.None, SubmissionFeedback.Correct, true));
            Assert.That(leftVisual.sprite, Is.SameAs(leftNormal), "The opposing crew must remain in its normal pose.");
            Assert.That(rightVisual.sprite, Is.SameAs(rightPulling));

            Invoke(game, "ResetTransientPresentation");
            Assert.That(leftVisual.sprite, Is.SameAs(leftNormal));
            Assert.That(rightVisual.sprite, Is.SameAs(rightNormal), "Reset must restore the normal pose.");
        }

        [UnityTest]
        public IEnumerator NeutralizedOrUnchangedSubmissionNeverSelectsAPullingPose()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            UnityEngine.UI.Image leftVisual = GetCharacterVisual("PurplePuller");
            UnityEngine.UI.Image rightVisual = GetCharacterVisual("OrangePuller");
            Sprite leftNormal = Resources.Load<Sprite>("Characters/PurpleCrewCharacter");
            Sprite rightNormal = Resources.Load<Sprite>("Characters/OrangeCrewCharacter");

            Invoke(game, "PresentSubmission", new SubmissionResult(SubmissionFeedback.Correct, SubmissionFeedback.None, true));
            Invoke(game, "PresentSubmission", new SubmissionResult(SubmissionFeedback.Neutralized, SubmissionFeedback.Neutralized, false));

            Assert.That(leftVisual.sprite, Is.SameAs(leftNormal));
            Assert.That(rightVisual.sprite, Is.SameAs(rightNormal));

            Invoke(game, "PresentSubmission", new SubmissionResult(SubmissionFeedback.Correct, SubmissionFeedback.None, false));
            Assert.That(leftVisual.sprite, Is.SameAs(leftNormal));
            Assert.That(rightVisual.sprite, Is.SameAs(rightNormal));
        }

        [UnityTest]
        public IEnumerator MissingCrewSpriteUsesNonInteractiveProceduralFallback()
        {
            yield return LoadNumberPull();
            Component game = GetGame();
            RectTransform animationLayer = GameObject.Find("AnimationCanvas").GetComponent<RectTransform>();
            MethodInfo createAvatar = game.GetType().GetMethod("CreateAvatar", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(createAvatar, Is.Not.Null);

            RectTransform fallback = (RectTransform)createAvatar.Invoke(game, new object[]
            {
                animationLayer,
                "FallbackPuller",
                Color.magenta,
                new Vector2(0.5f, 0.5f),
                false,
                null
            });

            Assert.That(FindChild(fallback, "CharacterVisual"), Is.Null);
            Assert.That(FindChild(fallback, "Body"), Is.Not.Null);
            Assert.That(FindChild(fallback, "Helmet"), Is.Not.Null);
            AssertImagesDoNotBlockInput(fallback);

            UnityEngine.UI.Image leftVisual = GetCharacterVisual("PurplePuller");
            Sprite leftNormal = Resources.Load<Sprite>("Characters/PurpleCrewCharacter");
            SetField(game, "leftPullingCharacterSprite", null);
            Invoke(game, "SetPullingPose", (MatchSide?)MatchSide.Left);
            Assert.That(leftVisual.sprite, Is.SameAs(leftNormal), "A missing pulling sprite must fall back to the normal sprite.");

            UnityEngine.Object.Destroy(fallback.gameObject);
            yield return null;
        }

        private static IEnumerator LoadNumberPull()
        {
            yield return SceneManager.LoadSceneAsync("Bootstrap", LoadSceneMode.Single);
            for (int frame = 0; frame < 30 && SceneManager.GetActiveScene().name != "Lobby"; frame++)
            {
                yield return null;
            }

            yield return SceneManager.LoadSceneAsync("NumberPull", LoadSceneMode.Single);
            for (int frame = 0; frame < 30 && GameObject.Find("LeftCard") == null; frame++)
            {
                yield return null;
            }

            Assert.That(GameObject.Find("LeftCard"), Is.Not.Null);
            Invoke(GetGame(), "SelectDifficulty", NumberPullDifficultyTier.LowerPrimary);
        }

        private static Component GetGame()
        {
            return GameObject.Find("NumberPullGame").GetComponent("NumberPullGame");
        }

        private static void AssertKeypadLayout(Transform card)
        {
            float minimumKeyHeight = card.GetComponent<RectTransform>().rect.height * 0.075f;
            string[] keyNames =
            {
                "1Key", "2Key", "3Key", "4Key", "5Key", "6Key", "7Key", "8Key", "9Key",
                "SignKey", "0Key", "ClearKey", "SubmitKey"
            };
            RectTransform[] keys = new RectTransform[keyNames.Length];
            for (int index = 0; index < keyNames.Length; index++)
            {
                keys[index] = FindChild(card, keyNames[index]).GetComponent<RectTransform>();
                Assert.That(keys[index].rect.width, Is.GreaterThan(0f), keyNames[index] + " must have a touch target.");
                Assert.That(keys[index].rect.height, Is.GreaterThanOrEqualTo(minimumKeyHeight), keyNames[index] + " must retain a comfortable touch target.");
            }

            RectTransform submit = FindChild(card, "SubmitKey").GetComponent<RectTransform>();
            RectTransform sign = FindChild(card, "SignKey").GetComponent<RectTransform>();
            RectTransform seven = FindChild(card, "7Key").GetComponent<RectTransform>();
            RectTransform clear = FindChild(card, "ClearKey").GetComponent<RectTransform>();
            Assert.That(FindChild(clear, "Label").GetComponent<UnityEngine.UI.Text>().text, Is.EqualTo("CLEAR"));
            Assert.That(FindChild(submit, "Label").GetComponent<UnityEngine.UI.Text>().text, Is.EqualTo("SUBMIT"));
            Assert.That(TopEdge(submit), Is.LessThan(BottomEdge(sign)), "SUBMIT must occupy a dedicated row below the sign controls.");
            Assert.That(TopEdge(sign), Is.LessThan(BottomEdge(seven)), "Sign controls must occupy a dedicated row below the digit grid.");

            for (int first = 0; first < keys.Length; first++)
            {
                for (int second = first + 1; second < keys.Length; second++)
                {
                    Assert.That(WorldRect(keys[first]).Overlaps(WorldRect(keys[second])), Is.False,
                        keyNames[first] + " must not overlap " + keyNames[second] + ".");
                }
            }
        }

        private static void AssertKeypadVisualRoles(Transform card, bool isPurpleCrew)
        {
            UnityEngine.UI.Image digit = FindChild(card, "1Key").GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image sign = FindChild(card, "SignKey").GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image clear = FindChild(card, "ClearKey").GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image submit = FindChild(card, "SubmitKey").GetComponent<UnityEngine.UI.Image>();

            Assert.That(sign.color, Is.EqualTo(clear.color), "The secondary controls must share a visual role.");
            Assert.That(sign.color, Is.Not.EqualTo(digit.color), "Secondary controls must be distinct from digit controls.");
            Assert.That(submit.color, Is.Not.EqualTo(digit.color), "The submit control must retain its primary-action treatment.");
            Assert.That(FindChild(card, "SubmitKey").GetComponentInChildren<UnityEngine.UI.Text>().color,
                Is.EqualTo(new Color(0x24 / 255f, 0x1A / 255f, 0x35 / 255f, 1f)));

            if (isPurpleCrew)
            {
                Assert.That(FindChild(card, "1KeyShadow"), Is.Null, "Purple digit keys must not use a heavier shadow layer than Orange keys.");
                Assert.That(FindChild(card, "1KeyBorder"), Is.Null, "Purple digit keys must not use a flashier border layer than Orange keys.");
            }
        }

        private static void AssertCrewVisual(string avatarName, string resourcePath, float expectedScaleX)
        {
            GameObject avatarObject = GameObject.Find(avatarName);
            Assert.That(avatarObject, Is.Not.Null);
            RectTransform avatar = avatarObject.GetComponent<RectTransform>();
            Transform visualTransform = FindChild(avatar, "CharacterVisual");
            Assert.That(visualTransform, Is.Not.Null);
            UnityEngine.UI.Image visual = visualTransform.GetComponent<UnityEngine.UI.Image>();
            Sprite expectedSprite = Resources.Load<Sprite>(resourcePath);

            Assert.That(expectedSprite, Is.Not.Null);
            Assert.That(visual.sprite, Is.SameAs(expectedSprite));
            Assert.That(visual.preserveAspect, Is.True);
            Assert.That(avatar.sizeDelta, Is.EqualTo(new Vector2(NumberPullBoardLayout.CharacterWidth, NumberPullBoardLayout.CharacterHeight)));
            Assert.That(avatar.anchorMin.y, Is.EqualTo(NumberPullBoardLayout.CharacterVerticalAnchor));
            Assert.That(avatar.anchorMax.y, Is.EqualTo(NumberPullBoardLayout.CharacterVerticalAnchor));
            Assert.That(visual.rectTransform.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(visual.rectTransform.anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(visual.rectTransform.localScale.x, Is.EqualTo(expectedScaleX));
            Assert.That(FindChild(avatar, "Body"), Is.Null, "The imported character should replace the procedural body when available.");
            AssertSpriteFitsCharacterBounds(visual.sprite, avatar.rect);
            AssertImagesDoNotBlockInput(avatar);
        }

        private static void AssertCharacterResource(string resourcePath)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            Assert.That(sprite, Is.Not.Null);
            Assert.That(sprite.texture.width, Is.GreaterThan(0));
            Assert.That(sprite.texture.height, Is.GreaterThan(0));
            AssertSpriteFitsCharacterBounds(sprite, new Rect(0f, 0f, NumberPullBoardLayout.CharacterWidth, NumberPullBoardLayout.CharacterHeight));
        }

        private static NumberPullResult CreateResult(MatchOutcome outcome, int balance)
        {
            return new NumberPullResult(outcome, balance, 42f, new PlayerStats(5, 6), new PlayerStats(4, 6));
        }

        private static NumberPullMatch ConfigureWarningTimer(Component game, float durationSeconds, float elapsedSeconds)
        {
            NumberPullMatch match = new(
                new MathProblemGenerator(7101),
                new MathProblemGenerator(7102),
                durationSeconds: durationSeconds);
            match.Tick(elapsedSeconds);
            SetField(game, "match", match);
            SetField(game, "matchStarted", true);
            SetField(game, "lastDisplayedSecond", -1);
            SetField(game, "lastCountdownWarningSecond", int.MaxValue);
            return match;
        }

        private static int GetRuntimeAudioSourceIndex(object runtimeAudio)
        {
            return (int)runtimeAudio.GetType()
                .GetField("sourceIndex", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(runtimeAudio);
        }

        private static float GetRuntimeAudioLastPlayed(object runtimeAudio, int cueIndex)
        {
            float[] lastPlayed = (float[])runtimeAudio.GetType()
                .GetField("lastPlayed", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(runtimeAudio);
            return lastPlayed[cueIndex];
        }

        private static AudioSource GetRuntimeMusicSource(object runtimeAudio)
        {
            return (AudioSource)runtimeAudio.GetType()
                .GetField("musicSource", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(runtimeAudio);
        }

        private static void AssertImportedAudioCue(Array cachedClips, int cueIndex, string resourcePath, float minimumLength, float maximumLength)
        {
            AudioClip importedClip = Resources.Load<AudioClip>(resourcePath);
            AudioClip cachedClip = (AudioClip)cachedClips.GetValue(cueIndex);
            Assert.That(importedClip, Is.Not.Null, resourcePath + " must be included as a runtime resource.");
            Assert.That(cachedClip, Is.SameAs(importedClip), resourcePath + " must be cached during RuntimeAudio construction.");
            Assert.That(cachedClip.length, Is.InRange(minimumLength, maximumLength));
            Assert.That(cachedClip.frequency, Is.EqualTo(44100));
            Assert.That(cachedClip.channels, Is.EqualTo(1));
            Assert.That(cachedClip.loadType, Is.Not.EqualTo(AudioClipLoadType.Streaming));
        }

        private static void AssertResultArtworkAndCopyRemainSeparated(
            UnityEngine.UI.Image purple,
            UnityEngine.UI.Image orange)
        {
            Assert.That(purple.preserveAspect, Is.True);
            Assert.That(orange.preserveAspect, Is.True);
            Assert.That(purple.raycastTarget, Is.False);
            Assert.That(orange.raycastTarget, Is.False);

            Rect purpleBounds = WorldRect(purple.rectTransform);
            Rect orangeBounds = WorldRect(orange.rectTransform);
            string[] readableElements =
            {
                "ResultTitle", "ResultStats", "Rematch", "ChangeDifficulty", "BackToHub"
            };
            for (int index = 0; index < readableElements.Length; index++)
            {
                Rect copyBounds = WorldRect(FindLoadedTransform(readableElements[index]));
                Assert.That(purpleBounds.Overlaps(copyBounds), Is.False,
                    "Purple result artwork must not obscure " + readableElements[index] + ".");
                Assert.That(orangeBounds.Overlaps(copyBounds), Is.False,
                    "Orange result artwork must not obscure " + readableElements[index] + ".");
            }
        }

        private static void AssertParticleOriginMatchesResultArtwork(RectTransform particle, RectTransform artwork)
        {
            Rect artworkBounds = artwork.rect;
            Vector2 upperTorsoPoint = new(
                artworkBounds.center.x,
                Mathf.Lerp(artworkBounds.yMin, artworkBounds.yMax, 0.62f));
            Vector3 expectedWorldPosition = artwork.TransformPoint(upperTorsoPoint);
            Vector3 actualWorldPosition = particle.TransformPoint(particle.rect.center);
            Assert.That(Vector3.Distance(actualWorldPosition, expectedWorldPosition), Is.LessThan(0.5f));
        }

        private static void AssertResultCardHasHubAndRematchTargets(Component game, RectTransform resultCard)
        {
            IList touchTargets = (IList)GetField(game, "touchTargets");
            bool hasHub = false;
            bool hasRematch = false;
            for (int index = 0; index < touchTargets.Count; index++)
            {
                object target = touchTargets[index];
                Type targetType = target.GetType();
                RectTransform rect = (RectTransform)targetType.GetProperty("Rect").GetValue(target);
                if (!rect.IsChildOf(resultCard))
                {
                    continue;
                }

                object action = targetType.GetProperty("Action").GetValue(target);
                hasHub |= action.ToString() == "Hub";
                hasRematch |= action.ToString() == "Rematch";
            }

            Assert.That(hasHub, Is.True);
            Assert.That(hasRematch, Is.True);
        }

        private static void AssertGameplayHasNoPauseHomeTarget(Component game)
        {
            IList touchTargets = (IList)GetField(game, "touchTargets");
            for (int index = 0; index < touchTargets.Count; index++)
            {
                object target = touchTargets[index];
                Type targetType = target.GetType();
                RectTransform rect = (RectTransform)targetType.GetProperty("Rect").GetValue(target);
                Assert.That(rect.name, Is.Not.EqualTo("HomeButton"));
                Assert.That(targetType.GetProperty("Action").GetValue(target).ToString(), Is.Not.EqualTo("Home"));
            }
        }

        private static void AssertSpriteFitsCharacterBounds(Sprite sprite, Rect bounds)
        {
            float spriteAspect = sprite.rect.width / sprite.rect.height;
            float fittedWidth = Mathf.Min(bounds.width, bounds.height * spriteAspect);
            float fittedHeight = fittedWidth / spriteAspect;
            Assert.That(fittedWidth, Is.GreaterThan(0f).And.LessThanOrEqualTo(NumberPullBoardLayout.CharacterWidth));
            Assert.That(fittedHeight, Is.GreaterThan(0f).And.LessThanOrEqualTo(NumberPullBoardLayout.CharacterHeight));
        }

        private static UnityEngine.UI.Image GetCharacterVisual(string avatarName)
        {
            RectTransform avatar = GameObject.Find(avatarName).GetComponent<RectTransform>();
            return FindChild(avatar, "CharacterVisual").GetComponent<UnityEngine.UI.Image>();
        }

        private static void AssertImagesDoNotBlockInput(Transform root)
        {
            UnityEngine.UI.Image[] images = root.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            Assert.That(images, Is.Not.Empty);
            foreach (UnityEngine.UI.Image image in images)
            {
                Assert.That(image.raycastTarget, Is.False, image.name + " must remain decorative.");
            }
        }

        private static float TopEdge(RectTransform rect)
        {
            return rect.anchorMax.y;
        }

        private static float BottomEdge(RectTransform rect)
        {
            return rect.anchorMin.y;
        }

        private static Rect WorldRect(RectTransform rect)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(
                Mathf.Min(corners[0].x, corners[2].x),
                Mathf.Min(corners[0].y, corners[2].y),
                Mathf.Max(corners[0].x, corners[2].x),
                Mathf.Max(corners[0].y, corners[2].y));
        }

        private static void Invoke(Component target, string method, params object[] arguments)
        {
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, arguments);
        }

        private static object GetField(Component target, string name)
        {
            return target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }

        private static void SetField(Component target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        private static void TapTarget(Component game, string targetName, int fingerId)
        {
            Canvas.ForceUpdateCanvases();
            RectTransform target = FindLoadedTransform(targetName);
            Vector2 position = RectTransformUtility.WorldToScreenPoint(null, target.TransformPoint(target.rect.center));
            Invoke(game, "TryBeginContact", fingerId, position);
            Invoke(game, "ReleaseContact", fingerId);
        }

        private static bool HasOwnedContact(Component game)
        {
            Array contacts = (Array)GetField(game, "contacts");
            foreach (object contact in contacts)
            {
                if ((bool)contact.GetType().GetProperty("Active").GetValue(contact))
                {
                    return true;
                }
            }

            return false;
        }

        private static int ActiveConfettiCount()
        {
            int count = 0;
            for (int index = 0; index < 28; index++)
            {
                GameObject particle = GameObject.Find("Confetti" + index);
                if (particle != null && particle.activeInHierarchy)
                {
                    count++;
                }
            }

            return count;
        }

        private static AudioListener AssertSingleActiveListenerOwnedBy(GameObject expectedOwner)
        {
            AudioListener[] listeners = ActiveAudioListeners();
            Assert.That(listeners, Has.Length.EqualTo(1));
            Assert.That(listeners[0].gameObject, Is.SameAs(expectedOwner));
            return listeners[0];
        }

        private static AudioListener[] ActiveAudioListeners()
        {
            return UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude);
        }

        private static Transform FindChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child;
                }
            }

            return null;
        }

        private static RectTransform FindLoadedTransform(string name)
        {
            RectTransform[] transforms = Resources.FindObjectsOfTypeAll<RectTransform>();
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == name && transforms[index].gameObject.scene.isLoaded)
                {
                    return transforms[index];
                }
            }

            return null;
        }
    }
}
