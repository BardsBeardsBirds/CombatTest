using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerformanceSlots : MonoBehaviour
{
    private float _performanceLength;
    private float _performanceTimer = 0;

    private float _cooldown;
    private int _currentPerformanceSlot;
    private Transform _lastTarget;

    public List<Image> CooldownFilterList = new List<Image>();

    public void Awake()
    {
        foreach (Transform trans in this.gameObject.transform)
        {
            foreach (Transform deepTrans in trans)
            {
                if (!trans.GetComponent<Image>())
                {
                    if(deepTrans.name == "Cooldown")
                    {
                        CooldownFilterList.Add(deepTrans.GetComponent<Image>());
                    }
                }
            }
        }
    }

    public void Cast(int slotNumber)
    {
        if (CombatManager.Instance.BusyWithPerformance)
            return; //we are already doing a performance

        if (CombatManager.Instance.PerformanceSlots[slotNumber - 1].CoolDownTimer > 0)
            return; //we can't cast because of cooldown

        if (CombatManager.Instance.PerformanceSlots[slotNumber - 1].GetType() == typeof(EmptyPerformance))
            return;

        CombatManager.Instance.PerformanceSlots[slotNumber - 1].SetCurrentPerformance();

        bool inRange = CombatManager.Instance.Targeting.TargetEnemy(CombatManager.Instance.CurrentPerformance);

        if (!inRange)
            return;

        _currentPerformanceSlot = slotNumber;

        //cast
        CombatManager.Instance.PerformanceSlots[_currentPerformanceSlot - 1].Cast(CombatManager.Instance.CombatTarget);

        //busy with performance
        CombatManager.Instance.PerformanceDuration.TryGetValue(CombatManager.Instance.CurrentPerformance, out _performanceLength);
        _performanceTimer = _performanceLength;
        CombatManager.Instance.BusyWithPerformance = true;

        CombatManager.Instance.PerformanceSlots[_currentPerformanceSlot - 1].CoolDownTimer = CombatManager.Instance.PerformanceCooldownDuration[CombatManager.Instance.CurrentPerformance];
    }

    public void Update()
    {
        //performance timer
        if(CombatManager.Instance.BusyWithPerformance)
        {
            if (_performanceTimer > 0)
            {
                _performanceTimer -= Time.deltaTime;
            }
            if (_performanceTimer <= 0)
            {
                //give damage and effects here
                CombatManager.Instance.PerformanceSlots[_currentPerformanceSlot - 1].Damage(CombatManager.Instance.CombatTarget);
                CombatManager.Instance.BusyWithPerformance = false;
                CombatManager.Instance.CurrentPerformance = PerformanceEnum.EmptyPerformance;
            }
        }

        //cooldown timers
        for (int i = 0; i < CombatManager.Instance.PerformanceSlots.Count; i++)
        {
            if (CombatManager.Instance.PerformanceSlots[i].CoolDownTimer > 0)
            {
                CombatManager.Instance.PerformanceSlots[i].CoolDownTimer -= Time.deltaTime;
            }
        }

        for (int i = 0; i < CombatManager.Instance.PerformanceSlots.Count; i++)
        {
            if (CombatManager.Instance.PerformanceSlots[i].CoolDownTimer > 0)
            {               
                CooldownFilterList[i].fillAmount = CombatManager.Instance.PerformanceSlots[i].CoolDownTimer / CombatManager.Instance.PerformanceCooldownDuration[CombatManager.Instance.Performances[i + 1]];
            }
            else
                CooldownFilterList[i].fillAmount = 0f;
        }
    }
}