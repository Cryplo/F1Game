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

    protected override void CollectObservations()
    {
        // collect linear car velocity
        sensor.AddObservation(playerMovementScript.GetCarLinearVelocity());

        //collect when at next waypoint (this should also solve the reverse direction issue)
        //calling IsAtWaypoint is necessary for playermovemnt script to function correctly for AI!!! (I think lol)
        bool isAtWaypoint = playerMovementScript.IsAtWaypoint();
        sensor.AddObservation(isAtWaypoint);
        addReward(1f);

        //collect distance to next waypoint
        float distanceToNextWaypoint = playerMovementScript.DistanceToNextWaypoint();
        sensor.AddObservation(distanceToNextWaypoint);
        //penalize for being far from the waypoint
        //this will probably have to be adjusted
        //i could also try 1/(distanceToNextWaypoint)
        addReward(-distanceToNextWaypoint * 0.01f);

        //collect if on track
        bool isOnTrack = playerMovementScript.IsOnTrack();
        sensor.AddObservation(isOnTrack);
        if (isOnTrack) addReward(0.1f);
        else addReward(-5f);

        //collect the raycast results
        List<float> raycastResults = playerMovementScript.GetRaycastResults();
        for (int i = 0; i < raycastResults.length(); i++)
        {
            sensor.AddObservation(raycastResults[i]);
        }

        //4 + 122 = 126 total observations
    }
    //actions are left/right arrow (or none) (three options)
    //forward/backward (or none) (three options)
    //space (or none) (two options)
    protected override void OnActionReceived(ActionBuffers actions)
    {
        //horizontal input, vertical input, brake input (returns an int, so 1 will be true and 0 will be false)
        playerMovementScript.SetAIInputs(actions.DiscreteActions[0], actions.DiscreteActions[1], actions.DiscreteActions[2] == 1);
    }
}
