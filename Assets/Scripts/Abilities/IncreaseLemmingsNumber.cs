using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class IncreaseLemmingsNumber : IAbility
{
    public event Action OnActivated;
    public event Action OnDeactivated;
    
    private readonly RandomSpawner _randomSpawner;
    private readonly LemmingPlaceHandler _lemmingPlaceHandler;
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;

    private int _additionalLemmingsCount = 1;
    
    public IncreaseLemmingsNumber(RandomSpawner randomSpawner,
        LemmingPlaceHandler lemmingPlaceHandler,
        LemmingsEventsHandler lemmingsEventsHandler)
    {
        _randomSpawner = randomSpawner;
        _lemmingPlaceHandler = lemmingPlaceHandler;
        _lemmingsEventsHandler = lemmingsEventsHandler;
    }

    

    /// <summary>
    /// Спавнит указанное количество новых леммингов и
    /// регистрирует их так, будто они были поочерёдно подобраны.
    /// В итоге все они сразу занимают свободные места в построении.
    /// </summary>
    public void Activate()
    {
        if (_additionalLemmingsCount <= 0)
            return;

        if (_randomSpawner == null || _lemmingsEventsHandler == null)
            return;

        var prefabs = _randomSpawner.LemmingPrefabs;
        if (prefabs == null || prefabs.Count == 0)
            return;

        for (int i = 0; i < _additionalLemmingsCount; i++)
        {
            SpawnAndRegisterLemming(prefabs);
        }

        OnActivated?.Invoke();

        // Способность мгновенная: отработали спавн — считаем деактивированной.
        Deactivate();
    }

    public void Update()
    {
        
    }

    public void Deactivate()
    {
        OnDeactivated?.Invoke();
    }

    private void SpawnAndRegisterLemming(IReadOnlyList<GameObject> prefabs)
    {
        var prefab = prefabs[Random.Range(0, prefabs.Count)];
        if (prefab == null)
            return;

        // Спавним нового лемминга (позиция не важна — сразу перенесём в RunPlace)
        var instance = Object.Instantiate(prefab);
        var lemmingView = instance.GetComponentInChildren<LemmingView>();
        if (lemmingView == null)
        {
            Object.Destroy(instance);
            return;
        }

        // Делаем его "подобранным" (как будто его поймали в цепочку)
        lemmingView.PickUp();

        // Регистрируем через общий обработчик — сработает тот же пайплайн,
        // что и при событии OnLemmingCaught (подписки, счётчики, события и т.д.)
        _lemmingsEventsHandler.AddLemming(lemmingView);

        // После регистрации LemmingPlaceHandler через событие OnLemmingCountAdd
        // выставит RunningPlace. Переносим лемминга сразу в его место в строю.
        if (lemmingView.RunningPlace != null)
        {
            lemmingView.transform.position = lemmingView.RunningPlace.position;
        }
    }
}
