/* ProceduralAnimation: James Agerton 2021
 * 
 * Animation script (also handles the collider size). This script controls which poses are applied
 * and the rate of blend between poses. The _animator variable needs to be set manually and should
 * follow the names and connections that the ProceduralAnimatorController.
 * 
 * When setting up your character, be sure that all empty GameObjects are aligned along the y axis.
 * Multiple functions assume that the XFormPivots have a x and z component of 0f.
 */

using UnityEngine;
using ProceduralCharacter.Movement;

namespace ProceduralCharacter.Animation
{

    [RequireComponent(typeof(ProceduralMeasurements), typeof(MovementInterpreter), typeof(CapsuleCollider))]
    public class ProceduralAnimation : MonoBehaviour
    {
        #region Variables (private)
        private ProceduralMeasurements _measurements;
        private MovementInterpreter _input;
        private CapsuleCollider _collider;
        private Transform _XForm;
        private Transform _XFormTiltPivot;
        private Transform _XFormTurnPivot;

        [SerializeField]
        public Animator _animator;

        [Header("Turning")]
        [SerializeField]
        private float _turnThreshold = 0.1f;
        [SerializeField]
        private float _turnRate = 60f;

        [Header("Acceleration Tilt")]
        [SerializeField]
        private float _accelerationTiltMax = 30f;

        [Header("Stride")]
        [SerializeField]
        public AnimationCurve _strideWeightCurve;
        [SerializeField]
        public AnimationCurve _strideSpeedCurve;
        [SerializeField]
        public AnimationCurve _strideBounceCurve;
        [SerializeField]
        private float _bounceSmoothTime = 0.1f;
        [SerializeField]
        private float _bounceHeight = 0.1f;

        [Header("Crouch")]
        [SerializeField]
        private float _crouchOffset = 0.1f;
        [SerializeField]
        private float _crouchStiffness = 50f;
        [SerializeField]
        private float _crouchDamping = 6f;
        [SerializeField]
        private Vector2 _colliderHeightAndOffsetScale = new Vector2(0.5f, 0.5f);

        [Header("Jump")]
        [SerializeField]
        private float _jumpTransitionTime = 0.15f;
        [SerializeField]
        private float _jumpCrouchEffector = 0.1f;


        private Vector3 _currentBounceVelocity;

        float _targetCrouchFraction = 0f;
        private float _currentCrouchFraction;
        private float _currentCrouchVelocity;
        private float _crouchThreshold = 0.01f;
        private float _crouchVelocityThreshold = 0.01f;
        private Vector3 _colliderCenter = new Vector3(0f, 1f, 0f);
        private float _colliderHeight = 2f;
        private Vector3 _XFormTiltCenter = new Vector3(0f, 1f, 0f);

        float _currJumpFraction = 0.5f;
        float _jumpVelocity = 0f;
        float _targetJumpFraction = 0f;
        #endregion

        #region Properties

        #endregion

        #region Unity Methods
        // Start is called before the first frame update
        void Start()
        {
            _measurements = GetComponent<ProceduralMeasurements>();
            _input = GetComponent<MovementInterpreter>();
            _collider = GetComponent<CapsuleCollider>();

            if (_animator == null)
            {
                Debug.LogError("No animated character assigned!", this);
                return;
            }
            else
            {
                //Make sure that basic parent/child structure is correct
                if (_animator.transform.parent.parent.parent == this.transform &&
                    _animator.transform.parent.parent != this.transform &&
                    _animator.transform.parent != this.transform &&
                    _animator.transform != this.transform)
                {
                    _XForm = _animator.transform;
                    _XFormTiltPivot = _animator.transform.parent;
                    _XFormTurnPivot = _animator.transform.parent.parent;
                }
                else
                {
                    Debug.LogWarning("Animated character must be a child of this transform.", this);
                }
            }

            _currentCrouchFraction = 1f - _crouchOffset;
            _colliderCenter = _collider.center;
            _colliderHeight = _collider.height;
            _XFormTiltCenter = _XFormTiltPivot.localPosition;
        }

        // Update is called once per frame
        void Update()
        {
            //Time.timeScale = 0.25f;
            if (_XForm != null)
            {
                //Rotate character to face direction of velocity
                HandleVelocityTurn();
                //Rotate character around COM toward acceleration
                HandleAccelerationTilt();
                //Bounce
                HandleBounce();
            }

            HandleCrouch(_input.Crouch);
            HandleJump(_measurements.IsGrounded);
            CalculateCrouchPosition(_targetCrouchFraction);

            HandlePose();
        }

        void OnDrawGizmos()
        {
            //Draw spring
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position + (1f - _targetCrouchFraction) * Vector3.up, 0.075f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * (1f - _currentCrouchFraction), 0.08f);
        }
        #endregion

