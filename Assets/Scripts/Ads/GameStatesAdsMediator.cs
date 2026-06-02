using UnityEngine;

public class GameStatesAdsMediator 
{
    private readonly UIHandler _uiHandler;
    private readonly AdsHandler _adsHandler;
    
    public GameStatesAdsMediator(UIHandler uiHandler, AdsHandler adsHandler)
    {
        _uiHandler = uiHandler;
        _adsHandler = adsHandler;
    }
    
    
}
