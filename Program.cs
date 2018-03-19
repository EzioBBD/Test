using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace WordCount
{
    class Program
    {
        static List<string> stopList = new List<string>();   //停用词表
        static List<string> files = new List<string>();      //要处理的文件
        static string outputFlie = "result.txt";            //输出文件名
        static List<string[]> outputInfo = new List<string[]>(); //要输出的信息
        
        static void Main(string[] args)
        {
            if (args.Length < 1)
                return;

            //记录输入文件的下标
            int indexOfFile = 0;
            while (indexOfFile < args.Length)
            {
                if (args[indexOfFile][0] != '-')
                    break;
                ++indexOfFile;
            }

            //处理-e和-o命令
            for (int i = indexOfFile + 1; i < args.Length; ++i)
            {
                if (args[i] == "-e" && i + 1 < args.Length)
                    AddStopList(args[i + 1]);
                else if (args[i] == "-o" && i + 1 < args.Length)
                    outputFlie = args[i + 1];
            }

            string inputFileName = args[indexOfFile];       //记录输入文件名
            //如果遇到-s命令递归处理目录下全部符合规定的文件
            if (args[0] == "-s")
            {
                string path = Directory.GetCurrentDirectory();
                //将通配符转为正则表达式
                string pattern = Regex.Replace(inputFileName, "\\.", "\\.");
                pattern = Regex.Replace(pattern, "\\*", ".*") + "$";
                DisposeAllFile(path, pattern);
                foreach (string file in files)
                    DisposeFile(args, file, indexOfFile);
            }
            else
                DisposeFile(args, inputFileName, indexOfFile);

            //输出到指定文件
            string result = "";
            foreach (string[] infos in outputInfo)
                foreach (string info in infos)
                    if (info != "")
                        result += info;
            File.WriteAllText(outputFlie, result);
        }

        //对-c -w -l -a命令的处理
        static void DisposeFile(string[] args, string fileName, int indexOfFile)
        {
            string[] infos = new string[4];
            for (int i = 0; i < indexOfFile; ++i)
            {
                if (args[i] == "-c")
                    infos[0] = GetCharNum(fileName);
                else if (args[i] == "-w")
                    infos[1] = GetWordNum(fileName);
                else if (args[i] == "-l")
                    infos[2] = GetLineNum(fileName);
                else if (args[i] == "-a")
                    infos[3] = GetMoreInfo(fileName);
            }
            outputInfo.Add(infos);
        }

        //获取字符数
        static string GetCharNum(string fileName)
        {
            string text = File.ReadAllText(fileName);
            return fileName + ",字符数：" + text.Length.ToString() + "\r\n";
        }

        //获取单词数
        static string GetWordNum(string fileName)
        {
            string[] text = File.ReadAllLines(fileName);
            int wordNum = 0;
            foreach (string line in text)
            {
                if (Regex.IsMatch(line, @"^\s*$"))
                    continue;
                string removed = Regex.Replace(line, @"^\s*|\s*$", "");
                string[] words = removed.Split(new char[] { ' ', ','});
                
                if (stopList.Count == 0)
                    wordNum += words.Length;
                else
                    foreach (string word in words)
                        if (!stopList.Contains(word))
                            ++wordNum;
            }
            return fileName + ",单词数：" + wordNum.ToString() + "\r\n";
        }

        //获取行数
        static string GetLineNum(string fileName)
        {
            string[] text = File.ReadAllLines(fileName);
            return fileName + ",行数：" + text.Length.ToString() + "\r\n";
        }

        //获取代码行/空行/注释行
        static string GetMoreInfo(string fileName)
        {
            int codeLine = 0, blankLine = 0, noteLine = 0;
            string[] text = File.ReadAllLines(fileName);
            foreach (string line in text)
            {
                //此处采用正则表达式判断空行和注释行
                if (Regex.IsMatch(line, @"^\s*{?\s*$"))
                    ++blankLine;
                else if (Regex.IsMatch(line, @"^}?//"))
                    ++noteLine;
                else
                    ++codeLine;
            }
            return fileName + ",代码行/空行/注释行：" + codeLine.ToString() + "/"
                + blankLine.ToString() + "/" + noteLine.ToString() + "\r\n";
        }
  
        //处理目录下所有符合条件的文件
        static void DisposeAllFile(string path, string pattern)
        {
            DirectoryInfo root = new DirectoryInfo(path);
            foreach(FileInfo file in root.GetFiles())
                if (Regex.IsMatch(file.Name, pattern))
                    files.Add(file.FullName);
            foreach (DirectoryInfo directory in root.GetDirectories())
                DisposeAllFile(directory.FullName, pattern);
        }

        //保存停用词表的单词
        static void AddStopList(string fileName)
        {
            string text = File.ReadAllText(fileName);
            string[] stopWords = text.Split(' ');
            foreach (string word in stopWords)
                stopList.Add(word);
        }
    }
}
