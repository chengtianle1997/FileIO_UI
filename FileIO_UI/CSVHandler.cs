using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using FileIO_UI;
using libMetroTunnelDB;

namespace FileIO
{
    public class CSVHandler
    {
        public const int DataRows = 2048;
        //RAW x-y data struct
        public struct ResPackage
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public char[] SerialNum;
            public int Timestamp;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DataRows)]
            public Single[] x;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DataRows)]
            public Single[] y;
        }
        //Converted s-a data struct
        public struct CalResPackage
        {
            public int SerialNum;
            public int Timestamp;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DataRows)]
            public Single[] s;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DataRows)]
            public Single[] a;
        }

        public struct MjpegTimeStamp
        {
            public int FrameNum;
            public int Timestamp;
        }

        public struct MileageTime
        {
            public int Timestamp;
            public double mileage;
        }

        public struct HandleCSVParam
        {
            public int record_id;
            public string filepath;
            public MainWindow mw;
        }

        public struct HandleTimestampParam
        {
            public int record_id;
            public int cam_num;
            public string csv_file_path;
            public string mjpeg_root_path;
            public MainWindow mw;
        }

        public struct HandleMileageParam
        {
            public int record_id;
            public string csv_file_path;
            public MainWindow mw;
        }
        
        // Read and Handle the mjpeg-timestamp.csv file
        public static void HandleTimestamp(object param)
        {
            HandleTimestampParam htime_param = (HandleTimestampParam)param;
            int record_id = htime_param.record_id;
            int cam_num = htime_param.cam_num;
            string csv_file_path = htime_param.csv_file_path;
            string mjpeg_root_path = htime_param.mjpeg_root_path;
            MainWindow mw = htime_param.mw;

            FileStream fs = new FileStream(csv_file_path, FileMode.Open, System.IO.FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            MetroTunnelDB database = new MetroTunnelDB();

            // Record the Line once
            string strline;
            string[] arrline = null;
            string[] tablehead = null;
            int columncount = 0;
            bool isfirst = true;

            while ((strline = sr.ReadLine()) != null)
            {
                MjpegTimeStamp mjpegTimeStamp = new MjpegTimeStamp();
                arrline = strline.Split(',');
                if (isfirst)
                {
                    columncount = arrline.Length;
                    isfirst = false;
                }
                for(int i = 0; i < columncount; i++)
                {
                    try
                    {
                        // FrameNum
                        if(i == 0)
                        {
                            mjpegTimeStamp.FrameNum = Convert.ToInt32(arrline[0]);
                        }
                        else if(i == 1)
                        {
                            mjpegTimeStamp.Timestamp = Convert.ToInt32(arrline[1]);
                        }
                    }
                    catch(System.NullReferenceException)
                    {
                        ;
                    }
                    catch(System.IndexOutOfRangeException)
                    {
                        ;
                    }
                }

                // Get ImageRaw
                String image_url = mjpeg_root_path + "\\f" + mjpegTimeStamp.FrameNum.ToString() + ".jpg";
                ImageRaw imageRaw = new ImageRaw(record_id, mjpegTimeStamp.Timestamp, cam_num, image_url);
                // mw.SubProcessReport(mw.line_counter++);
                mw.line_counter++;
                // Insert into database
                try
                {
                    database.InsertIntoImageRaw(imageRaw);
                }
                catch(System.Exception)
                {
                    mw.DebugWriteLine("视频序列数据库插入异常");
                }
            }
            DataAnalyze.threadControlCounter += 1;
        }

        // Read and Handle the Mileage csv file
        public static void HandleMileage(object param)
        {
            HandleMileageParam hm_param = (HandleMileageParam)param;
            int record_id = hm_param.record_id;
            string csv_file_path = hm_param.csv_file_path;
            MainWindow mw = hm_param.mw;
            FileStream fs = new FileStream(csv_file_path, FileMode.Open, System.IO.FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            MetroTunnelDB database = new MetroTunnelDB();

            // Record the line
            string strline;
            string[] arrline = null;
            int columncount = 0;
            bool isfirst = true;

            while((strline = sr.ReadLine()) != null)
            {
                MileageTime mtime = new MileageTime();
                arrline = strline.Split(',');
                if (isfirst)
                {
                    columncount = arrline.Length;
                    isfirst = false;
                }
                for (int i = 0; i < columncount; i++)
                {
                    try
                    {
                        // FrameNum
                        if (i == 0)
                        {
                            mtime.Timestamp = Convert.ToInt32(arrline[0]);
                        }
                        else if (i == 1)
                        {
                            mtime.mileage = Convert.ToDouble(arrline[1]);
                        }
                    }
                    catch (System.NullReferenceException)
                    {
                        ;
                    }
                    catch (System.IndexOutOfRangeException)
                    {
                        ;
                    }
                }

                // Get TandD
                TandD tandD = new TandD(record_id, mtime.Timestamp, mtime.mileage);
                // mw.SubProcessReport(mw.line_counter++);
                mw.line_counter++;
                // Insert into database
                try
                {
                    database.InsertIntoTandD(tandD);
                }
                catch (System.Exception)
                {
                    mw.DebugWriteLine("视频序列数据库插入异常");
                }
            }
            DataAnalyze.threadControlCounter += 1;
        }

        // Read and Handle the csv-Result File
        public static void HandleCSV(object param)
        {
            HandleCSVParam hcsv_param = (HandleCSVParam)param;
            MetroTunnelDB database = new MetroTunnelDB();
            int record_id = hcsv_param.record_id;
            string filepath = hcsv_param.filepath;
            MainWindow mw = hcsv_param.mw;
            FileStream fs = new FileStream(filepath, FileMode.Open, System.IO.FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            // Record the Line once
            string strline;
            string[] arrline = null;
            string[] tablehead = null;
            int columncount = 0;
            bool isfirst = true;
            int linecount = 1;

            while((strline = sr.ReadLine())!=null)
            {
                //Handle one line each time
                ResPackage LinePack = new ResPackage();
                LinePack.x = new float[DataRows];
                LinePack.y = new float[DataRows];
                CalResPackage LineCalPack = new CalResPackage();
                LineCalPack.s = new float[DataRows];
                LineCalPack.a = new float[DataRows];
                arrline = strline.Split(',');                
                if(isfirst)
                {
                    columncount = arrline.Length;
                    isfirst = false;
                }
                for(int i = 0; i < columncount; i++ )
                {
                    try
                    {
                        //SerialNum
                        if(i==0)
                        {
                            LinePack.SerialNum = arrline[0].ToCharArray();
                        }
                        //Timestamp
                        else if(i==1)
                        {
                            LinePack.Timestamp = Convert.ToInt32(arrline[1]);
                        }
                        else
                        {
                            int count = i / 2 - 1;
                            //x
                            if(i%2==0)
                            {
                                LinePack.x[count] = Convert.ToSingle(arrline[i]);
                            }
                            //y
                            else
                            {
                                // LinePack.y[count] = Convert.ToSingle(arrline[i]);
                                LinePack.y[count] = Convert.ToSingle(count);
                            }
                            
                        }
                    }
                    catch (System.NullReferenceException)
                    {
                        //SerialNum
                        if (i == 0)
                        {
                            continue;
                        }
                        //Timestamp
                        else if (i == 1)
                        {
                           continue;
                        }
                        else
                        {
                            int count = i / 2 - 1;
                            //x
                            if (i % 2 == 0)
                            {
                                LinePack.x[count] = 0;
                            }
                            //y
                            else
                            {
                                LinePack.y[count] = 0;
                            }

                        }
                    }
                    catch (System.IndexOutOfRangeException)
                    {
                        //SerialNum
                        if (i == 0)
                        {
                            continue;
                        }
                        //Timestamp
                        else if (i == 1)
                        {
                            continue;
                        }
                        else
                        {
                            int count = i / 2 -1;
                            //x
                            if (i % 2 == 0)
                            {
                                LinePack.x[count] = 0;
                            }
                            //y
                            else
                            {
                                LinePack.y[count] = 0;
                            }

                        }
                    }
                    
                    
                }
                //Convert to s-a
                bool ret = false;
                ret = ModelHandler.ConvertRes(LinePack, ref LineCalPack);
                if (!ret)
                {
                    Console.WriteLine("Covertion Failed ! ! !\n");
                }

                //Get DataRaw
                DataRaw dataraw = new DataRaw(record_id, LinePack.Timestamp, ConfigHandler.GetCameraNum(LinePack.SerialNum), LinePack.x, LinePack.y);

                //Send to MySQL
                int retm = 0;
                try
                {
                    retm = database.InsertIntoDataRaw(dataraw);
                }
                catch(System.Exception)
                {
                    mw.DebugWriteLine("截面数据库插入异常");
                }
                if (!Convert.ToBoolean(retm))
                {
                    mw.DebugWriteLine("截面数据库插入异常");
                }

                mw.line_counter++;

                // mw.SubProcessReport(mw.line_counter++);

            }
            DataAnalyze.threadControlCounter += 1;
        }

        // Read and Handle the csv-Result File (Multi-Line)
        public static void HandleCSV_multiLine(object param)
        {
            HandleCSVParam hcsv_param = (HandleCSVParam)param;
            MetroTunnelDB database = new MetroTunnelDB();
            int record_id = hcsv_param.record_id;
            string filepath = hcsv_param.filepath;
            MainWindow mw = hcsv_param.mw;
            FileStream fs = new FileStream(filepath, FileMode.Open, System.IO.FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            // Record the Line once
            string strline;
            string[] arrline = null;
            string[] tablehead = null;
            int columncount = 0;
            bool isfirst = true;
            int linecount = 0;

            // Multi-Line Param
            const int line_query_once = 50;
            List<ResPackage> LinePackList = new List<ResPackage>();
            

            while ((strline = sr.ReadLine()) != null)
            {
                //Handle one line each time
                ResPackage LinePack = new ResPackage();
                LinePack.x = new float[DataRows];
                LinePack.y = new float[DataRows];
                //CalResPackage LineCalPack = new CalResPackage();
                //LineCalPack.s = new float[DataRows];
                //LineCalPack.a = new float[DataRows];
                arrline = strline.Split(',');
                if (isfirst)
                {
                    columncount = arrline.Length;
                    isfirst = false;
                }
                for (int i = 0; i < columncount; i++)
                {
                    try
                    {
                        //SerialNum
                        if (i == 0)
                        {
                            LinePack.SerialNum = arrline[0].ToCharArray();
                        }
                        //Timestamp
                        else if (i == 1)
                        {
                            LinePack.Timestamp = Convert.ToInt32(arrline[1]);
                        }
                        else
                        {
                            int count = i / 2 - 1;
                            //x
                            if (i % 2 == 0)
                            {
                                LinePack.x[count] = Convert.ToSingle(arrline[i]);
                            }
                            //y
                            else
                            {
                                // LinePack.y[count] = Convert.ToSingle(arrline[i]);
                                LinePack.y[count] = Convert.ToSingle(count);
                            }

                        }
                    }
                    catch (System.NullReferenceException)
                    {
                        //SerialNum
                        if (i == 0)
                        {
                            continue;
                        }
                        //Timestamp
                        else if (i == 1)
                        {
                            continue;
                        }
                        else
                        {
                            int count = i / 2 - 1;
                            //x
                            if (i % 2 == 0)
                            {
                                LinePack.x[count] = 0;
                            }
                            //y
                            else
                            {
                                LinePack.y[count] = 0;
                            }

                        }
                    }
                    catch (System.IndexOutOfRangeException)
                    {
                        //SerialNum
                        if (i == 0)
                        {
                            continue;
                        }
                        //Timestamp
                        else if (i == 1)
                        {
                            continue;
                        }
                        else
                        {
                            int count = i / 2 - 1;
                            //x
                            if (i % 2 == 0)
                            {
                                LinePack.x[count] = 0;
                            }
                            //y
                            else
                            {
                                LinePack.y[count] = 0;
                            }

                        }
                    }


                }

                LinePackList.Add(LinePack);
                //Convert to s-a
                //bool ret = false;
                //ret = ModelHandler.ConvertRes(LinePack, ref LineCalPack);
                //if (!ret)
                //{
                //    Console.WriteLine("Covertion Failed ! ! !\n");
                //}

                if(LinePackList.Count % line_query_once == 0)
                {
                    DataRaw[] dataRaws = new DataRaw[LinePackList.Count];
                    for (int l = 0; l < LinePackList.Count; l++)
                    {
                        //Get DataRaw
                        dataRaws[l] = new DataRaw(record_id, LinePackList[l].Timestamp, ConfigHandler.GetCameraNum(LinePackList[l].SerialNum), LinePackList[l].x, LinePackList[l].y);                    
                        mw.line_counter++;
                    }
                    //Send to MySQL
                    int retm = 0;
                    try
                    {
                        retm = database.InsertIntoDataRaw(dataRaws);
                    }
                    catch (System.Exception)
                    {
                        mw.DebugWriteLine("截面数据库插入异常");
                    }
                    if (!Convert.ToBoolean(retm))
                    {
                        mw.DebugWriteLine("截面数据库插入异常");
                    }
                    linecount = 0;
                    LinePackList.Clear();
                }               
                // mw.SubProcessReport(mw.line_counter++);

            }
            if (LinePackList.Count > 0)
            {
                DataRaw[] dataRaws = new DataRaw[LinePackList.Count];
                for (int l = 0; l < LinePackList.Count; l++)
                {
                    //Get DataRaw
                    dataRaws[l] = new DataRaw(record_id, LinePackList[l].Timestamp, ConfigHandler.GetCameraNum(LinePackList[l].SerialNum), LinePackList[l].x, LinePackList[l].y);
                    mw.line_counter++;
                }
                //Send to MySQL
                int retm = 0;
                try
                {
                    retm = database.InsertIntoDataRaw(dataRaws);
                }
                catch (System.Exception)
                {
                    mw.DebugWriteLine("截面数据库插入异常");
                }
                if (!Convert.ToBoolean(retm))
                {
                    mw.DebugWriteLine("截面数据库插入异常");
                }
            }

            DataAnalyze.threadControlCounter += 1;
        }

        public static int GetLineCount(string filepath, MainWindow mw)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open, System.IO.FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            int line_counter = 0;
            string strline;

            while ((strline = sr.ReadLine()) != null)
            {
                line_counter++;
                mw.DebugReWriteLine("扫描文件: 已发现" + line_counter +"条数据");
            }

            return line_counter;
        }
    }
}