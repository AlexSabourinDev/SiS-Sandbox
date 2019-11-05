using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cat video log: https://youtu.be/rNSnfXl1ZjU?t=555
// Note: Cuccumbers!

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    enum JumpType
    {
        PressJump,
        HoldJump,
        ReleaseJump,
        None,
    }

    enum InputType
    {
        Right,
        Left,
        None,
    }

    enum MovementState
    {
        Move,
        Turn,
        Idle,
    }

    [SerializeField]
    private float m_ForwardAcceleration = 5.0f;

    [SerializeField]
    private float m_TurnAcceleration = 10.0f;

    [SerializeField]
    private float m_MaxHorizontalVelocity = 10.0f;

    [SerializeField]
    private float m_HighJumpHeight = 1.0f;

    [SerializeField]
    private float m_LowJumpHeight = 0.3f;

    [SerializeField]
    private float m_JumpWindow = 0.1f;

    MovementState m_MovementState;
    private Rigidbody2D m_Rigidbody;
    private SpriteRenderer m_Sprite;
    private float m_InitialVelocity;


    private float m_TimeAtLastJumpStart = -1.0f;

    private JumpType ReadJump()
    { 
        if(Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump"))
        {
            return JumpType.PressJump;
        }
        else if(Input.GetKey(KeyCode.Space) || Input.GetButton("Jump"))
        {
            return JumpType.HoldJump;
        }
        else if(Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump"))
        {
            return JumpType.ReleaseJump;
        }

        return JumpType.None;
    }

    private InputType ReadInput()
    {
        if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            return InputType.Right;
        }
        else if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            return InputType.Left;
        }

        return InputType.None;
    }

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        InputType input = ReadInput();

        // Horizontal Movement
        {
            float desiredDirection = 0.0f;

            if(input == InputType.Right)
            {
                desiredDirection = 1.0f;
            }
            else if(input == InputType.Left)
            {
                desiredDirection = -1.0f;
            }

            float desiredAcceleration = m_ForwardAcceleration;
            bool differentDirection = Mathf.Sign(m_Rigidbody.velocity.x) != desiredDirection;
            if(differentDirection)
            {
                desiredAcceleration = m_TurnAcceleration;
            }

            if(Mathf.Abs(m_Rigidbody.velocity.x) < m_MaxHorizontalVelocity)
            {
                Vector2 acceleration = Vector2.right * desiredAcceleration * desiredDirection;
                m_Rigidbody.AddForce(acceleration * m_Rigidbody.mass);
            }

            if (desiredDirection != 0.0f){
                transform.localScale = desiredDirection > 0? Vector3.one : new Vector3(-1, 1, 1);
            }
        }

        // Jumping
        {
            JumpType jumpType = ReadJump();
            if(jumpType == JumpType.PressJump)
            {
                m_TimeAtLastJumpStart = Time.time;

                float jumpVelocity = Mathf.Sqrt(-2.0f * Physics2D.gravity.y * m_HighJumpHeight);
                Vector2 velocity = m_Rigidbody.velocity;
                velocity.y = jumpVelocity;
                m_InitialVelocity = jumpVelocity;
                m_Rigidbody.velocity = velocity;
                m_Rigidbody.gravityScale = 1.0f;
            }
            
            if(m_TimeAtLastJumpStart > 0.0f && jumpType != JumpType.HoldJump && jumpType != JumpType.PressJump)
            {
                float newGravity = -3.0f*(m_InitialVelocity*m_InitialVelocity)/(2.0f*m_LowJumpHeight);
                m_Rigidbody.gravityScale = newGravity / Physics2D.gravity.y;

                m_TimeAtLastJumpStart = 0.0f;
            }

            if(m_Rigidbody.velocity.y <= 0.0f)
            {
                m_Rigidbody.gravityScale = 1.0f;
            }
        }
    }
}
