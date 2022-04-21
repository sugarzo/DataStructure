using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.IO;


class Program
{
    public static string articleName = "Optimal Static Output Feedback Design By Using a Trust Region Interior Point Method";
    //public static string filePath = "D:\\chrome\\DBLPQuickBrowser-master\\DBLPQuickBrowser-master\\DBLPQuickBrowser\\Runable_Program\\src\\";
    public static string filePath { get { return GlobalManager.filePath; } }
    //public static string filePath = "D:\\2022_DataStructure\\dblp_DataBase\\";
    public static List<string> title = new List<string>();
    public static List<int> rank = new List<int>();
    /*
    static void Main(string[] args)
    {
        Console.WriteLine("Program Start Run");
        Console.WriteLine("当前的文件路径：{0}\n", filePath);
        bool temp = dllTool.CheckFilePath(filePath);
        Console.WriteLine("文件路径正确性检查结果" + temp);
        dllAr.RunLogic(articleName);
        string ac;
        while (true)
        {
            Console.WriteLine("输入数字选择搜索类型\n 1.文章名搜索 2.作者搜索 3.合作作者 4.模糊搜索初始化 5.模糊搜索 6.释放模糊搜索 7.年份排序 8.获取哈希值 9.年份排行查询 10.建库 11.作者排序 12.DPS");
            string opType = Console.ReadLine();
            ac = Console.ReadLine();


            if (opType == "1")
                dllAr.RunLogic(ac);
            if (opType == "2")
                dllAu.RunLogic(ac);
            if (opType == "3")
                dllTool.GetCoauthor(ac, filePath);
            if (opType == "4")
                dllTool.FuzzySearchInit(filePath);
            if (opType == "5")
                dllTool.FuzzySearch(ac);
            if (opType == "6")
                dllTool.FuzzySearchRelease();
            if (opType == "7")
                dllTool.SortYear();
            if (opType == "8")
                dllTool.GetHashCode(ac);
            if (opType == "9")
                dllTool.GetSortYear(DXCode.StringToInt(ac), ref title, ref rank);
            if (opType == "10")
                dllTool.BuildDataBase(filePath);
            if (opType == "11")
                dllTool.AuthorSort();
            if (opType == "12")
                dllTool.AuthorDFS();
        }

    }
    */
    public static DLL_SearchArticleTitle dllAr = new DLL_SearchArticleTitle();
    public static DLL_SearchAuthor dllAu = new DLL_SearchAuthor();
    public static DLL dllTool = new DLL();
}
/// <summary>
/// DLL接口，文章名搜索所有内容
/// </summary>
public class DLL_SearchArticleTitle
{
    [DllImport("Dll1")]
    public static extern int add(int num1, int num2);
    [DllImport("Dll1")]
    public static extern void articleInfo(String articleName, String saveUrl, StringBuilder pt);
    public StringBuilder content = new StringBuilder(50000);
    public void RunLogic(string acName,ref Dictionary<string, List<string>> nodeMap)
    {
        Console.WriteLine("搜索了文章： {0}", acName);
        articleInfo(acName, Program.filePath, content);
        Console.WriteLine(content);
        //Dictionary<string, List<string>> nodeMap = new Dictionary<string, List<string>>();
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
    public static extern void main_Reader(String authorName, String saveUrl, StringBuilder pt); //已按行划分
    public StringBuilder content = new StringBuilder(100000);

    public void RunLogic(string auName,ref List<string> title,ref List<string> coAuthor)
    {
        Console.WriteLine("开始搜索作者：" + auName);
        main_Reader(auName, Program.filePath, content);
        title = new List<string>(content.ToString().Split("\n")) ;
        if (title.Count > 1)
            coAuthor = new List<string>(Program.dllTool.GetCoauthor(auName, Program.filePath).Split("\n"));
    }
}
/// <summary>
/// DLL接口，对应DLL名字DLL1(LYC) DLL2(LJL)
/// </summary>
public class DLL
{
    public Thread thread;
    StringBuilder stringBuilder = new StringBuilder(50000);

