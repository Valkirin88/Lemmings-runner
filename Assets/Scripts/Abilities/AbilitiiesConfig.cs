using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Create Ability")]
public class AbilitiiesConfig : ScriptableObject
{
    [SerializeField]
    private AbilityId _abilityId;
    
    [SerializeField]
    private Sprite _image;
    
    [SerializeField]
    private string _name;

    [SerializeField]
    private float _durationTime;

    public AbilityId AbilityId => _abilityId;
    public Sprite Image => _image;
    public string Name => _name;
    public float DurationTime => _durationTime;

}
