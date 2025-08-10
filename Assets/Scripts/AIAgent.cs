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
        // collect linear car velocity
        sensor.AddObservation(playerMovementScript.GetCarLinearVelocity());

        //collect when at next waypoint (this should also solve the reverse direction issue)
        //calling IsAtWaypoint is necessary for playermovemnt script to function correctly for AI!!! (I think lol)
        sensor.AddObservation(playerMovementScript.IsAtWaypoint());

        //collect distance to next waypoint
        sensor.AddObservation(playerMovementScript.DistanceToNextWaypoint());

        //collect if on track
        sensor.AddObservation(playerMovementScript.IsOnTrack());

        //collect the raycast results
        List<float> raycastResults = playerMovementScript.GetRaycastResults();
        for (int i = 0; i < raycastResults.length(); i++)
        {
            sensor.AddObservation(raycastResults[i]);
        }

    }
    //actions are left/right arrow (or none) (three options)
    //forward/backward (or none) (three options)
    //space (or none) (two options)
    
}
