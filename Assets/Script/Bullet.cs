using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public float speed = 15f;
    public float lifeTime = 4f;            // 맵이 커서 2초면 먼 적에게 못 감
    public string targetTag = "Enemy";     // 맞히면 데미지
    public string ownerTag = "Player";     // 쏜 쪽은 통과

    [Tooltip("이 레이어에 닿으면 무시하고 통과 (맵 바닥과 블록). 비워 두면 'Ground' 레이어를 자동으로 사용")]
    public LayerMask ignoreLayers;

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

        if (ignoreLayers.value == 0)
        {
            int g = LayerMask.NameToLayer("Ground");
            if (g >= 0) ignoreLayers = 1 << g;
        }
    }

    void Update()
    {
        if (launched)
            transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;                              // 아이템 같은 트리거는 통과
        if ((ignoreLayers.value & (1 << other.gameObject.layer)) != 0) return;   // 맵은 통과
        if (other.CompareTag(ownerTag)) return;                   // 쏜 플레이어는 통과

        // 콜라이더가 자식 오브젝트여도 부모의 Health를 찾음
        Health hp = other.GetComponentInParent<Health>();
        if (hp != null && (other.CompareTag(targetTag) || hp.CompareTag(targetTag)))
            hp.TakeDamage(damage);

        Destroy(gameObject);
    }
}