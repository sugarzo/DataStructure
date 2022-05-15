/*
------------------------------------
数据结构大作业
Code By LYC
202030444483 计算机科学与技术2班
------------------------------------
*/
#pragma once
#ifdef Fuzzy_Searcher_EXPORTS
#define EXP extern "C" __declspec(dllexport)
#else
#define EXP extern "C" __declspec(dllimport)
#endif
#include "Universal_headers.h"

namespace Fuzzy_Searcher {
	using namespace std;
	void calculate_thread(char*, DWORD, DWORD, DWORD);
	inline string Hash4(const string& tar);
	EXP int Full_word_match_fuzzy_search(char*, DWORD, char*);
	EXP int fuzzy_search(char*, DWORD, char*);
	EXP void initial_fuzzy(char*);
	EXP void release_fuzzy_searcher();
	EXP inline DWORD Format_Hash4(char*);
}
