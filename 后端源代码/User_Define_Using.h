/*
------------------------------------
���ݽṹ����ҵ
Code By LYC
202030444483 �������ѧ�뼼��2��
------------------------------------
*/
#pragma once
#include "Data_initial.h"
#include "Author_Sorting.h"
#include "fuzzy_search.h"
#include "Elements_Searcher.h"

using data_initial::Hash4;//ͨ���ַ���Hash����          DWORD Hash4(const string& ��Ҫhash���ַ�)
using data_initial::initial_readers;//���ݿ��ʼ�����⺯��  bool initial_readers(const DWORD ���߳���(����ֵ:16), DWORD ���ͬʱ�����߳���(����ֵ:4), bool �Ƿ����ļ�dblp(����ֵ:False), char* ���ݿ��ַ)
using Keyword_Sorting::Keyword_Sort;//�����������    void Keyword_Sort(DWORD ���, char* ���ݿ��ַ)
using Author_Sorting::Author_Sort;//���߷���������    bool Author_Sort(char* ���ݿ��ַ)
using Elements_Searcher::articleInfo;//�������������������µ����������Ϣ    void articleInfo(char* ������, char* ���ݿ��ַ, char* ���)
using Elements_Searcher::CheckFilePathCorrect;//����ļ�·���Ƿ���ȷ      bool CheckFilePathCorrect(char* ���ݿ��ַ)
using Elements_Searcher::CheckFilePathCorrect_Compatible;//����ļ�·���Ƿ���ȷ������ģʽ�� bool CheckFilePathCorrect_Compatible(char* ���ݿ��ַ)
using Elements_Searcher::main_Reader;//�����������֣��������߷�������������   void main_Reader(char* ��������, char* ���ݿ��ַ, char* ���)
using Elements_Searcher::CoAuthor;//�����������֣��������к�������   void CoAuthor(char* ��������, char* ���ݿ��ַ, char* ���)
using Fuzzy_Searcher::initial_fuzzy;//��ʼ��ģ������    void initial_fuzzy(char* ���ݿ��ַ)
using Fuzzy_Searcher::fuzzy_search;//ģ������   bool fuzzy_search(char* ���ݿ��ַ, DWORD �߳���, char* Ŀ������)
using Fuzzy_Searcher::Full_word_match_fuzzy_search;//ȫ��ƥ��ģ������   bool Full_word_match_fuzzy_search(char* ���ݿ��ַ, DWORD �߳���, char* Ŀ������)
using Fuzzy_Searcher::release_fuzzy_searcher;//�ͷ�ģ����������Դ(��Ҫ���³�ʼ��)    void release_fuzzy_searcher()
using Fuzzy_Searcher::Format_Hash4;//DWORD Format_Hash4(char* hash�ַ�)