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

[System.Serializable]
public class TitleColorSet
{
    [LabelText("节点名称")]
    public string titleName;
    [LabelText("节点Color")]
    public Color color;
    [LabelText("允许按钮调用搜索")]
    public enum SearchActionType { None, Author, Article };
    public SearchActionType actionType;
}

public class SearchManager : SingletonMono<SearchManager>
{
    
    public List<TitleColorSet> _listTitleColorSets = new List<TitleColorSet>();
    public Dictionary<string, TitleColorSet> _mapTitleColor = new Dictionary<string, TitleColorSet>();

    [Header("Button")]
    public Button SearchButton;
    public Button LockButton;
    public Button btn_page;
    public InputField inputField;
    public Dropdown dropdown;
    [Header("Toggle")]
    public Toggle toggle_fulword;
    [Header("Text")]
    public GameObject NotFound;
    public Transform parentContent;
    public GameObject ShowAreaElement; //一个展示单元格

    public Text debugText;
    public string debugString = string.Empty;

    [ReadOnly]
    public List<SearchElement> searchElements = new List<SearchElement>();

    public enum SearchState
    { 
        Idle,Searching,Finish
    }
    private SearchState currentState = SearchState.Idle;
    public void SetState(SearchState State)
    {
        
        currentState = State;
        EventManager.EmitEvent(currentState.ToString());
    }

    private Thread thread;
    private List<Action> threadActions = new List<Action>(); //在主线程update中持续检测

    private void OnEnable()
    {
        init();
    }
    /// <summary>
    /// 数据初始化和重置UI设置
    /// </summary>
    private void init()
    {
        SetState(SearchState.Idle);
        ClearShowArea();
    }
    [Button]
    /// <summary>
    /// 清空展示区域
    /// </summary>
    private void ClearShowArea()
    {
        foreach(var show in searchElements)
        {
            Destroy(show.gameObject);
        }
        searchElements.Clear();
    }


    // Start is called before the first frame update
    void Start()
    {
        btn_page.onClick.AddListener(DLL_SearchFuzzyNextPage);
        SearchButton.onClick.AddListener(OnSearchButtonClick);
        EventManager.StartListening(SearchState.Idle.ToString(), OnStateChange);
        EventManager.StartListening(SearchState.Finish.ToString(), OnStateChange);
        EventManager.StartListening(SearchState.Searching.ToString(), OnStateChange);
        foreach (var colorSet in _listTitleColorSets)
        {
            _mapTitleColor.Add(colorSet.titleName, colorSet);
        }
    }

    // Update is called once per frame
    void Update()
    {
        debugText.text = debugString; //更新管理日记
        if (threadActions.Count > 0)
        {
            foreach(var action in threadActions)
            {
                action?.Invoke();
            }
            threadActions.Clear();
        }
    }
    /// <summary>
    /// 外部调用OnSearchButtonClick()接口
    /// </summary>
    public void Search(string input,int type)
    {
        inputField.text = input;
        dropdown.value = type;
        //dropdown.captionText.text = searchType;
        OnSearchButtonClick();
    }
    /// <summary>
    /// 当搜索按钮被按下时
    /// </summary>
    [Button]
    private void OnSearchButtonClick()
    {
        if (string.IsNullOrEmpty(inputField.text))
            return;

        SetState(SearchState.Searching);

        if(thread != null)
        {
            if(thread.IsAlive)
                thread.Abort();
            Debug.Log("更换线程");
        }
        if(dropdown.captionText.text == "文章搜索")
        {
            thread = new Thread(DLL_SearchTitle);
        }
        if (dropdown.captionText.text == "作者搜索")
        {
            thread = new Thread(DLL_SearchAuthor);
        }
        if (dropdown.captionText.text == "模糊搜索")
        {
            thread = new Thread(DLL_SearchFuzzy);
        }
        if (dropdown.captionText.text == "聚团编号搜索")
        {
            thread = new Thread(SearchAua);
        }
        thread.Start();
    }
    /// <summary>
    /// 当搜索状态被改变时调用
    /// </summary>
    private void OnStateChange()
    {
        Debug.Log("搜索状态已改变->" + currentState.ToString());

        if(currentState == SearchState.Idle)
        {
            MainObject.Instance.SetCanChange(true);
            SearchButton.gameObject.SetActive(true);
            LockButton.gameObject.SetActive(false);
            ClearShowArea();
        }
        if (currentState == SearchState.Searching)
        {
            MainObject.Instance.SetCanChange(false);
            debugString = "";
            SearchButton.gameObject.SetActive(false);
            LockButton.gameObject.SetActive(true);
            ClearShowArea();
        }
        if (currentState == SearchState.Finish)
        {
            MainObject.Instance.SetCanChange(true);
            SearchButton.gameObject.SetActive(true);
            LockButton.gameObject.SetActive(false);
        }
    }
    
