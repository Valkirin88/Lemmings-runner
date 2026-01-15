using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class InputController
{
    public event Action<bool> OnMoveLeft;
    public event Action<bool> OnMoveRight;

    private bool _isMovingLeft = false;
    private bool _isMovingRight = false;
    
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
}
