using UnityEngine;

[RequireComponent(typeof(Health))]
public class GameOverTrigger : MonoBehaviour
{
    [Tooltip("체력이 0이 되면 켤 GAME OVER 패널 (평소에는 꺼 두세요)")]
    public GameObject gameOverPanel;
    [Tooltip("패널을 켜기 전 대기 시간(초)")]
    public float delay = 1f;
    [Tooltip("대기하는 동안 화면에 띄울 메시지 (비우면 표시 안 함)")]
    public string message = "지키는 오브젝트가 파괴되었습니다!";
    [Tooltip("켜면 게임오버 때 게임을 멈춤 (적, 총알, 웨이브가 멈춰요)")]
    public bool pauseGame = true;

    Health health;
    bool triggered;

    void Start()
    {
        health = GetComponent<Health>();
        health.OnDied += OnDied;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);   // 시작할 때는 숨김
    }

    void OnDied()
    {
        if (triggered) return;   // 한 번만 실행
        triggered = true;

        if (!string.IsNullOrEmpty(message)) ScreenMessage.Show(message);

        // 오브젝트가 곧 삭제되면 Invoke가 취소되니 바로 표시
        if (delay <= 0f || health.destroyOnDeath) ShowPanel();
        else Invoke(nameof(ShowPanel), delay);
    }

    void ShowPanel()
    {
        if (gameOverPanel == null)
        {
            Debug.LogWarning("Game Over Panel이 연결되지 않았습니다.");
            return;
        }

        gameOverPanel.SetActive(true);

        // 게임 중 마우스를 숨겼다면 버튼을 누를 수 있게 보이게 함
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pauseGame) Time.timeScale = 0f;
    }

    void OnDestroy()
    {
        if (health != null) health.OnDied -= OnDied;
    }
}