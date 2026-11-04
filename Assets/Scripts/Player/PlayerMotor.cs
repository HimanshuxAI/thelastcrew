using System;
using UnityEngine;

namespace TheLastCrew.Player
{
    /// <summary>
    /// Moves the player from any <see cref="IPlayerInput"/> source.
    ///
    /// No pitch, no head bob, no camera shake: this game is flat, and the
    /// player's facing direction is their listening direction. Anything that
    /// moves the camera unnecessarily costs the player information.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Input source")]
        [Tooltip("Assign the TouchInputProvider (or any MonoBehaviour implementing IPlayerInput).")]
        public MonoBehaviour inputSource;

        [Header("Speeds")]
        public float walkSpeed = 1.8f;
        public float quietSpeed = 0.6f;

        [Tooltip("Degrees per second at full turn input.")]
        public float turnSpeed = 90f;

        public float gravity = -20f;

        [Header("Footsteps")]
        [Tooltip("Metres travelled between steps.")]
        public float strideLength = 0.85f;

        /// <summary>
        /// Loudness of one footstep, 0..1. The audio layer turns this into
        /// sound, and the sound is how the player hears the space around them.
        /// </summary>
        public event Action<float> Footstep;

        private CharacterController controller;
        private IPlayerInput input;
        private float verticalVelocity;
        private float walkedSinceStep;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = inputSource as IPlayerInput;

            if (input == null)
            {
                Debug.LogError(
                    "PlayerMotor: inputSource is missing or does not implement IPlayerInput.",
                    this);
            }

            // Start ready to step, so the first movement makes a sound at once.
            walkedSinceStep = strideLength;
        }

        private void Update()
        {
            if (input == null)
                return;

            transform.Rotate(Vector3.up, input.Turn * turnSpeed * Time.deltaTime);

            float speed = input.IsHiding ? quietSpeed : walkSpeed;

            Vector3 wish = transform.right * input.Move.x + transform.forward * input.Move.y;
            wish = Vector3.ClampMagnitude(wish, 1f);

            if (controller.isGrounded && verticalVelocity.y < 0f)
                verticalVelocity.y = -2f;

            verticalVelocity.y += gravity * Time.deltaTime;

            Vector3 motion = wish * speed + Vector3.up * verticalVelocity.y;
            controller.Move(motion * Time.deltaTime);

            TrackFootsteps(wish, speed);
        }

        private void TrackFootsteps(Vector3 wish, float speed)
        {
            float travelled = wish.magnitude * speed * Time.deltaTime;

            if (travelled <= 0f)
                return;

            walkedSinceStep += travelled;

            if (walkedSinceStep < strideLength)
                return;

            walkedSinceStep = 0f;
            Footstep?.Invoke(input.IsHiding ? 0.2f : 1f);
        }
    }
}
