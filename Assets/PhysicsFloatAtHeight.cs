using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsFloatAtHeight : MonoBehaviour
{
    #region Variables (PRIVATE)
    [SerializeField]
    float _floatHeight = 1f;

    public PIDController altitudePID;

    Rigidbody _body;
    #endregion

    #region Properties (PUBLIC)

    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    private void Start()
    {
        _body = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hitinfo;
        if (Physics.Raycast(ray, out hitinfo, _floatHeight))
        {
            if(hitinfo.distance <= _floatHeight)
            {
                float current = hitinfo.distance;
                float err = _floatHeight - current;
                float proportionalHeight = altitudePID.Update(err);
                
                Vector3 appliedHoverForce = Vector3.up * proportionalHeight * 65f;
                _body.AddForce(appliedHoverForce, ForceMode.Acceleration);
            }
        }
    }
    #endregion

    #region Methods

    #endregion
}