    private void DLL_SearchTitle() //额外线程
    {
        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
        GlobalManager.dllAr.RunLogic(inputField.text,ref result);

        threadActions.Add(delegate() 
        { 
            SetState(SearchState.Finish);
            PrintResult(result);
        });
        thread.Abort();
    }
    private void DLL_SearchAuthor()
    {
        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
        List<string> au = new List<string>();
        List<string> co = new List<string>();
        GlobalManager.dllAu.RunLogic(inputField.text,ref au,ref co);

        foreach(var item in au)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;
            if (!result.ContainsKey("<title>"))
                result.Add("<title>", new List<string>());
            result["<title>"].Add(item);
        }
        foreach (var item in co)
        {
            if (string.IsNullOrWhiteSpace(item))
                continue;
            if (!result.ContainsKey("<author>"))
                result.Add("<author>", new List<string>());
            result["<author>"].Add(item);
        }
        threadActions.Add(delegate ()
        {
            SetState(SearchState.Finish);
            PrintResult(result);
        });
        thread.Abort();
    }
    private bool fuzzyInit = false;
    private List<string> listString = new List<string>();
    string FuzzyWord;
    /// <summary>
    /// 模糊搜索调用
    /// </summary>
    private void DLL_SearchFuzzy()
    {
        string content = inputField.text;
        FuzzyWord = inputField.text;

        

        pageIndex = 0;
        if (fuzzyInit == false)
        {
            fuzzyInit = true;
            debugString = "第一次加载模糊搜索，内存初始化中...";
            Program.dllTool.FuzzySearchInit(Program.filePath);
        }
        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
        debugString = "模糊搜索中...";
        if (content.Length <= 2)
        {
            toggle_fulword.isOn = true;
        }
        listString = new List<string>(Program.dllTool.FuzzySearch(content, toggle_fulword.isOn).Split("\n"));


        listString.RemoveAt(listString.Count - 1);//结尾换行删去
        var retList = new List<string>();

        if (listString.Count > 100)
        {
            for (int i = 0; i < 100; i++)
            {
                retList.Add(listString[i].Replace("\r",string.Empty));
                //Debug.Log((int)retList[i][list[i].Length - 1]);
            }
            debugString = "(结果0 - 100) 共计搜索结果：" + listString.Count;
        }
        else
        {
            for (int i = 0; i < listString.Count; i++)
            {
                retList.Add(listString[i].Replace("\r", string.Empty));
                //Debug.Log((int)retList[i][list[i].Length - 1]);
            }
            debugString = "模糊搜索完成，共计结果：" + listString.Count;
        }
        result.Add("<title>", retList);

        threadActions.Add(delegate ()
        {
            PrintResult(result);
            SetState(SearchState.Finish);
        });
        
        thread.Abort();
    }
    private int pageIndex = 0;
    /// <summary>
    /// 模糊搜索翻页
    /// </summary>
    private void DLL_SearchFuzzyNextPage()
    {
        if(listString.Count / 100 > pageIndex)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            var retList = new List<string>();

            pageIndex++;
            for (int i = pageIndex * 100; i < Math.Min(pageIndex * 100 + 100, listString.Count) ; i++)
            {
                retList.Add(listString[i].Replace("\r", string.Empty));
                //Debug.Log((int)retList[i][list[i].Length - 1]);
            }
            debugString = "(结果" + pageIndex * 100 + "-" + Math.Min(pageIndex * 100 + 100, listString.Count) + 
                ")共计搜索结果：" + listString.Count;
            ClearShowArea();
            result.Add("<title>", retList);
            PrintResult(result);
        }
    }
    /// <summary>
    /// 向ShowArea区域打印结果
    /// </summary>
    private void PrintResult(Dictionary<string,List<string>> value)
    {
        if(value.Count < 1) //如果结果少于1
        {
            NotFound.SetActive(true);
            return;
        }
        else
        {
            NotFound.SetActive(false);
        }
        foreach(var item in value) //遍历字典
        {
            var obj = Instantiate(ShowAreaElement);
            //obj.transform.parent = parentContent;
            obj.transform.SetParent(parentContent);
            obj.GetComponent<SearchElement>().Create(item.Key, item.Value);
            searchElements.Add(obj.GetComponent<SearchElement>());
        }
    }

    private void SearchAua()
    {
        string content = inputField.text;
        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

        List<string> _list = new List<string>();
        if(System.IO.File.Exists(Program.filePath + "database\\aua_result\\" + content + ".ini"))
        {
            _list = new List<string>(System.IO.File.ReadAllLines(Program.filePath + "database\\aua_result\\" + content + ".ini"));
            _list.RemoveAt(_list.Count - 1);//结尾换行删去
            result.Add("<author>", _list);
        }

        threadActions.Add(delegate ()
        {
            PrintResult(result);
            SetState(SearchState.Finish);
        });

        thread.Abort();
    }
}
/*
/// <summary>
/// DLL接口，文章名搜索所有内容
/// </summary>
public class DLL_SearchArticleTitle
{
    [DllImport("Dll1")]
    public static extern int add(int num1, int num2);
    [DllImport("Dll1")]
    public static extern void articleInfo(String articleName, String saveUrl, StringBuilder pt, StringBuilder state);
    public StringBuilder content = new StringBuilder(50000);

    public void RunLogic(string acName,ref string ResultContent,ref Dictionary<string, List<string>> result)
    {
        Debug.Log("搜索了文章："+ acName);

        articleInfo(acName, GlobalManager.filePath, content, SearchManager.Instance.debug);

        if (ResultContent != null)
            ResultContent = new string(content.ToString());
        //Debug.Log(content.ToString());
        //Console.WriteLine(content);

        Dictionary<string, List<string>> nodeMap = new Dictionary<string, List<string>>();
        List<string> nodeStack = new List<string>(); //节点栈
        string wap = content.ToString();

        int keyConut = 0;
        int valueConut = 0;
        while (wap.IndexOf("<") >= 0 && wap.IndexOf(">") >= 0)
        {
            var index1 = wap.IndexOf("<");
            var index2 = wap.IndexOf(">");
            var key = wap.Substring(index1, index2 - index1 + 1);

            if (key == "</article>" && keyConut + valueConut > 0)
            {
                wap = wap.Remove(0, index2 + 1);
                break;
            }

            if (key.Contains("</")) //如果是尾节点
            {
                StringRemove(ref key);
                keyConut++;
                if (!nodeMap.ContainsKey(key)) //新建字典键值对和链表
                {
                    //Console.WriteLine("存在不规范的{0}", key);
                    wap = wap.Remove(0, index2 + 1);
                    continue;
                }
                var value = wap.Substring(0, index1);
                nodeMap[key].Add(value);
                //Console.WriteLine("添加了隶属与{1}的新value - {0}", value,key);

                nodeStack.RemoveAt(nodeStack.Count - 1);
            }
            else //新的头节点
            {
                StringRemove(ref key);
                valueConut++;
                if (!nodeMap.ContainsKey(key)) //新建字典键值对和链表
                {
                    //Console.WriteLine("添加了新Key - {0}", key);
                    nodeMap.Add(key, new List<string>());
                }
                //压入节点栈
                nodeStack.Add(key);
            }

            wap = wap.Remove(0, index2 + 1);
        }

        //便利字典，输出
        foreach (var i in nodeMap)
        {
            Console.WriteLine(i.Key);
            var _list = i.Value;
            foreach (var j in _list)
            {
                Console.WriteLine("\t" + j);
            }
        }
        if(result != null)
        {
            result = new Dictionary<string, List<string>>(nodeMap);
        }
    }

    /// <summary>
    /// 得到某节点的内容
    /// </summary>
    /// <param name="acName"></param>
    /// <param name="_nodeName"></param>
    /// <returns></returns>
    public void GetNodeList(string acName, string _nodeName, ref List<string> ret)
    {

        Console.WriteLine("搜索了文章： {0}", acName);
        articleInfo(acName, GlobalManager.filePath, content, SearchManager.Instance.debug);

        Dictionary<string, List<string>> nodeMap = new Dictionary<string, List<string>>();
        List<string> nodeStack = new List<string>(); //节点栈
        string wap = content.ToString();

        int keyConut = 0;
        int valueConut = 0;
        while (wap.IndexOf("<") >= 0 && wap.IndexOf(">") >= 0)
        {
            var index1 = wap.IndexOf("<");
            var index2 = wap.IndexOf(">");
            var key = wap.Substring(index1, index2 - index1 + 1);

            if (key == "</article>" && keyConut + valueConut > 0)
            {
                wap = wap.Remove(0, index2 + 1);
                break;
            }

            if (key.Contains("</")) //如果是尾节点
            {
                StringRemove(ref key);
                keyConut++;
                if (!nodeMap.ContainsKey(key)) //新建字典键值对和链表
                {
                    //Console.WriteLine("存在不规范的{0}", key);
                    wap = wap.Remove(0, index2 + 1);
                    continue;
                }
                var value = wap.Substring(0, index1);
                nodeMap[key].Add(value);
                //Console.WriteLine("添加了隶属与{1}的新value - {0}", value,key);

                nodeStack.RemoveAt(nodeStack.Count - 1);
            }
            else //新的头节点
            {
                StringRemove(ref key);
                valueConut++;
                if (!nodeMap.ContainsKey(key)) //新建字典键值对和链表
                {
                    //Console.WriteLine("添加了新Key - {0}", key);
                    nodeMap.Add(key, new List<string>());
                }
                //压入节点栈
                nodeStack.Add(key);
            }

            wap = wap.Remove(0, index2 + 1);
        }

        if (nodeMap.ContainsKey(_nodeName))
        {
            //ret.AddRange(nodeMap[_nodeName]);
            //查重
            var _lists = nodeMap[_nodeName];
            foreach (var aut in _lists)
            {
                if (!ret.Exists(x => x == aut))
                {
                    ret.Add(aut);
                }
            }
        }
    }

    private void StringRemove(ref string ins)
    {
        //ins = ins.Replace("<", string.Empty);
        //ins = ins.Replace(">", string.Empty);
        ins = ins.Replace("/", string.Empty);
    }

}
/// <summary>
/// DLL接口，作者搜索所有的文章以及合作作者
/// </summary>
public class DLL_SearchAuthor
{
    [DllImport("Dll1")]
    public static extern int add(int num1, int num2);
    [DllImport("Dll1")]
    public static extern void main_Reader(String authorName, String saveUrl, StringBuilder pt,StringBuilder state); //已按行划分
    public StringBuilder content = new StringBuilder(50000);

    public void RunLogic(string auName, ref List<string> resultArticle, ref List<string> resultCoauthor)
    {
        Console.WriteLine("开始搜索作者：" + auName);
        main_Reader(auName, GlobalManager.filePath, content, SearchManager.Instance.debug);

        //输出文章
        resultArticle = new List<string>(content.ToString().Split("\n"));
        resultArticle.Remove(" ");
        resultArticle.Remove(string.Empty);
        List<string> authorList = new List<string>();
        if (resultArticle.Count > 0)
        {
            foreach (var str in resultArticle)
            {
                GlobalManager.dllAr.GetNodeList(str, "<author>", ref authorList); //遍历作者发布的文章，把所有的Author拉进来
            }
        }
        
        //输出合作作者
        authorList.Remove(auName); //先把自己去除了
        resultCoauthor = new List<string>(authorList);
    }

}
*/
