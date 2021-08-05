using UnityEngine;

namespace ProceduralCharacter.Movement
{
    /// <summary>
    /// Use physics and raycast to apply an upward force
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(MovementInterpreter))]
    public class MovementFloatRide : MonoBehaviour
    {
        #region Variables (PRIVATE)
        private Rigidbody _RB;

        [Header("Grounding")]
        [SerializeField]
        bool _groundRayGizmo = false;
        [SerializeField]
        float _sphereRadius = 0.3f;
        [SerializeField, Tooltip("Direction to aim raycast.")]
        Vector3 _downDir = Vector3.down;
        [SerializeField, Tooltip("Height above the ground for the transform to float")]
        float _rideHeight = 1f;
        [SerializeField, Range(-1, 1), Tooltip("Downward ray overshoot, allows for downward pull as well as upward push.")]
        float _rayOvershoot = 0.5f;
        [SerializeField, Tooltip("Strength of the spring which holds up the character, stronger makes faster movement. Relative to rigidbody mass.")]
        float _RideSpringStrength = 100f;
        [SerializeField]
        float _RideSpringDamper = 10f;
        [SerializeField, Tooltip("Layermask indicating the ground, used to check if the character is grounded.")]
        public LayerMask _ground;

        [Header("Step Raycast Settings")]
        [SerializeField]
        bool _drawStepSmoothGizmo = false;
        [SerializeField, Min(0), Tooltip("The number of rays used to smooth steps.")]
        int _checks = 4;
        [SerializeField, Range(0f, 180f), Tooltip("The range of the circle checked in front of the character.")]
        float _angleRange = 60f;
        [SerializeField, Min(0), Tooltip("Max speed used to scale Angle Range proportional to speed.")]
        float _maxSpeed = 12f;

        RaycastHit _rayHitInfo;
        RaycastHit _forwardHitInfo;
        bool _isGrounded = false;
        #endregion

        #region Properties (PUBLIC)
        [Space]
        public bool FloatEnable = true;
        public LayerMask Ground => _ground;
        public Vector3 DownDir => _downDir;
        [HideInInspector]
        public bool FloatEnableLock = false;
        public bool IsGrounded => _isGrounded;
        public RaycastHit RayHitInfo => _rayHitInfo;
        public float RideHeight => _rideHeight;
        [Tooltip("Modifier used by other scripts to adjust the height of the ride.")]
        public float RideHeightMultiplier = 1f;
        #endregion

        #region Unity Methods
        // Start is called before the first frame update
        private void Start()
        {
            _RB = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            //Reset Ride Height modifier
            RideHeightMultiplier = 1f;
        }

        private void FixedUpdate()
        {
            Ray groundingRay = new Ray(_RB.position, _downDir);
            if (Physics.Raycast(groundingRay, out _rayHitInfo, (_rideHeight * RideHeightMultiplier) + _rayOvershoot, _ground))
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }

            SmoothStep();

            HandleGrounding();
        }

        private void OnDrawGizmos()
        {
            if (_groundRayGizmo)
            {
                if (_isGrounded)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                Vector3 center = new Vector3(transform.position.x, transform.position.y - (((_rideHeight * RideHeightMultiplier) + _rayOvershoot) / 2f), transform.position.z);
                Vector3 size = new Vector3(0.01f, (_rideHeight * RideHeightMultiplier) + _rayOvershoot, 0.02f);
                Gizmos.DrawCube(center, size);
                Gizmos.DrawWireCube(_rayHitInfo.point, Vector3.one * 0.025f);

                Gizmos.color = Color.yellow;

                if (_drawStepSmoothGizmo && Application.isPlaying)
                {
                    Vector3 axis = transform.right;
                    RaycastHit testHitInfo;
                    _forwardHitInfo = _rayHitInfo;
                    float Angle = _angleRange * (Mathf.Clamp01(_RB.velocity.magnitude / _maxSpeed));

                    for (int i = 1; i <= _checks; i++)
                    {
                        //Cast a ray in a range of angles forward from the groundingRay
                        Vector3 dir = Quaternion.Euler(axis * (float)(i * (-Angle / _checks))) * _downDir;
                        Ray ray = new Ray(_RB.position, dir);
                        Gizmos.DrawRay(ray);
                    }
                    Gizmos.DrawWireSphere(_forwardHitInfo.point, 0.01f);
                }
            }
        }
        #endregion

        #region Methods
        private void HandleGrounding()
        {
            if (_isGrounded && FloatEnable)
            {
                Vector3 vel = _RB.velocity;
                Vector3 rayDir = _downDir;

                Vector3 otherVel = Vector3.zero;
                Rigidbody hitBody = _rayHitInfo.rigidbody;
                if (hitBody != null)
                {
                    otherVel = hitBody.velocity;
                }

                float rayDirVel = Vector3.Dot(rayDir, vel);
                float otherDirVel = Vector3.Dot(rayDir, otherVel);

                float relVel = rayDirVel - otherDirVel;

                float dist = _rayHitInfo.distance;
                if (_forwardHitInfo.distance + _sphereRadius - 1f < _rayHitInfo.distance)
                {
                    dist = _forwardHitInfo.distance;
                }

                float x = (dist - (_rideHeight * RideHeightMultiplier));

                float springForce = (x * _RideSpringStrength) - (relVel * _RideSpringDamper);

                //Debug.DrawLine(transform.position, transform.position + (rayDir * springForce), Color.yellow);

                _RB.AddForce(rayDir * springForce);

                if (hitBody != null)
                {
                    hitBody.AddForceAtPosition(rayDir * -springForce, _rayHitInfo.point);
                }
            }
        }

        private void SmoothStep()
        {
            if (_isGrounded)
            {
                Vector3 axis = transform.right;
                RaycastHit testHitInfo;
                _forwardHitInfo = _rayHitInfo;
                float Angle = _angleRange * (Mathf.Clamp01(_RB.velocity.magnitude / _maxSpeed));

                for (int i = 1; i <= _checks; i++)
                {
                    //Cast a ray in a range of angles forward from the groundingRay
                    Vector3 dir = Quaternion.Euler(axis * (float)(i * (-Angle / _checks))) * _downDir;
                    Ray ray = new Ray(_RB.position, dir);
                    //Debug.DrawRay(ray.origin, ray.direction);

                    if (Physics.Raycast(ray, out testHitInfo, (_rideHeight * RideHeightMultiplier) + _rayOvershoot, _ground))
                    {
                        if (testHitInfo.distance < _rayHitInfo.distance && (testHitInfo.distance < _forwardHitInfo.distance || _forwardHitInfo.point == null))
                        {
                            _forwardHitInfo = testHitInfo;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
