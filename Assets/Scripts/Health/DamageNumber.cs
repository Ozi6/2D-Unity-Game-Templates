using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float moveUpDistance = 2f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.Linear(0, 1.5f, 1, 0.5f);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Colors")]
    [SerializeField] private Color damageColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color healColor = new Color(0.2f, 1f, 0.2f);
    [SerializeField] private Color criticalColor = new Color(1f, 1f, 0.2f);
    [SerializeField] private Color missColor = new Color(0.7f, 0.7f, 0.7f);

    private TextMeshPro textMesh;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Color originalColor;
    private float startTime;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
            textMesh = gameObject.AddComponent<TextMeshPro>();
        textMesh.fontSize = 3f;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.sortingOrder = 100;
    }

    public void Initialize(float amount, DamageType damageType)
    {
        startPosition = transform.position;
        targetPosition = startPosition + Vector3.up * moveUpDistance;
        startTime = Time.time;
        switch (damageType)
        {
            case DamageType.Damage:
                textMesh.text = $"-{amount:F0}";
                textMesh.color = damageColor;
                break;
            case DamageType.Heal:
                textMesh.text = $"+{amount:F0}";
                textMesh.color = healColor;
                break;
            case DamageType.Critical:
                textMesh.text = $"-{amount:F0}!";
                textMesh.color = criticalColor;
                textMesh.fontSize = 4f;
                break;
            case DamageType.Miss:
                textMesh.text = "MISS";
                textMesh.color = missColor;
                break;
        }
        originalColor = textMesh.color;
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }
        StartCoroutine(AnimateNumber());
    }

    private IEnumerator AnimateNumber()
    {
        while (Time.time - startTime < duration)
        {
            float elapsed = Time.time - startTime;
            float t = elapsed / duration;
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, moveCurve.Evaluate(t));
            transform.position = currentPos;
            float scale = scaleCurve.Evaluate(t);
            transform.localScale = Vector3.one * scale;
            float alpha = alphaCurve.Evaluate(t);
            Color currentColor = originalColor;
            currentColor.a = alpha;
            textMesh.color = currentColor;
            yield return null;
        }
        Destroy(gameObject);
    }
}