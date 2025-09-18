using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterHealthDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarBackground;
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 offset = Vector3.up * 2.5f;

    [Header("Visual Settings")]
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private float hideDelay = 3f;
    [SerializeField] private float fadeSpeed = 2f;

    private float targetFillAmount;
    private float currentFillAmount;
    private float lastDamageTime;
    private bool isVisible = false;
    private CanvasGroup canvasGroup;

    void Start()
    {
        SetupWorldCanvas();
        SetupHealthGradient();
        if (followTarget == null)
            followTarget = transform.parent;
        canvasGroup = worldCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = worldCanvas.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        UpdatePosition();
        UpdateHealthBarAnimation();
        UpdateVisibility();
    }

    private void SetupWorldCanvas()
    {
        if (worldCanvas == null)
        {
            GameObject canvasGO = new GameObject("CharacterHealthCanvas");
            canvasGO.transform.SetParent(transform);
            worldCanvas = canvasGO.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingOrder = 5;
            worldCanvas.transform.localScale = Vector3.one * 0.01f;
        }
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

    private void UpdatePosition()
    {
        if (followTarget != null && worldCanvas != null)
        {
            Vector3 targetPosition = followTarget.position + offset;
            worldCanvas.transform.position = targetPosition;
            if (Camera.main != null)
            {
                worldCanvas.transform.LookAt(Camera.main.transform);
                worldCanvas.transform.Rotate(0, 180, 0);
            }
        }
    }

    private void UpdateHealthBarAnimation()
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

    private void UpdateVisibility()
    {
        bool shouldBeVisible = Time.time - lastDamageTime < hideDelay || targetFillAmount < 1f;

        if (shouldBeVisible && !isVisible)
            isVisible = true;
        else if (!shouldBeVisible && isVisible)
            isVisible = false;
        float targetAlpha = isVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        targetFillAmount = currentHealth / maxHealth;
        lastDamageTime = Time.time;
    }

    public void PlayDamageEffect()
    {
        lastDamageTime = Time.time;
    }
}