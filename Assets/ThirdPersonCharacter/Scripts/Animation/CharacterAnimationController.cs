using UnityEngine;
using ProceduralCharacter.Movement;

[RequireComponent(typeof(Rigidbody), typeof(MovementController))]
public class CharacterAnimationController : MonoBehaviour
{
    #region Variables (PRIVATE)
    Rigidbody _RB;
    MovementController _MC;

    [SerializeField]
    float _accelScale = 800f;

    Vector3 _acceleration = Vector3.zero;
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
        _acceleration = _RB.velocity * Time.fixedDeltaTime;

        AccelTilt(_acceleration);
    }
    #endregion

    #region Methods
    void AccelTilt(Vector3 accel)
    {
        Vector3 tiltAxis = Vector3.Cross(Vector3.up, _acceleration.normalized).normalized;

        _RB.AddTorque(tiltAxis * _acceleration.magnitude * _accelScale);
    }
    #endregion
}
