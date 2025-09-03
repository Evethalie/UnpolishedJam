using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")] public PlayerMovementStats moveStats;
    [SerializeField] private Collider2D feetColl;
    [SerializeField] private Collider2D bodyColl;
    private VelocityMods mods;

    private Rigidbody2D rb;

    private Vector2 moveVelocity;
    private bool isFacingRight;

    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;

    private bool isGrounded;
    private bool bumpedHead;

    //Jump Vars
    public float VerticalVelocity { get; private set; }
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int numberOfJumpsUsed;

    //Apex Vars
    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    //Jump Buffer vars
    private float jumpBufferTimer;
    private bool jumpReleasedDuringBuffer;

    private float coyoteTimer;
    private float ignoreGroundFor;

    #region Unity Functions

    private void Awake()
    {
        isFacingRight = true;
        rb = GetComponent<Rigidbody2D>();
        mods = GetComponent<VelocityMods>();
    }

    private void Update()
    {
        CountTimers();
        if (ignoreGroundFor > 0f) ignoreGroundFor -= Time.deltaTime;
        JumpChecks();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        if (isGrounded)
        {
            Move(moveStats.GroundAcceleration, moveStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(moveStats.AirAcceleration, moveStats.AirDeceleration, InputManager.Movement);
        }
        Vector2 baseVel = new Vector2(moveVelocity.x, VerticalVelocity);


        if (mods != null)
        {
            baseVel.x += mods.addX;
            baseVel.y += mods.addY;

           
            baseVel.x += mods.impulseX;
            baseVel.y += mods.impulseY;

            
            if (mods.setYOnce) baseVel.y = mods.setY;
        }


        baseVel.y = Mathf.Max(baseVel.y, -moveStats.MaxFallSpeed);


        rb.linearVelocity = baseVel;

        mods?.ClearOneFrame();
    }

    #endregion

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);
            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0) * moveStats.MaxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x, 0) * moveStats.MaxWalkSpeed;
            }

            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
           // rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }

        else if (moveInput == Vector2.zero)
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
          //  rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Jump

    private void JumpChecks()
    {
        if (InputManager.JumpPressed)
        {
            jumpBufferTimer = moveStats.JumpBufferTime;
            jumpReleasedDuringBuffer = false;
        }

        if (InputManager.JumpReleased)
        {
            if (jumpBufferTimer > 0f)
            {
                jumpReleasedDuringBuffer = true;
            }

            if (isJumping && VerticalVelocity > 0f)
            {
                if (isPastApexThreshold)
                {
                    isPastApexThreshold = false;
                    isFastFalling = true;
                    fastFallTime = moveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    isFastFalling = true;
                    fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        //Initiate Jump with jump buffering and coyote time
        if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (jumpReleasedDuringBuffer)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        //double jump
        else if (jumpBufferTimer > 0f && isJumping && numberOfJumpsUsed < moveStats.NumberOfJumpsAllowed - 1)
        {
            isFastFalling = false;
            InitiateJump(1);
        }
        
        //Air jump after coyote time lapsed
        else if (jumpBufferTimer > 0f && isFalling && numberOfJumpsUsed < moveStats.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            isFastFalling = false;
        }
        //Landed
        if ((isJumping || isFalling) && isGrounded && VerticalVelocity <= 0f)
        {
            isJumping = false;
            isFalling = false;
            isFastFalling = false;
            fastFallTime = 0;
            isPastApexThreshold = false;
            numberOfJumpsUsed = 0;
            VerticalVelocity = 0f;
        }
        
    }

    private void InitiateJump(int numberOfJumpsUsedUp)
    {
        if (!isJumping)
        {
            isJumping = true;
        }

        jumpBufferTimer = 0f;
        numberOfJumpsUsed += numberOfJumpsUsedUp;
        VerticalVelocity = moveStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        //Apply gravity while jumping
        if (isJumping)
        {
            //Check for head bump
            if (bumpedHead)
            {
                isFastFalling = true;
            }
            
            //Gravity on ascending
            if (VerticalVelocity >= 0f)
            {
                //apex controls
                apexPoint = Mathf.InverseLerp(moveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (apexPoint > moveStats.ApexThreshold)
                {
                    if (!isPastApexThreshold)
                    {
                        isPastApexThreshold = true;
                        timePastApexThreshold = 0f;
                    }

                    if (isPastApexThreshold)
                    {
                        timePastApexThreshold += Time.fixedDeltaTime;
                        if (timePastApexThreshold < moveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
                //Gravity on Descending but not past apex threshold
                else
                {
                    VerticalVelocity += moveStats.Gravity * Time.fixedDeltaTime;
                    if (isPastApexThreshold)
                    {
                        isPastApexThreshold = false;
                    }
                }
            }
            //Gravity on Descending
            else if (!isFastFalling)
            {
                VerticalVelocity += moveStats.Gravity *  Time.fixedDeltaTime;
            }
            else if (VerticalVelocity < 0f)
            {
                if (!isFalling)
                {
                    isFalling = true;
                }
            }
           
        }
      
        //Jump Cut
        if (isFastFalling)
        {
            if (fastFallTime >= moveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += moveStats.Gravity * moveStats.GravityOnReleaseMultiplier *  Time.fixedDeltaTime;
            }
            else if(fastFallTime < moveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, (fastFallTime / moveStats.TimeForUpwardsCancel));
            }
            fastFallTime += Time.fixedDeltaTime;
        }
        
        //Normal Gravity while falling
        if (!isGrounded && !isJumping)
        {
            if (!isFalling)
            {
                isFalling = true;
            }
            
            VerticalVelocity += moveStats.Gravity * Time.fixedDeltaTime;
        }
        
        //Clamp Fall Speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -moveStats.MaxFallSpeed, 50f);
        //rb.linearVelocity = new Vector2(rb.linearVelocity.x, VerticalVelocity);
    }
    
    public void ApplyBounce(float bounceY)
    {
        // override vertical for this frame and the next
        VerticalVelocity = Mathf.Max(VerticalVelocity, bounceY);

        // clear conflicting states
        isFastFalling = false;
        isFalling = false;
        isJumping = true;           // treat as an air state so gravity logic runs

        // prevent ground logic from zeroing Y on this tick
        ignoreGroundFor = 0.06f;    // ~3-4 physics ticks at 60Hz
    }

    #endregion

    #region CollisionChecks

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(feetColl.bounds.center.x, feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetColl.bounds.size.x, moveStats.GroundDetectionRayLength);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, moveStats.GroundDetectionRayLength,
            moveStats.GroundLayer);
        bool candidate = groundHit.collider != null && VerticalVelocity <= 0.05f;

        
        if (ignoreGroundFor > 0f) candidate = false;

        isGrounded = candidate;
        if (moveStats.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if (isGrounded)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y),
                Vector2.down * moveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y),
                Vector2.down * moveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(
                new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - moveStats.GroundDetectionRayLength),
                Vector2.right * boxCastSize.x, rayColor);
        }
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(feetColl.bounds.center.x, bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(feetColl.bounds.size.x * moveStats.HeadWidth, moveStats.HeadDetectionRayLength);

        headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, moveStats.HeadDetectionRayLength,
            moveStats.GroundLayer);
        if (headHit.collider != null)
        {
            bumpedHead = true;
        }
        else
        {
            bumpedHead = false;
        }

        if (moveStats.DebugShowHeadBumpBox)
        {
            float headWidth = moveStats.HeadWidth;
            Color rayColor;
            if (bumpedHead)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y),
                Vector2.up * moveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y),
                Vector2.up * moveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(
                new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + moveStats.HeadDetectionRayLength),
                Vector2.right * boxCastSize.x * headWidth, rayColor);
        }
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }

    #endregion
    
    #region Timers

    private void CountTimers()
    {
        jumpBufferTimer -= Time.deltaTime;

        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }
        else
        {
            coyoteTimer = moveStats.JumpCoyoteTime;
        }
    }
    
    #endregion
    
    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPosition = new Vector2(feetColl.bounds.center.x, feetColl.bounds.min.y);
        Vector2 previousPosition = startPosition;
        float speed =  moveSpeed;
        Vector2 velocity = new Vector2(speed, moveStats.InitialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * moveStats.TimeTillJumpApex / moveStats.ArcResolution; 
        float totalApexTime =
            (2 * moveStats.TimeTillJumpApex) + moveStats.ApexHangTime; 

        for (int i = 0; i < moveStats.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if (simulationTime <= moveStats.TimeTillJumpApex) 
            {
                displacement = velocity * simulationTime +
                               0.5f * new Vector2(0, moveStats.Gravity) * simulationTime * simulationTime;
            }
            else if (simulationTime < moveStats.TimeTillJumpApex + moveStats.ApexHangTime) // Apex hang time
            {
                float apexTime = simulationTime - moveStats.TimeTillJumpApex;
                displacement = velocity * moveStats.TimeTillJumpApex
                               + 0.5f * new Vector2(0, moveStats.Gravity) * moveStats.TimeTillJumpApex *
                               moveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime; // No vertical movement during hang time
            }
            else // Descending
            {
                float descendTime = simulationTime - moveStats.TimeTillJumpApex - moveStats.ApexHangTime;
                displacement = velocity * moveStats.TimeTillJumpApex
                               + 0.5f * new Vector2(0, moveStats.Gravity) * moveStats.TimeTillJumpApex *
                               moveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * moveStats.ApexHangTime; // Horizontal movement during hang time
                displacement += new Vector2(speed, 0) * descendTime
                                + 0.5f * new Vector2(0, moveStats.Gravity) * descendTime * descendTime;
            }

            drawPoint = startPosition + displacement;

            if (moveStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition,
                    Vector2.Distance(previousPosition, drawPoint), moveStats.GroundLayer);
                if (hit.collider != null)
                {
                    // If a hit is detected, stop drawing the arc at the hit point
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }

    private void OnDrawGizmos()
    {
        if (moveStats.ShowWalkJumpArc)
        {
            DrawJumpArc(moveStats.MaxWalkSpeed, Color.magenta);
        }

        if (moveStats.ShowRunJumpArc)
        {
            DrawJumpArc(moveStats.MaxRunSpeed, Color.magenta);
        }
    }
       
}