using UnityEngine;

public class EnemyHoverChaser : MonoBehaviour
{
    public float moveSpeed = 4f;
    [Tooltip("대상의 표면에서 이 거리 안이면 공격")]
    public float attackRange = 1.5f;
    public int damage = 1;
    public float attackCooldown = 1f;
    public string targetTag = "Protect";

    [Header("지형 위를 떠다니기")]
    [Tooltip("지형으로 취급할 레이어. 비워 두면 'Ground' 레이어를 자동으로 사용")]
    public LayerMask groundMask;
    [Tooltip("지형 위로 띄울 높이 (모델 피벗이 발 밑이면 0.5, 몸 중앙이면 키의 절반 이상)")]
    public float hoverHeight = 1f;
    [Tooltip("앞쪽 지형 높이를 미리 확인하는 거리 (블록 앞에서 미리 떠오름)")]
    public float lookAhead = 2f;
    [Tooltip("위아래로 움직이는 속도")]
    public float verticalSpeed = 8f;
    public float rayStartHeight = 50f;
    public float rayDistance = 200f;

    Transform target;
    Health targetHealth;
    Collider targetCol;
    float lastAttackTime = -999f;

    void Start()
    {
        GameObject t = GameObject.FindWithTag(targetTag);
        if (t != null)
        {
            target = t.transform;
            targetHealth = t.GetComponent<Health>();
            targetCol = t.GetComponentInChildren<Collider>();
        }
        else
        {
            Debug.LogWarning($"{name}: '{targetTag}' 태그의 오브젝트를 찾지 못했습니다.");
        }

        // 지형 레이어가 비어 있으면 'Ground' 레이어를 자동으로 사용
        if (groundMask.value == 0)
        {
            int g = LayerMask.NameToLayer("Ground");
            if (g >= 0) groundMask = 1 << g;
            else Debug.LogWarning($"{name}: Ground Mask가 비어 있고 'Ground' 레이어도 없습니다. 지형 높이를 읽지 못해요.");
        }
    }

    void Update()
    {
        if (target == null) return;

        Vector3 pos = transform.position;
        Vector3 closest = targetCol != null ? targetCol.bounds.ClosestPoint(pos) : target.position;
        Vector3 flat = closest - pos;
        flat.y = 0f;
        float distance = flat.magnitude;
        Vector3 dir = distance > 0.001f ? flat / distance : transform.forward;

        if (distance > 0.001f) transform.forward = dir;

        if (distance > attackRange)            // 추적
        {
            pos += dir * moveSpeed * Time.deltaTime;
        }
        else if (Time.time >= lastAttackTime + attackCooldown)
        {                                      // 공격
            lastAttackTime = Time.time;
            if (targetHealth != null) targetHealth.TakeDamage(damage);
        }

        // 지금 위치와 앞쪽 중 더 높은 지형 위로 떠오름
        float fallback = pos.y - hoverHeight;
        float ground = Mathf.Max(GroundY(pos, fallback), GroundY(pos + dir * lookAhead, fallback));
        pos.y = Mathf.MoveTowards(pos.y, ground + hoverHeight, verticalSpeed * Time.deltaTime);

        transform.position = pos;
    }

    float GroundY(Vector3 p, float fallback)
    {
        Vector3 origin = new Vector3(p.x, p.y + rayStartHeight, p.z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance,
                            groundMask, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return fallback;
    }
}