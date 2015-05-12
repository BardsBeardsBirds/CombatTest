using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowDamageImpact : MonoBehaviour
{
    public Text damageTextObject;
    private GameObject damageCanvas;

    // Use this for initialization
    void Awake()
    {
        damageCanvas = GameObject.FindGameObjectWithTag("CanvasWorldSpace");
    }

    public void Damage(float amount)
    {
        Text instance = Instantiate(damageTextObject, transform.position, damageTextObject.transform.rotation) as Text;
        instance.transform.LookAt(2 * transform.position - Camera.main.transform.position);
        instance.text = ((int)amount).ToString("F1");
        
        instance.transform.SetParent(damageCanvas.transform, false);
    }
}