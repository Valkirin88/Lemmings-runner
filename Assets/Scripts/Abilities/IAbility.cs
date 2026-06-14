using System;

public interface IAbility
{
    
    event Action OnDeactivated;
    void Activate();
    void Update();
    void Deactivate();
    
}