    [DllImport("Dll1")]
    public static extern void articleInfo(String articleName, String saveUrl, StringBuilder pt);
    [DllImport("Dll1")]
    public static extern void main_Reader(String authorName, String saveUrl, StringBuilder pt);
    [DllImport("Dll1")]
    public static extern bool CheckFilePathCorrect_Compatible(string filePath, StringBuilder refS);
    [DllImport("Dll1")]
    public static extern bool CheckFilePathCorrect(string filePath);
    [DllImport("Dll1")]
    public static extern void CoAuthor(string search_name, string _saveUrl, StringBuilder pt);
    [DllImport("Dll1")]
    public static extern void initial_fuzzy(string filePath);
    [DllImport("Dll1")]
    public static extern void fuzzy_search(string filePath, uint threadNum, string content);
    [DllImport("Dll1")]
    public static extern void release_fuzzy_searcher();
    [DllImport("Dll1")]
    public static extern void Keyword_Sort(uint year, string filePath); //年份排序
    [DllImport("Dll2")]
    public static extern uint GetHash(string tar);
    [DllImport("Dll2")]
    public static extern bool ChechFilePathCorrect_dblp(string saveUrl);
    [DllImport("Dll1")]
    public static extern bool initial_readers(uint totalThread, uint maxThread, bool debugText, string saveUrl);//建库
    [DllImport("Dll1")]
    public static extern bool Author_Sort(string saveUrl); //作者排序
    [DllImport("Dll1")]
    public static extern int Full_word_match_fuzzy_search(string filePath, uint threadNum, string content);
    [DllImport("Dll1")]
    public static extern uint Format_Hash4(string tar); //哈希值
    /*
    using data_initial::Hash4;//通用字符串Hash函数          DWORD Hash4(const string& 需要hash的字符)
    using data_initial::initial_readers;//数据库初始化建库函数  bool initial_readers(const DWORD 总线程数(建议值:16), DWORD 最多同时运行线程数(建议值:4), bool 是否显示文本(建议值:False), char* 数据库地址)
    using Keyword_Sorting::Keyword_Sort;//年份热搜排序    void Keyword_Sort(DWORD 年份, char* 数据库地址)
    using Author_Sorting::Author_Sort;//作者发文量排序    bool Author_Sort(char* 数据库地址)
    using Elements_Searcher::articleInfo;//输入文章名，返回文章的所有相关信息    void articleInfo(char* 文章名, char* 数据库地址, char* 结果)
    using Elements_Searcher::CheckFilePathCorrect;//检查文件路径是否正确      bool CheckFilePathCorrect(char* 数据库地址)
    using Elements_Searcher::CheckFilePathCorrect_Compatible;//检查文件路径是否正确（兼容模式） bool CheckFilePathCorrect_Compatible(char* 数据库地址)
    using Elements_Searcher::main_Reader;//输入作者名字，返回作者发布的所有文章   void main_Reader(char* 作者名字, char* 数据库地址, char* 结果)
    using Elements_Searcher::CoAuthor;//输入作者名字，返回所有合作作者   void CoAuthor(char* 作者名字, char* 数据库地址, char* 结果)
    using Fuzzy_Searcher::initial_fuzzy;//初始化模糊搜索    void initial_fuzzy(char* 数据库地址)
    using Fuzzy_Searcher::fuzzy_search;//模糊搜索   bool fuzzy_search(char* 数据库地址, DWORD 线程数, char* 目标内容)
    using Fuzzy_Searcher::release_fuzzy_searcher;//释放模糊搜索的资源(需要重新初始化)    void release_fuzzy_searcher()
        */
    public bool CheckFilePath_DBLP(string filePath)
    {
        Console.WriteLine("检查DBLP存在结果" + ChechFilePathCorrect_dblp(filePath));
        return ChechFilePathCorrect_dblp(filePath);
    }
    /// <summary>
    /// 检查路径是否建库完成
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public bool CheckFilePath(string filePath)
    {
        CheckFilePathCorrect_Compatible(filePath, stringBuilder);
        Console.WriteLine(stringBuilder);
        return CheckFilePathCorrect_Compatible(filePath, stringBuilder);
    }
    /// <summary>
    /// 输入作者名，得到合作过的作者列表
    /// </summary>
    /// <param name="au_name"></param>
    /// <param name="_saveUrl"></param>
    /// <returns></returns>
    public string GetCoauthor(string au_name, string _saveUrl)
    {
        CoAuthor(au_name, _saveUrl, stringBuilder);
        string temp = stringBuilder.ToString();
        Console.WriteLine(temp);
        return temp;
    }
    /// <summary>
    /// 模糊搜索初始化（需要内存1G空余）
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="threadNum"></param>
    public void FuzzySearchInit(string filePath, uint threadNum = 8)
    {
        Console.WriteLine("开始初始化模糊搜索");
        initial_fuzzy(filePath);
        Console.WriteLine("初始化模糊搜索完成");
    }
    /// <summary>
    /// 模糊搜索（注意需要初始化），返回所有文章标题（按换行符分开）
    /// </summary>
    /// <param name="articleName"></param>
    /// <returns></returns>
    public string FuzzySearch(string articleName, bool isFullWord = false)
    {
        Console.WriteLine("开始模糊搜索");
        uint hashCode = 0;
        if (isFullWord)
        {
            Full_word_match_fuzzy_search(Program.filePath, 10, articleName);
            hashCode = Format_Hash4(articleName);
        }
        else
        {
            fuzzy_search(Program.filePath, 10, articleName);
            hashCode = GetHashCode(articleName);
        }
        Console.WriteLine("模糊搜索完成,开始读取文件");
        
        string ret = string.Empty;
        try
        {
            ret = File.ReadAllText(Program.filePath + "\\database\\searchlog\\" + hashCode + ".db");
        }
        catch
        {
            Console.WriteLine("文件读取失败");
            return ret;
        }
        Console.WriteLine("文件读取成功！\n" + ret);
        return ret;

    }
    /// <summary>
    /// 释放模糊搜索资源
    /// </summary>
    public void FuzzySearchRelease()
    {
        Console.WriteLine("释放模糊搜索资源");
        release_fuzzy_searcher();
        Console.WriteLine("释放模糊搜索资源完成");
    }
    /// <summary>
    /// 年度热点单词排序 1936-2022年自动排列
    /// </summary>
    public void SortYear(string path = "")
    {
        if (path == "")
            path = Program.filePath;
        for (int i = 1936; i <= 2022; i++)
        {
            Console.WriteLine("正在排列年份：" + i);
            Keyword_Sort((uint)(i), path);
        }
    }
    /// <summary>
    /// 得到单词的哈希值
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public uint GetHashCode(string str, bool isFormat = false)
    {
        if (isFormat)
        {
            return Format_Hash4(str);
        }
        else
        {
            Console.WriteLine(str + " 哈希值-" + GetHash(str));
            return GetHash(str);
        }
    }
    /// <summary>
    /// 输入年份，得到热点单词表
    /// </summary>
    /// <param name="year"></param>
    /// <param name="title"></param>
    /// <param name="rank"></param>
    /// <param name="startIndex"></param>
    /// <param name="length"></param>
    public void GetSortYear(int year, ref List<string> title, ref List<int> rank, int startIndex = 0, int length = 100)
    {
        Console.WriteLine("检测到{0}的年份查询", year);

        if (year < 1936 || year > 2022)
            return;

        title.Clear();
        rank.Clear();
        string file = string.Empty;
        try
        {
            file = File.ReadAllText(Program.filePath + "\\database\\year_sorted\\" + year.ToString() + ".db");
        }
        catch
        {
            Console.WriteLine("文件读取失败");
            return;
        }
        string code = string.Empty;
        int tot = 0;
        for (int i = 0; i < file.Length; i++)
        {
            //先处理数字
            int index = i;
            while (i < file.Length && file[i] != ' ')
            {
                i++;
            }
            tot = DXCode.StringToInt(file.Substring(index, i - index));
            i++; //跳过空格
            //再处理字符
            index = i;
            while (i < file.Length && file[i] != '\0')
            {
                i++;
            }
            if (i == index)
                break;

            code = file.Substring(index, i - index);
            if (startIndex > 0)
            {
                startIndex--;
                continue;
            }
            if (DXCode.IsIgnore(code))
            {
                continue;
            }
            //添加到list作为结果输出
            string ad = code.Replace(".", string.Empty);
            if(title.Contains(ad))
            {
                continue;
            }

            title.Add(ad);
            rank.Add(tot);
            Console.WriteLine(code.Replace(".", string.Empty) + " " + tot);
            length--;
            if (length <= 0)
                break;
        }
    }
    /// <summary>
    /// 建库
    /// </summary>
    public void BuildDataBase(string filePath, uint totalThread = 16, uint maxThread = 4, bool debugText = true)
    {
        if (CheckFilePath(filePath))
        {
            Console.WriteLine("数据库已存在！");
            return;
        }
        if (!ChechFilePathCorrect_dblp(filePath))
        {
            Console.WriteLine("缺少DBLP文件");
            return;
        }
        Console.WriteLine("建库工作检查完成，执行中...");
        initial_readers(totalThread, maxThread, debugText, filePath);
        Console.WriteLine("建库完成！");
        Console.WriteLine("开始年份排序");
        SortYear();
        Console.WriteLine("开始作者排序");
        Author_Sort(filePath);
        Console.WriteLine("作者排序完成");
        Console.WriteLine("建库完成！");
        //thread = new Thread(delegate () {});
        //thread.Start();

    }
    public void AuthorSort()
    {
        Author_Sort(Program.filePath);
    }

