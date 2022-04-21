using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using System;

public class AuthorRankWindow : MonoBehaviour
{
    public Button btn_Query;
    public TMP_InputField InputField;
    public TMP_Text ShowArea;
    public TMP_Text ShowState;
    public Thread thread = null;
    public Action threadAction = null;
    public StateIcon stateIcon;

    public List<string> title = new List<string>();
    public List<int> rank = new List<int>();
    // Start is called before the first frame update
    void Start()
    {
        btn_Query.onClick.AddListener(QueryYear);
    }
    private int index = 0;
    private void OnEnable()
    {
        index = 0;
        ShowArea.text = "";
    }
    // Update is called once per frame
    void Update()
    {
        if(threadAction != null)
        {
            threadAction.Invoke();
            threadAction = null;
        }

    }

    void QueryYear()
    {
        int year = DXCode.StringToInt(InputField.text);
        MainObject.Instance.SetCanChange(false);
        btn_Query.interactable = false;
        ShowState.text = "Searching";
        stateIcon.SetState(StateIcon.State.Loading);
        if (thread != null && thread.IsAlive)
        {
            thread.Abort();
        }
        thread = new Thread(delegate ()
        {
            title.Clear();
            rank.Clear();
            Program.dllTool.GetSortYear(year, ref title, ref rank, 0, 10);

            threadAction += delegate() 
            {
                if(title.Count >= 10)
                    Print();
                ShowState.text = "Query";
                btn_Query.interactable = true;
                stateIcon.SetState(StateIcon.State.Finish);
                MainObject.Instance.SetCanChange(true);
            };
            thread.Abort();
        });
        thread.Start();
    }
    void Print()
    {
        ShowArea.text = "";
        for (int i = 1; i <= 10; i++)
        {
            ShowArea.text += "Top" + i + " " + title[i - 1] + " " + rank[i - 1] + "\n";
        }
    }
}
