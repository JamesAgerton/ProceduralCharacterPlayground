using UnityEngine;
using ProceduralCharacter.Movement;

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

    [Header("Stride Wheel")]
    [SerializeField]
    private AnimationCurve _strideWeightCurve;
    [SerializeField]
    private AnimationCurve _strideSpeedCurve;

    #endregion

    #region Properties (PUBLIC)

    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    private void Start()
    {
        _RB = GetComponent<Rigidbody>();
        _MC = GetComponent<MovementController>();
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
