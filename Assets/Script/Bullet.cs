using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public float speed = 15f;            // 날아가는 속도
    public float lifeTime = 2f;          // 2초 뒤 삭제
    public string targetTag = "Enemy";   // 맞히면 데미지
    public string ownerTag = "Player";   // 쏜 쪽은 통과

    private Vector3 direction;
    private bool launched;

    // 인벤토리에서 발사할 때 호출 (방향과 데미지를 받음)
    public void Launch(Vector3 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;
        launched = true;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Launch로 발사된 총알만 스스로 이동 (다른 방식으로 움직이는 총알과 충돌 방지)
        if (launched)
            transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;              // 맵 아이템 같은 트리거는 통과
        if (other.CompareTag(ownerTag)) return;

        if (other.CompareTag(targetTag))
        {
            Health hp = other.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}