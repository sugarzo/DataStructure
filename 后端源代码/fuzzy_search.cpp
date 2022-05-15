/*
------------------------------------
数据结构大作业
Code By LYC
202030444483 计算机科学与技术2班
------------------------------------
*/
#define Fuzzy_Searcher_EXPORTS
#include "fuzzy_search.h"

namespace Fuzzy_Searcher {
	bool change_fuzzy_string[257];
	string fuzzy_target = "";
	bool kill_me = false;
	vector<string> bags;
	bool finished_flag[257];
	vector<string> answers[257];
	int max_thread_num;
	bool is_initial = false;

	void calculate_thread(char* _saveUrl, DWORD lowbound, DWORD upbound, DWORD ID)
	{
		if (lowbound >= upbound)return;
		while (!kill_me) {
			if (fuzzy_target == "") {
				Sleep(100);
				continue;
			}
			if (!change_fuzzy_string[ID]) {
				Sleep(100);
				continue;
			}
			change_fuzzy_string[ID] = false;
			finished_flag[ID] = false;
			string ans = "";
			int* kmp = new int[fuzzy_target.length() + 10];
			kmp[0] = 0;
			int k1 = 0, j1 = 1;
			while (j1 < fuzzy_target.length()) {
				if (fuzzy_target[k1] == fuzzy_target[j1]) {
					k1++;
					kmp[j1] = k1;
					j1++;
				}
				else if (k1 != 0) {
					k1 = kmp[k1];
				}
				else {
					kmp[j1] = 0;
					j1++;
				}
			}
			for (int i = lowbound; i < upbound; i++) {
				k1 = 0, j1 = 0;
				while (j1 < bags[i].length()) {
					if (k1 >= fuzzy_target.length())break;
					else if (fuzzy_target[k1] == bags[i][j1]) {
						k1++; j1++;
					}
					else if (k1 != 0) {
						k1 = kmp[k1];
					}
					else {
						j1++;
					}
				}
				if (k1 >= fuzzy_target.length()) {
					answers[ID].push_back(bags[i]);
				}
			}
			finished_flag[ID] = true;
		}
		return;
	}

	inline string Hash4(const string& tar) {
		DWORD64 ans = 0;
		for (int i = 0; i < tar.length(); i++) {
			ans = ((ans >> 8) & 0xf) ^ ((ans << 4) ^ tar[i]);
		}
		ans = ans & 0xfff;
		return to_string(ans);
	}

	inline DWORD Format_Hash4(char* tars) {
		string tar = tars;
		DWORD64 ans = 0;
		while (tar[0] == ' ') {
			tar.erase(0, 1);
		}
		while (tar[tar.size() - 1] == ' ') {
			tar.erase(tar.size() - 1, 1);
		}
		tar = " " + tar + " ";
		for (int i = 0; i < tar.length(); i++) {
			ans = ((ans >> 8) & 0xf) ^ ((ans << 4) ^ tar[i]);
		}
		ans = ans & 0xfff;
		return ans;
	}

	int Full_word_match_fuzzy_search(char* _saveUrl, DWORD thread_num, char* tar)
	{
		string tmp = tar;
		while (tmp[0] == ' ') {
			tmp.erase(0, 1);
		}		
		while (tmp[tmp.size() - 1] == ' ') {
			tmp.erase(tmp.size() - 1, 1);
		}
		tmp = " " + tmp + " ";
		char* tmps = new char[tmp.size() + 1];
		for (int i = 0; i < tmp.size(); i++) {
			tmps[i] = tmp[i];
		}
		tmps[tmp.size()] = '\0';
		return fuzzy_search(_saveUrl, thread_num, tmps);
	}

	void initial_fuzzy(char* _saveUrl)
	{
		bags.clear();
		for (int i = 1973; i <= 2022; i++) {
			fstream infile((string)_saveUrl + "database\\year\\" + to_string(i) + ".ini", ios::in);
			if (!infile) {
				continue;
			}
			string instr;
			getline(infile, instr);
			while (!infile.eof()) {
				bags.push_back(instr);
				getline(infile, instr);
			}
		}
		is_initial = true;
		return;
	}

	bool check_finish() {
		bool ans = true;
		for (int i = 0; i < max_thread_num; i++) {
			ans = ans && finished_flag[i];
		}
		return (ans || kill_me);
	}

	int fuzzy_search(char* _saveUrl, DWORD thread_num, char* tar)
	{
		if (!is_initial) {
			cout << "未初始化" << endl;
			return false;
		}
		if (thread_num > 256)thread_num = 256;
		max_thread_num = thread_num;
		kill_me = false;
		DWORD numb = bags.size() / thread_num;
		fuzzy_target = tar;
		for (int i = 0; i < max_thread_num; i++) {
			answers[i].clear();
			finished_flag[i] = false;
			change_fuzzy_string[i] = true;
		}
		for (int i = 0; i < thread_num - 1; i++) {
			thread* tmp = new thread(calculate_thread, _saveUrl, numb * i, numb * (i + 1), i);
			tmp->detach();
		}
		thread* tmp = new thread(calculate_thread, _saveUrl, numb * (thread_num - 1), bags.size() - 1, thread_num - 1);
		tmp->detach();
		while (!check_finish()) { Sleep(100); }
		if (kill_me)return false;
		kill_me = true;
		string paths = "mkdir " + (string)_saveUrl + "database\\searchlog  2>nul";
		char* pathss = new char[paths.length() + 1];
		for (int i = 0; i < paths.length(); i++) {
			pathss[i] = paths[i];
		}
		pathss[paths.length()] = '\0';
		system(pathss);
		fstream outfile((string)_saveUrl + (string)"database\\searchlog\\" + Hash4(fuzzy_target) + ".db", ios::out);
		int tot = 0;
		for (int i = 0; i < thread_num; i++) {
			for (int j = 0; j < answers[i].size(); j++) {
				outfile << answers[i][j] << endl;
				tot++;
			}
			answers[i].clear();
		}
		outfile.close();
		return tot;
	}

	void release_fuzzy_searcher()
	{
		kill_me = true;
		bags.clear();
		for (int i = 0; i < 256; i++)answers[i].clear();
		is_initial = false;
		return;
	}
}
