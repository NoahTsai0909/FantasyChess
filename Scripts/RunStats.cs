using System;
using UnityEngine;

[System.Serializable]
public class RunStats
{
    private int _currentGold;
    public int CurrentGold
    {
        get => _currentGold;
        set
        {
            _currentGold = value;
            RunStatsEventBus.GoldChanged(_currentGold);
        }
    }

    private int _playerHealth;
    public int PlayerHealth
    {
        get => _playerHealth;
        set
        {
            _playerHealth = value;
            RunStatsEventBus.HealthChanged(_playerHealth);
        }
    }

    private int _currentDay;
    public int CurrentDay
    {
        get => _currentDay;
        set
        {
            _currentDay = value;
            RunStatsEventBus.DayChanged(_currentDay);
        }
    }

    private int _playerLevel;
    public int PlayerLevel
    {
        get => _playerLevel;
        set
        {
            _playerLevel = value;
            RunStatsEventBus.LevelChanged(_playerLevel);
        }
    }

    private int _reputation;
    public int Reputation
    {
        get => _reputation;
        set
        {
            _reputation = value;
            RunStatsEventBus.ReputationChanged(_reputation);
        }
    }

    private int _provisionCap;
    public int ProvisionCap
    {
        get => _provisionCap;
        set
        {
            _provisionCap = value;
            RunStatsEventBus.ProvisionCapChanged(_provisionCap);
        }
    }

    // Initialize with default values
    public void Initialize()
    {
        CurrentGold = 10;
        PlayerHealth = 12;
        CurrentDay = 1;
        PlayerLevel = 1;
        Reputation = 0;
        ProvisionCap = 4;
    }
}
