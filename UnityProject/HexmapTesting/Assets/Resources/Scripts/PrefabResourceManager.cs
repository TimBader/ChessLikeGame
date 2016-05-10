using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class PrefabResourceManager
{
    private const string PREFABS_RESOURCE_PATH = "Prefabs/";

    public static GameObject loadPrefab(string prefabName)
    {
        GameObject prefab;
        prefab = Resources.Load<GameObject>(PREFABS_RESOURCE_PATH + prefabName);
        if (prefab == null)
        {
            throw new UnityException("Cannot find '" + PREFABS_RESOURCE_PATH + prefabName + "' prefab");
        }
        return prefab;
    }
}
