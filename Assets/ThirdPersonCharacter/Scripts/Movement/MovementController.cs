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
using ProceduralCharacter.Animation;

namespace ProceduralCharacter.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(MovementInterpreter), typeof(SphereCollider))]
    public class MovementController : MonoBehaviour
    {
        #region Variables(Private)
        private MovementInterpreter _input;
        private Rigidbody _body;
        private SphereCollider _sphereCollider;

        [Header("Speed")]
        [SerializeField, Tooltip("The default movement speed.")]
        float _defaultSpeed = 5f;
        [SerializeField, Tooltip("The default movement speed while crouching.")]
        float _crouchSpeed = 2.5f;
        [SerializeField, Tooltip("The default sprint speed.")]
        float _sprintSpeed = 10f;
        [SerializeField, Tooltip("The default speed when movement is disabled.")]
        float _disabledSpeed = 0f;

        [Space]
        [SerializeField, Tooltip("The time over which a change in speed is smoothed.")]
        float _walkSpeedSmoothTime = 0.1f;
        [SerializeField, Tooltip("The smooth time for changing velocity magnitude.")]
        float _movementSmoothTime = 0.2f;
        [SerializeField, Tooltip("The smooth time for changing velocity direction.")]
        float _directionSmoothTime = 0.05f;

        [Header("Ground Check")]
        [SerializeField, Tooltip("Length of the raycast used to check if the character is grounded.")]
        private float _groundDistance = 1f;
        [SerializeField]
        private float _groundCheckSphereRadius = 0.3f;
        [SerializeField]
        private float _groundCheckOvershoot = 0.1f;
        [SerializeField, Tooltip("Layermask indicating the ground, used to check if the character is grounded.")]
        public LayerMask _ground;

        [Space]
        [SerializeField]
        float _slopeLimit = 35f;
        [SerializeField]
        AnimationCurve _slopeSpeed;
        float _slopeYMax = 0f;
        [Space]

        float _walkSpeed = 0f;
        float _wSVelocity = 0f;

        [SerializeField]
        bool _slopeCircleGizmo = false;
        float _desiredSpeedRef = 0f;
        float _desiredSpeedTime = 0.2f;
        float _desiredSpeed = 0f;
        float _currentSpeed = 0f;
        float _deltaSpeed = 0f;
        Vector3 _desiredDirectionRef = Vector3.zero;

        bool _isMoving = false;
        bool _isGrounded = false;
        RaycastHit _groundHit;

        Vector3 _desiredDirection = Vector3.zero;
        #endregion

        #region Properties
        public bool MovementEnable = true;
        public bool IsMoving => _isMoving;
        public float Speed => _defaultSpeed;
        public float CrouchSpeed => _crouchSpeed;
        #endregion

        #region UnityMethods
        // Start is called before the first frame update
        void Start()
        {
            _input = GetComponent<MovementInterpreter>();
            _body = GetComponent<Rigidbody>();
            _sphereCollider = GetComponent<SphereCollider>();

            //_groundDistance = _sphereCollider.radius;

            _slopeYMax = Mathf.Sin(Mathf.Deg2Rad * _slopeLimit);
        }

        // Update is called once per frame
        void Update()
        {
            HandleGrounding();
        }

        private void FixedUpdate()
        {
            Ray ray = new Ray(_body.position + Vector3.up, Vector3.down);
            //if (Physics.Raycast(ray, out _groundHit, _groundDistance + _groundCheckOvershoot, _ground))
            if (Physics.SphereCast(ray, _groundCheckSphereRadius, out _groundHit, _groundDistance + _groundCheckOvershoot, _ground))
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }

            //Desired XZ plane speed
            HandleMovement();
        }

        private void OnDrawGizmos()
        {
            //Draw IsGrounded Sphere
            if (_isGrounded)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_groundHit.point, _groundCheckSphereRadius);

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
            else
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position - Vector3.up * _groundCheckOvershoot, _groundCheckSphereRadius);
            }

            Gizmos.DrawLine(transform.position, transform.position + _desiredDirection);
        }
        #endregion

        #region Methods
        private void HandleGrounding()
        {
            float speed = _walkSpeed;
            if (MovementEnable && _isGrounded)
            {
                if (_input.Crouch)          // Crouching
                {
                    speed = _crouchSpeed;
                }else if (_input.Sprint)    // Sprint
                {
                    speed = _sprintSpeed;
                }
                else                        // Default/Walking
                {
                    speed = _defaultSpeed;
                }
            }
            else                            // Movement is disabled
            {
                speed = _disabledSpeed;
            }

            _walkSpeed = Mathf.SmoothDamp(_walkSpeed, speed, ref _wSVelocity, _walkSpeedSmoothTime);
        }

        private void HandleMovement()
        {
            _desiredSpeed = _input.MoveDirection.magnitude * _walkSpeed;
            _currentSpeed = (new Vector3(_body.velocity.x, 0f, _body.velocity.z)).magnitude;
            _deltaSpeed = Mathf.SmoothDamp(_currentSpeed, _desiredSpeed, ref _desiredSpeedRef, _desiredSpeedTime);

            _desiredDirection = Vector3.SmoothDamp(_desiredDirection, _input.MoveDirection, ref _desiredDirectionRef, _directionSmoothTime);
            _desiredDirection = HandleSlope(_desiredDirection.normalized);

            float y = _body.velocity.y;

            if (_isGrounded)
            {
                _body.velocity = (_desiredDirection * _deltaSpeed);
            }

            if(_input.MoveDirection != Vector3.zero)
            {
                _isMoving = true;
            }
            else
            {
                _isMoving = false;
            }
        }

        private Vector3 HandleSlope(Vector3 input)
        {
            Vector3 output = input;
            if (_isGrounded && _slopeLimit > 0f)
            {
                output = Vector3.ProjectOnPlane(input, _groundHit.normal);
                float ya = _slopeSpeed.Evaluate(Mathf.Sin(Mathf.Deg2Rad * Vector3.Angle(Vector3.up, _groundHit.normal)) / _slopeYMax);
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
