using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject gameClearPanel; // ⭐ 1. 게임클리어 판넬 변수 추가

    [Header("Scene Names")]
    public string titleSceneName = "Title";
    public string nextSceneName;      // ⭐ 2. 다음 스테이지 씬 이름 (인스펙터에서 입력 가능)

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;      // 다시 불러와도 시간 정상
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ⭐ 3. 게임클리어 함수 추가 (게임오버와 판넬만 다르고 구성이 같아요!)
    public void GameClear()
    {
        gameClearPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Retry()
    {
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
    }

    // ⭐ 4. 다음 스테이지로 이동하는 함수 추가
    public void NextStage()
    {
        Time.timeScale = 1f; // 씬 넘어가기 전 시간 복구
        SceneManager.LoadScene(nextSceneName);
    }

    public void GoTitle()
    {
        Time.timeScale = 1f; // 씬 넘어가기 전 시간 복구
        SceneManager.LoadScene(titleSceneName);
    }
}