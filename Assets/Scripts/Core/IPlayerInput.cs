using UnityEngine;

namespace TheLastCrew.Player
{
    /// <summary>
    /// Anything that can drive the player. Touch, keyboard, a test bot -
    /// the motor never knows which one it is talking to.
    /// </summary>
    public interface IPlayerInput
    {
        /// <summary>Walk direction. x = strafe, y = forward. Length 0..1.</summary>
        Vector2 Move { get; }

        /// <summary>Turn rate, -1 (left) to +1 (right).</summary>
        float Turn { get; }

        /// <summary>True while the player is trying to stay quiet.</summary>
        bool IsHiding { get; }
    }
}
