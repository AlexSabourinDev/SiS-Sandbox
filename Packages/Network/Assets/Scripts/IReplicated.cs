namespace Game.Networking
{
    /// <summary>
    /// Classes that implement this interface can delcare a static method
    /// to register information regarding network replication.
    /// 
    /// static void StaticNetInitialize(ReplicationType type);
    /// 
    /// </summary>
    public interface IReplicated
    {
        uint NetworkID { get; set; }
        ReplicationType NetworkType { get; set; }
    }
}
