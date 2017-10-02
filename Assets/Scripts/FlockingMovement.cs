using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingMovement : MonoBehaviour {

    [Header("Seek information")]
    public float max_speed = 1.0f;
    public float acceleration = 1.0f;

    public float angular_force = 1.0f;
    public float max_angular_speed = 3.0f;

    [Header("General Movement information")]
    public float sin_frequency = 2.0f;
    public float sin_magnitude = 2.0f;

    Rigidbody2D rb;
    public Vector2 dest;
    float m_lifetime = 0.0f;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
	}
	
	
    // Update is called once per frame
	void FixedUpdate () {

        /*
         * // manually move the agent in a sine curve
        dest = transform.position;
        dest.y = Mathf.Sin(sin_frequency * m_lifetime) * sin_magnitude;
        dest.x += max_speed * Time.deltaTime;
        transform.position = dest;

        m_lifetime += Time.deltaTime;
        */

        //seek
        
        // for now always go at max speed
        rb.AddForce(transform.right * acceleration);
        if (rb.velocity.magnitude > max_speed)
            rb.velocity = transform.right * max_speed;

        float m_dot = Vector2.Dot(transform.up, (dest - (Vector2)transform.position).normalized);
        if (Mathf.Abs(m_dot) > 0.01f)
        {
            if(m_dot > 0)
            {
                rb.AddTorque(angular_force);
                if (rb.angularVelocity > max_angular_speed)
                    rb.angularVelocity = max_angular_speed;
            }
            else
            {
                rb.AddTorque(-1.0f * angular_force);
                if (rb.angularVelocity < -1.0f * max_angular_speed)
                    rb.angularVelocity = -1.0f * max_angular_speed;
            }
            
            
        }

    }
}
