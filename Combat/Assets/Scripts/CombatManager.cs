using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    
   // public Performance CurrentPerformance;
    public GameObject Player;
    public Transform SelectedEnemy;
    public GameObject CharacterStatsCanvas;
    public static GameObject EnemyStatsGo;
    public EnemyStats MyEnemyStats;
    public static CombatManager Instance;
    public EnemyManager ThisEnemyManager;
    public List<GameObject> EnemiesInSelectionRange = new List<GameObject>();
    public List<Performance> PerformanceSlots = new List<Performance>();
    public List<GameObject> PerformanceSlotButtons = new List<GameObject>();
    public PerformanceSlots ThisPerformanceSlots;
    public PerformanceAudio ThisPerformanceAudio;
    public PerformanceData ThisPerformanceData;
    public PerformanceEnum CurrentPerformance;
    public DamageCalculation DamageCalculations;
    public bool BusyWithPerformance = false;
    public Targeting Targeting;

    public Transform CombatTarget;

    [Header("Icons")]
    public Sprite EmptySlotIcon;
    public Sprite AwkwardDanceIcon;
    public Sprite RestorationSongIcon;
    public Sprite StridentStringCutIcon;
    public Sprite ToneWaterfallIcon;

    public Dictionary<int, PerformanceEnum> Performances = new Dictionary<int, PerformanceEnum>() { };
    public Dictionary<PerformanceEnum, Sprite> PerformanceIcons = new Dictionary<PerformanceEnum, Sprite>() { };

    public Dictionary<PerformanceEnum, float> PerformanceDuration = new Dictionary<PerformanceEnum, float>() { };
    public Dictionary<PerformanceEnum, float> PerformanceCooldownDuration = new Dictionary<PerformanceEnum, float>() { };
    public Dictionary<PerformanceEnum, float> PerformanceRange = new Dictionary<PerformanceEnum, float>() { };
    public Dictionary<PerformanceEnum, float> PerformanceDamage = new Dictionary<PerformanceEnum, float>() { };

    public void Awake()
    {
        Instance = this;
        ThisEnemyManager = new EnemyManager();
        if(Player == null)
            Player = GameObject.Find("Emmon");

        CharacterStatsCanvas = GameObject.Find("CharacterStatsCanvas");
        EnemyStatsGo = CharacterStatsCanvas.transform.FindChild("EnemyStats").gameObject;
        MyEnemyStats = CharacterStatsCanvas.GetComponentInChildren<EnemyStats>();

        ThisPerformanceSlots = GameObject.Find("Performances").GetComponent<PerformanceSlots>();
        ThisPerformanceAudio = GameObject.Find("PerformanceAudioSource").GetComponent<PerformanceAudio>();

        //Load performance data
        PerformanceData ThisPerformaneData = new PerformanceData();
        ThisPerformaneData.LoadPerformanceData();   

        Targeting = new Targeting();
        DamageCalculations = new DamageCalculation();

        //attacks
        Player.AddComponent<EmptyPerformance>();
        EmptyPerformance emptyPerformance = Player.GetComponent<EmptyPerformance>();
       
        Player.AddComponent<ToneWaterfall>();
        ToneWaterfall toneWaterfall = Player.GetComponent<ToneWaterfall>();

        Player.AddComponent<StridentStingCut>();
        StridentStingCut stringCut = Player.GetComponent<StridentStingCut>();

        Player.AddComponent<AwkwardDance>();
        AwkwardDance awkwardDance = Player.GetComponent<AwkwardDance>();

        Player.AddComponent<RestorationSong>();
        RestorationSong restorationSong = Player.GetComponent<RestorationSong>();

        AddPerformancesToSlots(1, toneWaterfall);
        AddPerformancesToSlots(2, stringCut);
        AddPerformancesToSlots(3, restorationSong);
        AddPerformancesToSlots(4, awkwardDance);
        AddPerformancesToSlots(5, emptyPerformance);
        AddPerformancesToSlots(6, emptyPerformance);
        AddPerformancesToSlots(7, emptyPerformance);
        AddPerformancesToSlots(8, emptyPerformance);
        AddPerformancesToSlots(9, toneWaterfall);
        AddPerformancesToSlots(10, emptyPerformance);

        foreach (Transform trans in ThisPerformanceSlots.gameObject.transform)
        {
            PerformanceSlotButtons.Add(trans.gameObject);
        }

        for (int i = 1; i < PerformanceSlotButtons.Count + 1; i++)
        {
            foreach (Transform trans in PerformanceSlotButtons[i - 1].transform)
            {
                if(trans.name == "Button")
                {
                    Sprite icon = PerformanceIcons[Performances[i]];
                    trans.GetComponent<Image>().sprite = icon;
                }
            }
        }

        CurrentPerformance = PerformanceEnum.EmptyPerformance;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ThisPerformanceSlots.Cast(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ThisPerformanceSlots.Cast(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ThisPerformanceSlots.Cast(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ThisPerformanceSlots.Cast(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ThisPerformanceSlots.Cast(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            ThisPerformanceSlots.Cast(6);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ThisPerformanceSlots.Cast(7);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            ThisPerformanceSlots.Cast(8);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            ThisPerformanceSlots.Cast(9);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ThisPerformanceSlots.Cast(10);
        }
    }

    public void AddPerformancesToSlots(int slotNumber, Performance performance)
    {
        PerformanceSlots.Add(performance);

        PerformanceEnum performanceType = PerformanceEnum.EmptyPerformance;

        if (performance.GetType() == typeof(EmptyPerformance))
            performanceType = PerformanceEnum.EmptyPerformance;
        else if (performance.GetType() == typeof(AwkwardDance))
            performanceType = PerformanceEnum.AwkwardDance;
        else if (performance.GetType() == typeof(RestorationSong))
            performanceType = PerformanceEnum.RestorationSong;
        else if (performance.GetType() == typeof(StridentStingCut))
            performanceType = PerformanceEnum.StridentStringCut;
        else if (performance.GetType() == typeof(ToneWaterfall))
            performanceType = PerformanceEnum.ToneWaterfall;
        else
            Debug.LogError("I don't know this type of performance!");

        Performances.Add(slotNumber, performanceType);
    }

    public float FindDistance(GameObject source, GameObject target)
    {
        return Vector3.Distance(source.transform.position, target.transform.position);
    }

    public void DeselectEnemy()
    {
        if (SelectedEnemy != null)
        {
            SelectedEnemy.GetComponent<Chicken>().Deselect();
            CombatManager.Instance.SelectedEnemy = null;
        }
    }

    public void UntargetEnemy()
    {
        if (CombatTarget != null)
        {
            CombatTarget.GetComponent<Chicken>().Untargeted();
            CombatManager.Instance.CombatTarget = null;
        }
    }
}
