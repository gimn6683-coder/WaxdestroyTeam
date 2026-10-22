using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [HideInInspector] public int materialIndex;   // 스포너가 설정

    [Header("Audio Settings")]
    public AudioClip collectSound;
    [Range(0f, 1f)] public float volume = 1f;

    void OnTriggerStay(Collider other) { TryPickup(other.gameObject); }
    void OnTriggerStay2D(Collider2D other) { TryPickup(other.gameObject); }

    void TryPickup(GameObject who)
    {
        if (!who.CompareTag("Player")) return;

        // 인벤토리가 받아야 성공 (칸이 가득 차면 false -> 아이템과 소리 모두 그대로)
        if (InventorySystem.Instance.AcquireMaterial(materialIndex))
        {
            PlayCollectSound();
            Destroy(gameObject);
        }
    }

    void PlayCollectSound()
    {
        if (collectSound == null) return;

        // 아이템이 곧 삭제되므로 별도 오브젝트에서 재생 (거리와 상관없이 같은 크기의 2D 소리)
        GameObject go = new GameObject("CollectSound");
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = collectSound;
        src.volume = volume;
        src.spatialBlend = 0f;
        src.Play();
        Destroy(go, collectSound.length + 0.1f);
    }
}