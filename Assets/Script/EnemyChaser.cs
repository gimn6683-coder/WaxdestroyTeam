using UnityEngine;

public class EnemyChaser : MonoBehaviour
{
    public float moveSpeed = 2f;
    [Tooltip("이 거리 안이면 추적. 0이면 거리 상관없이 항상 추적")]
    public float detectRange = 0f;
    public float attackRange = 1.5f;  // 이 안이면 공격
    public int damage = 1;
    public float attackCooldown = 1f;
    [Tooltip("쫓아가서 공격할 대상의 태그")]
    public string targetTag = "Protect";

    Transform target;
    Health targetHealth;
    float lastAttackTime = -999f;

    void Start()
    {
        GameObject t = GameObject.FindWithTag(targetTag);
        if (t != null)
        {
            target = t.transform;
            targetHealth = t.GetComponent<Health>();
        }

    }

    void Update()
    {
        if (target == null) return;   // 대상이 없거나 파괴됐으면 정지

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0;               // 높이 무시
        float distance = toTarget.magnitude;

        if (detectRange > 0f && distance > detectRange) return; // 대기

        if (toTarget.sqrMagnitude > 0.0001f)
            transform.forward = toTarget.normalized;

        if (distance > attackRange)   // 추적
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else if (Time.time >= lastAttackTime + attackCooldown)
        {                             // 공격
            lastAttackTime = Time.time;
            if (targetHealth != null) targetHealth.TakeDamage(damage);
        }
    }
}