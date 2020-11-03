using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace FileIO
{
    public class ConfigHandler
    {
        public static float BP1;

        public static float BP2;

        public static float BP3;

        //0到8存储 0为vo 1-8为1-8号相机
        public static double[] CameraDegree = new double[9];
        public static string[] CameraSerial = new string[9];

        public static float[] CameraBmm = new float[9];
        public static float[] CameraPhi = new float[9];
        public static float[] CameraUo = new float[9];
        public static float[] CameraVo = new float[9];
        public static float[] CameraFx = new float[9];
        public static float[] CameraFy = new float[9];
        public static float[] CameraM = new float[9];
        public static float[] CameraP00 = new float[9];
        public static float[] CameraP10 = new float[9];
        public static float[] CameraP01 = new float[9];
        public static float[] CameraP20 = new float[9];
        public static float[] CameraP11 = new float[9];
        public static float[] CameraP02 = new float[9];
        public static float[] CameraK00 = new float[9];
        public static float[] CameraK10 = new float[9];
        public static float[] CameraK01 = new float[9];
        public static float[] CameraK11 = new float[9];
        public static float[] CameraK02 = new float[9];

        public static string LocalPath = System.IO.Directory.GetCurrentDirectory().ToString();

        public static string ConfigFile = LocalPath + "\\Config.ini";
        
        [DllImport("kernel32")]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string filePath);

        public static string ReadString(string Section, string Ident, string Default)
        {
            Byte[] Buffer = new byte[10240];
            int buflen = GetPrivateProfileString(Section, Ident, Default, Buffer, Buffer.GetUpperBound(0), ConfigFile);
            string s = Encoding.GetEncoding(0).GetString(Buffer);
            s = s.Substring(0, buflen);
            return s.Trim();
        }

        public static bool ConfigInit()
        {
            try
            {
                for (int i = 0; i < 9; i++)
                {
                    string CameraN = "Camera" + i;
                    CameraSerial[i] = ReadString("CameraSerial", CameraN, null);
                    CameraDegree[i] = System.Convert.ToDouble(ReadString("CameraDegree", CameraN, null));
                }
                
                BP1 = System.Convert.ToSingle(ReadString("ParamDetail", "BP1", null));
                BP2 = System.Convert.ToSingle(ReadString("ParamDetail", "BP2", null));
                BP3 = System.Convert.ToSingle(ReadString("ParamDetail", "BP3", null));
                for (int i = 1; i <= 8; i++)
                {
                    string CameraNBmm = "Camera" + i + "Bmm";
                    string CameraNPhi = "Camera" + i + "Phi";
                    string CameraNUo = "Camera" + i + "Uo";
                    string CameraNVo = "Camera" + i + "Vo";
                    string CameraNFx = "Camera" + i + "Fx";
                    string CameraNFy = "Camera" + i + "Fy";
                    string CameraNM = "Camera" + i + "M";
                    string CameraNP00 = "Camera" + i + "P00";
                    string CameraNP10 = "Camera" + i + "P10";
                    string CameraNP01 = "Camera" + i + "P01";
                    string CameraNP20 = "Camera" + i + "P20";
                    string CameraNP11 = "Camera" + i + "P11";
                    string CameraNP02 = "Camera" + i + "P02";
                    string CameraNK00 = "Camera" + i + "K00";
                    string CameraNK10 = "Camera" + i + "K10";
                    string CameraNK01 = "Camera" + i + "K01";
                    string CameraNK11 = "Camera" + i + "K11";
                    string CameraNK02 = "Camera" + i + "K02";

                    CameraBmm[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNBmm, null));
                    CameraPhi[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNPhi, null));
                    CameraUo[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNUo, null));
                    CameraVo[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNVo, null));
                    CameraFx[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNFx, null));
                    CameraFy[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNFy, null));
                    CameraM[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNM, null));
                    CameraP00[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNP00, null));
                    CameraP10[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNP10, null));
                    CameraP01[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNP01, null));
                    CameraP20[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNP20, null));
                    CameraP11[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNP11, null));
                    CameraP02[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNP02, null));
                    CameraK00[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNK00, null));
                    CameraK10[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNK10, null));
                    CameraK01[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNK01, null));
                    CameraK11[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNK11, null));
                    CameraK02[i] = System.Convert.ToSingle(ReadString("CameraParam", CameraNK02, null));

                }

            }
            catch (FormatException)
            {
                MessageBox.Show("配置文件缺失，请重启软件", "确认", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        //Get CameraNum from SerialNum
        public static int GetCameraNum(object serialNum)
        {
            char[] SerialNumc = (char[])serialNum;
            int SerialLength = SerialNumc.Length;
            //for (int i = 0; i < 64; i++)
            //{
            //    if (SerialNumc[i].ToString() != "\0")
            //    {
            //        ;
            //    }
            //    else
            //    {
            //        SerialLength = i;
            //        break;
            //    }
            //}

            //string SerialNums = new string(SerialNumc);
            //string SerialNum = SerialNums.Substring(0, SerialLength);
            string SerialNum = new string(SerialNumc);
            int CameraNum = 999;
            for (int i = 0; i < 9; i++)
            {
                if (SerialNum == CameraSerial[i])
                {
                    CameraNum = i;
                }
            }
            return CameraNum;
        }

    }
}