using libMetroTunnelDB;
using System;
using System.Runtime.InteropServices;

namespace FileIO
{
    public class ModelHandler
    {
        //Row num for the whole section and the displayed section
        public const int SectionRowAll = 2048*8;
        public const int SectionRowShow = 2048;
        public struct WholeSection
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SectionRowAll)]
            public Single[] s;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SectionRowAll)]
            public Single[] a;
        }
        public struct ShowSection
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SectionRowShow)]
            public Single[] s;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SectionRowShow)]
            public Single[] a;
        }

        public struct ConvertResParam
        {
            DataRaw inputxy;
            DataConv outputsa;
            int cam_num;
        }

        // Covert from x,y to s,a  return: true->success  false->fail  2020/10/20 updated
        public static bool ConvertRes(libMetroTunnelDB.DataRaw inputxy, ref libMetroTunnelDB.DataConv_SingleCam outputsa)
        {

            try
            {
                int CameraNum = inputxy.CameraID;
                if (CameraNum == 999 || CameraNum == 0)
                {
                    Console.WriteLine("That is a wrong camera Serialnum for result conversion!\n");
                    return false;
                }
                //Copy the SerialNum and Timestamp
                outputsa.CameraID = CameraNum;
                outputsa.Timestamp = inputxy.TimeStamp;
                for (int i = 0; i < CSVHandler.DataRows; i++)
                {
                    //Calculate the s a from x y
                    if (inputxy.x[i] == 0 || inputxy.y[i] == 0)
                    {
                        outputsa.s[i] = 0;
                        outputsa.a[i] = 0;
                    }
                    else
                    {
                        try
                        {
                            float cd = Convert.ToSingle(Math.Atan((ConfigHandler.CameraVo[CameraNum] - inputxy.y[i]) / ConfigHandler.CameraFy[CameraNum]));
                            float x = inputxy.x[i];
                            //ay load the device degree
                            outputsa.a[i] = System.Convert.ToSingle(ConfigHandler.CameraK00[CameraNum] + ConfigHandler.CameraK10[CameraNum] * cd + ConfigHandler.CameraK01[CameraNum] * x + ConfigHandler.CameraK11[CameraNum] * cd * x + ConfigHandler.CameraK02[CameraNum] * x * x);
                            float Yy = cd;
                            float alpha = System.Convert.ToSingle(ConfigHandler.CameraDegree[CameraNum] * 3.14159265358 / 180 + outputsa.a[i]);
                            float Fx = Convert.ToSingle(ConfigHandler.CameraP00[CameraNum] + ConfigHandler.CameraP10[CameraNum] * x + ConfigHandler.CameraP01[CameraNum] * Yy + ConfigHandler.CameraP20[CameraNum] * x * x + ConfigHandler.CameraP11[CameraNum] * x * Yy + ConfigHandler.CameraP02[CameraNum] * Yy * Yy);
                            //float Fx = (_Default.CameraP1[CameraNum] * SockPackc.s[i] * SockPackc.s[i] * SockPackc.s[i] + _Default.CameraP2[CameraNum] * SockPackc.s[i] * SockPackc.s[i] + _Default.CameraP3[CameraNum] * SockPackc.s[i] + _Default.CameraP4[CameraNum]);
                            float scor = System.Convert.ToSingle((ConfigHandler.CameraBmm[CameraNum] * Math.Tan(ConfigHandler.CameraPhi[CameraNum] + Math.Atan((ConfigHandler.CameraUo[CameraNum] - inputxy.x[i]) / ConfigHandler.CameraFx[CameraNum])) + ConfigHandler.CameraM[CameraNum]) / Fx);
                            float num = (scor * ConfigHandler.CameraBmm[CameraNum] + (scor - ConfigHandler.CameraM[CameraNum]) * ConfigHandler.BP1);
                            float den = Convert.ToSingle(ConfigHandler.CameraBmm[CameraNum] + (ConfigHandler.CameraM[CameraNum] - scor) * (ConfigHandler.BP2 * Math.Cos(alpha) + ConfigHandler.BP3 * Math.Sin(alpha)));
                            outputsa.s[i] = num / den;
                            outputsa.a[i] += (float)(ConfigHandler.CameraDegree[CameraNum] * Math.PI / 180);
                        }
                        catch (System.NullReferenceException)
                        {

                        }
                    }
                }
                return true;
            }
            catch (System.NullReferenceException)
            {
                return false;
            }

        }


        //Covert from x,y to s,a  return: true->success  false->fail
        public static bool ConvertRes(CSVHandler.ResPackage inputxy, ref CSVHandler.CalResPackage outputsa)
        {
            try
            {
                int CameraNum = ConfigHandler.GetCameraNum(inputxy.SerialNum);
                if (CameraNum == 999 || CameraNum == 0)
                {
                    Console.WriteLine("That is a wrong camera Serialnum for result conversion!\n");
                    return false;
                }
                //Copy the SerialNum and Timestamp
                outputsa.SerialNum = CameraNum;
                outputsa.Timestamp = inputxy.Timestamp;
                for (int i = 0; i < CSVHandler.DataRows; i++)
                {
                    //Calculate the s a from x y
                    if (inputxy.x[i] == 0 || inputxy.y[i] == 0)
                    {
                        outputsa.s[i] = 0;
                        outputsa.a[i] = 0;
                    }
                    else
                    {
                        try
                        {
                            float cd = Convert.ToSingle(Math.Atan((ConfigHandler.CameraVo[CameraNum] - inputxy.y[i]) / ConfigHandler.CameraFy[CameraNum]));
                            float x = inputxy.x[i];
                            //ay load the device degree
                            outputsa.a[i] = System.Convert.ToSingle(ConfigHandler.CameraK00[CameraNum] + ConfigHandler.CameraK10[CameraNum] * cd + ConfigHandler.CameraK01[CameraNum] * x + ConfigHandler.CameraK11[CameraNum] * cd * x + ConfigHandler.CameraK02[CameraNum] * x * x);
                            float Yy = cd;
                            float alpha = System.Convert.ToSingle(ConfigHandler.CameraDegree[CameraNum] * 3.14159265358 / 180 + outputsa.a[i]);
                            float Fx = Convert.ToSingle(ConfigHandler.CameraP00[CameraNum] + ConfigHandler.CameraP10[CameraNum] * x + ConfigHandler.CameraP01[CameraNum] * Yy + ConfigHandler.CameraP20[CameraNum] * x * x + ConfigHandler.CameraP11[CameraNum] * x * Yy + ConfigHandler.CameraP02[CameraNum] * Yy * Yy);
                            //float Fx = (_Default.CameraP1[CameraNum] * SockPackc.s[i] * SockPackc.s[i] * SockPackc.s[i] + _Default.CameraP2[CameraNum] * SockPackc.s[i] * SockPackc.s[i] + _Default.CameraP3[CameraNum] * SockPackc.s[i] + _Default.CameraP4[CameraNum]);
                            float scor = System.Convert.ToSingle((ConfigHandler.CameraBmm[CameraNum] * Math.Tan(ConfigHandler.CameraPhi[CameraNum] + Math.Atan((ConfigHandler.CameraUo[CameraNum] - inputxy.x[i]) / ConfigHandler.CameraFx[CameraNum])) + ConfigHandler.CameraM[CameraNum]) / Fx);
                            float num = (scor * ConfigHandler.CameraBmm[CameraNum] + (scor - ConfigHandler.CameraM[CameraNum]) * ConfigHandler.BP1);
                            float den = Convert.ToSingle(ConfigHandler.CameraBmm[CameraNum] + (ConfigHandler.CameraM[CameraNum] - scor) * (ConfigHandler.BP2 * Math.Cos(alpha) + ConfigHandler.BP3 * Math.Sin(alpha)));
                            outputsa.s[i] = num / den;
                        }
                        catch (System.NullReferenceException)
                        {
                            
                        }
                    }
                }
                return true;
            }
            catch(System.NullReferenceException)
            {
                return false;
            }

        }


        
        //Filter for Display (Band-Pass Filter and Kalman Filter）
        public void SmoothDataDisp(WholeSection inputsection, ShowSection outputsection)
        {

        }
    }
}