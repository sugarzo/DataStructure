/*
------------------------------------
���ݽṹ����ҵ
Code By LYC
202030444483 �������ѧ�뼼��2��
------------------------------------
*/
#define Elements_Searcher_EXPORTS
#include "Elements_Searcher.h"

namespace Elements_Searcher {


    inline string Hash4(const string& tar) {
        DWORD64 ans = 0;
        for (int i = 0; i < tar.length(); i++) {
            ans = ((ans >> 8) & 0xf) ^ ((ans << 4) ^ tar[i]);
        }
        ans = ans & 0xfff;
        return to_string(ans);
    }

    void Strcpy(char* pt, const string& p2)
    {
        strcpy_s(pt, p2.length() + 1, p2.c_str());
    }

    void Strcpy(char* pt, const char* p2)
    {
        string temp = p2;
        strcpy_s(pt, temp.length() + 1, temp.c_str());
    }

    size_t stringToLonglong(string str)
    {
        size_t ret = 0;
        for (auto i : str)
            ret = ret * 10 + i - '0';
        return ret;
    }

    bool check_TailElem(string tempstr)//β�ڵ��ж�
    {
        if ((tempstr.find("</article>") != string::npos) || (tempstr.find("</inproceedings>") != string::npos) ||
            (tempstr.find("</proceedings>") != string::npos) || (tempstr.find("</book>") != string::npos) ||
            (tempstr.find("</incollection>") != string::npos) || (tempstr.find("</phdthesis>") != string::npos) ||
            (tempstr.find("</mastersthesis>") != string::npos) || (tempstr.find("</www>") != string::npos))
            return false;
        return true;
    };

    /// �������������������µ����������Ϣ
    /// <param name="_articleName">������</param>
    /// <param name="_saveUrl">�ļ�����·��</param>
    /// <param name="pt">����������</param>
    void articleInfo(char* _articleName, char* _saveUrl, char* pt)
    {
        string articleName = _articleName;
        string saveUrl = _saveUrl;
        string ret = ""; //����ֵ
        string flag = Hash4(articleName);//�ļ�λ��
        ifstream infile(saveUrl + "database\\article\\" + flag + ".ini", ios::in); //�򿪶�Ӧ�ĵ�

        //·�����󣬷��ر�����ʾ
        if (!infile){
            Strcpy(pt, "");
            return;
        }

        //�ҵ���ϣ�ļ������¶�Ӧ��ƫ����
        size_t urlpt = 0;
        string tempstr;
        getline(infile, tempstr);

        while (!infile.eof()) {
            //if (tempstr.find(articleName + " <-> ") != string::npos)//Bug�޸���
            if (tempstr.substr(0, articleName.length() + 5) == articleName + " <-> ")
            {
                size_t startPos = articleName.length() + 5;//��ȡλ�ú�+1
                size_t endPos = tempstr.size() - 1;//����λ��
                string d = tempstr.substr(startPos, endPos);
                urlpt = stringToLonglong(d);
                break;
            }
            getline(infile, tempstr);
        }
        infile.close();

        if (urlpt == 0)
        {
            Strcpy(pt, "");
            return;
        }

        //��dblp�ļ���ͨ��ƫ�����ƶ�ָ�룬Ȼ�󷵻ظ������������
        int tot = 0;
        if (urlpt != 0)
        {
            ifstream indblp(saveUrl + "dblp.xml", ios::in | ios::binary);
            indblp.seekg(urlpt, ios::beg);
            //����ƫ����
            char tmp = indblp.get();
            while (tmp != '<') {
                //cout << tmp<<" "<< indblp.tellg()<<endl;
                indblp.seekg(--urlpt);
                tmp = indblp.get();
            }
            indblp.seekg(urlpt, ios::beg);
            getline(indblp, tempstr);

            //ֱ���ҵ����½�β������ѭ��
            while ((check_TailElem(tempstr) || tot <= 3) && !indblp.eof())//�������3-1000��
            {
                if (!check_TailElem(tempstr) && tempstr.length() <= 17) {
                    tot++;
                    if (tot > 1000) //��ֹ���������ѭ��
                        break;
                    getline(indblp, tempstr);
                    continue;
                }
                ret += tempstr + "\n";
                tot++;
                if (tot > 1000) //��ֹ���������ѭ��
                    break;
                getline(indblp, tempstr);
            }
            //ret += tempstr + "\n";
        }
        //�������c#�Խ�
        //һ��Ҫʹ��3��������strcpy_s����Ȼ�ᱨ��strcpyҲ����ɵ��vs��
        //ԭ��https://blog.csdn.net/leowinbow/article/details/82380252
        if (ret.length() > 0)
        {
            strcpy_s(pt, ret.length() + 1, ret.c_str()); //�����������
        }
        else
        {
            Strcpy(pt, "");
        }
        return;
    }
    /// �����������֣��������߷�������������
    /// <param name="searchName">��������</param>
    /// <param name="_saveUrl">�ļ�����·��</param>
    /// <param name="pt">������</param>
    void main_Reader(char* searchName, char* _saveUrl, char* pt)
    {
        Strcpy(pt, "");
        string search_name = searchName;
        string saveUrl = _saveUrl;
        string ret = ""; //����ֵ
        string flag = Hash4(search_name);
        string tempstr;

        ifstream infile(saveUrl + "database\\author\\" + flag + ".ini", ios::in);
        getline(infile, tempstr);

        if (!infile) {
            Strcpy(pt, "");
            return;
        }

        while (!infile.eof()) {
            if (tempstr.find("$" + search_name + "$") != string::npos)
            {
                size_t startPos = tempstr.find_last_of("$") + 1;//��ȡλ�ú�+1
                size_t endPos = tempstr.size() - 1;//����λ��
                ret += tempstr.substr(startPos, endPos - startPos + 1) + "\n";
            }
            getline(infile, tempstr);
        }
        infile.close();
        if (ret.length() > 0){
            Strcpy(pt, ret);
        }
        return;
    }

