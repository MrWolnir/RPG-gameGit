using UnityEngine;
using System.Collections.Generic;

public class TerrainTreeBaker : MonoBehaviour
{
    [ContextMenu("1. Создать временные коллайдеры")]
    public void CreateTemporaryColliders()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null) return;

        TerrainData data = terrain.terrainData;
        GameObject parent = new GameObject("_TempTreeBakeHolders");
        parent.transform.position = terrain.transform.position;

        foreach (TreeInstance tree in data.treeInstances)
        {
            GameObject prefab = data.treePrototypes[tree.prototypeIndex].prefab;
            Vector3 worldPos = Vector3.Scale(tree.position, data.size) + terrain.transform.position;

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.transform.position = worldPos;
            obj.transform.parent = parent.transform;

            // Настраиваем размер под ствол (можно отрегулировать)
            obj.transform.localScale = new Vector3(tree.widthScale * 0.5f, tree.heightScale * 2f, tree.widthScale * 0.5f);

            DestroyImmediate(obj.GetComponent<MeshRenderer>());
        }
        Debug.Log("Временные преграды созданы! Теперь выберите NavMesh Surface и нажмите Bake.");
    }

    [ContextMenu("2. Удалить временные коллайдеры")]
    public void DeleteTemporaryColliders()
    {
        GameObject parent = GameObject.Find("_TempTreeBakeHolders");
        if (parent != null)
        {
            DestroyImmediate(parent);
            Debug.Log("Временные объекты удалены. Чистая оптимизированная карта готова!");
        }
    }
}