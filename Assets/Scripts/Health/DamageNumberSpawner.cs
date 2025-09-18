using UnityEngine;
using UnityEngine.UI;

public class DamageNumberSpawner : MonoBehaviour
{
    [Header("Damage Number Settings")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Vector3 spawnOffset = Vector3.up * 2f;
    [SerializeField] private float spawnRandomRange = 0.5f;

    void Start()
    {
        if (worldCanvas == null)
        {
            GameObject canvasGO = new GameObject("DamageNumberCanvas");
            worldCanvas = canvasGO.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingOrder = 10;
            canvasGO.AddComponent<GraphicRaycaster>();
        }
    }

    public void SpawnDamageNumber(float amount, DamageType damageType)
    {
        if (damageNumberPrefab == null) return;
        Vector3 spawnPosition = transform.position + spawnOffset;
        spawnPosition += new Vector3(
            Random.Range(-spawnRandomRange, spawnRandomRange),
            Random.Range(-spawnRandomRange * 0.5f, spawnRandomRange * 0.5f),
            0
        );
        GameObject damageNumber = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity, worldCanvas.transform);
        DamageNumber damageNumberComponent = damageNumber.GetComponent<DamageNumber>();
        if (damageNumberComponent != null)
            damageNumberComponent.Initialize(amount, damageType);
    }
}