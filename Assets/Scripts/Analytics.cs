using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class Analytics : MonoBehaviour
{
    async void Start()
    {
        await UnityServices.InitializeAsync();
        AnalyticsService.Instance.StartDataCollection();
    }
}
