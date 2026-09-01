using UnityEngine;

namespace Lbs.MiniGames.Shared.Audio
{
    /// <summary>
    /// Persistent audio boundary. Lives on ApplicationBootstrap's DontDestroyOnLoad object.
    /// Owns three dedicated AudioSources (Music, Voice, SFX) with explicit lifecycle.
    /// Music survives scene transitions and is not restarted for the same clip.
    /// Voice ducks music and interrupts previous voice. SFX uses PlayOneShot.
    /// </summary>
    public sealed class AppAudioService : MonoBehaviour, IAppAudioService
    {
        private AudioSource musicSource;
        private AudioSource voiceSource;
        private AudioSource sfxSource;
        private AppAudioConfig config;
        private AudioClip currentMusicClip;
        private float baseMusicVolume = 0.25f;
        private float duckedMusicVolume = 0.125f;
        private bool isPaused;
        private bool isVoiceDucking;
        private bool isApplicationPaused;
        private Coroutine musicPlayback;
        private Coroutine voicePlayback;
        private AudioClip pendingVoiceClip;

        public void Initialize(AppAudioConfig audioConfig)
        {
            config = audioConfig;
            baseMusicVolume = config != null ? config.MusicVolume : 0.25f;
            duckedMusicVolume = config != null ? config.DuckedMusicVolume : 0.125f;
            EnsureSources();
            ApplyVolumes();
        }

