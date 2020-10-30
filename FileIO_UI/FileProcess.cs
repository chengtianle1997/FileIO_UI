using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using libMetroTunnelDB;
using FileIO_UI;

namespace FileIO
{
    public class FileNames
    {
        public int id { get; set; }
        public string text { get; set; }
        public state state { get; set; }
        public List<FileNames> children { get; set; }
        public string icon { get; set; }
    }
    public class state
    {
        public bool opened { get; set; }
    }
    //以上字段为树形控件中需要的属性
    public class GetSystemAllPath
    {
        //获得指定路径下所有文件名
        public static List<FileNames> getFileName(List<FileNames> list, string filepath)
        {
            DirectoryInfo root = new DirectoryInfo(filepath);
            foreach (FileInfo f in root.GetFiles())
            {
                list.Add(new FileNames
                {
                    text = f.Name,
                    state = new state { opened = false },
                    icon = "jstree-file"
                });
            }
            return list;
        }
        //获得指定路径下的所有子目录名
        // <param name="list">文件列表</param>
        // <param name="path">文件夹路径</param>        
        public static List<FileNames> GetallDirectory(List<FileNames> list, string path)
        {
            DirectoryInfo root = new DirectoryInfo(path);
            var dirs = root.GetDirectories();
            if (dirs.Count() != 0)
            {
                foreach (DirectoryInfo d in dirs)
                {
                    if(IsSystemHidden(d))
                    {
                        continue;
                    }
                    list.Add(new FileNames
                    {
                        text = d.Name,
                        state = new state { opened = false },
                        children = GetallDirectory(new List<FileNames>(), d.FullName)
                    });
                }
            }
            list = getFileName(list, path);
            return list;
        }

