using UnityEngine;

public class EnemyChaser : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float detectRange = 15f;   // 이 안이면 추적
    public float attackRange = 1.5f;  // 이 안이면 공격
    public int damage = 1;
    public float attackCooldown = 1f;

    Transform player;
    float lastAttackTime = -999f;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }
    void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;                     // 높이 무시
        float distance = toPlayer.magnitude;

        if (distance > detectRange) return; // 대기

        transform.forward = toPlayer.normalized;

        if (distance > attackRange)         // 추적
        {
            float move = moveSpeed * Time.deltaTime;
            transform.position += transform.forward * move;
        }
        else if (Time.time >= lastAttackTime + attackCooldown)
        {                                   // 공격
            lastAttackTime = Time.time;
            player.GetComponent<Health>().TakeDamage(damage);
        }
    }
}
