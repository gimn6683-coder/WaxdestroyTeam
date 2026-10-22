using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public float speed = 15f;
    public float lifeTime = 4f;            // 맵이 커서 2초면 먼 적에게 못 감
    public string targetTag = "Enemy";     // 맞히면 데미지
    public string ownerTag = "Player";     // 쏜 쪽은 통과

    private Vector3 direction;
    private bool launched;

    // 인벤토리에서 발사할 때 호출
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
        if (launched)
            transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;              // 아이템 같은 트리거는 통과
        if (other.CompareTag(ownerTag)) return;   // 쏜 플레이어는 통과

        // 콜라이더가 자식 오브젝트여도 부모의 Health를 찾음
        Health hp = other.GetComponentInParent<Health>();
        if (hp != null && (other.CompareTag(targetTag) || hp.CompareTag(targetTag)))
            hp.TakeDamage(damage);

        Destroy(gameObject);
    }
}