using UnityEngine;
using System.Collections;

public class HP : MonoBehaviour
{
    public int Level = 1;
    public float MaxHealth = 1f;
    public float CurrentHealth;
    public ShowDamageImpact showDamageNumber;

    public float Virtuoso;
    public float Rubato;
    public float Might;
    public float Block;
    public float Armour;

    private bool _isDead = false;

    void Start()
    {
        showDamageNumber = this.gameObject.GetComponentInChildren<ShowDamageImpact>();

        if (this.gameObject.tag == "Player")
        {
            Level = PlayerStats.Instance.CurrentLevel;
            return;
        }
        else
        {
            Level = Random.Range(1, 4);
            Rubato = 3 + (1.5f * Level);   //determine later on basis of enemy type and level
            Virtuoso = 10;
            Armour = 2;

        }

        MaxHealth = Level * 10; //look level health up in list and calculate
        CurrentHealth = MaxHealth;


    }

    public void Damage(float amount)
    {
    //    Debug.Log("I'm hit!");
        CurrentHealth -= amount;
        CombatManager.Instance.MyEnemyStats.UpdateStats("Kaas", Level, CurrentHealth, MaxHealth);

        if(CurrentHealth <= 0f && !_isDead)
        {
            Die();
            _isDead = true;
        }
    }

    public void ShowDamage(float damageAmount)
    {
        showDamageNumber.Damage(damageAmount);
    }

    public void Die()
    {
     //   Debug.Log("I'm dead");
        GetComponent<Death>().Die();
    }
}

