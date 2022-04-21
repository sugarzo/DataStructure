using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using System.Threading;
using System.IO;
public class FilenameManager : SingletonMono<FilenameManager>
{
    public Button Enter;
    public Button Loading;
    public Button Build;
    public Button Check;
    public Button btn_RestartThread;
    public InputField inputField;
    [Header("�ļ�������״̬Iron")]
    public StateIcon State_dblp;
    public StateIcon State_database;
    [Header("����״̬Iron")]
    public StateIcon State_InitBuild;
    public StateIcon State_AuthorSort;
    public StateIcon State_YearSort;
    public StateIcon State_Aua;
    public GameObject Panel_BuildInfo;

    private string path;
    private Thread thread = null;
    private Action threadAction = null;

    private void Update()
    {
        if(threadAction != null)
        {
            threadAction.Invoke();
            threadAction = null;
        }
    }


    private void Start()
    {
        

        btn_RestartThread.onClick.AddListener(RestartThread);
        FSM(State.Idle);

        Enter.onClick.AddListener(delegate ()
        {
            MainObject.Instance.OpenWindow(WindowType.Search);
        });
        Check.onClick.AddListener(delegate ()
        {
            if (inputField.text[inputField.text.Length - 1] != '\\')
                inputField.text += '\\';
            FSM(State.Checking);

            if(Program.dllTool.CheckFilePath(path) && CheckAu(path))
            {
                FSM(State.Finish);
            }
            else
            {
                if(Program.dllTool.CheckFilePath_DBLP(path))
                {
                    FSM(State.Ready);
                }
                else
                {
                    FSM(State.Error);
                }
            }
        });
        Build.onClick.AddListener(delegate ()
        {
            if (programState == State.Ready)
                FSM(State.Building);
        });

        

    }

    public enum State
    { 
        Null, //δ���õĳ�ʼֵ
        Idle, //Ĭ��
        Checking, //���·����
        Building, //������
        Finish, //���ݿ���ȷ��׼���������
        Error,  //·��������Ҫ��������·��
        Ready, //���ݿ�δ������������dblp,����׼������״̬
    }
    public State programState = State.Null;  //����ǰ״̬
    private Dictionary<State, IState> StateMachine;
    public void FSM(State state) //�л�״̬����
    {
        if (state == programState) //���״̬û�б仯
            return;
        StateMachine[programState].OnExit(); //ִ��ǰһ��״̬���˳��¼�
        programState = state;                //����״̬
        StateMachine[programState].OnEnter();//ִ����һ��״̬�Ľ����¼�
    }


    private class State_Empty : IState
    {
        public override void OnEnter()
        {

        }

        public override void OnExit()
        {

        }
    }
    public abstract class IState
    {
        public abstract void OnEnter(); //״̬����ʱ
        public abstract void OnExit();  //״̬�˳�ʱ
    }
    private class State_Idle : IState
    {
        public override void OnEnter()
        {
            Instance.Panel_BuildInfo.SetActive(false);
            Instance.State_dblp.SetState(StateIcon.State.Waiting);
            Instance.State_database.SetState(StateIcon.State.Waiting);
        }

        public override void OnExit()
        {

        }
    }
    private class State_Checking : IState
    {
        public override void OnEnter()
        {
            Instance.path = Instance.inputField.text;
            Instance.Check.gameObject.SetActive(false);
            Instance.Loading.gameObject.SetActive(true);
        }

        public override void OnExit()
        {
            Instance.Check.gameObject.SetActive(true);
            Instance.Loading.gameObject.SetActive(false);
        }
    }
    private class State_Building : IState
    {
        public override void OnEnter()
        {
            Instance.Panel_BuildInfo.SetActive(true);
            Instance.BuildDataBase();
        }

        public override void OnExit()
        {
            
        }
    }
    private class State_Finish : IState
    {
        public override void OnEnter()
        {
            Instance.State_dblp.SetState(StateIcon.State.Finish);
            Instance.State_database.SetState(StateIcon.State.Finish);
            GlobalManager.filePath = Instance.path;
            Instance.Enter.interactable = true;
            Instance.Loading.gameObject.SetActive(false);
        }

        public override void OnExit()
        {
            Instance.Enter.interactable = false;
        }
    }
    private class State_Error : IState
    {
        public override void OnEnter()
        {
            Instance.State_dblp.SetState(StateIcon.State.Error);
            Instance.State_database.SetState(StateIcon.State.Error);
        }

        public override void OnExit()
        {
            
        }
    }
    private class State_Ready : IState
    {
        public override void OnEnter()
        {
            Instance.State_dblp.SetState(StateIcon.State.Finish);
            Instance.State_database.SetState(StateIcon.State.Error);
            Instance.Build.interactable = true;
        }

