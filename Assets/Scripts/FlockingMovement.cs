using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingMovement : MonoBehaviour {

    [Header("Flocking info")]
    public float seperation_strength_const;
    public float center_strength_const;
    public bool is_leader = false;

    [Header("Seek information")]
    public float max_speed = 1.0f;
    public float acceleration = 1.0f;

    public float angular_force = 1.0f;
    public float max_angular_speed = 3.0f;

    [Header("General Movement information")]
    public float sin_frequency = 2.0f;
    public float sin_magnitude = 2.0f;
    public float turn_time = 0.5f;
    public float straight_time = 1.0f;

    [Header("Cone Collision info")]
    public float viewAngle = 30.0f;

    Rigidbody2D rb;

    float m_lifetime = 0.0f;
    GameObject[] flock_members;
    Rigidbody2D[] allRb;
    float cur_turn_time;
    float cur_straight_time = 0.0f;
    bool turn_down = true;


	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();

        // find all objects with the same tag as me
        flock_members = GameObject.FindGameObjectsWithTag(gameObject.tag);
        allRb = FindObjectsOfType(typeof(Rigidbody2D)) as Rigidbody2D[];

        cur_turn_time = turn_time / 2;
    }
	
	
    // Update is called once per frame
	void FixedUpdate () {
        if(!is_leader)
            FlockMovement();
        if (is_leader)
            MoveLeader();
    }

    void MoveLeader()
    {
        rb.AddForce(transform.right * acceleration);
        if (rb.velocity.magnitude > max_speed)
            rb.velocity = transform.right * max_speed;

        if(cur_turn_time < turn_time)
        {
            //add a torque
            if(turn_down)
            {
                rb.AddTorque(-1.0f * angular_force);
                if (rb.angularVelocity < -1.0f * max_angular_speed)
                    rb.angularVelocity = -1.0f * max_angular_speed;
                    
            }
            else
            {
                rb.AddTorque(angular_force);
                if (rb.angularVelocity > max_angular_speed)
                    rb.angularVelocity = max_angular_speed;
                    
            }

            cur_turn_time += Time.fixedDeltaTime;
        }
        else
        {
            if (cur_straight_time >= straight_time)
            {
                turn_down = !turn_down;
                cur_turn_time = 0.0f;
                cur_straight_time = 0.0f;
            }
            cur_straight_time += Time.fixedDeltaTime;
        }
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

        Vector2 center_point = pos_sum / flock_members.Length;
        return (center_point - (Vector2)gameObject.transform.position) * center_strength_const;
    }

    //Cone Check, returns velocity adjustment based on closest detected collision
    Vector2 CollisionCheck()
    {
        Vector2 adjust = Vector2.zero;

        //get the direction this unit is moving
        Vector2 heading = rb.velocity;
        heading.Normalize();
        //get the closest rigid Body in view
        Rigidbody2D closestCollision = null;
        float nextCol = Mathf.Infinity;
        foreach (Rigidbody2D otherRB in allRb)
        {
            if (otherRB != rb)
            {
                Vector2 dirOther = otherRB.position - rb.position;
                float distOther = dirOther.magnitude;

                if (distOther < nextCol)
                {
                    //direction to the other as a unit Vector
                    dirOther /= distOther;
                    float angle = Vector2.Angle(heading, dirOther);
                    if (angle < viewAngle && distOther < nextCol)
                    {
                        closestCollision = otherRB;
                        nextCol = distOther;
                    }
                }

            }
        }

        //a collision is predicted
        if (nextCol < Mathf.Infinity)
        {
            //some avoid maneuver
            print(this.name + " detects a collision with " + closestCollision.name);
        }
        return adjust;
    }

    void FlockMovement()
    {
        // still need to match rotation

        // Seperation strength:
        Vector2 speration_strength = CalcSeperation();

        // match velocity
        //Vector2 match_vel_strength = MatchVelocity();

        // flock to center
        Vector2 center_strength = MoveCenterStrength();

        CollisionCheck();

        // update my stats
        //rb.AddForce(speration_strength + match_vel_strength + center_strength);
        rb.AddForce(speration_strength + center_strength);
        //rb.AddForce(center_strength);
        if (rb.velocity.magnitude > max_speed)
        {
            rb.velocity = rb.velocity.normalized * max_speed;
        }
    }
}
