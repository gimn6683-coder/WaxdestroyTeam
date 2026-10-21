using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;   // 근접, 원거리
    public Transform[] spawnPoints;
    public float spawnInterval = 3f;
    public int maxAlive = 10;

    [Header("바닥 높이 맞추기")]
    [Tooltip("스폰 위치 아래의 바닥을 찾아 그 위에 적을 놓음")]
    public bool snapToGround = true;
    [Tooltip("바닥을 찾는 레이를 스폰 위치에서 얼마나 위에서 쏠지")]
    public float rayStartHeight = 50f;
    [Tooltip("레이가 내려가는 최대 거리")]
    public float rayDistance = 200f;
    [Tooltip("바닥 위로 띄울 높이. 피벗이 발 밑이면 0, 몸 중앙이면 키의 절반 정도")]
    public float spawnHeightOffset = 0.5f;
    [Tooltip("바닥으로 취급할 레이어 (기본: 전부)")]
    public LayerMask groundMask = ~0;




    float timer;
    void Update()
    {
        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;


        int alive = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (alive >= maxAlive) return;

        int e = Random.Range(0, enemyPrefabs.Length);
        int s = Random.Range(0, spawnPoints.Length);
        if (enemyPrefabs[e] == null)
        {
            Debug.LogWarning($"Enemy Prefabs의 {e}번 칸이 비어 있습니다.");
            return;
        }
        if (spawnPoints[s] == null)
        {
            Debug.LogWarning($"Spawn Points의 {s}번 칸이 비어 있습니다.");
            return;
        }

        Vector3 pos = spawnPoints[s].position;
        if (snapToGround) pos = FindGroundPosition(pos);
       
        Instantiate(enemyPrefabs[e], pos, Quaternion.identity);
    }

  Vector3 FindGroundPosition(Vector3 point)
    {
        Vector3 origin = point + Vector3.up * rayStartHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance,
                            groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * spawnHeightOffset;
        }

        Debug.LogWarning("스폰 위치 아래에서 바닥 콜라이더를 찾지 못했습니다. 바닥에 콜라이더가 있는지 확인하세요.");
        return point;   // 못 찾으면 원래 위치 사용
    }
}

