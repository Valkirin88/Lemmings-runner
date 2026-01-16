using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _currentQuantityText;
    

    public void ShowCurrentQuantity(int quantity)
    {
        _currentQuantityText.text = quantity.ToString();
    }
    
    
}
