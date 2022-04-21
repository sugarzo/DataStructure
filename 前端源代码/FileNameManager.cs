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
    [Header("文件完整性状态Iron")]
    public StateIcon State_dblp;
    public StateIcon State_database;
    [Header("建库状态Iron")]
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
        Null, //未设置的初始值
        Idle, //默认
        Checking, //检查路径中
        Building, //建库中
        Finish, //数据库正确，准备进入程序
        Error,  //路径错误，需要重新配置路径
        Ready, //数据库未建立，但是有dblp,进入准备建库状态
    }
    public State programState = State.Null;  //程序当前状态
    private Dictionary<State, IState> StateMachine;
    public void FSM(State state) //切换状态函数
    {
        if (state == programState) //如果状态没有变化
            return;
        StateMachine[programState].OnExit(); //执行前一个状态的退出事件
        programState = state;                //更换状态
        StateMachine[programState].OnEnter();//执行下一个状态的进入事件
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
        public abstract void OnEnter(); //状态进入时
        public abstract void OnExit();  //状态退出时
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
        Debug.Log("开始建立数据库");

        if (Program.dllTool.CheckFilePath(path)) 
        {
            Debug.Log("数据库已存在！");
            return;
        }
        if (!DLL.ChechFilePathCorrect_dblp(path))
        {
            Debug.Log("缺少DBLP文件");
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
                Debug.Log("建库工作检查完成，执行中...");
                State_InitBuild.SetState(StateIcon.State.Loading);
            };

            DLL.initial_readers(16, 4, false, path);

            threadAction += delegate ()
            {
                State_InitBuild.SetState(StateIcon.State.Finish);
                Debug.Log("建库完成！");
                Debug.Log("开始年份排序");
                State_YearSort.SetState(StateIcon.State.Loading);
            };

            Program.dllTool.SortYear(path);

            threadAction += delegate ()
            {
                State_YearSort.SetState(StateIcon.State.Finish);
                Debug.Log("年份排序完成");
                Debug.Log("开始作者排序");
                State_AuthorSort.SetState(StateIcon.State.Loading);
            };
            
            DLL.Author_Sort(path);

            threadAction += delegate ()
            {
                State_AuthorSort.SetState(StateIcon.State.Finish);
                Debug.Log("作者排序完成");
                Debug.Log("开始聚团分析");
                State_Aua.SetState(StateIcon.State.Loading);
            };

            Agglomerate.ParallelSearch();

            threadAction += delegate ()
            {
                State_Aua.SetState(StateIcon.State.Finish);
                Debug.Log("聚团分析完成");
                Debug.Log("建库完成！");
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
        Debug.Log("开始建立数据库Au");

        if (!DLL.ChechFilePathCorrect_dblp(path))
        {
            Debug.Log("缺少DBLP文件");
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
                Debug.Log("聚团分析完成");
                Debug.Log("建库完成！");
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
            //注册状态机
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
            Debug.LogWarning("重启了一次线程");
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
/// 聚团分析
/// </summary>
public static class Agglomerate
{
    static Dictionary<int, List<string>> Discret = new Dictionary<int, List<string>>();
    public static void ParallelSearch()
    {
        Console.WriteLine("并查集初始化分配内存开始");
        int fileIndex = 0; //文件夹编号0-4095
        int authorIndex = 0; //作者编号
        Dictionary<string, int> Discretization = new Dictionary<string, int>(); //作者名字数据离散化
        int[] Parallel = new int[3500000]; //300w并查集     
        for (int i = 0; i < 3500000; i++) //并查集初始化
        {
            Parallel[i] = i;
        }
        Console.WriteLine("并查集初始化完成");
        for (fileIndex = 0; fileIndex < 4096; fileIndex++)
        {
            try
            {
                var infile = File.ReadAllText(Program.filePath + "database\\author\\" + fileIndex + ".ini").Split('$');
                int titleAua = 0; //该文章的合作作者
                for (int auI = 1; auI < infile.Length; auI++) // 这里跳过0号元素，因为0号元素是空值
                {
                    if (infile[auI].Contains("\n")) //如果有换行符，即该数组元素是标题，否则为作者名
                    {
                        titleAua = 0; //重置序列号
                    }
                    else
                    {
                        titleAua++;
                        if (!Discretization.ContainsKey(infile[auI]))
                        {
                            Discretization.Add(infile[auI], authorIndex); //分配一个离散化编号
                            authorIndex++;
                        }
                        if (titleAua > 2) //如果该作者不是该文章中的第一个作者
                            ParallelNodeMerge(ref Parallel, Discretization[infile[auI]], Discretization[infile[auI - 1]]); //与前一个作者节点合并
                    }
                }
                if (fileIndex % 100 == 0)
                    Console.WriteLine("并查集" + fileIndex + "号文件完成，记录作者数目-" + authorIndex + " 并查集数目" + GetParallelCount(ref Parallel, authorIndex));

                //if(authorIndex >= 3000000)
                //     Parallel.;
            }
            catch
            {
                Console.WriteLine("未成功打开" + fileIndex + "号文件！");
                continue;
            }
        }
        Console.WriteLine("并查集算法完成-并查集数目" + GetParallelCount(ref Parallel, authorIndex));
        Console.WriteLine("并查集结果写入文件中...");

        File.WriteAllText(Program.filePath + "database\\aua_result.dat", "总作者数 " + authorIndex + "  聚团总数 " + GetParallelCount(ref Parallel, authorIndex) + "\n");

        Console.WriteLine("统计并查集集合排行中...");
        List<int> aua = new List<int>(); //大于100需要统计的聚团编号
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

        Console.WriteLine("作者数大于100的集合数目：" + aua.Count);
        Console.WriteLine("将排行榜数据写入文件中..." + aua.Count);

        aua.Sort(new cmp());
        File.WriteAllText(Program.filePath + "database\\aua_rank_result.dat", "作者数>100的聚团数目 " + aua.Count + "\n");
        if (!Directory.Exists(Program.filePath + "database\\aua_result"))
            Directory.CreateDirectory(Program.filePath + "database\\aua_result");

        foreach (var i in aua)
        {
            File.AppendAllText(Program.filePath + "database\\aua_rank_result.dat", i + " " + Discret[i].Count + "\n");
            File.WriteAllLines(Program.filePath + "database\\aua_result\\" + i + ".ini", Discret[i]);
        }
    }
    /// <summary>
    /// 并查集节点合并
    /// </summary>
    private static void ParallelNodeMerge(ref int[] Parallel, int a, int b)//并查集节点合并
    {
        if (Findx(ref Parallel, a) != Findx(ref Parallel, b))
            Parallel[b] = Findx(ref Parallel, a);
    }
    private static int Findx(ref int[] Parallel, int x) //查找并查集父节点
    {
        if (Parallel[x] != x)
            return Parallel[x] = Findx(ref Parallel, Parallel[x]);
        return x;
    }
    private static int GetParallelCount(ref int[] Parallel, int n) //查询并查集中拥有的总集合数
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


