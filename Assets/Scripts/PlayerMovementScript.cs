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

public class PlayerMovementScript : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] float driftFactor;
    [SerializeField] float accelerationFactor;
    [SerializeField] float turnFactor;
    [SerializeField] float maxSpeed;
    [SerializeField] float offTrackFactor;
    [SerializeField] float dragFactor;
    [SerializeField] float brakeFactor;
    [SerializeField] Rigidbody2D myRigidbody2D;

    [Header("AI Settings")]
    [SerializeField] bool isAI;
    [SerializeField] WaypointScript nextWaypoint;

    float accelerationInput = 0;
    float steeringInput = 0;
    float rotationAngle = 0;
    float velocityVsUp = 0;
    float randomGen = 0;
    float aggression = 0;
    float randomTurn = 1;
    bool onTrack = true;
    float tightness = 0f;
    float turnAmount = 0f;
    bool wrongWay = false;
    bool cautious = true;
    bool onEdge = false;
    Vector2 pastPosition = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {

        //if(isAI)accelerationFactor += Random.value * 0.5f - 0.25f;
        aggression = (Random.value * 4 - 2);
        //aggression = 0;
        //tightness = Random.value * 0.2f + 0.5f;
        pastPosition = gameObject.transform.position;
        maxSpeed += (Random.value * 2) - 1;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        Debug.Log(gameObject.name + " " + onTrack);
        updateInputVector();
        ApplyEngineForce();
        KillOrthogonalVelocity();
        ApplySteering();
    }

    Vector2 Rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    Vector2 returnToTrack()
    {
        //myRigidbody2D.velocity = new Vector2(0, 0);
        //return new Vector2(0, 0);
        Vector2 input = Vector2.zero;
        input.y = 1;

        Vector2 end = nextWaypoint.gameObject.transform.position;
        Vector2 origin = pastPosition;
        Vector2 point = gameObject.transform.position;

        //Get heading
        Vector2 heading = (end - origin);
        float magnitudeMax = heading.magnitude;
        heading.Normalize();

        //Do projection from the point but clamp it
        Vector2 lhs = point - origin;
        float dotP = Vector2.Dot(lhs, heading);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        Vector2 closestPosition = origin + heading * dotP;


        Vector2 target = (closestPosition - (Vector2)gameObject.transform.position).normalized * 1.5f + ((Vector2)nextWaypoint.gameObject.transform.position - (Vector2)gameObject.transform.position).normalized;
        target.Normalize();
        Debug.DrawRay(gameObject.transform.position, target);
        float angleToTarget = Vector2.SignedAngle(transform.up, target);
        angleToTarget *= -1;
        input.x = Mathf.Sign(angleToTarget);
        return input;
    }

    void wrongWayCheck(out bool wrongWayHolder, out Vector2 input)
    {
        input = new Vector2(0, 1f);
        Vector2 target = nextWaypoint.gameObject.transform.position - gameObject.transform.position;
        float angleToTarget = Vector2.SignedAngle(transform.up, target);
        angleToTarget *= -1;
        if (!wrongWay)
        {
            if (Mathf.Abs(angleToTarget) > 120)
            {
                Debug.DrawRay(gameObject.transform.position, target, UnityEngine.Color.green);
                input.x = Mathf.Sign(angleToTarget);
                wrongWayHolder = true;
                return;
            }
            wrongWayHolder = false;
        }
        else
        {
            if (Mathf.Abs(angleToTarget) > 60)
            {
                Debug.DrawRay(gameObject.transform.position, target, UnityEngine.Color.green);
                input.x = Mathf.Sign(angleToTarget);
                wrongWayHolder = true;
                return;
            }
            wrongWayHolder = false;
        }
    }


    /*
    //original code
    //https://stackoverflow.com/questions/51905268/how-to-find-closest-point-on-line
    Vector2 calculateInput()
    {
        Vector2 input = Vector2.zero;
        bool wrongWayHolder = false;
        wrongWayCheck(out wrongWayHolder, out input);
        if (wrongWayHolder)
        {
            wrongWay = true;
            return input;
        }
        wrongWay = false;
        input = Vector2.zero;
        if (!onTrack)
        {
            input = returnToTrack();
            return input;
        }
        float distance = maxSpeed * 2; //https://answers.unity.com/questions/555056/can-raycast-get-multiple-points-from-the-same-coll-2.html
        bool overtake = false;
        float carRelVelocity = -1;
        float frontRealDist = maxSpeed * 2;

        //new addition
        Vector2 target = nextWaypoint.gameObject.transform.position - gameObject.transform.position;
        float angleToTarget = Vector2.SignedAngle(transform.up, target);
        angleToTarget *= -1;
        Debug.Log(angleToTarget);
        //input.x = Mathf.Sign(angleToTarget) * 12;
        //input.x = Mathf.Sign(angleToTarget) * 7 * Mathf.Clamp(Mathf.Abs(angleToTarget), 0, 1);
        input.x = Mathf.Sign(angleToTarget) * 25 * Mathf.Clamp(Mathf.Abs(angleToTarget) / 4f, 0, 1);
        //input.x = Mathf.Sign(angleToTarget) * 7 * Mathf.Abs(angleToTarget) / 90f;
        //


        for (int i = -90; i <= 90; i += 3)
        {
            //distance = 4 * Mathf.Cos(i * Mathf.Deg2Rad);
            Vector2 direction = Rotate(transform.up, i);
            //Vector2 pos = transform.TransformPoint(new Vector2(0, -0.4f));
            Vector2 position = (Vector2)transform.position;
            //Vector2 position = pos;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, direction, distance, LayerMask.GetMask("AIAvoid"));
            //Debug.DrawRay(position, direction * distance, UnityEngine.Color.green);
            float realDist;
            float carFactor = 1;

            if (hits.Length > 1)
            {
                realDist = hits[1].distance;
                if (i == 0)
                {
                    for (int j = 1; j < hits.Length; j++)
                    {
                        if (hits[j].collider.name == "TrackCollider")
                        {
                            realDist = hits[j].distance;
                            frontRealDist = realDist;
                            break;
                        }
                    }
                }
                if (Mathf.Abs(i) <= 5)
                {
                    for (int j = 1; j < hits.Length; j++)
                    {
                        if (hits[j].collider.name != "TrackCollider")
                        {
                            if (hits[j].distance < 10)
                            {
                                overtake = true;
                                Debug.DrawRay(position, (Vector2)hits[j].collider.transform.position - position, UnityEngine.Color.green);
                            }
                        }
                    }

                }
                if (hits[1].collider.name != "TrackCollider")
                {
                    carFactor = 1.5f;
                    if (i == 0)
                    {
                        Vector3 relative = hits[1].collider.GetComponent<Rigidbody2D>().linearVelocity - myRigidbody2D.linearVelocity;
                        if (relative.sqrMagnitude - 2 * (accelerationFactor * brakeFactor * Mathf.Abs(relative.magnitude) * hits[1].distance) > 0)
                        {
                            carRelVelocity = relative.magnitude;
                        }
                    }

                }
            }
            else
            {
                realDist = distance;
            }
            //Debug.DrawRay(position, direction * realDist, Color.red);

            float modifiedDist = 1 / Mathf.Pow(realDist, 1f / 2f); //good range between 0.4 and 0.6
            //float modifiedDist = 1 / Mathf.Pow(realDist, 1);
            //float moveForwardDist = (realDist - (distance) + 1);

            //turn type 1
            if (carFactor != 1) input += new Vector2(Mathf.Sign(i) * modifiedDist * carFactor, 0);

            //turn type 2
            //input += new Vector2((Mathf.Sign(i) + Mathf.Sin(Mathf.Deg2Rad * i)) * modifiedDist * carFactor, 0);

            //turn type 3
            //input += new Vector2(Mathf.Sin(Mathf.Deg2Rad * i) * modifiedDist * carFactor, 0);
            if (Mathf.Abs(i) == 0)
            {
                float corneringSpeed = Mathf.Max(maxSpeed + aggression - myRigidbody2D.linearVelocity.magnitude - 18, 0);
                if (cautious) corneringSpeed = 0;
                //float corneringSpeed = aggression;
                //corneringSpeed = 0;
                if (myRigidbody2D.linearVelocity.sqrMagnitude - 2 * (accelerationFactor * brakeFactor * Mathf.Abs(myRigidbody2D.linearVelocity.magnitude)) * frontRealDist > Mathf.Pow(corneringSpeed, 2))
                {
                    input.y = -1;
                }
                else
                {
                    input.y = 1;
                }
            }
        }
        input.y = Mathf.Sign(input.y);
        if (input.y == -1)
        {
            input.y = -1 * brakeFactor * Mathf.Abs(myRigidbody2D.linearVelocity.magnitude);
        }
        if (input.y == 1)
        {
            if (overtake && !cautious)
            {
                input.y += 0.5f;
                Debug.Log("Overtake!!!");
            }
            if (carRelVelocity != -1)
            {
                if (frontRealDist < 8 && !cautious)
                {
                    input.y = -1 * brakeFactor * Mathf.Abs(myRigidbody2D.linearVelocity.magnitude);
                    //input.y = -carRelVelocity / maxSpeed;
                }
                else if (frontRealDist < 20 && cautious)
                {
                    input.y = -1 * brakeFactor * Mathf.Abs(myRigidbody2D.linearVelocity.magnitude);
                }
            }
        }
        //if (Mathf.Abs(input.x) < 0.5) input.x = 0;
        //else input.x = Mathf.Sign(input.x);
        if (Mathf.Abs(input.x) > 1) input.x = Mathf.Sign(input.x);

        if (Mathf.Abs(input.x) < 0.3) input.x = 0;
        //map steering factor
        else
        {
            float value = Mathf.Abs(input.x);
            float normal = Mathf.InverseLerp(0.3f, 1, value);
            float bValue = Mathf.Lerp(0, 1, normal);
            input.x = Mathf.Sign(input.x) * bValue;
        }

        if (Mathf.Abs(input.x) <= 0.01)
        {
            randomTurn = (Random.value / 5) + 0.9f;
            aggression = (Random.value * 6 - 4);
            //aggression = 0;
        }
        else
        {
            input.x *= randomTurn;
        }

        return input;
    }*/

    void updateInputVector()
    {
        //CHANGE SOMETIME
        Vector2 inputVector = Vector2.zero;
        if (!isAI)
        {
            inputVector.x = UnityEngine.Input.GetAxis("Horizontal");
            inputVector.y = UnityEngine.Input.GetAxis("Vertical");
            if (UnityEngine.Input.GetKey(KeyCode.Space))
            {
                inputVector.y = -1 * brakeFactor * Mathf.Abs(myRigidbody2D.linearVelocity.magnitude);
                //inputVector.y = -1 * brakeFactor;
            }
        }
        //this is where I would implement AI
        else
        {
            inputVector = GetAIInput();
        }
        steeringInput = inputVector.x;
        //if (!isAI) {
        if (turnAmount < 0 && inputVector.x > 0)
        {
            steeringInput = 0;
        }
        if (turnAmount > 0 && inputVector.x < 0)
        {
            steeringInput = 0;
        }
        //steeringInput += Mathf.Sign(inputVector.x); //* Time.deltaTime;
        if (Mathf.Abs(steeringInput) > 1) steeringInput = Mathf.Sign(steeringInput);
        //}

        //detect slip stream
        if (!isAI)
        {
            float distance = maxSpeed; //this is inconsistent with the other raycasting
            Vector2 direction = Rotate(transform.up, 0);
            Vector2 position = transform.position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, direction, distance, LayerMask.GetMask("AIAvoid"));
            if (hits.Length > 1)
            {
                if (hits[1].collider.name != "TrackCollider" && hits[1].distance < 10)
                {
                    if (inputVector.y > 0) inputVector.y += 0.5f;
                }
            }
        }

        accelerationInput = inputVector.y;
        steeringInput = inputVector.x;
        //transform.GetChild(0).GetChild(3).transform.localRotation = Quaternion.Euler(0, 180, -45 * inputVector.x);
        //transform.GetChild(0).GetChild(4).transform.localRotation = Quaternion.Euler(0, 0, -45 * inputVector.x);

    }

    void ApplyEngineForce()
    {
        velocityVsUp = Vector2.Dot(transform.up, myRigidbody2D.linearVelocity);
        if (velocityVsUp > maxSpeed && accelerationInput > 0)
        {
            return;
        }
        if (velocityVsUp < -maxSpeed && accelerationInput < 0)
        {
            return;
        }
        if (myRigidbody2D.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed && accelerationInput > 0)
        {
            return;
        }
        Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;

        myRigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering()
    {
        float minSpeedBeforeAllowTurningFactor = myRigidbody2D.linearVelocity.magnitude / 8;
        minSpeedBeforeAllowTurningFactor = Mathf.Clamp01(minSpeedBeforeAllowTurningFactor);
        float change = steeringInput * turnFactor;
        rotationAngle = myRigidbody2D.rotation - change;

        myRigidbody2D.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(myRigidbody2D.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(myRigidbody2D.linearVelocity, transform.right);
        myRigidbody2D.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        myRigidbody2D.linearDamping = offTrackFactor;
        if (collision.name == "RaceTrack")
        {
            onTrack = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (accelerationInput == 0)
        {
            myRigidbody2D.linearDamping = dragFactor;
        }
        else
        {
            myRigidbody2D.linearDamping = 0;
        }
        if (collision.name == "RaceTrack")
        {
            onTrack = true;
        }
        if (collision.name == "TrackCollider")
        {
            onTrack = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Square")
        {
            if (isAI) accelerationFactor += Random.value * 0.5f - 0.25f;
        }
    }

    public float GetCarLinearVelocity()
    {
        return myRigidbody2D.linearVelocity.magnitude;
    }

    //this is intended for the AIAgent
    public bool IsAtWaypoint()
    {
        bool advanceWaypoint = (nextWaypoint.gameObject.transform.position - gameObject.transform.position).magnitude <= 7;

        if (advanceWaypoint)
        {
            pastPosition = nextWaypoint.gameObject.transform.position;
            nextWaypoint = nextWaypoint.GetComponent<WaypointScript>().getNext();
            if (cautious) cautious = false;
        }

        return advanceWaypoint;
    }

    public float DistanceToNextWaypoint()
    {
        return (nextWaypoint.gameObject.transform.position - gameObject.transform.position).magnitude;
    }

    public bool IsOnTrack()
    {
        return onTrack;
    }

    //61 * 2 = 122 items in list
    // -90 car distance, -90 wall distance, -87 car distance, -87 wall distance, etc....
    // "distance" will be a percentange of max distance
    public List<float> GetRaycastResults()
    {
        float raycastDistance = maxSpeed * 2;
        List<float> raycastResults = new List<float>();
        for (int i = -90; i <= 90; i += 3)
        {
            Vector2 direction = Rotate(transform.up, i);
            Vector2 position = (Vector2)transform.position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, direction, raycastDistance, LayerMask.GetMask("AIAvoid"));
            //Debug.DrawRay(position, direction * distance, UnityEngine.Color.green);
            //float realDist;
            //float carFactor = 1;
            // the first hit will be itself, so ignore that
            float percentDistanceToNearestCar = 1;
            float percentDistanceToNearestTrack = 1;
            if (hits.Length > 1)
            {
                //start at 1 to avoid the first hit (which is itself)
                for (int j = 1; j < hits.Length; j++)
                {
                    //either a car
                    if (hits[j].collider.name != "TrackCollider")
                    {
                        percentDistanceToNearestCar = Mathf.Min(hits[j].distance / distance, percentDistanceToNearestCar);
                    }
                    // or a track
                    else
                    {
                        percentDistanceToNearestTrack = Mathf.Min(hits[j].distance / distance, percentDistanceToNearestTrack);
                    }
                }
            }
            raycastResults.Add(percentDistanceToNearestCar);
            raycastResults.Add(percentDistanceToNearestTrack);
        }
    }

    private Vector2 GetAIInput()
    {
        
    }
}