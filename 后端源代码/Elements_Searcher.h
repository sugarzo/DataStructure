/*
------------------------------------
数据结构大作业
Code By LYC
202030444483 计算机科学与技术2班
------------------------------------
*/
#pragma once
#ifdef Elements_Searcher_EXPORTS
#define EXP extern "C" __declspec(dllexport)
#else
#define EXP extern "C" __declspec(dllimport)
#endif
#include "Universal_headers.h"

namespace Elements_Searcher {
	using namespace std;
    inline string Hash4(const string&);
	inline void Strcpy(char*, const string&);
	inline size_t stringToLonglong(string);
	inline void Strcpy(char*, const char*);
	inline LPCWSTR stringtolstr(string);
	inline bool check_TailElem(string);
	class set_coauthor;
	EXP void articleInfo(char*, char*, char*);
	EXP void main_Reader(char*, char*, char*);
	EXP bool CheckFilePathCorrect_Compatible(char*);
	EXP bool CheckFilePathCorrect(char*);
	EXP void CoAuthor(char*, char*, char*);
}
