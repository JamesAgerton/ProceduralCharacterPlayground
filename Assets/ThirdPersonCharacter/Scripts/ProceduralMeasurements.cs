/* ProceduralMeasurements: James Agerton 2021
 * 
 * Description: 
 *      This script makes a large number of measurements intended to be used by other scripts.
 *      
 *      IsGrounded:     Checks if the character is grounded using a checksphere with radius = _groundDistance and the _ground
 *                          layermask.
 *      Velocity:       Harvested and adjusted from rigidbody.
 *      Acceleration:   Using velocity and fixedDeltaTime, calculates the acceleration vector. The value is smoothDamped using
 *                          _accelerationFilter, 49 works well.
 *      Stride Wheel:   The stridewheel is a measurement of distance travelled but in rotations instead of meters. A full rotation
 *                          should be equal to a full walk cycle.
 * 
 * Dependencies: 
 *      Rigidbody: The physics component of the gameobject.
 *               
 * Variables:   
 *      _velocityLimit:         Minimum velocity to consider in calculations.
 *      _accelerationFilter:    Filter value for smoothing Acceleration measurement.
 *      _maxAccelerationScale:  Used for clamping acceleration measurement to a max scale, calculations that use it won't
 *                                  be given values beyond that maximum.
 *      _runStrideRadius:       The size of the stride wheel used to determine the stride length during max speeds.
 *      _walkStrideRadius:      The size of the stride wheel used to determine the stride length during minimum speeds.
 *      _runSpeed:              The maximum speed the character runs, should match a speed set by the MovementController.
 *      _groundDistance:        Radius of the sphere used to check if the character is grounded.
 *      _ground:                Layermask indicating the ground, used to check if the character is grounded.
 *              
 * Properties:
 *      IsGrounded (bool):                      Bool indicating if the gameobject is touching the ground.
 *      Velocity (Vector3):                     Vector3 of the velocity as measured by this script.
 *      VelocityFlat (Vector3):                 The flattened velocity vector, the y value has been set to 0.
 *      VelocityDirection (Vector3):            The normalized velocity vector.
 *      Acceleration (Vector3):                 The acceleration vector measured with the fixedDeltaTime. Instantaneous acceleration
 *                                                  isn't exactly real, but this is pretty close. The value is filtered to reduce
 *                                                  noise.
 *      AccelerationFlat (Vector3):             The flattened acceleration vector, same as above except the y value is set to 0.
 *      AccelerationDirection (Vector3):        The direction of the acceleration vector.
 *      AccelerationFlatDirection (Vector3):    The direction of the flat acceleration vector.
 *      AccelerationMagnitude (float):          The magnitude of the acceleration vector.
 *      AccelerationFlatMagnitude (float):      The magnitude of the flat acceleration vector.
 *      StrideFraction (float):                 The fraction of the stride wheel that the character is currently at.
 *      SpeedFraction (float):                  The fraction of the maximum speed, used to blend between the two stride wheel sizes.
 *      CurrentStrideRadius (float):            The radius of the current stride wheel.
 *      StrideCircumference (float):            The circumference of the current stride wheel.
 */

using UnityEngine;
using UnityEditor;

