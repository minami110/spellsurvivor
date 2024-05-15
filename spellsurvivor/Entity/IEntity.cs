#nullable enable
namespace spellsurvivor;

public enum Race
{
    Player,
    Slime,
    Bat
}

public interface IEntity
{
    public Race Race { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public float MaxHealth { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public float Health { get; }
    
    public void TakeDamage(float amount, IEntity? instigator);
}