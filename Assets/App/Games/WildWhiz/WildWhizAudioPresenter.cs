using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lbs.MiniGames.Games.WildWhiz
{
    /// <summary>
    /// Screen-owned audio presenter with 2 AudioSources and a listener reference.
    /// Tries clip first, falls back to runtime-generated tones. Missing-clip logs once.
    /// </summary>
    public sealed class WildWhizAudioPresenter : MonoBehaviour
    {
        [SerializeField] private AudioClip instructionClip;

        private AudioSource voiceSource;
        private AudioSource sfxSource;
        private AudioListener localListener;
        private AudioClip fallbackClip;
        private bool missingClipLogged;
        private bool listenerChecked;
        private readonly List<AudioListener> changedListeners = new();
        private readonly List<bool> changedListenerStates = new();

        public int AudioSourceCount => (voiceSource != null ? 1 : 0) + (sfxSource != null ? 1 : 0);

        public int ActiveListenerCount => FindObjectsOfType<AudioListener>().Length;

        public AudioClip InstructionClip => instructionClip;

        public void SetInstructionClip(AudioClip clip)
        {
            instructionClip = clip;
            missingClipLogged = false;
        }

        private void Awake()
        {
            EnsureAudio();
        }

        private void OnEnable()
        {
            EnsureAudio();
        }

        private void OnDisable()
        {
            StopAll();
            RestoreListeners();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                StopAll();
            }
        }

        public void EnsureAudio()
        {
            if (voiceSource == null)
            {
                voiceSource = gameObject.GetComponent<AudioSource>();
                if (voiceSource == null)
                {
                    voiceSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (sfxSource == null)
            {
                AudioSource[] sources = gameObject.GetComponents<AudioSource>();
                if (sources.Length >= 2)
                {
                    // Reuse second source if already present.
                    voiceSource = sources[0];
                    sfxSource = sources[1];
                }
                else if (sources.Length == 1)
                {
                    sfxSource = gameObject.AddComponent<AudioSource>();
                }
                else
                {
                    sfxSource = gameObject.AddComponent<AudioSource>();
                }
            }

            ConfigureSource(voiceSource);
            ConfigureSource(sfxSource);

            EnsureListener();
        }

        private static void ConfigureSource(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.spatialBlend = 0f;
            source.playOnAwake = false;
            source.loop = false;
        }

        private void EnsureListener()
        {
            if (listenerChecked && localListener != null)
            {
                return;
            }

            AudioListener[] listeners = FindObjectsOfType<AudioListener>(true);
            if (listeners.Length == 0)
            {
                localListener = gameObject.GetComponent<AudioListener>();
                if (localListener == null)
                {
                    localListener = gameObject.AddComponent<AudioListener>();
                }
            }
            else if (listeners.Length == 1)
            {
                localListener = listeners[0];
            }
            else
            {
                // Never mutate listeners owned by another object. Unity may enforce
                // listener uniqueness before this component can observe the prior state.
                localListener = gameObject.GetComponent<AudioListener>();
                if (localListener == null)
                {
                    localListener = listeners[0];
                }
            }

            listenerChecked = true;
        }

        private void RestoreListeners()
        {
            for (int index = changedListeners.Count - 1; index >= 0; index--)
            {
                if (changedListeners[index] != null)
                {
                    changedListeners[index].enabled = changedListenerStates[index];
                }
            }

            changedListeners.Clear();
            changedListenerStates.Clear();
            listenerChecked = false;
            localListener = null;
        }

        public void Replay()
        {
            EnsureAudio();

            if (instructionClip != null)
            {
                if (voiceSource != null)
                {
                    voiceSource.Stop();
                    voiceSource.clip = instructionClip;
                    voiceSource.Play();
                }

                return;
            }

            if (!missingClipLogged)
            {
                missingClipLogged = true;
                Debug.LogWarning("[WildWhizAudioPresenter] Instruction clip missing — using runtime tone fallback.", this);
            }

            fallbackClip ??= CreateFallbackInstructionClip();
            if (voiceSource != null && fallbackClip != null)
            {
                voiceSource.Stop();
                voiceSource.clip = fallbackClip;
                voiceSource.Play();
            }
        }

        public void PlaySuccess()
        {
            EnsureAudio();
            AudioClip clip = CreateSuccessClip();
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, 0.18f);
            }
        }

        public void PlayError()
        {
            EnsureAudio();
            AudioClip clip = CreateErrorClip();
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, 0.16f);
            }
        }

        public void StopAll()
        {
            if (voiceSource != null)
            {
                voiceSource.Stop();
            }

            if (sfxSource != null)
            {
                sfxSource.Stop();
            }
        }

        private static AudioClip CreateFallbackInstructionClip()
        {
            const float duration = 0.22f;
            return CreateRuntimeClip("WildWhizInstructionFallback", duration, time =>
            {
                float progress = time / duration;
                float phase = 2f * Mathf.PI * (320f * time + 80f * Mathf.Sin(2f * Mathf.PI * 2f * time));
                return Mathf.Sin(phase) * Mathf.Sin(Mathf.PI * progress) * 0.26f;
            });
        }

        private static AudioClip CreateSuccessClip()
        {
            const float duration = 0.18f;
            return CreateRuntimeClip("WildWhizSuccess", duration, time =>
            {
                float progress = time / duration;
                float phase = 2f * Mathf.PI * (520f * time + 60f * progress * time);
                return Mathf.Sin(phase) * Mathf.Sin(Mathf.PI * progress) * 0.28f;
            });
        }

        private static AudioClip CreateErrorClip()
        {
            const float duration = 0.14f;
            return CreateRuntimeClip("WildWhizError", duration, time =>
            {
                float progress = time / duration;
                float phase = 2f * Mathf.PI * (180f * time - 30f * time * progress);
                return Mathf.Sin(phase) * Mathf.Pow(1f - progress, 2f) * 0.30f;
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
    }
}
