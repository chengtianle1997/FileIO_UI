using System;
using MySql.Data;
using MySql.Data.MySqlClient;
//using System.Data;
using System.Collections.Generic;
using MySqlX.XDevAPI.CRUD;
using FileIO;
using System.ComponentModel.DataAnnotations.Schema;
using FileIO_UI;
using Newtonsoft.Json;

namespace libMetroTunnelDB
{
    interface IGetTimeStamp
    {
        ref int GetTimeStamp();
    }

    public class Line
    {
        public String LineNumber;
        public String LineName;
        public float TotalMileage;
        public DateTime CreateTime;
        public bool? IsValid;
        public int? LineID;

        public Line(String _LineNumber, String _LineName, float _TotalMileage, DateTime _CreateTime, bool? _IsValid = true, int? _LineID = null)
        {
            LineNumber = _LineNumber;
            LineName = _LineName;
            TotalMileage = _TotalMileage;
            CreateTime = _CreateTime;
            IsValid = _IsValid;
            LineID = _LineID;
        }
    }

    public class DetectDevice
    {
        public String DetectDeviceNumber;
        public String DetectDeviceName;
        public int LineID;
        public DateTime CreateTime;
        public bool? IsValid;
        public int? DetectDeviceID;

        public DetectDevice(String _DetectDeviceNumber, String _DetectDeviceName, int _LineID, DateTime _CreateTime, bool? _IsValid = true, int? _DetectDeviceID = null)
        {
            DetectDeviceNumber = _DetectDeviceNumber;
            DetectDeviceName = _DetectDeviceName;
            LineID = _LineID;
            CreateTime = _CreateTime;
            IsValid = _IsValid;
            DetectDeviceID = _DetectDeviceID;
        }
    }

    public class DetectRecord
    {
        public int LineID;
        public DateTime DetectTime;
        public String DeviceID;
        public Single Length;
        public Single Start_Loc;
        public Single Stop_Loc;
        public int? RecordID;

        public DetectRecord(int _LineID, DateTime _DetectTime, String _DeviceID, Single _Length, Single _Start_Loc, Single _Stop_Loc, int? _RecordID = null)
        {
            LineID = _LineID;
            DetectTime = _DetectTime;
            DeviceID = _DeviceID;
            Length = _Length;
            Start_Loc = _Start_Loc;
            Stop_Loc = _Stop_Loc;
            RecordID = _RecordID;
        }
    }

    public class DataOverview
    {
        public int RecordID;
        public double Distance;
        public float LongAxis;
        public float ShortAxis;
        public float HorizontalAxis;
        public float Rotation;
        public bool Constriction;
        public bool Crack;

        public DataOverview(int _RecordID, double _Distance, float _LongAxis, float _ShortAxis, float _HorizontalAxis, float _Rotation, bool _Constriction, bool _Crack)
        {
            RecordID = _RecordID;
            Distance = _Distance;
            LongAxis = _LongAxis;
            ShortAxis = _ShortAxis;
            HorizontalAxis = _HorizontalAxis;
            Rotation = _Rotation;
            Constriction = _Constriction;
            Crack = _Crack;
        }
    }

    // Size: 4K * sizeof(float)
    public class DataRaw : IGetTimeStamp
    {
        public int RecordID;
        public int TimeStamp;
        public int CameraID;
        public float[] x;
        public float[] y;

        public DataRaw(int _RecordID, int _TimeStamp, int _CameraID, float[] _x, float[] _y)
        {
            RecordID = _RecordID;
            TimeStamp = _TimeStamp;
            CameraID = _CameraID;
            x = new float[2048];
            y = new float[2048];
            Array.Copy(_x, 0, x, 0, 2048);
            Array.Copy(_y, 0, y, 0, 2048);
        }

        public DataRaw(int _RecordID, int _Timestamp, int _CameraID)
        {
            RecordID = _RecordID;
            TimeStamp = _Timestamp;
            CameraID = _CameraID;
            x = new float[2048];
            y = new float[2048];
        }

        public ref int GetTimeStamp()
        {
            return ref TimeStamp;
        }
    }

    public class DataConv_SingleCam
    {
        public int RecordID;
        public int Timestamp;
        public int CameraID;
        public float[] s;
        public float[] a;

        public const int floatArrLength = 2048;

        public DataConv_SingleCam(int _RecordID, int _Timestamp, int _CameraID, float[] _s, float[] _a)
        {
            RecordID = _RecordID;
            Timestamp = _Timestamp;
            CameraID = _CameraID;
            s = new float[floatArrLength];
            a = new float[floatArrLength];
            Array.Copy(_s, 0, s, 0, floatArrLength);
            Array.Copy(_a, 0, a, 0, floatArrLength);
        }

        public DataConv_SingleCam(int _RecordID, int _Timestamp, int _CameraID)
        {
            RecordID = _RecordID;
            Timestamp = _Timestamp;
            CameraID = _CameraID;
            s = new float[floatArrLength];
            a = new float[floatArrLength];
        }
    }

    public class DataConv
    {
        public int RecordID;
        public float Distance;
        public float[] s;
        public float[] a;

        public const int floatArrLength = 2048 * 8;

        public DataConv(int _RecordID, float _Distance, float[] _s, float[] _a)
        {
            RecordID = _RecordID;
            Distance = _Distance;
            s = new float[floatArrLength];
            a = new float[floatArrLength];
            Array.Copy(_s, 0, s, 0, floatArrLength);
            Array.Copy(_a, 0, a, 0, floatArrLength);
        }

        public DataConv(int _RecordID, float _Distance)
        {
            RecordID = _RecordID;
            Distance = _Distance;
            s = new float[floatArrLength];
            a = new float[floatArrLength];
        }
    }

    public class DataDisp
    {
        public int RecordID;
        public float Distance;
        public String JsonString;

        public DataDisp(int _RecordID, float _Distance, String _JsonString)
        {
            RecordID = _RecordID;
            Distance = _Distance;
            JsonString = _JsonString;
        }
    }

    public class TandD : IGetTimeStamp
    {
        public int RecordID;
        public int TimeStamp;
        public float Distance;

        public TandD(int _RecordID, int _TimeStamp, float _Distance)
        {
            RecordID = _RecordID;
            TimeStamp = _TimeStamp;
            Distance = _Distance;
        }

        public ref int GetTimeStamp()
        {
            return ref TimeStamp;
        }
    }

    public class ImageRaw
    {
        public int RecordID;
        public int TimeStamp;
        public int CameraID;
        public String FileUrl;

        public ImageRaw(int _RecordID, int _TimeStamp, int _CameraID, String _FileUrl)
        {
            RecordID = _RecordID;
            TimeStamp = _TimeStamp;
            CameraID = _CameraID;
            FileUrl = _FileUrl;
        }

        public ImageRaw(int _RecordID, int _TimeStamp, int _CameraID)
        {
            RecordID = _RecordID;
            TimeStamp = _TimeStamp;
            CameraID = _CameraID;
            FileUrl = "";
        }
    }

    public class ImageDisp
    {
        public int RecordID;
        public float Distance;
        public String[] FileUrl;

        public const int StringArrLength = 8;

        public ImageDisp(int _RecordID, float _Distance, String[] _FileUrl)
        {
            RecordID = _RecordID;
            Distance = _Distance;
            FileUrl = new String[StringArrLength];
            for (int i = 0; i < StringArrLength; i++)
                FileUrl[i] = _FileUrl[i];
        }

        public ImageDisp(int _RecordID, float _Distance)
        {
            RecordID = _RecordID;
            Distance = _Distance;
            FileUrl = new string[StringArrLength];
            for (int i = 0; i < StringArrLength; i++)
                FileUrl[i] = "";
        }
    }

    public class DataEntry
    {
        public static int maxCameraID = 8;
        public static int tsTolerate = 100;
        public static int floatArrLength = 2048;

        public int RecordID;
        public int CameraID;
        public int TimeStamp;
        public float Distance;
        public float[] x;
        public float[] y;
        public float[] s;
        public float[] a;
        public String JsonString;
        public String FileUrl;

        public static float[] ByteArr2FloatArr(Byte[] buf)
        {
            int nfeatures = buf.Length / sizeof(float);
            float[] x = new float[nfeatures];
            for (int i = 0; i < nfeatures; i++)
                x[i] = BitConverter.ToSingle(buf, i * sizeof(float));
            return x;
        }

