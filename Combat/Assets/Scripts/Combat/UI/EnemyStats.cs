using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    public Text EnemyNameText;
    public Text HealthText;
    public Text LevelText;

    public Image HealthBar;

    public string EnemyName = "";
    public string HealthString;
    public float CurrentHealth;
    public float MaxHealth;
    public int Level;

    public bool HasEnemySelected = false;


    public void Start()
    {
        LevelText = this.transform.FindChild("Level").GetComponent<Text>();
        EnemyNameText = this.transform.FindChild("Name").GetComponent<Text>();
        HealthText = this.transform.FindChild("Health").GetComponent<Text>();
        HealthBar = this.transform.FindChild("HealthBar").GetComponent<Image>();
    }

    public void Update()
    {

    }

    public void UpdateStats(string name, int level, float currentHealth, float maxHealth)
    {
        EnemyName = name;
        EnemyNameText.text = EnemyName;

        Level = level;
        LevelText.text = "Level: " + Level.ToString();

        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        HealthString = CurrentHealth.ToString("F1") + " / " + MaxHealth.ToString("F1");
        HealthText.text = HealthString;

        HealthBar.fillAmount = currentHealth / maxHealth;
    }

    public void ShowStats()
    {
        this.GetComponent<Image>().enabled = true;
        EnemyNameText.enabled = true;
        HealthText.enabled = true;
        HealthBar.enabled = true;
    }

    public void HideStats()
    {
        this.GetComponent<Image>().enabled = false;
        EnemyNameText.enabled = false;
        HealthText.enabled = false;
        HealthBar.enabled = false;
    }
}

