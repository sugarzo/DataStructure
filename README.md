# DataStructure

**
大二数据结构课设-科学文献管理系统dblp.xml解析

程序中主要运用的技术：C++/C#、文件哈希管理、哈希表与字典、字典树、Dll动态链接库编写与调用、程序线程调度和管理、C++/C#间的数据交换、并查集、状态机设计模式。

前端：unity c#
后端：c++

使用了C++语言作为后端语言，使用了Unity开发了程序前端，C#语言负责与后端数据交互。其中涉及到了不同语言间的函数调用和数据交换。我们采用了先将后台C++代码打包成Dll动态链接库，再在前端C#语言中调用的方式。用这种方式实现了前后端的数据分离。

需要解析的DBLB文件大小为3G-4G的。因此每次启动程序前都读取一遍DBLP并全部写入内存的方法是不现实的，这需要我们提前在磁盘中建立对应的数据库文件，在第一次打开程序时进行建立数据库的操作，将需要的数据写入磁盘。之后对DBLP的数据查询只需要打开已经建立好的数据库进行IO操作即可。

由于数据在磁盘和内存中的读取速度差距极大，在磁盘中删改添加数据效率是很低的，因此如何建立对应数据的索引，以及在选择一个在磁盘文件中容易维护的数据结构是非常重要的。在保证查找效率时，还需要做到方便在磁盘中维护。经过讨论，我们选择了文件哈希+线性遍历的方法，将需要的数据通过一个哈希函数映射到一个文件中，之后查询时只需要根据搜索内容的哈希值打开对应的数据文件进行查询即可。

![image](https://user-images.githubusercontent.com/74815734/164518525-4e50de9d-a36a-4725-b440-89de256c2285.png)

基于这种思想，在解析DBLP完成后，在磁盘中保留了[作者名->发表文章和合作作者]、[文章标题->DBLP文件中偏移量]、[作者发文排序]、[年份关键词排序]、[合作图并查集解析结果]等数据。在建库完成后，在程序前端中可以输入对应搜索内容，通过哈希值的映射打开对应的数据文件，将所需要的数据文件IO出来并解析后打印在屏幕上。

![image](https://user-images.githubusercontent.com/74815734/164518832-2c3341c2-d4d1-47f5-b80c-02fb70d9e97e.png)

![image](https://user-images.githubusercontent.com/74815734/164518865-9f8be4d1-f9ca-4625-849e-313ebb0c67c2.png)

![image](https://user-images.githubusercontent.com/74815734/164518972-1ac79236-3040-490e-b0cc-41fef08a0010.png)


运行截图
![image](https://user-images.githubusercontent.com/74815734/164518237-a542aede-bfb4-4cf3-b5d0-f5b09c5a6bcb.png)
![image](https://user-images.githubusercontent.com/74815734/164518262-44649e5e-dc9d-4a7c-bfc4-12f0267fff39.png)
![image](https://user-images.githubusercontent.com/74815734/164518296-b20de763-8ecf-4cc1-b42b-6701aaff8a53.png)
![image](https://user-images.githubusercontent.com/74815734/164518319-4e1a814a-9890-4282-a8a7-360527fca5b2.png)
![image](https://user-images.githubusercontent.com/74815734/164518349-fea03b99-e9c2-4aec-b857-b796bd0cf014.png)


功能实现
F1. 基本搜索功能。输入作者名，能展示该作者发表的所有论文信息。输入完整的论文的题目，能展示该论文的其他相关信息
F2. 相关搜索。输入作者名，能展示于该作者有合作关系的其他所以作者。
F3. 作者统计功能。输出写文章最多的前100名作者。
F4. 热点分析功能。分析每一年发表的文章中，题目所包含的单词中，出现频率排名前10的关键词。
F5. 部分匹配搜索功能。给定若干个关键字，能快速搜索到题目中包含该关键字的文章信息
F6. 聚团分析。作者之间的合作关系可以看成是一个图，每个作者对应一个顶点，任两个作者之间如果存在合作关系，则在两个顶点之间建立连边。这个图中的每一个完全子图我们称为一个聚团（所谓完全子图指的是该子图的任意顶点都和该子图的其他顶点有连边，完全子图的顶点个数称为该完全子图的阶数），请统计整个图中各阶完全子图的个数。
F7. 可视化显示。通过图形化界面，展示作者之间合作关系图及其相关文章信息。


7.科学文献管理系统
科研工作者的日常工作离不开查阅科学文献，并对其中的信息进行分析、筛选、挖掘和管理。请你为科研工作者设计一个管理系统，提高科学文献的管理效率。
目标用户：科研工作者。
数据配置：请通过以下方法下载数据文件dblp.xml.gz.
http://dblp.uni-trier.de/xml/dblp.xml.gz
   将该数据文件解压后，其中包含一个dblp.xml文件。该文件由科学文献的记录序列组成，记录的格式如下所示：
 <article mdate="2002-01-03" key="persons/Codd71a">
<author>E. F. Codd</author>
<title>Further Normalization of the Data Base Relational Model.</title>
<journal>IBM Research Report, San Jose, California</journal>
<volume>RJ909</volume>
<month>August</month>
<year>1971</year>
<cdrom>ibmTR/rj909.pdf</cdrom>
<ee>db/labs/ibm/RJ909.html</ee>
</article>
每个记录对应一篇文章，其中包含对作者，题名，发表杂志，卷号，出版时间等的详细说明。请基于该数据，设计能满足后述功能的文献管理系统。
1）dblp.xml的大小超过1G，所以不要直接点击打开该文件。需要通过命令行命令’more’ 或者自行编程查看。
2） 编程过程中，不允许使用数据库系统。需要自己建立管理文献数据的数据结构。
