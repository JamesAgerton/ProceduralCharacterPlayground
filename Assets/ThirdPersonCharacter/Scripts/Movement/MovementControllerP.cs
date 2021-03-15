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

        [SerializeField, Tooltip("The default movement speed.")]
        float _defaultSpeed = 5f;
        [SerializeField, Tooltip("The default movement speed while crouching.")]
        float _crouchSpeed = 2.5f;
        [SerializeField, Tooltip("The time over which a change in speed is smoothed.")]
        float _walkSpeedSmoothTime = 0.1f;
        [SerializeField, Tooltip("The desired maximum height of a jump.")]
        float _jumpHeight = 2f;
        [SerializeField, Tooltip("A gravity multiplier applied when the jump button is not held.")]
        float _fallMultiplier = 2.5f;
        [SerializeField, Tooltip("The drag value on the attached rigidbody.")]
        private float _rbDrag = 8f;

        float _walkSpeed = 0f;
        float _wSVelocity = 0f;

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
            jumpVelocity = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y);

            if (_input.Jump && _measurements.IsGrounded && !_isJumping)
            {
                _body.velocity += new Vector3(0f, jumpVelocity, 0f);
                _isJumping = true;
            }

            if (_isJumping && _measurements.IsGrounded && _body.velocity.y <= 0 && !_input.Jump)
            {
                _isJumping = false;
            }

            if (!_measurements.IsGrounded)
            {
                _walkSpeed *= 0.5f;
                _body.drag = 0f;
            }
            else if (_measurements.IsGrounded && _input.Crouch)
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

        private void FixedUpdate()
        {
            //Calculate jump
            if(_body.velocity.y < 0)
            {
                //Character is falling
                _body.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.deltaTime;
            }
            else if(_body.velocity.y > 0 && !_input.Jump)
            {
                //use higher multiplier to halt upward momentum
                _body.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.deltaTime;
            }

            //Desired XZ plane speed
            float desiredSpeed = _input.MoveDirection.magnitude * _walkSpeed;
            float currentSpeed = new Vector3(_body.velocity.x, 0f, _body.velocity.z).magnitude;
            float deltaSpeed = desiredSpeed - currentSpeed;
            deltaSpeed = Mathf.Clamp(deltaSpeed, 0f, 1000f);
            if (Mathf.Abs(deltaSpeed) > 0.5f)
            {
                _body.AddForce(_input.MoveDirection * deltaSpeed, ForceMode.VelocityChange);
            }
        }

        private void OnDrawGizmos()
        {

        }
        #endregion

        #region Methods

        #endregion
    }
}
