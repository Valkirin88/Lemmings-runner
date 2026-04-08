using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
   [SerializeField]
   private Button _abilityButton;
   
   [SerializeField]
   private Image _cooldownImage;

   [SerializeField]
   private Image _currentAbilityImage;
   
   
   [SerializeField]
   private float _cooldownTime = 3f;

   private AbilitiesHandler _abilitiesHandler;
   private AbilitiiesConfig _currentAbilitiesConfig;
   private float _cooldownTimer;
   private bool _isPushed;
   
   public void Initialize(AbilitiesHandler abilitiesHandler)
   {
      _abilitiesHandler = abilitiesHandler;
      _abilityButton.interactable = true;
      _cooldownImage.fillAmount = 0;
      
      _currentAbilitiesConfig = _abilitiesHandler.GetRandomAbility();
      _currentAbilityImage.sprite = _currentAbilitiesConfig.Image;
      _cooldownTime = _currentAbilitiesConfig.DurationTime;
      
      _abilityButton.onClick.AddListener(StartTimer);
   }

   private void StartTimer()
   {
      if (!_isPushed)
      {
         _abilitiesHandler.ActivateAbility();
         _currentAbilityImage.enabled = false;
         _isPushed = true;
         _cooldownImage.fillAmount = 1;
         _cooldownTimer = _cooldownTime;
         _abilityButton.interactable = false;
      }
   }

   private void StopTimer()
   {
      _cooldownImage.fillAmount = 0;
      _cooldownTimer = 0;
      _isPushed = false;
      _abilityButton.interactable = true;
      
      _currentAbilitiesConfig = _abilitiesHandler.GetRandomAbility();
      _currentAbilityImage.sprite = _currentAbilitiesConfig.Image;
      _cooldownTime = _currentAbilitiesConfig.DurationTime;

      _currentAbilityImage.enabled = true;
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
