using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.Events;
using TigerForge;
using System;
using System.Runtime.InteropServices;
using System.Text;
using Sirenix.OdinInspector;
public enum WindowType
{ 
    Search,FileCheck,Ranking_Year,Ranking_Author,AuaRanking
}
[System.Serializable]
public class WindowObject
{
    public WindowType type;
    public GameObject instance;
    public Button selectButton;
}


public class MainObject : SingletonMono<MainObject>
{
    public List<WindowObject> listWindow = new List<WindowObject>();
    public Button QuitGame;
    public bool canChange = true;
    public Action ThreadAction = null;
    public void SetCanChange(bool cc)
    {
        foreach (var btn in listWindow)
        {
            if (btn.selectButton != null)
                btn.selectButton.interactable = cc;
        }
        canChange = cc;
    }

    void Start()
    {
        QuitGame.onClick.AddListener(() => Application.Quit());
        Init();
    }
    void Init()
    {
        foreach(var btn in listWindow)
        {
            if(btn.selectButton != null)
                btn.selectButton.onClick.AddListener(delegate () { OpenWindow(btn.type); });
        }
    }
    /// <summary>
    /// 打开一个窗口
    /// </summary>
    public void OpenWindow(WindowType windowType)
    {
        foreach (var win in listWindow)
        {
            if (win.type == windowType)
            {
                win.instance.SetActive(true);
            }
            else
            {
                win.instance.SetActive(false);
            }
        }
    }
    private void Update()
    {
        if(ThreadAction != null)
        {
            ThreadAction.Invoke();
            ThreadAction = null;
        }
    }


}
public static class ThreadManager
{ 
    private static Thread thread = null;
    /// <summary>
    /// 开始一个线程
    /// </summary>
    /// <param name="runlogic">线程运行逻辑</param>
    /// <param name="callback">线程执行完成后，回调函数主线程逻辑</param>
    public static void StartThread(Action runlogic,Action callback)
    {
        if(thread != null)
        {
            if (thread.IsAlive)
                thread.Abort();
        }
        Debug.Log("新建进程");
        thread = new Thread(delegate() { ThreadRun(runlogic, callback); });
        thread.Start();
    }
    private static void ThreadRun(Action runlogic, Action callback)
    {
        Debug.Log("进入进程");
        runlogic?.Invoke();
        Debug.Log("进程执行完成，进入回调函数");

        MainObject.Instance.ThreadAction += delegate () //将回调事件委托，写入主线程
        {
            callback?.Invoke();
            Debug.Log("回调函数完成，结束进程");
        };

        thread.Abort(); //终止线程
    }
}
