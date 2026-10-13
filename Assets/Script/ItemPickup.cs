using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [HideInInspector] public int materialIndex; // 스포너가 설정

    void OnTriggerStay(Collider other) { TryPickup(other.gameObject); }
    void OnTriggerStay2D(Collider2D other) { TryPickup(other.gameObject); }

    void TryPickup(GameObject who)
    {
        if (!who.CompareTag("Player")) return;

        // 칸이 가득 차면 false -> 아이템은 남아 있고 다음 프레임에 재시도
        if (InventorySystem.Instance.AcquireMaterial(materialIndex))
            Destroy(gameObject);
    }
}