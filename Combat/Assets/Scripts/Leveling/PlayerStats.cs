using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public Text StatsDisplay;
    public static PlayerStats Instance;

    [Header("Tweak variables")]
    public float PerLevelBonusPercentage = 0.1f;    //10%
    public float PerLevelAttackRatingIncrease = 1;
    public float PerLevelDefenseRatingIncrease = 1;
    public int PerLevelHealthIncrease = 10;

    [Header("In-game variables")]
    [SerializeField] private float _HP;
    [SerializeField] private float _damageBonusPercentage = 1f;
    public float Virtuoso = 3;
    public float Rubato = 3;
    public int CurrentLevel = 1;
    [SerializeField] private int _maxLevel = 50;
    [SerializeField] private int _XP = 0;
    [SerializeField] private bool _isDead;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        DisplayStatsOnScreen();
    }

    public void Update()
    {
        DisplayStatsOnScreen();
    }

    public static void AddXP(int amount)
    {
        Instance._XP += amount;
        while (Instance._XP > GetNextLevelXPRequirement())
        {
            LevelUp();
        }
        if (Instance.CurrentLevel > Instance._maxLevel) // cannot go higher than maximum level
        {
            Instance.CurrentLevel = Instance._maxLevel;
        }
    }

    public static void LevelUp()
    {
        Instance.CurrentLevel++;
        Instance._damageBonusPercentage += Instance.PerLevelBonusPercentage;
        Instance.Virtuoso += Instance.PerLevelAttackRatingIncrease;
        Instance.Rubato += Instance.PerLevelDefenseRatingIncrease;
    }

    public static int GetNextLevelXPRequirement()
    {
        int returnValue = 10 * Instance.CurrentLevel;
        return returnValue;
    }

    public void Damage(float amount)
    {
        Debug.Log("Player is hit!");
        _HP -= amount;
        if (_HP <= 0f && !_isDead)
        {
            Die();
            _isDead = true;
        }
    }

    public void Die()
    {
        Debug.Log("I'm dead");
        GetComponent<Death>().Die();
    }

    public void DisplayStatsOnScreen()
    {
        StatsDisplay.text = "Level: " + CurrentLevel + "\n" +
                            "XP: " + _XP + "\n" + 
                            "Next level at: " + GetNextLevelXPRequirement() + "XP" + "\n" + 
                             "Virtuoso: " + Virtuoso + "\n" + 
                             "Rubato: " + Rubato;
    }
}
