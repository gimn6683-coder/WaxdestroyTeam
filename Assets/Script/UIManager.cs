using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public GameObject SettingPanel;


    public string gameSceneName = "game scene";
    public string titleSceneName = "TitleScene";

    public void OpenHelpPanel()
    {
        SettingPanel.SetActive(true);
    }

    public void CloseHelpPanel()
    {
        SettingPanel.SetActive(false);
    }

    public void StartButtonAction()
    {
        SceneManager.LoadScene(gameSceneName);
    }


    public void RestartButtonAction()
    {
        Time.timeScale = 1f;   // 게임오버 때 멈춰 둔 시간을 되돌림
        SceneManager.LoadScene(gameSceneName);
    }


    public void TitleButtonAction()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }

    public void QuitButtonAction()
    {
        Application.Quit();
        Debug.Log("게임 종료");
    }

    void Start()
    {

    }

    void Update()
    {

    }
}