namespace ProceduralCharacter.Animation
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProceduralMeasurements : MonoBehaviour
    {
        #region Variables(Private)
        private Rigidbody _body;

        [Header("Velocity & Acceleration")]
        [SerializeField, Tooltip("Minimum velocity to use in calculations.")]
        private float _velocityLimit = 0.1f;
        [SerializeField, Tooltip("Filter value for smoothing Acceleration measurement.")]
        private float _accelerationFilter = 5f;
        [SerializeField, Tooltip("Used for clamping acceleration measurement to a max scale.")]
        private float _maxAccelerationScale = 5f;

        [Header("Stride Wheel")]
        [SerializeField, Tooltip("The size of the stride wheel used to determine the stride length during max speeds.")]
        private float _runStrideRadius = 1.25f;
        [SerializeField, Tooltip("The size of the stride wheel used to determine the stride length during minimum speeds.")]
        private float _walkStrideRadius = 0.25f;
        [SerializeField, Tooltip("")]
        private float _minSpeed = 2.5f;
        [SerializeField, Tooltip("The maximum speed the character runs, should match a speed set by the MovementController.")]
        private float _maxSpeed = 8f;   //Velocity magnitude when wheel should be at its largest
        [SerializeField, Tooltip("Length of the raycast used to check if the character is grounded.")]
        private float _groundDistance = 1f;
        [SerializeField, Tooltip("Layermask indicating the ground, used to check if the character is grounded.")]
        public LayerMask _ground;

        private bool _isGrounded = false;
        private RaycastHit _groundHit;

        private Vector3 _velocity = Vector3.forward;
        private Vector3 _velocityFlat = Vector3.forward;
        private Vector3 _velocityDirection = Vector3.forward;

        private Vector3 _lastVelocity = Vector3.zero;
        private Vector3 _lastVelocityFlat = Vector3.zero;
        private Vector3 _acceleration = Vector3.zero;
        private Vector3 _accelerationFlat = Vector3.zero;
        private Vector3 _accelerationDirection = Vector3.zero;
        private Vector3 _accelerationFlatDirection = Vector3.zero;
        private float _accelerationMagnitude = 0f;
        private float _accelerationFlatMagnitude = 0f;
        private Vector3 _accDirVel = Vector3.zero;
        private Vector3 _accFlatDirVel = Vector3.zero;

        private float _strideAngle = 0f;
        private float _strideFraction = 0f;
        private float _refSpeedFractionVelocity = 0f;
        private float _speedFraction = 0f;  //fraction used to transition stride wheel radius based on speed
        private float _currentStrideRadius = 1f;
        private float _strideCircumference = 0f;
        #endregion

        #region Properties
        public bool IsGrounded => _isGrounded;
        public RaycastHit GroundHit => _groundHit;
        public LayerMask Ground => _ground;
        public Vector3 Velocity => _velocity;
        public Vector3 VelocityFlat => _velocityFlat;
        public Vector3 VelocityDirection => _velocityDirection;
        public Vector3 Acceleration => _acceleration;
        public Vector3 AccelerationFlat => _accelerationFlat;
        public Vector3 AccelerationDirection => _accelerationDirection;
        public Vector3 AccelerationFlatDirection => _accelerationFlatDirection;
        public float AccelerationMagnitude => _accelerationMagnitude;
        public float AccelerationFlatMagnitude => _accelerationFlatMagnitude;
        public float StrideFraction => _strideFraction;
        public float SpeedFraction => _speedFraction;
        public float CurrentStrideRadius => _currentStrideRadius;
        public float StrideCircumference => _strideCircumference;
        #endregion

        #region Unity Methods
        // Start is called before the first frame update
        void Start()
        {
            _body = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            Ray ray = new Ray(_body.position + Vector3.up * _groundDistance, Vector3.down);
            if (Physics.Raycast(ray, out _groundHit, _groundDistance + 0.5f, _ground))
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }

            //Find velocity direction and flatten vector
            CalculateVelocity();

            //Calculate Stride wheel?
            CalculateStrideWheel();

            //Find average acceleration over deltatime
            CalculateAcceleration();
        }

        private void FixedUpdate()
        {

        }

        private void OnDrawGizmos()
        {
            //Draw IsGrounded Sphere
            if (_isGrounded)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_groundHit.point, 0.3f);
            }
            else
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position - Vector3.up * 0.5f, 0.3f);
            }

            //Draw Velocity Direction
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, transform.position +
                _velocityDirection + Vector3.up);
            Gizmos.DrawWireSphere(transform.position + _velocityDirection + Vector3.up, 0.2f);
            //raw Velocity vector
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(transform.position + Vector3.up, transform.position + (_velocity/5f) + Vector3.up);
            Gizmos.DrawWireSphere(transform.position + (_velocity/5f) + Vector3.up, 0.15f);

            //Draw Acceleration Direction
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up,
                transform.position + _accelerationDirection.normalized *
                _accelerationMagnitude + Vector3.up);
            Gizmos.DrawWireSphere(transform.position + _accelerationDirection.normalized *
                _accelerationMagnitude + Vector3.up, 0.1f);
            Gizmos.DrawLine(transform.position, transform.position + _accelerationDirection.normalized *
                _accelerationMagnitude + Vector3.up);

            //Draw Stride Wheel
            Gizmos.color = Color.white;
            //Handles.color = Color.white;
            //Handles.DrawWireDisc(transform.position + Vector3.up * _currentStrideRadius,
            //    transform.GetChild(0).right, _currentStrideRadius);
            for (int i = 0; i < 4; i++)
            {
                float div = 0.75f;
                float angle = (_strideFraction + (i * 0.25f) % 1f) * 360f * Mathf.PI / 180f;
                float zCoord = _currentStrideRadius * Mathf.Cos(angle);
                float zinCoord = _currentStrideRadius * div * Mathf.Cos(angle);
                float yCoord = _currentStrideRadius * Mathf.Sin(angle);
                float yinCoord = _currentStrideRadius * div * Mathf.Sin(angle);
                Gizmos.DrawLine(
                    transform.GetChild(0).TransformPoint(
                        new Vector3(0f, yinCoord + _currentStrideRadius, -zinCoord)) -
                        transform.GetChild(0).position + transform.position,
                    transform.GetChild(0).TransformPoint(
                        new Vector3(0f, yCoord + _currentStrideRadius, -zCoord)) -
                        transform.GetChild(0).position + transform.position
                    );
            }
        }
        #endregion

        #region Methods
        private void CalculateVelocity()
        {
            _velocity = _body.velocity;
            _velocityFlat = _velocity;
            _velocityFlat.y = 0;
            if (_velocityFlat.magnitude > _velocityLimit)
            {
                _velocityDirection = Vector3.Normalize(_velocityFlat);
            }
        }

        private void CalculateAcceleration()
        {
            _acceleration = ((_velocity - _lastVelocity) / Time.fixedDeltaTime);
            _lastVelocity = _velocity;
            _accelerationFlat = ((_velocityFlat - _lastVelocityFlat) / Time.fixedDeltaTime);
            _lastVelocityFlat = _velocityFlat;
            //Smooth that shaky shit
            //_accelerationDirection = Vector3.Lerp(_accelerationDirection, _acceleration, _accelerationFilter * Time.deltaTime);
            //_accelerationFlatDirection = Vector3.Lerp(_accelerationFlatDirection, _accelerationFlat, _accelerationFilter * Time.deltaTime);
            _accelerationDirection = Vector3.SmoothDamp(_accelerationDirection, _acceleration, ref _accDirVel, _accelerationFilter);
            _accelerationFlatDirection = Vector3.SmoothDamp(_accelerationFlatDirection, _accelerationFlat, ref _accFlatDirVel, _accelerationFilter);
            
            //rescale?
            _accelerationMagnitude =Mathf.Clamp(_accelerationDirection.magnitude, 0, _maxAccelerationScale) / _maxAccelerationScale;
            _accelerationFlatMagnitude = Mathf.Clamp(_accelerationFlatDirection.magnitude, 0, _maxAccelerationScale) / _maxAccelerationScale;
        }

        private void CalculateStrideWheel()
        {
            //distance = speed * time
            //I can calculate the circumference of a stride wheel based on the radius
            //and then I can use modulo and division to pick which part of the wheel 
            //is currently pointed directly down

            //calculate which radius to use based on current velocity
            float SpeedTarget = 0f;
            if (_velocityFlat.magnitude > _minSpeed)
            {
                SpeedTarget = Mathf.Clamp(_velocityFlat.magnitude / _maxSpeed, 0f, 1f);
            }
            else if (_velocityFlat.magnitude > _velocityLimit)
            {
                SpeedTarget = 0.1f;
            }
            _speedFraction = Mathf.SmoothDamp(_speedFraction, SpeedTarget, ref _refSpeedFractionVelocity, 0.1f);
            _currentStrideRadius = Mathf.Lerp(_walkStrideRadius, _runStrideRadius, _speedFraction);

            _strideCircumference = 2f * Mathf.PI * _currentStrideRadius;
            float _frameDistance = _velocityFlat.magnitude * (Time.deltaTime * Time.timeScale);
            float Angle = (_frameDistance / _strideCircumference) * 360f;
            _strideAngle = (_strideAngle + Angle) % 360f;
            _strideFraction = _strideAngle / 360f;
        }
        #endregion
    }
}
