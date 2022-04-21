#define Data_initial_EXPORTS
#include "Data_initial.h"

namespace data_initial {
    const int MAX_NUM = 4096;//HASH最大值(请勿修改，由Hash4函数获得)
    size_t total_num = 0;
    int alive_thread_initial = 0;
    bool sys_writing = false;

    bool outtmpwzfl[MAX_NUM] = { false };
    bool outtmpzzfl[MAX_NUM] = { false };
    bool outtmpyearfl[3000] = { false };
    out_string* outtmpwz[MAX_NUM] = { 0 };
    out_string* outtmpzz[MAX_NUM] = { 0 };
    out_string* outtmpyear[3000] = { 0 };
    vector<string> auth[16];
    string quike_reader[16];
    DWORD quike_reader_p[16] = { 0 };

    class out_string { //输出内容缓存
    public:
        string out;
        out_string* next;
        out_string() {
            out = "";
            next = NULL;
        }
    };

    template<typename T> inline void sort(vector<T>& a, int s, int t) {
        if (s >= t)return;
        int l = s, r = t;
        T tmp = a[s];
        while (l < r) {
            while (l < r && a[r] >= tmp)r--;
            while (l < r && a[l] <= tmp)l++;
            if (l != r)swap(a[r], a[l]);
        }
        swap(a[s], a[r]);
        sort(a, s, l - 1);
        sort(a, l + 1, t);
        return;
    }

    inline void push_wz(string content, DWORD num) {
        while (sys_writing) { Sleep(1000); }
        while (outtmpwzfl[num]) {}
        outtmpwzfl[num] = true;
        out_string* tmp = outtmpwz[num];
        outtmpwz[num] = new out_string;
        outtmpwz[num]->out = content;
        outtmpwz[num]->next = tmp;
        outtmpwzfl[num] = false;
        return;
    }

    inline void push_year(string content, DWORD num) {
        while (sys_writing) { Sleep(1000); }
        while (outtmpyearfl[num]) {}
        outtmpyearfl[num] = true;
        out_string* tmp = outtmpyear[num];
        outtmpyear[num] = new out_string;
        outtmpyear[num]->out = content;
        outtmpyear[num]->next = tmp;
        outtmpyearfl[num] = false;
        return;
    }

    inline void push_author(string content, vector<string>& author) {
        while (sys_writing) { Sleep(1000); }
        string atmp = "";
        for (int i = 0; i < author.size(); i++) {
            atmp = atmp + "$" + author[i];
        }
        for (int i = 0; i < author.size(); i++) {
            while (outtmpzzfl[Hash4(author[i])]) {}
            outtmpzzfl[Hash4(author[i])] = true;
            out_string* tmp = outtmpzz[Hash4(author[i])];
            outtmpzz[Hash4(author[i])] = new out_string;
            outtmpzz[Hash4(author[i])]->out = atmp + "$" + content;
            outtmpzz[Hash4(author[i])]->next = tmp;
            outtmpzzfl[Hash4(author[i])] = false;
        }
        return;
    }

    inline DWORD Hash4(const string& tar) {
        DWORD64 ans = 0;
        for (int i = 0; i < tar.length(); i++) {
            ans = ((ans >> 8) & 0xf) ^ ((ans << 4) ^ tar[i]);
        }
        return ans & 0xfff;
    }

    inline bool gotchar(fstream& iner, DWORD ID, char& outs) {
        if (quike_reader[ID] == "" || quike_reader_p[ID] >= quike_reader[ID].size()) {
            if (getline(iner, quike_reader[ID]))
                quike_reader_p[ID] = 0;
            else return false;
        }
        outs = quike_reader[ID][quike_reader_p[ID]];
        quike_reader_p[ID]++;
        return true;
    }

    inline void Create_article(string name, DWORD flag, vector<string>& author, DWORD years, DWORD ID) {
        if (author.size() > 256) {//疑惑问题处理???
            DWORD nh = Hash4(name);
            push_wz(name + " <-> " + to_string(flag), nh);
            push_year(name, years);
            total_num++;
            return;
        }
        for (int i = 0; i < author.size(); i++)auth[ID].push_back(author[i]);
        DWORD nh = Hash4(name);
        push_wz(name + " <-> " + to_string(flag), nh);
        push_year(name, years);
        push_author(name, author);
        author.clear();
        total_num++;
        return;
    }

