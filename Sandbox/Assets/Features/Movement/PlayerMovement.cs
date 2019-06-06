using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float m_ForwardSpeed = 1.0f;

    [SerializeField]
    private float m_SideSpeed = 1.0f;

    [SerializeField]
    private float m_RotationSpeed = 1.0f;

    private Rigidbody m_Rigidbody = null;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Basic movement
        {
            float forward = 0.0f;
            float right = 0.0f;
            // Basic input, if we need something more complex, we'll add it then.
            if(Input.GetKey(KeyCode.W))
            {
                forward = 1.0f;
            }
            else if(Input.GetKey(KeyCode.S))
            {
                forward = -1.0f;
            }

            if(Input.GetKey(KeyCode.D))
            {
                right = 1.0f;
            }
            else if(Input.GetKey(KeyCode.A))
            {
                right = -1.0f;
            }

            Vector3 forwardDir = transform.forward * forward;
            Vector3 rightDir = transform.right * right;
            Vector3 moveDir = Vector3.Normalize(forwardDir + rightDir);

            float forwardMag = Mathf.Abs(Vector3.Dot(moveDir, forwardDir)) * m_ForwardSpeed;
            float rightMag = Mathf.Abs(Vector3.Dot(moveDir, rightDir)) * m_SideSpeed;

            float verticalVelocity = m_Rigidbody.velocity.y;
            Vector3 moveVec = moveDir * (forwardMag + rightMag) + Vector3.up * verticalVelocity;

            m_Rigidbody.velocity = moveVec;
        }

        // basic rotation
        {
            float horizontal = Input.GetAxis("Horizontal");
            m_Rigidbody.angularVelocity = Vector3.up * horizontal * m_RotationSpeed;
        }
    }
}
