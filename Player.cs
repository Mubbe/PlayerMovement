using Godot;
using System;
using System.Diagnostics;
//using System.Numerics;

public partial class Player : CharacterBody3D
{
    public const float Speed = 5.0f;
    public const float WalkSpeed = 3.5f;
    public const float JumpVelocity = 4.5f;

    public const float StandingHight = 1.0f;
    public const float CrouchingHight = 0.5f;
    public bool Crouching = false;
    public bool jumping = false;
    public bool hasJumped = false;
    public bool coyote = false;



    [Export] public Node3D meshroot;
    [Export] public CollisionShape3D CollisonHead;
    [Export] public Timer delayjump;
    [Export] public Node3D Pivot;
    [Export] public Node3D CameraPivot;
    [Export]public AnimationPlayer anime;
    [Export] public float hoverTime=1f;
    [Export] public float hovergraph = 1f;

    public enum SpeedMode
    {
        Instant,
        Linear,
        Ease
    }

    [Export] public SpeedMode motionMode = SpeedMode.Linear;

    [Export] public float accel = 10f;
    [Export] public float decel = 20f;

    [Export] public float minAccel = 2f;
    [Export] public float maxAccel = 12f;


    float Hover;


    public Vector3 direction = Vector3.Zero;


    [Export]
    Curve curve;

    private float timer = 0.0f;

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        if (Input.IsActionJustPressed("Crouch"))
        {
            Crouching = true;
            meshroot.Scale = new Vector3(1, CrouchingHight, 1);
        
            CollisonHead.Disabled = true;
        }
        if(Input.IsActionJustReleased("Crouch"))
        {
            Crouching = false;
            meshroot.Scale = new Vector3(1, StandingHight, 1);
            CollisonHead.Disabled = false;
        }

      
        if(IsOnFloor())
        {
            if(Hover > 0.01f)
            {
                hasJumped = false;
            }
            Hover = 0;
            coyote = false;
        }
        else
        {
            float d = (float)delta;
            Hover += d;
            // statement ? ifTrue : ifFalse
            velocity.Y += GetGravity().Y * ((Hover < 0.5f) ? d : ((Hover < hoverTime) ? (hovergraph * d) : (d * 3.0f)));  // This is if and else:
        }

        if (Input.IsActionJustPressed("Jump") && (Hover < 0.5f) && !hasJumped)
        {
            jumping = true;
            coyote = !IsOnFloor();
            delayjump.Start();

            meshroot.Scale = new Vector3(1, CrouchingHight, 1);// mesh scale down
        }
        if (Input.IsKeyPressed(Key.Key1))
        {
            motionMode = SpeedMode.Instant;
            GD.Print("INstant");
        }
           

        if (Input.IsKeyPressed(Key.Key2))
        {
            motionMode = SpeedMode.Linear;
            GD.Print("Linear");
        }
            

        if (Input.IsKeyPressed(Key.Key3))
        {
            motionMode = SpeedMode.Ease;
            GD.Print("Ease");
        }
            




        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 inputDir = Input.GetVector("Left", "Right", "Up", "Down");
        direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            float angle = Mathf.Atan2(-direction.X, -direction.Z);

            meshroot.Rotation = new Vector3(0,angle,0);
        }

        float currentSpeed = Crouching ? WalkSpeed : Speed;
        Vector3 targetVelocity = direction * currentSpeed;
        

        switch (motionMode)
        {
            case SpeedMode.Instant:
                if (direction != Vector3.Zero)
                {
                    velocity.X = targetVelocity.X;
                    velocity.Z = targetVelocity.Z;
                }
                else
                {
                    velocity.X = 0;
                    velocity.Z = 0;
                }
                break;

            case SpeedMode.Linear:
                velocity.X = Mathf.MoveToward(velocity.X, targetVelocity.X, accel * (float)delta);
                velocity.Z = Mathf.MoveToward(velocity.Z, targetVelocity.Z, accel * (float)delta);

                if (direction == Vector3.Zero)
                {
                    velocity.X = Mathf.MoveToward(velocity.X, 0, decel * (float)delta);
                    velocity.Z = Mathf.MoveToward(velocity.Z, 0, decel * (float)delta);
                }
                break;

            case SpeedMode.Ease:
                if (direction != Vector3.Zero)
                {
                    float speedPercent = new Vector2(velocity.X, velocity.Z).Length() / Speed;
                    float eased = speedPercent * speedPercent; // ease-in curve

                    float currentAccel = Mathf.Lerp(minAccel, maxAccel, eased);

                    velocity.X = Mathf.MoveToward(velocity.X, targetVelocity.X, currentAccel * (float)delta);
                    velocity.Z = Mathf.MoveToward(velocity.Z, targetVelocity.Z, currentAccel * (float)delta);
                }
                else
                {
                    velocity.X = Mathf.MoveToward(velocity.X, 0, decel * (float)delta);
                    velocity.Z = Mathf.MoveToward(velocity.Z, 0, decel * (float)delta);
                }
                break;
        }


        Velocity = velocity;
        MoveAndSlide();
        
    }
    void Delaytimer()
    {
        Vector3 velocity = Velocity;
        velocity.Y = JumpVelocity;
        jumping = false;
        hasJumped = true;
        Velocity = velocity;

        meshroot.Scale = new Vector3(1, StandingHight, 1);
    }





}
