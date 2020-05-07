using UnityEngine;

namespace SiS
{

    [RequireComponent(typeof(Rigidbody2D))]
    public class Vase : MonoBehaviour, IDamageable
    {
        [SerializeField] private PhysicsMaterial2D m_BounceMaterial;
        [SerializeField] private float m_PushRotation = 1.5f;

        private bool m_Falling = false;

        public void ReceiveEvent(DamageEvent damageEvent)
        {
            gameObject.layer = LayerMask.NameToLayer("Fallthrough");
            m_Falling = true;

            Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
            rigidbody.sharedMaterial = m_BounceMaterial;
            rigidbody.AddTorque(m_PushRotation);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(m_Falling)
            {
                Break();
            }
        }

        private void Break()
        {

        }
    }

}