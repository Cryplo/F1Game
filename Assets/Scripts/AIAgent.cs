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
    float lastTimeOffTrack = 0f;

    //true if lastDistanceToWaypoint variable is valid to use for finding a difference. Two conditions
    //One, lastDistanceToWaypoint has been set already (there has already been a frame where we have recorded lastDistanceToWaypoint)
    //Two, the waypoint used for this variable has not changed
    bool useLastDistanceToWaypoint = false;
    bool firstTimeOffTrack = true;
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
        //AddReward(playerMovementScript.GetCarLinearVelocity() * 3f);

        //collect when at next waypoint (this should also solve the reverse direction issue)
        //calling IsAtWaypoint is necessary for playermovemnt script to function correctly for AI!!! (I think lol)
        bool isAtWaypoint = playerMovementScript.IsAtWaypoint();
        sensor.AddObservation(isAtWaypoint);
        if (isAtWaypoint)
        {
            //asume maybe 5 seconds to reach next waypoint and scale accordingly
            //square to favor faster speeds even more
            //AddReward(Mathf.Pow((10f - (Time.realtimeSinceStartup - lastTime)) * 0.1f, 2f));
            //AddReward(1.0f);
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
                        //AddReward(Mathf.Pow(Mathf.Abs(lastDistanceToWaypoint - distanceToNextWaypoint), 1.2f) * -1f);
                    }
            else
            {
                //250 reward / 4000 steps = 0.0625 reward per step for this add reward very approximately
                //once AI gets close then don't try to reward it for going close to the waypoint but let it take the corner
                //how it wants
                AddReward(Mathf.Pow(Mathf.Abs(lastDistanceToWaypoint - distanceToNextWaypoint), 1.1f) * 0.5f);
            }
        }
        lastDistanceToWaypoint = distanceToNextWaypoint;
        useLastDistanceToWaypoint = true;

        float degreesToNextWaypoint = playerMovementScript.DegreesToNextWaypoint();
        sensor.AddObservation(degreesToNextWaypoint); //unnormalize for models including and before second_id_7_second_checkpoint?
        //AddReward((180 - Mathf.Abs(degreesToNextWaypoint)) * 10000000f);

        //collect if on track
        bool isOnTrack = playerMovementScript.IsOnTrack();
        sensor.AddObservation(isOnTrack);
        if (isOnTrack)
        {
            AddReward(0.01f); //if I leave this in the episodes could take longer
            firstTimeOffTrack = true;
        }
        else
        {
            AddReward(-0.8f);
            //car linear velocity may be in the range from 5-25?
            //AddReward(-playerMovementScript.GetCarLinearVelocity() * 0.1f);
            if (firstTimeOffTrack)
            {
                firstTimeOffTrack = false;
                lastTimeOffTrack = Time.realtimeSinceStartup;
            }
            else
            {
                if (Time.realtimeSinceStartup - lastTimeOffTrack > 5.0f) endEpisode = true;
            }
        }
        //collect the raycast results
        List<float> raycastResults = playerMovementScript.GetRaycastResults();
        for (int i = 0; i < raycastResults.Count; i++)
        {
            sensor.AddObservation(raycastResults[i]);
        }

        //4 + 19 * 2 = 42 total observations
    }
    //actions are left/right arrow (or none) (three options)
    //forward/backward (or none) (three options)
    //space (or none) (two options)
    public override void OnActionReceived(ActionBuffers actions)
    {
        //if(actions.DiscreteActions[1] - 1 == -1) AddReward(-10000f);
        //horizontal input, vertical input, brake input (returns an int, so 1 will be true and 0 will be false)
        playerMovementScript.SetAIInputs(actions.DiscreteActions[0] - 1, actions.DiscreteActions[1] - 1, actions.DiscreteActions[2] == 1);
    }

    public override void OnEpisodeBegin()
    {
        playerMovementScript.ResetToStartTransform(startPosition, startRotation);
        endEpisode = false;
    }
}
