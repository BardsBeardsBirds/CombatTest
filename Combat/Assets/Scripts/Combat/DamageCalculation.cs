using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageCalculation
{
    public float CalculateDamage(float baseDamage)
    {
        float baseAttack = PlayerStats.Instance.Virtuoso;
        float weaponDamage = PlayerStats.Instance.gameObject.GetComponent<Lute>().InstrumentDamage;

        float levelBonus = PlayerEnemyLevelDifferenceBonus();

        float enemyDefence = CombatManager.Instance.CombatTarget.GetComponent<HP>().Rubato + CombatManager.Instance.CombatTarget.GetComponent<HP>().Armour;

        float fullDamage = (baseDamage + weaponDamage + baseAttack) * levelBonus;
        float damage = fullDamage - enemyDefence;
        Debug.Log("Player damage: " + baseDamage + " " + weaponDamage + " " + baseAttack + " " + levelBonus + " attack: " + fullDamage + ". Enemy defence: " + enemyDefence + ". We did " + damage + " damage.");

        return damage;
    }

    public float PlayerEnemyLevelDifferenceBonus()
    {
        float bonus = 1f;
        int levelDifference = CombatManager.Instance.MyEnemyStats.Level - PlayerStats.Instance.CurrentLevel;
        Debug.Log("difference: " + levelDifference); // -2 = enemy is two levels lower

        if (levelDifference < -6)
            levelDifference = -6;
        else if (levelDifference > 6)
            levelDifference = 6;

        switch (levelDifference)
        {
            case -6:
                bonus = 1.25f;
                break;
            case -5:
                bonus = 1.18f;
                break;
            case -4:
                bonus = 1.12f;
                break;
            case -3:
                bonus = 1.07f;
                break;
            case -2:
                bonus = 1.04f;
                break;
            case -1:
                bonus = 1.02f;
                break;
            case 0:
                bonus = 1f;
                break;
            case 1:
                bonus = .98f;
                break;
            case 2:
                bonus = .96f;
                break;
            case 3:
                bonus = .93f;
                break;
            case 4:
                bonus = .90f;
                break;
            case 5:
                bonus = .86f;
                break;
            case 6:
                bonus = .80f;
                break;
            default:
                break;
        }
        return bonus;
    }
}