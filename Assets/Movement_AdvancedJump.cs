using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralCharacter.Animation;

namespace ProceduralCharacter.Movement
{
    [RequireComponent(typeof(MovementController))]
    public class Movement_AdvancedJump : MonoBehaviour
    {
        #region Variables (PRIVATE)
        private MovementInterpreter _input;
        private Rigidbody _body;
        private ProceduralMeasurements _measurements;
        private MovementController _movementController;

        [SerializeField, Tooltip("The desired maximum height of a jump.")]
        float _jumpHeight = 2f;
        [SerializeField, Tooltip("A gravity multiplier applied when the jump button is not held.")]
        float _fallMultiplier = 2.5f;

        bool _isJumping = false;
        float jumpVelocity = 0f;
        #endregion

        #region Properties (PUBLIC)
        public bool IsJumping => _isJumping;
        #endregion

        #region Unity Methods
        // Start is called before the first frame update
        private void Start()
        {
            _input = GetComponent<MovementInterpreter>();
            _body = GetComponent<Rigidbody>();
            _measurements = GetComponent<ProceduralMeasurements>();
            _movementController = GetComponent<MovementController>();
        }

        private void Update()
        {
            HandleJump();
        }

        private void FixedUpdate()
        {
            HandleAirtime();
        }
        #endregion

        #region Methods
        private void HandleJump()
        {
            jumpVelocity = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y);

            if (_input.Jump && _measurements.IsGrounded && !_isJumping)
            {
                _body.velocity += new Vector3(0f, jumpVelocity, 0f);
                _isJumping = true;
                _movementController.MovementEnable = !_isJumping;
            }

            if (!_input.Jump)
            {
                if (_isJumping && _measurements.IsGrounded && _body.velocity.y <= 0.01f)
                {
                    _isJumping = false;
                    _movementController.MovementEnable = !_isJumping;
                }
            }
            else
            {
                _movementController.MovementEnable = _isJumping;
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
        #endregion
    }
}

