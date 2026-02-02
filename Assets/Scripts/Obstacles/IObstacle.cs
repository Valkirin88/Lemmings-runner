
public interface IObstacle 
{
    BloodZone BloodZone { get; }
    
    /// <summary>
    /// Вызывает появление пятен крови в зоне препятствия
    /// </summary>
    void SpawnBlood();
}
