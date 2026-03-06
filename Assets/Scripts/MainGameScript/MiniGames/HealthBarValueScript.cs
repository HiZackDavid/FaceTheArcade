using UnityEngine;
using UnityEngine.UI;

public class HealthBarValueScript : MonoBehaviour
{
    public CharacterHealthScript characterHealthScript;
    public Slider slider;

    public void Start()
    {
        if (characterHealthScript == null) Debug.LogError("Character Health Script reference is missing for " + transform.name);
        if (slider == null) Debug.LogError("Health Bar Slider reference is missing for " + transform.name);
    }

    private void OnEnable()
    {
        characterHealthScript.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDisable()
    {
        characterHealthScript.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(float healthPercentage)
    {
        slider.value = healthPercentage;
    }


}
