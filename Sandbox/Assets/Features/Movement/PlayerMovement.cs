using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cat video log: https://youtu.be/rNSnfXl1ZjU?t=555
// Note: Cuccumbers!

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    enum InputType
    {
        Right,
        Left,
        PressJump,
        ReleaseJump,
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
    private float m_HopJumpHeight = 0.1f;

    [SerializeField]
    private float m_JumpWindow = 0.1f; // TODO: Rename

    MovementState m_MovementState;
    private Rigidbody2D m_Rigidbody;

    private float m_TimeAtLastJumpStart = -1.0f;

    private InputType ReadInput()
    {
        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            return InputType.Right;
        }
        else if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            return InputType.Left;
        }
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            return InputType.PressJump;
        }
        else if(Input.GetKeyUp(KeyCode.Space))
        {
            return InputType.ReleaseJump;
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
        }

        // Jumping
        {
            if(input == InputType.PressJump)
            {
                m_TimeAtLastJumpStart = Time.time;
            }
            
            if(m_TimeAtLastJumpStart > 0.0f)
            {
                float jumpTime = Time.time - m_TimeAtLastJumpStart;
                if(input == InputType.ReleaseJump || jumpTime >= m_JumpWindow)
                {
                    float height = m_HighJumpHeight;
                    if(jumpTime < m_JumpWindow)
                    {
                        height = m_HopJumpHeight;
                    }

                    float jumpVelocity = Mathf.Sqrt(-2.0f * Physics2D.gravity.y * height);
                    Vector2 velocity = m_Rigidbody.velocity;
                    velocity.y = jumpVelocity;
                    m_Rigidbody.velocity = velocity;

                    m_TimeAtLastJumpStart = -1.0f;
                }
            }
        }
    }
}