    inline DWORD Write_Record(fstream& iner, DWORD flag, string& tar, DWORD ID) {
        if (tar.find("<article") == string::npos && tar.find("<inproceedings") == string::npos && tar.find("<proceedings") == string::npos &&
            tar.find("<book") == string::npos && tar.find("<incollection") == string::npos && tar.find("<phdthesis") == string::npos &&
            tar.find("<mastersthesis") == string::npos && tar.find("<www") == string::npos && tar.find("<person") == string::npos &&
            tar.find("<data") == string::npos)//寻找起始地址
            return 0;
        char tmpc;
        string tmp;
        int kd = 0;
        vector<string> author;
        string name = "";
        author.clear();
        DWORD err_protection = 0;//非法数据格式兼容
        while (gotchar(iner, ID, tmpc)) {
            if (tmpc == '<' && kd == 0) {
                tmp = "<";
                kd = 1;
                err_protection = (DWORD)(iner.tellg()) + quike_reader_p[ID] - quike_reader[ID].length();
                continue;
            }
            else if (tmpc == '<' && kd > 0) {
                tmp = tmp + '<';
                kd++;
                continue;
            }
            else if (tmpc == '>' && kd > 0) {
                kd--;
                tmp = tmp + '>';
                if (kd <= 0) {
                    kd = 0;
                    if (tmp == "<author>" || tmp == "<editor>") {
                        tmp = "";
                        while (gotchar(iner, ID, tmpc)) {
                            if (tmpc != '<')tmp = tmp + tmpc;
                            else {
                                author.push_back(tmp);
                                break;
                            }
                        }
                    }
                    else if (tmp == "<title>" || tmp == "<booktitle>") {
                        tmp = "";
                        while (gotchar(iner, ID, tmpc)) {
                            if (tmpc != '<')tmp = tmp + tmpc;
                            else {
                                name = tmp;
                                break;
                            }
                        }
                    }
                    else if (tmp == "<year>") {//完成记录需要数据的处理
                        int years = 0;
                        while (gotchar(iner, ID, tmpc)) {
                            if (tmpc != ' ')break;
                        }
                        while (tmpc >= '0' && tmpc <= '9') {
                            years = years * 10 + tmpc - '0';
                            gotchar(iner, ID, tmpc);
                        }
                        Create_article(name, flag, author, years, ID);
                        break;
                    }
                }
            }
            else if (kd > 0) {
                tmp = tmp + tmpc;
                if (tar.find("<article") != string::npos && tar.find("<inproceedings") != string::npos && tar.find("<proceedings") != string::npos &&
                    tar.find("<book") != string::npos && tar.find("<incollection") != string::npos && tar.find("<phdthesis") != string::npos &&
                    tar.find("<mastersthesis") != string::npos && tar.find("<www") != string::npos && tar.find("<person") != string::npos &&
                    tar.find("<data") != string::npos)//未知错误兼容
                {
                    Create_article(name, flag, author, 1, ID);//错误数据强制储存
                    author.clear();
                    return err_protection;
                    //Write_Record(iner, err_protection, tmp, ID);//再处理
                    break;
                }
            }
        }
        return 0;
    }

    void reader(fstream& iner, DWORD offset, DWORD Terminal, DWORD ID) {
        iner.seekg(offset);
        char tmpc;
        string tmp;
        int kd = 0;
        DWORD flag = 0;//当前偏移量
        DWORD position = (DWORD)(iner.tellg()) + quike_reader_p[ID] - quike_reader[ID].length();
        while (position <= Terminal + 1 && gotchar(iner, ID, tmpc)) {
            if (tmpc == '<' && kd == 0) {
                tmp = "<";
                kd = 1;
                flag = (DWORD)(iner.tellg()) + quike_reader_p[ID] - quike_reader[ID].length();
                continue;
            }
            else if (tmpc == '<' && kd > 0) {
                tmp = tmp + '<';
                kd++;
                continue;
            }
            else if (tmpc == '>' && kd > 0) {
                kd--;
                tmp = tmp + '>';
                if (kd <= 0) {
                    kd = 0;
                    while (flag) {
                        flag = Write_Record(iner, flag, tmp, ID);//异常数据兼容
                    }
                }
            }
            else if (kd > 0) {
                tmp = tmp + tmpc;
            }
            position = (DWORD)(iner.tellg()) + quike_reader_p[ID] - quike_reader[ID].length();
        }
        while (flag) {
            flag = Write_Record(iner, flag, tmp, ID);//异常数据兼容
        }
        alive_thread_initial--;
        
    }

