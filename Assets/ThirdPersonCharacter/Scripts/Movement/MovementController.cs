﻿/* MovementControllerP: James Agerton 2021
 * 
 * Description: 
 *      This script controls walking and jumping. It also manages the drag value in the rigidbody component so that
 *          while on the ground the character can step up over obsticles, but in the air the character flies with 0
 *          drag. The fall multiplier is applied whenever the character is not grounded and not pressing the jump button.
 *          This gives extra control over height and distance while the character is jumping.
 *      It takes some information from other components to ensure it works correctly. This script may be expanded 
 *          later to include things like rolling and wall running and edge hanging.
 * 
 * Dependencies: 
 *      ProceduralCharacter.Animation:  ProceduralMeasurements provides a bool to indicate the character is grounded.
 *      MovementInterpreter:            Provides the input values.
 *      Rigidbody:                      The physics component for the character.
 *               
 * Variables:   
 *      _defaultSpeed:          Default movement speed goal.
 *      _crouchSpeed:           Default crouch movement speed goal.
 *      _sprintSpeed:
 *      _disabledSpeed:
 *      
 *      _walkSpeedSmoothTime:   SmoothDamp time for smoothing the current speed.
 *      
 *      _slopeLimit:            (Set to negative value to disable)
 *              
 * Properties:
 *      Speed:                  Public access to the default walking speed.
 *      CrouchSpeed:            Public access to the default crouch speed.
 */

using UnityEngine;

