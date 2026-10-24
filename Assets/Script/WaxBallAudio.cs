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

    [Tooltip("이 레이어에 닿으면 소리를 내지 않음 (맵 바닥과 블록). 비워 두면 'Ground' 레이어를 자동으로 사용")]
    public LayerMask ignoreLayers;

    [Tooltip("Bullet 스크립트가 이미 총알을 삭제하므로 보통 끄세요")]
    public bool destroyOnHit = false;

    private bool played;   // 한 번만 재생

    void Start()
    {
        if (ignoreLayers.value == 0)
        {
            int g = LayerMask.NameToLayer("Ground");
            if (g >= 0) ignoreLayers = 1 << g;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;                                             // 아이템 같은 트리거는 통과
        if ((ignoreLayers.value & (1 << other.gameObject.layer)) != 0) return;   // 맵은 통과
        if (other.CompareTag(ownerTag)) return;                                  // 쏜 플레이어는 통과
        if (played) return;
        played = true;

        if (breakSound != null) PlaySound();

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
        src.spatialBlend = 0f;
        src.Play();
        Destroy(go, breakSound.length + 0.1f);
    }
}