using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    [Tooltip("맵에 동시에 존재할 최대 아이템 수")] public int maxItemsOnMap = 6;
    [Tooltip("몇 초마다 스폰을 시도할지")] public float spawnInterval = 3f;
    [Tooltip("맵에 머무는 시간(초). 지나면 사라지고 새 아이템이 뜸. 0이면 사라지지 않음")]
    public float itemLifetime = 40f;

    private GameObject[] spawned;
    private List<int> bag = new List<int>();
    private float timer;

    void Start() { spawned = new GameObject[spawnPoints.Length]; }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;
        TrySpawn();
    }

    void TrySpawn()
    {
        int count = 0;
        List<int> freePoints = new List<int>();
        for (int i = 0; i < spawned.Length; i++)
        {
            if (spawned[i] != null) count++;   // 먹히거나 수명이 끝나 삭제된 칸은 비어 있게 됨
            else freePoints.Add(i);
        }
        if (count >= maxItemsOnMap || freePoints.Count == 0) return;

        int materialIndex = DrawFromBag();
        if (materialIndex < 0) return;

        GameObject prefab = InventorySystem.Instance.materials[materialIndex].worldPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"{InventorySystem.Instance.materials[materialIndex].name}의 World Prefab이 비어 있습니다.");
            return;
        }

        int pointIndex = freePoints[Random.Range(0, freePoints.Count)];
        GameObject go = Instantiate(prefab, spawnPoints[pointIndex].position, Quaternion.identity);

        if (itemLifetime > 0f) Destroy(go, itemLifetime); // 시간이 지나면 자동으로 사라짐

        ItemPickup pickup = go.GetComponent<ItemPickup>();
        if (pickup == null) pickup = go.AddComponent<ItemPickup>();
        pickup.materialIndex = materialIndex;

        spawned[pointIndex] = go;
    }

    // 셔플 백: 가중치만큼 넣고 섞어 뽑아서 비율이 정확히 지켜짐
    int DrawFromBag()
    {
        if (bag.Count == 0)
        {
            var mats = InventorySystem.Instance.materials;
            for (int i = 0; i < mats.Count; i++)
                for (int w = 0; w < mats[i].spawnWeight; w++)
                    bag.Add(i);
        }
        if (bag.Count == 0) return -1; // 모든 가중치가 0

        int pick = Random.Range(0, bag.Count);
        int result = bag[pick];
        bag.RemoveAt(pick);
        return result;
    }
}