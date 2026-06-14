
public interface IObstacle 
{
    BloodZone BloodZone { get; }
    void SpawnBlood();
    
    void MakeSound();
    void OnDestroy();
}
