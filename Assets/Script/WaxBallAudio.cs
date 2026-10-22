using UnityEngine;

public class WaxBallAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip breakSound; 
    
    private void OnTriggerEnter(Collider other)
    {
        
        if (!other.CompareTag("Player"))
        {
           
            if (breakSound != null)
            {
                AudioSource.PlayClipAtPoint(breakSound, transform.position);
            }

            
            Destroy(gameObject);
        }
    }
}