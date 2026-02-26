using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GreyArea.FPC.U6C3
{
    public class ShoulderSwap : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject cmThirdPerson; // Drag CM_ThirdPerson here

        [Header("Over-Shoulder Offset")]
        [SerializeField] private float shoulderOffsetX = 0.5f;
        [SerializeField] private float offsetY = 0f;
        [SerializeField] private float offsetZ = 0f;
        [SerializeField] private float smoothSpeed = 12f;

        [Header("Defaults")]
        [SerializeField] private bool startRightShoulder = true;

        private PlayerControls controls;
        private bool rightShoulder;

        private float currentX;
        private float targetX;

        private Component cameraOffsetComponent;

        // We'll store a getter/setter for a Vector3 "offset-like" member
        private Func<Vector3> getOffset;
        private Action<Vector3> setOffset;

        private void Awake()
        {
            controls = new PlayerControls();
            controls.Player.ShoulderSwap.performed += _ => ToggleShoulder();
        }

        private void OnEnable() => controls.Enable();
        private void OnDisable() => controls.Disable();

        private void Start()
        {
            if (cmThirdPerson == null)
            {
                Debug.LogError("ShoulderSwap: CM_ThirdPerson is not assigned.", this);
                enabled = false;
                return;
            }

            // Find CinemachineCameraOffset extension on CM_ThirdPerson
            cameraOffsetComponent = cmThirdPerson.GetComponent("CinemachineCameraOffset");
            if (cameraOffsetComponent == null)
            {
                Debug.LogError(
                    "ShoulderSwap: CinemachineCameraOffset not found on CM_ThirdPerson. " +
                    "Add it via CM_ThirdPerson -> Cinemachine Camera -> Add Extension -> CinemachineCameraOffset.",
                    cmThirdPerson
                );
                enabled = false;
                return;
            }

            if (!TryBindOffsetAccessors(cameraOffsetComponent, out getOffset, out setOffset))
            {
                Debug.LogError(
                    "ShoulderSwap: Could not find a usable Vector3 offset field/property on CinemachineCameraOffset. " +
                    "Cinemachine API/serialization may have changed in this version.",
                    cmThirdPerson
                );
                enabled = false;
                return;
            }

            // Initialize
            rightShoulder = startRightShoulder;
            currentX = rightShoulder ? shoulderOffsetX : -shoulderOffsetX;
            targetX = currentX;

            ApplyOffset(currentX);
        }

        private void Update()
        {
            currentX = Mathf.Lerp(currentX, targetX, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
            ApplyOffset(currentX);
        }

        private void ToggleShoulder()
        {
            rightShoulder = !rightShoulder;
            targetX = rightShoulder ? shoulderOffsetX : -shoulderOffsetX;
        }

        private void ApplyOffset(float x)
        {
            Vector3 offset = getOffset();
            offset.x = x;
            offset.y = offsetY;
            offset.z = offsetZ;
            setOffset(offset);
        }

        /// <summary>
        /// Finds a Vector3 "offset" member on the CinemachineCameraOffset component.
        /// Works across Cinemachine 3 variants by searching for:
        /// - Property named Offset / containing "offset"
        /// - Field named Offset / m_Offset / containing "offset"
        /// - Fallback: first Vector3 property/field
        /// </summary>
        private static bool TryBindOffsetAccessors(Component comp, out Func<Vector3> getter, out Action<Vector3> setter)
        {
            getter = null;
            setter = null;

            var t = comp.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // 1) Try common property names first
            PropertyInfo prop =
                t.GetProperty("Offset", flags) ??
                t.GetProperties(flags).FirstOrDefault(p =>
                    p.PropertyType == typeof(Vector3) &&
                    p.CanRead && p.CanWrite &&
                    p.Name.IndexOf("offset", StringComparison.OrdinalIgnoreCase) >= 0);

            if (prop != null)
            {
                getter = () => (Vector3)prop.GetValue(comp);
                setter = v => prop.SetValue(comp, v);
                return true;
            }

            // 2) Try common field names next
            FieldInfo field =
                t.GetField("Offset", flags) ??
                t.GetField("m_Offset", flags) ??
                t.GetFields(flags).FirstOrDefault(f =>
                    f.FieldType == typeof(Vector3) &&
                    f.Name.IndexOf("offset", StringComparison.OrdinalIgnoreCase) >= 0);

            if (field != null)
            {
                getter = () => (Vector3)field.GetValue(comp);
                setter = v => field.SetValue(comp, v);
                return true;
            }

            // 3) Fallback: any Vector3 property (rare)
            prop = t.GetProperties(flags).FirstOrDefault(p => p.PropertyType == typeof(Vector3) && p.CanRead && p.CanWrite);
            if (prop != null)
            {
                getter = () => (Vector3)prop.GetValue(comp);
                setter = v => prop.SetValue(comp, v);
                return true;
            }

            // 4) Fallback: any Vector3 field (rare)
            field = t.GetFields(flags).FirstOrDefault(f => f.FieldType == typeof(Vector3));
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
