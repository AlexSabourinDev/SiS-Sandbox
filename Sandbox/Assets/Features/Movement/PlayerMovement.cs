using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    enum InputType
    {
        Right,
        Left,
        Jump,
        None,
    }

    enum MovementState
    {
        Move,
        Turn,
        Idle,
    }

    [SerializeField]
    private float m_MoveSpeed = 1.0f;

    [SerializeField]
    private float m_TurnTime = 1.0f;

    MovementState m_MovementState;

    private Rigidbody2D m_Rigidbody;

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
        else if(Input.GetKey(KeyCode.Space))
        {
            return InputType.Jump;
        }

        return InputType.None;
    }

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        InputType inputType = ReadInput();
        if(inputType == InputType.Jump)
        {
            // TODO: If falling:
            //  - Freak out (TODO watch videos of cats falling)
            //  - Jump if jump is pressed close enough to the ground
            //  - Jump when falling might change the angle
        }

        // TODO:
        //  - Movement left and right

        m_MovementState = MovementState.Idle;

        float currentVelocity = m_Rigidbody.velocity.x;
        float nextVelocity = 0.0f;
        if(inputType == InputType.Right)
        {
            nextVelocity = m_MoveSpeed;

            float sign = Mathf.Sign(currentVelocity);
            if(sign != 0.0f && sign == -1.0f) // Are we not going right?
            {
                m_MovementState = MovementState.Turn;
            }
            else
            {
                m_MovementState = MovementState.Move;
            }
        }
        else if(inputType == InputType.Left)
        {
            nextVelocity = -m_MoveSpeed;

            float sign = Mathf.Sign(currentVelocity);
            if(sign != 0.0f && sign == 1.0f) // Are we not going right?
            {
                m_MovementState = MovementState.Turn;
            }
            else
            {
                m_MovementState = MovementState.Move;
            }
        }

        if(m_MovementState == MovementState.Move)
        {
            m_Rigidbody.velocity = new Vector2(nextVelocity, m_Rigidbody.velocity.y);
        }
        else if(m_MovementState == MovementState.Turn)
        {
            float acceleration = m_MoveSpeed * 2.0f * Mathf.Sign(nextVelocity) / m_TurnTime;
            m_Rigidbody.AddForce(Vector2.right * acceleration * m_Rigidbody.mass);

            if(Mathf.Abs(m_Rigidbody.velocity.x) >= Mathf.Abs(nextVelocity))
            {
                m_MovementState = MovementState.Move;
            }
        }
        else
        {
            float acceleration = m_MoveSpeed * -Mathf.Sign(currentVelocity) / m_TurnTime;
            m_Rigidbody.AddForce(Vector2.right * acceleration * m_Rigidbody.mass);

            if(Mathf.Abs(currentVelocity) < 0.01f)
            {
                m_Rigidbody.velocity = new Vector2(0.0f, m_Rigidbody.velocity.y);
            }
        }
    }
}
