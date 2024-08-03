using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnderbarUIManager : MonoBehaviour
{
    public Button home;
    public Button timer;
    public Button list;
    public Button trophy;
    public Button config;
    public Button offConfig;
    public GameObject configUI;
    public ToDoListManager toDoListManager;
    void Start()
    {
        timer.onClick.AddListener(OnTimer);
        config.onClick.AddListener(OnConfig);
        offConfig.onClick.AddListener(OffConfig);
    }

    private void OnTimer()
    {
        toDoListManager.timerUI.SetActive(true);
        toDoListManager.toDoListUI.SetActive(false);
    }

    private void OnHome()
    {

    }
    private void OnConfig()
    {
        configUI.SetActive(true);
    }
    private void OffConfig()
    {
        configUI.SetActive(false);
    }
}
