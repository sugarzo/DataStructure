/*
------------------------------------
数据结构大作业
Code By LYC
202030444483 计算机科学与技术2班
------------------------------------
*/
#define Author_Sorting_EXPORTS
#include "Author_Sorting.h"

namespace Author_Sorting {
	vector<authors> alls;
	vector<authors> bucket[100001];

	class node
	{
	public:
		static const node* Not_found;
		char str;
		DWORD weight;
		vector<node*> child;
		node(char Strings = '\0', DWORD weights = 0) {
			str = Strings;
			weight = weights;
			child.clear();
		}
		node* add_child(char String, DWORD weights = 0) {
			node* ans = new node(String, weights);
			child.push_back(ans);
			return ans;
		}
		node* find_child(char String) {
			for (vector<node*>::iterator now = child.begin(); now != child.end(); now++) {
				if ((*now)->str == String)
					return *now;
			}
			return NULL;
		}
	};
	const node* node::Not_found = NULL;

	class authors
	{
	private:
	public:
		string name;
		DWORD weight;
		authors(const string& names, const DWORD& weights) {
			name = names;
			weight = weights;
		}
		DWORD operator++() {
			return ++weight;
		}
		bool operator < (const authors& tar) {
			return this->weight < tar.weight;
		}
		bool operator > (const authors& tar) {
			return this->weight > tar.weight;
		}
		bool operator >= (const authors& tar) {
			return this->weight >= tar.weight;
		}
		bool operator <= (const authors& tar) {
			return this->weight <= tar.weight;
		}
		authors& operator =(const authors& tar) {
			name = tar.name;
			weight = tar.weight;
			return *this;
		}
	};

	template<typename T> inline void quick_sort(vector<T>& a, DWORD s, DWORD t) {
		if (s >= t)return;
		DWORD l = s, r = t;
		while (l < r) {
			while (l < r && a[r] >= a[s])r--;
			while (l < r && a[l] <= a[s])l++;
			if (l != r)swap(a[r], a[l]);
		}
		if (s != r)swap(a[s], a[r]);
		if (r - 1 > s && r >= 1)
			sort(a, s, r - 1);
		if (r + 1 < t)
			sort(a, r + 1, t);
		return;
	}

	template<typename T> inline void  bubble_sort(vector<T>& a, DWORD s, DWORD t) {
		if (s >= t)return;
		bool tflag = true;
		DWORD round = 0;
		while (tflag) {
			tflag = false;
			for (int i = s + round; i < t - round; i++) {
				if (a[i] > a[i + 1]) {
					swap(a[i], a[i + 1]);
					tflag = true;
				}
			}
			for (int i = t - round; i > s + round; i--) {
				if (a[i] < a[i - 1]) {
					swap(a[i], a[i - 1]);
					tflag = true;
				}
			}
			round++;
		}
		return;
	}

	LPCWSTR stringtolstr(string str) {
		LPWSTR ans = new WCHAR[str.length() + 1];
		MultiByteToWideChar(CP_ACP, 0, (LPCSTR)str.c_str(), -1, ans, str.length());
		return ans;
	}

	inline void sort(vector<authors>& a, DWORD s, DWORD t) {
		for (int i = 0; i < 100000; i++) {
			bucket[i].clear();
		}
		for (int i = 0; i < a.size(); i++) {
			if (a[i].weight < 100000)
				bucket[a[i].weight].push_back(a[i]);
			else
				bucket[100000].push_back(a[i]);
		}
		a.clear();
		for (int i = 100000; i >= 0; i--) {
			for (int j = 0; j < bucket[i].size(); j++) {
				authors tmp(bucket[i][j]);
				a.push_back(tmp);
			}
		}
		return;
	}

	bool Author_Sort(char* _FileUrl)
	{
		HANDLE check_author_database_handle = CreateFile(stringtolstr((string)_FileUrl + "database\\author_rank"), FILE_READ_EA, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
		if (check_author_database_handle == INVALID_HANDLE_VALUE) {//因兼容已废弃
			cerr << "未检测到author_rank文件" << endl;
			//return false;
		}
		node* root = NULL;//字典树树根
		fstream source_file;
		source_file.open((string)_FileUrl + "database\\author_rank", ios::in);
		if (!source_file) {
			return false;
		}
		Build_Tree(source_file, root);
		Found_Tree(root, alls);
		sort(alls, 0, alls.size() - 1);
		fstream answer_file((string)_FileUrl + "database\\author_rank_result", ios::out);
		for (int i = 0; i < alls.size(); i++) {
			answer_file << alls[i].weight << " " << alls[i].name << ends;
		}
		answer_file.close();
		source_file.close();
		return true;
	}

	inline void Build_Tree(fstream& iner, node*& root)
	{
		root = new node;
		DWORD weight = 0;
		string name;
		while (iner >> weight) {
			getline(iner, name);
			while (name[0] == ' ')name.erase(0, 1);
			Insert_Tree(root, name, weight);
		}
		return;
	}

	void Insert_Tree(node* root, string tar, DWORD weight)
	{
		node* now = root;
		for (int s = 0; s < tar.length() - 1; s++) {
			char tmpstr = tar[s];
			if (now->find_child(tmpstr) == node::Not_found) {
				now->add_child(tmpstr);
			}
			now = now->find_child(tmpstr);
		}
		char tmpstr = tar[tar.size() - 1];
		if (now->find_child(tmpstr) == node::Not_found) {
			now->add_child(tmpstr, weight);
		}
		else now->find_child(tmpstr)->weight += weight;
		return;
	}

	void Found_Tree(node*& root, vector<authors>& allauthor, string TH)
	{
		if (root == NULL)return;
		if (root->weight != 0) {
			authors tmp(TH + root->str, root->weight);
			allauthor.push_back(tmp);
		}
		for (int i = 0; i < root->child.size(); i++) {
			if (root->str != '\0')
				Found_Tree(root->child[i], allauthor, TH + root->str);
			else
				Found_Tree(root->child[i], allauthor, TH);
		}
		return;
	}
}

namespace Keyword_Sorting {
	inline void Build_Trees(fstream& iner, Author_Sorting::node*& root)
	{
		root = new Author_Sorting::node;
		string name;
		while (iner >> name) {
			Author_Sorting::Insert_Tree(root, name, 1);
		}
		return;
	}

	void Keyword_Sort(DWORD year, char* _FileUrl)
	{
		string paths = "mkdir " + (string)_FileUrl + "database\\year_sorted 2>nul";
		char* pathss = new char[paths.length() + 1];
		for (int i = 0; i < paths.length(); i++) {
			pathss[i] = paths[i];
		}
		pathss[paths.length()] = '\0';
		system(pathss);
		Author_Sorting::node* root;
		fstream iner((string)_FileUrl + "database\\year\\" + to_string(year) + ".ini", ios::in);
		Build_Trees(iner, root);
		vector<Author_Sorting::authors> alls;
		Author_Sorting::Found_Tree(root, alls);
		Author_Sorting::sort(alls, 0, alls.size() - 1);
		fstream outer((string)_FileUrl + "database\\year_sorted\\" + to_string(year) + ".db", ios::out);
		for (int i = 0; i < alls.size(); i++) {
			outer << alls[i].weight << " " << alls[i].name << ends;
		}
		iner.close();
		outer.close();
		return;
	}

}
