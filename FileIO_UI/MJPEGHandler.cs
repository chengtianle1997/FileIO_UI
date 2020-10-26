using System;
using System.Diagnostics;
using System.Threading;

namespace FileIO
{
    class MjpegHandler
    {
        public struct DecodeParam
        {
            public string inputvideo;
            public string outputpath;
        }
        public static void CallPythonToDecode(object decodeparam)
        {
            DecodeParam param = (DecodeParam)decodeparam;
            string inputvideo = param.inputvideo;
            string outputpath = param.outputpath;
            //获取本地程序路径以确定python文件位置
            string LocalPath = System.IO.Directory.GetCurrentDirectory().ToString();
            Process decoder = new Process();
            //decoder.StartInfo.FileName = LocalPath + "\\DecodeMod.exe";
            decoder.StartInfo.FileName = "python.exe";
            decoder.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
            decoder.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            decoder.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            decoder.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            decoder.StartInfo.CreateNoWindow = true;//不显示程序窗口
            decoder.StartInfo.Arguments = LocalPath + "\\DecodeMod.py -i \"" + inputvideo + "\" -o \"" + outputpath + "\"";
            decoder.Start();//启动程序
            ////获取本地程序路径以确定python文件位置
            //string LocalPath = System.IO.Directory.GetCurrentDirectory().ToString();
            ////键入命令
            //string decodecmd = "python " + LocalPath + "/DecodeMod.py -i " + inputvideo + " -o " + outputpath;
            //string decodecmd = LocalPath + "\\DecodeMod.exe -i " + inputvideo + " -o " + outputpath;
            //decoder.StandardInput.WriteLine(decodecmd);
            //decoder.StandardInput.AutoFlush = true;
            //while (!decoder.StandardOutput.EndOfStream)
            //{
            //    //Console.WriteLine(decoder.StandardOutput.ReadLine());
            //    Thread.Sleep(10);
            //}
            decoder.WaitForExit();
            decoder.Close();
            DataAnalyze.threadControlCounter += 1;
        }
    }
}