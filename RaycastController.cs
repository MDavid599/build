using UnityEngine;
using System.Collections;
//code cannot compile without Collider Component
[RequireComponent(typeof(BoxCollider2D))]//Note:  All types in script must change if using different collider type.
public class RaycastController : MonoBehaviour {
    public LayerMask collisionMask;//object defined
    public const float skinWidth = .015f;//places ray origins slightly inside collider to prevent collision bugs
                                  //important for collision detection
    public int horizontalRayCount = 4;//how many rays come out of the sides
    public int verticalRayCount = 4;//how many rays come from top and bottom
    [HideInInspector]
    public float horizontalRaySpacing;//Spacing between horizontal rays
    [HideInInspector]
    public float verticalRaySpacing;//spacing between vertical rays
    [HideInInspector]
    public BoxCollider2D bodyCollider;//instantiate bodyCollider object
    public RaycastOrigins raycastOrigins;//instantiate origin Object

    // Use this for SUPAH initialization
    public virtual void Awake()
    {
        bodyCollider = GetComponent<BoxCollider2D>();//assign collider component to object
        
    }
    // Use this for initialization
    public virtual void Start()
    {
        CalculateRaySpacing();//function call
    }
    // function that finds adjusted collider corners to allow proper ray origins
    public void UpdateRaycastOrigins()
    {
        Bounds bounds = bodyCollider.bounds;//find the collider boundaries
        bounds.Expand(skinWidth * -2);//expanding bounds by a negative number shrinks bounds

        //the next four lines determine the corners in 2d space for the collider in use.
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    // function that autocalculates the spacing betwen rays for uniform spacing
    public void CalculateRaySpacing()
    {

        Bounds bounds = bodyCollider.bounds;//find the edges
        bounds.Expand(skinWidth * -2);//shrink to ray origins

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);//makes sure that there are always at least 2 rays
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);//makes sure that there are always at least 2 rays

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);//spacing calculations for horizontal
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);//spacing calculation for vertical


    }

    public struct RaycastOrigins//structure for defining all 4 corners in 2D space
    {
        public Vector2 topLeft, topRight;//Top corner Variables
        public Vector2 bottomLeft, bottomRight;//Bottom corner variables
    }
}
