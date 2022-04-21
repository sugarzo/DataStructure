#pragma once
#ifdef Elements_Searcher_EXPORTS
#define EXP extern "C" __declspec(dllexport)
#else
#define EXP extern "C" __declspec(dllimport)
#endif
#include "Universal_headers.h"
#include "GPUCalculate_headers.h"

namespace Elements_Searcher {
	using namespace std;
	using namespace concurrency;
	using namespace Concurrency::precise_math;
    inline string Hash4(const string&);
	EXP inline bool find_article(const string&);
	EXP inline bool find_author_article(const string&);
	EXP inline bool read_data(const DWORD);

}