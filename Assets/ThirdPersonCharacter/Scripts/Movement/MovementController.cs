using UnityEngine;

namespace ProceduralCharacter.Movement
{
    /// <summary>
    /// Keeps the rigidbody at the given rotation. Rotation can be changed to control direction.
    /// </summary>
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
        float _MaxSpeed = 12f;

        [SerializeField, Tooltip("Acceleration of the character.")]
        float _baseAcceleration = 200f;
        [SerializeField, Tooltip("Multiplier relative to velocity, when changing directions gives a boost to keep time to full speed constant.")]
        AnimationCurve _AccelerationFactorFromDot;
        [SerializeField]
        float _MaxAccelForce = 150f;
        [SerializeField, Tooltip("Multiplier relative to velocity, when changing directions, gives a boost to keep time to full speed constant.")]
        AnimationCurve _MaxAccelerationForceFactorFromDot;
        [SerializeField, Tooltip("Prevents movement forces from applying additional upward or downward force. Leave as (1, 0, 1).")]
        Vector3 _ForceScale = new Vector3(1f, 0f, 1f);

        [Space]
        [SerializeField, Min(0), Tooltip("Multiplier for sprinting speed, recommend 1, higher values will exceed MaxSpeed.")]
        float _sprintFactor = 1f;
        [SerializeField, Min(0), Tooltip("Multiplier for default speed, recommend < SprintFactor.")]
        float _defaultFactor = 0.5f;
        [SerializeField, Min(0), Tooltip("Multiplier for when movement is not allowed, recommend 0.")]
        float _disabledFactor = 0.1f;
        [Space]
        [SerializeField]
        float _speedFactorSmoothTime = 0.01f;
        float _speedFactor = 1f;
        float _speedRefVel = 0f;

        Vector3 _UnitGoal = Vector3.zero;

        [Header("Slope Check")]
        [SerializeField, Range(0f, 90f), Tooltip("Max angle the character can walk up")]
        float _slopeLimit = 45f;
        [SerializeField, Tooltip("Multiplier which effects speed on slopes, faster on downslope slower on upslope.")]
        AnimationCurve _slopeSpeed;
        float _slopeYMax = 0f;

        [SerializeField]
        bool _slopeCircleGizmo = false;

        bool _isMoving = false;

        #endregion

        #region Properties
        [Space]
        public bool MovementEnable = true;
        public bool DisableMovementInAir = false;
        public bool IsMoving => _isMoving;
        public float MaxSpeed => _MaxSpeed;
        public float SpeedFactor => _speedFactor;
        [HideInInspector]
        public float DefaultSpeedMultiplier = 1f;
        public Vector3 Acceleration = Vector3.zero;
        [HideInInspector, Min(0)]
        public float AccelerationFactorFromOutside = 1f;
        public float BaseAcceleration => _baseAcceleration;
        public AnimationCurve AccelerationFromDot => _AccelerationFactorFromDot;
        public Vector3 ForceScale => _ForceScale;
        public Vector3 GoalVel = Vector3.zero;
        //public Vector3 UnitGoal => _UnitGoal;
        #endregion

        #region UnityMethods
        void Start()
        {
            _input = GetComponent<MovementInterpreter>();
            _RB = GetComponent<Rigidbody>();
            _MFR = GetComponent<MovementFloatRide>();

            _slopeYMax = Mathf.Sin(Mathf.Deg2Rad * _slopeLimit);
        }

        void Update()
        {
            HandleSpeedFactor();
        }

        private void FixedUpdate()
        {
            //HandleJump();

            Acceleration = HandleMovement();
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                //Draw IsGrounded debug
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
        }
        #endregion

        #region Methods
        private void HandleSpeedFactor()
        {
            float speed = _defaultFactor;
            if(MovementEnable)
            {
                if (_MFR.IsGrounded)
                {
                    if (_input.Sprint && DefaultSpeedMultiplier == 1f)
                    {
                        speed = _sprintFactor;
                    }
                    else
                    {
                        speed = _defaultFactor * DefaultSpeedMultiplier;
                    }
                }
            }
            else
            {
                speed = _disabledFactor;
            }

            _speedFactor = Mathf.SmoothDamp(_speedFactor, speed, ref _speedRefVel, _speedFactorSmoothTime);

            DefaultSpeedMultiplier = 1f;
        }

        private Vector3 HandleMovement()
        {
            Vector3 neededAccel = Vector3.zero;
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

            bool move = true;
            if (DisableMovementInAir && !_MFR.IsGrounded)
            {
                move = false;
            }

            if (move)
            {
                //calculate new goal vel...
                Vector3 unitVel = GoalVel.normalized;

                float velDot = Vector3.Dot(_UnitGoal, unitVel);

                float accel = _baseAcceleration * _AccelerationFactorFromDot.Evaluate(velDot) * AccelerationFactorFromOutside;

                //Reset outside factor for acceleration
                AccelerationFactorFromOutside = 1f;

                Vector3 goalVel = _UnitGoal * _MaxSpeed * _speedFactor;

                Vector3 groundVel = Vector3.zero;
                if (_MFR.RayHitInfo.rigidbody != null)
                {
                    groundVel = _MFR.RayHitInfo.rigidbody.GetPointVelocity(_MFR.RayHitInfo.point);
                }

                GoalVel = Vector3.MoveTowards(GoalVel, (goalVel) + groundVel,
                    accel * Time.fixedDeltaTime);

                //Actual force...
                neededAccel = (GoalVel - _RB.velocity) / Time.fixedDeltaTime;

                float maxAccel = _MaxAccelForce * _MaxAccelerationForceFactorFromDot.Evaluate(velDot);// * _maxAccelForceFactor;

                neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

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

        
        #endregion
    }
}