namespace ProceduralCharacter.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(MovementInterpreter))]
    [SelectionBase]
    public class MovementController : MonoBehaviour
    {
        #region Variables(Private)
        private MovementInterpreter _input;
        private Rigidbody _RB;

        [Header("Grounding")]
        [SerializeField]
        bool _groundRayGizmo = false;
        Vector3 DownDir = Vector3.down;
        [SerializeField, Tooltip("Height above the ground for the transform to float")]
        float _rideHeight = 1f;
        [SerializeField, Range(-1, 1), Tooltip("Downward ray overshoot, allows for downward pull as well as upward push.")]
        float _rayOvershoot = 0.5f;
        [SerializeField, Tooltip("Strength of the spring which holds up the character, stronger makes faster movement.")]
        float _RideSpringStrength = 10f;
        [SerializeField]
        float _RideSpringDamper = 10f;
        [SerializeField, Tooltip("Layermask indicating the ground, used to check if the character is grounded.")]
        public LayerMask _ground;

        Vector3 _groundVel = Vector3.zero;
        Vector3 _groundAngVel = Vector3.zero;

        [Header("Speed")]
        [SerializeField, Min(0)]
        float _MaxSpeed = 8f;

        [SerializeField]
        float _Acceleration = 200f;
        [SerializeField, Tooltip("Multiplier relative to velocity, when changing directions gives a boost to keep time to full speed constant.")]
        AnimationCurve _AccelerationFactorFromDot;
        [SerializeField]
        float _MaxAccelForce = 150f;
        [SerializeField, Tooltip("Multiplier relative to velocity, when changing directions, gives a boost to keep time to full speed constant.")]
        AnimationCurve _MaxAccelerationForceFactorFromDot;
        [SerializeField, Tooltip("Prevents movement forces from applying additional upward or downward force. Leave as (1, 0, 1).")]
        Vector3 _ForceScale = Vector3.zero;

        [Space]
        [SerializeField, Min(0), Tooltip("Multiplier for sprinting speed, recommend 1, higher values will exceed MaxSpeed.")]
        float _sprintFactor = 1f;
        [SerializeField, Min(0), Tooltip("Multiplier for default speed, recommend < SprintFactor.")]
        float _defaultFactor = 0.5f;
        //[SerializeField]
        //float _crouchFactor = 0.4f;
        [SerializeField, Min(0), Tooltip("Multiplier for acceleration while in the air, keep low.")]
        float _inAirFactor = 0.2f;
        [SerializeField, Min(0), Tooltip("Multiplier for when movement is not allowed, recommend 0.")]
        float _disabledFactor = 0.1f;
        [Space]
        [SerializeField]
        float _speedFactorSmoothTime = 0.01f;
        float _speedFactor = 1f;
        float _speedRefVel = 0f;

        Vector3 _GoalVel = Vector3.zero;

        [Header("Slope Check")]
        [SerializeField, Range(-90f, 90f)]
        float _slopeLimit = 35f;
        [SerializeField, Tooltip("Multiplier which effects speed on slopes, faster on downslope slower on upslope.")]
        AnimationCurve _slopeSpeed;
        float _slopeYMax = 0f;

        [SerializeField]
        bool _slopeCircleGizmo = false;

        [Header("Jump")]
        [SerializeField, Min(0), Tooltip("Desired jump height.")]
        float _jumpHeight = 2f;
        [SerializeField, Min(0), Tooltip("Mario style fall multiplier, releasing jump key adds this multiplier to fall faster.")]
        float _fallMultiplier = 2.5f;

        float _jumpVelocity = 0f;

        bool _disableGrounding = false;     //Turns off the grounding/float forces to allow the character to jump
        bool _disableGroundingLock = false; //Makes it so that _disableGrounding must remain off until the Jump input is released

        bool _isMoving = false;
        bool _isGrounded = false;

        [Header("Acceleration Tilt")]
        [SerializeField, Range(0,6), Tooltip("Scaled force relative to torque strength.")]
        float _accelScale = 2f;

        [Header("Rotation (Turning & Uprightness)")]
        [SerializeField, Range(0, 10)]
        float _turnThreshold = 1f;

        [SerializeField]
        float _torqueStrength = 1000f;
        [SerializeField]
        float _torqueDamping = 100f;
        [SerializeField]
        Quaternion _uprightRotation = Quaternion.identity;


        RaycastHit _rayHit;

        Vector3 _UnitGoal = Vector3.zero;
        #endregion

        #region Properties
        [Space]
        public bool MovementEnable = true;
        public bool IsMoving => _isMoving;
        public bool IsGrounded => _isGrounded;
        public RaycastHit GroundHitInfo => _rayHit;
        public float MaxSpeed => _MaxSpeed;
        public float RideHeight => _rideHeight;
        [HideInInspector]
        public float RideHeightMultiplier = 1f;
        [HideInInspector]
        public float DefaultSpeedMultiplier = 1f;
        #endregion

        #region UnityMethods
        // Start is called before the first frame update
        void Start()
        {
            _input = GetComponent<MovementInterpreter>();
            _RB = GetComponent<Rigidbody>();

            _slopeYMax = Mathf.Sin(Mathf.Deg2Rad * _slopeLimit);
        }

        // Update is called once per frame
        void Update()
        {
            HandleSpeedFactor();
        }

        private void FixedUpdate()
        {
            Ray ray = new Ray(_RB.position, Vector3.down);
            if (Physics.Raycast(ray, out _rayHit, (_rideHeight * RideHeightMultiplier) + _rayOvershoot, _ground))
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }

            if(_isGrounded && _rayHit.rigidbody != null)
            {
                _groundVel = _rayHit.rigidbody.GetPointVelocity(_rayHit.point);
                _groundAngVel = _rayHit.rigidbody.angularVelocity;
            }
            else
            {
                _groundVel = Vector3.zero;
            }

            HandleJump();

            HandleGrounding();
            //Desired XZ plane speed
            Vector3 accel = HandleMovement();
            //Debug.DrawLine(transform.position, transform.position + accel);

            VelTurn();
            UpdateUprightForce();
            //AccelTilt(_RB.velocity * Time.fixedDeltaTime);
            AccelTilt(accel);
        }

        private void OnDrawGizmos()
        {
            //Draw IsGrounded Sphere
            if (_isGrounded)
            {
                if (_groundRayGizmo)
                {
                    Gizmos.color = Color.green;
                    Vector3 center = new Vector3(transform.position.x, transform.position.y  - (((_rideHeight * RideHeightMultiplier) + _rayOvershoot) / 2f), transform.position.z);
                    Vector3 size = new Vector3(0.025f, (_rideHeight * RideHeightMultiplier) + _rayOvershoot, 0.025f);
                    Gizmos.DrawCube(center, size);
                    Gizmos.DrawWireCube(_rayHit.point, Vector3.one * 0.05f);
                }

                if (_slopeCircleGizmo)
                {
                    int x = 36;
                    for (int i = 0; i < x; i++)
                    {
                        Vector3 start = new Vector3(Mathf.Cos(Mathf.Deg2Rad * (i * (360 / x))) * 1, 0, Mathf.Sin(Mathf.Deg2Rad * (i * (360 / x))) * 1);
                        Vector3 end = new Vector3(Mathf.Cos(Mathf.Deg2Rad * ((i + 1) * (360 / x))) * 1, 0, Mathf.Sin(Mathf.Deg2Rad * ((i + 1) * (360 / x))) * 1);

                        start = HandleSlope(start);
                        end = HandleSlope(end);

                        Gizmos.DrawLine(start + transform.position, end + transform.position);
                    }
                }
            }

            //Gizmos.DrawLine(transform.position, transform.position + _desiredDirection);
        }
        #endregion

        #region Methods
        private void HandleGrounding()
        {
            if (_isGrounded && !_disableGrounding)
            {
                Vector3 vel = _RB.velocity;
                Vector3 rayDir = transform.TransformDirection(DownDir);

                Vector3 otherVel = Vector3.zero;
                Rigidbody hitBody = _rayHit.rigidbody;
                if (hitBody != null)
                {
                    otherVel = hitBody.velocity;
                }

                float rayDirVel = Vector3.Dot(rayDir, vel);
                float otherDirVel = Vector3.Dot(rayDir, otherVel);

                float relVel = rayDirVel - otherDirVel;

                float x = _rayHit.distance - (_rideHeight * RideHeightMultiplier);

                float springForce = (x * _RideSpringStrength) - (relVel * _RideSpringDamper);

                //Debug.DrawLine(transform.position, transform.position + (rayDir * springForce), Color.yellow);

                _RB.AddForce(rayDir * springForce);

                if (hitBody != null)
                {
                    hitBody.AddForceAtPosition(rayDir * -springForce, _rayHit.point);
                }
            }
            else
            {
                if (_RB.velocity.y < 0)
                {
                    //Character is falling (probably)
                    _RB.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.fixedDeltaTime;
                }
                else if (_RB.velocity.y > 0 && !_input.Jump)
                {
                    //use higher multiplier to halt upward momentum
                    _RB.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.fixedDeltaTime;
                }
            }
        }

        private void HandleSpeedFactor()
        {
            float speed = _defaultFactor;
            if(MovementEnable)
            {
                if (_isGrounded)
                {
                    //if (_input.Crouch)
                    //{
                    //    speed = _crouchFactor;
                    //}
                    //else 
                    if (_input.Sprint)
                    {
                        speed = _sprintFactor;
                        if(DefaultSpeedMultiplier != 1f)
                        {
                            speed = _defaultFactor * DefaultSpeedMultiplier;
                        }
                    }
                    else
                    {
                        speed = _defaultFactor * DefaultSpeedMultiplier;
                    }
                }
                else
                {
                    speed = _inAirFactor;
                }
            }
            else
            {
                speed = _disabledFactor;
            }

            _speedFactor = Mathf.SmoothDamp(_speedFactor, speed, ref _speedRefVel, _speedFactorSmoothTime);
        }

        private Vector3 HandleMovement()
        {
            Vector3 neededAccel;
            //input ...
            _UnitGoal = HandleSlope(_input.MoveDirection);
            if (_UnitGoal.magnitude > 1f)
            {
                _UnitGoal.Normalize();
            }

            if(_UnitGoal.magnitude > 0f)
            {
                _isMoving = true;
            }
            else
            {
                _isMoving = false;
            }

            if (_isGrounded)
            {
                //calculate new goal vel...
                Vector3 unitVel = _GoalVel.normalized;

                float velDot = Vector3.Dot(_UnitGoal, unitVel);

                float accel = _Acceleration * _AccelerationFactorFromDot.Evaluate(velDot);

                Vector3 goalVel = _UnitGoal * _MaxSpeed * _speedFactor;

                _GoalVel = Vector3.MoveTowards(_GoalVel, (goalVel) + (_groundVel),
                    accel * Time.fixedDeltaTime);

                //Actual force...
                neededAccel = (_GoalVel - _RB.velocity) / Time.fixedDeltaTime;

                float maxAccel = _MaxAccelForce * _MaxAccelerationForceFactorFromDot.Evaluate(velDot);// * _maxAccelForceFactor;

                neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

                _RB.AddForce(Vector3.Scale(neededAccel * _RB.mass, _ForceScale));

                
            }
            else
            {
                neededAccel = _UnitGoal * _MaxSpeed * _speedFactor;
                neededAccel = Vector3.ClampMagnitude(neededAccel, _MaxAccelForce);

                _RB.AddForce(Vector3.Scale(neededAccel * _RB.mass, _ForceScale));
            }

            return neededAccel;
        }

        private Vector3 HandleSlope(Vector3 input)
        {
            Vector3 output = input;
            if (_isGrounded && _slopeLimit > 0f)
            {
                output = Vector3.ProjectOnPlane(input, _rayHit.normal);
                float ya = _slopeSpeed.Evaluate(Mathf.Sin(Mathf.Deg2Rad * Vector3.Angle(Vector3.up, _rayHit.normal)) / _slopeYMax);
                float yA = output.normalized.y;

                float X = (yA == 0 || ya == 0) ? 0 : ya / yA;

                float frac = Mathf.Clamp(yA / _slopeYMax, -1f, 1f);

                float mapped = _slopeSpeed.Evaluate(frac);

                float mult = yA > ya ? X : mapped;

                output *= mult;
            }
            return output;
        }

        void HandleJump()
        {
            _jumpVelocity = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y);

            if (_input.Jump)
            {
                if (_isGrounded)
                {
                    if (!_disableGroundingLock)
                    {
                        _disableGrounding = true;
                        Vector3 jumpForce = (new Vector3(0f, _jumpVelocity, 0f) / Time.fixedDeltaTime) * _RB.mass;
                        _disableGrounding = true;
                        _disableGroundingLock = true;
                        //MovementEnable = !_isJumping;

                        //_RB.velocity += (jumpForce);
                        _RB.AddForce(jumpForce);

                        Rigidbody otherRB = _rayHit.rigidbody;
                        if (otherRB != null)
                        {
                            otherRB.AddForceAtPosition(-jumpForce, _rayHit.point);
                        }
                    }
                }
                else
                {
                    _disableGrounding = false;
                }
            }
            else
            {
                _disableGrounding = false;
                _disableGroundingLock = false;
            }
        }

        void AccelTilt(Vector3 accel)
        {
            accel.y = 0;
            Vector3 tiltAxis = Vector3.Cross(Vector3.up, accel.normalized).normalized;

            float ClampedAccel = Mathf.Clamp(accel.magnitude * _accelScale * _RB.mass, 0f, _MaxAccelForce);

            _RB.AddTorque(tiltAxis * ClampedAccel);
        }

        void VelTurn()
        {
            Vector3 dir = GetRelativeVelocity();

            if (dir.magnitude > _turnThreshold)
            {
                _uprightRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }

            //Rotate based on rotation of the object under the player's feet
            if(_isGrounded && _rayHit.rigidbody != null)
            {
                Vector3 angleVelY = new Vector3(0f, _groundAngVel.y * 1.15f, 0f);
                _uprightRotation = Quaternion.Euler(angleVelY) * _uprightRotation;
            }
        }

        public Vector3 GetRelativeVelocity()
        {
            Vector3 vel = new Vector3(_RB.velocity.x, 0f, _RB.velocity.z);
            Vector3 otherVel = Vector3.zero;
            if (GroundHitInfo.rigidbody != null)
            {
                otherVel = GroundHitInfo.rigidbody.GetPointVelocity(GroundHitInfo.point);
                otherVel.y = 0f;
            }
            Vector3 dir = vel - otherVel;

            if (dir.magnitude < _turnThreshold)
            {
                dir = Vector3.zero;
            }

            return dir;
        }

        void UpdateUprightForce()
        {
            Quaternion current = transform.rotation;
            Quaternion toGoal = ShortestRotation(_uprightRotation, current);

            Vector3 rotAxis;
            float rotDegrees;

            toGoal.ToAngleAxis(out rotDegrees, out rotAxis);
            rotAxis.Normalize();

            float rotRadians = rotDegrees * Mathf.Deg2Rad;

            rotAxis.y *= 2f;    //TODO: Turn this into a variable, I should be able to customize this

            _RB.AddTorque((rotAxis * rotRadians * _torqueStrength) - (_RB.angularVelocity * _torqueDamping));
        }
        Quaternion ShortestRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                return a * Quaternion.Inverse(Multiply(b, -1));
            }
            else
            {
                return a * Quaternion.Inverse(b);
            }
        }
        Quaternion Multiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }
        #endregion
    }
}