    inline void do_writer() {
        for (int i = 0; i < MAX_NUM; i++) {
            if (outtmpwz[i] == NULL)continue;
            fstream ot("database\\article\\" + to_string(i) + ".ini", ios::app);
            out_string* now = outtmpwz[i];
            outtmpwz[i] = NULL;
            while (now != NULL) {
                ot << (now->out) << endl;
                out_string* tmp = now;
                now = now->next;
                delete tmp;
            }
            ot.close();
        }
        for (int i = 0; i < MAX_NUM; i++) {
            if (outtmpzz[i] == NULL)continue;
            fstream ot("database\\author\\" + to_string(i) + ".ini", ios::app);
            out_string* now = outtmpzz[i];
            outtmpzz[i] = NULL;
            while (now != NULL) {
                ot << (now->out) << endl;
                out_string* tmp = now;
                now = now->next;
                delete tmp;
            }
            ot.close();
        }
        for (int i = 0; i < 3000; i++) {
            if (outtmpyear[i] == NULL)continue;
            fstream ot("database\\year\\" + to_string(i) + ".ini", ios::app);
            out_string* now = outtmpyear[i];
            outtmpyear[i] = NULL;
            while (now != NULL) {
                ot << (now->out) << endl;
                out_string* tmp = now;
                now = now->next;
                delete tmp;
            }
            ot.close();
        }

        for (int i = 1; i < 16; i++) {
            for (int j = 0; j < auth[i].size(); j++) {
                auth[0].push_back(auth[i][j]);
            }
            auth[i].clear();
        }
        fstream allauthor("database\\author_rank", ios::app);
        sort(auth[0], 0, auth[0].size() - 1);
        string tmpau = auth[0][0];
        int tot = 0;
        for (int i = 0; i < auth[0].size(); i++) {
            if (auth[0][i] != tmpau) {
                allauthor << tot << " " << tmpau << endl;
                tmpau = auth[0][i];
                tot = 1;
            }
            else {
                tot++;
            }
        }
        allauthor.close();
        auth[0].clear();
    }

    bool writing_finished = false;

    void writer() {
        int mytmp = 0;
        while (!writing_finished) {
            if (total_num - mytmp >= 1000000) {
                sys_writing = true;
                mytmp = total_num;
                do_writer();
                sys_writing = false;
            }
            Sleep(500);
        }
        sys_writing = true;
        mytmp = total_num;
        do_writer();
        sys_writing = false;
        return;
    }

    bool initial_readers(const DWORD Max_thread, DWORD TOTAL_THREAD, bool ShowText) {
        HANDLE check_file_handle = CreateFile(L"database\\finish.db", FILE_READ_EA, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
        if (check_file_handle != INVALID_HANDLE_VALUE) {
            if (ShowText)cout << "已存在完成的数据库构建";
            return true;
        }
        if (Max_thread > 16)return false;
        alive_thread_initial = 0;
        system("mkdir database\\author 2>nul");
        system("mkdir database\\article 2>nul");
        system("mkdir database\\year 2>nul");
        thread* readers[16];
        fstream* infile = new fstream[Max_thread];
        for (int i = 0; i < Max_thread; i++) {
            infile[i].open("dblp.xml", ios::in);
        }
        DWORD64 filesize = 0;
        HANDLE target_file_handle = CreateFile(L"dblp.xml", FILE_READ_EA, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
        filesize = GetFileSize(target_file_handle, NULL);
        DWORD64 offset = filesize / Max_thread;
        thread writers(writer);
        infile[Max_thread - 1].seekg(0, ios::beg);
        for (int i = 0; i < Max_thread - 1; i++) {
            DWORD startpoi = infile[Max_thread - 1].tellg();
            infile[Max_thread - 1].seekg((i + 1) * offset);
            char tmpchar;
            while (tmpchar = infile[Max_thread - 1].get()) {
                if (tmpchar == '>') {
                    DWORD tmpe = infile[Max_thread - 1].tellg();
                    alive_thread_initial++;
                    (readers[i]) = new thread(reader, ref(infile[i]), startpoi, tmpe, i);
                    while (alive_thread_initial >= TOTAL_THREAD) {
                        if (!sys_writing)cout << "读取中:" << total_num << endl;
                        else cout << "写入中:" << total_num << endl;
                        Sleep(1000);
                    }
                    break;
                }
            }
        }
        alive_thread_initial++;
        (readers[Max_thread - 1]) = new thread(reader, ref(infile[Max_thread - 1]), (DWORD)infile[Max_thread - 1].tellg(), filesize - 1, Max_thread - 1);
        while (alive_thread_initial) {
            if (!sys_writing)cout << "读取中:" << total_num << endl;
            else cout<<"写入中:" << total_num << endl;
            Sleep(1000);
        }
        cout << "已完成读取:" << total_num << endl;
        writing_finished = true;
        writers.join();
        cout << "已完成数据写入" << endl;
        fstream finishes("database\\finish.db", ios::out);
        finishes << total_num;
        finishes.close();
        return true;
    }
}