    LPCWSTR stringtolstr(string str) {
        LPWSTR ans = new WCHAR[str.length() + 1];
        MultiByteToWideChar(CP_ACP, 0, (LPCSTR)str.c_str(), -1, ans, str.length());
        return ans;
    }


    // ����ļ�·���Ƿ���ȷ
    bool CheckFilePathCorrect_Compatible(char* _saveUrl)
    {
        ifstream infile((string)_saveUrl + "database\\finish.db", ios::in);
        //·�����󣬷��ر���
        if (!infile)
        {
            return false;
        }
        ifstream indblp((string)_saveUrl + "dblp.xml", ios::in);
        if (!indblp)
        {
            return false;
        }
        return true;
    }

    // ����ļ�·���Ƿ���ȷ
    bool CheckFilePathCorrect(char* _saveUrl)
    {
        string saveUrl = _saveUrl;
        HANDLE check_file_handle = CreateFile(stringtolstr(saveUrl + "database\\finish.db"), FILE_READ_EA, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
        if (check_file_handle == INVALID_HANDLE_VALUE) {
            return false;
        }
        HANDLE check_file_handle2 = CreateFile(stringtolstr(saveUrl + "dblp.xml"), FILE_READ_EA, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
        if (check_file_handle2 == INVALID_HANDLE_VALUE)
            return false;
        return true;
        //ifstream infile(saveUrl + "database\\finish.db", ios::in);
        ////·�����󣬷��ر���
        //if (!infile)
        //{
        //    return false;
        //}
        //ifstream indblp(saveUrl + "dblp.xml", ios::in | ios::binary);
        //if (!indblp)
        //{
        //    return false;
        //}
    }

    class set_coauthor {
    private:
        set<string> coauthor_all;
        int total;
    public:
        set_coauthor() {
            coauthor_all.clear();
            total = 0;
        }
        ~set_coauthor(){}
        void put(string tar) {
            string tmp = "";
            string::iterator t = tar.begin();
            while (t != tar.end()) {
                if (*t == '$' && tmp != "") {
                    coauthor_all.insert(tmp);
                    tmp = "";
                    total++;
                }
                else if (*t != '$') {
                    tmp = tmp + *t;
                }
                t++;
            }
        }
        void clear() {
            coauthor_all.clear();
            total = 0;
        }
        string get(string himself = "") {
            string ans = "";
            set<string>::iterator t = coauthor_all.begin();
            while (t != coauthor_all.end()) {
                if (*t != himself)
                    ans += *t + "\n";
                t++;
            }
            return ans;
        }
    };
    

    void CoAuthor(char* searchName, char* _saveUrl, char* pt)
    {
        Strcpy(pt, "");
        string search_name = searchName;
        string saveUrl = _saveUrl;
        string ret = ""; //����ֵ
        string flag = Hash4(search_name);
        string tempstr;

        ifstream infile(saveUrl + "database\\author\\" + flag + ".ini", ios::in);
        getline(infile, tempstr);

        if (!infile) {
            Strcpy(pt, "");
            return;
        }
        set_coauthor tmps;
        while (!infile.eof()) {
            if (tempstr.find("$" + search_name + "$") != string::npos)
            {
                tmps.put(tempstr);
            }
            getline(infile, tempstr);
        }
        infile.close();
        ret = tmps.get(search_name);
        if (ret.length() > 0) {
            Strcpy(pt, ret);
        }
        return;
    }

}
