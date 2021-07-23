using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class SimpleMovingPlatform : MonoBehaviour
{
    [SerializeField]
    public List<Vector3> waypoints = new List<Vector3>();
    [SerializeField]
    float maxSpeed = 5f;
    [SerializeField]
    float Limit = 0.1f;

    Rigidbody _rb;
    BoxCollider _collider;
    Vector3 velocity = Vector3.zero;
    int count = 0;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<BoxCollider>();
        if(waypoints.Count <= 0)
        {
            waypoints.Add(Vector3.zero);
        }
        transform.position = waypoints[0];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _rb.MovePosition(Vector3.SmoothDamp(transform.position, waypoints[count], ref velocity, Time.fixedDeltaTime, maxSpeed));
        if ((_rb.position - waypoints[count]).magnitude < Limit)
        {
            count++;
            if(count >= waypoints.Count)
            {
                count = 0;
            }
        }
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
