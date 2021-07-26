using UnityEngine;
using ProceduralCharacter.Movement;

namespace ProceduralCharacter.Animation
{

    [RequireComponent(typeof(Rigidbody), typeof(MovementController))]
    public class CharacterAnimationController : MonoBehaviour
    {
        #region Variables (PRIVATE)
        Rigidbody _RB;
        MovementController _MC;
        MovementCrouch _MCrouch;
        MovementFloatRide _MFR;
        MovementRotationForces _MRF;

        [SerializeField]
        Animator _animator;
        [SerializeField]
        bool SlowTime = false;
        [SerializeField]
        float TimeScale = 0.1f;

        [Header("Stride Wheel")]
        [SerializeField]
        bool _drawStrideWheel = false;
        [SerializeField]
        private AnimationCurve _strideWeightCurve;
        [SerializeField]
        private AnimationCurve _strideSpeedCurve;
        [SerializeField]
        public AnimationCurve _strideBounceCurve;
        [SerializeField, Min(0)]
        float _minStrideRadius = 0f;
        [SerializeField, Min(0)]
        float _maxStrideRadius = 0f;

        [SerializeField]
        private float _bounceSmoothTime = 0.1f;
        [SerializeField]
        private float _bounceHeight = 0.1f;

        float _strideFraction = 0f; //LERP value along the stridewheel
        float _strideAngle = 0f;
        float _currentStrideRadius = 0f;
        float _strideCircumference = 0f;
        float _speedFraction = 0f;  //LERP value along stride radius
        float _refSpeedFractionVel = 0f;

        [Header("Jump")]
        [SerializeField]
        private float _jumpTransitionTime = 0.15f;
        [SerializeField]
        private float _jumpCrouchEffector = 0.1f;


        private Vector3 _currentBounceVelocity;

        float _currJumpFraction = 0.5f;
        float _jumpVelocity = 0f;
        float _targetJumpFraction = 0f;

        #endregion

        #region Properties (PUBLIC)

        #endregion

        #region Unity Methods
        // Start is called before the first frame update
        private void Start()
        {
            _RB = GetComponent<Rigidbody>();
            _MC = GetComponent<MovementController>();

            if(_animator == null)
            {
                Debug.LogError("Animator required for procedural character animation!", this);
                return;
            }

            if(GetComponent<MovementCrouch>() != null)
            {
                _MCrouch = GetComponent<MovementCrouch>();
            }

            if(GetComponent<MovementFloatRide>() != null)
            {
                _MFR = GetComponent<MovementFloatRide>();
            }

            if(GetComponent<MovementRotationForces>() != null)
            {
                _MRF = GetComponent<MovementRotationForces>();
            }
        }

        private void Update()
        {
            if (SlowTime)
            {
                Time.timeScale = TimeScale;
            }
            else
            {
                Time.timeScale = 1f;
            }
            HandlePose();
        }

        private void FixedUpdate()
        {
            CalculateStrideWheel();
        }

        private void OnDrawGizmos()
        {
            //Draw StrideWheel
            if (_drawStrideWheel)
            {
                Gizmos.color = Color.white;
                int size = 36;
                for (int i = 0; i < size; i++)
                {
                    float angle = i * (360f / (float)size) * Mathf.Deg2Rad;
                    float zcoord = _currentStrideRadius * Mathf.Cos(angle);
                    float ycoord = _currentStrideRadius * Mathf.Sin(angle);
                    float angle2 = (i + 1) * (360f / (float)size) * Mathf.Deg2Rad;
                    float zcoord2 = _currentStrideRadius * Mathf.Cos(angle2);
                    float ycoord2 = _currentStrideRadius * Mathf.Sin(angle2);

                    Gizmos.DrawLine(transform.TransformPoint(new Vector3(0f, ycoord, zcoord)),
                        transform.TransformPoint(new Vector3(0f, ycoord2, zcoord2)));
                }

                float mainDiv = 0.75f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = (_strideFraction + (i * 0.25f) % 1f) * 360f * Mathf.Deg2Rad;
                    float zCoord = _currentStrideRadius * Mathf.Cos(-angle);
                    float zinCoord = _currentStrideRadius * mainDiv * Mathf.Cos(-angle);
                    float yCoord = _currentStrideRadius * Mathf.Sin(-angle);
                    float yinCoord = _currentStrideRadius * mainDiv * Mathf.Sin(-angle);

                    Gizmos.DrawLine(transform.TransformPoint(new Vector3(0f, yCoord, zCoord)),
                        transform.TransformPoint(new Vector3(0f, yinCoord, zinCoord)));
                }

                float div = 0.5f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = (_strideFraction + (i * 0.25f + 0.125f) % 1f) * 360f * Mathf.Deg2Rad;
                    float zCoord = _currentStrideRadius * Mathf.Cos(-angle);
                    float zinCoord = _currentStrideRadius * div * Mathf.Cos(-angle);
                    float yCoord = _currentStrideRadius * Mathf.Sin(-angle);
                    float yinCoord = _currentStrideRadius * div * Mathf.Sin(-angle);

                    Gizmos.DrawLine(transform.TransformPoint(new Vector3(0f, yCoord, zCoord)),
                        transform.TransformPoint(new Vector3(0f, yinCoord, zinCoord)));
                }
            }
        }
        #endregion

        #region Methods
        private void CalculateStrideWheel()
        {
            //distance = speed * time;
            float sign = Vector3.Cross(_MRF.GetRelativeVelocity(), transform.right).normalized.y;
            float SpeedTarget = Mathf.Clamp(_MRF.GetRelativeVelocity().magnitude, 0f, _MC.MaxSpeed);

            _speedFraction = Mathf.SmoothDamp(_speedFraction, SpeedTarget / _MC.MaxSpeed, ref _refSpeedFractionVel, 0.1f);
            _currentStrideRadius = Mathf.Lerp(_minStrideRadius, _maxStrideRadius, _speedFraction);

            _strideCircumference = 2f * Mathf.PI * _currentStrideRadius;
            float Distance = _MRF.GetRelativeVelocity().magnitude * sign * (Time.fixedDeltaTime);
            float Angle = (Distance / _strideCircumference) * 360f;
            _strideAngle = (_strideAngle + Angle) % 180f;
            _strideFraction = _strideAngle / 360f;
        }

        private void HandlePose()
        {
            _animator.SetBool("IsMoving", _MC.IsMoving);
            if(_MFR != null)
            {
                _animator.SetBool("IsGrounded", _MFR.IsGrounded);
            }
            else
            {
                _animator.SetBool("IsGrounded", true);
            }

            //Walking Animations
            float frac = _strideWeightCurve.Evaluate(_strideFraction);
            float spd = _strideSpeedCurve.Evaluate(_speedFraction);
            _animator.SetFloat("StrideFraction", frac);
            _animator.SetFloat("StrideSpeed", spd);

            //Crouching Animation
            if(_MCrouch != null && _MFR != null)
            {
                frac = (_MFR.RideHeight - _MFR.RayHitInfo.distance) / _MFR.RideHeight / (1f - _MCrouch.CrouchRideMultiplier);
                //Debug.Log(frac);
            }
            else
            {
                frac = (_MFR.RideHeight - _MFR.RayHitInfo.distance) / _MFR.RideHeight;
            }
            _animator.SetFloat("CrouchFraction", frac);
        }
        #endregion
    }
}
