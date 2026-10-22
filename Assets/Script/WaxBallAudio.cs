using UnityEngine;

public class WaxBallAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip breakSound;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("켜면 거리와 상관없이 같은 크기로 들림(2D 소리). 끄면 카메라에서 멀수록 작아짐(3D 소리)")]
    public bool play2D = true;

    [Tooltip("이 태그에 닿으면 소리를 내지 않음 (쏜 플레이어)")]
    public string ownerTag = "Player";

    [Tooltip("Bullet 스크립트가 이미 총알을 삭제하므로 보통 끄세요")]
    public bool destroyOnHit = false;

    [Tooltip("켜면 Console에 재생 여부를 출력 (원인을 찾은 뒤 끄세요)")]
    public bool debugLog = true;

    private bool played;   // 한 번만 재생

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;              // 아이템 같은 트리거는 통과
        if (other.CompareTag(ownerTag)) return;   // 쏜 플레이어는 통과
        if (played) return;
        played = true;

        if (breakSound == null)
        {
            Debug.LogWarning("[WaxBallAudio] Break Sound가 비어 있습니다. 오디오 파일을 연결하세요.");
        }
        else
        {
            PlaySound();
            if (debugLog) Debug.Log($"[WaxBallAudio] 소리 재생: {other.name} 에 닿음");
        }

        if (destroyOnHit) Destroy(gameObject);
    }

    private void PlaySound()
    {
        if (!play2D)
        {
            // 3D 소리: 닿은 위치에서 재생, 카메라에서 멀수록 작아짐
            AudioSource.PlayClipAtPoint(breakSound, transform.position, volume);
            return;
        }

        // 2D 소리: 거리와 상관없이 같은 크기로 재생
        GameObject go = new GameObject("WaxBallBreakSound");
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = breakSound;
        src.volume = volume;
        src.spatialBlend = 0f;                       // 0 = 2D
        src.Play();
        Destroy(go, breakSound.length + 0.1f);       // 소리가 끝나면 정리
    }
}