        private void EnsureSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.spatialBlend = 0f;
                musicSource.volume = baseMusicVolume;
            }

            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.loop = false;
                voiceSource.playOnAwake = false;
                voiceSource.spatialBlend = 0f;
                voiceSource.volume = config != null ? config.VoiceVolume : 1f;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
                sfxSource.volume = config != null ? config.SfxVolume : 1f;
            }
        }

        public void PlayMusic(AudioClip clip, bool loop = true, float volume = 0.25f)
        {
            if (clip == null) return;
            EnsureSources();

            // Idempotent: same clip already playing — do not restart, only ensure volume/loop.
            if (currentMusicClip == clip && musicSource.isPlaying && musicSource.clip == clip)
            {
                musicSource.loop = loop;
                if (!isVoiceDucking)
                {
                    baseMusicVolume = volume;
                    if (!isPaused && !isApplicationPaused) musicSource.volume = volume;
                }
                else
                {
                    baseMusicVolume = volume;
                }
                return;
            }

            currentMusicClip = clip;
            baseMusicVolume = volume;
            if (musicPlayback != null)
            {
                StopCoroutine(musicPlayback);
                musicPlayback = null;
            }
            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = loop;
            if (!isPaused && !isApplicationPaused)
            {
                musicSource.volume = isVoiceDucking ? duckedMusicVolume : baseMusicVolume;
            }

            if (clip.loadState == AudioDataLoadState.Unloaded)
            {
                clip.LoadAudioData();
            }
            if (clip.loadState == AudioDataLoadState.Loading)
            {
                musicPlayback = StartCoroutine(PlayMusicWhenReady(clip));
            }
            else if (!isPaused && !isApplicationPaused)
            {
                musicSource.Play();
            }
        }

        private System.Collections.IEnumerator PlayMusicWhenReady(AudioClip clip)
        {
            while (clip != null && clip.loadState == AudioDataLoadState.Loading) yield return null;
            if (clip == null || clip != currentMusicClip || isPaused || isApplicationPaused)
            {
                musicPlayback = null;
                yield break;
            }
            if (musicSource != null && musicSource.clip == clip) musicSource.Play();
            musicPlayback = null;
        }

        public void StopMusic()
        {
            if (musicSource == null) return;
            musicSource.Stop();
            musicSource.clip = null;
            currentMusicClip = null;
            if (musicPlayback != null) StopCoroutine(musicPlayback);
            musicPlayback = null;
            isVoiceDucking = false;
        }

        public bool IsMusicPlaying(AudioClip clip)
        {
            return clip != null && currentMusicClip == clip && musicSource != null && musicSource.isPlaying && musicSource.clip == clip;
        }

        public bool IsVoicePlaying(AudioClip clip = null)
        {
            if (voiceSource == null || !voiceSource.isPlaying) return false;
            if (clip == null) return true;
            return voiceSource.clip == clip;
        }

        public void PlayVoice(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            EnsureSources();
            CancelPendingVoicePlayback();
            if (clip.loadState == AudioDataLoadState.Loading)
            {
                // Schedule playback when ready without restarting music.
                pendingVoiceClip = clip;
                voicePlayback = StartCoroutine(PlayVoiceWhenReady(clip, volume));
                return;
            }

            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.volume = volume;
            if (!isPaused && !isApplicationPaused)
            {
                voiceSource.Play();
                ApplyVoiceDuck(true);
            }
        }

        private System.Collections.IEnumerator PlayVoiceWhenReady(AudioClip clip, float volume)
        {
            while (clip != null && clip.loadState == AudioDataLoadState.Loading) yield return null;
            if (clip == pendingVoiceClip && (clip.loadState == AudioDataLoadState.Loaded || clip.loadState == AudioDataLoadState.Unloaded))
            {
                voiceSource.Stop();
                voiceSource.clip = clip;
                voiceSource.volume = volume;
                if (!isPaused && !isApplicationPaused)
                {
                    voiceSource.Play();
                    ApplyVoiceDuck(true);
                }
            }
            pendingVoiceClip = null;
            voicePlayback = null;
        }

        public void StopVoice()
        {
            CancelPendingVoicePlayback();
            if (voiceSource == null) return;
            voiceSource.Stop();
            voiceSource.clip = null;
            ApplyVoiceDuck(false);
        }

        public void StopVoiceIfPlaying(AudioClip clip)
        {
            if (clip == null) return;
            if (pendingVoiceClip == clip) CancelPendingVoicePlayback();
            if (voiceSource != null && voiceSource.clip == clip) StopVoice();
        }

        private void CancelPendingVoicePlayback()
        {
            if (voicePlayback != null) StopCoroutine(voicePlayback);
            voicePlayback = null;
            pendingVoiceClip = null;
        }

        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;
            EnsureSources();
            if (!isPaused && !isApplicationPaused)
            {
                sfxSource.PlayOneShot(clip, volumeScale);
            }
        }

        public void PauseAll()
        {
            SetPaused(true);
        }

        public void ResumeAll()
        {
            SetPaused(false);
        }

        public void SetPaused(bool paused)
        {
            if (isPaused == paused) return;
            isPaused = paused;
            if (paused)
            {
                if (musicSource != null && musicSource.isPlaying) musicSource.Pause();
                if (voiceSource != null && voiceSource.isPlaying) voiceSource.Pause();
                if (sfxSource != null && sfxSource.isPlaying) sfxSource.Pause();
            }
            else
            {
                if (isApplicationPaused) return;
                if (musicSource != null && currentMusicClip != null && !musicSource.isPlaying)
                {
                    musicSource.UnPause();
                }
                if (voiceSource != null && voiceSource.clip != null && !voiceSource.isPlaying)
                {
                    voiceSource.UnPause();
                }
                if (sfxSource != null && sfxSource.clip != null && !sfxSource.isPlaying)
                {
                    sfxSource.UnPause();
                }
            }
        }

        public void StopAll()
        {
            // Idempotent cleanup - safe to call multiple times or after partial init.
            if (musicSource != null)
            {
                musicSource.Stop();
                musicSource.clip = null;
            }
            currentMusicClip = null;
            if (musicPlayback != null) StopCoroutine(musicPlayback);
            musicPlayback = null;
            CancelPendingVoicePlayback();
            isVoiceDucking = false;
            if (voiceSource != null)
            {
                voiceSource.Stop();
                voiceSource.clip = null;
            }
            if (sfxSource != null)
            {
                sfxSource.Stop();
            }
        }

        private void ApplyVoiceDuck(bool duck)
        {
            isVoiceDucking = duck;
            if (musicSource == null) return;
            if (duck)
            {
                if (musicSource.isPlaying) musicSource.volume = duckedMusicVolume;
            }
            else
            {
                if (musicSource.isPlaying) musicSource.volume = baseMusicVolume;
            }
        }

        private void ApplyVolumes()
        {
            if (musicSource != null) musicSource.volume = isVoiceDucking ? duckedMusicVolume : baseMusicVolume;
            if (voiceSource != null) voiceSource.volume = config != null ? config.VoiceVolume : 1f;
            if (sfxSource != null) sfxSource.volume = config != null ? config.SfxVolume : 1f;
        }

        private void Update()
        {
            // Detect voice completion to unduck music.
            if (isVoiceDucking && voiceSource != null && !voiceSource.isPlaying)
            {
                ApplyVoiceDuck(false);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            isApplicationPaused = pauseStatus;
            if (pauseStatus)
            {
                if (musicSource != null && musicSource.isPlaying) musicSource.Pause();
                if (voiceSource != null && voiceSource.isPlaying) voiceSource.Pause();
                if (sfxSource != null && sfxSource.isPlaying) sfxSource.Pause();
            }
            else
            {
                if (isPaused) return;
                if (musicSource != null && currentMusicClip != null) musicSource.UnPause();
                if (voiceSource != null && voiceSource.clip != null) voiceSource.UnPause();
                // SFX is one-shot, not resumed.
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Mirror pause behavior: losing focus pauses, gaining focus resumes unless explicitly paused.
            if (!hasFocus)
            {
                if (musicSource != null && musicSource.isPlaying) musicSource.Pause();
                if (voiceSource != null && voiceSource.isPlaying) voiceSource.Pause();
            }
            else
            {
                if (isPaused || isApplicationPaused) return;
                if (musicSource != null && currentMusicClip != null) musicSource.UnPause();
                if (voiceSource != null && voiceSource.clip != null) voiceSource.UnPause();
            }
        }

        private void OnDisable()
        {
            // MonoBehaviour OnDisable is not used for cleanup of persistent service; kept symmetric for testability.
        }

        private void OnDestroy()
        {
            StopAll();
        }
    }
}
