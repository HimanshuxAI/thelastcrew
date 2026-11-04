using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace TheLastCrew.Player
{
    /// <summary>
    /// Where a finger landed. The four corners are the only places a player
    /// can find reliably without looking, so every button lives there.
    /// </summary>
    public enum TouchZone
    {
        None,
        Move,
        TurnLeft,
        TurnRight,
        Hand,
        Ear,
        Menu
    }

    /// <summary>
    /// Touch-first input for a game played with headphones and no screen.
    ///
    /// Layout (portrait or landscape, works in both):
    ///
    ///   [turn left]                    [turn right]
    ///
    ///                 [ menu ]
    ///
    ///         (anywhere here = walk)
    ///
    ///   [hand]                              [ear]
    ///
    /// Nothing is drawn. Every zone reports itself through <see cref="Announce"/>
    /// so a self-voicing layer can speak it.
    /// </summary>
    public class TouchInputProvider : MonoBehaviour, IPlayerInput
    {
        [Header("Zones (fraction of screen)")]
        [Tooltip("Size of each corner button.")]
        [Range(0.15f, 0.40f)] public float cornerSize = 0.25f;

        [Header("Gestures")]
        [Tooltip("Seconds a finger must rest before it counts as a hold.")]
        public float holdTime = 0.5f;

        [Tooltip("Max seconds between two taps to count as a double tap.")]
        public float doubleTapWindow = 0.35f;

        [Tooltip("Distance, as a fraction of screen height, that means full walking speed.")]
        public float joystickRadius = 0.12f;

        [Tooltip("Movement below this fraction of the radius is ignored, so a resting thumb does not drift.")]
        public float deadZone = 0.15f;

        [Header("Keyboard fallback (editor / PC)")]
        public bool keyboardFallback = true;

        // --- IPlayerInput ---
        public Vector2 Move { get; private set; }
        public float Turn { get; private set; }
        public bool IsHiding { get; private set; }

        // --- Events for the audio and gameplay layers ---
        /// <summary>Fired the moment a finger touches a zone. Speak the zone name here.</summary>
        public event Action<TouchZone> ZoneTouched;

        /// <summary>Short tap on a zone.</summary>
        public event Action<TouchZone> ZoneTapped;

        /// <summary>Finger held on a zone past <see cref="holdTime"/>.</summary>
        public event Action<TouchZone> ZoneHoldStarted;

        /// <summary>Double tap on the menu button.</summary>
        public event Action MenuConfirmed;

        /// <summary>Text a self-voicing layer should speak.</summary>
        public event Action<string> Announce;

        private class Pointer
        {
            public TouchZone zone;
            public Vector2 origin;
            public Vector2 position;
            public float startedAt;
            public bool holdFired;
        }

        private readonly Dictionary<int, Pointer> pointers = new Dictionary<int, Pointer>();
        private float lastMenuTapAt = -10f;
        private int activeMoveFinger = -1;

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            Touch.onFingerDown += OnFingerDown;
            Touch.onFingerUp += OnFingerUp;
            Touch.onFingerMove += OnFingerMove;
        }

        private void OnDisable()
        {
            Touch.onFingerDown -= OnFingerDown;
            Touch.onFingerUp -= OnFingerUp;
            Touch.onFingerMove -= OnFingerMove;
            EnhancedTouchSupport.Disable();

            pointers.Clear();
            activeMoveFinger = -1;
        }

        private void Update()
        {
            Move = Vector2.zero;
            Turn = 0f;
            IsHiding = false;

            UpdatePointers();

            if (keyboardFallback)
                UpdateKeyboard();
        }

        // ------------------------------------------------------------------
        // Touch
        // ------------------------------------------------------------------

        private void OnFingerDown(Finger finger)
        {
            TouchZone zone = ZoneAt(finger.screenPosition);

            pointers[finger.index] = new Pointer
            {
                zone = zone,
                origin = finger.screenPosition,
                position = finger.screenPosition,
                startedAt = Time.unscaledTime,
                holdFired = false
            };

            if (zone == TouchZone.Move && activeMoveFinger < 0)
                activeMoveFinger = finger.index;

            ZoneTouched?.Invoke(zone);
            Announce?.Invoke(ZoneName(zone));
        }

        private void OnFingerMove(Finger finger)
        {
            if (pointers.TryGetValue(finger.index, out Pointer pointer))
                pointer.position = finger.screenPosition;
        }

        private void OnFingerUp(Finger finger)
        {
            if (!pointers.TryGetValue(finger.index, out Pointer pointer))
                return;

            float held = Time.unscaledTime - pointer.startedAt;
            float slop = Screen.height * 0.03f;
            bool moved = (pointer.position - pointer.origin).sqrMagnitude > slop * slop;

            if (!pointer.holdFired && held < holdTime && !moved)
                HandleTap(pointer.zone);

            if (pointer.zone == TouchZone.Move && activeMoveFinger == finger.index)
                activeMoveFinger = -1;

            pointers.Remove(finger.index);
        }

        private void HandleTap(TouchZone zone)
        {
            ZoneTapped?.Invoke(zone);
            Announce?.Invoke(ZoneName(zone) + " tapped");

            if (zone != TouchZone.Menu)
                return;

            float now = Time.unscaledTime;

            if (now - lastMenuTapAt <= doubleTapWindow)
            {
                lastMenuTapAt = -10f;
                MenuConfirmed?.Invoke();
                Announce?.Invoke("Selected");
            }
            else
            {
                lastMenuTapAt = now;
            }
        }

        private void UpdatePointers()
        {
            if (pointers.Count == 0)
                return;

            float turn = 0f;
            bool hiding = false;

            foreach (KeyValuePair<int, Pointer> pair in pointers)
            {
                Pointer pointer = pair.Value;
                float held = Time.unscaledTime - pointer.startedAt;

                if (!pointer.holdFired && held >= holdTime)
                {
                    pointer.holdFired = true;
                    ZoneHoldStarted?.Invoke(pointer.zone);
                    Announce?.Invoke(ZoneName(pointer.zone) + " held");
                }

                switch (pointer.zone)
                {
                    case TouchZone.TurnLeft:
                        turn -= 1f;
                        break;

                    case TouchZone.TurnRight:
                        turn += 1f;
                        break;

                    case TouchZone.Hand:
                        hiding = pointer.holdFired;
                        break;

                    case TouchZone.Move:
                        if (pair.Key == activeMoveFinger)
                            Move = ReadStick(pointer);
                        break;
                }
            }

            Turn = Mathf.Clamp(turn, -1f, 1f);
            IsHiding = hiding;
        }

        /// <summary>
        /// Floating joystick: wherever the thumb lands becomes the centre,
        /// so the player never has to find a widget they cannot see.
        /// </summary>
        private Vector2 ReadStick(Pointer pointer)
        {
            float radius = Mathf.Max(1f, joystickRadius * Screen.height);
            Vector2 delta = pointer.position - pointer.origin;
            float distance = delta.magnitude;

            if (distance > radius)
            {
                // Push the centre along so the stick keeps up with a long drag.
                pointer.origin = pointer.position - delta / distance * radius;
                delta = delta / distance * radius;
                distance = radius;
            }

            float strength = distance / radius;

            if (strength < deadZone)
                return Vector2.zero;

            float remapped = Mathf.InverseLerp(deadZone, 1f, strength);
            return delta.normalized * remapped;
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private TouchZone ZoneAt(Vector2 screenPosition)
        {
            float nx = screenPosition.x / Mathf.Max(1f, Screen.width);
            float ny = screenPosition.y / Mathf.Max(1f, Screen.height);

            bool left = nx <= cornerSize;
            bool right = nx >= 1f - cornerSize;
            bool top = ny >= 1f - cornerSize;
            bool bottom = ny <= cornerSize;

            if (left && top) return TouchZone.TurnLeft;
            if (right && top) return TouchZone.TurnRight;
            if (left && bottom) return TouchZone.Hand;
            if (right && bottom) return TouchZone.Ear;

            // Centre of the screen, at thumb height: the menu.
            if (Mathf.Abs(nx - 0.5f) <= 0.18f && Mathf.Abs(ny - 0.5f) <= 0.18f)
                return TouchZone.Menu;

            return TouchZone.Move;
        }

        private static string ZoneName(TouchZone zone)
        {
            switch (zone)
            {
                case TouchZone.TurnLeft: return "Turn left";
                case TouchZone.TurnRight: return "Turn right";
                case TouchZone.Hand: return "Hand";
                case TouchZone.Ear: return "Ear";
                case TouchZone.Menu: return "Menu";
                case TouchZone.Move: return string.Empty;
                default: return string.Empty;
            }
        }

        // ------------------------------------------------------------------
        // Keyboard, so the editor stays usable
        // ------------------------------------------------------------------

        private void UpdateKeyboard()
        {
            if (Touch.activeTouches.Count > 0)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            float x = 0f;
            float y = 0f;

            if (keyboard.aKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed) x += 1f;
            if (keyboard.sKey.isPressed) y -= 1f;
            if (keyboard.wKey.isPressed) y += 1f;
            if (x != 0f || y != 0f)
                Move = Vector2.ClampMagnitude(new Vector2(x, y), 1f);

            if (keyboard.qKey.isPressed) Turn = -1f;
            if (keyboard.eKey.isPressed) Turn = 1f;

            if (keyboard.cKey.isPressed || keyboard.leftCtrlKey.isPressed)
                IsHiding = true;
        }
    }
}
