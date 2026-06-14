using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class InputController :IDisposable
{
    public event Action<bool> OnMoveLeft;
    public event Action<bool> OnMoveRight;
    public event Action OnJump;
    public event Action OnAccelerate;

    private bool _isMovingLeft = false;
    private bool _isMovingRight = false;
    
    private Button _accelerateButton;
    private Button _jumpButton;

    public InputController(Button accelerateButton, Button jumpButton)
    {
        _accelerateButton = accelerateButton;
        _jumpButton = jumpButton;

        _accelerateButton.onClick.AddListener(Accelerate);
        _jumpButton.onClick.AddListener(Jump);
    }

    private void Accelerate()
    {
        OnAccelerate?.Invoke();
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
            OnMoveLeft?.Invoke(true);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            OnMoveRight?.Invoke(true);
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

    private void HandleMouseAndTouchInput()
    {
        bool isInputActive = Input.GetMouseButton(0) || Input.touchCount > 0;
        
        // Проверяем, не нажали ли на UI элемент
        if (isInputActive && IsPointerOverUI())
        {
            return;
        }
        
        if (isInputActive)
        {
            Vector3 inputPosition = Input.mousePosition;
            
            // Если используется тач, берем позицию первого касания
            if (Input.touchCount > 0)
            {
                inputPosition = Input.GetTouch(0).position;
            }
            
            float screenCenter = Screen.width / 2f;
            
            // Левая половина экрана - движение влево
            if (inputPosition.x < screenCenter)
            {
                if (!_isMovingLeft)
                {
                    OnMoveLeft?.Invoke(true);
                    OnMoveRight?.Invoke(false);
                    _isMovingLeft = true;
                    _isMovingRight = false;
                }
            }
            // Правая половина экрана - движение вправо
            else
            {
                if (!_isMovingRight)
                {
                    OnMoveRight?.Invoke(true);
                    OnMoveLeft?.Invoke(false);
                    _isMovingRight = true;
                    _isMovingLeft = false;
                }
            }
        }
        else
        {
            // Отпускаем управление, когда нет ввода
            if (_isMovingLeft || _isMovingRight)
            {
                OnMoveLeft?.Invoke(false);
                OnMoveRight?.Invoke(false);
                _isMovingLeft = false;
                _isMovingRight = false;
            }
        }
    }
    
    private bool IsPointerOverUI()
    {
        // Проверка для мыши
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        
        // Проверка для тача
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
            {
                return true;
            }
        }
        
        return false;
    }

    public void Dispose()
    {
        _accelerateButton.onClick.RemoveListener(Accelerate);
        _jumpButton.onClick.RemoveListener(Jump);
    }
}
