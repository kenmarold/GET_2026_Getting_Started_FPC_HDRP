using UnityEngine;
using UnityEngine.InputSystem;

namespace GreyArea.FPC.U6C3
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraTransform;   // Main Camera (CinemachineBrain)
        [SerializeField] private Transform cameraTarget;      // Player/CameraTarget (local height adjusted)
        [SerializeField] private MovementSettings movementSettings;

        private CharacterController controller;
        private PlayerControls controls;

        private Vector2 moveInput;
        private bool sprintHeld;
        private bool jumpPressed;

        private bool crouchHeld;
        private bool crouchToggled;

        private Vector3 velocity;             // vertical uses velocity.y
        private Vector3 horizontalVelocity;   // smoothed horizontal velocity
        private float coyoteTimer;

        private float currentHeight;
        private float targetHeight;

        private float currentCameraY;
        private float targetCameraY;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            controls = new PlayerControls();

            controls.Player.Move.performed += c => moveInput = c.ReadValue<Vector2>();
            controls.Player.Move.canceled += _ => moveInput = Vector2.zero;

            controls.Player.Sprint.performed += _ => sprintHeld = true;
            controls.Player.Sprint.canceled += _ => sprintHeld = false;

            controls.Player.Jump.performed += _ => jumpPressed = true;

            // Crouch: support both Hold and Toggle behaviors
            controls.Player.Crouch.performed += _ =>
            {
                if (movementSettings != null && movementSettings.crouchIsToggle)
                    crouchToggled = !crouchToggled;
                else
                    crouchHeld = true;
            };

            controls.Player.Crouch.canceled += _ =>
            {
                if (movementSettings != null && !movementSettings.crouchIsToggle)
                    crouchHeld = false;
            };
        }

        private void OnEnable() => controls.Enable();
        private void OnDisable() => controls.Disable();

        private void Start()
        {
            if (movementSettings == null)
            {
                Debug.LogError("PlayerMotor: MovementSettings is not assigned.", this);
                enabled = false;
                return;
            }

            if (cameraTransform == null)
            {
                Debug.LogError("PlayerMotor: cameraTransform (Main Camera) is not assigned.", this);
                enabled = false;
                return;
            }

            if (cameraTarget == null)
            {
                Debug.LogError("PlayerMotor: cameraTarget (CameraTarget transform) is not assigned.", this);
                enabled = false;
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Initialize controller dimensions
            currentHeight = movementSettings.standingHeight;
            targetHeight = currentHeight;

            controller.height = currentHeight;
            controller.center = new Vector3(0f, currentHeight * 0.5f, 0f);

            // Initialize camera target local Y
            currentCameraY = movementSettings.standingCameraY;
            targetCameraY = currentCameraY;

            var localPos = cameraTarget.localPosition;
            localPos.y = currentCameraY;
            cameraTarget.localPosition = localPos;
        }

        private void Update()
        {
            HandleCrouch();

            Move();
            jumpPressed = false; // consume press each frame
        }

        private void HandleCrouch()
        {
            bool wantsCrouch = movementSettings.crouchIsToggle ? crouchToggled : crouchHeld;

            // If trying to stand up, ensure there's clearance above the head
            if (!wantsCrouch)
            {
                if (!CanStandUp())
                {
                    // Force crouch if blocked
                    wantsCrouch = true;
                    if (movementSettings.crouchIsToggle) crouchToggled = true;
                }
            }

            targetHeight = wantsCrouch ? movementSettings.crouchingHeight : movementSettings.standingHeight;
            targetCameraY = wantsCrouch ? movementSettings.crouchingCameraY : movementSettings.standingCameraY;

            // Smoothly move toward target height
            currentHeight = Mathf.Lerp(
                currentHeight,
                targetHeight,
                1f - Mathf.Exp(-movementSettings.crouchTransitionSpeed * Time.deltaTime)
            );

            controller.height = currentHeight;
            controller.center = new Vector3(0f, currentHeight * 0.5f, 0f);

            // Smooth camera target height
            currentCameraY = Mathf.Lerp(
                currentCameraY,
                targetCameraY,
                1f - Mathf.Exp(-movementSettings.crouchTransitionSpeed * Time.deltaTime)
            );

            Vector3 localPos = cameraTarget.localPosition;
            localPos.y = currentCameraY;
            cameraTarget.localPosition = localPos;
        }

        private bool CanStandUp()
        {
            // If we're already at (or very near) standing height, no need to check
            if (controller.height >= movementSettings.standingHeight - 0.01f)
                return true;

            // Spherecast upward from current head position to where the standing head would be.
            // We check the space that would be occupied when expanding from crouch to stand.
            float radius = controller.radius;
            float padding = movementSettings.standClearancePadding;

            Vector3 bottom = transform.position + Vector3.up * (radius);
            float currentTop = controller.height - radius;
            float desiredTop = movementSettings.standingHeight - radius;

            float castDistance = Mathf.Max(0f, (desiredTop - currentTop) + padding);

            // Start near the current top of the controller
            Vector3 castStart = bottom + Vector3.up * currentTop;

            // Ignore triggers so trigger volumes don't block standing
            return !Physics.SphereCast(
                castStart,
                radius,
                Vector3.up,
                out _,
                castDistance,
                ~0,
                QueryTriggerInteraction.Ignore
            );
        }

        private void Move()
        {
            bool grounded = controller.isGrounded;

            // Coyote time
            if (grounded) coyoteTimer = movementSettings.coyoteTime;
            else coyoteTimer -= Time.deltaTime;

            // Stick to ground
            if (grounded && velocity.y < 0f)
                velocity.y = movementSettings.groundedStickForce;

            // Jump (only when NOT crouching, optional rule)
            bool isCrouchingNow = controller.height < movementSettings.standingHeight - 0.05f;
            if (jumpPressed && coyoteTimer > 0f)
            {
                velocity.y = Mathf.Sqrt(movementSettings.jumpHeight * -2f * movementSettings.gravity);
                coyoteTimer = 0f;
            }

            // Gravity
            velocity.y += movementSettings.gravity * Time.deltaTime;

            // Camera-relative horizontal input
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 inputDir = (camRight * moveInput.x + camForward * moveInput.y);
            inputDir = Vector3.ClampMagnitude(inputDir, 1f);

            // Optional: slower speed while crouched
            bool wantsCrouch = movementSettings.crouchIsToggle ? crouchToggled : crouchHeld;
            float speed = sprintHeld ? movementSettings.sprintSpeed : movementSettings.walkSpeed;
            if (wantsCrouch) speed *= movementSettings.crouchSpeedMultiplier;

            Vector3 targetHorizontal = inputDir * speed;

            // Smooth acceleration/deceleration
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetHorizontal,
                movementSettings.acceleration * Time.deltaTime
            );

            // ONE Move call per frame
            Vector3 motion = horizontalVelocity + Vector3.up * velocity.y;
            controller.Move(motion * Time.deltaTime);
        }
    }
}
