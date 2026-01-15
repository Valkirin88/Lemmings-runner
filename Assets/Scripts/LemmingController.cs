using System;

public class LemmingController: IDisposable
{
    public readonly LemmingView _view;
    public readonly InputController _inputController;
    
    public LemmingController(LemmingView view, InputController inputController)
    {
        _view = view;
        _inputController = inputController;

        _inputController.OnMoveLeft += MoveLeft;
        _inputController.OnMoveRight += MoveRight;

    }

    private void MoveLeft(bool isMoving)
    {
        if(isMoving)
            _view.IsMovingLeft = true;
        else
        {
            _view.IsMovingLeft = false;
        }
    }

    private void MoveRight(bool isMoving)
    {
        if(isMoving)
            _view.IsMovingRight = true;
        else
        {
            _view.IsMovingRight = false;
        }
    }

    public void Dispose()
    {
        _inputController.OnMoveLeft += MoveLeft;
        _inputController.OnMoveRight += MoveRight;
    }
}