        public static Byte[] FloatArr2ByteArr(float[] x)
        {
            Byte[] arr = new Byte[sizeof(float) * x.Length];
            for (int i = 0; i < x.Length; i++)
                Array.Copy(BitConverter.GetBytes(x[i]), 0, arr, i * sizeof(float), sizeof(float));
            return arr;
        }

        public DataEntry(in DataRaw entry)
        {

        }
    }

    public class DisplayPCLJson
    {
        public String dpNo { set; get; }
        public List<String> value { set; get; }
        public itemSyleJson itemStyle { set; get; }

        public DisplayPCLJson(int _dpNo, float _x, float _y, float _z, bool _isAbnormal)
        {
            dpNo = _dpNo.ToString();
            value = new List<String>();
            value.Add(_x.ToString());
            value.Add(_y.ToString());
            value.Add(_z.ToString());
            itemStyle = new itemSyleJson(_isAbnormal);
        }
    }

    public class itemSyleJson
    {
        public String color { set; get; }
        public itemSyleJson(bool _isAbnormal)
        {
            if (_isAbnormal)
                color = "red";
            else
                color = "blue";
        }
    }

    public class MetroTunnelDB
    {
        // MySQL connector.
        private MySqlConnection conn;
        // Randomize generator.
        private Random ro;
        private readonly int milisec_hour = 60 * 60 * 1000;
        private readonly int milisec_day = 24 * 60 * 60 * 1000;

        private int camera_num_max = 8;


        // ---------------------------------------------------------------
        // Build connector and test connection.
        public MetroTunnelDB(String server = "localhost", int port = 3306, String user = "root", String passwd = "voies2020")
        {
            String connStr = String.Format("server={0}; user={1}; database=metrodb; port={2}; password={3}", server, user, port, passwd);
            ro = new Random();
            conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
            }
            finally
            {
                conn.Close();
            }
        }

        // ---------------------------------------------------------------
        // Declaration of entryWriter delegates, to provide tidier
        // query interface implementations.

        protected delegate void entryWriter<T>(ref MySqlCommand cmd, in T entry);

        // ---------------------------------------------------------------
        // Query engines.
        protected int DoInsertQuery<T>(String queryStr, in T entry, entryWriter<T> entryWriter)
        {
            int ret = 0;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(queryStr, conn);
                entryWriter(ref cmd, entry);
                ret = cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
            return ret;
        }
        protected int DoBatchInsertQuery<T>(String queryStr, in T[] arr, entryWriter<T> entryWriter)
        {
            int ret = 0;
            try
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(queryStr, conn);
                foreach (T entry in arr)
                {
                    entryWriter(ref cmd, entry);
                    ret += cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
            }
            finally
            {
                conn.Close();
            }
            return ret;
        }

        // ---------------------------------------------------------------
        // Implementations of data writers.

        protected void WriteLine(ref MySqlCommand cmd, in Line entry)
        {
            cmd.Parameters.AddWithValue("@LineNumber", entry.LineNumber);
            cmd.Parameters.AddWithValue("@LineName", entry.LineName);
            cmd.Parameters.AddWithValue("@TotalMileage", entry.TotalMileage);
            cmd.Parameters.AddWithValue("@CreateTime", entry.CreateTime);
            if (entry.IsValid.HasValue)
                cmd.Parameters.AddWithValue("@IsValid", entry.IsValid.Value);
            else
                cmd.Parameters.AddWithValue("@IsValid", true);
            if (entry.LineID.HasValue)
                cmd.Parameters.AddWithValue("@LineID", entry.LineID.Value);
        }

        protected void WriteDetectDevice(ref MySqlCommand cmd, in DetectDevice entry)
        {
            cmd.Parameters.AddWithValue("@DetectDeviceNumber", entry.DetectDeviceNumber);
            cmd.Parameters.AddWithValue("@DetectDeviceName", entry.DetectDeviceName);
            cmd.Parameters.AddWithValue("@LineID", entry.LineID);
            cmd.Parameters.AddWithValue("@CreateTime", entry.CreateTime);
            if (entry.IsValid.HasValue)
                cmd.Parameters.AddWithValue("@IsValid", entry.IsValid);
            else
                cmd.Parameters.AddWithValue("@IsValid", true);
            if (entry.DetectDeviceID.HasValue)
                cmd.Parameters.AddWithValue("@LineID", entry.DetectDeviceID.Value);
        }

        protected void WriteDetectRecord(ref MySqlCommand cmd, in DetectRecord entry)
        {
            cmd.Parameters.AddWithValue("@LineID", entry.LineID);
            cmd.Parameters.AddWithValue("@DetectTime", entry.DetectTime);
            cmd.Parameters.AddWithValue("@DeviceID", entry.DeviceID);
            cmd.Parameters.AddWithValue("@Length", entry.Length);
            cmd.Parameters.AddWithValue("@Start_Loc", entry.Start_Loc);
            cmd.Parameters.AddWithValue("@Stop_Loc", entry.Stop_Loc);
            if (entry.RecordID.HasValue)
                cmd.Parameters.AddWithValue("@RecordID", entry.RecordID.Value);
        }

        protected void WriteDataOverview(ref MySqlCommand cmd, in DataOverview entry)
        {
            cmd.Parameters.AddWithValue("@RecordID", entry.RecordID);
            cmd.Parameters.AddWithValue("@Distance", entry.Distance);
            cmd.Parameters.AddWithValue("@LongAxis", entry.LongAxis);
            cmd.Parameters.AddWithValue("@ShortAxis", entry.ShortAxis);
            cmd.Parameters.AddWithValue("@HorizontalAxis", entry.HorizontalAxis);
            cmd.Parameters.AddWithValue("@Rotation", entry.Rotation);
            cmd.Parameters.AddWithValue("@Constriction", entry.Constriction);
            cmd.Parameters.AddWithValue("@Crack", entry.Crack);
        }

        protected void WriteDataRaw(ref MySqlCommand cmd, in DataRaw entry)
        {
            cmd.Parameters.AddWithValue("@RecordID", entry.RecordID);
            cmd.Parameters.AddWithValue("@TimeStamp", entry.TimeStamp);
            cmd.Parameters.AddWithValue("@CameraID", entry.CameraID);
            cmd.Parameters.AddWithValue("@x", DataEntry.FloatArr2ByteArr(entry.x));
            cmd.Parameters.AddWithValue("@y", DataEntry.FloatArr2ByteArr(entry.y));
        }

        protected void WriteDataConv(ref MySqlCommand cmd, in DataConv entry)
        {
            cmd.Parameters.AddWithValue("@RecordID", entry.RecordID);
            cmd.Parameters.AddWithValue("@Distance", entry.Distance);
            cmd.Parameters.AddWithValue("@s", DataEntry.FloatArr2ByteArr(entry.s));
            cmd.Parameters.AddWithValue("@a", DataEntry.FloatArr2ByteArr(entry.a));
        }

        protected void WriteDataDisp(ref MySqlCommand cmd, in DataDisp entry)
        {
            cmd.Parameters.AddWithValue("@RecordID", entry.RecordID);
            cmd.Parameters.AddWithValue("@Distance", entry.Distance);
            cmd.Parameters.AddWithValue("@JsonString", entry.JsonString);
        }

        protected void WriteTandD(ref MySqlCommand cmd, in TandD entry)
        {
            cmd.Parameters.AddWithValue("@RecordID", entry.RecordID);
            cmd.Parameters.AddWithValue("@TimeStamp", entry.TimeStamp);
            cmd.Parameters.AddWithValue("@Distance", entry.Distance);
        }

        protected void WriteImageRaw(ref MySqlCommand cmd, in ImageRaw entry)
        {
            cmd.Parameters.AddWithValue("@RecordID", entry.RecordID);
            cmd.Parameters.AddWithValue("@TimeStamp", entry.TimeStamp);
            cmd.Parameters.AddWithValue("@CameraID", entry.CameraID);
            cmd.Parameters.AddWithValue("@FileUrl", entry.FileUrl);
        }

        protected void WriteImageDisp(ref MySqlCommand cmd, in ImageDisp entry)
        {
            cmd.Parameters.AddWithValue("@RecordID", entry.RecordID);
            cmd.Parameters.AddWithValue("@Distance", entry.Distance);
            cmd.Parameters.AddWithValue("@FileUrlCam1", entry.FileUrl[0]);
            cmd.Parameters.AddWithValue("@FileUrlCam2", entry.FileUrl[1]);
            cmd.Parameters.AddWithValue("@FileUrlCam3", entry.FileUrl[2]);
            cmd.Parameters.AddWithValue("@FileUrlCam4", entry.FileUrl[3]);
            cmd.Parameters.AddWithValue("@FileUrlCam5", entry.FileUrl[4]);
            cmd.Parameters.AddWithValue("@FileUrlCam6", entry.FileUrl[5]);
            cmd.Parameters.AddWithValue("@FileUrlCam7", entry.FileUrl[6]);
            cmd.Parameters.AddWithValue("@FileUrlCam8", entry.FileUrl[7]);
        }

