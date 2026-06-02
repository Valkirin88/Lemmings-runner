using UnityEngine;
using UnityEngine.EventSystems;
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
   private EventTrigger _trigger;
   private EventTrigger.Entry _entryDown;
   
   public void Initialize(AbilitiesHandler abilitiesHandler)
   {
      _abilitiesHandler = abilitiesHandler;
      _abilityButton.interactable = true;
      _cooldownImage.fillAmount = 0;
      
      _currentAbilitiesConfig = _abilitiesHandler.GetRandomAbility();
      _currentAbilityImage.sprite = _currentAbilitiesConfig.Image;
      _cooldownTime = _currentAbilitiesConfig.DurationTime;

      _trigger = _abilityButton.gameObject.GetComponent<EventTrigger>();
      if (_trigger == null)
         _trigger = _abilityButton.gameObject.AddComponent<EventTrigger>();

      _entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
      _entryDown.callback.AddListener(_ => StartTimer());
      _trigger.triggers.Add(_entryDown);
   }

   private void OnDestroy()
   {
      if (_trigger != null && _entryDown != null)
         _trigger.triggers.Remove(_entryDown);
   }

   private void StartTimer()
   {
      if (!_isPushed)
      {
         if (_abilitiesHandler != null && !_abilitiesHandler.CanActivateAbility())
            return;
         
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
      // Пока нет кулдауна — следим, есть ли вообще живые лемминги.
      if (!_isPushed)
      {
         if (_abilitiesHandler != null)
            _abilityButton.interactable = _abilitiesHandler.CanActivateAbility();
         return;
      }

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