    public void AuthorDFS()
    {
        int coRunTime = 0;
        Dictionary<string, int> map = new Dictionary<string, int>();
        int mapIndex = 0; //聚团编号
        string authorText = null;
        try
        {
            authorText = File.ReadAllText(Program.filePath + "database\\author_rank_result");
            //Console.WriteLine(authorText);
        }
        catch
        {
            Console.WriteLine("未成功打开author_rank_result文件！");
            return;
        }
        var authorList = authorText.Split("\0");
        Console.WriteLine("作者数目：" + authorList.Length);
        for (int i = 0; i < authorList.Length; i++)
        {
            int spaceIndex = authorList[i].IndexOf(" ") + 1;
            authorList[i] = authorList[i].Substring(spaceIndex, authorList[i].Length - spaceIndex);
        }
        HashSet<string> flag = new HashSet<string>();
        HashSet<string> Inlist = new HashSet<string>();
        int tot = 0;
        foreach (var str_author in authorList)
        {
            tot++;
            if (map.ContainsKey(str_author))
                continue;
            //新建聚团编号
            map.Add(str_author, ++mapIndex);
            flag.Add(str_author);
            // Console.WriteLine("聚团编号{0}创建！", mapIndex);
            stringBuilder.Clear();
            CoAuthor(str_author, Program.filePath, stringBuilder);
            coRunTime++;
            List<string> coauthorList = new List<string>(stringBuilder.ToString().Split("\n"));

            foreach (var co in coauthorList)
            {
                if (!map.ContainsKey(co))
                    map.Add(co, mapIndex); //加入当前序号聚团
            }

            for (int i = 0; i < coauthorList.Count; i++)
            {
                // if (!map.ContainsKey(coauthorList[i]))
                //     map.Add(coauthorList[i], mapIndex); //加入当前序号聚团

                if (!flag.Contains(coauthorList[i]))
                {
                    stringBuilder.Clear();
                    CoAuthor(coauthorList[i], Program.filePath, stringBuilder);
                    coRunTime++;
                    if (coRunTime % 10000 == 0)
                        Console.WriteLine("CoAuthor执行" + coRunTime);
                    flag.Add(coauthorList[i]); //已经检测
                    var addList = stringBuilder.ToString().Split("\n");

                    foreach (var ad in addList)
                    {
                        if (!map.ContainsKey(ad))
                        {
                            map.Add(ad, mapIndex); //加入当前序号聚团
                            if (!flag.Contains(ad))
                            {
                                coauthorList.Add(ad); //同伙加入待检测序列
                                flag.Add(ad);
                            }

                        }
                    }

                }
            }
            //Console.WriteLine("当前人数数目：" + map.Count);
        }
        Console.WriteLine("聚团数目：" + map.Count);
    }
    public void ReadAuthorRank(ref List<int> rank, ref List<string> aList,int startPos = 0 ,int maxAuthorNum = 100)
    {
        rank.Clear();
        aList.Clear();

        string authorText = null;
        try
        {
            authorText = File.ReadAllText(Program.filePath + "database\\author_rank_result");
            //Console.WriteLine(authorText);
        }
        catch
        {
            Console.WriteLine("未成功打开author_rank_result文件！");
            return;
        }
        var authorList = authorText.Split("\0");
        List<int> rankList = new List<int>();
        Console.WriteLine("作者数目：" + authorList.Length);
        int maxNum = startPos * maxAuthorNum + maxAuthorNum;

        for (int i = startPos; i < maxNum && i < authorList.Length; i++)
        {
            int spaceIndex = authorList[i].IndexOf(" ");

            rank.Add(DXCode.StringToInt(authorList[i].Substring(0, spaceIndex)));

            aList.Add(authorList[i].Substring(spaceIndex + 1, authorList[i].Length - spaceIndex - 1));
        }

    }
}
/// <summary>
/// 一些转码Static函数
/// </summary>
public static class DXCode
{
    /// <summary>
    /// 将str在[index_1,index_2]区间的字符转换成数字,不支持负数
    /// </summary>
    /// <param name="str"></param>
    /// <param name="index_1"></param>
    /// <param name="index_2"></param>
    /// <returns></returns>
    public static int StringToInt(ref string str, int index_1, int index_2)
    {
        int ret = 0;
        int len = str.Length;
        while (index_1 < index_2 && index_1 < len)
        {
            var code = str[index_1];
            if (code > '0' && code <= '9')
            {
                ret = ret * 10 + (code - '0');
            }
            index_1++;
        }
        return ret;
    }
    public static int StringToInt(string str)
    {
        int ret = 0;
        foreach (var chr in str)
        {
            if (chr >= '0' && chr <= '9')
            {
                ret = ret * 10 + (chr - '0');
            }
        }
        // Console.WriteLine("StringToInt转换：{0}->{1}", str, ret);
        return ret;
    }
    static string[] strunim = { "a", "b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","A","An","Via",
    "t","u","v","w","x","y","z","an", "are", "all", "any", "been", "both", "each", "either", "one", "two", "three",
    "four", "five", "six", "seven", "eigth", "nine", "ten", "none", "little", "few", "many", "much",
    "other", "another", "some", "no", "every", "nobody", "anybody", "somebody", "everybody", "when", "on",
    "at", "as", "first", "secend", "third", "fouth", "fifth", "sixth", "ninth", "above", "over", "below",
    "under", "beside", "behind", "of", "the", "after", "from", "since", "for", "which", "by", "next",
    "where", "how", "who", "there", "where", "is", "was", "were", "do", "did", "this", "that", "in",
    "last", "tomorrow", "yesterday", "before", "because", "against", "except", "beyond", "along", "among",
    "but", "so", "towards", "to", "it", "me", "i", "he", "she", "his", "they", "them", "her", "its", "and",
    "has", "have", "my", "would", "then", "too", "or", "our", "off", "we", "be", "into", "weel", "can",
    "having", "being", "even", "us", "these", "those", "if", "ours", "with", "using", "the", "based","The","On","Using","-"};
    public static Dictionary<string, bool> strUnim = new Dictionary<string, bool>();
    public static void InitDic()
    {
        if (strUnim.Count > 0)
            return;
        foreach (var item in strunim)
        {
            if (!strUnim.ContainsKey(item))
                strUnim.Add(item, true);
        }
    }
    public static bool IsIgnore(string code)
    {
        if (strUnim.Count == 0)
            InitDic();
        return strUnim.ContainsKey(code);
    }
}

