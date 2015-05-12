using System;
using UnityEngine;

public class Chicken : Enemy
{
    public GameObject TargetRing;
    public GameObject SelectionRing;
    public HP EnemyHP;

    public void Awake()
    {
        CombatManager.Instance.ThisEnemyManager.Enemies.Add(this.gameObject);
        TargetRing = this.transform.FindChild("Target").gameObject;
        SelectionRing = this.transform.FindChild("Select").gameObject;

        EnemyHP = this.gameObject.GetComponent<HP>();
    }

    public void Targeted()
    {
        if (SelectionRing.GetComponent<MeshRenderer>().enabled)
            CombatManager.Instance.DeselectEnemy();

        TargetRing.GetComponent<MeshRenderer>().enabled = true;

        CombatManager.Instance.MyEnemyStats.ShowStats();
        CombatManager.Instance.MyEnemyStats.UpdateStats(gameObject.name, EnemyHP.Level, EnemyHP.CurrentHealth, EnemyHP.MaxHealth);

    }

    public void Untargeted()
    {
        TargetRing.GetComponent<MeshRenderer>().enabled = false;
    }

    public void Select()
    {
        SelectionRing.GetComponent<MeshRenderer>().enabled = true;

        CombatManager.Instance.SelectedEnemy = this.transform;
        CombatManager.Instance.MyEnemyStats.ShowStats();
        CombatManager.Instance.MyEnemyStats.UpdateStats(gameObject.name, EnemyHP.Level, EnemyHP.CurrentHealth, EnemyHP.MaxHealth);
    }

    public void Deselect()
    {
        SelectionRing.GetComponent<MeshRenderer>().enabled = false;
    }

    public void OnMouseDown()
    {
        CombatManager.Instance.Targeting.FindEnemiesInCombatRange();
        bool inRange = false;
        foreach (GameObject enemy in CombatManager.Instance.EnemiesInSelectionRange)
        {
            if (this.gameObject == enemy)
            {
                inRange = true;
            }
        }
        if (!inRange)
            return;

        if (!TargetRing.GetComponent<MeshRenderer>().enabled)
        {
            CombatManager.Instance.DeselectEnemy(); //deselect old selection
            Select();
            CombatManager.Instance.SelectedEnemy = this.transform;

            

        }
    }
}
