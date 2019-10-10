using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour {
    public BiomeSettings biomeSettings;
    public int poolSize = 100;
    Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();

    static PoolManager _instance;

    public static PoolManager Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<PoolManager>();
            }
            return _instance;
        }
    }

    private void Start() {
        for (int i = 0; i < biomeSettings.biomes.Count; i++) {
            for (int j = 0; j < biomeSettings.biomes[i].GetPlants().Length; j++) {
                CreatePool(biomeSettings.biomes[i].GetPlants()[j], poolSize);
            }
        }
    }

    public void CreatePool(GameObject prefab, int poolSize) {
        int poolKey = prefab.GetInstanceID();

        GameObject poolHolder = new GameObject(prefab.name + " pool " + poolKey);
        poolHolder.transform.parent = transform;

        if (!poolDictionary.ContainsKey(poolKey)) {
            poolDictionary.Add(poolKey, new Queue<GameObject>());
            for (int i = 0; i < poolSize; i++) {
                GameObject newObject = Instantiate(prefab) as GameObject;
                newObject.SetActive(false);
                newObject.transform.parent = poolHolder.transform;
                poolDictionary[poolKey].Enqueue(newObject);
            }
        }
    }

    public GameObject RequestObject(GameObject prefab, Vector3 position) {
        int poolKey = prefab.GetInstanceID();
        if (poolDictionary.ContainsKey(poolKey)) {
            GameObject objectToUse = poolDictionary[poolKey].Dequeue();
            if (!objectToUse.activeSelf) {
                poolDictionary[poolKey].Enqueue(objectToUse);
                objectToUse.SetActive(true);
                objectToUse.transform.position = position;
                return objectToUse;
            }
        }
        return null;
    }

    public GameObject GetObject(GameObject prefab, Vector3 position) {
        int poolKey = prefab.GetInstanceID();
        GameObject objectToGet = null;
        if (poolDictionary.ContainsKey(poolKey)) {
            if(poolDictionary[poolKey].Count == 0) {

                GameObject poolHolder = new GameObject(prefab.name + " pool " + poolKey);
                poolHolder.transform.parent = transform;

                for (int i = 0; i < poolSize; i++) {
                    GameObject newObject = Instantiate(prefab) as GameObject;
                    newObject.SetActive(false);
                    newObject.transform.parent = poolHolder.transform;
                    poolDictionary[poolKey].Enqueue(newObject);
                }
            }
            objectToGet = poolDictionary[poolKey].Dequeue();
            objectToGet.SetActive(true);
            objectToGet.transform.position = position;
        }
        return objectToGet;
    }
}