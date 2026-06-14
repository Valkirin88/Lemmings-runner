using System;

public class LemmingController: IDisposable
{
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;
    private readonly InputController _inputController;
    private readonly SoundsHandler _soundsHandler;
    
    public LemmingController(LemmingsEventsHandler lemmingsEventsHandler, InputController inputController, SoundsHandler soundsHandler = null)
    {
        _lemmingsEventsHandler = lemmingsEventsHandler;
        _inputController = inputController;
        _soundsHandler = soundsHandler;
        
        _inputController.OnJump += Jump;
    }

    private void Jump()
    {
        var lemmings = _lemmingsEventsHandler.RunningLemmingViews;
        if (lemmings.Count == 0)
            return;

        foreach (var lemmingView in lemmings)
        {
            lemmingView.Jump();
        }

        if (_soundsHandler != null)
            _soundsHandler.PlayJump();
    }

    public void Dispose()
    {
        _inputController.OnJump -= Jump;
    }
}
