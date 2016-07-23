using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController
{

    public LayerMask passengerMask;//what can ride the platform

    public Vector3[] localWaypoints;//allows setting of local waypoints to direct where the platform goes(editor accesible)
    Vector3[] globalWaypoints;//private accesor for functions

    public float speed;//how fast can the platform move(editor accesible)
    public bool cyclic;//back and forth or in a circle(editor accesible)
    public float waitTime;//how long platform stops at each checkpoint(editor accesible)
    [Range(0, 2)]
    public float easeAmount;//smooths or bends the transition(editor accesible)

    int fromWaypointIndex;//counter to determine what platform to move to next
    float percentBetweenWaypoints;//percent traveled from one waypoint to another
    float nextMoveTime;//when to start moving

    List<PassengerMovement> passengerMovement;//array based container to hold passengers on the platform
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();//array based container to move passengers with the platform

    //use this for initialization
    public override void Start()
    {
        base.Start();//raycast goes firstv 

        globalWaypoints = new Vector3[localWaypoints.Length];//sets waypoints
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;//fixes waypoint values for function use
        }
    }
    //called once per frame
    void Update()
    {

        UpdateRaycastOrigins();//function call

        Vector3 velocity = CalculatePlatformMovement();//function call

        CalculatePassengerMovement(velocity);//function call

        MovePassengers(true);//function call
        transform.Translate(velocity);//moves the platform
        MovePassengers(false);//function call
    }

    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }
    //calculate velocity platform should move
    Vector3 CalculatePlatformMovement()
    {

        if (Time.time < nextMoveTime)//if we're not ready to move
        {
            return Vector3.zero;//then don't move
        }

        fromWaypointIndex %= globalWaypoints.Length;//find where we're coming from
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;//find where we're going
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);//find the distance
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;//add how far we wen't
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);//make sure it's a valid percent, prevents bound errors
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);//add ease losah

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);//get velocity component

        if (percentBetweenWaypoints >= 1)//if we reached the checkpoint
        {
            percentBetweenWaypoints = 0;//restart the clock
            fromWaypointIndex++;//go to the next one

            if (!cyclic)//if going back and forth
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)//if we reached the last check point
                {
                    fromWaypointIndex = 0;//end is start
                    System.Array.Reverse(globalWaypoints);//flip the array order
                }
            }
            nextMoveTime = Time.time + waitTime;//set when to move again
        }

        return newPos - transform.position;//return velocity to move platform by
    }
    //move things on platform
    void MovePassengers(bool beforeMovePlatform)
    {
        foreach (PassengerMovement passenger in passengerMovement)//for every object
        {
            if (!passengerDictionary.ContainsKey(passenger.transform))//if it's not on the passengerDictionary
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());//put it in
            }

            if (passenger.moveBeforePlatform == beforeMovePlatform)//check when to move the thing
            {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);//move the thing
            }
        }
    }
    //calculate velocity things on platform should move
    void CalculatePassengerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();//hashset is used to account for multiple objects
        passengerMovement = new List<PassengerMovement>();//same for list

        float directionX = Mathf.Sign(velocity.x);//left or right
        float directionY = Mathf.Sign(velocity.y);//up or down

        // Vertically moving platform
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                    }
                }
            }
        }

        // Horizontally moving platform
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = -skinWidth;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }

        // Passenger on top of a horizontally or downward moving platform
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }
    //holds movement data for each object on platform
    struct PassengerMovement
    {
        public Transform transform;//where is it going
        public Vector3 velocity;//how fast
        public bool standingOnPlatform;//is it on the platform
        public bool moveBeforePlatform;//should it move before or after the platform
        //constructor
        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }
    //draws waypoint pluses
    void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }

}