        public override void OnExit()
        {
            Instance.Build.interactable = false;
        }
    }
    public void BuildDataBase()
    {
        if(Program.dllTool.CheckFilePath(path) && !CheckAu(path))
        {
            BuildDataBaseAu();
            return;
        }    

        GlobalManager.filePath = path;
        State_InitBuild.SetState(StateIcon.State.Waiting);
        State_AuthorSort.SetState(StateIcon.State.Waiting);
        State_YearSort.SetState(StateIcon.State.Waiting);
        State_Aua.SetState(StateIcon.State.Waiting);
        
        Check.interactable = false;
        Debug.Log("��ʼ�������ݿ�");

        if (Program.dllTool.CheckFilePath(path)) 
        {
            Debug.Log("���ݿ��Ѵ��ڣ�");
            return;
        }
        if (!DLL.ChechFilePathCorrect_dblp(path))
        {
            Debug.Log("ȱ��DBLP�ļ�");
            return;
        }

        btn_RestartThread.interactable = true;
        if (thread != null && thread.IsAlive)
        {
            thread.Abort();
        }
        thread = new Thread(delegate ()
        {
            threadAction += delegate ()
            {
                Debug.Log("���⹤�������ɣ�ִ����...");
                State_InitBuild.SetState(StateIcon.State.Loading);
            };

            DLL.initial_readers(16, 4, false, path);

            threadAction += delegate ()
            {
                State_InitBuild.SetState(StateIcon.State.Finish);
                Debug.Log("������ɣ�");
                Debug.Log("��ʼ�������");
                State_YearSort.SetState(StateIcon.State.Loading);
            };

            Program.dllTool.SortYear(path);

            threadAction += delegate ()
            {
                State_YearSort.SetState(StateIcon.State.Finish);
                Debug.Log("����������");
                Debug.Log("��ʼ��������");
                State_AuthorSort.SetState(StateIcon.State.Loading);
            };
            
            DLL.Author_Sort(path);

            threadAction += delegate ()
            {
                State_AuthorSort.SetState(StateIcon.State.Finish);
                Debug.Log("�����������");
                Debug.Log("��ʼ���ŷ���");
                State_Aua.SetState(StateIcon.State.Loading);
            };

            Agglomerate.ParallelSearch();

            threadAction += delegate ()
            {
                State_Aua.SetState(StateIcon.State.Finish);
                Debug.Log("���ŷ������");
                Debug.Log("������ɣ�");
                Check.interactable = true;
                btn_RestartThread.interactable = false;
            };

            thread.Abort();
        });
        thread.Start();
    }

