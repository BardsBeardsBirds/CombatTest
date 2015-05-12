using UnityEngine;
using System.Collections;


public class Death : MonoBehaviour
{
    public int XPValue = 1;

    public virtual void Die()
    {
        CombatManager.Instance.MyEnemyStats.HideStats();

        CombatManager.Instance.ThisEnemyManager.Enemies.Remove(this.gameObject);

        if (CombatManager.Instance.SelectedEnemy != null)
            if (CombatManager.Instance.SelectedEnemy.gameObject == this.gameObject)
                    CombatManager.Instance.DeselectEnemy();
        if (CombatManager.Instance.CombatTarget != null)
            if(CombatManager.Instance.CombatTarget.gameObject == this.gameObject)
                        CombatManager.Instance.UntargetEnemy();
        //foreach(GameObject enemy in CombatManager.EnemiesInCombatRange)
        //{
        //    if (this.gameObject == enemy)
        //        CombatManager.EnemiesInCombatRange.Remove(this.gameObject);
        //}

        PlayerStats.AddXP(XPValue);
    }
}