        // ---------------------------------------------------------------
        // Implements of insert queries.

        public int InsertIntoLine(in Line entry)
        {
            String queryStr;
            if (entry.LineID.HasValue)
            {
                queryStr = "INSERT INTO Line(LineID, LineNumber, LineName, TotalMileage, CreateTime, IsValid)";
                queryStr += "VALUES(@LineID, @LineNumber, @LineName, @TotalMileage, @CreateTime, @IsValid)";
            }
            else
            {
                queryStr = "INSERT INTO Line(LineNumber, LineName, TotalMileage, CreateTime, IsValid)";
                queryStr += "VALUES(@LineNumber, @LineName, @TotalMileage, @CreateTime, @IsValid)";
            }
            int ret = DoInsertQuery(queryStr, entry, WriteLine);
            return ret;
        }

        public int InsertIntoDetectDevice(in DetectDevice entry)
        {
            String queryStr;
            if (entry.DetectDeviceID.HasValue)
            {
                queryStr = "INSERT INTO DetectDevice(DetectDeviceID, DetectDeviceNumber, DetectDeviceName, LineID, CreateTime, IsValid)";
                queryStr += "VALUES(@DetectDeviceID, @DetectDeviceNumber, @DetectDeviceName, @LineID, @CreateTime, @IsValid)";
            }
            else
            {
                queryStr = "INSERT INTO DetectDevice(DetectDeviceNumber, DetectDeviceName, LineID, CreateTime, IsValid)";
                queryStr += "VALUES(@DetectDeviceNumber, @DetectDeviceName, @LineID, @CreateTime, @IsValid)";
            }
            int ret = DoInsertQuery(queryStr, entry, WriteDetectDevice);
            return ret;
        }

        public int InsertIntoDetectRecord(in DetectRecord entry)
        {
            String queryStr;
            if (entry.RecordID.HasValue)
            {
                queryStr = "INSERT INTO DetectRecord(RecordID, LineID, DetectTime, DeviceID, Length, Start_Loc, Stop_Loc)";
                queryStr += "VALUES(@RecordID, @LineID, @DetectTime, @DeviceID, @Length, @Start_Loc, @Stop_Loc)";
            }
            else
            {
                queryStr = "INSERT INTO DetectRecord(LineID, DetectTime, DeviceID, Length, Start_Loc, Stop_Loc)";
                queryStr += "VALUES(@LineID, @DetectTime, @DeviceID, @Length, @Start_Loc, @Stop_Loc)";
            }
            int ret = DoInsertQuery(queryStr, entry, WriteDetectRecord);
            return ret;
        }

        public int InsertIntoDataOverview(in DataOverview entry)
        {
            String queryStr;
            queryStr = "INSERT INTO DataOverview(RecordID, Distance, LongAxis, ShortAxis, HorizontalAxis, Rotation, Constriction, Crack)";
            queryStr += "VALUES(@RecordID, @Distance, @LongAxis, @ShortAxis, @HorizontalAxis, @Rotation, @Constriction, @Crack)";
            int ret = DoInsertQuery(queryStr, entry, WriteDataOverview);
            return ret;
        }

        public int InsertIntoDataOverview(in DataOverview[] arr)
        {
            String queryStr;
            queryStr = "INSERT INTO DataOverview(RecordID, Distance, LongAxis, ShortAxis, HorizontalAxis, Rotation, Constriction, Crack)";
            queryStr += "VALUES(@RecordID, @Distance, @LongAxis, @ShortAxis, @HorizontalAxis, @Rotation, @Constriction, @Crack)";
            int ret = DoBatchInsertQuery(queryStr, arr, WriteDataOverview);
            return ret;
        }

        public int InsertIntoDataRaw(in DataRaw entry)
        {
            String queryStr;
            queryStr = "INSERT INTO DataRaw(RecordID, TimeStamp, CameraID, x, y)";
            queryStr += "VALUES(@RecordID, @TimeStamp, @CameraID, @x, @y)";
            int ret = DoInsertQuery(queryStr, entry, WriteDataRaw);
            return ret;
        }

        public int InsertIntoDataRaw(in DataRaw[] arr)
        {
            String queryStr;
            queryStr = "INSERT INTO DataRaw(RecordID, TimeStamp, CameraID, x, y)";
            queryStr += "VALUES(@RecordID, @TimeStamp, @CameraID, @x, @y)";
            int ret = DoBatchInsertQuery(queryStr, arr, WriteDataRaw);
            return ret;
        }

        public int InsertIntoDataConv(in DataConv entry)
        {
            String queryStr;
            queryStr = "INSERT INTO DataConv(RecordID, Distance, s, a)";
            queryStr += "VALUES(@RecordID, @Distance, @s, @a)";
            int ret = DoInsertQuery(queryStr, entry, WriteDataConv);
            return ret;
        }

        public int InsertIntoDataConv(in DataConv[] arr)
        {
            String queryStr;
            queryStr = "INSERT INTO DataConv(RecordID, Distance, s, a)";
            queryStr += "VALUES(@RecordID, @Distance, @s, @a)";
            int ret = DoBatchInsertQuery(queryStr, arr, WriteDataConv);
            return ret;
        }

        public int InsertIntoDataDisp(in DataDisp entry)
        {
            String queryStr;
            queryStr = "INSERT INTO DataDisp(RecordID, Distance, JsonString)";
            queryStr += "VALUES(@RecordID, @Distance, @JsonString)";
            int ret = DoInsertQuery(queryStr, entry, WriteDataDisp);
            return ret;
        }

        public int InsertIntoDataDisp(in DataDisp[] arr)
        {
            String queryStr;
            queryStr = "INSERT INTO DataDisp(RecordID, Distance, JsonString)";
            queryStr += "VALUES(@RecordID, @Distance, @JsonString)";
            int ret = DoBatchInsertQuery(queryStr, arr, WriteDataDisp);
            return ret;
        }

        public int InsertIntoTandD(in TandD entry)
        {
            String queryStr;
            queryStr = "INSERT INTO TandD(RecordID, TimeStamp, Distance)";
            queryStr += "VALUES(@RecordID, @TimeStamp, @Distance)";
            int ret = DoInsertQuery(queryStr, entry, WriteTandD);
            return ret;
        }

        public int InsertIntoTandD(in TandD[] arr)
        {
            String queryStr;
            queryStr = "INSERT INTO TandD(RecordID, TimeStamp, Distance)";
            queryStr += "VALUES(@RecordID, @TimeStamp, @Distance)";
            int ret = DoBatchInsertQuery(queryStr, arr, WriteTandD);
            return ret;
        }

        public int InsertIntoImageRaw(in ImageRaw entry)
        {
            String queryStr;
            queryStr = "INSERT INTO ImageRaw(RecordID, TimeStamp, CameraID, FileUrl)";
            queryStr += "VALUES(@RecordID, @TimeStamp, @CameraID, @FileUrl)";
            int ret = DoInsertQuery(queryStr, entry, WriteImageRaw);
            return ret;
        }

        public int InsertIntoImageRaw(in ImageRaw[] arr)
        {
            String queryStr;
            queryStr = "INSERT INTO ImageRaw(RecordID, TimeStamp, CameraID, FileUrl)";
            queryStr += "VALUES(@RecordID, @TimeStamp, @CameraID, @FileUrl)";
            int ret = DoBatchInsertQuery(queryStr, arr, WriteImageRaw);
            return ret;
        }

        public int InsertIntoImageDisp(in ImageDisp entry)
        {
            String queryStr;
            queryStr = "INSERT INTO ImageDisp(RecordID, Distance, FileUrlCam1, FileUrlCam2, FileUrlCam3, FileUrlCam4, FileUrlCam5, FileUrlCam6, FileUrlCam7, FileUrlCam8)";
            queryStr += "VALUES(@RecordID, @Distance, @FileUrlCam1, @FileUrlCam2, @FileUrlCam3, @FileUrlCam4, @FileUrlCam5, @FileUrlCam6, @FileUrlCam7, @FileUrlCam8)";
            int ret = DoInsertQuery(queryStr, entry, WriteImageDisp);
            return ret;
        }

