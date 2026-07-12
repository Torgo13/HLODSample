using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StarterAssetsInputs))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.1f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.5f;
        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -90.0f;

        // cinemachine
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // reset position
        private TransformHandle _handle;
        private Vector3 _initialPosition;
        private const float _minHeight = -1f;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
                #if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
                #else
                return false;
                #endif
            }
        }

#if UNUSED
        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }
#endif // UNUSED

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            // reset position
            _handle = transformHandle;
            _initialPosition = _handle.position;
            if (Physics.Raycast(_initialPosition, Vector3.down, out var hit))
            {
                _initialPosition = hit.point;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            JumpAndGravity(deltaTime);
            GroundedCheck(ResetPosition(_controller, _handle, _initialPosition));
            Move(deltaTime);
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private Vector3 ResetPosition(CharacterController controller, TransformHandle handle, Vector3 initialPosition)
        {
            Vector3 position = handle.position;
            if (position.y < _minHeight)
            {
                bool controllerEnabled = controller.enabled;
                controller.enabled = false;

                position = initialPosition;
                handle.position = initialPosition;

                controller.enabled = controllerEnabled;
            }

            return position;
        }

        private void GroundedCheck(Vector3 position)
        {
            // set sphere position, with offset
            Vector3 spherePosition = position + Vector3.down * GroundedOffset;
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void CameraRotation()
        {
            // if there is an input
            if (_input.look.sqrMagnitude >= _threshold)
            {
                //Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                deltaTimeMultiplier *= RotationSpeed;

                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * deltaTimeMultiplier;

                // clamp our pitch rotation
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                // Update Cinemachine camera target pitch
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move(float deltaTime)
        {
            float magnitude = _input.move.magnitude;
            bool stationary = Mathf.Approximately(0, magnitude);
            
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error-prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (stationary)
                targetSpeed = 0.0f;
            else
                targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a reference to the players current horizontal velocity
            Vector3 velocity = _controller.velocity;
            float currentHorizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;

            const
            float speedOffset = 0.1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                float inputMagnitude = _input.analogMovement ? magnitude : 1f;
                
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection;

            // note: Vector2's != operator uses approximation so is not floating point error-prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (!stationary)
            {
                // move
                Quaternion rotation = transform.rotation;
                inputDirection = (rotation * Vector3.right) * _input.move.x + (rotation * Vector3.forward) * _input.move.y;
            }
            else
            {
                inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y);
            }

            // move the player
            if (_controller.enabled)
            {
                _controller.Move(inputDirection.normalized * (_speed * deltaTime) + Vector3.up * (_verticalVelocity * deltaTime));
            }
        }

        private void JumpAndGravity(float deltaTime)
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= deltaTime;
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f)
                lfAngle += 360f;
            
            if (lfAngle > 360f)
                lfAngle -= 360f;
            
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Grounded
                ? new Color(0.0f, 1.0f, 0.0f, 0.35f)
                : new Color(1.0f, 0.0f, 0.0f, 0.35f);

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(transform.position + Vector3.down * GroundedOffset, GroundedRadius);
        }
    }
}
