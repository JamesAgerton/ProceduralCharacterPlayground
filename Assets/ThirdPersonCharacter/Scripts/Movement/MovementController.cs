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
        private Rigidbody _RB;
        //private SphereCollider _sphereCollider;

        [Header("Grounding")]
        Vector3 DownDir = Vector3.down;
        [SerializeField]
        float _rayLength = 2f;
        [SerializeField]
        float _RideHeight = 1f;
        [SerializeField]
        float _RideSpringStrength = 10f;
        [SerializeField]
        float _RideSpringDamper = 10f;
        [SerializeField, Tooltip("Layermask indicating the ground, used to check if the character is grounded.")]
        public LayerMask _ground;

        Vector3 _groundVel = Vector3.zero;

        [Header("Speed")]
        [SerializeField]
        float _MaxSpeed = 8f;

        [SerializeField]
        float _Acceleration = 200f;
        [SerializeField]
        AnimationCurve _AccelerationFactorFromDot;
        [SerializeField]
        float _MaxAccelForce = 150f;
        [SerializeField]
        AnimationCurve _MaxAccelerationForceFactorFromDot;
        [SerializeField]
        Vector3 _ForceScale = Vector3.zero;
        [SerializeField]
        float _GravityScaleDrop = 10f;

        [Space]
        [SerializeField]
        float _sprintFactor = 1.5f;
        [SerializeField]
        float _walkFactor = 0.5f;
        [SerializeField]
        float _crouchFactor = 0.4f;
        [SerializeField]
        float _disabledFactor = 0.1f;
        [Space]
        [SerializeField]
        float _speedFactorSmoothTime = 0.01f;
        float _speedFactor = 1f;
        float _speedRefVel = 0f;

        Vector3 _GoalVel = Vector3.zero;

        [Header("Slope Check")]
        [SerializeField]
        float _slopeLimit = 35f;
        [SerializeField]
        AnimationCurve _slopeSpeed;
        float _slopeYMax = 0f;

        [SerializeField]
        bool _slopeCircleGizmo = false;


        bool _isMoving = false;
        bool _isGrounded = false;
        RaycastHit _rayHit;

        Vector3 _UnitGoal = Vector3.zero;
        #endregion

        #region Properties
        public bool MovementEnable = true;
        public bool IsMoving => _isMoving;
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
            Ray ray = new Ray(_RB.position + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out _rayHit, _rayLength, _ground))
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }

            if(_isGrounded && _rayHit.rigidbody != null)
            {
                _groundVel = _rayHit.rigidbody.velocity;
            }
            else
            {
                _groundVel = Vector3.zero;
            }

            HandleGrounding();
            //Desired XZ plane speed
            HandleMovement();
        }

        private void OnDrawGizmos()
        {
            //Draw IsGrounded Sphere
            if (_isGrounded)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_rayHit.point, 0.1f);

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
            if (_isGrounded)
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

                float x = _rayHit.distance - _RideHeight;

                float springForce = (x * _RideSpringStrength) - (relVel * _RideSpringDamper);

                //Debug.DrawLine(transform.position, transform.position + (rayDir * springForce), Color.yellow);

                _RB.AddForce(rayDir * springForce);

                if (hitBody != null)
                {
                    hitBody.AddForceAtPosition(rayDir * -springForce, _rayHit.point);
                }
            }
        }

        private void HandleSpeedFactor()
        {
            float speed = _walkFactor;
            if(MovementEnable && _isGrounded)
            {
                if (_input.Crouch)
                {
                    speed = _crouchFactor;
                }else if (_input.Sprint)
                {
                    speed = _sprintFactor;
                }
                else
                {
                    speed = 1f;
                }
            }
            else
            {
                speed = 1f; //_disabledFactor;
            }

            _speedFactor = Mathf.SmoothDamp(_speedFactor, speed, ref _speedRefVel, _speedFactorSmoothTime);
        }

        private void HandleMovement()
        {
            if (_isGrounded)
            {
                //input ...
                _UnitGoal = HandleSlope(_input.MoveDirection);
                if (_UnitGoal.magnitude > 1f)
                {
                    _UnitGoal.Normalize();
                }

                //calculate new goal vel...
                Vector3 unitVel = _GoalVel.normalized;

                float velDot = Vector3.Dot(_UnitGoal, unitVel);

                float accel = _Acceleration * _AccelerationFactorFromDot.Evaluate(velDot);

                Vector3 goalVel = _UnitGoal * _MaxSpeed * _speedFactor;

                _GoalVel = Vector3.MoveTowards(_GoalVel, (goalVel) + (_groundVel),
                    accel * Time.fixedDeltaTime);

                //Actual force...
                Vector3 neededAccel = (_GoalVel - _RB.velocity) / Time.fixedDeltaTime;

                float maxAccel = _MaxAccelForce * _MaxAccelerationForceFactorFromDot.Evaluate(velDot);// * _maxAccelForceFactor;

                neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

                _RB.AddForce(Vector3.Scale(neededAccel * _RB.mass, _ForceScale));
            }
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
        #endregion
    }
}
