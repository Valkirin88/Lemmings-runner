using System;

public class LemmingController: IDisposable
{
    public LemmingView View;
    private readonly InputController _inputController;
    
    public LemmingController(LemmingView view, InputController inputController)
    {
        View = view;
        _inputController = inputController;

        _inputController.OnMoveLeft += MoveLeft;
        _inputController.OnMoveRight += MoveRight;

    }

    private void MoveLeft(bool isMoving)
    {
        if(isMoving)
            View.IsMovingLeft = true;
        else
        {
            View.IsMovingLeft = false;
        }
    }

    private void MoveRight(bool isMoving)
    {
        if(isMoving)
            View.IsMovingRight = true;
        else
        {
            View.IsMovingRight = false;
        }
    }

    public void Dispose()
    {
        _inputController.OnMoveLeft -= MoveLeft;
        _inputController.OnMoveRight -= MoveRight;
    }
}
