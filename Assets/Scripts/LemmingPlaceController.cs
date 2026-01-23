using System;

public class LemmingPlaceController: IDisposable
{
    private readonly LemmingPlaceView _lemmingPlaceView;
    private readonly InputController _inputController;
    private readonly LemmingConfig _lemmingConfig;
    
    public LemmingPlaceController(LemmingPlaceView lemmingPlaceView, InputController inputController, LemmingConfig lemmingConfig)
    {
        _lemmingPlaceView = lemmingPlaceView;
        _inputController = inputController;
        _lemmingConfig = lemmingConfig;
        
        _lemmingPlaceView.SideSpeed = _lemmingConfig.SideSpeed;
        _lemmingPlaceView.ForwardSpeed = _lemmingConfig.ForwardSpeed;

        _inputController.OnMoveLeft += MoveLeft;
        _inputController.OnMoveRight += MoveRight;
    }

    private void MoveLeft(bool isMoving)
    {
        if(isMoving)
            _lemmingPlaceView.IsMovingLeft = true;
        else
        {
            _lemmingPlaceView.IsMovingLeft = false;
        }
    }

    private void MoveRight(bool isMoving)
    {
        if(isMoving)
            _lemmingPlaceView.IsMovingRight = true;
        else
        {
            _lemmingPlaceView.IsMovingRight = false;
        }
    }

    public void Dispose()
    {
        _inputController.OnMoveLeft -= MoveLeft;
        _inputController.OnMoveRight -= MoveRight;
    }
}
