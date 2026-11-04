using UnityEngine;

namespace TheLastCrew.Audio
{
    /// <summary>
    /// Catches the two mistakes that quietly destroy spatial audio:
    /// a second Audio Listener, or a listener nowhere near the player's ears.
    ///
    /// Cheap to run, and it fails loudly in development rather than sounding
    /// "a bit wrong" on a phone in someone's hand.
    /// </summary>
    public class SpatialAudioGuard : MonoBehaviour
    {
        [Tooltip("The camera the listener must be attached to.")]
        public Transform expectedListener;

        private void Start()
        {
            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);

            if (listeners.Length == 0)
            {
                Debug.LogError("SpatialAudioGuard: no AudioListener in the scene. Nothing will be heard.", this);
                return;
            }

            if (listeners.Length > 1)
            {
                Debug.LogError(
                    $"SpatialAudioGuard: {listeners.Length} AudioListeners found. " +
                    "Only the first one is used, and it may not be the player's.", this);
            }

            if (expectedListener == null)
                return;

            AudioListener listener = expectedListener.GetComponent<AudioListener>();

            if (listener == null)
                Debug.LogWarning("SpatialAudioGuard: expected listener is not on the player camera.", this);
            else if (listener != listeners[0])
                Debug.LogWarning("SpatialAudioGuard: the active listener is not the player camera's.", this);
        }
    }
}
