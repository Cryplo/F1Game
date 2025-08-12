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
    private bool endEpisode = false;
    Vector2 startPosition;
    Quaternion startRotation;
    float lastDistanceToWaypoint = 0f;

    //true if lastDistanceToWaypoint variable is valid to use for finding a difference. Two conditions
    //One, lastDistanceToWaypoint has been set already (there has already been a frame where we have recorded lastDistanceToWaypoint)
    //Two, the waypoint used for this variable has not changed
    bool useLastDistanceToWaypoint = false;
    void Start()
    {
        startPosition = playerMovementScript.GetStartTransform().position;
        startRotation = playerMovementScript.GetStartTransform().rotation;
    }

    void FixedUpdate()
    {
        if (endEpisode) EndEpisode();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // collect linear car velocity
        sensor.AddObservation(playerMovementScript.GetCarLinearVelocity());

        //collect when at next waypoint (this should also solve the reverse direction issue)
        //calling IsAtWaypoint is necessary for playermovemnt script to function correctly for AI!!! (I think lol)
        bool isAtWaypoint = playerMovementScript.IsAtWaypoint();
        sensor.AddObservation(isAtWaypoint);
        if (isAtWaypoint)
        {
            AddReward(100f);
            useLastDistanceToWaypoint = false; //since waypoint has been advanced, then skip the calculating for this frame
        }
        //collect distance to next waypoint
        float distanceToNextWaypoint = playerMovementScript.DistanceToNextWaypoint();
        sensor.AddObservation(distanceToNextWaypoint);
        if (useLastDistanceToWaypoint)
        {
            //mathf.pow is used to incentivize faster speed
            //instead of going 2 one frame and 2 the next being equal to 4...
            //doing 4 in one frame has higher reward due to power
            if ((lastDistanceToWaypoint - distanceToNextWaypoint) < 0)
            {
                        AddReward(Mathf.Pow(Mathf.Abs(lastDistanceToWaypoint - distanceToNextWaypoint), 1.5f) * -20f);
                    }
            else
            {
                AddReward(Mathf.Pow(Mathf.Abs(lastDistanceToWaypoint - distanceToNextWaypoint), 1.5f) * 10f);
            }
            }
        lastDistanceToWaypoint = distanceToNextWaypoint;
        useLastDistanceToWaypoint = true;

        float degreesToNextWaypoint = playerMovementScript.DegreesToNextWaypoint();
        sensor.AddObservation(degreesToNextWaypoint);
        //AddReward((360 - Mathf.Abs(degreesToNextWaypoint)) * 0.01f);

        //collect if on track
        bool isOnTrack = playerMovementScript.IsOnTrack();
        sensor.AddObservation(isOnTrack);
        if (isOnTrack) AddReward(0f); //if I leave this in the episodes could take longer
        else
        {
            AddReward(0f);
            endEpisode = true;
        }
        //collect the raycast results
        List<float> raycastResults = playerMovementScript.GetRaycastResults();
        for (int i = 0; i < raycastResults.Count; i++)
        {
            sensor.AddObservation(raycastResults[i]);
        }

        //4 + 122 = 126 total observations
    }
    //actions are left/right arrow (or none) (three options)
    //forward/backward (or none) (three options)
    //space (or none) (two options)
    public override void OnActionReceived(ActionBuffers actions)
    {
        //horizontal input, vertical input, brake input (returns an int, so 1 will be true and 0 will be false)
        playerMovementScript.SetAIInputs(actions.DiscreteActions[0] - 1, actions.DiscreteActions[1] - 1, actions.DiscreteActions[2] == 1);
    }

    public override void OnEpisodeBegin()
    {
        playerMovementScript.ResetToStartTransform(startPosition, startRotation);
        endEpisode = false;
    }
}