        // 获取目录下所有文件总大小
        public static long GetDirectorySize(string dirPath)
        {
            if (!System.IO.Directory.Exists(dirPath))
                return 0;
            long len = 0;
            DirectoryInfo di = new DirectoryInfo(dirPath);
            //获取di目录中所有文件的大小
            foreach (FileInfo item in di.GetFiles())
            {
                len += item.Length;
            }

            //获取di目录中所有的文件夹,并保存到一个数组中,以进行递归
            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectorySize(dis[i].FullName);//递归dis.Length个文件夹,得到每隔dis[i]下面所有文件的大小
                }
            }
            return len;
        }

        // 获取文件大小
        public static long GetFileSize(string filePath)
        {
            long temp = 0;
            //判断当前路径是否指向某个文件
            if (!File.Exists(filePath))
            {
                string[] strs = Directory.GetFileSystemEntries(filePath);
                foreach (string item in strs)
                {
                    temp += GetFileSize(item);
                }
            }
            else
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            return temp;
        }

        private static bool IsSystemHidden(DirectoryInfo dirInfo)
        {
            if (dirInfo.Parent == null)
            {
                return false;
            }
            string attributes = dirInfo.Attributes.ToString();
            if (attributes.IndexOf("Hidden") > -1 && attributes.IndexOf("System") > -1)
            {
                return true;
            }
            return false;
        }

    }

    //Scan all the files and analyze
    public class DataAnalyze
    {
        private static MetroTunnelDB DataBase;
        public static int threadControlCounter = 0;
        // public static int record_id = 1;
        private static MainWindow mw;
        public static void DataAnalyzeInit(MetroTunnelDB database, MainWindow _mw)
        {
            DataBase = database;
            mw = _mw;
            //DataBase.InsertIntoLine(new Line("1", "1号线", 234.43F, DateTime.Now));
            //DataBase.InsertIntoDetectRecord(new DetectRecord(record_id, new DateTime(2019, 11, 28, 23, 30, 00), "1-01", 300, 23421, 23721));
        }
        //The Main Function
        //public static void AnalyzeAll(MainWindow _mw, object o_filepath, int record_id)
        //{
        //    string filepath = (string)o_filepath;
        //    List<FileNames> Filelist = new List<FileNames>();
        //    GetSystemAllPath.GetallDirectory(Filelist,filepath);
            
        //    for(int i = 0; i < Filelist.Count(); i++ )
        //    {              
        //        string timeFolder = filepath +"\\"+ Filelist[i].text;          
        //        //Thread ScanFolderThread = new Thread(ScanFolder);
        //        //ScanFolderThread.Start(timeFolder);
        //        //Console.WriteLine("Analyzing " + timeFolder + ".....\n");
        //        mw.DebugWriteLine("开始分析" + timeFolder + "...");
        //        ScanFolder(timeFolder, record_id);
        //    }
        //}

        //Scan one folder for time
        //public static void ScanFolder(object o_filepath, int record_id)
        //{
        //    string filepath = (string)o_filepath;
        //    string CalResFolder = filepath + "\\CalResult";
        //    //string EncodeFolder = filepath + "\\EncodeResult";
        //    string EncodeFolder = filepath;
        //    //Thread ScanCalThread = new Thread(ScanCalResult);
        //    //Thread ScanEncThread = new Thread(ScanEncodeResult);
        //    //ScanCalThread.Start(CalResFolder);
        //    //ScanEncThread.Start(EncodeFolder);
        //    ScanCalResult(CalResFolder, record_id);
        //    ScanEncodeResult(EncodeFolder, record_id);
        //} 
        
        //Deal with CalResult
        public static void ScanCalResult(object o_filepath, int record_id)
        {
            string filepath = (string)o_filepath;
            List<FileNames> Filelist = new List<FileNames>();
            GetSystemAllPath.GetallDirectory(Filelist, filepath);
            if(Filelist.Count() < 1)
            {
                mw.DebugWriteLine("未发现数据文件");
                return;
            }
            for(int i=0 ; i<Filelist.Count() ; i++)
            {
                string csvpath = filepath + "\\" + Filelist[i].text;
                //Console.WriteLine("---Analyzing " + Filelist[i].text + ".....\n");
                
                mw.DebugWriteLine("分析数据文件" + Filelist[i].text + "...");
                //Handle the csv-Result
                CSVHandler.HandleCSV(DataBase, record_id, csvpath, mw);
                mw.DebugWriteLine("分析数据文件" + Filelist[i].text + "完成");
            }
            
        }

        public static void MergeCalResult(int record_id)
        {
            mw.DebugWriteLine("数据模型解析...");
            // Generate DataConv
            DataBase.ProcessDataRaw(record_id, mw);
            mw.DebugWriteLine("数据模型解析完成");
        }

        //Deal with EncodeResult
        public static void ScanEncodeResult(object o_filepath, int record_id)
        {
            string filepath = (string)o_filepath;
            string EncodeResult = filepath + "\\EncodeResult";
            List<FileNames> Filelist = new List<FileNames>();
            GetSystemAllPath.GetallDirectory(Filelist, EncodeResult);
            //Console.WriteLine("---Decoding " + EncodeResult + ".....\n");
            if(Filelist.Count() < 1)
            {
                mw.DebugWriteLine("未发现视频文件");
                return;
            }
            mw.DebugWriteLine("解析视频目录" + EncodeResult + "...");
            for (int i = 0; i < Filelist.Count(); i++)
            {
                string mjpegpath = EncodeResult + "\\" + Filelist[i].text +"\\"+ Filelist[i].children[0].text;
                string outputpath = filepath + "\\DecodeResult\\" + Filelist[i].text;
                //Console.WriteLine("------Decoding " + Filelist[i].text + ".....\n");
                mw.DebugWriteLine("解析视频文件" + Filelist[i].text + "...");
                //Decode the mjpeg
                MjpegHandler.DecodeParam param;
                param.inputvideo = mjpegpath;
                param.outputpath = outputpath;
                Thread DecodeThread = new Thread(MjpegHandler.CallPythonToDecode);
                DecodeThread.Start(param);
                //MjpegHandler.CallPythonToDecode(param);
                
            }
            mw.DebugWriteLine("导入视频序列" + EncodeResult + "...");
            for(int i = 0; i < Filelist.Count(); i++)
            {
                mw.DebugWriteLine("导入视频序列" + Filelist[i].text + "...");
                string image_root_url = filepath + "\\DecodeResult\\" + Filelist[i].text;
                string mjpeg_csv_path = EncodeResult + "\\" + Filelist[i].text + "\\" + Filelist[i].children[1].text;
                // Get CameraNum from filename
                int cam_num = ConfigHandler.GetCameraNum(Filelist[i].children[1].text.Split("_")[1].Split(".")[0].ToCharArray());
                if (cam_num < 1 || cam_num == 999)
                    continue;
                CSVHandler.HandleTimestamp(DataBase, record_id, cam_num, mjpeg_csv_path, image_root_url, mw);
                mw.DebugWriteLine("导入视频序列" + Filelist[i].text + "完成");
            }
            mw.DebugWriteLine("导入视频序列" + EncodeResult + "完成");
            
            while(threadControlCounter < Filelist.Count())
            {
                int task_now = (int)(GetSystemAllPath.GetDirectorySize(filepath + "\\DecodeResult")/1000000);
                mw.SubProcessReport(task_now + mw.line_counter);
                Thread.Sleep(100);
            }
            threadControlCounter = 0;
            //Console.WriteLine("---Decoding Finished " + EncodeResult + ".....\n");
            mw.DebugWriteLine("解析视频目录" + EncodeResult + "完成");
        }

        public static void MergeEncodeResult(int record_id)
        {
            mw.DebugWriteLine("视频序列整合...");
            DataBase.ProcessImageRaw(record_id, mw);
            mw.DebugWriteLine("视频序列整合完成");
        }
        
        //Deal with Mjpeg
        public static void DealMjpeg(object o_filepath)
        {
            string filepath = (string)o_filepath;
        }
        //Deal with Mjpeg-CSV
        public static void DealMjpegCSV(object o_filepath)
        {
            string filepath = (string)o_filepath;
        }

        public static bool TimeFolderToTime(String time_folder, ref DateTime dateTime)
        {
            dateTime = new DateTime();
            try
            {
                String[] time_arr = time_folder.Split('-');
                String date_time_string = String.Format("{0}-{1}-{2} {3}:{4}:{5}",
                    time_arr[0], time_arr[1], time_arr[2], time_arr[3], time_arr[4], time_arr[5]);
                dateTime = Convert.ToDateTime(date_time_string);
                return true;
            }
            catch(SystemException)
            {
                return false;
            }           
        }
    }



    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        //DataBase Initialize
    //        MetroTunnelDB DataBase = new MetroTunnelDB();
    //        //Scanner Initialize
    //        DataAnalyze.DataAnalyzeInit(DataBase);
    //        //ConfigHandler Initialize
    //        ConfigHandler.ConfigInit();
    //        //Scan the Folder
    //        DataAnalyze.AnalyzeAll("E:\\Test Data");
    //        Console.WriteLine("Finished!");
    //    }
    //}
}
