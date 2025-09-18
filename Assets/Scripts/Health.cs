using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Options")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float minHealth = 0.0f;
    [SerializeField] private float currentHealth;

    [Header("Display Components")]
    [SerializeField] private HealthDisplayManager healthDisplayManager;
    [SerializeField] private DamageNumberSpawner damageNumberSpawner;

    public UnityEvent<float, float> OnHealthChanged;
    public UnityEvent<float> OnDamageTaken;
    public UnityEvent<float> OnHealthRestored;
    public UnityEvent OnEntityDied;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => currentHealth / maxHealth;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthDisplayManager == null)
            healthDisplayManager = FindObjectOfType<HealthDisplayManager>();
        if (damageNumberSpawner == null)
            damageNumberSpawner = GetComponent<DamageNumberSpawner>();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        dummyDamager();
    }

    public void ChangeHealth(float val)
    {
        float checker = currentHealth + val;
        float previousHealth = currentHealth;

        if (checker > minHealth)
        {
            SetHealth(Mathf.Clamp(checker, minHealth, maxHealth));
        }
        else
        {
            SetHealth(0);
            KillEntity();
        }

        float healthChange = currentHealth - previousHealth;

        if (healthChange < 0)
        {
            OnDamageTaken?.Invoke(-healthChange);
            if (damageNumberSpawner != null)
                damageNumberSpawner.SpawnDamageNumber(-healthChange, DamageType.Damage);
        }
        else if (healthChange > 0)
        {
            OnHealthRestored?.Invoke(healthChange);
            if (damageNumberSpawner != null)
                damageNumberSpawner.SpawnDamageNumber(healthChange, DamageType.Heal);
        }
    }

    private void KillEntity()
    {
        Debug.Log($"{gameObject.name} has died!");
        OnEntityDied?.Invoke();
    }

    public void SetHealth(float val)
    {
        currentHealth = Mathf.Clamp(val, minHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void RestoreToFull()
    {
        float healAmount = maxHealth - currentHealth;
        if (healAmount > 0)
        {
            SetHealth(maxHealth);
            OnHealthRestored?.Invoke(healAmount);
            if (damageNumberSpawner != null)
                damageNumberSpawner.SpawnDamageNumber(healAmount, DamageType.Heal);
        }
    }

    private void dummyDamager()
    {
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            ChangeHealth(-Random.Range(5f, 15f));
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            ChangeHealth(Random.Range(10f, 20f));
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestoreToFull();
        }
    }
}