using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float m_ForwardSpeed = 1.0f;

    private Rigidbody2D m_Rigidbody = null;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
       if(Input.GetKey(KeyCode.D))
       {
           m_Rigidbody.velocity += Vector2.right * m_ForwardSpeed;
       }
       else if(Input.GetKey(KeyCode.A))
       {
           m_Rigidbody.velocity += Vector2.left * m_ForwardSpeed;
       }
       else
       {
           m_Rigidbody.velocity *= Vector2.up;
       }
    }
}
