using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    [Header("적 프리팹")]
    public GameObject meleePrefab;     // 근접 적
    public GameObject rangedPrefab;    // 원거리 적
    public Transform[] spawnPoints;

    [Header("웨이브")]
    public int totalWaves = 2;         // 전체 웨이브 수
    public int meleePerWave = 5;       // 1웨이브 근접 마릿수
    public int rangedPerWave = 5;      // 1웨이브 원거리 마릿수
    public int meleeAddPerWave = 0;    // 웨이브마다 근접 추가 (난이도 조절용)
    public int rangedAddPerWave = 0;   // 웨이브마다 원거리 추가
    public float startDelay = 3f;      // 게임 시작 후 첫 웨이브까지 대기
    public float waveDelay = 4f;       // 웨이브를 클리어한 뒤 다음 웨이브까지 쉬는 시간
    public bool shuffleOrder = true;   // 근접/원거리를 섞어서 내보내기
    public TMP_Text waveText;          // 비워 둬도 됨
    [Header("나오는 속도")]
    [Tooltip("한 마리씩 나오는 간격(초). 클수록 천천히 나옴")]
    public float spawnGap = 3f;
    [Tooltip("맵에 동시에 있을 수 있는 적 수. 이 수에 도달하면 한 마리가 죽을 때까지 더 안 나옴 (0이면 제한 없음)")]
    public int maxAliveAtOnce = 4;

    [Header("모든 웨이브 클리어 후")]
    [Tooltip("이동할 씬 이름. 비우면 이동하지 않음 (Build Profiles의 씬 목록에 있어야 함)")]
    public string clearSceneName = "";
    public float clearDelay = 2f;

    [Header("바닥 높이 맞추기")]
    [Tooltip("지형으로 취급할 레이어. 비워 두면 'Ground' 레이어를 자동으로 사용")]
    public LayerMask groundMask;
    public float rayStartHeight = 50f;
    public float rayDistance = 200f;
    [Tooltip("바닥 위로 띄울 높이. 떠다니는 적은 알아서 높이를 맞추니 1 정도면 충분")]
    public float spawnHeightOffset = 1f;

    int wave = 0;                      // 지금까지 시작한 웨이브 수
    bool waveRunning;                  // 웨이브 진행 중 (스폰 중이거나 적이 남아 있음)
    bool cleared;                      // 모든 웨이브 끝
    float timer;
    float nextWaveTime;                // 다음 웨이브를 시작할 시각

    readonly Queue<GameObject> queue = new Queue<GameObject>();   // 이번 웨이브에서 남은 적
    readonly List<GameObject> alive = new List<GameObject>();     // 이 스포너가 만든 살아 있는 적
    readonly HashSet<Transform> warned = new HashSet<Transform>();

    void Start()
    {

        Physics.queriesHitBackfaces = true;

        if (groundMask.value == 0)
        {
            int g = LayerMask.NameToLayer("Ground");
            if (g >= 0) groundMask = 1 << g;
            else Debug.LogWarning($"{name}: Ground Mask가 비어 있고 'Ground' 레이어도 없습니다. Project Settings > Tags and Layers의 Layers 칸에 Ground를 만들어야 해요 (위쪽 Tags 칸이 아니에요).");
        }

        nextWaveTime = Time.time + startDelay;
        ShowText("");
    }

    void Update()
    {
        if (cleared) return;

        // 1) 이번 웨이브의 적을 한 마리씩 내보내는 중
        if (queue.Count > 0)
        {

            if (maxAliveAtOnce > 0)
            {
                alive.RemoveAll(e => e == null);
                if (alive.Count >= maxAliveAtOnce)
                {
                    timer = 0f;   // 빈자리가 나도 spawnGap만큼 기다렸다가 나옴
                    return;
                }
            }

            timer += Time.deltaTime;
            if (timer < spawnGap) return;
            timer = 0f;
            SpawnOne(queue.Dequeue());
            return;
        }

        // 2) 웨이브 진행 중: 다 잡을 때까지 대기
        if (waveRunning)
        {
            alive.RemoveAll(e => e == null);   // 죽어서 삭제된 적 정리
            if (alive.Count > 0) return;

            waveRunning = false;

            if (wave >= totalWaves)            // 마지막 웨이브까지 클리어
            {
                AllCleared();
                return;
            }

            ScreenMessage.Show($"웨이브 {wave} 클리어!");
            nextWaveTime = Time.time + waveDelay;
            return;
        }

        // 3) 쉬는 시간이 끝나면 다음 웨이브 시작
        if (Time.time < nextWaveTime) return;
        StartWave();
    }

    void StartWave()
    {
        wave++;

        int melee = meleePerWave + meleeAddPerWave * (wave - 1);
        int ranged = rangedPerWave + rangedAddPerWave * (wave - 1);

        List<GameObject> list = new List<GameObject>();

        if (meleePrefab == null && melee > 0)
            Debug.LogWarning($"{name}: Melee Prefab이 비어 있습니다.");
        else
            for (int i = 0; i < melee; i++) list.Add(meleePrefab);

        if (rangedPrefab == null && ranged > 0)
            Debug.LogWarning($"{name}: Ranged Prefab이 비어 있습니다.");
        else
            for (int i = 0; i < ranged; i++) list.Add(rangedPrefab);

        if (shuffleOrder)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                GameObject tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        foreach (GameObject prefab in list) queue.Enqueue(prefab);

        waveRunning = true;
        timer = spawnGap;                      // 첫 마리는 바로 나오게

        ShowText("WAVE " + wave);
        ScreenMessage.Show($"웨이브 {wave} 시작!");
    }

    void SpawnOne(GameObject prefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"{name}: Spawn Points가 비어 있습니다.");
            return;
        }

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        if (point == null)
        {
            Debug.LogWarning($"{name}: Spawn Points에 빈 칸이 있습니다.");
            return;
        }

        Vector3 pos = FindGroundPosition(point);
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
        alive.Add(enemy);
    }

    void AllCleared()
    {
        cleared = true;
        ShowText("ALL CLEAR!");
        ScreenMessage.Show("모든 웨이브 클리어!");

        if (!string.IsNullOrEmpty(clearSceneName))
            Invoke(nameof(LoadClearScene), clearDelay);
    }

    void LoadClearScene()
    {
        SceneManager.LoadScene(clearSceneName);
    }

    void ShowText(string s)
    {
        if (waveText != null) waveText.text = s;
    }


    Vector3 FindGroundPosition(Transform point)
    {
        Vector3 origin = point.position + Vector3.up * rayStartHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance,
                            groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * spawnHeightOffset;
        }

        if (warned.Add(point)) ExplainMiss(point, origin);
        return point.position;
    }


    void ExplainMiss(Transform point, Vector3 origin)
    {
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit any, rayDistance,
                            ~0, QueryTriggerInteraction.Ignore))
        {
            Debug.LogWarning(
                $"{name}: '{point.name}' 아래에서 '{any.collider.name}'(레이어: {LayerMask.LayerToName(any.collider.gameObject.layer)})가 감지됐지만 Ground 레이어가 아니에요. " +
                "해당 오브젝트(자식 포함)의 Layer를 Ground로 바꾸세요.", point);
        }
        else
        {
            Debug.LogWarning(
                $"{name}: '{point.name}'(위치 {point.position}) 아래에 콜라이더가 하나도 없어요. " +
                "스폰 위치가 맵 밖이거나 맵에서 너무 멀어요. 맵 위로 옮겨 주세요.", point);
        }
    }
}