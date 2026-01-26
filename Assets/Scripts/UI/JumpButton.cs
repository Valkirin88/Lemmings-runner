using UnityEngine;
using UnityEngine.UI;

public class JumpButton : MonoBehaviour
{
   [SerializeField]
   private Button _jumpButton;
   
   [SerializeField]
   private Image _cooldownImage;
   
   [SerializeField]
   private float _cooldownTime = 3f;

   private float _cooldownTimer;
   private bool _isPushed;
   
   private void Start()
   {
      _jumpButton.interactable = true;
      _cooldownImage.fillAmount = 0;
      
      _jumpButton.onClick.AddListener(StartTimer);
   }

   private void StartTimer()
   {
      if (!_isPushed)
      {
         _isPushed = true;
         _cooldownImage.fillAmount = 1;
         _cooldownTimer = _cooldownTime;
         _jumpButton.interactable = false;
      }
   }

   private void StopTimer()
   {
      _cooldownImage.fillAmount = 0;
      _cooldownTimer = 0;
      _isPushed = false;
      _jumpButton.interactable = true;
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
