using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportLocomotion : MonoBehaviour {

    public Vector3 movement;
    public float movementX;
    public float movementY;
    public float inputX;
    public float inputY;
    public float speed = 10f;
    public Vector3 facingMovement;

    [SerializeField] GameObject Head;

    // Start is called before the first frame update
    void Start() {
        if(Head == null) {
            Debug.Log("Error No Player Avatar Found! Things may break...");
        }
        
    }

    // Update is called once per frame
    void Update() {
        LocalUpdate(); ;
    }
    private void LocalUpdate() {
        OVRInput.Update();
        //print(Head.transform.eulerAngles.y);
        Movement();
    }

    private void LocalFixedUpdate() {
        OVRInput.FixedUpdate();
    }


    void Movement() {
        //first movement type: WASD Inputs; runs every frame; 
        //need to find way to shut it off if the player is using vr
        KeyboardInput();

        //if nothing is received from the keyboard, run the VR movement
        //if (movementX == 0 && movementY == 0) {
            
                //VRThumbstickInput();
        //}

        //VectorFromEuler();

        movementX = inputX;
        movementY = inputY;


        //The inputed movement
        movement = new Vector3(movementX, 0.0f, movementY).normalized;
        //print(movement);
        //The movement edited to include the current facing direction
        //facingMovement.x = movement.x * Head.transform.forward.x;
        //facingMovement.z = movement.z * Head.transform.forward.z;
        //facingMovement.y = 0;

        //MoveForwardRight();
        transform.Translate(movement * Time.deltaTime * speed);
    }


    //Takes in WASD and adds force to the front/back and left/right axes.
    //Need to run this before the VR input sliding because it overwrites the movementX and Y.
    //Could potentially check to see if they are 0 after running the vr thumbstick method, 
    //but if there is drift in the thumbstick, you wouldn't be able to move via WASD.
    void KeyboardInput() {
        var tempFB = 0f;
        var tempLR = 0f;

        if (Input.GetKey(KeyCode.W)) {
            tempFB += 1;
        }
        if (Input.GetKey(KeyCode.S)) {
            tempFB -= 1;
        }
        if (Input.GetKey(KeyCode.A)) {
            tempLR -= 1;
        }
        if (Input.GetKey(KeyCode.D)) {
            tempLR += 1;
        }
        inputX = tempLR;
        inputY = tempFB;
    }

    //Takes in the Left VR Thumbstick from Oculus controllers and turns that into 2 
    //floats for front/back and left/right movement. 
    //Just checks for the left thumbstick for now; could check for both
    private void VRThumbstickInput() {
        var tempLeft = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        //check if the thumbstick is being pressed in any direction
        //if (tempLeft != new Vector2(0f, 0f)) {
        inputX = tempLeft.x;
        inputY = tempLeft.y;
        
        //}
    }

    void MoveForwardRight() {
        //I think forward isn't forward
        //I think z is what's normally x and vice versa
        if(movement.x > 0) {
            
            transform.position += transform.forward * Time.deltaTime * speed;
        }
        else if(movement.x < 0) {
            transform.position -= transform.forward * Time.deltaTime * speed;
        }
        if(movement.z > 0) {
            
            transform.position += transform.right * Time.deltaTime * speed;
        }
        else if (movement.z < 0) {
            transform.position -= transform.right * Time.deltaTime * speed;
        }
        
    }


    void VectorFromEuler() {
        double tempx = Math.Cos(Head.transform.eulerAngles.y);
        double tempz = Math.Sin(Head.transform.eulerAngles.y);
        movementX = -1f * inputX * (float)tempx;
        movementY = -1f * inputY * (float)tempz;
    }
}
