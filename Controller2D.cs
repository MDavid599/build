/*
Name: Michael David
Filename: Controller2D.cs
Date created: 7/15/2016
Date Updated: 7/20/2016

Description:  Raycasting involves using rays; linear sensors that radiate out from the object.
Using these sensors can assist in determining whether one is in the air or on the wall.
Has greater precision than child object colliders and interfaces with unity events
*/

using UnityEngine;//library include for UnityScript
using System.Collections;//Library include for all scripts in project


//replace monobehavior with script you wanna link up to.
public class Controller2D : RaycastController
{

    public float maxClimbAngle = 80;//highest hill I can climb(editor accesible. Units in Degrees)
    public float maxDescendAngle = 80;//steepest cliff I can descend(editor accesible. Units in Degrees)

    public CollisionInfo collisions;//instantiated collisions data object

    public override void Start()
    {
        base.Start();//raycast goes first
    }

    public void Move(Vector3 velocity, bool standingOnPlatform = false)
    {

        UpdateRaycastOrigins();//function call.
        collisions.Reset();//reseting collisions untrips all triggers.
        collisions.velocityOld = velocity;//saves velocity from prior frame.

        if (velocity.y < 0)//for if i'm descending
        {
            DescendSlope(ref velocity);//function call
        }

        if (velocity.x != 0)//am I moving horizontally?
        {
            HorizontalCollisions(ref velocity);//function call
        }

        if (velocity.y != 0)//am I moving vertically?
        {
            VerticalCollisions(ref velocity);//function call
        }

        transform.Translate(velocity);//function call

        if (standingOnPlatform)//Am i standing on a thing?
        {
            collisions.below = true;//not in th air then.
        }

    }

    //function that checks horizontal collisions
    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);//right is positive, left is negative. 
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;//length adjusts according to speed for tighter control
        //loop handles wall collisions for each ray
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;//starts from front
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);//determines which ray is being spawned
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);//really fancy bool to determine if we hit something
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);//colors rays from bottom in red
            if (hit)//did we hit something?
            {
                if (hit.distance == 0)//if i'm already pushing into it
                {
                    continue;//proceed as normal
                }
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);//if there's an angle, find the value

                if (i == 0 && slopeAngle <= maxClimbAngle)//if my front corner is hitting it and I can climb it
                {
                    if (collisions.descendingSlope)//if i was descending a slope
                    {
                        collisions.descendingSlope = false;//not doing that no more
                        velocity = collisions.velocityOld;//gimme my speed back
                    }
                    float distanceToSlopeStart = 0;//how far from slope
                    if (slopeAngle != collisions.slopeAngleOld)//if its a different angle than before
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;//how far am I from it?
                        velocity.x -= distanceToSlopeStart * directionX;//change my speed to account for the approach
                    }
                    ClimbSlope(ref velocity, slopeAngle);//call fuction
                    velocity.x += distanceToSlopeStart * directionX;//restore speed
                }

                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)//if i'm not climbing a slope OR the angle is too steep
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;//change my speed
                    rayLength = hit.distance;//adjust my rayLength

                    if (collisions.climbingSlope)//if it's just the angle is too steep
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);//start sliding down
                    }
                    collisions.left = directionX == -1;//normal collision
                    collisions.right = directionX == 1;//normal collision
                }
            }
        }
    }

    //function that checks vertical collisions
    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);//up is positive, down is negative. 
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;//length adjusts according to speed for tighter control
        //loop handles ground collision per ray
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;//starting from the back
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);//spawning rays
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);//bool check for hit

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);//colors rays from bottom in red

            if (hit)//did we hit something?
            {
                velocity.y = (hit.distance - skinWidth) * directionY;//adjust speed
                rayLength = hit.distance;//adjust raylength
                if (collisions.climbingSlope)//if i'm climbing a slope
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);//adjust angular speed
                }
                collisions.below = directionY == -1;//restore collisions
                collisions.above = directionY == 1;//restore collisions
            }
        }
        if (collisions.climbingSlope)//if i'm climbing a slope
        {
            float directionX = Mathf.Sign(velocity.x);//which way am i facing?
            rayLength = Mathf.Abs(velocity.x) + skinWidth;//how long is my ray?
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;//spawn ray
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);//check for hits

            if (hit)//did we hit something?
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);//find the angle
                if (slopeAngle != collisions.slopeAngle)//if new angle
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;//adjust speed
                    collisions.slopeAngle = slopeAngle;//change angle
                }
            }
        }
    }

    void ClimbSlope(ref Vector3 velocity, float slopeAngle)//for climbing slopes
    {
        float moveDistance = Mathf.Abs(velocity.x);//unsign for calculation
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;//determines vertical adjusted velocity
        if (velocity.y <= climbVelocityY)//if slower than adjusted velocity
        {
            velocity.y = climbVelocityY;//match speed
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);//determine vertical velocity
            collisions.below = true;//still on ground
            collisions.climbingSlope = true;//still climbing
            collisions.slopeAngle = slopeAngle;//keep the angle
        }


    }

    void DescendSlope(ref Vector3 velocity)//fuction for descending slopes
    {
        float directionX = Mathf.Sign(velocity.x);//which way am I facing?
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;//spawn ray from back foot
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);//check for hits

        if (hit)//did we hit something?
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);//find angle
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)//is it not too steep?
            {
                if (Mathf.Sign(hit.normal.x) == directionX)//if we're actually going downhill
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))//are we touching it?
                    {
                        float moveDistance = Mathf.Abs(velocity.x);//unsigned for calculation
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;//adjust vertical velocity
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);//adjust horizontal velocity
                        velocity.y -= descendVelocityY;//apply vertical velocity.  -= for going down.

                        collisions.slopeAngle = slopeAngle;//adjust angle
                        collisions.descendingSlope = true;//we are going down
                        collisions.below = true;//still on the ground
                    }
                }
            }
        }
    }


    public struct CollisionInfo//determines if rays are activated
    {
        public bool above, below;//did i hit something above or below me?
        public bool left, right;//did i hit something left or right of me?
        public bool climbingSlope;//am i climbing?
        public bool descendingSlope;//am i descending?
        public float slopeAngle, slopeAngleOld;//did i change angles?
        public Vector3 velocityOld;//how fast was I going?

        public void Reset()//resets data
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
