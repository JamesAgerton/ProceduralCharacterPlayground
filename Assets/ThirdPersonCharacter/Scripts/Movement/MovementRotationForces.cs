using UnityEngine;

namespace ProceduralCharacter.Movement
{
    [RequireComponent(typeof(MovementFloatRide), typeof(Rigidbody))]
    public class MovementRotationForces : MonoBehaviour
    {
        /// <summary>
        /// Maintain a specific rotation defined by _uprightRotation quaternion.
        /// </summary>
     
        #region Variables (PRIVATE)
        MovementFloatRide _MFR;
        MovementController _MC;
        Rigidbody _RB;


        [Header("Acceleration Tilt")]
        [SerializeField]
        bool _useVelocity = false;
        [SerializeField, Min(0), Tooltip("Scaled force relative to torque strength.")]
        float _velocityForceScale = 2f;
        [SerializeField, Tooltip("Gives turning tilts")]
        bool _useMovementControllerAcceleration = false;
        [SerializeField, Tooltip("Use values <1 for best results")]
        float _MCAccelForceScale = 0.1f;
        [SerializeField]
        float _maxAccelForce = 150f;

        [Header("Rotation (Turning & Uprightness)")]
        [SerializeField, Range(0, 10)]
        float _turnThreshold = 1f;

        [SerializeField]
        float _torqueStrength = 1000f;
        [SerializeField]
        float _torqueDamping = 100f;
        [SerializeField]
        Quaternion _uprightRotation = Quaternion.identity;
        #endregion

        #region Properties (PUBLIC)
        [Space]
        public bool RotationEnable = true;
        #endregion

        #region Unity Methods
        // Start is called before the first frame update
        private void Start()
        {
            _MFR = GetComponent<MovementFloatRide>();
            _MC = GetComponent<MovementController>();
            _RB = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (RotationEnable)
            {
                VelTurn();
                UpdateUprightForce();

                Vector3 acceleration = Vector3.zero;
                if (_useMovementControllerAcceleration && _MC != null)
                {
                    acceleration += _MC.Acceleration * (_MCAccelForceScale);
                }
                if (_useVelocity)
                {
                    acceleration += _RB.velocity * Time.fixedDeltaTime * _velocityForceScale;
                }

                AccelTilt(acceleration);
            }
        }
        #endregion

        #region Methods
        void VelTurn()
        {
            //TODO: this has to be faster, the turn is too slow when turning 180.
            Vector3 dir = GetRelativeVelocity();

            if (dir.magnitude > _turnThreshold)
            {
                _uprightRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }

            //Rotate based on rotation of the object under the player's feet
            if (_MFR.IsGrounded && _MFR.RayHitInfo.rigidbody != null)
            {
                Vector3 GroundAngleVel = _MFR.RayHitInfo.rigidbody.angularVelocity;
                Vector3 angleVelY = new Vector3(0f, GroundAngleVel.y * 1.15f, 0f);  //Magical number which somehow gives the correct rotation
                _uprightRotation = Quaternion.Euler(angleVelY) * _uprightRotation;
            }
        }

        public Vector3 GetRelativeVelocity()
        {
            Vector3 vel = new Vector3(_RB.velocity.x, 0f, _RB.velocity.z);
            Vector3 otherVel = Vector3.zero;
            if (_MFR.RayHitInfo.rigidbody != null)
            {
                otherVel = _MFR.RayHitInfo.rigidbody.GetPointVelocity(_MFR.RayHitInfo.point);
                otherVel.y = 0f;
            }
            Vector3 dir = vel - otherVel;

            if (dir.magnitude < _turnThreshold)
            {
                dir = Vector3.zero;
            }

            return dir;
        }

        void AccelTilt(Vector3 accel)
        {
            accel.y = 0;
            Vector3 tiltAxis = Vector3.Cross(Vector3.up, accel.normalized).normalized;

            float ClampedAccel = Mathf.Clamp(accel.magnitude * _RB.mass, 0f, _maxAccelForce);

            _RB.AddTorque(tiltAxis * ClampedAccel);
        }

        void UpdateUprightForce()
        {
            Quaternion current = transform.rotation;
            Quaternion toGoal = ShortestRotation(_uprightRotation, current);

            Vector3 rotAxis;
            float rotDegrees;

            toGoal.ToAngleAxis(out rotDegrees, out rotAxis);
            rotAxis.Normalize();

            float rotRadians = rotDegrees * Mathf.Deg2Rad;

            _RB.AddTorque(((rotAxis * rotRadians * _torqueStrength) - (_RB.angularVelocity * _torqueDamping)));
        }
        Quaternion ShortestRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                return a * Quaternion.Inverse(Multiply(b, -1));
            }
            else
            {
                return a * Quaternion.Inverse(b);
            }
        }
        Quaternion Multiply(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }
        #endregion
    }
}
