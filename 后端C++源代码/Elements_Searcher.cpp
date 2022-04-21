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

    EXP bool find_article(const string& name)
    {
        system("mkdir database\\data_search 2>nul");
        time_t nows = time(0);
        string poi = Hash4(name);
        fstream outer("database\\data_search\\" + poi + ".tmp", ios::app);
        fstream files("database\\article\\" + poi + ".ini", ios::in);
        string tmp;
        while (getline(files, tmp)) {
            if (tmp.find(name) != string::npos) {
                size_t pois = tmp.find("<->");
                pois += 3;
                /*
                for (int i = poi.length(); i <= tmp.size(); i++) {

                }*/
            }
        }
        return  DWORD();
    }

    EXP bool find_author_article(const string&)
    {
        return DWORD();
    }

    EXP bool read_data(const DWORD)
    {
        return true;
    }

}