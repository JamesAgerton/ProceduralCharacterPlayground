/* MovementControllerP: James Agerton 2021
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
 *      _walkSpeedSmoothTime:   SmoothDamp time for smoothing the current speed.
 *      
 *      _stepHeight:            (Set to negative value to disable)
 *      _stepSmooth:            
 *      _slopeLimit:            (Set to negative value to disable)
 *      
 *      _jumpHeight:            Used to determine the initial velocity of the jump.
 *      _fallMultiplier:        How fast the character falls, a multiplier gives extra control to jump height.
 *      _rbDrag:                The drag value applied to the rigidbody while the character is grounded, this gives
 *                                  the character extra grip and momentum while moving and helps with step up over 
 *                                  obsticles.
 *              
 * Properties:
 *      Speed:                  Public access to the default walking speed.
 *      CrouchSpeed:            Public access to the default crouch speed.
 */

using UnityEngine;
using ProceduralCharacter.Animation;

namespace ProceduralCharacter.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(MovementInterpreter), typeof(ProceduralMeasurements))]
    public class MovementControllerP : MonoBehaviour
    {
        #region Variables(Private)
        private MovementInterpreter _input;
        private Rigidbody _body;
        private ProceduralMeasurements _measurements;

        [Header("Speed")]
        [SerializeField, Tooltip("The default movement speed.")]
        float _defaultSpeed = 5f;
        [SerializeField, Tooltip("The default movement speed while crouching.")]
        float _crouchSpeed = 2.5f;
        [SerializeField, Tooltip("The time over which a change in speed is smoothed.")]
        float _walkSpeedSmoothTime = 0.1f;

        [Header("Step Up")]
        [SerializeField]
        float _slopeLimit = 35f;

        [Header("Jumping")]
        [SerializeField, Tooltip("The desired maximum height of a jump.")]
        float _jumpHeight = 2f;
        [SerializeField, Tooltip("A gravity multiplier applied when the jump button is not held.")]
        float _fallMultiplier = 2.5f;
        [SerializeField, Tooltip("The drag value on the attached rigidbody.")]
        private float _rbDrag = 8f;

        float _walkSpeed = 0f;
        float _wSVelocity = 0f;
        float _desiredSpeed = 0f;
        float _currentSpeed = 0f;
        float _deltaSpeed = 0f;

        bool _isJumping = false;
        float jumpVelocity = 0f;
        #endregion

        #region Properties
        public float Speed => _defaultSpeed;
        public float CrouchSpeed => _crouchSpeed;
        #endregion

        #region UnityMethods
        // Start is called before the first frame update
        void Start()
        {
            _input = GetComponent<MovementInterpreter>();
            _body = GetComponent<Rigidbody>();
            _measurements = GetComponent<ProceduralMeasurements>();
        }

        // Update is called once per frame
        void Update()
        {
            HandleGrounding();

            HandleJump();
        }

        private void FixedUpdate()
        {
            //Desired XZ plane speed
            HandleMovement();

            //Calculate jump
            HandleAirtime();
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                //float lowDist = _walkSphere.radius;
                //float highDist = _walkSphere.radius;

                //Ray rayForwardLow = new Ray(transform.position + (Vector3.up * 0.05f), _forward);
                //Ray rayForwardHigh = new Ray(transform.position + (Vector3.up * _stepHeight), _forward);
                //Ray rayRightLow = new Ray(transform.position + (Vector3.up * 0.05f), _fortyfiveright);
                //Ray rayRightHigh = new Ray(transform.position + (Vector3.up * _stepHeight), _fortyfiveright);
                //Ray rayLeftLow = new Ray(transform.position + (Vector3.up * 0.05f), _fortyfiveleft);
                //Ray rayLeftHigh = new Ray(transform.position + (Vector3.up * _stepHeight), _fortyfiveleft);

                //Gizmos.color = Color.white;
                //if((Physics.Raycast(rayForwardLow, _walkSphere.radius, _measurements.Ground) &&
                //    !Physics.Raycast(rayForwardHigh, _walkSphere.radius, _measurements.Ground)) ||
                //    (Physics.Raycast(rayRightLow, _walkSphere.radius, _measurements.Ground) &&
                //    !Physics.Raycast(rayRightHigh, _walkSphere.radius, _measurements.Ground)) ||
                //    (Physics.Raycast(rayLeftLow, _walkSphere.radius, _measurements.Ground) &&
                //    !Physics.Raycast(rayLeftHigh, _walkSphere.radius, _measurements.Ground)) )
                //{
                //    Gizmos.color = Color.green;
                //}

                //Gizmos.DrawLine(transform.position + (Vector3.up * _stepHeight), 
                //    transform.position + Vector3.up * _stepHeight + (_forward * lowDist));
                //Gizmos.DrawLine(transform.position + (Vector3.up * 0.05f), 
                //    transform.position + (Vector3.up * 0.05f) + (_forward * highDist));
                //Gizmos.DrawLine(transform.position + Vector3.up * _stepHeight + (_forward * lowDist),
                //    transform.position + (Vector3.up * 0.05f) + (_forward * highDist));

                //Gizmos.DrawLine(transform.position + (Vector3.up * _stepHeight),
                //    transform.position + Vector3.up * _stepHeight + (_fortyfiveright * lowDist));
                //Gizmos.DrawLine(transform.position + (Vector3.up * 0.05f),
                //    transform.position + (Vector3.up * 0.05f) + (_fortyfiveright * highDist));
                //Gizmos.DrawLine(transform.position + Vector3.up * _stepHeight + (_fortyfiveright * lowDist),
                //    transform.position + (Vector3.up * 0.05f) + (_fortyfiveright * highDist));

                //Gizmos.DrawLine(transform.position + (Vector3.up * _stepHeight),
                //    transform.position + Vector3.up * _stepHeight + (_fortyfiveleft * lowDist));
                //Gizmos.DrawLine(transform.position + (Vector3.up * 0.05f),
                //    transform.position + (Vector3.up * 0.05f) + (_fortyfiveleft * highDist));
                //Gizmos.DrawLine(transform.position + Vector3.up * _stepHeight + (_fortyfiveleft * lowDist),
                //    transform.position + (Vector3.up * 0.05f) + (_fortyfiveleft * highDist));
            }
        }
        #endregion

        #region Methods
        private void HandleJump()
        {
            jumpVelocity = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y);

            if (_input.Jump && _measurements.IsGrounded && !_isJumping)
            {
                _body.velocity += new Vector3(0f, jumpVelocity, 0f);
                _body.drag = 0f;
                _isJumping = true;
            }

            if (_isJumping && _measurements.IsGrounded && _body.velocity.y <= 0 && !_input.Jump)
            {
                _isJumping = false;
                _body.drag = _rbDrag;
            }
        }

        private void HandleGrounding()
        {
            if (!_measurements.IsGrounded)
            {
                _walkSpeed *= 0.5f;
                _body.drag = 0f;
            }
            else if (_measurements.IsGrounded && _input.Crouch && !_isJumping)
            {
                _body.drag = _rbDrag;
                _walkSpeed = Mathf.SmoothDamp(_walkSpeed, _crouchSpeed, ref _wSVelocity, _walkSpeedSmoothTime);
            }
            else 
            {
                _walkSpeed = Mathf.SmoothDamp(_walkSpeed, _defaultSpeed, ref _wSVelocity, _walkSpeedSmoothTime);
                _body.drag = _rbDrag;
            }
        }

        private void HandleAirtime()
        {
            if (_body.velocity.y < 0)
            {
                //Character is falling
                _body.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.deltaTime;
            }
            else if (_body.velocity.y > 0 && !_input.Jump)
            {
                //use higher multiplier to halt upward momentum
                _body.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.deltaTime;
            }
        }

        private void HandleMovement()
        {
            _desiredSpeed = _input.MoveDirection.magnitude * _walkSpeed;
            _currentSpeed = _measurements.VelocityFlat.magnitude;
            _deltaSpeed = _desiredSpeed - _currentSpeed;
            _deltaSpeed = Mathf.Clamp(_deltaSpeed, 0f, 1000f);

            Vector3 desiredDirection = _input.MoveDirection;
            desiredDirection = HandleSlope(desiredDirection);
            if (Mathf.Abs(_deltaSpeed) > 0.5f)
            {
                _body.AddForce(desiredDirection * _deltaSpeed, ForceMode.VelocityChange);
                //_body.velocity += desiredDirection * _deltaSpeed;
            }
        }

        private Vector3 HandleSlope(Vector3 input)
        {
            Vector3 output = input;
            if (_measurements.IsGrounded && _slopeLimit > 0f)
            {
                float mult = Mathf.Clamp01((_slopeLimit - Vector3.Angle(_measurements.GroundHit.normal, Vector3.up)) / _slopeLimit);
                //Debug.Log(mult);
                output = Vector3.ProjectOnPlane(input, _measurements.GroundHit.normal).normalized;
                if(output.y > 0)
                {
                    //output = new Vector3(output.x, output.y * mult, output.z);
                    output *= mult;
                }
            }
            return output;
        }

        #endregion
    }
}
