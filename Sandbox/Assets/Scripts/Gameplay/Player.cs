using System;
using UnityEngine;

namespace SiS
{
    using System.Collections.Generic;
    using UnityEngine;

    internal enum ActionType
    {
        Jump, Forward, Back, Scratch, SuperJump, None
    }

    internal struct ActionEvent
    {
        public float m_TimeStamp;
        public ActionType m_Type;
        public bool m_ActionOn;
    }

    internal struct Combo
    {
        public ActionType[] Actions;
        public float[] Deltas;
        public Func<ActionEvent> OnCombo;
    }

    [Serializable]
    class ActionInterpreter
    {
		Combo[] Combos = new Combo[]
        {
            new Combo()
            {
                Actions = new ActionType[]{ActionType.Forward, ActionType.Back, ActionType.Jump},
                Deltas = new float[]{float.MaxValue,0.1f,0.1f},
                OnCombo = ()=>
                {
                    return new ActionEvent()
                    {
                        m_TimeStamp = Time.time,
                        m_Type = ActionType.SuperJump
                    };
                }
            },
            new Combo()
            {
                Actions = new ActionType[]{ActionType.Back, ActionType.Forward, ActionType.Jump},
                Deltas = new float[]{float.MaxValue,0.1f,0.1f},
                OnCombo = ()=>
                {
                    return new ActionEvent()
                    {
                        m_TimeStamp = Time.time,
                        m_Type = ActionType.SuperJump
                    };
                }
            }
        };        enum InputType
        {
            Jump, Right, Left, Attack, None
        }

        [SerializeField] float m_FilterWindow = 1.0f;
        [SerializeField] int m_MaxActionCount = 10;

        ActionEvent[] m_Actions;
        int m_ActionStart = 0;
        int m_ActionEnd = 0;

        Player m_Player;

        internal ActionEvent ActionFromBack(int i)
        {
            int index = m_ActionEnd - (i + 1);
            if(index < 0)
            {
                index = m_MaxActionCount + index;
            }
            return m_Actions[index];
        }

        private void AddAction(ActionEvent action)
        {
            // Only On Actions are added to our action queue
            if(action.m_ActionOn)
            {
                int nextIndex = (m_ActionEnd + 1) % m_MaxActionCount;
                Debug.Assert(nextIndex != m_ActionStart); // Reached the end of our buffer. Do we want to regrow it?
                    
                m_Actions[m_ActionEnd] = action;
                m_ActionEnd = nextIndex;
            }
        }
        internal void Init(Player player)
        {
            m_Actions = new ActionEvent[m_MaxActionCount];
            m_Player = player;
        }

