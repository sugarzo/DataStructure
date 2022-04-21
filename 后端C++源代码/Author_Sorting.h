#pragma once
#ifdef Author_Sorting_EXPORTS
#define EXP extern "C" __declspec(dllexport)
#else
#define EXP extern "C" __declspec(dllimport)
#endif
#include "Universal_headers.h"

namespace Author_Sorting {
	using namespace std;
	class node;
	class authors;
	EXP bool Author_Sort(void);
	inline void Build_Tree(fstream&, node*&);
	void Insert_Tree(node*, string, DWORD);
	void Found_Tree(node*&, vector<authors>&, string = "");
	inline void sort(vector<authors>&, DWORD, DWORD);
	template<typename T> inline void quick_sort(vector<T>&, DWORD, DWORD);
	template<typename T> inline void bubble_sort(vector<T>&, DWORD, DWORD);
}

namespace Keyword_Sorting {
	using namespace std;
	EXP void Keyword_Sort(DWORD year);
	inline void Build_Trees(fstream&, Author_Sorting::node*&);
}