        public int InsertIntoImageDisp(in ImageDisp[] arr)
        {
            String queryStr;
            queryStr = "INSERT INTO ImageDisp(RecordID, Distance, FileUrlCam1, FileUrlCam2, FileUrlCam3, FileUrlCam4, FileUrlCam5, FileUrlCam6, FileUrlCam7, FileUrlCam8)";
            queryStr += "VALUES(@RecordID, @Distance, @FileUrlCam1, @FileUrlCam2, @FileUrlCam3, @FileUrlCam4, @FileUrlCam5, @FileUrlCam6, @FileUrlCam7, @FileUrlCam8)";
            int ret = DoBatchInsertQuery(queryStr, arr, WriteImageDisp);
            return ret;
        }

        // ---------------------------------------------------------------
        // Declaration of entryReader delegates, to provide tidier
        // query interface implementations.

        protected delegate T entryReader<T>(MySqlDataReader reader);

        // ---------------------------------------------------------------
        // Query engine.
        protected void DoQuery<T>(String queryStr, ref List<T> arr, entryReader<T> entryReader)
        {
            arr.Clear();
            MySqlCommand cmd = new MySqlCommand(queryStr, conn);
            conn.Open();

            MySqlDataReader reader;
            reader = cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    T entry = entryReader(reader);
                    arr.Add(entry);
                }
            }
            finally
            {
                reader.Close();
                conn.Close();
            }
        }

        // ---------------------------------------------------------------
        // Deletion engine
        protected void DoDelete(String deleteStr)
        {
            MySqlCommand cmd = new MySqlCommand(deleteStr, conn);
            conn.Open();
            try
            {
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
            
        }

        // ---------------------------------------------------------------
        // Implementations of data readers.

        protected Line ReadLine(MySqlDataReader reader)
        {
            Line entry = new Line
            (
                reader.GetString("LineNumber"),
                reader.GetString("LineName"),
                reader.GetFloat("TotalMileage"),
                reader.GetDateTime("CreateTime"),
                reader.GetBoolean("IsValid"),
                reader.GetInt32("LineID")
            );
            return entry;
        }

        protected DetectDevice ReadDetectDevice(MySqlDataReader reader)
        {
            DetectDevice entry = new DetectDevice
            (
                reader.GetString("DetectDeviceNumber"),
                reader.GetString("DetectDeviceName"),
                reader.GetInt32("LineID"),
                reader.GetDateTime("CreateTime"),
                reader.GetBoolean("IsValid"),
                reader.GetInt32("DetectDeviceID")
            );
            return entry;
        }

        protected DetectRecord ReadDetectRecord(MySqlDataReader reader)
        {
            DetectRecord entry = new DetectRecord
            (
                reader.GetInt32("LineID"),
                reader.GetDateTime("DetectTime"),
                reader.GetString("DeviceID"),
                reader.GetFloat("Length"),
                reader.GetFloat("Start_Loc"),
                reader.GetFloat("Stop_Loc"),
                reader.GetInt32("RecordID")
            );
            return entry;
        }

        protected int ReadMaxDetectRecordID(MySqlDataReader reader)
        {
            int RecordID = reader.GetInt32("MAX(RecordID)");
            return RecordID;
        }

        protected DateTime ReadMinDetectRecordTime(MySqlDataReader reader)
        {
            DateTime CreateTime = reader.GetDateTime("MIN(DetectTime)");
            return CreateTime;
        }

        protected DateTime ReadMaxDetectRecordTime(MySqlDataReader reader)
        {
            DateTime CreateTime = reader.GetDateTime("MAX(DetectTime)");
            return CreateTime;
        }

        protected DataOverview ReadDataOverview(MySqlDataReader reader)
        {
            DataOverview entry = new DataOverview
            (
                reader.GetInt32("RecordID"),
                reader.GetDouble("Distance"),
                reader.GetFloat("LongAxis"),
                reader.GetFloat("ShortAxis"),
                reader.GetFloat("HorizontalAxis"),
                reader.GetFloat("Rotation"),
                reader.GetBoolean("Constriction"),
                reader.GetBoolean("Crack")
            );
            return entry;
        }

        protected DataRaw ReadDataRaw(MySqlDataReader reader)
        {
            Byte[] a = new Byte[DataEntry.floatArrLength * sizeof(float)];
            Byte[] b = new Byte[DataEntry.floatArrLength * sizeof(float)];
            reader.GetBytes(3, 0, a, 0, DataEntry.floatArrLength * sizeof(float));
            reader.GetBytes(4, 0, b, 0, DataEntry.floatArrLength * sizeof(float));
            DataRaw entry = new DataRaw
            (
                reader.GetInt32("RecordID"),
                reader.GetInt32("TimeStamp"),
                reader.GetInt32("CameraID"),
                DataEntry.ByteArr2FloatArr(a),
                DataEntry.ByteArr2FloatArr(b)
            );
            return entry;
        }

        protected DataConv ReadDataConv(MySqlDataReader reader)
        {
            Byte[] s = new Byte[DataConv.floatArrLength * sizeof(float)];
            Byte[] a = new Byte[DataConv.floatArrLength * sizeof(float)];
            reader.GetBytes(2, 0, s, 0, DataConv.floatArrLength * sizeof(float));
            reader.GetBytes(3, 0, a, 0, DataConv.floatArrLength * sizeof(float));
            DataConv entry = new DataConv
            (
                reader.GetInt32("RecordID"),
                reader.GetFloat("Distance"),
                DataEntry.ByteArr2FloatArr(s),
                DataEntry.ByteArr2FloatArr(a)
            );
            return entry;
        }

        protected DataDisp ReadDataDisp(MySqlDataReader reader)
        {
            DataDisp entry = new DataDisp
            (
                reader.GetInt32("RecordID"),
                reader.GetFloat("Distance"),
                reader.GetString("JsonString")
            );
            return entry;
        }

        protected TandD ReadTandD(MySqlDataReader reader)
        {
            TandD entry = new TandD
            (
                reader.GetInt32("RecordID"),
                reader.GetInt32("TimeStamp"),
                reader.GetFloat("Distance")
            );
            return entry;
        }

        protected ImageRaw ReadImageRaw(MySqlDataReader reader)
        {
            ImageRaw entry = new ImageRaw
            (
                reader.GetInt32("RecordID"),
                reader.GetInt32("TimeStamp"),
                reader.GetInt32("CameraID"),
                reader.GetString("FileUrl")
            );
            return entry;
        }

        protected ImageDisp ReadImageDisp(MySqlDataReader reader)
        {
            String[] arr = new string[8];
            for (int i = 0; i < 8; i++)
                arr[i] = reader.GetString("FileUrlCam" + (i + 1).ToString());
            ImageDisp entry = new ImageDisp
            (
                reader.GetInt32("RecordID"),
                reader.GetFloat("Distance"),
                arr
            );
            return entry;
        }

        // ---------------------------------------------------------------
        // Implementations of query fuctions.

        public void QueryLine(ref List<Line> e, String LineNumber)
        {
            String queryStr = String.Format("SELECT * FROM Line WHERE LineNumber={0}", LineNumber);
            List<Line> arr = new List<Line>();
            DoQuery(queryStr, ref arr, ReadLine);
            e = arr;
        }

        public void QueryLine(ref Line e, int LineID)
        {
            String queryStr = String.Format("SELECT * FROM Line WHERE LineID={0}", LineID);
            List<Line> arr = new List<Line>();
            DoQuery(queryStr, ref arr, ReadLine);
            e = arr[0];
        }

        public void QueryLine(ref List<Line> e)
        {
            String queryStr = "SELECT * FROM Line";
            List<Line> arr = new List<Line>();
            DoQuery(queryStr, ref arr, ReadLine);
            e = arr;
        }

        public void DeleteLine(String LineNumber)
        {
            String deleteStr = String.Format("DELETE FROM Line WHERE LineNumber={0}", LineNumber);
            DoDelete(deleteStr);
        }

        public void QueryDetectDevice(ref List<DetectDevice> e, String DetectDeviceNumber)
        {
            String queryStr = String.Format("SELECT * FROM DetectDevice WHERE DetectDeviceNumber={0}", DetectDeviceNumber);
            List<DetectDevice> arr = new List<DetectDevice>();
            DoQuery(queryStr, ref arr, ReadDetectDevice);
            e = arr;
        }

        public void QueryDetectDevice(ref DetectDevice e, String DetectDeviceID)
        {
            String queryStr = String.Format("SELECT * FROM DetectDevice WHERE DetectDeviceID={0}", DetectDeviceID);
            List<DetectDevice> arr = new List<DetectDevice>();
            DoQuery(queryStr, ref arr, ReadDetectDevice);
            e = arr[0];
        }

        public void QueryDetectDevice(ref List<DetectDevice> e)
        {
            String queryStr = "SELECT * FROM DetectDevice";
            List<DetectDevice> arr = new List<DetectDevice>();
            DoQuery(queryStr, ref arr, ReadDetectDevice);
            e = arr;
        }

        public void DeleteDetectDevice(String DetectDeviceNumber)
        {
            String deleteStr = String.Format("DELETE FROM DetectDevice WHERE DetectDeviceNumber={0}", DetectDeviceNumber);
            DoDelete(deleteStr);
        }

        public void QueryDetectRecord(ref List<DetectRecord> e)
        {
            String queryStr = "SELECT * FROM DetectRecord";
            List<DetectRecord> arr = new List<DetectRecord>();
            DoQuery(queryStr, ref arr, ReadDetectRecord);
            e = arr;
        }

        public void QueryDetectRecord(ref List<DetectRecord> e, int LineID)
        {
            String queryStr = String.Format("SELECT * FROM DetectRecord WHERE LineID={0}", LineID);
            List<DetectRecord> arr = new List<DetectRecord>();
            DoQuery(queryStr, ref arr, ReadDetectRecord);
            e = arr;
        }

        public void QueryDetectRecord(ref List<DetectRecord> e, DateTime start_time, DateTime stop_time)
        {
            String queryStr = String.Format("SELECT * FROM DetectRecord WHERE DetectTime>=\"{0}\" AND DetectTime<=\"{1}\"", start_time.ToString(), stop_time.ToString());
            List<DetectRecord> arr = new List<DetectRecord>();
            DoQuery(queryStr, ref arr, ReadDetectRecord);
            e = arr;
        }

        public void QueryDetectRecord(ref List<DetectRecord> e, int LineID, DateTime start_time, DateTime stop_time)
        {
            String queryStr = String.Format("SELECT * FROM DetectRecord WHERE LineID={0} AND DetectTime>=\"{1}\" AND DetectTime<=\"{2}\"", 
                LineID, start_time.ToString(), stop_time.ToString());
            List<DetectRecord> arr = new List<DetectRecord>();
            DoQuery(queryStr, ref arr, ReadDetectRecord);
            e = arr;
        }

        public void QueryDetectRecord(ref DetectRecord e, int RecordID)
        {
            String queryStr = String.Format("SELECT * FROM DetectRecord WHERE RecordID={0}", RecordID);
            List<DetectRecord> arr = new List<DetectRecord>();
            DoQuery(queryStr, ref arr, ReadDetectRecord);
            e = arr[0];
        }   

        public void QueryDetectRecord(ref DetectRecord e, String CreateTime)
        {
            String queryStr = String.Format("SELECT * FROM DetectRecord WHERE DetectTime=\"{0}\"", CreateTime);
            List<DetectRecord> arr = new List<DetectRecord>();
            DoQuery(queryStr, ref arr, ReadDetectRecord);
            if (arr.Count < 1)
            {
                e = null;
                return;
            }
            e = arr[0];
        }

        public void GetMaxDetectRecordID(ref int record_id)
        {
            String queryStr = "SELECT MAX(RecordID) FROM DetectRecord";
            List<int> record_ids = new List<int>();
            DoQuery(queryStr, ref record_ids, ReadMaxDetectRecordID);
            record_id = record_ids[0];
        }

        public void GetMaxMinDetectRecordTime(ref DateTime start_time, ref DateTime stop_time)
        {
            String queryStrMin = "SELECT MIN(DetectTime) FROM DetectRecord";
            String queryStrMax = "SELECT MAX(DetectTime) FROM DetectRecord";
            List<DateTime> dateTimes_min = new List<DateTime>();
            List<DateTime> dateTimes_max = new List<DateTime>();
            DoQuery(queryStrMin, ref dateTimes_min, ReadMinDetectRecordTime);
            DoQuery(queryStrMax, ref dateTimes_max, ReadMaxDetectRecordTime);
            start_time = dateTimes_min[0];
            stop_time = dateTimes_max[0];
        }

        public void DeleteDetectRecord(int record_id)
        {
            String deleteStr = "DELETE FROM {0} WHERE RecordID={1}";
            // Delete DataRaw
            String deleteDataRawStr = String.Format(deleteStr, "DataRaw", record_id);
            DoDelete(deleteDataRawStr);
            // Delete DataConv
            String deleteDataConvStr = String.Format(deleteStr, "DataConv", record_id);
            DoDelete(deleteDataConvStr);
            // Delete DataDisp
            String deleteDataDispStr = String.Format(deleteStr, "DataDisp", record_id);
            DoDelete(deleteDataDispStr);
            // Delete DataOverview
            String deleteDataOverviewStr = String.Format(deleteStr, "DataOverview", record_id);
            DoDelete(deleteDataOverviewStr);
            // Delete ImageRaw
            String deleteImageRawStr = String.Format(deleteStr, "ImageRaw", record_id);
            DoDelete(deleteImageRawStr);
            // Delete ImageDisp
            String deleteImageDispStr = String.Format(deleteStr, "ImageDisp", record_id);
            DoDelete(deleteImageDispStr);
            // Delete TandD
            String deleteTandDStr = String.Format(deleteStr, "TandD", record_id);
            DoDelete(deleteTandDStr);
            // Delete DetectRecord
            String deleteDetectRecordStr = String.Format(deleteStr, "DetectRecord", record_id);
            DoDelete(deleteDetectRecordStr);
        }

        public void QueryDataOverview(ref List<DataOverview> arr, int RecordID, float min_Distance = 0, float max_Distance = float.MaxValue)
        {
            String formatStr = "SELECT * FROM DataOverview WHERE RecordID={0} AND Distance>={1} AND Distance<={2}";
            String queryStr = String.Format(formatStr, RecordID, min_Distance, max_Distance);
            DoQuery(queryStr, ref arr, ReadDataOverview);
        }

        public void QueryDataOverview(ref List<DataOverview> arr, int RecordID, int QueryStart, int QueryNum)
        {
            String formatStr = "SELECT * FROM DataOverview WHERE RecordID={0} LIMIT {1},{2}";
            String queryStr = String.Format(formatStr, RecordID, QueryStart, QueryNum);
            DoQuery(queryStr, ref arr, ReadDataOverview);
        }

        public void QueryDataOverview(ref List<DataOverview> arr, int min_RecordID, int max_RecordID, float min_Distance = 0, float max_Distance = float.MaxValue)
        {
            String formatStr = "SELECT * FROM DataOverview WHERE RecordID>={0} AND RecordID<={1} AND Distance>={2} AND Distance<={3}";
            String queryStr = String.Format(formatStr, min_RecordID, max_RecordID, min_Distance, max_Distance);
            DoQuery(queryStr, ref arr, ReadDataOverview);
        }

        public void QueryDataRaw(ref List<DataRaw> arr, int RecordID, int min_TimeStamp = 0, int max_TimeStamp = int.MaxValue)
        {
            String queryStr = String.Format("SELECT * FROM DataRaw WHERE RecordID={0} AND TimeStamp>={1} AND TimeStamp<={2}", RecordID, min_TimeStamp, max_TimeStamp);
            DoQuery(queryStr, ref arr, ReadDataRaw);
        }

        public void QueryDataRaw(ref List<DataRaw> arr, int min_RecordID, int max_RecordID, int min_TimeStamp = 0, int max_TimeStamp = int.MaxValue)
        {
            String formatStr = "SELECT * FROM DataRaw WHERE RecordID>={0} AND RecordID<={1} AND TimeStamp>={2} AND TimeStamp<={3}";
            String queryStr = String.Format(formatStr, min_RecordID, max_RecordID, min_TimeStamp, max_TimeStamp);
            DoQuery(queryStr, ref arr, ReadDataRaw);
        }

        public void QueryDataConv(ref List<DataConv> arr, int RecordID, float min_Distance = 0, float max_Distance = float.MaxValue)
        {
            String formatStr = "SELECT * FROM DataConv WHERE RecordID={0} AND Distance>={1} AND Distance<={2}";
            String queryStr = String.Format(formatStr, RecordID, min_Distance, max_Distance);
            DoQuery(queryStr, ref arr, ReadDataConv);
        }

        public void QueryDataConv(ref List<DataConv> arr, int min_RecordID, int max_RecordID, float min_Distance = 0, float max_Distance = float.MaxValue)
        {
            String formatStr = "SELECT * FROM DataConv WHERE RecordID>={0} AND RecordID<{1} AND Distance>={2} AND Distance<={3}";
            String queryStr = String.Format(formatStr, min_RecordID, max_RecordID, min_Distance, max_Distance);
            DoQuery(queryStr, ref arr, ReadDataConv);
        }

        public void QueryDataConv(ref List<DataConv> arr, int RecordID, int QueryFrom, int maxQuery, float min_Distance = 0, float max_Distance = float.MaxValue)
        {
            String formatStr = "SELECT * FROM DataConv WHERE RecordID={0} AND Distance>={1} AND Distance<={2} LIMIT {3},{4}";
            String queryStr = String.Format(formatStr, RecordID, min_Distance, max_Distance, QueryFrom, maxQuery);
            DoQuery(queryStr, ref arr, ReadDataConv);
        }


        public void QueryDataDisp(ref List<DataDisp> arr, int RecordID, float min_Distance = 0, float max_Distance = float.MaxValue)
        {
            String formatStr = "SELECT * FROM DataDisp WHERE RecordID={0} AND Distance>={1} AND Distance<={2}";
            String queryStr = String.Format(formatStr, RecordID, min_Distance, max_Distance);
            DoQuery(queryStr, ref arr, ReadDataDisp);
        }

        public void QueryDataDisp(ref List<DataDisp> arr, int min_RecordID, int max_RecordID, float min_Distance = 0, float max_Distance = float.MaxValue)
        {
            String formatStr = "SELECT * FROM DataDisp WHERE RecordID>={0} AND RecordID<={1} AND Distance>={2} AND Distance<={3}";
            String queryStr = String.Format(formatStr, min_RecordID, max_RecordID, min_Distance, max_Distance);
            DoQuery(queryStr, ref arr, ReadDataDisp);
        }

        public void QueryTandD(ref List<TandD> arr, int RecordID, int min_TimeStamp = 0, int max_TimeStamp = int.MaxValue)
        {
            String formatStr = "SELECT * FROM TandD WHERE RecordID={0} AND TimeStamp>={1} AND TimeStamp<={2}";
            String queryStr = String.Format(formatStr, RecordID, min_TimeStamp, max_TimeStamp);
            DoQuery(queryStr, ref arr, ReadTandD);
        }

        public void QueryTandD(ref List<TandD> arr, int min_RecordID, int max_RecordID, int min_TimeStamp = 0, int max_TimeStamp = int.MaxValue)
        {
            String formatStr = "SELECT * FROM TandD WHERE RecordID>={0} AND RecordID<={1} AND TimeStamp>={2} AND TimeStamp<={3}";
            String queryStr = String.Format(formatStr, min_RecordID, max_RecordID, min_TimeStamp, max_TimeStamp);
            DoQuery(queryStr, ref arr, ReadTandD);
        }

        public void QueryImageRaw(ref List<ImageRaw> arr, int RecordID, int min_TimeStamp = 0, int max_TimeStamp = int.MaxValue)
        {
            String formatStr = "SELECT * FROM ImageRaw WHERE RecordID={0} AND TimeStamp>={1} AND TimeStamp<={2}";
            String queryStr = String.Format(formatStr, RecordID, min_TimeStamp, max_TimeStamp);
            DoQuery(queryStr, ref arr, ReadImageRaw);
        }

        public void QueryImageRaw(ref List<ImageRaw> arr, int min_RecordID, int max_RecordID, int min_TimeStamp = 0, int max_TimeStamp = int.MaxValue)
        {
            String formatStr = "SELECT * FROM ImageRaw WHERE RecordID>={0} AND RecordID<={1} AND TimeStamp>={2} AND TimeStamp<={3}";
            String queryStr = String.Format(formatStr, min_RecordID, max_RecordID, min_TimeStamp, max_TimeStamp);
            DoQuery(queryStr, ref arr, ReadImageRaw);
        }

        public void QueryImageDisp(ref List<ImageDisp> arr, int RecordID, float min_Distance = 0, float max_Distance = float.MaxValue)
        {
            String formatStr = "SELECT * FROM ImageDisp WHERE RecordID={0} AND Distance>={1} AND Distance<={2}";
            String queryStr = String.Format(formatStr, RecordID, min_Distance, max_Distance);
            DoQuery(queryStr, ref arr, ReadImageDisp);
        }

        //public void QueryImageDisp(ref List<ImageDisp> arr, int min_RecordID, int max_RecordID, float min_Distance = 0, float max_Distance = float.MaxValue)
        //{
        //    String formatStr = "SELECT * FROM ImageDisp WHERE RecordID>={0} AND RecordID<={1} AND Distance>={2} AND Distance<={3}";
        //    String queryStr = String.Format(formatStr, min_RecordID, max_RecordID, min_Distance, max_Distance);
        //    DoQuery(queryStr, ref arr, ReadImageDisp);
        //}

        public String SearchImageUrl(int RecordID, int TimeStamp, int CameraID, int min_TimeStamp = 0, int max_TimeStamp = int.MaxValue)
        {
            String formatStr = "SELECT * FROM ImageRaw WHERE RecordID={0} AND CameraID={1} AND TimeStamp >= {2} AND TimeStamp <= {3}";
            String queryStr = String.Format(formatStr, RecordID, CameraID, min_TimeStamp, max_TimeStamp);

            List<ImageRaw> arr = new List<ImageRaw>();
            DoQuery(queryStr, ref arr, ReadImageRaw);

            int best_index = -1, closest_diff = int.MaxValue;
            for (int i = 0; i < arr.Count; i++)
            {
                if (Math.Abs(arr[i].TimeStamp - TimeStamp) < closest_diff)
                {
                    best_index = i;
                    closest_diff = Math.Abs(arr[i].TimeStamp - TimeStamp);
                }
            }

            return arr[best_index].FileUrl;
        }

        // --------------------------------------------------------------- 2020/10/20 updated
        // Generate DataConv from DataRaw (merge data from 8 cameras
        
        // Get alive camera index
        public void GetAliveCam(int record_id, ref List<int> cam_alive)
        {
            int max_record_num = 10;
            String formatStr = "SELECT * FROM DataRaw WHERE RecordID={0} AND CameraID={1} LIMIT 0,{2}";
            for (int i = 1; i <= camera_num_max; i++)
            {
                String queryStr = String.Format(formatStr, record_id, i, max_record_num);
                List<DataRaw> arr = new List<DataRaw>();
                DoQuery(queryStr, ref arr, ReadDataRaw);
                if(arr.Count > max_record_num - 1)
                {
                    cam_alive.Add(i);
                }
            }
        }

        // Get the interval time between two frames
        public int GetFrameInterval(int record_id, int camera_id)
        {
            int max_record_num = 10;
            String formatStr = "SELECT * FROM DataRaw WHERE RecordID={0} AND CameraID={1} LIMIT 0,{2}";
            String queryStr = String.Format(formatStr, record_id, camera_id, max_record_num);
            List<DataRaw> arr = new List<DataRaw>();
            DoQuery(queryStr, ref arr, ReadDataRaw);
            if (arr.Count < max_record_num)
            {
                return 0;
            }
            int interval_sum = 0;
            int interval_count = 0;
            for (int i = 1; i < arr.Count; i++)
            {
                int int_seq = arr[i].TimeStamp - arr[i - 1].TimeStamp;
                if(int_seq > 0)
                {
                    interval_sum += int_seq;
                    interval_count++;
                }           
            }
            int interval = interval_sum / interval_count;
            return interval;
        }

        // Merge DataRaw from 8 cameras
        public void ProcessDataRaw(int record_id, MainWindow mw)
        {
            // Get alive camera index
            List<int> cam_alive = new List<int>();
            GetAliveCam(record_id, ref cam_alive);

            // Get frame interval
            int interval = GetFrameInterval(record_id, cam_alive[0]);
            if (interval == 0)
                return;

            // Basic Param
            int max_int = Convert.ToInt32(interval * 0.3);
            int query_max = 5000;
            int query_count = 0;
            bool query_exit = false;
            String formatStr = "SELECT * FROM DataRaw WHERE RecordID={0} AND CameraID={1} AND TimeStamp>={2} AND TimeStamp<={3}";
            String formatStrMain = "SELECT * FROM DataRaw WHERE RecordID={0} AND CameraID={1} LIMIT {2},{3}";
            int cam_base = cam_alive[0];
            int MillisecondsMax = 24 * 3600 * 1000;
            int json_cut = 10;
            
            while(!query_exit)
            {
                // Query cam_base
                List<DataRaw> base_raw = new List<DataRaw>();
                String queryStrMain = String.Format(formatStrMain, record_id, cam_base, query_max * query_count, query_max);
                query_count++;
                DoQuery(queryStrMain, ref base_raw, ReadDataRaw);
                if (base_raw.Count <= 0)
                    break;
                //Console.WriteLine("---Merging Group " + query_count + ": " + base_raw.Count +" lines.....\n");
                mw.DebugWriteLine("合并数据分组 " + query_count + ": 总计" + base_raw.Count + "行...");
                // Deal the query_max lines of DataRaw
                for (int i = 0; i < base_raw.Count; i++)
                {
                    int time_base = base_raw[i].TimeStamp;
                    List<DataRaw> line = new List<DataRaw>();
                    List<DataConv> line_conv = new List<DataConv>();
                    DataConv dataConv = new DataConv(record_id, Convert.ToSingle(time_base));
                    // Save the data from base camera
                    line.Add(base_raw[i]);
                    // Query each camera
                    for (int j = 1; j < cam_alive.Count; j++)
                    {
                        String queryStr = String.Format(formatStr, record_id, cam_alive[j], Math.Max(time_base - max_int, 0), Math.Min(time_base + max_int, MillisecondsMax));
                        List<DataRaw> arr = new List<DataRaw>();
                        DoQuery(queryStr, ref arr, ReadDataRaw);
                        if (arr.Count < 1)
                        {
                            // Set all x and y to 0
                            line.Add(new DataRaw(record_id, time_base, cam_alive[j]));
                            continue;
                        }
                        if (arr.Count == 1)
                        {
                            line.Add(arr[0]);
                            continue;
                        }
                        // Find the closest timestamp
                        int closest_time_index = 0;
                        for (int k = 1; k < arr.Count; k++)
                        {
                            if (Math.Abs(arr[k].TimeStamp - time_base) < Math.Abs(arr[closest_time_index].TimeStamp - time_base))
                                closest_time_index = k;
                        }
                        // Save the closest timestamp
                        line.Add(arr[closest_time_index]);
                        
                    }

                    mw.DebugReWriteLine("合并数据分组 " + query_count + ": " + i + "/" + base_raw.Count + "...");
                    mw.SubProcessReport(i + (query_count - 1) * query_max);
                    //Console.Write("\r----->Merging..... " + i + "/" + base_raw.Count + " lines");

                    // Convert x, y to s, a
                    for (int j = 0; j < cam_alive.Count; j++)
                    {
                        // Set timestamp to base time
                        line[j].TimeStamp = time_base;
                        DataConv_SingleCam dataconv_single = new DataConv_SingleCam(record_id, time_base, line[j].TimeStamp);
                        ModelHandler.ConvertRes(line[j], ref dataconv_single);
                        Array.Copy(dataconv_single.s, 0, dataConv.s, 2048 * j, 2048);
                        Array.Copy(dataconv_single.a, 0, dataConv.a, 2048 * j, 2048);
                    }
                    // Send to MySQL
                    InsertIntoDataConv(dataConv);

                    // DataOverview
                    float LongAxis = 0, ShortAxis = 0, HorizontalAxis = 0, Rotation = 0;
                    bool Constriction = false, Crack = false;
                    DataOverview dataOverview = new DataOverview(record_id, dataConv.Distance, LongAxis, ShortAxis, HorizontalAxis, Rotation, Constriction, Crack);
                    InsertIntoDataOverview(dataOverview);
                    
                    List<DisplayPCLJson> pcl_json_list = new List<DisplayPCLJson>();
                    // Package Json
                    for (int j = 0; j < 2048 * cam_alive.Count; j++)
                    {
                        if (j % json_cut == 0)
                        {
                            float a_rotate = (float)(270 * Math.PI / 180 - dataConv.a[i]);
                            float x = dataConv.Distance;
                            float y = (float)(dataConv.s[i] * Math.Cos(a_rotate));
                            float z = (float)(dataConv.s[i] * Math.Sin(a_rotate));
                            pcl_json_list.Add(new DisplayPCLJson((int)(dataConv.Distance), x, y, z, false));
                        }
                    }
                    String pcl_json_str = JsonConvert.SerializeObject(pcl_json_list);
                    DataDisp dataDisp = new DataDisp(record_id, dataConv.Distance, pcl_json_str);
                    InsertIntoDataDisp(dataDisp);
                }
                mw.DebugReWriteLine("合并数据分组 " + query_count + ": 完成");
                Console.WriteLine("\n");
            }
        }

        // Get alive encode camera index
        public void GetAliveCamEnc(int record_id, ref List<int> cam_alive)
        {
            int max_record_num = 10;
            String formatStr = "SELECT * FROM ImageRaw WHERE RecordID={0} AND CameraID={1} LIMIT 0,{2}";
            for(int i = 1; i <= camera_num_max; i++)
            {
                String queryStr = String.Format(formatStr, record_id, i, max_record_num);
                List<ImageRaw> arr = new List<ImageRaw>();
                DoQuery(queryStr, ref arr, ReadImageRaw);
                if(arr.Count > max_record_num - 1)
                {
                    cam_alive.Add(i);
                }
            }
        }

        // Get the interval time between two encode frames
        public int GetFrameIntervalEnc(int record_id, int camera_id)
        {
            int max_record_num = 10;
            String formatStr = "SELECT * FROM ImageRaw WHERE RecordID={0} AND CameraID={1} LIMIT 0,{2}";
            String queryStr = String.Format(formatStr, record_id, camera_id, max_record_num);
            List<ImageRaw> arr = new List<ImageRaw>();
            DoQuery(queryStr, ref arr, ReadImageRaw);
            if(arr.Count < max_record_num)
            {
                return 0;
            }
            int interval_sum = 0;
            int interval_count = 0;
            for(int i = 1; i < arr.Count; i++)
            {
                int int_seq = arr[i].TimeStamp - arr[i - 1].TimeStamp;
                if(int_seq > 0)
                {
                    interval_sum += int_seq;
                    interval_count++;
                }
            }
            int interval = interval_sum / interval_count;
            return interval;
        }

        // Merge ImageRaw from 8 cameras
        public void ProcessImageRaw(int record_id, MainWindow mw)
        {
            // Get alive camera index
            List<int> cam_alive = new List<int>();
            GetAliveCamEnc(record_id, ref cam_alive);

            // Get frame interval
            int interval = GetFrameInterval(record_id, cam_alive[0]);
            if (interval == 0)
                return;

            // Basic Param
            int max_int = Convert.ToInt32(interval * 0.3);
            int query_max = 5000;
            int query_count = 0;
            bool query_exit = false;
            String formatStr = "SELECT * FROM ImageRaw WHERE RecordID={0} AND CameraID={1} AND TimeStamp>={2} AND TimeStamp<={3}";
            String formatStrMain = "SELECT * FROM ImageRaw WHERE RecordID={0} AND CameraID={1} LIMIT {2},{3}";
            int cam_base = cam_alive[0];
            int MillisecondMax = 24 * 3600 * 1000;

            while(!query_exit)
            {
                // Query cam_base
                List<ImageRaw> base_raw = new List<ImageRaw>();
                String queryStrMain = String.Format(formatStrMain, record_id, cam_base, query_max * query_count, query_max);
                query_count++;
                DoQuery(queryStrMain, ref base_raw, ReadImageRaw);
                if (base_raw.Count <= 0)
                    break;
                mw.DebugWriteLine("合并视频序列分组 " + query_count + ": 总计" + base_raw.Count + "行...");
                for (int i = 0; i < base_raw.Count; i++)
                {
                    int time_base = base_raw[i].TimeStamp;
                    List<ImageRaw> line = new List<ImageRaw>();
                    //List<ImageDisp> line_conv = new List<ImageDisp>();
                    ImageDisp imageDisp = new ImageDisp(record_id, Convert.ToSingle(time_base));
                    // Save the data from base camera
                    line.Add(base_raw[i]);
                    // Query each camera
                    for (int j = 1; j < cam_alive.Count; j++)
                    {
                        String queryStr = String.Format(formatStr, record_id, cam_alive[j], Math.Max(time_base - max_int, 0), Math.Min(time_base + max_int, MillisecondMax));
                        List<ImageRaw> arr = new List<ImageRaw>();
                        DoQuery(queryStr, ref arr, ReadImageRaw);
                        if (arr.Count < 1)
                        {
                            line.Add(new ImageRaw(record_id, time_base, cam_alive[j]));
                            continue;
                        }
                        if (arr.Count == 1)
                        {
                            line.Add(arr[0]);
                            continue;
                        }
                        // Find the closest timestamp
                        int close_time_index = 0;
                        for (int k = 1; k < arr.Count; k++)
                        {
                            if (Math.Abs(arr[k].TimeStamp - time_base) < Math.Abs(arr[close_time_index].TimeStamp - time_base))
                                close_time_index = k;
                        }
                        // Save the closest timestamp
                        line.Add(arr[close_time_index]);
                    }

                    mw.DebugReWriteLine("合并视频序列分组 " + query_count + ": " + i + "/" + base_raw.Count + "...");
                    mw.SubProcessReport(i + (query_count - 1) * query_max);

                    // Save info and Send to MySQL
                    for (int j = 0; j < cam_alive.Count; j++)
                    {
                        // FileUrl index start from 0: 0 - CAM1, 1 - CAM2, etc
                        imageDisp.FileUrl[cam_alive[j] - 1] = line[j].FileUrl;
                    }
                    InsertIntoImageDisp(imageDisp);

                }
                mw.DebugReWriteLine("合并视频序列分组 " + query_count + ": 完成");
            }
        }


        // ---------------------------------------------------------------
        // Programming interfaces.

        public delegate void DataConverter(DataRaw[] cluster, ref DataConv dataConverter);

        public void ProcessDetectRecord(int RecordID, DataConverter ProcessData)
        {
            List<DataRaw> arr = new List<DataRaw>();
            QueryDataRaw(ref arr, RecordID);
            int dateDivision = refineTimeStamps(arr);

            List<TandD> tnd_arr = new List<TandD>();
            QueryTandD(ref tnd_arr, RecordID);
            refineTimeStamps(tnd_arr, dateDivision);

            List<DataRaw>[] clustered_arr = new List<DataRaw>[8];
            List<String>[] clustered_fpath = new List<string>[8];
            List<int> clustered_ts = new List<int>();
            ClusterDataRaw(arr, clustered_arr, clustered_fpath, clustered_ts);

            for (int i = 0; i < clustered_ts.Count; i++)
            {
                DataRaw[] cluster = new DataRaw[8];
                String[] imageFileUrl = new string[8];
                // judge incomplete clusters
                bool flag = false;
                for (int j = 0; j < 8; j++)
                {
                    if (clustered_arr[j][i] == null)
                    {
                        flag = true;
                        break;
                    }
                    else
                    {
                        cluster[j] = clustered_arr[j][i];
                        imageFileUrl[j] = clustered_fpath[j][i];
                    }
                }
                if (flag)
                    continue;

                float distance = EstimateDistance(RecordID, clustered_ts[i], dateDivision);

                DataConv dataConverted = new DataConv(RecordID, distance);
                ProcessData(cluster, ref dataConverted);

                InsertIntoDataConv(dataConverted);
                InsertIntoImageDisp(new ImageDisp(RecordID, distance, imageFileUrl));
            }
        }

        // Find a divider for detecting works last until the last day.
        // We sort all the timestamps, and if we find ts[i] < ts[i+1] and ts[i+1] - ts[i] > 2 hours,
        // we think it's a detection job started at night and ended after 24:00.
        private int refineTimeStamps<T>(List<T> arr, int? dateDivision = null) where T : IGetTimeStamp
        {
            if (!dateDivision.HasValue)
            {
                dateDivision = -1;

                int[] ts_arr = new int[arr.Count];
                for (int i = 0; i < arr.Count; i++)
                    ts_arr[i] = arr[i].GetTimeStamp();
                Array.Sort(ts_arr);

                for (int i = 0; i < ts_arr.Length - 1; i++)
                {
                    if (ts_arr[i + 1] - ts_arr[i] > 2 * milisec_hour)
                    {
                        dateDivision = (ts_arr[i] + ts_arr[i + 1]) / 2;
                        break;
                    }
                }
            }

            // refine ts < dateDivision to ts + milisec_day
            for (int i = 0; i < arr.Count; i++)
                if (arr[i].GetTimeStamp() < dateDivision)
                    arr[i].GetTimeStamp() += milisec_day;

            return dateDivision.Value;
        }

        private float EstimateDistance(int RecordID, int TimeStamp, int dateDivision)
        {
            List<TandD> arr = new List<TandD>();
            QueryTandD(ref arr, RecordID);
            if (arr.Count < 2)
                return -1;
            refineTimeStamps(arr, dateDivision);

            int ts1 = arr[0].TimeStamp, ts2 = -1;
            float dist1 = arr[0].Distance, dist2 = -1;
            int diff1 = Math.Abs(arr[0].TimeStamp - TimeStamp), diff2 = int.MaxValue;

            for (int i = 1; i < arr.Count; i++)
            {
                if (Math.Abs(arr[i].TimeStamp - TimeStamp) < diff1)
                {
                    ts2 = ts1;
                    dist2 = dist1;
                    ts1 = arr[i].TimeStamp;
                    dist1 = arr[i].Distance;
                }
                else if (Math.Abs(arr[i].TimeStamp - TimeStamp) < diff2)
                {
                    ts2 = arr[i].TimeStamp;
                    dist2 = arr[i].Distance;
                }
            }

            return ((float)TimeStamp - ts1) / (ts2 - ts1) * (dist2 - dist1) + dist1;
        }

        private void ClusterDataRaw(List<DataRaw> arr, List<DataRaw>[] clustered_arr, List<String>[] clustered_fpath, List<int> clustered_ts)
        {
            // initialize output array_list
            for (int i = 0; i < 8; i++)
            {
                clustered_arr[i] = new List<DataRaw>();
                clustered_fpath[i] = new List<String>();
            }


            while (arr.Count != 0)
            {
                int target_ts = -1;
                int[] best = new int[8];
                int[] cluster = new int[8];
                for (int i = 0; i < 8; i++)
                {
                    best[i] = int.MaxValue;
                    cluster[i] = -1;
                }

                // randomly select a number 1-8 as reference
                int camid = ro.Next(1, 9);
                // get reference timestamp
                for (int i = 0; i < arr.Count; i++)
                    if (arr[i].CameraID == camid)
                    {
                        target_ts = arr[i].TimeStamp;
                        best[camid - 1] = 0;
                        cluster[camid - 1] = i;
                    }
                // reference not found
                if (target_ts == -1)
                    continue;

                // find best fit entries
                for (int i = 0; i < arr.Count; i++)
                {
                    if (cluster[arr[i].CameraID - 1] >= 0)
                        continue;
                    int diff = Math.Abs(target_ts - arr[i].TimeStamp);
                    if (diff < DataEntry.tsTolerate && diff < best[arr[i].CameraID - 1])
                    {
                        best[arr[i].CameraID - 1] = 0;
                        cluster[arr[i].CameraID - 1] = i;
                    }
                }

                // commit a cluster
                int ts_sum = 0, ts_cnt = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (cluster[i] == -1)
                    {
                        clustered_arr[i].Add(null);
                        clustered_fpath[i].Add(null);
                    }
                    else
                    {
                        ts_sum += arr[cluster[i]].TimeStamp;
                        ts_cnt += 1;

                        int key = arr[cluster[i]].TimeStamp;
                        if (key >= milisec_day)
                            key -= milisec_day;
                        //arr[cluster[i]].FileUrl = SearchImageUrl(arr[cluster[i]].RecordID, key, arr[cluster[i]].CameraID);
                        clustered_arr[i].Add(arr[cluster[i]]);
                        clustered_fpath[i].Add(SearchImageUrl(arr[cluster[i]].RecordID, key, arr[cluster[i]].CameraID));
                    }
                }
                int cluster_avg_ts = ts_sum / ts_cnt;
                clustered_ts.Add(cluster_avg_ts);
                Array.Sort(cluster);
                for (int i = 7; i >= 0 && cluster[i] != -1; i--)
                    arr.RemoveAt(cluster[i]);
            }

        }
    }
}
