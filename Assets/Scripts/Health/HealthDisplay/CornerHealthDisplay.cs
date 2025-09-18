using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CornerHealthDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarBackground;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image damageOverlay;

    [Header("Visual Settings")]
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float damageFlashDuration = 0.3f;

    private float targetFillAmount;
    private float currentFillAmount;
    private Coroutine damageFlashCoroutine;

    void Start()
    {
        SetupHealthGradient();
        if (damageOverlay != null)
            damageOverlay.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, 0);
    }

    void Update()
    {
        if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, animationSpeed * Time.deltaTime);
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = currentFillAmount;
                healthBarFill.color = healthGradient.Evaluate(currentFillAmount);
            }
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        targetFillAmount = currentHealth / maxHealth;
        if (healthText != null)
            healthText.text = $"{currentHealth:F0} / {maxHealth:F0}";
    }

    public void PlayDamageEffect()
    {
        if (damageFlashCoroutine != null)
            StopCoroutine(damageFlashCoroutine);
        damageFlashCoroutine = StartCoroutine(DamageFlashEffect());
    }

    private void SetupHealthGradient()
    {
        if (healthGradient.colorKeys.Length == 0)
        {
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0].color = Color.red;
            colorKeys[0].time = 0f;
            colorKeys[1].color = Color.yellow;
            colorKeys[1].time = 0.5f;
            colorKeys[2].color = Color.green;
            colorKeys[2].time = 1f;
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = 1f;
            alphaKeys[0].time = 0f;
            alphaKeys[1].alpha = 1f;
            alphaKeys[1].time = 1f;
            healthGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    private IEnumerator DamageFlashEffect()
    {
        if (damageOverlay == null) yield break;
        float elapsed = 0f;
        while (elapsed < damageFlashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 0f, elapsed / damageFlashDuration);
            damageOverlay.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, alpha);
            yield return null;
        }
        damageOverlay.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, 0);
    }
}