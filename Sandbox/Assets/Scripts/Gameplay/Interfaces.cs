namespace SiS
{

    public interface IEventHandler<EventType>
    {
        void ReceiveEvent(EventType receivedEvent);
    }

    public struct DamageEvent
    {
        public float m_Damage;
    }
    public interface IDamageable : IEventHandler<DamageEvent> {};

}
