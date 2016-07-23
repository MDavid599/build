/*
Name: Michael David
Filename: Player.cs
Date created: 7/15/2016
Date Updated: 7/20/2016

*/

using UnityEngine;//library include for UnityScript
using System.Collections;//Library include for all scripts in project

[RequireComponent (typeof (Controller2D))]//script cannot operate without controller2D component

public class Player : MonoBehaviour {
    public float jumpHeight = 4;//how high can I jump?
    public float timeToJumpApex = .4f;//how long until i reach the top of my Jump?
    public float moveSpeed = 6;// how fast can i move left and right?
    public float jumpAdjust = .2f;

    bool inAir;
    float inputXvector;
    float gravity;//player subjective; derived by gravity equation in Start()
    float jumpVelocity;//player subjective; derived by jumpVelocity equation in Start()
	Vector3 velocity;//used for states
    float velocityXsmoothing;//smooths horizontal movement
    public float accelerationTimeAirborne = .2f;//acceleration in air
    public float accelerationTimeGrounded = .1f;//acceleration on ground
    Controller2D controller;// instantiate controller object

	// Use this for initialization
	void Start () {
		controller = GetComponent<Controller2D>();// assign component to object
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);//physics equation for gravity, gravity is negative
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;//physics equation for velocity
	}
	
	// Update is called once per frame
	void Update () {

        

        if (controller.collisions.above || controller.collisions.below)//if i hit the ceiling or ground
        {
            velocity.y = 0;//i dont move through the ceiling or ground
            inAir = false;
        }
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));//input reciever, edited in input project settings

        if(Input.GetAxis("Jump") >0 && controller.collisions.below)//jump command.
        {
            velocity.y = jumpVelocity;//jump execution
            inputXvector = input.x;
            inAir = true;
        }

        if(!controller.collisions.below&& inAir)
        {
            if(Input.GetAxis("Horizontal") > 0 && inputXvector != 0)
            {
                inputXvector += jumpAdjust;
            }
            if (Input.GetAxis("Horizontal") < 0 && inputXvector != 0)
            {
                inputXvector -= jumpAdjust;
            }
            input.x = inputXvector;
        }
     
        float targetVelocityX = input.x * moveSpeed;//horizontal target speed
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXsmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);// go to target speed based on accelerator in use(on ground or in air)
		velocity.y += gravity * Time.deltaTime;//gravity effector
		controller.Move (velocity * Time.deltaTime);//moving now
	}
}
