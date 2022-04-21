using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//保存一些全局设定
public static class GlobalManager
{
    public static string articleName = "Optimal Static Output Feedback Design By Using a Trust Region Interior Point Method";
    //public static string filePath = "D:\\chrome\\DBLPQuickBrowser-master\\DBLPQuickBrowser-master\\DBLPQuickBrowser\\Runable_Program\\src\\";
    public static string filePath = "D:\\2022_DataStructure\\dblp.xml\\dblp\\";
    //D:\chrome\DBLPQuickBrowser-master\DBLPQuickBrowser-master\DBLPQuickBrowser\Runable_Program\src\
    public static DLL_SearchArticleTitle dllAr = new DLL_SearchArticleTitle();
    public static DLL_SearchAuthor dllAu = new DLL_SearchAuthor();
}
