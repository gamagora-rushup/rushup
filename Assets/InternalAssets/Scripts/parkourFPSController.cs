﻿using System.Collections;
using UnityStandardAssets.Characters.FirstPerson; // only for MouseLook
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine;

public class parkourFPSController : MonoBehaviour
{
    private enum PlayerState {running, jumping, walling, sliding, edging, pushing};
    // Describing current state of the player : edging <=> grabed the edge of a cliff
    //                                          pushing <=> pushing up from edging state


    [Header("Global Variables")]
    [SerializeField] private float gravity = 9.81f;         // Gravity applied to the vector on the Y axis
    [SerializeField] private float jumpStrength = 20f;      // Impulse given at the start of a jump
    [SerializeField] private float minSpeed = 10f;          // Player will start running at this speed
    [SerializeField] private float maxNominalSpeed = 100f;  // Player's max speed without any killSpeedBonus
    [SerializeField] private float rampUpTime = 3.0f;       // Time for player to reach maxNominalSpeed (in seconds)
    [SerializeField] private float killSpeedBonus = 5f;     // Speed boost given immediately for each ennemy killed
    [Space(10)]
    [Header("Acceleration/Deceleration Factors")]
    [Space(10)]
    [Header("Mouse Properties")]
    [SerializeField] private MouseLook mouseLook = null;


    private Camera camera = null;
    private CharacterController controller;
    private PlayerState playerState = PlayerState.running;
    private Vector3 moveDir=Vector3.zero, prevMoveDir=Vector3.zero;
    private bool forwardKeyDown, prevGroundedState;
    private UnityEngine.UI.Text m_SpeedOMeterText;       // Text printed on the UI containing speed informations
    private UnityEngine.UI.Text m_DebugZoneText;         // Text printed on the UI containing speed informations
    private float forwardKeyDownTime = 0f;


	// Use this for initialization
	void Start ()
    {
        camera = Camera.main;
        controller = GetComponent<CharacterController>();
        controller.detectCollisions = true;
        mouseLook.Init(transform , camera.transform);
        m_SpeedOMeterText = GameObject.Find ("SpeedOMeter").GetComponent<UnityEngine.UI.Text>();
        m_DebugZoneText = GameObject.Find ("DebugZone").GetComponent<UnityEngine.UI.Text>();

        // Teleport Player to the ground to be sure of its playerState at startup
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 1000))
        {
            transform.position = new Vector3(hit.point.x, hit.point.y + controller.height/2f, hit.point.z);
        }
        else
        {
            Debug.Log("Please put the Player prefab above a floor/closer to it");
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
Debug.Log("playerState : "+playerState);
//TODO : demux input and physic handling for better performances ?

        updateUI();

        /*** CALCULATING FORCE FROM INPUTS & STATE***/ 
        switch(playerState)
        {
        case PlayerState.running:
            {
                updateRunning();
                break; 
            }
        case PlayerState.jumping:
            {
                updateJumping();
                break; 
            }
        case PlayerState.walling:
            {
                updateWalling();
                break; 
            }
        case PlayerState.sliding:
            {
                updateSliding();
                break; 
            }
        case PlayerState.edging:
            {
                updateEdging();
                break; 
            }
        case PlayerState.pushing:
            {
                updatePushing();
                break; 
            }
        default:
            { break; }
        }

        /*** APPLYING FORCE ***/
        controller.Move(moveDir * Time.deltaTime);

        /*** CONSERVING DATA FOR FUTURE REFERENCES ***/
        prevMoveDir = moveDir;
        prevGroundedState = controller.isGrounded;

        /*** LOCK mouseLook TO PREVENT UNWANTED INPUTS ***/
        mouseLook.UpdateCursorLock();
	}

    // FixedUpdate is called once per physic cycle
//    void FixedUpdate ()
//    { }
        
    void updateRunning()
    {
        // Update Camera look and freedom according to playerState
        updateCamera();


        // Build up the "momementum" as long as player is pressing "forward"
        forwardKeyDown = (CrossPlatformInputManager.GetAxis("Vertical")>0) ? true : false;
        if(forwardKeyDown && forwardKeyDownTime <= rampUpTime)
        {
            forwardKeyDownTime += Time.deltaTime;
            if (forwardKeyDownTime > forwardKeyDownTime)
            {
                forwardKeyDownTime = forwardKeyDownTime;
            }
        }

        if(controller.isGrounded)
        {
            // Make sure that our state is set (in case of falling of a clif => no jump but still been airborne for a while)
            playerState = PlayerState.running;

            // If Player is letting go of the "forward" key, stop accelerating
            forwardKeyDown = (CrossPlatformInputManager.GetAxis("Vertical")>0) ? true : false;
            if(!forwardKeyDown)
            {
                rampUpTime = 0f;
            }

            // get direction Vector3 from input
            moveDir = new Vector3(CrossPlatformInputManager.GetAxis("Horizontal"), 0f, CrossPlatformInputManager.GetAxis("Vertical"));
            moveDir = transform.TransformDirection(moveDir);
            moveDir.Normalize();

            // Correct moveDir according to the floor's slant
            RaycastHit hitInfoDown;
            if(Physics.SphereCast(transform.position, controller.radius, Vector3.down, out hitInfoDown,
               controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                moveDir = Vector3.ProjectOnPlane(moveDir, hitInfoDown.normal).normalized;
            }

            // Compute moveDir according to minSpeed, maxNominalSpeed, deltaTime, killStackSpeed, etc
//            moveDir *= minSpeed + ((maxNominalSpeed-minSpeed) * (forwardKeyDownTime / rampUpTime)); // MESSED UP BECAUSE FOR SOME REASON playerState is constantly changing !!!
            moveDir *= minSpeed;
            Debug.Log("test");

//            if(CrossPlatformInputManager.GetButton("Jump"))
//            {   // Jump Requested 
//                Debug.Log("check");
//                playerState = PlayerState.jumping;
//                moveDir.y = jumpStrength;
//            }
        }
        else // Player is running from an edge => change state to "jumping" and override current update()'s cycle result
        {
            playerState = PlayerState.jumping;
        }
    }

    void updateJumping()
    {
        // Update Camera look and freedom according to playerState
        updateCamera();





        //TODO 

        // DEBUG for running
        if(controller.isGrounded)
        {
            playerState = PlayerState.running;
            return;
        }

        moveDir.y -= gravity * Time.deltaTime;
    }

    void updateWalling()
    {
        // Update Camera look and freedom according to playerState
        updateCamera();
        
    }

    void updateSliding()
    {
        // Update Camera look and freedom according to playerState
        updateCamera();
    
    }

    void updateEdging()
    {
        // Update Camera look and freedom according to playerState
        updateCamera();
        
    }

    void updatePushing()
    {
        // Update Camera look and freedom according to playerState
        updateCamera();
        
    }

    void updateCamera()
    {
        switch(playerState)
        {
            default:
            {   // Allow rotation on every axis by default
                mouseLook.LookRotation (transform, camera.transform);
                break;
            }
        }
    }
    
    void updateUI()
    {
        float speed = (float) Mathf.Sqrt(controller.velocity.x * controller.velocity.x +
            controller.velocity.z * controller.velocity.z);
        // Actualize SpeedOMeter UI text
        m_SpeedOMeterText.text = speed + "m/s";
//        m_DebugZoneText.text = "m_speedPorcentage : " + m_speedPorcentage;
    }
}
