using UnityEngine;
using TheLastCrew.Player;

namespace TheLastCrew.Audio
{
    /// <summary>
    /// Turns <see cref="PlayerMotor.Footstep"/> into sound.
    ///
    /// Footsteps are the player's passive sonar: the reflection around each
    /// step is how the maze reveals itself. So they must never be inaudible,
    /// and they must change when the player is trying to stay quiet.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class FootstepAudio : MonoBehaviour
    {
        [Header("Source")]
        public PlayerMotor motor;

        [Header("Clips")]
        [Tooltip("Two or three variations stop the loop sounding mechanical.")]
        public AudioClip[] clips;

        [Header("Mix")]
        [Range(0f, 1f)] public float baseVolume = 0.7f;
        public Vector2 pitchRange = new Vector2(0.92f, 1.08f);

        private AudioSource source;
        private int lastClip = -1;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // your own steps are inside your head
        }

        private void OnEnable()
        {
            if (motor != null)
                motor.Footstep += Play;
        }

        private void OnDisable()
        {
            if (motor != null)
                motor.Footstep -= Play;
        }

        private void Play(float loudness)
        {
            if (clips == null || clips.Length == 0)
                return;

            AudioClip clip = PickClip();

            source.pitch = Random.Range(pitchRange.x, pitchRange.y);
            source.PlayOneShot(clip, baseVolume * Mathf.Clamp01(loudness));
        }

        private AudioClip PickClip()
        {
            if (clips.Length == 1)
                return clips[0];

            int index = Random.Range(0, clips.Length);

            // Avoid the same foot twice in a row.
            if (index == lastClip)
                index = (index + 1) % clips.Length;

            lastClip = index;
            return clips[index];
        }
    }
}
