using System;

public class LemmingPlaceController: IDisposable
{
    private readonly LemmingPlaceView _lemmingPlaceView;
    private readonly InputController _inputController;
    private readonly LemmingConfig _lemmingConfig;
    private readonly GameStateCollector _gameStateCollector;
    
    public LemmingPlaceController(LemmingPlaceView lemmingPlaceView, InputController inputController, LemmingConfig lemmingConfig, GameStateCollector gameStateCollector)
    {
        _lemmingPlaceView = lemmingPlaceView;
        _inputController = inputController;
        _lemmingConfig = lemmingConfig;
        _gameStateCollector = gameStateCollector;
        
        _lemmingPlaceView.SideSpeed = _lemmingConfig.SideSpeed;
        _lemmingPlaceView.ForwardSpeed = _lemmingConfig.ForwardSpeed;
        _lemmingPlaceView.AccelerateDuration = _lemmingConfig.AccelerateDuration;
        _lemmingPlaceView.AccelerateMultiplier = _lemmingConfig.AccelerateMultiplier;

        _inputController.OnMoveLeft += MoveLeft;
        _inputController.OnMoveRight += MoveRight;
        _inputController.OnAccelerate += Accelerate;
    }

    private void Accelerate()
    {
        _lemmingPlaceView.Accelerate();
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

    public void Update()
    {
        if (_gameStateCollector.State == GameState.Game)
        {
            _lemmingPlaceView.IsMoving = true;
        }
        else
        {
            _lemmingPlaceView.IsMoving = false;
        }
    }

    public void Dispose()
    {
        _inputController.OnMoveLeft -= MoveLeft;
        _inputController.OnMoveRight -= MoveRight;
        _inputController.OnAccelerate -= Accelerate;
    }
}
