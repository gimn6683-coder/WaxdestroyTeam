using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public GameObject SettingPanel;

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
        SceneManager.LoadScene("game scene");
    }
    public void QuitButtonAction()
    {
        Application.Quit();
        Debug.Log("게임 종료");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
