using UnityEngine;

namespace GreyArea.FPC.U6C3
{
    [CreateAssetMenu(
        fileName = "FPC_CameraSettings",
        menuName = "GreyArea/FPC U6C3/Camera Settings",
        order = 2)]
    public class CameraSettings : ScriptableObject
    {
        [Header("Look Sensitivity (Cinemachine Input Axis Controller Gain)")]
        [Tooltip("Gain applied to Look X (Pan) on Cinemachine Input Axis Controller.")]
        public float lookXGain = 10f;

        [Tooltip("Gain applied to Look Y (Tilt). Use negative to invert Y.")]
        public float lookYGain = 10f;

        [Header("Field of View")]
        [Min(1f)] public float baseFov = 75f;
        [Min(0f)] public float sprintFovBonus = 10f;
        [Min(0f)] public float fovLerpSpeed = 10f;

        [Header("Cursor")]
        public bool lockCursor = true;
    }
}
