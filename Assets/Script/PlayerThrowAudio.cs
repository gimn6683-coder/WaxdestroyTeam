using UnityEngine;

public class PlayerThrowAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip throwSound;

    // 오디오 소스가 없으면 자동으로 가져오기
    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // 외부(다른 던지기 스크립트)에서 이 함수를 호출하면 소리가 납니다!
    public void PlayThrowSound()
    {
        if (throwSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(throwSound);
        }
    }
}