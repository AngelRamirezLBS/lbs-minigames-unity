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
            Assert.That(sources, Has.Length.EqualTo(4));
            foreach (AudioSource source in sources)
            {
                Assert.That(source.enabled, Is.True);
                Assert.That(source.playOnAwake, Is.False);
                Assert.That(source.loop, Is.False);
                Assert.That(source.mute, Is.False);
                Assert.That(source.volume, Is.EqualTo(1f));
                Assert.That(source.spatialBlend, Is.EqualTo(0f));
            }

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

            Canvas animationCanvas = GameObject.Find("AnimationCanvas").GetComponent<Canvas>();
            Canvas resultCanvas = FindLoadedTransform("ResultOverlay").GetComponent<Canvas>();
            Assert.That(resultCanvas.overrideSorting, Is.True);
            Assert.That(resultCanvas.sortingOrder, Is.GreaterThan(animationCanvas.sortingOrder));

            gameObject.SetActive(false);
            yield return null;
            Assert.That(ActiveAudioListeners(), Is.Empty);
            gameObject.SetActive(true);
            yield return null;

            AssertSingleActiveListenerOwnedBy(gameObject);
            Assert.That(GameObject.Find("SafeAreaRoot"), Is.Not.Null);
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
            UnityEngine.Object.Destroy(clip);
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

            game.gameObject.SetActive(false);
            yield return null;
            game.gameObject.SetActive(true);
            yield return null;

            Assert.That(ActiveConfettiCount(), Is.Zero);
            Assert.That(leftCard.GetComponent<RectTransform>().anchoredPosition, Is.EqualTo(Vector2.zero));
            Assert.That(HasOwnedContact(game), Is.False);
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
            Assert.That(GameObject.Find("HomeButton").activeSelf, Is.True);
            Assert.That(GameObject.Find("HomeButton").GetComponent<Canvas>().sortingOrder,
                Is.GreaterThan(GameObject.Find("DifficultySelector").GetComponent<Canvas>().sortingOrder));
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
        public IEnumerator HomeButtonOpensSpanishPauseMenuAndBlocksGameplayContacts()
        {
            yield return LoadNumberPull();
            Component game = GetGame();

            Assert.That(GameObject.Find("HomeButton"), Is.Not.Null);
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
        public IEnumerator HomeButtonDuringDifficultySelectionShowsOnlyContinueAndExit()
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
        public IEnumerator TappingHomeDuringDifficultySelectionOpensPauseAndKeepsDifficultyChoicesUsable()
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
            TapTarget(game, "HomeButton", 89);

            Assert.That(GameObject.Find("PauseOverlay").activeSelf, Is.True);
            Assert.That(GameObject.Find("ContinueMatch").activeSelf, Is.True);
            Assert.That(GameObject.Find("ExitToHub").activeSelf, Is.True);
            Assert.That(FindLoadedTransform("RestartMatch").gameObject.activeSelf, Is.False);
            Assert.That(FindLoadedTransform("ChangeLevel").gameObject.activeSelf, Is.False);

            TapTarget(game, "DifficultyLowerPrimary", 90);
            Assert.That(GameObject.Find("PauseOverlay").activeSelf, Is.True, "The pause modal must reject selector-card touches.");
            Assert.That(GameObject.Find("DifficultySelector").activeSelf, Is.True);
            Assert.That(GetField(game, "match"), Is.Null);

            TapTarget(game, "ContinueMatch", 90);
            Assert.That(GameObject.Find("DifficultySelector").activeSelf, Is.True);

            TapTarget(game, "DifficultyLowerPrimary", 91);
            Assert.That(GameObject.Find("DifficultySelector").activeSelf, Is.False);
            Assert.That(GetField(game, "match"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator ExitFromSelectorPauseNavigatesToHubWithoutStartingAMatch()
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
            object services = GetField(game, "services");
            object session = services.GetType().GetProperty("Session").GetValue(services);
            TapTarget(game, "HomeButton", 92);
            TapTarget(game, "ExitToHub", 93);

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Lobby"));
            Assert.That(session.GetType().GetProperty("LastResult").GetValue(session), Is.Null);
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
            Assert.That(TopEdge(submit), Is.LessThan(BottomEdge(sign)), "LISTO must occupy a dedicated row below the sign controls.");
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
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
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
