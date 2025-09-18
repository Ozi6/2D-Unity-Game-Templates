using UnityEngine;
using UnityEngine.InputSystem;

public class Health : MonoBehaviour
{
    [Header("Health Options")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float minHealth = 0.0f;
    [SerializeField] private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        dummyDamager();
    }

    public void ChangeHealth(float val)
    {
        float checker = currentHealth + val;
        if(checker > minHealth)
        {
            SetHealth(checker);
        }
        else
        {
            KillEntity();
        }
    }

    private void KillEntity()
    {
        Debug.Log($"{gameObject.name} has died!");
    }

    public void SetHealth(float val)
    {
        currentHealth = val;
    }

    public void RestoreToFull()
    {
        SetHealth(maxHealth);
    }

    private void dummyDamager()
    {
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            ChangeHealth(-1);
        }
    }
}
