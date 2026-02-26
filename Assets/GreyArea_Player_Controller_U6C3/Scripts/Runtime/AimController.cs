using System;
using System.Reflection;
using UnityEngine;

namespace GreyArea.FPC.U6C3
{
    public class AimController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;             // Drag Main Camera here
        [SerializeField] private AimSettings aimSettings;       // Drag FPC_AimSettings_Default here
        [SerializeField] private GameObject cmThirdPerson;      // Drag CM_ThirdPerson here (for shoulder offset)

        private PlayerControls controls;
        private bool isAiming;

        private float currentFov;
        private float targetFov;

        // CinemachineCameraOffset access (reflection, robust across versions)
        private Component cameraOffsetComponent;
        private Func<Vector3> getOffset;
        private Action<Vector3> setOffset;

        private float currentShoulderAbs; // absolute shoulder magnitude (positive number)
        private float targetShoulderAbs;

        private bool rightShoulder = true; // assume right by default; optional integration with ShoulderSwap later

        private void Awake()
        {
            controls = new PlayerControls();

            controls.Player.Aim.performed += _ => isAiming = true;
            controls.Player.Aim.canceled += _ => isAiming = false;

            // controls.Player.Aim.performed += _ => Debug.Log("AIM PERFORMED");
            // controls.Player.Aim.canceled += _ => Debug.Log("AIM CANCELED");
        }

        private void OnEnable() => controls.Enable();
        private void OnDisable() => controls.Disable();

        private void Start()
        {
            if (mainCamera == null)
            {
                Debug.LogError("AimController: Main Camera is not assigned.", this);
                enabled = false;
                return;
            }

            if (aimSettings == null)
            {
                Debug.LogError("AimController: AimSettings is not assigned.", this);
                enabled = false;
                return;
            }

            // Initialize FOV
            currentFov = aimSettings.normalFov;
            targetFov = currentFov;
            mainCamera.fieldOfView = currentFov;

            // Setup shoulder offset control (TPS only)
            if (cmThirdPerson != null)
            {
                cameraOffsetComponent = cmThirdPerson.GetComponent("CinemachineCameraOffset");
                if (cameraOffsetComponent != null && TryBindOffsetAccessors(cameraOffsetComponent, out getOffset, out setOffset))
                {
                    // Start at normal shoulder
                    currentShoulderAbs = Mathf.Abs(aimSettings.tpsNormalShoulderX);
                    targetShoulderAbs = currentShoulderAbs;
                    ApplyShoulder(currentShoulderAbs);
                }
                else
                {
                    // Not fatal: aim will still work via FOV
                    cameraOffsetComponent = null;
                }
            }
        }

        private void Update()
        {
            // FOV target
            targetFov = isAiming ? aimSettings.aimFov : aimSettings.normalFov;
            currentFov = Mathf.Lerp(currentFov, targetFov, 1f - Mathf.Exp(-aimSettings.fovLerpSpeed * Time.deltaTime));
            mainCamera.fieldOfView = currentFov;

            // TPS shoulder target (if we have CinemachineCameraOffset)
            if (cameraOffsetComponent != null)
            {
                targetShoulderAbs = isAiming ? Mathf.Abs(aimSettings.tpsAimShoulderX) : Mathf.Abs(aimSettings.tpsNormalShoulderX);
                currentShoulderAbs = Mathf.Lerp(currentShoulderAbs, targetShoulderAbs, 1f - Mathf.Exp(-aimSettings.shoulderLerpSpeed * Time.deltaTime));
                ApplyShoulder(currentShoulderAbs);
            }
        }

        private void ApplyShoulder(float absX)
        {
            // Preserve left/right shoulder by sign
            float signedX = rightShoulder ? absX : -absX;

            Vector3 offset = getOffset();
            offset.x = signedX;
            setOffset(offset);
        }

        // Robustly find an "offset" Vector3 member on CinemachineCameraOffset (works across CM3 versions)
        private static bool TryBindOffsetAccessors(Component comp, out Func<Vector3> getter, out Action<Vector3> setter)
        {
            getter = null;
            setter = null;

            var t = comp.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // Property named Offset or containing offset
            var prop = t.GetProperty("Offset", flags) ??
                       Array.Find(t.GetProperties(flags), p =>
                           p.PropertyType == typeof(Vector3) && p.CanRead && p.CanWrite &&
                           p.Name.IndexOf("offset", StringComparison.OrdinalIgnoreCase) >= 0);

            if (prop != null)
            {
                getter = () => (Vector3)prop.GetValue(comp);
                setter = v => prop.SetValue(comp, v);
                return true;
            }

            // Field named Offset/m_Offset or containing offset
            var field = t.GetField("Offset", flags) ??
                        t.GetField("m_Offset", flags) ??
                        Array.Find(t.GetFields(flags), f =>
                            f.FieldType == typeof(Vector3) &&
                            f.Name.IndexOf("offset", StringComparison.OrdinalIgnoreCase) >= 0);

            if (field != null)
            {
                getter = () => (Vector3)field.GetValue(comp);
                setter = v => field.SetValue(comp, v);
                return true;
            }

            return false;
        }
    }
}
