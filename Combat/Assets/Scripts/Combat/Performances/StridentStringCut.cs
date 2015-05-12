using System;
using UnityEngine;

public class StridentStingCut : Performance
{
    public override void Cast(Transform target)
    {
        CombatManager.Instance.ThisPerformanceAudio.IsPlayingPerformance = true;

      //  Damage(target);
    }

    public override void SetCurrentPerformance()
    {
        CombatManager.Instance.CurrentPerformance = PerformanceEnum.StridentStringCut;
    }

    public override void Damage(Transform target)
    {
        float baseDamage = CombatManager.Instance.PerformanceDamage[PerformanceEnum.StridentStringCut];
        float calculatedDamage = CombatManager.Instance.DamageCalculations.CalculateDamage(baseDamage);

        target.GetComponent<HP>().Damage(calculatedDamage);
        target.GetComponent<HP>().ShowDamage(calculatedDamage);
    }
}