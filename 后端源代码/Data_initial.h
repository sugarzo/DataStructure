/*
------------------------------------
���ݽṹ����ҵ
Code By LYC
202030444483 �������ѧ�뼼��2��
------------------------------------
*/
#pragma once
#ifdef Data_initial_EXPORTS
#define EXP extern "C" __declspec(dllexport)
#else
#define EXP extern "C" __declspec(dllimport)
#endif
#include "Universal_headers.h"

namespace data_initial {
	using namespace std;
	EXP inline DWORD Hash4(const string&);//12λ�ַ�����ϣ
	EXP bool initial_readers(DWORD, DWORD, bool, char*);//�̲߳�������
	void reader(fstream&, DWORD, DWORD, DWORD);//���ݴ�������߳�
	inline DWORD Write_Record(fstream&, DWORD, string&, DWORD);
	inline void Create_article(string, DWORD, vector<string>&, DWORD, DWORD);
	inline void do_writer();//�ļ��������
	void writer();//���ݴ�������߳�
	inline void push_wz(string, DWORD);
	inline void push_year(string, DWORD);
	inline void push_author(string, DWORD);
	inline bool gotchar(fstream&, DWORD, char&);
	inline LPCWSTR stringtolstr(string);
	template<typename T> inline void sort(vector<T>&, int, int);
	class out_string; //������ݻ���
}