        internal void InterpretActions()
        {
            List<(bool, InputType)> inputs = new List<(bool, InputType)>();

            // Read input type
            {
                if(Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump"))
                {
                    inputs.Add((true, InputType.Jump));
                }
                else if(Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump"))
                {
                    inputs.Add((false, InputType.Jump));
                }
                
                if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    inputs.Add((true, InputType.Right));
                }
                else if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    inputs.Add((true, InputType.Left));
                }

                if(Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
                {
                    inputs.Add((false, InputType.Right));
                }
                else if(Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
                {
                    inputs.Add((false, InputType.Left));
                }

                if(Input.GetKeyDown(KeyCode.Q))
                {
                    inputs.Add((true, InputType.Attack));
                }
            }

            // Interpret our inputs into actions based on the state of the player
            List<ActionEvent> newActions = new List<ActionEvent>();
            foreach((bool actionOn, InputType inputType) in inputs)
            {
                ActionEvent actionEvent = new ActionEvent()
                {
                    m_TimeStamp = Time.time,
                    m_ActionOn = actionOn
                };

                switch(inputType)
                {
                case InputType.Jump:
                    actionEvent.m_Type = ActionType.Jump;
                    break;
                case InputType.Right:
                    // TODO: Fix this, it doesn't work with our forward and back. We don't actually know what direction the player is facing.
                    actionEvent.m_Type = Vector3.Dot(m_Player.transform.right, Vector3.right) > 0.0f ? ActionType.Forward : ActionType.Back;
                    break;
                case InputType.Left:
                    actionEvent.m_Type = Vector3.Dot(m_Player.transform.right, Vector3.right) < 0.0f ? ActionType.Forward : ActionType.Back;
                    break;
                case InputType.Attack:
                    actionEvent.m_Type = ActionType.Scratch;
                    break;
                }

                newActions.Add(actionEvent);
            }

            for(int i = m_ActionStart; i != m_ActionEnd; i = (i + 1) % m_MaxActionCount)
            {
                if(Time.time - m_Actions[i].m_TimeStamp < m_FilterWindow)
                {
                    m_ActionStart = i;
                    break;
                }
            }

            foreach(ActionEvent action in newActions)
            {
                AddAction(action);
            }

            // Filter our actions to determine what combo we're activating
            {
                foreach(Combo combo in Combos)
                {
                    bool comboMet = true;
                    
                    float timeAtLastEvent = Time.time;
                    for(int i = 0; i < combo.Actions.Length; i++)
                    {
                        ActionEvent action = ActionFromBack(i);

                        ActionType comboType = combo.Actions[combo.Actions.Length - i - 1];
                        if(comboType != action.m_Type)
                        {
                            comboMet = false;
                            break;
                        }

                        if(timeAtLastEvent - action.m_TimeStamp > combo.Deltas[combo.Deltas.Length - i - 1])
                        {
                            comboMet = false;
                            break;
                        }
                        timeAtLastEvent = action.m_TimeStamp;
                    }

                    if(comboMet)
                    {
                        ActionEvent newAction = combo.OnCombo();
                        newActions.Add(newAction);
                        AddAction(newAction);
                    }
                }

                // TODO: We want to do some filtering here
                foreach(ActionEvent action in newActions)
                {
                    m_Player.OnAction(action);   
                }
            }
        }
    }



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
        public float m_defaultGravityScale;
        public float m_gravityScaleFastFallMultiplier;
        public float m_terminalVelocity;
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        [Flags]
        enum MovementState
        {
            Jumping = 0x01, Forward = 0x02, Back = 0x04
        }

        [SerializeField] Movement m_Movement = new Movement()
        {
            m_ForwardAcceleration = 18.0f,
            m_TurnAcceleration = 30.0f,
            m_MaxHorizontalVelocity = 5.0f,
            m_Deceleration = 20.0f,
            m_HighJumpHeight = 1.0f,
            m_LowJumpHeight = 0.3f,
            m_gravityScaleFastFallMultiplier = 2f,
            m_defaultGravityScale = 3f,
            m_terminalVelocity = -10.0f
        };

        [SerializeField] Rect m_AttackBox;
        [SerializeField] ActionInterpreter m_ActionInterpreter = new ActionInterpreter();

        private SpriteAnim m_Animator;
		private Rigidbody2D m_Rigidbody;
        private float m_InitialVelocity;

        private float m_TimeAtLastJumpStart = -1.0f;
        private MovementState m_MovementState;

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody2D>();
            m_Animator = GetComponent<SpriteAnim>();
            m_ActionInterpreter.Init(this);
        }

        private void Update()
        {
            m_ActionInterpreter.InterpretActions();
            if((m_MovementState & (MovementState.Forward | MovementState.Back)) != 0)
            {
                m_Animator.Play(0);
            }
            else
            {
                m_Animator.Play(1);
            }
        }

        private void FixedUpdate()
        {
            ProcessMovement();
        }

        private void OnDrawGizmos()
        {
            Vector3 attackOffset = new Vector3(m_AttackBox.x, m_AttackBox.y);
            Vector3 cubeSize = new Vector3(m_AttackBox.width, m_AttackBox.height, 1.0f);
            Gizmos.DrawWireCube(transform.position + attackOffset * Mathf.Sign(transform.localScale.x), cubeSize);
        }

