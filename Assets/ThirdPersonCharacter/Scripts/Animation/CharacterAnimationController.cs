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

        //[Header("Acceleration Tilt")]
        //[SerializeField]
        //float _accelScale = 800f;

        //Vector3 _acceleration = Vector3.zero;

        //[Header("Rotation (Turning & Uprightness)")]
        //[SerializeField]
        //float _turnThreshold = 1f;

        //[SerializeField]
        //float _torqueStrength = 1000f;
        //[SerializeField]
        //float _torqueDamping = 100f;
        //[SerializeField]
        //Quaternion _uprightRotation = Quaternion.identity;

        [SerializeField]
        Animator _animator;

        [Header("Stride Wheel")]
        [SerializeField]
        private AnimationCurve _strideWeightCurve;
        [SerializeField]
        private AnimationCurve _strideSpeedCurve;
        [SerializeField]
        public AnimationCurve _strideBounceCurve;
        [SerializeField]
        private float _bounceSmoothTime = 0.1f;
        [SerializeField]
        private float _bounceHeight = 0.1f;

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
        }

        private void Update()
        {

        }

        private void FixedUpdate()
        {

        }

        private void OnDrawGizmos()
        {

        }
        #endregion

        #region Methods

        #endregion
    }
}
