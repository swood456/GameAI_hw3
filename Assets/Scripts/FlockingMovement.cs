﻿using System.Collections;
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
    public float turn_time = 0.5f;
    public float straight_time = 1.0f;

    [Header("Collision info")]
    public float viewAngle = 30.0f;
    public float avoidanceConstant;
    float colRadius = 0.25f;

    Rigidbody2D rb;
    public LineRenderer sep_line;
    public LineRenderer vel_line;
    public LineRenderer center_line;

    //GameObject[] flock_members;
    Rigidbody2D[] flock_members;
    Rigidbody2D[] allRb;
    float cur_turn_time;
    float cur_straight_time = 0.0f;
    public bool turn_down = true;

    public Transform leader;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();

        // find all objects with the same tag as me
        GameObject[] flock_member_objs = GameObject.FindGameObjectsWithTag(gameObject.tag);
        flock_members = new Rigidbody2D[flock_member_objs.Length];
        for(int i = 0; i < flock_members.Length; ++i)
        {
            flock_members[i] = flock_member_objs[i].GetComponent<Rigidbody2D>();
        }
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
        foreach (Rigidbody2D obj in flock_members)
        {
            Vector2 diff = (gameObject.transform.position - obj.transform.position);
            float dist = diff.magnitude;
            // for now I am doing linear
            if(dist > float.Epsilon)
                sep_str += seperation_strength_const / (dist);

            seperation_dir += diff;
        }
        return sep_str * seperation_dir.normalized;
    }

    Vector2 MatchVelocity()
    {
        Vector2 velocity_sum = Vector2.zero;
        foreach (Rigidbody2D obj in flock_members)
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
        foreach (Rigidbody2D obj in flock_members)
        {
            pos_sum += (Vector2) obj.transform.position;
        }

        Vector2 center_point = pos_sum / flock_members.Length;
        return (center_point - (Vector2)gameObject.transform.position) * center_strength_const;
    }

    //Cone Check, returns velocity adjustment based on closest detected collision
    Vector2 ConeCheck()
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

    Vector2 CollisionCheck()
    {
        Vector2 adjust = Vector2.zero;

        float shortestTime = Mathf.Infinity;
        Rigidbody2D firstTarget = null;
        float firstMinSeperation = Mathf.Infinity;
        float firstDistance = Mathf.Infinity;
        Vector2 firstRelativePos = Vector2.positiveInfinity;
        Vector2 firstRelativeVel = Vector2.positiveInfinity;

        foreach (Rigidbody2D target in allRb)
        {
            //calculate time to collision
            Vector2 relativePos = target.position - rb.position;
            Vector2 relativeVel = target.velocity - rb.velocity;
            float relativeSpeed = relativeVel.magnitude;
            float tClosest = -Vector2.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);

            //check if a collision exists
            float distance = relativePos.magnitude;
            float minSeperation = distance - relativeSpeed * tClosest;
            if (minSeperation > colRadius*2)
            {
                continue;
            }

            //Check if this is the closest projected collision
            if (tClosest > 0 && tClosest < shortestTime)
            {
                shortestTime = tClosest;
                firstTarget = target;
                firstMinSeperation = minSeperation;
                firstDistance = distance;
                firstRelativePos = relativePos;
                firstRelativeVel = relativeVel;
            }
        }

        if (firstTarget == null)
        {
            return adjust;
        }

        Vector2 relPos;

        //the collision ispredicted to be head on or is already happening
        if (firstMinSeperation <= 0 || firstDistance < 2* colRadius)
        {
            relPos = firstTarget.position - rb.position;
        }
        else
        {
            relPos = firstRelativePos + firstRelativeVel * shortestTime;
        }

        relPos.Normalize();

        adjust = relPos * acceleration * avoidanceConstant;

        print(this.name + " detects a collision with " +firstTarget.name);
        return adjust;

    }

    float averageRotation()
    {
        float rot_sum = 0.0f;
        foreach(Rigidbody2D g in flock_members)
        {
            rot_sum += g.GetComponent<Rigidbody2D>().rotation;
        }
        return rot_sum / flock_members.Length;
    }

    void FlockMovement()
    {
        // still need to match rotation

        // Seperation strength:
        Vector2 speration_strength = CalcSeperation();
        if(sep_line)
        {
            sep_line.SetPosition(0, transform.position);
            sep_line.SetPosition(1, transform.position + (Vector3)speration_strength * 0.0125f * seperation_strength_const);
        }
        

        // match velocity
        Vector2 match_vel_strength = MatchVelocity();
        if(vel_line)
        {
            vel_line.SetPosition(0, transform.position);
            vel_line.SetPosition(1, transform.position + (Vector3)match_vel_strength * 0.25f);
        }

        // flock to center
        Vector2 center_strength = MoveCenterStrength();
        if (center_line)
        {
            center_line.SetPosition(0, transform.position);
            center_line.SetPosition(1, transform.position + (Vector3)center_strength * 0.0125f * center_strength_const);
        }

        //collision prediction
        Vector2 collision = CollisionCheck();

        print(speration_strength.magnitude + " " + match_vel_strength.magnitude + " " + center_strength.magnitude + " " + collision.magnitude);

        // update my stats
        rb.AddForce(speration_strength + match_vel_strength + center_strength + collision);

        // average out rotation
        float avg_rot = averageRotation();
        if(avg_rot < rb.rotation)
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
        
        rb.AddForce(acceleration * transform.right);
        
        if (rb.velocity.magnitude > max_speed)
        {
            rb.velocity = rb.velocity.normalized * max_speed;
        }

        Debug.DrawRay(rb.position, rb.velocity);
        
    }
}
