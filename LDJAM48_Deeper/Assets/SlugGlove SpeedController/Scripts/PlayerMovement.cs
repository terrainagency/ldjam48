using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public enum PlayerStates
    {
        Grounded,//on the ground
        InAir, //in the air
        OnWalls, //running on the walls
        LedgeGrab, //pulling up a ledge
        Sliding, //Sliding Along the ground
        Death, //this unit is dead
    }

    private PlayerCollision Coli;
    private Rigidbody Rigid;
    private Animator Anim;
    private CameraCtrl Cam;
    private PlayerVisual Visual;

    [Header("Physics")]
    public float MaxSpeed; //how fast we run forward
    public float SpeedClamp;//how fast we can possibly be
    [Range(0, 1)]
    public float InAirControl; //how much control you have over your movement direction when in air

    private float ActSpeed; //how much speed is applied to the rigidbody
    public float Acceleration; //how fast we build speed
    public float Decceleration; //how fast we slow down
    public float DirectionControl = 8; //how much control we have over changing direction
    private float AdjustMentCtrl = 1; //a lerp for how much control we have over our character
    private PlayerStates CurrentState; //the current state the player is in
    private float InAirTimer; //how long we are in the air for (this is for use when wall running or falling off the ground
    private float OnGroundTimer; //how long we are on the ground for
    private float SlidingTimer; //how long we are sliding
    public float Stickyness; //how much we stick to the ground

    [Header("SlopeMovement")]
    public float MaxSlopeAmt; //the max  slope that will influence our player
    public float MinSlopeAmt; //the smallest slope that will influence our player
    public float SlopeSpeedGain; //the max speed we can add to our controller
    public float SlopeSpeedLoss; //the min speed we can add to our controller

    [Header("DistanceCheck")]
    public float SpeedNeededToCheck; //how fast we must be going to check distance
    public float TimeBtwSpeedChecks; //how frequently we check for distance between positions
    private float SpeedCheckTime; //actual timer
    private Vector3 LastPosition; //the last position we were at
    public float DistanceNeeded; //the distance needed for are player to move

    [Header("Turning")]
    public float TurnSpeed; //how fast we turn when on the ground
    public float TurnSpeedInAir; //how fast we turn when in air
    public float TurnSpeedSliding; //how fast we are when sliding
    public float TurnSpeedOnWalls; //how fast we are when running on a wall


    [Header("Jumping")]
    public float MaxSpeedBoostOnJump;
    public float JumpHeight; //how high we jump
    [Header("BackFlip")]
    public float BackFlipHeight; //how high a backflip is
    public float BackFlipBackwards; //how far backwards we go on backflip
    [Header("Leap")]
    public float LeapHeight; //how high up our leap is
    public float LeapForwards; //for far forwards our leep is

    [Header("Better Jumping")]
    public float JumpHoldAmt; //how much holding jump adds to a jump 
    public float JumpHoldTime; //how long we can hold jump for
    public float fallMultiplier = 2.5f; //multiplier to falling 
    public float lowJumpMultiplier = 2f; //multiplier to begining of jump

    [Header("Wall Runs")]
    public float SpeedBeforeWallRun; //how fast we must be before wall running
    public float WallRunTime = 2f; //how long we can run on walls
    private float ActWallRunTime = 0; //how long we are actually on a wall
    public float TimeBeforeWallRun = 0.2f; //how long we have to be in the air before we can wallrun
    public float WallRunUpwardsMovement = 2f; //how much we move up a wall when running on it (make this 0 to just slightly move down a wall we run on
    public float WallRunSpeedAcceleration = 2f; //how quickly we build speed to run up walls
    private Vector3 WallNormal; //direction of the wall

    [Header("Crouching")]
    public float CrouchSpeed = 10; //how fast we move when crouching
    private bool Crouch;  //if we are crouching

    [Header("Sliding")]
    public float SlideAmt; //how far we slide when pressing crouch
    public float SlideSpeedLimit; //how fast we have to be traveling before a crouch will stop
    public float SpeedBeforeSlide; //how fast we are going before we can slide
    public float SlideTime; //how long we can slide for

    [Header("WallGrabbing")]
    public float PullUpTime; //the time it takes to pull onto a ledge
    private float ActPullTm; //the actual time it takes to pull up a ledge
    private Vector3 OrigPos; //the original Position before grabbing a ledge
    private Vector3 LedgePos; //the ledge position to move to

    [Header("Respawn")]
    public float TimeBtwRespawn;
    private float RespawnTimer;
    private RespawnPoint LastRespawn;

    // Start is called before the first frame update
    void Start()
    {
        Coli = GetComponent<PlayerCollision>();      
        Rigid = GetComponent<Rigidbody>();
        Anim = GetComponentInChildren<Animator>();
        Visual = GetComponent<PlayerVisual>();

        Cam = GetComponentInChildren<CameraCtrl>();
        Cam.GetComponent<CameraCtrl>().Setup(this.transform);
        Cam.transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        float XMove = Input.GetAxis("Horizontal");
        float YMove = Input.GetAxis("Vertical");
        //movement direction
        Vector3 MoveDir = (Cam.transform.forward * YMove) + (Cam.transform.right * XMove);

        if (CurrentState == PlayerStates.Grounded)
        {
            //if we press jump
            if (Input.GetButtonDown("Jump"))
            {
                //jump upwards
                if (Crouch)
                {
                    //backflip
                    Anim.SetTrigger("Backflip");
                    JumpUp(BackFlipHeight, -BackFlipBackwards);

                    //create jumpfx
                    Visual.JumpHeavy();

                    return;
                }
                else
                {
                    //normal jump
                    JumpUp(JumpHeight, 0);

                    //create jumpfx
                    Visual.Jump();

                    return;
                }
            }
        }
        else if (CurrentState == PlayerStates.Sliding)
        {
            //if we press jump
            if (Input.GetButtonDown("Jump"))
            {
                //leap
                Anim.SetTrigger("Leap");
                JumpUp(LeapHeight, LeapForwards);


                //create jumpfx
                Visual.JumpHeavy();

                return;
            }
        }
        else if (CurrentState == PlayerStates.InAir)
        {
            //check for ledge grabs
            if (Rigid.velocity.y < 4)
            {
                if (XMove != 0 || YMove != 0)
                {
                    Vector3 LedgePos = Coli.CheckLedges(transform.forward, Vector3.up);
                    if (LedgePos != Vector3.zero)
                    {
                        LedgeGrab(LedgePos);
                    }
                }
            }

            //Check if there is a wall to run on
            Vector3 Wall = CheckWalls(MoveDir);

            //we are on the wall
            if (Wall != Vector3.zero)
            {
                if (InAirTimer > TimeBeforeWallRun)
                {
                    SetOnWall();
                    return;
                }
            }

            //check for the ground 
            Vector3 Grounded = Coli.CheckFloor(-Vector3.up);

            //we are on the ground (and have been in the air for a short time, to prevent multiple jump glitched
            if (Grounded != Vector3.zero && InAirTimer > 0.25f)
            {
                SetOnGround();
            }
        }
        else if (CurrentState == PlayerStates.OnWalls)
        {
            //check for ledge grabs
            if (Rigid.velocity.y < 6)
            {
                if (XMove != 0 || YMove != 0)
                {
                    Vector3 LedgePos = Coli.CheckLedges(-transform.up, Vector3.up);

                    if (LedgePos != Vector3.zero)
                    {
                        LedgeGrab(LedgePos);
                    }
                }
            }

            //if we press jump
            if (Input.GetButtonDown("Jump"))
            {
                //normal jump
                JumpUp(JumpHeight, 0);

                //create jumpfx
                Visual.Jump();

                return;
            }

            //Check if there is a wall to run on
            Vector3 Wall = CheckWalls(-WallNormal);

            //we are no longer on the wall, fall off it
            if (Wall == Vector3.zero)
            {
                Wall = CheckWalls(MoveDir);
                SetInAir();
                return;
            }

            //check for the ground 
            Vector3 Grounded = Coli.CheckFloor(-Vector3.up);

            //we are on the ground
            if (Grounded != Vector3.zero)
            {
                SetOnGround();
            }
        }
        else if(CurrentState == PlayerStates.LedgeGrab)
        {
            //clamp our rigid velocity to nothing
            Rigid.velocity = Vector3.zero;
        }

        AnimCtrl();
    }

    void AnimCtrl()
    {
       //current animated state
        int State = 0;
        if (CurrentState == PlayerStates.InAir)
            State = 1;
        else if (CurrentState == PlayerStates.OnWalls)
            State = 2;
        else if (CurrentState == PlayerStates.LedgeGrab)
            State = 3;
        else if (CurrentState == PlayerStates.Sliding)
            State = 4;
        else if (CurrentState == PlayerStates.Death)
            State = 5;

        Anim.SetInteger("State", State);
        Anim.SetBool("Crouching", Crouch);

        Vector3 Vel = transform.InverseTransformDirection(Rigid.velocity);
        Anim.SetFloat("XVelocity", Vel.x);
        Anim.SetFloat("ZVelocity", Vel.z);
        Anim.SetFloat("YVelocity", Rigid.velocity.y);
        Anim.SetFloat("Speed", ActSpeed);
    }

    private void FixedUpdate()
    {
        float Del = Time.deltaTime;

        //get inputs
        float horInput = Input.GetAxis("Horizontal");
        float verInput = Input.GetAxis("Vertical");
        //get magnituded of our inputs
        float InputMagnitude = new Vector2(horInput, verInput).normalized.magnitude;
        //movement direction
        Vector3 MoveDir = (Cam.transform.forward * verInput) + (Cam.transform.right * horInput);
        MoveDir.y = 0;

        //handle our fov
        HandleFov(Del);

        if (CurrentState == PlayerStates.Grounded)
        {
            //tick our ground timer
            if (OnGroundTimer < 10)
                OnGroundTimer += Del;

            //check for the ground 
            Vector3 Grounded = Coli.CheckFloor(-Vector3.up);

            //we are in the air
            if (Grounded == Vector3.zero)
            {
                if (InAirTimer > 0.05f)
                {
                    SetInAir();
                    return;
                }
                else
                {
                    InAirTimer += Del;
                }
            }
            else
                InAirTimer = 0;

            //get the amount of speed, based on if we press forwards or backwards
            float TargetSpd = MaxSpeed;
            float SlopeAmt = 0;

            //if we are crouching our target speed is our crouch speed
            if (Crouch)
                TargetSpd = CrouchSpeed;

            //we are running up a hill
            if(Rigid.velocity.y > MinSlopeAmt)
            {
                if(ActSpeed > 7)
                {
                    float LossAmt = Rigid.velocity.y / MaxSlopeAmt;
                    float SpeedLoss = Mathf.Lerp(0, SlopeSpeedLoss, LossAmt);
                    SlopeAmt = -SpeedLoss;
                }
            }
            else if (Rigid.velocity.y < -MinSlopeAmt) //we are running down a hill
            {
                float GainAmt = -Rigid.velocity.y / -MaxSlopeAmt;
                GainAmt = GainAmt * -1;
                float SpeedGain = Mathf.Lerp(0, SlopeSpeedGain, GainAmt);
                SlopeAmt = SpeedGain;
            }

            //check we are not moving against a walls
            if (ActSpeed >= SpeedNeededToCheck)
            {
                //get distance
                float DistanceCheck = CheckDis(Del);
                //multiply the speed by distance
                TargetSpd = TargetSpd * DistanceCheck;
            }

            //accelerate speed
            LerpSpeed(InputMagnitude, Del, TargetSpd, SlopeAmt);
            //lerp our adjustment control
            float Ctrl = DirectionControl;
            if(AdjustMentCtrl < 1)
            {
                Ctrl = Ctrl * AdjustMentCtrl;
                AdjustMentCtrl += Del;
            }

            //get ground direction
            Vector3 GroundDir = Grounded;
            //move and turn player
            MovePlayer(MoveDir, InputMagnitude, Del, Ctrl);
            TurnPlayer(MoveDir, Del, TurnSpeed, GroundDir);

            //check for crouching 
            if(Input.GetButton("Crouching"))
            { 
                //start crouching
                if(!Crouch)
                {
                    StartCrouch();
                }
            }
            else if(Crouch)
            {
                //stand up
                StopCrouching();
            }           
        }
        else if(CurrentState == PlayerStates.InAir)
        {
            //tick our Air timer
            if (InAirTimer < 10)
                InAirTimer += Del;
            //if we can hold jump
            if(InAirTimer < JumpHoldTime)
            {
                //if jump is held
                if (Input.GetButton("Jump"))
                    Rigid.velocity += transform.up * JumpHoldAmt;
            }

            //lerp speed
            float TargetSpd = MaxSpeed;
            InputMagnitude = InputMagnitude * InAirControl;
            //check we are not moving against a walls
            if (ActSpeed >= SpeedNeededToCheck)
            {
                //get distance
                float DistanceCheck = CheckDis(Del);
                //multiply the speed by distance
                TargetSpd = TargetSpd * DistanceCheck;
            }

            //Move our speed Slowly
            LerpSpeed(InputMagnitude, Del, TargetSpd, 0);

            //move player
            InAirMovement(MoveDir, InputMagnitude, Del, InAirControl);

            //turn our player with the in air modifier
            TurnPlayer(MoveDir, Del, TurnSpeedInAir, Vector3.up);
        }
        else if (CurrentState == PlayerStates.OnWalls)
        {
            //tick our wall run timer
            ActWallRunTime += Del;

            //move our player when on a wall
            WallMove(MoveDir, InputMagnitude, Del);

            Vector3 WallDir = WallNormal;
            //turn our player with the on walls modifier
            TurnPlayer(MoveDir, Del, TurnSpeedOnWalls, WallDir);
        }
        else if(CurrentState == PlayerStates.LedgeGrab)
        {
            TurnPlayer(transform.forward, Del, TurnSpeed, Vector3.up);

            //tick ledge grab time 
            ActPullTm += Del;

            //pull up the ledge
            float PullUpLerp = ActPullTm / PullUpTime;

            if (PullUpLerp < 0.5)
            {              
                //lerp our player upwards to the leges y position
                float LAmt = PullUpLerp * 2;
                transform.position = Vector3.Lerp(OrigPos, new Vector3(OrigPos.x, LedgePos.y, OrigPos.z), LAmt);               
            }
            else if(PullUpLerp <= 1)
            {
                //set new pull up position
                if (OrigPos.y != LedgePos.y)
                    OrigPos = new Vector3(transform.position.x, LedgePos.y, transform.position.z);


                //move to the ledge position
                float LAmt = (PullUpLerp - 0.5f) * 2;
                transform.position = Vector3.Lerp(OrigPos, LedgePos, PullUpLerp);
            }
            else
            {
                //we have finished pulling up!
                SetOnGround();
            }
        }
        else if(CurrentState == PlayerStates.Sliding)
        {
            //Reduce Sliding Timer
            SlidingTimer += Del;

            float Mag = new Vector2(Rigid.velocity.x, Rigid.velocity.z).magnitude;
            float Control = 1; 

            if (SlidingTimer > SlideTime)
            {
                //slow our slide now
                Control = 0.5f;
                //go back to grounded if we have finished a slide
                if (Mag <= SlideSpeedLimit)
                    SetOnGround();
            }

            //move ourself
            SlidePlayer(Mag, Control, Del, DirectionControl);
            TurnPlayer(MoveDir, Del, TurnSpeedSliding, Vector3.up);
            //reduce our speed to a crouch
            LerpSpeed(1, Del, CrouchSpeed, 0);

            //check for the ground 
            Vector3 Grounded = Coli.CheckFloor(-Vector3.up);

            //we are in the air
            if (Grounded == Vector3.zero)
            {
                if (InAirTimer > 0.05f)
                {
                    SetInAir();
                    return;
                }
                else
                {
                    InAirTimer += Del;
                }
            }
            else
                InAirTimer = 0;
        }
        else if(CurrentState == PlayerStates.Death)
        {
            //we are dead, reduce the death timer
            RespawnTimer -= Del;
            
            if(RespawnTimer <= 0)
            {
                RespawnPlayer();

                SetOnGround();
            }
        }
    }

    void SpeedBoost(float Amt)
    {
        //add to our speed
        ActSpeed += Amt;
        //stop crouching if we are
        if(Crouch)
            StopCrouching();
    }

    public void BounceForce(float Amt, Vector3 Dir)
    {
        //add force
        Rigid.AddForce(Dir * Amt, ForceMode.Impulse); 
        //set our adjustment to 0
        AdjustMentCtrl = 0;
    }

    //lerp our current speed to our set max speed, by how much we are pressing the horizontal and vertical input
    void LerpSpeed(float InputMag, float D, float TargetSpeed, float SlopeBoost)
    {
        if (InputMag == 0 && ActSpeed == 0) //do not lerp on no speed
            return;
        else if (InputMag == 0 && ActSpeed < 0.1)
            ActSpeed = 0;

        //multiply our speed by our input amount
        float LerpAmt = TargetSpeed * InputMag;
        //get our acceleration (if we should speed up or slow down
        float Accel = Acceleration;
        if (InputMag == 0)
            Accel = Decceleration;

        //increase our actual speed by any slope boost
        if (SlopeBoost != 0)
            LerpAmt += SlopeBoost;

        //lerp by a factor of our acceleration
        ActSpeed = Mathf.Lerp(ActSpeed, LerpAmt, D * Accel);
        //add boost
        ActSpeed += SlopeBoost * D;
        //clamp speed
        ActSpeed = Mathf.Clamp(ActSpeed, -SpeedClamp, SpeedClamp);
    }

    //when in the air or on a wall, we set our action speed to the velocity magnitude, this is so that when we reach the ground again, our speed will carry over our momentum
    void SetSpeedToVelocity()
    {
        float Mag = new Vector2(Rigid.velocity.x, Rigid.velocity.z).magnitude;
        ActSpeed = Mag;
    }

    Vector3 CheckWalls(Vector3 Dir)
    {
        if (Dir == Vector3.zero) //if no direction input we are not wall running
            return Vector3.zero;

        if (ActWallRunTime >= WallRunTime) //if our wall run timer is more than the amount we can run on walls for, we cannot wall run
            return Vector3.zero;

        if (ActSpeed < SpeedBeforeWallRun) //if we are not fast enough to wall run
            return Vector3.zero;

        Vector3 WallCol = Coli.CheckWall(Dir);

        //set our wall direction
        WallNormal = WallCol;

        return WallCol;
    }

    void SetInAir()
    {
        if(Crouch)
            StopCrouching(); //cannot crouch in airosh

        //remove any extra downwards momentum
        Vector3 VelAmt = new Vector3(Rigid.velocity.x, 0, Rigid.velocity.z);
        Rigid.velocity = VelAmt;

        OnGroundTimer = 0; //remove the on ground timer
        CurrentState = PlayerStates.InAir;
    }

    void SetOnGround()
    {
        //set our current speed to our velocity
        //SetSpeedToVelocity();

        ActWallRunTime = 0; //we are on the ground again, our wall run timer is reset
        InAirTimer = 0; //remove the in air timer
        CurrentState = PlayerStates.Grounded;

        //create landing fx 
        Visual.Landing();
    }

    void SetOnWall()
    {
        OnGroundTimer = 0; //remove the on ground timer
        InAirTimer = 0; //remove the in air timer
        CurrentState = PlayerStates.OnWalls;
    }

    void LedgeGrab(Vector3 Ledge)
    {
        //set our ledge position
        LedgePos = Ledge;
        OrigPos = transform.position;
        //reset ledge grab time
        ActPullTm = 0;
        //remove speed and velocity
        Rigid.velocity = Vector3.zero;
        ActSpeed = 0;
        //start ledge grabs
        CurrentState = PlayerStates.LedgeGrab;
    }

    void StartCrouch()
    {
        Crouch = true;

        if (ActSpeed > SpeedBeforeSlide)
            SlideSelf();
    }

    void StopCrouching()
    {
        Crouch = false;
    }

    void TurnPlayer(Vector3 Dir, float D, float turn, Vector3 FloorDirection)
    {
        //old rotation settings
        float singleStep = (turn * Time.deltaTime);
        /*
        //Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, Dir, singleStep, 0.0f);

        //Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);
        */
        //lerp our upwards rotation to stick to the floor
        Vector3 LerpDir = Vector3.Lerp(transform.up, FloorDirection, D * 8f);
        transform.rotation = Quaternion.FromToRotation(transform.up, LerpDir) * transform.rotation;

        //lerp our transform rotation to the direction of movement input
        if (Dir == Vector3.zero)
            Dir = transform.forward;
        Quaternion SlerpRot = Quaternion.LookRotation(Dir, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, SlerpRot, turn * D);
    }
    
    void MovePlayer(Vector3 Dir, float InputAmt, float D, float Ctrl) //direction to move, input of axis, delta, control over velocity
    {
        //find the direction to move in, based on the direction inputs
        Vector3 MovementDirection = Dir * InputAmt;
        MovementDirection = MovementDirection.normalized;
        //if we are no longer pressing and input, carryon moving in the last direction we were set to move in
        if (Dir == Vector3.zero)
            MovementDirection = transform.forward;

        MovementDirection = MovementDirection * ActSpeed;

        //apply Gravity and Y velocity to the movement direction 
        MovementDirection.y = Rigid.velocity.y;

        float AdjustCtrl = Ctrl;
        //lerp to our movement direction based on how much airal control we have
        Vector3 LerpVelocity = Vector3.Lerp(Rigid.velocity, MovementDirection, AdjustCtrl * D);
        //stick to grounds
        LerpVelocity += -transform.up * Stickyness;
        //set velocity
        Rigid.velocity = LerpVelocity;
    }

    void InAirMovement(Vector3 Dir, float InputAmt, float D, float Ctrl) //direction to move, input of axis, delta, control over velocity
    {
        //find the direction to move in, based on the direction inputs
        Vector3 MovementDirection = Dir * InputAmt;
        MovementDirection = MovementDirection.normalized;
        //if we are no longer pressing and input, carryon moving in the last direction we were set to move in
        if (Dir == Vector3.zero)
            MovementDirection = transform.forward;

        MovementDirection = MovementDirection * ActSpeed;

        //apply Gravity and Y velocity to the movement direction 
        MovementDirection.y = Rigid.velocity.y;

        //better jumping
        if (MovementDirection.y < 0)
            MovementDirection.y += Physics.gravity.y * (fallMultiplier - 1) * D;
        else if (MovementDirection.y > 0)
            MovementDirection.y -= Physics.gravity.y * (lowJumpMultiplier - 1) * D;

        float AdjustCtrl = Ctrl;
        //lerp to our movement direction based on how much airal control we have
        Vector3 LerpVelocity = Vector3.Lerp(Rigid.velocity, MovementDirection, AdjustCtrl * D);
        //set velocity
        Rigid.velocity = LerpVelocity;
    }

    void SlidePlayer(float Spd, float Magnitude, float D, float Ctrl)
    {
        //move ourself forawrds
        Vector3 MovementDirection = transform.forward;
        //apply speed
        MovementDirection = MovementDirection * Spd * Magnitude;
        //apply Gravity and Y velocity to the movement direction 
        MovementDirection.y = Rigid.velocity.y;
        //lerp to our movement direction based on how much airal control we have
        Vector3 LerpVelocity = Vector3.Lerp(Rigid.velocity, MovementDirection, Ctrl * D);
        //stick to grounds
        LerpVelocity += -transform.up * Stickyness;
        //add rigidbody
        Rigid.velocity = LerpVelocity;
    }

    void WallMove(Vector3 MovDir, float InputDir, float D)
    {
        //get the direction to run up this wall if we press forward (keep in mind this only works if the wall is infront or to the side of the player as we run along on, on walls to our immediate right or left we slide down
        Vector3 MovementDirection = Vector3.up;
        MovementDirection = MovementDirection * WallRunUpwardsMovement;

        //our x z velocity are our momentum applied to our forward direction
        MovementDirection += transform.forward * ActSpeed;
        MovementDirection += -WallNormal * 2f; //push down onto wall

        Vector3 LerpVelocity = Vector3.Lerp(Rigid.velocity, MovementDirection, WallRunSpeedAcceleration * D);
        Rigid.velocity = LerpVelocity;
    }

    void JumpUp(float Upwards, float Forwards)
    {        
        //reduce our velocity on the y axis so our jump force can be added
        Vector3 VelAmt = Rigid.velocity;
        //clamp velocity
        VelAmt.y = 0;
        float AddAmt = VelAmt.magnitude; //get speed to add to our jump
        AddAmt = Mathf.Clamp(AddAmt, -MaxSpeedBoostOnJump, MaxSpeedBoostOnJump);

        Rigid.velocity = Vector3.zero;
        //add our jump force
        Vector3 ForceAmt = (transform.up * Upwards) + (transform.forward * (Forwards + AddAmt));
        Rigid.AddForce(ForceAmt, ForceMode.Impulse);
        //we are now in the air
        SetInAir();
    }

    //increase our fov at high speed and reduce it at low speed
    void HandleFov(float Del)
    {
        //get our velocity magniture
        //float mag = new Vector2(Rigid.velocity.x, Rigid.velocity.z).magnitude;
        //send to cam
        Cam.FovHandle(Del, ActSpeed);
    }

    //slide our character forwards
    void SlideSelf()
    {
        //reduce our speed
        ActSpeed = SpeedBeforeSlide;

        //find direction
        Vector3 Dir = Rigid.velocity.normalized;
        Dir.y = 0;

        //slide in direction
        Rigid.AddForce(transform.forward * SlideAmt, ForceMode.Impulse);

        //start sliding
        CurrentState = PlayerStates.Sliding;
        SlidingTimer = 0;

        //set visuals for slide
        Visual.Slide();
    }

    float CheckDis(float D)
    {
        if (SpeedCheckTime <= 0)
        {
            SpeedCheckTime = TimeBtwSpeedChecks;
            LastPosition = transform.position;
        }
        else
            SpeedCheckTime -= D;

        float Dis = Vector3.Distance(transform.position, LastPosition);

        if (Dis <= DistanceNeeded)
            return 0f;

        return 1;
    }

    public void Damage()
    {
        //cannot die when we are dead
        if (CurrentState == PlayerStates.Death)
            return;

        //set our animation state
        if(Anim)
            Anim.SetTrigger("Death");
        //set our respawn timer
        RespawnTimer = TimeBtwRespawn;
        //set our state
        CurrentState = PlayerStates.Death;

        //remove all velocity and set kinematic
        Rigid.velocity = Vector3.zero;
        Rigid.isKinematic = true;
        //remove our speed
        ActSpeed = 0;
    }

    void RespawnPlayer()
    {
        //respawn the player in our last respawn point
        Vector3 Pos = Vector3.zero;
        if (LastRespawn)
            Pos = LastRespawn.transform.position;
        //reset our position
        transform.position = Pos;
        //turn off kinematic body
        Rigid.isKinematic = false;
        //set on ground again
        SetOnGround();
    }

    public void CheckPointSet(RespawnPoint NewPoint)
    {
        //set last point as inactive (if there is one
        if(LastRespawn)
            LastRespawn.SetOff();
        //set our new respawn point
        LastRespawn = NewPoint;
    }
}
