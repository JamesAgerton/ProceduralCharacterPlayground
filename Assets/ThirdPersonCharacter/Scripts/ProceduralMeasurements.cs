/* ProceduralMeasurements: James Agerton 2021
 * 
 * This script does the calculations that are used in the ProceduralAnimation script. It also
 * exposes several useful values that could be used elsewhere.
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

        [SerializeField]
        private float VelocityLimit = 0.1f;
        [SerializeField]
        private float AccFilter = 5f;
        [SerializeField]
        private float _maxAccScale = 5f;
        [SerializeField]
        private float _runStrideRadius = 1.25f;
        [SerializeField]
        private float _walkStrideRadius = 0.25f;
        [SerializeField]
        private float _runSpeed = 8f;   //Velocity magnitude when wheel should be at its largest
        [SerializeField]
        private float groundDistance = 0.5f;
        [SerializeField]
        public LayerMask Ground;

        private bool _isGrounded = false;

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

        private float _strideAngle = 0f;
        private float _strideFraction = 0f;
        private float _speedFraction = 0f;  //fraction used to transition stride wheel radius based on speed
        private float _currentStrideRadius = 1f;
        private float _strideCircumference = 0f;
        #endregion

        #region Properties
        public bool IsGrounded => _isGrounded;
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
            _isGrounded = Physics.CheckSphere(transform.position,
                groundDistance, Ground, QueryTriggerInteraction.Ignore);

            //Find velocity direction and flatten vector
            CalculateVelocity();

            //Find average acceleration over deltatime
            CalculateAcceleration();

            //Calculate Stride wheel?
            CalculateStrideWheel();
        }

        private void OnDrawGizmos()
        {
            //Draw IsGrounded Sphere
            if (_isGrounded)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.magenta;
            }
            Gizmos.DrawWireSphere(transform.position, groundDistance);

            //Draw Velocity Direction
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, transform.position +
                _velocityDirection + Vector3.up);
            Gizmos.DrawWireSphere(transform.position + _velocityDirection + Vector3.up, 0.2f);

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
            Handles.color = Color.white;
            Handles.DrawWireDisc(transform.position + Vector3.up * _currentStrideRadius,
                transform.GetChild(0).right, _currentStrideRadius);
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
            _velocityFlat = _body.velocity;
            _velocityFlat.y = 0;
            if (_velocityFlat.magnitude > VelocityLimit)
            {
                _velocityDirection = Vector3.Normalize(_velocityFlat);
            }
        }

        private void CalculateAcceleration()
        {
            _acceleration = (_velocity - _lastVelocity) / Time.fixedDeltaTime;
            _lastVelocity = _velocity;
            _accelerationFlat = (_velocityFlat - _lastVelocityFlat) / Time.fixedDeltaTime;
            _lastVelocityFlat = _velocityFlat;
            //Smooth that shaky shit
            _accelerationDirection = Vector3.Lerp(_acceleration, _accelerationDirection, AccFilter * Time.fixedDeltaTime);
            _accelerationFlatDirection = Vector3.Lerp(_accelerationFlat, _accelerationFlatDirection, AccFilter * Time.fixedDeltaTime);
            //rescale?
            _accelerationMagnitude = Mathf.Clamp(_accelerationDirection.magnitude, 0, _maxAccScale) / _maxAccScale;
            _accelerationFlatMagnitude = Mathf.Clamp(_accelerationFlatDirection.magnitude, 0, _maxAccScale) / _maxAccScale;
        }

        private void CalculateStrideWheel()
        {
            //distance = speed * time
            //I can calculate the circumference of a stride wheel based on the radius
            //and then I can use modulo and division to pick which part of the wheel 
            //is currently pointed directly down

            //calculate which radius to use based on current velocity
            _speedFraction = Mathf.Clamp(_velocityFlat.magnitude / _runSpeed, 0f, 1f);
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
