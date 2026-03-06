using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObjects/Create Ability")]
public class AbilitiiesConfig : ScriptableObject
{
    [SerializeField]
    private Image _image;
    
    [SerializeField]
    private Text _name;
}
