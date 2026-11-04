using UnityEngine;

namespace TheLastCrew.Audio
{
    /// <summary>
    /// The maze's voice.
    ///
    /// A ring of probes fires outward from the player. Whenever a probe finds a
    /// wall, a looping rustle is placed at that spot, in that direction. So the
    /// player hears which side the wall is on, how close it is, and - with
    /// several walls sounding at once - whether they are in a corridor, a
    /// chamber or a corner.
    ///
    /// Nothing here needs a map. The sound is the map.
    /// </summary>
    public class WallProximityAudio : MonoBehaviour
    {
        [Header("Probes")]
        [Tooltip("How many directions are tested. 8 is smooth, 4 is cheap.")]
        [Range(4, 16)] public int probeCount = 8;

        [Tooltip("Furthest distance a wall can be heard from.")]
        public float maxRange = 5f;

        [Tooltip("Which layers count as walls.")]
        public LayerMask wallMask = ~0;

        [Tooltip("Height above the player's feet that probes travel at.")]
        public float probeHeight = 0.9f;

        [Header("Sound")]
        public AudioClip rustleClip;

        [Range(0f, 1f)] public float maxVolume = 0.30f;

        [Tooltip("Distance at which the rustle is at full strength.")]
        public float nearDistance = 0.8f;

        [Tooltip("How quickly the volume follows the player. Higher is snappier.")]
        public float smoothing = 7f;

        [Header("Debug")]
        public bool drawGizmos = true;

        private class Probe
        {
            public AudioSource source;
            public Vector3 direction;
            public float volume;
            public float distance;
        }

        private Probe[] probes;
        private Vector3 origin;

        private void Awake()
        {
            if (rustleClip == null)
                Debug.LogWarning("WallProximityAudio: no rustle clip assigned - the walls will be silent.", this);

            probes = new Probe[probeCount];
            float step = 360f / probeCount;

            for (int i = 0; i < probeCount; i++)
            {
                GameObject child = new GameObject($"Probe_{i * step:0}");
                child.transform.SetParent(transform, false);

                AudioSource source = child.AddComponent<AudioSource>();
                source.clip = rustleClip;
                source.loop = true;
                source.playOnAwake = false;
                source.spatialBlend = 1f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = nearDistance;
                source.maxDistance = maxRange;
                source.dopplerLevel = 0f;
                source.volume = 0f;

                probes[i] = new Probe
                {
                    source = source,
                    direction = Quaternion.Euler(0f, i * step, 0f) * Vector3.forward,
                    volume = 0f
                };

                if (rustleClip != null)
                {
                    source.Play();
                    // Offset each loop so eight copies do not phase into one tone.
                    source.time = Random.value * rustleClip.length;
                }
            }
        }

        private void Update()
        {
            if (probes == null)
                return;

            origin = transform.position + Vector3.up * probeHeight;
            float follow = 1f - Mathf.Exp(-smoothing * Time.deltaTime);

            for (int i = 0; i < probes.Length; i++)
            {
                Probe probe = probes[i];
                Vector3 worldDirection = transform.rotation * probe.direction;

                float strength;
                float distance;

                if (Physics.Raycast(origin, worldDirection, out RaycastHit hit,
                        maxRange, wallMask, QueryTriggerInteraction.Ignore))
                {
                    distance = hit.distance;
                    strength = 1f - Mathf.InverseLerp(nearDistance, maxRange, distance);
                }
                else
                {
                    distance = maxRange;
                    strength = 0f;
                }

                probe.distance = distance;
                probe.volume = Mathf.Lerp(probe.volume, strength * maxVolume, follow);
                probe.source.volume = probe.volume;

                // Stand the rustle on the wall itself. Its direction is what the
                // player's ears read; its distance sets how loud it arrives.
                Vector3 local = probe.direction * Mathf.Max(0.1f, distance - 0.2f);
                probe.source.transform.localPosition = local;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            Vector3 center = transform.position + Vector3.up * probeHeight;
            Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.6f);

            int count = Mathf.Max(1, probeCount);
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = transform.rotation * (Quaternion.Euler(0f, i * 360f / count, 0f) * Vector3.forward);
                Gizmos.DrawLine(center, center + dir * maxRange);
            }
        }
    }
}
