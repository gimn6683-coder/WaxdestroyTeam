using UnityEngine;

public class ItemAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip collectSound; 

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어와 부딪혔을 때
        if (other.CompareTag("Player"))
        {
            
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }

            // 아이템 오브젝트 삭제
            Destroy(gameObject);
        }
    }
}