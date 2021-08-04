using UnityEngine;

namespace ProceduralCharacter.Movement
{
    [RequireComponent(typeof(MovementInterpreter), typeof(MovementFloatRide), typeof(MovementController))]
    [RequireComponent(typeof(Rigidbody))]
    public class MovementJump : MonoBehaviour
    {
        #region Variables (PRIVATE)
        MovementInterpreter _input;
        MovementFloatRide _MFR;
        MovementController _MC;
        Rigidbody _RB;

        //Vector3 _GoalVel = Vector3.zero;
        Vector3 _UnitGoal = Vector3.zero;

        [Header("Jump")]
        [SerializeField]
        bool _OverrideInAirMovement;
        [SerializeField, Min(0)]
        float _inAirAccelerationFactor = 0.5f;
        [SerializeField, Min(0), Tooltip("Multiplier for acceleration while in the air, keep low.")]
        float _inAirSpeedFactor = 1f;
        
        [Space]
        [SerializeField, Min(0), Tooltip("Desired jump height.")]
        float _jumpHeight = 2f;
        [SerializeField, Min(0), Tooltip("Mario style fall multiplier, releasing jump key adds this multiplier to fall faster.")]
        float _fallMultiplier = 2.5f;

        float _jumpVelocity = 0f;
        #endregion

        #region Properties (PUBLIC)

        #endregion

        #region Unity Methods

        private void Start()
        {
            _input = GetComponent<MovementInterpreter>();
            _MFR = GetComponent<MovementFloatRide>();
            _MC = GetComponent<MovementController>();
            _RB = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!_MFR.IsGrounded && !_OverrideInAirMovement)
            {
                _MC.AccelerationFactorFromOutside = _inAirAccelerationFactor;
                _MC.DefaultSpeedMultiplier = _inAirSpeedFactor;
            }
        }

        private void FixedUpdate()
        {
            HandleJump();

            if (_OverrideInAirMovement && !_MFR.IsGrounded)
            {
                _MC.DisableMovementInAir = true;

                _MC.Acceleration = HandleMovementJump();
            }
        }

        #endregion

        #region Methods
        void HandleJump()
        {
            _jumpVelocity = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y);

            if (_input.Jump)
            {
                if (_MFR.IsGrounded)
                {
                    if (!_MFR.FloatEnableLock)
                    {
                        _MFR.FloatEnable = false;
                        Vector3 jumpForce = (new Vector3(0f, _jumpVelocity, 0f) / Time.fixedDeltaTime) * _RB.mass;
                        _MFR.FloatEnableLock = true;

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
                    _MFR.FloatEnable = true;
                }
            }
            else
            {
                _MFR.FloatEnable = true;
                _MFR.FloatEnableLock = false;
            }
        }

        Vector3 HandleMovementJump()
        {
            _UnitGoal = _input.MoveDirection;
            if(_UnitGoal.magnitude > 1f)
            {
                _UnitGoal.Normalize();
            }

            Vector3 unitVel = _MC.GoalVel.normalized;
            float velDot = Vector3.Dot(_UnitGoal, unitVel);
            float accel = _MC.BaseAcceleration * _MC.AccelerationFromDot.Evaluate(velDot) * _inAirAccelerationFactor;

            Vector3 goalVel = _UnitGoal * _MC.MaxSpeed * _MC.SpeedFactor * _inAirSpeedFactor;
            _MC.GoalVel = Vector3.MoveTowards(_MC.GoalVel, (goalVel), accel * Time.fixedDeltaTime);

            Vector3 neededAccel = (_MC.GoalVel - _RB.velocity) / Time.fixedDeltaTime;

            _RB.AddForce(Vector3.Scale(neededAccel * _RB.mass, _MC.ForceScale));

            return neededAccel;
        }
        #endregion
    }
}
