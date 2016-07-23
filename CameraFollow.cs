using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    public Controller2D target;//acceses player(editor accesible)
    public float verticalOffest;//camera offset(editor accesible)
    public float screenSize;//how big the sreen is in relation to the level(editor accesible)
    /*public float lookAheadDstX;
    public float lookSmoothTimeX;
    public float verticalSmoothTime;*/
    public Vector2 focusAreaSize;//how big is my bounding box(editor accesible)

    FocusArea focusArea;//instantiate focus area

    /*float currentLookAheadX;
    float targetLookAheadX;
    float LookAheadDirX;
    float smoothLookVelocityX;
    float smoothVelocityY;*/

    void Start()
    {
        focusArea = new FocusArea(target.bodyCollider.bounds, focusAreaSize);//constructor called using editor settings
    }

    void LateUpdate()//procceses after all normal update loops
    {
        focusArea.Update(target.bodyCollider.bounds);//check the box to see if the camera needs to be moved
        Vector2 focusPosition = focusArea.centre + Vector2.up * verticalOffest;

       /* if(focusArea.velocity.x != 0)
        {
            LookAheadDirX = Mathf.Sign(focusArea.velocity.x);
        }

        targetLookAheadX = LookAheadDirX * lookAheadDstX;
        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

        focusPosition = Vector2.right * currentLookAheadX;*/

        transform.position = (Vector3)focusPosition + Vector3.forward * -1 * screenSize;//moves camera; negative zooms out. 
    }

    void OnDrawGizmos()//draws camera follow box in scene window
    { 
        Gizmos.color = new Color(0, 0, 1,.5f);//colors it
        Gizmos.DrawCube(focusArea.centre, focusAreaSize);//draws it
    }

    struct FocusArea//struct that defings camera loolow box
    {
        public Vector2 centre;//where's the middle?
        public Vector2 velocity;//how fast is box moving?
        float left, right;//these define
        float top, bottom;//the edges

        public FocusArea (Bounds targetBounds, Vector2 size)//constructor, first variable determines the centre, second variable determines the boundary size
        {
            //four equations to define edges
            left = targetBounds.center.x - size.x / 2;
            right = targetBounds.center.x + size.x / 2;
            bottom = targetBounds.min.y;
            top = targetBounds.min.y+size.y;

            velocity = Vector2.zero;//starts not moving
            centre = new Vector2 ( (left + right) / 2, (top + bottom) / 2);//defines centre
        }

        public void Update (Bounds targetBounds)//function checks if the camera needs to be moved 
        {
            float shiftX = 0;//value to shift by horizontally
            if (targetBounds.min.x < left)//am i pushing the left edge?
            {
                shiftX = targetBounds.min.x - left;//how far over?
            }
            else if (targetBounds.max.x > right)//am i pushing the right edge?
            {
                shiftX = targetBounds.max.x - right;//how far over?
            }
            left += shiftX;//move left edge by the difference
            right += shiftX;//move right edge by the difference

            float shiftY = 0;//value to shift by vertically
            if (targetBounds.min.y < bottom)//am i pushing the bottom edge?
            {
                shiftY = targetBounds.min.y - bottom;//how far over?
            }
            else if (targetBounds.max.y > top)//am i pushing the top edge?
            {
                shiftY = targetBounds.max.y - top;//how far over?
            }
            top += shiftY;//move top edge by the difference
            bottom += shiftY;//move bottom edge by the difference

            centre = new Vector2 ( (left + right) / 2, (top + bottom) / 2);//find centre after changes
            velocity = new Vector2 (shiftX, shiftY);//how fast did it move?
        }
    }
}
