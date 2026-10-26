using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab;    // EnemyBullet
    public Transform firePoint;
    public float moveSpeed = 2f;
    public float keepDistance = 8f;    // 대상 표면에서 여기까지만 다가옴
    public float fireRange = 12f;      // 이 안이면 발사
    public float fireInterval = 2f;
    public float bulletSpeed = 10f;


    [Header("공격 대상")]
    [Tooltip("공격할 대상의 태그")]
    public string targetTag = "Protect";
    [Tooltip("총알 한 발의 데미지")]
    public int damage = 1;


    [Header("지형 위를 떠다니기")]
    [Tooltip("지형으로 취급할 레이어. 비워 두면 'Ground' 레이어를 자동으로 사용")]
    public LayerMask groundMask;
    [Tooltip("지형 위로 띄울 높이")]
    public float hoverHeight = 3f;
    [Tooltip("앞쪽 지형 높이를 미리 확인하는 거리")]
    public float lookAhead = 2f;
    public float verticalSpeed = 8f;
    [Tooltip("둥실둥실 위아래로 흔들리는 크기 (0이면 끔)")]
    public float bobHeight = 0.3f;
    public float bobSpeed = 2f;
    public float rayStartHeight = 50f;
    public float rayDistance = 200f;

    Transform target;
    Collider targetCol;
    float lastFireTime;
    float bobOffset;

    void Start()
    {

        GameObject t = GameObject.FindWithTag(targetTag);
        if (t != null)
        {
            target = t.transform;
            targetCol = t.GetComponentInChildren<Collider>();
        }
        else
        {
            Debug.LogWarning($"{name}: '{targetTag}' 태그의 오브젝트를 찾지 못했습니다.");
        }

        if (groundMask.value == 0)
        {
            int g = LayerMask.NameToLayer("Ground");
            if (g >= 0) groundMask = 1 << g;
            else Debug.LogWarning($"{name}: Ground Mask가 비어 있고 'Ground' 레이어도 없습니다.");
        }

        bobOffset = Random.value * 10f;   // 여러 마리가 같은 박자로 흔들리지 않게
        lastFireTime = Time.time;
    }

    void Update()
    {
        if (target == null) return;


        Vector3 pos = transform.position;
        Vector3 closest = targetCol != null ? targetCol.bounds.ClosestPoint(pos) : target.position;
        Vector3 toTarget = closest - pos;
        toTarget.y = 0;
        float distance = toTarget.magnitude;
        Vector3 dir = distance > 0.001f ? toTarget / distance : transform.forward;

        if (distance > 0.001f) transform.forward = dir;

        if (distance > keepDistance)        // 다가가기
            pos += dir * moveSpeed * Time.deltaTime;

        float fallback = pos.y - hoverHeight;
        float ground = Mathf.Max(GroundY(pos, fallback), GroundY(pos + dir * lookAhead, fallback));
        float bob = bobHeight > 0f ? Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobHeight : 0f;
        pos.y = Mathf.MoveTowards(pos.y, ground + hoverHeight + bob, verticalSpeed * Time.deltaTime);
        transform.position = pos;

        bool inRange = distance < fireRange;
        bool ready = Time.time >= lastFireTime + fireInterval;
        if (inRange && ready)
        {
            lastFireTime = Time.time;
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning($"{name}: Bullet Prefab이 비어 있습니다.");
            return;
        }


        Vector3 origin = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up * 0.5f + transform.forward;


        Vector3 aimPoint = targetCol != null ? targetCol.bounds.center : target.position;
        Vector3 dir = (aimPoint - origin).normalized;

        GameObject go = Instantiate(bulletPrefab, origin, Quaternion.LookRotation(dir));

        Bullet bullet = go.GetComponent<Bullet>();
        if (bullet != null)
        {
            Rigidbody rbBullet = go.GetComponent<Rigidbody>();
            if (rbBullet != null) rbBullet.isKinematic = true;   // 좌표로 날아가니 물리와 겹치지 않게

            bullet.targetTag = targetTag;         // 프로텍트 오브젝트에 데미지
            bullet.ownerTag = gameObject.tag;     // 쏜 적(Enemy)과 다른 적은 통과
            bullet.speed = bulletSpeed;
            bullet.Launch(dir, damage);
        }
        else
        {
            // Bullet 스크립트가 없는 총알은 예전 방식(Rigidbody 속도)으로 발사
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = dir * bulletSpeed;
        }
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