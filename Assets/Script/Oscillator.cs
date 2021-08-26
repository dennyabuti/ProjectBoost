using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
    
    Vector3 startingPosition;
    [SerializeField] Vector3 movementVector;
    float movementFactor;
    [SerializeField] float period = 2f;

    public float speed;
    public float Speed { get { return speed;}}
    
    Rigidbody rb;

    void Awake()
    {
        rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = 400;
    }
    void Start()
    {
       startingPosition = transform.position;
    }

    void Update()
    {
        // var velocity = transform.TransformDirection(rb.velocity);
        // Debug.Log(velocity.x + "   " + transform.position.x + " " + transform.right );

        float speed;
        
        if (period <= Mathf.Epsilon) return;

        const float tau = Mathf.PI * 2; 
        float cycles = Time.time / period; // continually growing over time;
        float rawSinWave = Mathf.Sin(cycles * tau);

        movementFactor = (rawSinWave + 1f) / 2f; // go from 0 - 1

        Vector3 offset = movementVector * movementFactor;

        var nextPosition = startingPosition + offset;

        speed = (nextPosition.x - transform.position.x) / Time.deltaTime;
        transform.position = nextPosition;
    }
}
