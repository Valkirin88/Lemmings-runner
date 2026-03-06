using System;

public class LemmingController: IDisposable
{
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;
    private readonly InputController _inputController;
    
    public LemmingController(LemmingsEventsHandler lemmingsEventsHandler, InputController inputController)
    {
        _lemmingsEventsHandler = lemmingsEventsHandler;
        _inputController = inputController;
        
        _inputController.OnJump += Jump;
    }

    private void Jump()
    {
        foreach (var lemmingView in _lemmingsEventsHandler.RunningLemmingViews)
        {
            lemmingView.Jump();
        }
    }

    public void Dispose()
    {
        _inputController.OnJump -= Jump;
    }
}
