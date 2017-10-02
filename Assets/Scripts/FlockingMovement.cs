using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingMovement : MonoBehaviour {

    [Header("Flocking info")]
    public float seperation_strength_const;
    public float center_strength_const;

    [Header("Seek information")]
    public float max_speed = 1.0f;
    public float acceleration = 1.0f;

    public float angular_force = 1.0f;
    public float max_angular_speed = 3.0f;

    [Header("General Movement information")]
    public float sin_frequency = 2.0f;
    public float sin_magnitude = 2.0f;

    Rigidbody2D rb;

    float m_lifetime = 0.0f;
    GameObject[] flock_members;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();

        // find all objects with the same tag as me
        flock_members = GameObject.FindGameObjectsWithTag(gameObject.tag);
	}
	
	
    // Update is called once per frame
	void FixedUpdate () {

        FlockMovement();

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
        //rb.AddForce(transform.right * acceleration);
        if (rb.velocity.magnitude > max_speed)
            rb.velocity = transform.right * max_speed;

        /*
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
        */

    }

    Vector2 CalcSeperation()
    {
        //Vector2 seperation_sum = Vector2.zero;
        float sep_str = 0.0f;
        Vector2 seperation_dir = Vector2.zero;
        foreach (GameObject obj in flock_members)
        {
            Vector2 diff = (gameObject.transform.position - obj.transform.position);
            float dist = diff.magnitude;
            // for now I am doing inverse square, linear is also fine
            if(dist > float.Epsilon)
                sep_str += seperation_strength_const / (dist * dist);

            seperation_dir += diff;
        }
        return sep_str * seperation_dir.normalized;
    }

    Vector2 MatchVelocity()
    {
        Vector2 velocity_sum = Vector2.zero;
        foreach (GameObject obj in flock_members)
        {
            // probably not great to get all these rb every frame
            Rigidbody2D obj_rb = GetComponent<Rigidbody2D>();
            velocity_sum += rb.velocity;
        }

        return velocity_sum / flock_members.Length;
    }

    Vector2 MoveCenterStrength()
    {
        Vector2 pos_sum = Vector2.zero;
        foreach (GameObject obj in flock_members)
        {
            pos_sum += (Vector2) obj.transform.position;
        }

        return pos_sum / flock_members.Length;
    }

    void FlockMovement()
    {
        // still need to match rotation

        // Seperation strength:
        Vector2 speration_strength = CalcSeperation();

        // match velocity
        Vector2 match_vel_strength = MatchVelocity();

        // flock to center
        Vector2 center_strength = MatchVelocity();

        // update my stats
        rb.AddForce(speration_strength + match_vel_strength + center_strength);
    }
}
