using UnityEngine;
using UnityEngine.UI;

public class AccelerateButton : MonoBehaviour
{
   [SerializeField]
   private Button _accelerateButton;
   
   [SerializeField]
   private Image _cooldownImage;
   
   [SerializeField]
   private float _cooldownTime = 3f;

   private float _cooldownTimer;
   private bool _isPushed;
   
   private void Start()
   {
      _accelerateButton.interactable = true;
      _cooldownImage.fillAmount = 0;
      
      _accelerateButton.onClick.AddListener(StartTimer);
   }

   private void StartTimer()
   {
      Debug.Log("Accelerate");
      if (!_isPushed)
      {
         _isPushed = true;
         _cooldownImage.fillAmount = 1;
         _cooldownTimer = _cooldownTime;
         _accelerateButton.interactable = false;
      }
   }

   private void StopTimer()
   {
      _cooldownImage.fillAmount = 0;
      _cooldownTimer = 0;
      _isPushed = false;
      _accelerateButton.interactable = true;
   }

   private void Update()
   {
      if (!_isPushed) return;
      
      if (_cooldownTimer > 0)
      {
         _cooldownTimer -= Time.deltaTime;
         _cooldownImage.fillAmount = _cooldownTimer / _cooldownTime;
      }
      else
      {
         StopTimer();
      }
   }
}
