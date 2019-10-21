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
    private float m_ForwardAcceleration = 5.0f;

    [SerializeField]
    private float m_TurnAcceleration = 10.0f;

    [SerializeField]
    private float m_MaxHorizontalVelocity = 10.0f;

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

    private void Update()
    {
        float desiredDirection = 0.0f;

        InputType input = ReadInput();
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
}
