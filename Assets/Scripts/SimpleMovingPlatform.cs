using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleMovingPlatform : MonoBehaviour
{
    [SerializeField]
    public List<Vector3> waypoints = new List<Vector3>();
    [SerializeField]
    float maxSpeed = 5f;

    Rigidbody rb;
    Vector3 velocity = Vector3.zero;
    int count = 0;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if(waypoints.Count <= 0)
        {
            waypoints.Add(Vector3.zero);
        }
        transform.position = waypoints[0];
    }

    // Update is called once per frame
    void Update()
    {
        rb.MovePosition(Vector3.SmoothDamp(transform.position, waypoints[count], ref velocity, Time.deltaTime, maxSpeed));
        if ((rb.position - waypoints[count]).magnitude < 0.5f)
        {
            count++;
            if(count >= waypoints.Count)
            {
                count = 0;
            }
        }
    }
}
