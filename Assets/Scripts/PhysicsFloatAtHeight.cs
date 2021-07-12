using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsFloatAtHeight : MonoBehaviour
{
    #region Variables (PRIVATE)
    [SerializeField]
    bool _useRay = false;
    [SerializeField]
    public LayerMask ground;
    [SerializeField]
    float _floatHeight = 1f;

    [Space]
    [SerializeField]
    float _springStrength = 5000f;
    [SerializeField]
    float _springDamping = 100f;
    [SerializeField]
    float _torqueStrength = 5000f;
    [SerializeField]
    float _torqueDamping = 100f;

    Rigidbody _body;
    Vector3 _pos;
    Quaternion _rot;
    #endregion

    #region Properties (PUBLIC)

    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    private void Start()
    {
        _body = GetComponent<Rigidbody>();
        _pos = transform.position;
        _rot = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (_useRay)
        {
            RayCastFloat();
        }
        else
        {
            PositionFloat();
        }

        UpdateUprightForce();
    }
    #endregion

    #region Methods
    void RayCastFloat()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hitinfo;
        if (Physics.Raycast(ray, out hitinfo, _floatHeight * 2f, ground))
        {
            Vector3 vel = _body.velocity;
            Vector3 rayDir = transform.TransformDirection(Vector3.down);

            Vector3 otherVel = Vector3.zero;
            Rigidbody hitBody = hitinfo.rigidbody;
            if (hitBody != null)
            {
                otherVel = hitBody.velocity;
            }

            float rayDirVel = Vector3.Dot(rayDir, vel);
            float otherDirVel = Vector3.Dot(rayDir, otherVel);

            float relVel = rayDirVel - otherDirVel;

            float x = hitinfo.distance - _floatHeight;

            float springForce = (x * _springStrength) - (relVel * _springDamping);

            _body.AddForce(ray.direction * springForce);
        }
    }

    void PositionFloat()
    {
        Vector3 offset = _pos - _body.position;

        Vector3 spring = (offset * _springStrength) - (_body.velocity * _springDamping);

        //Debug.DrawLine(transform.position, transform.position + spring);

        _body.AddForce(spring);
    }

    void UpdateUprightForce()
    {
        Quaternion current = transform.rotation;
        Quaternion toGoal = ShortestRotation(_rot, current);

        Vector3 rotAxis;
        float rotDegrees;

        toGoal.ToAngleAxis(out rotDegrees, out rotAxis);
        rotAxis.Normalize();

        float rotRadians = rotDegrees * Mathf.Deg2Rad;

        _body.AddTorque((rotAxis * (rotRadians * _torqueStrength)) - (_body.angularVelocity * _torqueDamping));
    }

    Quaternion ShortestRotation(Quaternion a, Quaternion b)
    {
        if(Quaternion.Dot(a,b) < 0)
        {
            return a * Quaternion.Inverse(Multiply(b, -1));
        }
        else
        {
            return a * Quaternion.Inverse(b);
        }
    }

    Quaternion Multiply (Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }
    #endregion
}
