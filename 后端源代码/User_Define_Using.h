/*
------------------------------------
数据结构大作业
Code By LYC
202030444483 计算机科学与技术2班
------------------------------------
*/
#pragma once
#include "Data_initial.h"
#include "Author_Sorting.h"
#include "fuzzy_search.h"
#include "Elements_Searcher.h"

using data_initial::Hash4;//通用字符串Hash函数          DWORD Hash4(const string& 需要hash的字符)
using data_initial::initial_readers;//数据库初始化建库函数  bool initial_readers(const DWORD 总线程数(建议值:16), DWORD 最多同时运行线程数(建议值:4), bool 是否检查文件dblp(建议值:False), char* 数据库地址)
using Keyword_Sorting::Keyword_Sort;//年份热搜排序    void Keyword_Sort(DWORD 年份, char* 数据库地址)
using Author_Sorting::Author_Sort;//作者发文量排序    bool Author_Sort(char* 数据库地址)
using Elements_Searcher::articleInfo;//输入文章名，返回文章的所有相关信息    void articleInfo(char* 文章名, char* 数据库地址, char* 结果)
using Elements_Searcher::CheckFilePathCorrect;//检查文件路径是否正确      bool CheckFilePathCorrect(char* 数据库地址)
using Elements_Searcher::CheckFilePathCorrect_Compatible;//检查文件路径是否正确（兼容模式） bool CheckFilePathCorrect_Compatible(char* 数据库地址)
using Elements_Searcher::main_Reader;//输入作者名字，返回作者发布的所有文章   void main_Reader(char* 作者名字, char* 数据库地址, char* 结果)
using Elements_Searcher::CoAuthor;//输入作者名字，返回所有合作作者   void CoAuthor(char* 作者名字, char* 数据库地址, char* 结果)
using Fuzzy_Searcher::initial_fuzzy;//初始化模糊搜索    void initial_fuzzy(char* 数据库地址)
using Fuzzy_Searcher::fuzzy_search;//模糊搜索   bool fuzzy_search(char* 数据库地址, DWORD 线程数, char* 目标内容)
using Fuzzy_Searcher::Full_word_match_fuzzy_search;//全字匹配模糊搜索   bool Full_word_match_fuzzy_search(char* 数据库地址, DWORD 线程数, char* 目标内容)
using Fuzzy_Searcher::release_fuzzy_searcher;//释放模糊搜索的资源(需要重新初始化)    void release_fuzzy_searcher()
using Fuzzy_Searcher::Format_Hash4;//DWORD Format_Hash4(char* hash字符)