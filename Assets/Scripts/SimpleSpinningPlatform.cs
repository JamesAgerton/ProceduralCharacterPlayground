using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class SimpleSpinningPlatform : MonoBehaviour
{
    private Rigidbody _body;
    private BoxCollider _collider;

    [SerializeField]
    private Vector3 _axisScale = Vector3.up;
    [SerializeField]
    private float _spinRate = 1f;

    float angle = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        angle += _spinRate * Time.fixedDeltaTime;
        Quaternion newRotation = Quaternion.Euler(_axisScale * angle);
        _body.MoveRotation(newRotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Parent other to this
        if (!other.transform.IsChildOf(this.transform))
        {
            other.transform.parent = this.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Unparent other from this
        if (other.transform.IsChildOf(this.transform))
        {
            other.transform.parent = null;
        }
    }
}
