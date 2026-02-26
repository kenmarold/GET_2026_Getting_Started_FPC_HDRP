using UnityEngine;

namespace GreyArea.FPC.U6C3
{
    [CreateAssetMenu(
        fileName = "FPC_AimSettings",
        menuName = "GreyArea/FPC U6C3/Aim Settings",
        order = 3)]
    public class AimSettings : ScriptableObject
    {
        [Header("Field of View")]
        [Min(1f)] public float normalFov = 75f;
        [Min(1f)] public float aimFov = 60f;
        [Min(0f)] public float fovLerpSpeed = 12f;

        [Header("TPS Shoulder Offset (X)")]
        [Tooltip("Normal third-person shoulder offset X (right shoulder positive).")]
        public float tpsNormalShoulderX = 0.5f;

        [Tooltip("Aiming shoulder offset X (smaller = closer to center).")]
        public float tpsAimShoulderX = 0.2f;

        [Min(0f)] public float shoulderLerpSpeed = 12f;
    }
}
