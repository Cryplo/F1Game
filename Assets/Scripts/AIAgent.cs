//using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

//acts as the brain of the AI to interact with playermovement script
public class AIAgentScript : Agent
{
    [SerializeField] PlayerMovementScript playerMovementScript;
    void Start()
    {

    }

    void FixedUpdate()
    {

    }

    void CollectObservations()
    {
        // collect car velocity
        sensor.AddObservation(playerMovementScript.GetCarLinearVelocity());

        //collect when at next waypoint (this should also solve the reverse direction issue)
        //calling IsAtWaypoint is necessary for playermovemnt script to function correctly for AI!!! (I think lol)
        sensor.AddObservation(playerMovementScript.IsAtWaypoint());

        //collect distance to next waypoint
        sensor.AddObservation(playerMovementScript.DistanceToNextWaypoint());

        // collect ray cast
        //something along the lines like this, with 1 being optimal (furthest distance away)
        /*
        // Detect wall
        if (Physics.Raycast(transform.position, dir, out RaycastHit hitWall, maxDistance, wallMask))
            sensor.AddObservation(hitWall.distance / maxDistance);
        else
            sensor.AddObservation(1f);

        // Detect car
        if (Physics.Raycast(transform.position, dir, out RaycastHit hitCar, maxDistance, carMask))
            sensor.AddObservation(hitCar.distance / maxDistance);
        else
            sensor.AddObservation(1f);
        */
    }
}
