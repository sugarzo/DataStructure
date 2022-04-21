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
    /// ��һ������
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
    /// ��ʼһ���߳�
    /// </summary>
    /// <param name="runlogic">�߳������߼�</param>
    /// <param name="callback">�߳�ִ����ɺ󣬻ص��������߳��߼�</param>
    public static void StartThread(Action runlogic,Action callback)
    {
        if(thread != null)
        {
            if (thread.IsAlive)
                thread.Abort();
        }
        Debug.Log("�½�����");
        thread = new Thread(delegate() { ThreadRun(runlogic, callback); });
        thread.Start();
    }
    private static void ThreadRun(Action runlogic, Action callback)
    {
        Debug.Log("�������");
        runlogic?.Invoke();
        Debug.Log("����ִ����ɣ�����ص�����");

        MainObject.Instance.ThreadAction += delegate () //���ص��¼�ί�У�д�����߳�
        {
            callback?.Invoke();
            Debug.Log("�ص�������ɣ���������");
        };

        thread.Abort(); //��ֹ�߳�
    }
}
