using UnityEngine;
using UnityEngine.Audio;

public class SoundVolumeController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    // 슬라이더 값에 따라 볼륨을 조절하는 함수
    public void SetSFXVolume(float sliderValue)
    {
        if (sliderValue <= 0.0001f)
        {
            audioMixer.SetFloat("SFXVol", -80f);
        }
        else
        {
            audioMixer.SetFloat("SFXVol", Mathf.Log10(sliderValue) * 20);
        }
    }
}