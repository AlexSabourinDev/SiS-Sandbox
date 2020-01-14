using System;
using UnityEngine;

// Cat video log: https://youtu.be/rNSnfXl1ZjU?t=555
// Note: Cuccumbers!

[Serializable]
public struct Movement
{
    public float m_ForwardAcceleration;
    public float m_TurnAcceleration;
    public float m_MaxHorizontalVelocity;
    public float m_Deceleration;
    public float m_HighJumpHeight;
    public float m_LowJumpHeight;
}

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

    enum MotionDirection
    {
        Right,
        Left,
        None,
    }

    enum AttackType
    {
        Scratch,
        None,
    }

    [SerializeField] Movement m_Movement = new Movement()
    {
        m_ForwardAcceleration = 18.0f,
        m_TurnAcceleration = 30.0f,
        m_MaxHorizontalVelocity = 5.0f,
        m_Deceleration = 20.0f,
        m_HighJumpHeight = 1.0f,
        m_LowJumpHeight = 0.3f
    };

    [SerializeField] Rect m_AttackBox;

    private Rigidbody2D m_Rigidbody;
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

    private MotionDirection ReadMotion()
    {
        if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            return MotionDirection.Right;
        }
        else if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            return MotionDirection.Left;
        }

        return MotionDirection.None;
    }

    private AttackType ReadAttack()
    {
        return Input.GetKeyDown(KeyCode.Q) ? AttackType.Scratch : AttackType.None;
    }

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        MotionDirection desiredMotion = ReadMotion();

        // Horizontal Movement
        {
            if(desiredMotion != MotionDirection.None)
            {
                float desiredDirection = 0.0f;

                if(desiredMotion == MotionDirection.Right)
                {
                    desiredDirection = 1.0f;
                }
                else if(desiredMotion == MotionDirection.Left)
                {
                    desiredDirection = -1.0f;
                }

                float desiredAcceleration = m_Movement.m_ForwardAcceleration;
                bool differentDirection = Mathf.Sign(m_Rigidbody.velocity.x) != desiredDirection;
                if(differentDirection)
                {
                    desiredAcceleration = m_Movement.m_TurnAcceleration;
                }

                if(Mathf.Abs(m_Rigidbody.velocity.x) < m_Movement.m_MaxHorizontalVelocity || differentDirection)
                {
                    Vector2 acceleration = Vector2.right * desiredAcceleration * desiredDirection;
                    m_Rigidbody.AddForce(acceleration * m_Rigidbody.mass);
                }

                if (desiredDirection != 0.0f)
                {
                    transform.localScale = desiredDirection > 0 ? Vector3.one : new Vector3(-1, 1, 1);
                }
            }
            else
            {
                // TODO: We might have to look at this later if motion is controlled by external factors.
                if(Mathf.Abs(m_Rigidbody.velocity.x) > 0.3f)
                {
                    float decelerationDirection = Mathf.Sign(m_Rigidbody.velocity.x) * -1.0f;
                    m_Rigidbody.AddForce(Vector2.right * m_Movement.m_Deceleration * m_Rigidbody.mass * decelerationDirection);
                }
                else
                {
                    m_Rigidbody.velocity = new Vector2(0.0f, m_Rigidbody.velocity.y);
                }
            }
        }

        // Jumping
        {
            JumpType jumpType = ReadJump();
            if(jumpType == JumpType.PressJump)
            {
                m_TimeAtLastJumpStart = Time.time;

                float jumpVelocity = Mathf.Sqrt(-2.0f * Physics2D.gravity.y * m_Movement.m_HighJumpHeight);
                Vector2 velocity = m_Rigidbody.velocity;
                velocity.y = jumpVelocity;
                m_InitialVelocity = jumpVelocity;
                m_Rigidbody.velocity = velocity;
                m_Rigidbody.gravityScale = 1.0f;
            }
            
            if(m_TimeAtLastJumpStart > 0.0f && jumpType != JumpType.HoldJump && jumpType != JumpType.PressJump)
            {
                float newGravity = -3.0f*(m_InitialVelocity*m_InitialVelocity)/(2.0f*m_Movement.m_LowJumpHeight);
                m_Rigidbody.gravityScale = newGravity / Physics2D.gravity.y;

                m_TimeAtLastJumpStart = 0.0f;
            }

            if(m_Rigidbody.velocity.y <= 0.0f)
            {
                m_Rigidbody.gravityScale = 1.0f;
            }
        }

        // Attack Type
        {
            AttackType attackType = ReadAttack();
            if(attackType == AttackType.Scratch)
            {

            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 attackOffset = new Vector3(m_AttackBox.x, m_AttackBox.y);
        Vector3 cubeSize = new Vector3(m_AttackBox.width, m_AttackBox.height, 1.0f);
        Gizmos.DrawWireCube(transform.position + attackOffset, cubeSize);
    }
}
