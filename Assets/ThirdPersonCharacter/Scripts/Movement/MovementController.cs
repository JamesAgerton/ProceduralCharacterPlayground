/* MovementControllerP: James Agerton 2021
 *  Handles jumping and horizontal movement.
 */

using UnityEngine;

namespace ProceduralCharacter.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(MovementInterpreter), typeof(MovementFloatRide))]
    [SelectionBase]
    public class MovementController : MonoBehaviour
    {
        #region Variables(Private)
        private MovementInterpreter _input;
        private Rigidbody _RB;
        private MovementFloatRide _MFR;

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
        Vector3 _acceleration = Vector3.zero;

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

        bool _isMoving = false;

        Vector3 _UnitGoal = Vector3.zero;
        #endregion

        #region Properties
        [Space]
        public bool MovementEnable = true;
        public bool IsMoving => _isMoving;
        public float MaxSpeed => _MaxSpeed;
        [HideInInspector]
        public float RideHeightMultiplier = 1f;
        [HideInInspector]
        public float DefaultSpeedMultiplier = 1f;
        public Vector3 Acceleration => _acceleration;
        #endregion

        #region UnityMethods
        // Start is called before the first frame update
        void Start()
        {
            _input = GetComponent<MovementInterpreter>();
            _RB = GetComponent<Rigidbody>();
            _MFR = GetComponent<MovementFloatRide>();

            _slopeYMax = Mathf.Sin(Mathf.Deg2Rad * _slopeLimit);
        }

        // Update is called once per frame
        void Update()
        {
            HandleSpeedFactor();
        }

        private void FixedUpdate()
        {
            HandleJump();

            _acceleration = HandleMovement();
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                //Draw IsGrounded Sphere
                if (_MFR.IsGrounded && _slopeCircleGizmo)
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
        //private void HandleGrounding()
        //{
        //    if (_isGrounded && !_disableGrounding)
        //    {
        //        Vector3 vel = _RB.velocity;
        //        Vector3 rayDir = transform.TransformDirection(DownDir);

        //        Vector3 otherVel = Vector3.zero;
        //        Rigidbody hitBody = _rayHit.rigidbody;
        //        if (hitBody != null)
        //        {
        //            otherVel = hitBody.velocity;
        //        }

        //        float rayDirVel = Vector3.Dot(rayDir, vel);
        //        float otherDirVel = Vector3.Dot(rayDir, otherVel);

        //        float relVel = rayDirVel - otherDirVel;

        //        float x = _rayHit.distance - (_rideHeight * RideHeightMultiplier);

        //        float springForce = (x * _RideSpringStrength) - (relVel * _RideSpringDamper);

        //        //Debug.DrawLine(transform.position, transform.position + (rayDir * springForce), Color.yellow);

        //        _RB.AddForce(rayDir * springForce);

        //        if (hitBody != null)
        //        {
        //            hitBody.AddForceAtPosition(rayDir * -springForce, _rayHit.point);
        //        }
        //    }
        //    else
        //    {
        //        if (_RB.velocity.y < 0)
        //        {
        //            //Character is falling (probably)
        //            _RB.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.fixedDeltaTime;
        //        }
        //        else if (_RB.velocity.y > 0 && !_input.Jump)
        //        {
        //            //use higher multiplier to halt upward momentum
        //            _RB.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.fixedDeltaTime;
        //        }
        //    }
        //}

        private void HandleSpeedFactor()
        {
            float speed = _defaultFactor;
            if(MovementEnable)
            {
                if (_MFR.IsGrounded)
                {
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

            if (_MFR.IsGrounded)
            {
                //calculate new goal vel...
                Vector3 unitVel = _GoalVel.normalized;

                float velDot = Vector3.Dot(_UnitGoal, unitVel);

                float accel = _Acceleration * _AccelerationFactorFromDot.Evaluate(velDot);

                Vector3 goalVel = _UnitGoal * _MaxSpeed * _speedFactor;

                Vector3 groundVel = Vector3.zero;
                if (_MFR.RayHitInfo.rigidbody != null)
                {
                    groundVel = _MFR.RayHitInfo.rigidbody.GetPointVelocity(_MFR.RayHitInfo.point);
                }

                _GoalVel = Vector3.MoveTowards(_GoalVel, (goalVel) + groundVel,
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
            if (_MFR.IsGrounded && _slopeLimit > 0f)
            {
                output = Vector3.ProjectOnPlane(input, _MFR.RayHitInfo.normal);
                float ya = _slopeSpeed.Evaluate(Mathf.Sin(Mathf.Deg2Rad * Vector3.Angle(Vector3.up, _MFR.RayHitInfo.normal)) / _slopeYMax);
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
                if (_MFR.IsGrounded)
                {
                    if (!_MFR.FloatEnableLock)     //!_disableGroundingLock
                    {
                        _MFR.FloatEnable = false;   //_disableGrounding = true;
                        Vector3 jumpForce = (new Vector3(0f, _jumpVelocity, 0f) / Time.fixedDeltaTime) * _RB.mass;
                        //_MFR.FloatEnable = false;   //_disableGrounding = true;
                        _MFR.FloatEnableLock = true;    //_disableGroundingLock = true;

                        //_RB.velocity += (jumpForce);
                        _RB.AddForce(jumpForce);

                        Rigidbody otherRB = _MFR.RayHitInfo.rigidbody;
                        if (otherRB != null)
                        {
                            otherRB.AddForceAtPosition(-jumpForce, _MFR.RayHitInfo.point);
                        }
                    }
                }
                else
                {
                    _MFR.FloatEnable = true;      //_disableGrounding = false;
                }
            }
            else
            {
                _MFR.FloatEnable = true;            //_disableGrounding = false;
                _MFR.FloatEnableLock = false;            //_disableGroundingLock = false;
            }
        }

        //void AccelTilt(Vector3 accel)
        //{
        //    accel.y = 0;
        //    Vector3 tiltAxis = Vector3.Cross(Vector3.up, accel.normalized).normalized;

        //    float ClampedAccel = Mathf.Clamp(accel.magnitude * _accelScale * _RB.mass, 0f, _MaxAccelForce);

        //    _RB.AddTorque(tiltAxis * ClampedAccel);
        //}

        //void VelTurn()
        //{
        //    Vector3 dir = GetRelativeVelocity();

        //    if (dir.magnitude > _turnThreshold)
        //    {
        //        _uprightRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        //    }

        //    //Rotate based on rotation of the object under the player's feet
        //    if(_isGrounded && _rayHit.rigidbody != null)
        //    {
        //        Vector3 angleVelY = new Vector3(0f, _groundAngVel.y * 1.15f, 0f);
        //        _uprightRotation = Quaternion.Euler(angleVelY) * _uprightRotation;
        //    }
        //}

        //public Vector3 GetRelativeVelocity()
        //{
        //    Vector3 vel = new Vector3(_RB.velocity.x, 0f, _RB.velocity.z);
        //    Vector3 otherVel = Vector3.zero;
        //    if (GroundHitInfo.rigidbody != null)
        //    {
        //        otherVel = GroundHitInfo.rigidbody.GetPointVelocity(GroundHitInfo.point);
        //        otherVel.y = 0f;
        //    }
        //    Vector3 dir = vel - otherVel;

        //    if (dir.magnitude < _turnThreshold)
        //    {
        //        dir = Vector3.zero;
        //    }

        //    return dir;
        //}

        //void UpdateUprightForce()
        //{
        //    Quaternion current = transform.rotation;
        //    Quaternion toGoal = ShortestRotation(_uprightRotation, current);

        //    Vector3 rotAxis;
        //    float rotDegrees;

        //    toGoal.ToAngleAxis(out rotDegrees, out rotAxis);
        //    rotAxis.Normalize();

        //    float rotRadians = rotDegrees * Mathf.Deg2Rad;

        //    rotAxis.y *= 2f;    //TODO: Turn this into a variable, I should be able to customize this

        //    _RB.AddTorque((rotAxis * rotRadians * _torqueStrength) - (_RB.angularVelocity * _torqueDamping));
        //}
        //Quaternion ShortestRotation(Quaternion a, Quaternion b)
        //{
        //    if (Quaternion.Dot(a, b) < 0)
        //    {
        //        return a * Quaternion.Inverse(Multiply(b, -1));
        //    }
        //    else
        //    {
        //        return a * Quaternion.Inverse(b);
        //    }
        //}
        //Quaternion Multiply(Quaternion input, float scalar)
        //{
        //    return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        //}
        #endregion
    }
}
