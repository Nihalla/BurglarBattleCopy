// Team Sisyphean - Beckham Bagley, Charlie Light, Joe Gollin, Louis Phillips, Ryan Sewell, Tom Roberts

using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace PlayerControllers
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player Variables")]
        public Rigidbody rb;
        private Loot _playerLoot;
        private bool _isVisible;
        [Space]
        [SerializeField] private bool _confusionInvertsMovement;
        [SerializeField] private bool _confusionInvertsCamera;

        [Header("Grounded Variables")]
        [SerializeField] private float _groundedOffset = -0.14f;
        [SerializeField] private float _groundedRadius = 0.5f;
        [SerializeField] private LayerMask _groundLayers;

        [Header("Camera Variables")]
        [SerializeField] private Camera _camera;
        [SerializeField] private float _topClamp = 90.0f;
        [SerializeField] private float _bottomClamp = -90.0f;

        [Header("Multiplayer Variables")]
        public int playerID;
        public PlayerTeam playerTeam;

        [Header("Required Components")]
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private GuardManager _guardManager;
        private InputActionsInputs _inputs;

        [Header("Player Sounds")]
        [SerializeField] private Audio _footstep;
        [SerializeField] private Audio _jumpLand;
        [SerializeField] private Audio _jumpUp;
        [SerializeField] private Audio _confusion;

        // movement variables
        private bool _canMove = true;
        private float _baseMoveSpeed = 8.0f;
        private float _maxMoveSpeed = 20f;
        private float _currentMoveSpeed;
        private float _maxSprintSpeed = 120f;
        private float _speedChangeRate = 10.0f;
        private float _crouchMoveSpeed = 6f;
        private float _jumpHeight = 5f;
        private float _gravity = 2;
        private float _verticalVelocity;
        private float _moveSpeedMultiplier = 1f;
        private bool _isStunned;
        private bool _stunInvincibility;
        private float _currentStunInvincibilityDuration;
        private bool _isConfused;
        private bool _isCaught;
        private CapsuleCollider _capsuleCollider;
        private bool _isCrouched = false;
        private bool _isSprinting = false;
        private bool _canSprint = true;
        private bool _isBeingChainStunned = false;
        private bool _isFrozen = false;

        //Fall vars
        private float _fallTimer;
        private bool _playerLanded = true;


        // grounded variables
        private bool _grounded = true;

        // camera rotation variables
        private float _rotationSpeed = 10.0f;
        private float _rotationVelocity;
        private float _targetPitch;
        private float _baseFOV;
        private Vector3 _baseCameraPos;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // constant variables
        private const float THRESHHOLD = 0.01f;
        private const float JUMP_TIMEOUT = 0.1f;
        private const float FALL_TIMEOUT = 0.15f;
        private const float TERMINAL_VELOCITY = -50;
        private const float STUN_INVINCIBILITY_DURATION = 5f;
        private const float START_FALL_TIMER = 1.0f;
        private const float FALL_MULTIPLIER = 4;
        private const float SPRINT_BONUS_FOV = 20.0f;
        private const float CROUCH_TIMER = 0.2f;
        private const float SPRINT_TIMER = 0.2f;

        // invisibility timer
        private float _invisibilityTimer;
        private float _currentInvisibilityDurationTimer;

        //crouch timer
        private float _currentCrouchTimer = 0.2f;

        //Sprint timer
        private float _currentSprintTimer = 0.2f;

        // Footstep timer
        private float _footstepTimer = 0f;
        private float _footstepTimerMax = 0.2f;

        //Movespeed multiplier timer
        private float _moveSpeedMultiplierTimer = 0.2f;
        private float _currentMoveSpeedMultiplierTimer = 0.2f;
        private bool _moveSpeedMultiplierApplied = false;

        //StunDuration timer
        private float _stunDurationTimer;
        private float _currentStunDurationTimer;

        // confusion timer
        private float _confusionTimer;
        private float _currentConfusionDurationTimer;
        private bool _confusionPlayOnce = false;

        //Animator variables
        [Header("Animation Components")]
        public Animator animator;
        private PlayerHashing _playerHash;
        [SerializeField] private GameObject _blueTeam;
        [SerializeField] private GameObject _redTeam;

        [SerializeField] private GameObject _blueTeamMarker;
        [SerializeField] private GameObject _redTeamMarker;

        [Header("Effects")]
        [SerializeField] private GameObject _stunEffect;
        [SerializeField] private ParticleSystem _freezeEffect;

        public static bool IsDisabled { get; set; }

        // enums
        public enum PlayerTeam
        {
            TEAM_ONE = 0,
            TEAM_TWO,
            UNKNOWN = -1
        }

        private void Awake()
        {
            if (playerTeam == PlayerTeam.TEAM_ONE)
            {
                _blueTeam.SetActive(true);
                _blueTeamMarker.SetActive(true);
                animator = _blueTeam.GetComponent<Animator>();
            } 
            else if (playerTeam == PlayerTeam.TEAM_TWO)
            {
                _redTeam.SetActive(true);
                _redTeamMarker.SetActive(true);
                animator = _redTeam.GetComponent<Animator>();
            }
            else // NOTE(Zack): we have a default case so we don't get a null ref
            {
                _blueTeam.SetActive(true);
                _blueTeamMarker.SetActive(true);
                animator = _blueTeam.GetComponent<Animator>();
            }

            _playerHash = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerHashing>();

            animator?.SetLayerWeight(1, 1f);

            _freezeEffect.Stop();
        }


        private void Start()
        {
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _inputs = GetComponent<InputActionsInputs>();
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            _playerInput = GetComponent<PlayerInput>();
            _playerLoot = GetComponent<Loot>();

            _isVisible = true;

            _baseFOV = _camera.fieldOfView;
            _currentMoveSpeed = _baseMoveSpeed;
            _baseCameraPos = _camera.transform.localPosition;

            _inputs.SetDevice(playerID);
            _inputs.SetUpDevice();
            _playerInput.actions = _inputs.GetActions().asset;


            // reset our timeouts on start
            _jumpTimeoutDelta = JUMP_TIMEOUT;
            _fallTimeoutDelta = FALL_TIMEOUT;

            _crouchMoveSpeed = _baseMoveSpeed / 2;

            _fallTimer = START_FALL_TIMER;

        }

        // NOTE(Zack): all of these functions have been moved out of the FixedUpdate, as none of them are doing operations on the
        // Rigidbody. This speeds things up as FixedUpdate is called between 2-6 times per player __each__ frame, meaning that these
        // functions were doing unnecessary work, as the values would not have changed from the players previous state.
        private void Update()
        {
            if (Time.timeScale > 0f)
            {
                if (!IsDisabled)
                {
                    Crouch();
                    InvertCrouch();
                    Sprint();
                    CameraRotation();
                }

                MoveSpeedMultiplier();
                FOVCheck();
                InvisibiltyDuration();
                if (_isStunned && !_isBeingChainStunned)
                {
                    StunPlayerDuration();
                }
                ConfusePlayerDuration();
                PlayerShareLoot();
                // PlayerAudio();
                AnimationState();
                OnStun();
                OnFreeze();
                StunImmunity();
            }
        }

        private void FixedUpdate()
        {
            if (Time.timeScale > 0 && !IsDisabled)
            {
                Jump();
                GroundedCheck();
                Move();
            }
        }

        private void AnimationState()
        {
            // BUG(Felix): this is still constantly throwing errors, so it has been commented out
            // if it requires setup please put it on the wiki
            if (_inputs.move != Vector2.zero)
            {
                animator?.SetBool(_playerHash.walkBool, true);
            }
            else
            {
                animator?.SetBool(_playerHash.walkBool, false);
            }

            if (_inputs.move != Vector2.zero && _isCrouched)
            {
                animator?.SetBool(_playerHash.crouchWalkBool, true);
            }
            else
            {
                animator?.SetBool(_playerHash.crouchWalkBool, false);
            }

            if (_inputs.move == Vector2.zero && _isCrouched)
            {
                animator?.SetBool(_playerHash.crouchIdleBool, true);
            }
            else
            {
                animator?.SetBool(_playerHash.crouchIdleBool, false);
            }

            if (_inputs.move != Vector2.zero && _isSprinting)
            {
                animator?.SetBool(_playerHash.sprintBool, true);
            }
            else
            {
                animator?.SetBool(_playerHash.sprintBool, false);
            }
        }

        /// <summary>
        /// Gets the camera object attached to the player
        /// </summary>
        /// <returns>Camera</returns>
        public Camera GetPlayerCamera()
        {
            return _camera;
        }

        private void StunImmunity()
        {
            if(_stunInvincibility)
            {
                if (_currentStunInvincibilityDuration < 0)
                {
                    _stunInvincibility = false;
                    _currentStunInvincibilityDuration = STUN_INVINCIBILITY_DURATION;
                }
                else
                {
                    _currentStunInvincibilityDuration -= Time.deltaTime;
                }
            }
        }
        /// <summary>
        /// Sphere casts towards the ground and checks if grounded in order to jump. Uses ground layers to check. Sets _grounded to true if on layer.
        /// </summary>
        private void GroundedCheck()
        {
            float raycastDistance = 0.5f;
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z);
            _grounded = Physics.Raycast(spherePosition, Vector3.down, raycastDistance, _groundLayers);
            if (_grounded && !_playerLanded)
            {
                AudioManager.PlayPlayerSpace(_jumpLand, playerID);
                _playerLanded = true;
            }
            else if (rb.velocity.y < -0.5)
            {
                _playerLanded = false;
            }
            if (rb.velocity.y < 0 && rb.velocity.y > TERMINAL_VELOCITY)
            {
                rb.AddForce(Vector3.down * _fallTimer, ForceMode.Impulse);
                _fallTimer += Time.fixedDeltaTime * FALL_MULTIPLIER;
            }
        }

        /// <summary>
        /// Rotates and clamps the camera rotation. Works from player input.  
        /// </summary>
        private void CameraRotation()
        {
            if (!_isStunned)
            {
                // if there is an input
                if (_inputs.look.sqrMagnitude >= THRESHHOLD)
                {
                    //Don't multiply mouse input by Time.deltaTime
                    float deltaTimeMultiplier = Time.deltaTime * 20;

                    _targetPitch += _inputs.look.y * -_rotationSpeed * deltaTimeMultiplier;
                    _rotationVelocity = _inputs.look.x * _rotationSpeed * deltaTimeMultiplier;

                    // clamp our pitch rotation
                    _targetPitch = ClampAngle(_targetPitch, _bottomClamp, _topClamp);

                    // Update camera target pitch
                    _camera.transform.localRotation = Quaternion.Euler(_targetPitch, 0.0f, 0.0f);

                    // rotate the player left and right
                    transform.Rotate(Vector3.up * _rotationVelocity);
                }
            }
        }

        /// <summary>
        /// Moves the player through physics based movement using rigidbodys. Normalizes the movement vector.
        /// </summary>
        private void Move()
        {
            if (!_canMove)
            {
                return;
            }

            // set target speed based on move speed.
            float targetSpeed = _currentMoveSpeed;

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_inputs.move == Vector2.zero)
            {
                targetSpeed = 0.0f;
            }
            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = 1f;

            inputMagnitude = _inputs.move.magnitude;

            // normalise input direction
            if (!_isStunned)
            {
                Vector3 inputDirection = new Vector3(_inputs.move.x, 0.0f, _inputs.move.y).normalized;

                // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                // if there is a move input rotate player when the player is moving
                if (_inputs.move != Vector2.zero)
                {
                    // move
                    inputDirection = transform.right * _inputs.move.x + transform.forward * _inputs.move.y;
                }

                // move the player
                if (Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z) < _maxMoveSpeed)
                {
                    // NOTE(WSWhitehouse): Multiplication order is important here; this is the most efficient way of doing it...
                    rb.AddForce(inputDirection.normalized * (_currentMoveSpeed * _moveSpeedMultiplier * 10f), ForceMode.Force);
                }
            }
        }

        /// <summary>
        /// toggles the sprint and adjust the movment speed uses the isSprinting and isCrouching to determine if the 
        /// movement speed should be increased or not.
        /// </summary>
        private void Sprint()
        {
            if (!_isCrouched)
            {
                if (_inputs.sprint && _canSprint && _grounded)
                {
                    _isSprinting = !_isSprinting;
                    _inputs.sprint = false;
                }

                if (_inputs.move.magnitude == 0)
                {
                    if (_currentSprintTimer < 0)
                    {
                        _isSprinting = false;
                        _currentSprintTimer = SPRINT_TIMER;
                    }
                    else
                    {
                        _currentSprintTimer -= Time.deltaTime;
                    }
                }


                if (_isSprinting && !_isCrouched)
                {
                    _currentMoveSpeed = _maxMoveSpeed;
                }
                else
                {
                    _currentMoveSpeed = _baseMoveSpeed;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void PlayerAudio()
        {
            /*/// NOTE FOR SELF: Change 0.5 to not a magic number, curretnly to check if moving
            if (Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z) > 0.5f)
            {
                _footstepTimer += Time.deltaTime;
                if (_footstepTimer > _footstepTimerMax)
                {
                    AudioManager.PlayPlayerSpace(_footstep, playerID);
                    _footstepTimer = 0f;
                }
            }
            *//*else
            {
                _footstepTimer = 0;
            }*/


        }

        /// <summary>
        /// Checks current speed and base speed and changes the FOV if the current is greater than the base movement speed.
        /// </summary>

        private void FOVCheck()

        {

            if (_isSprinting)

            {

                FOVAccelerationChange(_baseFOV + SPRINT_BONUS_FOV);

            }

            else

            {

                FOVAccelerationChange(_baseFOV);

            }

        }



        /// <summary>
        /// Function that adjust the fov with the given parameter. The FOV increases gradually until it reaches its desired value.
        /// </summary>
        /// <param name="targetFOV"></param>

        private void FOVAccelerationChange(float targetFOV)
        {
            float stepIncrement = 50f;
            if (targetFOV == _baseFOV)
            {
                if ((_camera.fieldOfView >= targetFOV))
                {
                    _camera.fieldOfView -= (stepIncrement * Time.deltaTime);
                }
            }
            else
            {
                if ((_camera.fieldOfView <= targetFOV))
                {
                    _camera.fieldOfView += (stepIncrement * Time.deltaTime);

                }

            }

        }

        /// <summary>
        /// Allows the player to jump based on if _grounded.
        /// </summary>
        private void Jump()
        {
            if (!_isStunned)
            {
                if (_grounded)
                {
                    _fallTimer = START_FALL_TIMER;
                    rb.drag = 10f;
                    // reset the fall timeout timer
                    _fallTimeoutDelta = FALL_TIMEOUT;

                    // stop our velocity dropping infinitely when grounded
                    if (_verticalVelocity < 0.0f)
                    {
                        _verticalVelocity = -2f;
                    }

                    // Jump
                    if (_inputs.jump && _jumpTimeoutDelta <= 0.0f)
                    {
                        // the square root of H * -2 * G = how much velocity needed to reach desired height
                        //_verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                        rb.AddForce(transform.up * _jumpHeight, ForceMode.Impulse);
                        AudioManager.PlayPlayerSpace(_jumpUp, playerID);
                    }


                    // jump timeout
                    if (_jumpTimeoutDelta >= 0.0f)
                    {
                        _jumpTimeoutDelta -= Time.fixedDeltaTime;
                    }
                }
                else
                {
                    // reset the jump timeout timer
                    _jumpTimeoutDelta = JUMP_TIMEOUT;

                    // fall timeout
                    if (_fallTimeoutDelta >= 0.0f)
                    {
                        _fallTimeoutDelta -= Time.fixedDeltaTime;
                    }

                    // if we are not grounded, do not jump
                    _inputs.jump = false;
                }
            }

            /* // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
             if (_verticalVelocity < _terminalVelocity)
             {
                 _verticalVelocity += _gravity * Time.deltaTime;
             }*/
        }

        /// <summary>
        /// Allows the player to crouch. Sets _isCrouched.
        /// </summary>
        private void Crouch()
        {

            if (_isCrouched)
            {
                _isSprinting = false;
                //crouch
                _camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, new Vector3(_baseCameraPos.x,
                                                         _baseCameraPos.y - 0.5f,
                                                         _baseCameraPos.z), 0.1f);

                _currentMoveSpeed = _crouchMoveSpeed;
            }
            else
            {
                _camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, _baseCameraPos, 0.1f);

                _currentMoveSpeed = _baseMoveSpeed;
            }
        }

        /// <summary>

        /// Uses a timer to invert the crouch bool, required for the crouch toggle input.

        /// </summary>
        private void InvertCrouch()
        {
            if (_currentCrouchTimer < 0)
            {
                if (_inputs.crouch)
                {
                    _isCrouched = !_isCrouched;
                    _currentCrouchTimer = CROUCH_TIMER;
                }
            }
            else
            {
                _currentCrouchTimer -= Time.deltaTime;
            }

        }

        /// <summary>
        /// Clamps an angle between two values
        /// </summary>
        /// <param name="lfAngle"> The angle to clamp </param>
        /// <param name="lfMin"> The minimum angle </param>
        /// <param name="lfMax"> The maximum angle </param>
        /// <returns></returns>
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f)
            {
                lfAngle += 360f;
            }

            if (lfAngle > 360f)
            {
                lfAngle -= 360f;
            }
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void PlayerShareLoot()
        {
            _playerLoot.ShareLoot();
        }


        // Getters and Setters
        public void SetInvisibleForTimer(float time)
        {
            _currentInvisibilityDurationTimer = time;
            _isVisible = false;

            if (playerTeam == PlayerTeam.TEAM_ONE)
            {
                foreach (SkinnedMeshRenderer _renderTemp in _blueTeam.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    _renderTemp.enabled = false;
                }
            }

            if (playerTeam == PlayerTeam.TEAM_TWO)
            {
                foreach (SkinnedMeshRenderer _renderTemp in _redTeam.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    _renderTemp.enabled = false;
                }
            }

            GuardManager.IgnorePlayer(gameObject);
        }
        private void InvisibiltyDuration()
        {
            if (_currentInvisibilityDurationTimer < 0)
            {
                _isVisible = true;

                if (playerTeam == PlayerTeam.TEAM_ONE)
                {
                    foreach (SkinnedMeshRenderer _renderTemp in _blueTeam.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        _renderTemp.enabled = true;
                    }
                }
                
                if (playerTeam == PlayerTeam.TEAM_TWO)
                {
                    foreach (SkinnedMeshRenderer _renderTemp in _redTeam.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        _renderTemp.enabled = true;
                    }
                }

                GuardManager.UnignorePlayer(gameObject);
            }
            else
            {
                _currentInvisibilityDurationTimer -= Time.deltaTime;
            }
        }

        public bool GetIsVisible()
        {
            return _isVisible;
        }

        public void SetIsVisible(bool newVisibility)
        {
            _isVisible = newVisibility;
        }

        public InputActionsInputs GetInputActionsInputs()
        {
            return _inputs;
        }

        public float GetCurrentMoveSpeed()
        {
            return _currentMoveSpeed;
        }
        public void SetCurrentMoveSpeed(float newCurrentMoveSpeed)
        {
            _currentMoveSpeed = newCurrentMoveSpeed;
        }
        public float GetBaseMoveSpeed()
        {
            return _baseMoveSpeed;
        }
        public void SetBaseMoveSpeed(float newBaseMoveSpeed)
        {
            _baseMoveSpeed = newBaseMoveSpeed;
        }
        public void SetMoveSpeedMultiplierForTimer(float newMoveSpeedMultiplier, float time)
        {
            _currentMoveSpeedMultiplierTimer = time;
            _moveSpeedMultiplier = newMoveSpeedMultiplier;

        }

        private void MoveSpeedMultiplier()
        {
            if (_currentMoveSpeedMultiplierTimer < 0)
            {
                _moveSpeedMultiplier = 1f;
            }
            else
            {
                _currentMoveSpeedMultiplierTimer -= Time.deltaTime;
            }
        }
        /// <summary>
        /// Stuns the player by setting the _isStunned bool to true for the amount an amount of time specified in the parenthesis
        /// </summary>
        /// <param name="time"></param>
        public void StunPlayerForTimer(float time)
        {
            if (!_stunInvincibility)
            {
                _currentStunDurationTimer = time;
                _isStunned = true;
                _stunInvincibility = true;

                InputDevices.Devices[playerID].RumblePulse(0.2f, 0.2f, _currentStunDurationTimer, this);
            }
        }

        /// <summary>
        /// Updates the stun timer and when it reaches 0 un stun the player
        /// </summary>
        private void StunPlayerDuration()
        {
            if (_currentStunDurationTimer < 0)
            {
                
                _isStunned = false;
                
            }
            else
            {
                _currentStunDurationTimer -= Time.deltaTime;
            }
        }
        //toggle friendly stun state for use when a guard is holding onto a player
        public void SetStunnedState(bool newStunnedState)
        {
            _isStunned = newStunnedState;
            _isBeingChainStunned = newStunnedState;
            //_stunInvincibility = true;
        }

        public void ConfusePlayerForTimer(float time)
        {
            _currentConfusionDurationTimer = time;
            _isConfused = true;

            if (!_confusionPlayOnce)
            {
                AudioManager.PlayPlayerSpace(_confusion, playerID);
                _confusionPlayOnce = true;
                // //Debug.Log("Beep boop im confused1");
            }

            if (_confusionInvertsMovement)
            {
                // Initially setting move to the opposite of what it was as otherwise player can hold forward and continue moving forward
                _inputs.move = new Vector2(-_inputs.move.x, -_inputs.move.y);
                _inputs.SetMovementInverseControls(true);
            }

            if (_confusionInvertsCamera)
            {
                _inputs.SetLookInverseControls(true);
            }
        }

        private void ConfusePlayerDuration()
        {
            if (_currentConfusionDurationTimer < 0)
            {
                _isConfused = false;
                _confusionPlayOnce = false;

                if (_confusionInvertsMovement)
                {
                    _inputs.SetMovementInverseControls(false);
                }

                if (_confusionInvertsCamera)
                {
                    _inputs.SetLookInverseControls(false);
                }
            }

            else
            {
                _currentConfusionDurationTimer -= Time.deltaTime;
            }
        }

        private void OnStun()
        {
            if (_isStunned)
            {
                _stunEffect.SetActive(true);
            }
            else
            {
                _stunEffect.SetActive(false);
            }
        }

        public bool GetStunned()
        {
            return _isStunned;
        }

        private void OnFreeze()
        {
            if (_isFrozen)
            {
                _freezeEffect.Play();
            }
            else
            {
                _freezeEffect.Stop();
            }
        }

        public void SetFreeze(bool state)
        {
            _isFrozen = state;
        }

        public void SetCanSprint(bool newCanSprint)
        {
            _canSprint = newCanSprint;
            if (!_canSprint)
            {
                _isSprinting = false;
            }
        }
        public void SetCaught(bool caught)
        {
            _isCaught = caught;
        }
        public bool GetCaught()
        {
            return _isCaught;
        }
        public void SetMaxMoveSpeed(float newMaxMoveSpeed)
        {
            _maxMoveSpeed = newMaxMoveSpeed;
        }
        public float GetRotationSpeed()
        {
            return _rotationSpeed;
        }
        public void SetRotationSpeed(float newRotationSpeed)
        {
            _rotationSpeed = newRotationSpeed;
        }
        public float GetSpeedChangeRate()
        {
            return _speedChangeRate;
        }
        public void SetSpeedChangeRate(float newSpeedChangeRate)
        {
            _speedChangeRate = newSpeedChangeRate;
        }
        public float GetJumpHeight()
        {
            return _jumpHeight;
        }
        public void SetJumpHeight(float newJumpHeight)
        {
            _jumpHeight = newJumpHeight;
        }

        public float GetBaseFOV()
        {
            return _baseFOV;
        }
        public void SetBaseFOV(float newBaseFOV)
        {
            _baseFOV = newBaseFOV;
        }
        public PlayerTeam GetTeam()
        {
            return playerTeam;
        }
        public void SetTeam(PlayerTeam newTeam)
        {
            playerTeam = newTeam;
        }
        public int GetPlayerID()
        {
            return playerID;
        }
        public void SetPlayerID(int newID)
        {
            playerID = newID;
        }

        /// <summary>
        /// Sets the players position to the Vector3 And te rotation to the Quaternion 
        /// </summary>
        /// <param name="newPosition"></param>
        /// <param name="newRotation"></param>
        public void SetPlayerPosition(Vector3 newPosition, Quaternion newRotation)
        {
            _capsuleCollider.enabled = false;
            transform.SetPositionAndRotation(newPosition, newRotation);
            _capsuleCollider.enabled = true;
        }

        /// <summary>
        /// Alternative to setting the player based only on a Vector3 without rotation
        /// </summary>
        /// <param name="newTransform"></param>
        public void SetPlayerPosition(Transform newTransform)
        {
            _capsuleCollider.enabled = false;
            transform.SetPositionAndRotation(newTransform.position, newTransform.rotation);
            _capsuleCollider.enabled = true;
        }

        public float GetPlayerColliderRadius()
        {
            return _capsuleCollider.radius;
        }

        public float GetPlayerColliderHeight()
        {
            return _capsuleCollider.height;
        }

        public void SetPlayerCanMove(bool canMove)
        {
            _canMove = canMove;
        }

        public InputActionsInputs GetPlayerInput()
        {
            return _inputs;
        }
    }
}
