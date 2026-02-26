using UnityEngine;

namespace GreyArea.FPC.U6C3
{
    [CreateAssetMenu(
        fileName = "FPC_MovementSettings",
        menuName = "GreyArea/FPC U6C3/Movement Settings",
        order = 1)]
    public class MovementSettings : ScriptableObject
    {
        [Header("Movement")]
        [Min(0f)] public float walkSpeed = 5f;
        [Min(0f)] public float sprintSpeed = 8f;
        [Min(0f)] public float acceleration = 18f;

        [Header("Jump & Gravity")]
        [Min(0f)] public float jumpHeight = 1.2f;
        public float gravity = -24f;

        [Header("Grounding Helpers")]
        public float groundedStickForce = -2f;
        [Min(0f)] public float coyoteTime = 0.1f;

        [Header("Crouch")]
        public bool crouchIsToggle = false; // false = hold, true = toggle
        [Min(0.5f)] public float standingHeight = 1.8f;
        [Min(0.5f)] public float crouchingHeight = 1.2f;
        [Min(0f)] public float crouchTransitionSpeed = 12f;

        [Tooltip("Local Y height of CameraTarget when standing.")]
        public float standingCameraY = 1.6f;

        [Tooltip("Local Y height of CameraTarget when crouching.")]
        public float crouchingCameraY = 1.1f;

        [Tooltip("Extra headroom check when trying to stand.")]
        public float standClearancePadding = 0.05f;

        [Range(0.1f, 1f)]
        public float crouchSpeedMultiplier = 0.6f;

    }
}
