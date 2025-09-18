using TMPro;
using UnityEngine;

[System.Serializable]
public class DamageNumberPrefabCreator : MonoBehaviour
{
    [ContextMenu("Create Damage Number Prefab")]
    void CreatePrefab()
    {
        GameObject prefab = new GameObject("DamageNumber");
        prefab.AddComponent<DamageNumber>();
        TextMeshPro tmp = prefab.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = "-99";
            tmp.fontSize = 3f;
            tmp.color = Color.red;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }
}