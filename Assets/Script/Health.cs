using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    public int maxHp = 3;
    [Tooltip("체력이 0이 되면 오브젝트 삭제 (적은 켜고, 프로텍트 오브젝트는 끄세요)")]
    public bool destroyOnDeath = true;

    public int CurrentHp { get; private set; }

    public event Action<int, int> OnHpChanged; // (현재 체력, 최대 체력)
    public event Action OnDied;                // 체력이 0이 됐을 때 (게임 오버 연결용)

    void Awake() { CurrentHp = maxHp; }

    public void TakeDamage(int amount)
    {
        if (CurrentHp <= 0) return;

        CurrentHp = Mathf.Max(0, CurrentHp - amount);
        OnHpChanged?.Invoke(CurrentHp, maxHp);

        if (CurrentHp <= 0)
        {
            OnDied?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
        }
    }
}