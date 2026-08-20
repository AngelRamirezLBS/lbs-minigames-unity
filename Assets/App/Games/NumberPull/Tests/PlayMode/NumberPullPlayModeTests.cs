using System;
using System.Collections;
using System.Reflection;
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
        }

        private static Component GetGame()
        {
            return GameObject.Find("NumberPullGame").GetComponent("NumberPullGame");
        }

        private static void Invoke(Component target, string method, params object[] arguments)
        {
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, arguments);
        }

        private static object GetField(Component target, string name)
        {
            return target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
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

        private static Transform FindLoadedTransform(string name)
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
