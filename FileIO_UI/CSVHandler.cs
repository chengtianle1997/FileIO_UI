﻿using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
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

        // Read and Handle the mjpeg-timestamp.csv file
        public static void HandleTimestamp(MetroTunnelDB database, int record_id, int cam_num, string csv_file_path, string mjpeg_root_path)
        {
            FileStream fs = new FileStream(csv_file_path, FileMode.Open, System.IO.FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

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

                // Insert into database
                database.InsertIntoImageRaw(imageRaw);
            }
        }

        //Read and Handle the csv-Result File
        public static void HandleCSV(MetroTunnelDB database, int record_id, string filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open, System.IO.FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            //Record the Line once
            string strline;
            string[] arrline = null;
            string[] tablehead = null;
            int columncount = 0;
            bool isfirst = true;

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
                retm = database.InsertIntoDataRaw(dataraw);
                if (!Convert.ToBoolean(retm))
                {
                    Console.WriteLine("Insert MySQL Failed !!! \n");
                }
            }
        }

    }
}