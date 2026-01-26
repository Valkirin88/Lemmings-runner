using System;

public class LemmingController: IDisposable
{
    private readonly LemmingsStateSet _lemmingsStateSet;
    private readonly InputController _inputController;
    
    public LemmingController(LemmingsStateSet lemmingsStateSet, InputController inputController)
    {
        _lemmingsStateSet = lemmingsStateSet;
        _inputController = inputController;
        
        _inputController.OnJump += Jump;
    }

    private void Jump()
    {
        foreach (var lemmingView in _lemmingsStateSet.RunningLemmingViews)
        {
            lemmingView.Jump();
        }
    }

    public void Dispose()
    {
        _inputController.OnJump -= Jump;
    }
}
