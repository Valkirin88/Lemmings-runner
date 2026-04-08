using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using UnityEngine.Events;

public class InputController :IDisposable
{
    public event Action<bool> OnMoveLeft;
    public event Action<bool> OnMoveRight;
    public event Action OnJump;
    public event Action OnAccelerate;

    private bool _isMovingLeft = false;
    private bool _isMovingRight = false;
    
    
    private Button _jumpButton;
    private Button _leftButton;
    private Button _rightButton;

    private EventTrigger _leftTrigger;
    private EventTrigger _rightTrigger;
    private EventTrigger.Entry _leftEntryDown;
    private EventTrigger.Entry _leftEntryUp;
    private EventTrigger.Entry _rightEntryDown;
    private EventTrigger.Entry _rightEntryUp;
    private UnityAction<BaseEventData> _leftActionDown;
    private UnityAction<BaseEventData> _leftActionUp;
    private UnityAction<BaseEventData> _rightActionDown;
    private UnityAction<BaseEventData> _rightActionUp;

    public InputController(Button accelerateButton, Button jumpButton, Button leftButton, Button rightButton)
    {
        _jumpButton = jumpButton;
        _leftButton = leftButton;
        _rightButton = rightButton;

        _jumpButton.onClick.AddListener(Jump);
        SetupHoldButton(_leftButton, () => OnMoveLeft?.Invoke(true), () => OnMoveLeft?.Invoke(false),
            out _leftTrigger, out _leftEntryDown, out _leftEntryUp, out _leftActionDown, out _leftActionUp);
        SetupHoldButton(_rightButton, () => OnMoveRight?.Invoke(true), () => OnMoveRight?.Invoke(false),
            out _rightTrigger, out _rightEntryDown, out _rightEntryUp, out _rightActionDown, out _rightActionUp);
    }

    private void SetupHoldButton(Button button, Action onPointerDown, Action onPointerUp,
        out EventTrigger trigger, out EventTrigger.Entry entryDown, out EventTrigger.Entry entryUp,
        out UnityAction<BaseEventData> actionDown, out UnityAction<BaseEventData> actionUp)
    {
        trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        actionDown = _ => onPointerDown();
        actionUp = _ => onPointerUp();

        entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entryDown.callback.AddListener(actionDown);
        trigger.triggers.Add(entryDown);

        entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entryUp.callback.AddListener(actionUp);
        trigger.triggers.Add(entryUp);
    }


    private void Jump()
    {
        OnJump?.Invoke();
    }
    
    public void Update()
    {
        // Управление клавиатурой
        HandleKeyboardInput();
        
        // Управление мышью и тачем
        HandleMouseAndTouchInput();
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            MoveRight();
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            OnMoveLeft?.Invoke(false);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            OnMoveRight?.Invoke(false);
        }
    }

    private void MoveLeft()
    {
        OnMoveLeft?.Invoke(true);
    }

    private void MoveRight()
    {
        OnMoveRight?.Invoke(true);
    }

    private void HandleMouseAndTouchInput()
    {
        // bool isInputActive = Input.GetMouseButton(0) || Input.touchCount > 0;
        //
        // // Проверяем, не нажали ли на UI элемент
        // if (isInputActive && IsPointerOverUI())
        // {
        //     return;
        // }
        //
        // if (isInputActive)
        // {
        //     Vector3 inputPosition = Input.mousePosition;
        //     
        //     // Если используется тач, берем позицию первого касания
        //     if (Input.touchCount > 0)
        //     {
        //         inputPosition = Input.GetTouch(0).position;
        //     }
        //     
        //     float screenCenter = Screen.width / 2f;
        //     
        //     // Левая половина экрана - движение влево
        //     if (inputPosition.x < screenCenter)
        //     {
        //         if (!_isMovingLeft)
        //         {
        //             OnMoveLeft?.Invoke(true);
        //             OnMoveRight?.Invoke(false);
        //             _isMovingLeft = true;
        //             _isMovingRight = false;
        //         }
        //     }
        //     // Правая половина экрана - движение вправо
        //     else
        //     {
        //         if (!_isMovingRight)
        //         {
        //             OnMoveRight?.Invoke(true);
        //             OnMoveLeft?.Invoke(false);
        //             _isMovingRight = true;
        //             _isMovingLeft = false;
        //         }
        //     }
        // }
        // else
        // {
        //     // Отпускаем управление, когда нет ввода
        //     if (_isMovingLeft || _isMovingRight)
        //     {
        //         OnMoveLeft?.Invoke(false);
        //         OnMoveRight?.Invoke(false);
        //         _isMovingLeft = false;
        //         _isMovingRight = false;
        //     }
        // }
    }
    
    // private bool IsPointerOverUI()
    // {
    //     // // Проверка для мыши
    //     // if (EventSystem.current.IsPointerOverGameObject())
    //     // {
    //     //     return true;
    //     // }
    //     //
    //     // // Проверка для тача
    //     // for (int i = 0; i < Input.touchCount; i++)
    //     // {
    //     //     if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
    //     //     {
    //     //         return true;
    //     //     }
    //     // }
    //     //
    //     // return false;
    // }

    public void Dispose()
    {
        _jumpButton.onClick.RemoveListener(Jump);

        if (_leftTrigger != null)
        {
            _leftEntryDown?.callback.RemoveListener(_leftActionDown);
            _leftEntryUp?.callback.RemoveListener(_leftActionUp);
            _leftTrigger.triggers.Remove(_leftEntryDown);
            _leftTrigger.triggers.Remove(_leftEntryUp);
        }

        if (_rightTrigger != null)
        {
            _rightEntryDown?.callback.RemoveListener(_rightActionDown);
            _rightEntryUp?.callback.RemoveListener(_rightActionUp);
            _rightTrigger.triggers.Remove(_rightEntryDown);
            _rightTrigger.triggers.Remove(_rightEntryUp);
        }
    }
}
