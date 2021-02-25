/* MovementControllerP: James Agerton 2021
 * 
 * This script is ONLY for physics based movement including jump.
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

        [SerializeField]
        float _defaultSpeed = 5f;
        [SerializeField]
        float _crouchSpeed = 2.5f;
        [SerializeField]
        float _walkSpeedSmoothTime = 0.1f;
        [SerializeField]
        float jumpHeight = 2f;
        [SerializeField]
        float fallMultiplier = 2.5f;
        [SerializeField]
        float lowFallMultiplier = 2f;
        [SerializeField]
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
            jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);

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
            if (_body.velocity.y < 0)
            {
                _body.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (_body.velocity.y > 0 && !_input.Jump)
            {
                _body.velocity += Vector3.up * Physics.gravity.y * (lowFallMultiplier - 1) * Time.fixedDeltaTime;
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