        void ProcessMovement()
        {
            {
                if(m_TimeAtLastJumpStart > 0.0f && (m_MovementState & MovementState.Jumping) == 0) // TODO: Reverse this, so that the default is when we're not holding Jump.
                {                                                                                       // Holding Jump should give us more floatiness.
                    //float newGravity = -3.0f*(m_InitialVelocity*m_InitialVelocity)/(2.0f*m_Movement.m_LowJumpHeight);
                    //m_Rigidbody.gravityScale = newGravity / Physics2D.gravity.y;
                    m_Rigidbody.gravityScale = m_Movement.m_defaultGravityScale * m_Movement.m_gravityScaleFastFallMultiplier;
                    m_TimeAtLastJumpStart = 0.0f;
                }

                if(m_Rigidbody.velocity.y <= 0.0f)
                {
                    //m_Rigidbody.gravityScale = m_Movement.m_defaultGravityScale; TODO -> put this in a "Landing" function
                    float yVelocity = Mathf.Max(m_Rigidbody.velocity.y, m_Movement.m_terminalVelocity);
                    m_Rigidbody.velocity = new Vector2(m_Rigidbody.velocity.x, yVelocity);
                }
            }


            if((m_MovementState & (MovementState.Forward | MovementState.Back)) != 0)
            {
                float desiredDirection = 0.0f;
                if((m_MovementState & MovementState.Forward) != 0)
                {
                    desiredDirection = Vector3.Dot(transform.right, Vector3.right);
                }
                else if((m_MovementState & MovementState.Back) != 0)
                {
                    desiredDirection = Vector3.Dot(transform.right, Vector3.left);
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

        internal void OnAction(ActionEvent actionEvent)
        {
            // Horizontal Movement
            {
                if(actionEvent.m_Type == ActionType.Forward)
                {
                    m_MovementState = actionEvent.m_ActionOn ? m_MovementState | MovementState.Forward : m_MovementState & ~MovementState.Forward;
                }
                else if(actionEvent.m_Type == ActionType.Back)
                {
                    m_MovementState = actionEvent.m_ActionOn ? m_MovementState | MovementState.Back : m_MovementState & ~MovementState.Back;
                }
            }

            // Jumping
            {
                if(actionEvent.m_Type == ActionType.Jump)
                {
                    if(actionEvent.m_ActionOn)
                    {
                        m_TimeAtLastJumpStart = Time.time;

                        float jumpVelocity = Mathf.Sqrt(-2.0f * Physics2D.gravity.y * m_Movement.m_HighJumpHeight);
                        Vector2 velocity = m_Rigidbody.velocity;
                        velocity.y = jumpVelocity;
                        m_InitialVelocity = jumpVelocity;
                        m_Rigidbody.velocity = velocity;
                        m_Rigidbody.gravityScale = m_Movement.m_defaultGravityScale;

                         m_MovementState |= MovementState.Jumping;
                    }
                    else
                    {
                        m_MovementState &= ~MovementState.Jumping;
                    }
                    
                }
                else if(actionEvent.m_Type == ActionType.SuperJump)
                {
                    m_Rigidbody.gravityScale = 0.01f;
                }
            }

            // Attack Type
            {
                if(actionEvent.m_Type == ActionType.Scratch)
                {
                    Collider2D collision = Physics2D.OverlapBox(
                        m_Rigidbody.position + m_AttackBox.position * Mathf.Sign(transform.localScale.x),
                        m_AttackBox.size, 0.0f,
                        LayerMask.GetMask("Interactive"));
                    if(collision != null)
                    {
                        IDamageable damageTarget = collision.GetComponent<IDamageable>();
                        if(damageTarget != null)
                        {
                            damageTarget.ReceiveEvent(new DamageEvent() { m_Damage = 0.0f });
                        }
                    }
                }
            }
        }
    }

}