    public void BuildDataBaseAu()
    {
        GlobalManager.filePath = path;
        State_InitBuild.SetState(StateIcon.State.Finish);
        State_AuthorSort.SetState(StateIcon.State.Finish);
        State_YearSort.SetState(StateIcon.State.Finish);

        State_Aua.SetState(StateIcon.State.Loading);

        Check.interactable = false;
        Debug.Log("��ʼ�������ݿ�Au");

        if (!DLL.ChechFilePathCorrect_dblp(path))
        {
            Debug.Log("ȱ��DBLP�ļ�");
            return;
        }

        btn_RestartThread.interactable = true;
        if (thread != null && thread.IsAlive)
        {
            thread.Abort();
        }
        thread = new Thread(delegate ()
        {
            Agglomerate.ParallelSearch();
            threadAction += delegate ()
            {
                State_Aua.SetState(StateIcon.State.Finish);
                Debug.Log("���ŷ������");
                Debug.Log("������ɣ�");
                Check.interactable = true;
                btn_RestartThread.interactable = false;
            };
            thread.Abort();
        });
        thread.Start();
    }
    public GameObject buttonPanel;
    private void OnEnable()
    {
        if(StateMachine == null)
        {
            StateMachine = new Dictionary<State, IState>();
            //ע��״̬��
            StateMachine.Add(State.Null, new State_Empty());
            StateMachine.Add(State.Idle, new State_Idle());
            StateMachine.Add(State.Building, new State_Building());
            StateMachine.Add(State.Checking, new State_Checking());
            StateMachine.Add(State.Error, new State_Error());
            StateMachine.Add(State.Finish, new State_Finish());
            StateMachine.Add(State.Ready, new State_Ready());
        }
        FSM(State.Idle);
        buttonPanel.SetActive(false);
    }
    private void OnDisable()
    {
        buttonPanel.SetActive(true);
    }
    private void RestartThread()
    {
        if(thread != null && thread.IsAlive)
        {
            Debug.LogWarning("������һ���߳�");
            thread.Abort();
            thread.Start();
        }
    }
    private bool CheckAu(string path)
    {
        return File.Exists(path + "database\\aua_rank_result.dat");
    }
}
/// <summary>
/// ���ŷ���
/// </summary>
public static class Agglomerate
{
    static Dictionary<int, List<string>> Discret = new Dictionary<int, List<string>>();
    public static void ParallelSearch()
    {
        Console.WriteLine("���鼯��ʼ�������ڴ濪ʼ");
        int fileIndex = 0; //�ļ��б��0-4095
        int authorIndex = 0; //���߱��
        Dictionary<string, int> Discretization = new Dictionary<string, int>(); //��������������ɢ��
        int[] Parallel = new int[3500000]; //300w���鼯     
        for (int i = 0; i < 3500000; i++) //���鼯��ʼ��
        {
            Parallel[i] = i;
        }
        Console.WriteLine("���鼯��ʼ�����");
        for (fileIndex = 0; fileIndex < 4096; fileIndex++)
        {
            try
            {
                var infile = File.ReadAllText(Program.filePath + "database\\author\\" + fileIndex + ".ini").Split('$');
                int titleAua = 0; //�����µĺ�������
                for (int auI = 1; auI < infile.Length; auI++) // ��������0��Ԫ�أ���Ϊ0��Ԫ���ǿ�ֵ
                {
                    if (infile[auI].Contains("\n")) //����л��з�����������Ԫ���Ǳ��⣬����Ϊ������
                    {
                        titleAua = 0; //�������к�
                    }
                    else
                    {
                        titleAua++;
                        if (!Discretization.ContainsKey(infile[auI]))
                        {
                            Discretization.Add(infile[auI], authorIndex); //����һ����ɢ�����
                            authorIndex++;
                        }
                        if (titleAua > 2) //��������߲��Ǹ������еĵ�һ������
                            ParallelNodeMerge(ref Parallel, Discretization[infile[auI]], Discretization[infile[auI - 1]]); //��ǰһ�����߽ڵ�ϲ�
                    }
                }
                if (fileIndex % 100 == 0)
                    Console.WriteLine("���鼯" + fileIndex + "���ļ���ɣ���¼������Ŀ-" + authorIndex + " ���鼯��Ŀ" + GetParallelCount(ref Parallel, authorIndex));

                //if(authorIndex >= 3000000)
                //     Parallel.;
            }
            catch
            {
                Console.WriteLine("δ�ɹ���" + fileIndex + "���ļ���");
                continue;
            }
        }
        Console.WriteLine("���鼯�㷨���-���鼯��Ŀ" + GetParallelCount(ref Parallel, authorIndex));
        Console.WriteLine("���鼯���д���ļ���...");

        File.WriteAllText(Program.filePath + "database\\aua_result.dat", "�������� " + authorIndex + "  �������� " + GetParallelCount(ref Parallel, authorIndex) + "\n");

        Console.WriteLine("ͳ�Ʋ��鼯����������...");
        List<int> aua = new List<int>(); //����100��Ҫͳ�Ƶľ��ű��
        Discret.Clear();
        List<string> content = new List<string>();
        int tot = 0;
        foreach (var pr in Discretization)
        {
            tot++;
            content.Add(pr.Key + " " + Findx(ref Parallel, Parallel[pr.Value]));
            if (!Discret.ContainsKey(Parallel[pr.Value]))
                Discret.Add(Parallel[pr.Value], new List<string>());
            Discret[Parallel[pr.Value]].Add(pr.Key);

            if (Discret[Parallel[pr.Value]].Count == 100)
                aua.Add(Parallel[pr.Value]);
        }
        File.AppendAllLines(Program.filePath + "database\\aua_result.dat", content);

        Console.WriteLine("����������100�ļ�����Ŀ��" + aua.Count);
        Console.WriteLine("�����а�����д���ļ���..." + aua.Count);

        aua.Sort(new cmp());
        File.WriteAllText(Program.filePath + "database\\aua_rank_result.dat", "������>100�ľ�����Ŀ " + aua.Count + "\n");
        if (!Directory.Exists(Program.filePath + "database\\aua_result"))
            Directory.CreateDirectory(Program.filePath + "database\\aua_result");

        foreach (var i in aua)
        {
            File.AppendAllText(Program.filePath + "database\\aua_rank_result.dat", i + " " + Discret[i].Count + "\n");
            File.WriteAllLines(Program.filePath + "database\\aua_result\\" + i + ".ini", Discret[i]);
        }
    }
    /// <summary>
    /// ���鼯�ڵ�ϲ�
    /// </summary>
    private static void ParallelNodeMerge(ref int[] Parallel, int a, int b)//���鼯�ڵ�ϲ�
    {
        if (Findx(ref Parallel, a) != Findx(ref Parallel, b))
            Parallel[b] = Findx(ref Parallel, a);
    }
    private static int Findx(ref int[] Parallel, int x) //���Ҳ��鼯���ڵ�
    {
        if (Parallel[x] != x)
            return Parallel[x] = Findx(ref Parallel, Parallel[x]);
        return x;
    }
    private static int GetParallelCount(ref int[] Parallel, int n) //��ѯ���鼯��ӵ�е��ܼ�����
    {
        int ans = 0;
        for (int i = 0; i < n; i++)
        {
            if (i == Findx(ref Parallel, i))
                ans++;
        }
        return ans;
    }
    class cmp : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            return Discret[y].Count - Discret[x].Count;
        }
    }

    

}


