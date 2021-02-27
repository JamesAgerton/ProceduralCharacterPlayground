using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleSpinningPlatform : MonoBehaviour
{
    private Rigidbody _body;

    [SerializeField]
    private float _spinRate = 1f;

    float angle = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        angle += _spinRate * Time.deltaTime;
        Quaternion newRotation = Quaternion.Euler(0f, angle, 0f);
        _body.MoveRotation(newRotation);
    }
}