        #region Methods
        private void HandleVelocityTurn()
        {
            float angle = Vector3.Angle(_XForm.forward, _measurements.VelocityDirection);
            if (angle > _turnThreshold)
            {
                _XFormTurnPivot.forward = Vector3.Slerp(
                    _XFormTurnPivot.forward, _measurements.VelocityDirection, _turnRate * Time.fixedDeltaTime);
            }
        }

        private void HandleAccelerationTilt()
        {
            Vector3 rotateAxis = Vector3.Cross(Vector3.up,
                _measurements.AccelerationFlatDirection.normalized).normalized;

            _XFormTiltPivot.rotation = _XFormTurnPivot.rotation;
            if (_XFormTiltPivot == null)
            {
                Debug.LogWarning("Character missing tilt pivot transform.", this);
            }
            else
            {
                _XFormTiltPivot.Rotate(rotateAxis, _measurements.AccelerationFlatMagnitude *
                    _accelerationTiltMax, Space.World);
            }
        }

        public void TiltCharacter(Vector3 rotateAxis, float rotateAmount)
        {
            _XFormTiltPivot.rotation = _XFormTurnPivot.rotation;
            if (_XFormTiltPivot == null)
            {
                Debug.LogWarning("Character missing tilt pivot transform.", this);
            }
            else
            {
                _XFormTiltPivot.Rotate(rotateAxis, rotateAmount, Space.World);
            }
        }

        private void HandlePose()
        {
            float frac = _strideWeightCurve.Evaluate(_measurements.StrideFraction);
            float spd = _strideSpeedCurve.Evaluate(_measurements.SpeedFraction);
            _animator.SetFloat("CrouchFraction", _currentCrouchFraction);
            _animator.SetFloat("JumpFraction", _currJumpFraction);
            _animator.SetBool("IsGrounded", _measurements.IsGrounded);
            _animator.SetFloat("StrideFraction", frac);
            _animator.SetFloat("StrideSpeed", spd);
        }

        private void HandleBounce()
        {
            if (_measurements.VelocityFlat.magnitude > 0.1f && _measurements.IsGrounded)
            {
                float currentHeight = _strideBounceCurve.Evaluate(_measurements.StrideFraction * 2f % 1f) *
                    _strideSpeedCurve.Evaluate(_measurements.SpeedFraction);

                Vector3 bounce = new Vector3(0f, currentHeight * _bounceHeight, 0f);
                _XFormTurnPivot.localPosition = Vector3.SmoothDamp(_XFormTurnPivot.localPosition, bounce,
                    ref _currentBounceVelocity, _bounceSmoothTime);
            }
            else
            {
                _XFormTurnPivot.localPosition = Vector3.SmoothDamp(_XFormTurnPivot.localPosition, Vector3.zero,
                    ref _currentBounceVelocity, _bounceSmoothTime);
            }
        }

        private void HandleCrouch(bool crouch)
        {
            if (crouch)
            {
                _targetCrouchFraction = 1f - _crouchOffset;
            }
            else
            {
                _targetCrouchFraction = 0f + _crouchOffset;
            }

            _collider.height = _colliderHeight * (1f - (_currentCrouchFraction * _colliderHeightAndOffsetScale.x));
            _collider.center = _colliderCenter * (1f - (_currentCrouchFraction * _colliderHeightAndOffsetScale.y));
            _XFormTiltPivot.localPosition = _XFormTiltCenter * (1f - (_currentCrouchFraction * _colliderHeightAndOffsetScale.y));
        }

        private void HandleJump(bool grounded)
        {
            if (grounded)
            {
                if (_input.Jump)
                {
                    _currJumpFraction = 0f;
                }
                else
                {
                    _currJumpFraction = 0.5f;
                }

                if (_measurements.Acceleration.y > 0f)
                {
                    _targetCrouchFraction += _measurements.Acceleration.y * _jumpCrouchEffector * Time.fixedDeltaTime;
                }
            }
            else
            {
                if (_measurements.Velocity.y > 0)
                {
                    _targetJumpFraction = 0f;
                }
                else if (_measurements.Velocity.y < 0)
                {
                    _targetJumpFraction = 1f;
                }
            }

            _currJumpFraction = Mathf.SmoothDamp(_currJumpFraction, _targetJumpFraction, ref _jumpVelocity, _jumpTransitionTime);
        }

        private void CalculateCrouchPosition(float targetValue)
        {
            float dampingFactor = Mathf.Max(0f, 1f - _crouchDamping * Time.deltaTime);
            float acceleration = (targetValue - _currentCrouchFraction) * _crouchStiffness * Time.deltaTime;
            _currentCrouchVelocity = _currentCrouchVelocity * dampingFactor + acceleration;
            _currentCrouchFraction += _currentCrouchVelocity * Time.deltaTime;

            if (Mathf.Abs(_currentCrouchFraction - targetValue) < _crouchThreshold && Mathf.Abs(_currentCrouchVelocity) < _crouchVelocityThreshold)
            {
                _currentCrouchFraction = targetValue;
                _currentCrouchVelocity = 0f;
            }
        }
        #endregion
    }
}
