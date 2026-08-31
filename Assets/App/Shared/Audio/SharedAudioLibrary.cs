using System.Collections.Generic;
using UnityEngine;

namespace Lbs.MiniGames.Shared.Audio
{
    /// <summary>
    /// Immutable library for shared voice and SFX clips. Authored as ScriptableObject; runtime never mutates the asset.
    /// Provides compliments, encouragements, success and fail clips used across games.
    /// </summary>
    [CreateAssetMenu(menuName = "LBS Mini Games/Audio/Shared Audio Library", fileName = "SharedAudioLibrary")]
    public sealed class SharedAudioLibrary : ScriptableObject
    {
        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip failClip;
        [SerializeField] private AudioClip[] compliments = System.Array.Empty<AudioClip>();
        [SerializeField] private AudioClip[] encouragements = System.Array.Empty<AudioClip>();

        public AudioClip SuccessClip => successClip;
        public AudioClip FailClip => failClip;
        public IReadOnlyList<AudioClip> Compliments => compliments;
        public IReadOnlyList<AudioClip> Encouragements => encouragements;

        public bool HasSuccess => successClip != null;
        public bool HasFail => failClip != null;

        public AudioClip PickRandomCompliment()
        {
            if (compliments == null || compliments.Length == 0) return null;
            return compliments[Random.Range(0, compliments.Length)];
        }

        public AudioClip PickRandomEncouragement()
        {
            if (encouragements == null || encouragements.Length == 0) return null;
            return encouragements[Random.Range(0, encouragements.Length)];
        }

#if UNITY_EDITOR
        public void Configure(AudioClip success, AudioClip fail, AudioClip[] complimentClips, AudioClip[] encouragementClips)
        {
            successClip = success;
            failClip = fail;
            compliments = complimentClips ?? System.Array.Empty<AudioClip>();
            encouragements = encouragementClips ?? System.Array.Empty<AudioClip>();
        }
#endif
    }
}
