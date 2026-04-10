using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AbilitiesHandler : IDisposable
{
    private readonly ObstaclesSet _obstaclesSet;
    private readonly RandomSpawner _randomSpawner;
    private readonly LemmingPlaceHandler _lemmingPlaceHandler;
    private readonly LemmingsEventsHandler _lemmingsEventsHandler;
    private readonly LemmingPlaceView _lemmingPlaceView;
    private readonly List<AbilitiiesConfig> _abilitiesConfigs;
    private readonly AbilitiesFX _abilitiesFX;
    private readonly AudioClip _destroyAllObstaclesClip;
    
    private IAbility _currentAbility;
    
    
    public AbilitiesHandler(ObstaclesSet obstaclesSet, RandomSpawner randomSpawner, LemmingPlaceHandler lemmingPlaceHandler,
        LemmingsEventsHandler lemmingsEventsHandler, LemmingPlaceView lemmingPlaceView, List<AbilitiiesConfig> abilitiesConfigs, AbilitiesFX abilitiesFX, AudioClip destroyAllObstaclesClip)
    {
        _obstaclesSet = obstaclesSet;
        _randomSpawner = randomSpawner;
        _lemmingPlaceHandler = lemmingPlaceHandler;
        _lemmingsEventsHandler = lemmingsEventsHandler;
        _lemmingPlaceView = lemmingPlaceView;
        _abilitiesConfigs = abilitiesConfigs;
        _abilitiesFX = abilitiesFX;
        _destroyAllObstaclesClip = destroyAllObstaclesClip;
    }

    public AbilitiiesConfig GetRandomAbility()
    {
        if (_abilitiesConfigs == null || _abilitiesConfigs.Count == 0)
            return null;

        var config = _abilitiesConfigs[Random.Range(0, _abilitiesConfigs.Count)];
        if (config == null)
            return null;
        
        if (_currentAbility != null)
            ResetCurrentAbility();
        
        _currentAbility = CreateAbilityFromConfig(config);

        return config;
    }

    public void Update()
    {
        if (_currentAbility != null)
            _currentAbility.Update();
    }
    
    public void ActivateAbility()
    {
        _currentAbility.Activate();
        _currentAbility.OnDeactivated += ResetCurrentAbility;

    }

    private void ResetCurrentAbility()
    {
        _currentAbility.OnDeactivated -= ResetCurrentAbility;
        _currentAbility = null;
    }


    private IAbility CreateAbilityFromConfig(AbilitiiesConfig config)
    {
        switch (config.AbilityId)
        {
            case AbilityId.DestroyAllObstacles:
                return new DestroyAllObstacles(_obstaclesSet, _destroyAllObstaclesClip);

            case AbilityId.MakeLemmingsInvincible:
            {
                MakeLemmingsInvinsible _makeLemmingsInvinsible = new MakeLemmingsInvinsible(_lemmingPlaceView, _lemmingsEventsHandler);
                _makeLemmingsInvinsible.AbilitiesConfig = config;
                return _makeLemmingsInvinsible;
            }
            case AbilityId.IncreaseLemmingsNumber:
            {
                var increaseLemmingsNumber = new IncreaseLemmingsNumber(_randomSpawner, _lemmingPlaceHandler, _lemmingsEventsHandler);
                if (_abilitiesFX != null)
                    _abilitiesFX.Initialize(increaseLemmingsNumber);
                return increaseLemmingsNumber;
            }

            default:
                return null;
        }
    }

    public void Dispose()
    {
        if (_currentAbility != null)
            _currentAbility.OnDeactivated -= ResetCurrentAbility;
    }
}
