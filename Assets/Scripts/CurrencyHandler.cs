using System;
using UnityEngine;

public class CurrencyHandler : IDisposable
{
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;
    
    private int _currencyRate = 1;
    
    public CurrencyHandler(LemmingsEventsHandler lemmingsEventsHandler)
    {
        _lemmingsEventsHandler = lemmingsEventsHandler;

        _lemmingsEventsHandler.OnCurrencyGot += SaveCurrency;
    }

    private void SaveCurrency(int obj)
    {
        _currencyRate = PlayerPrefs.GetInt("CurrencyRate");
        _currencyRate++;
        PlayerPrefs.SetInt("Currency", _currencyRate);
    }

    public void Dispose()
    {
        _lemmingsEventsHandler.OnCurrencyGot -= SaveCurrency;
    }
}
