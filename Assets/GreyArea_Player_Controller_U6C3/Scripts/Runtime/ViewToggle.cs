using UnityEngine;

namespace GreyArea.FPC.U6C3
{
    public class ViewToggle : MonoBehaviour
    {
        [SerializeField] private GameObject firstPersonCam;
        [SerializeField] private GameObject thirdPersonCam;

        private PlayerControls controls;
        private bool firstPerson = true;

        private void Awake()
        {
            controls = new PlayerControls();
            controls.Player.ToggleView.performed += _ => Toggle();
        }

        private void OnEnable() => controls.Enable();
        private void OnDisable() => controls.Disable();

        private void Start() => Apply();

        private void Toggle()
        {
            firstPerson = !firstPerson;
            Apply();
        }

        private void Apply()
        {
            if (firstPersonCam == null || thirdPersonCam == null)
            {
                Debug.LogError("ViewToggle: Assign FirstPersonCam and ThirdPersonCam in the Inspector.", this);
                return;
            }

            firstPersonCam.SetActive(firstPerson);
            thirdPersonCam.SetActive(!firstPerson);
        }
    }
}
