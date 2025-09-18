using UnityEngine;

public class HealthDisplayManager : MonoBehaviour
{
    [Header("Corner UI Health Display")]
    [SerializeField] private CornerHealthDisplay cornerHealthDisplay;

    [Header("Character Health Display")]
    [SerializeField] private CharacterHealthDisplay characterHealthDisplay;

    [Header("Health Bar Settings")]
    [SerializeField] private bool showCornerDisplay = true;
    [SerializeField] private bool showCharacterDisplay = true;

    void Start()
    {
        if (cornerHealthDisplay == null)
            cornerHealthDisplay = FindAnyObjectByType<CornerHealthDisplay>();
        if (characterHealthDisplay == null)
            characterHealthDisplay = FindAnyObjectByType<CharacterHealthDisplay>();
        Health[] healthComponents = FindObjectsOfType<Health>();
        foreach (Health health in healthComponents)
        {
            health.OnHealthChanged.AddListener(UpdateHealthDisplays);
            health.OnDamageTaken.AddListener(OnDamageTaken);
        }
    }

    private void UpdateHealthDisplays(float currentHealth, float maxHealth)
    {
        if (showCornerDisplay && cornerHealthDisplay != null)
            cornerHealthDisplay.UpdateHealth(currentHealth, maxHealth);
        if (showCharacterDisplay && characterHealthDisplay != null)
            characterHealthDisplay.UpdateHealth(currentHealth, maxHealth);
    }

    private void OnDamageTaken(float damageAmount)
    {
        if (showCornerDisplay && cornerHealthDisplay != null)
            cornerHealthDisplay.PlayDamageEffect();
        if (showCharacterDisplay && characterHealthDisplay != null)
            characterHealthDisplay.PlayDamageEffect();
    }
}