using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace Inovance
{
    public enum DataType
    {
        //AM600
        ELEM_QX = 0,     //QX元件
        ELEM_MW = 1,     //MW元件
        ELEM_X = 2,		 //X元件(对应QX200~QX300)
        ELEM_Y = 3,		 //Y元件(对应QX300~QX400)

        //H3U
        REGI_H3U_Y = 0x20,       //Y元件的定义	
        REGI_H3U_X = 0x21,		//X元件的定义							
        REGI_H3U_S = 0x22,		//S元件的定义				
        REGI_H3U_M = 0x23,		//M元件的定义							
        REGI_H3U_TB = 0x24,		//T位元件的定义				
        REGI_H3U_TW = 0x25,		//T字元件的定义				
        REGI_H3U_CB = 0x26,		//C位元件的定义				
        REGI_H3U_CW = 0x27,		//C字元件的定义				
        REGI_H3U_DW = 0x28,		//D字元件的定义				
        REGI_H3U_CW2 = 0x29,	//C双字元件的定义
        REGI_H3U_SM = 0x2a,		//SM
        REGI_H3U_SD = 0x2b,		//
        REGI_H3U_R = 0x2c,		//
        //H5u
        REGI_H5U_Y = 0x30,       //Y元件的定义	
        REGI_H5U_X = 0x31,		//X元件的定义							
        REGI_H5U_S = 0x32,		//S元件的定义				
        REGI_H5U_M = 0x33,		//M元件的定义	
        REGI_H5U_B = 0x34,       //B元件的定义
        REGI_H5U_D = 0x35,       //D字元件的定义
        REGI_H5U_R = 0x36,       //R字元件的定义

        //EASY
        REGI_EASY_Y = 0x30,       //Y元件的定义	
        REGI_EASY_X = 0x31,		//X元件的定义							
        REGI_EASY_S = 0x32,		//S元件的定义				
        REGI_EASY_M = 0x33,		//M元件的定义	
        REGI_EASY_B = 0x34,       //B元件的定义
        REGI_EASY_D = 0x35,       //D字元件的定义
        REGI_EASY_R = 0x36,       //R字元件的定义
    }
    public enum PlcType
    {
        AM600,
        H3U,
        H5U,
        EASY
    }
    public class PLC : IDisposable
    {
        private string _ip;
        private int _netId;
        private int _port;
        private bool _isConnected;

        public bool IsConnected { get => _isConnected; private set => _isConnected = value; }
        public string IP { get => _ip; }
        public int NetID { get => _netId; }
        public int Port { get => _port; }

        PlcType plcType;
        object plc = new object();
        public PLC(PlcType type, string ip, int port = 502, int NetiD = 0)
        {
            this.plcType = type;
            _ip = ip;
            _port = port;
            _netId = NetiD;
            IsConnected = false;
        }
        public bool Connect()
        {
            if (disposed) throw new ObjectDisposedException("PLC");

            try
            {
                if (!IsConnected)
                {
                    IsConnected = Init_ETH_String(_ip, _netId, _port);
                    if (!IsConnected)
                    {
                        throw new InvalidOperationException($"Failed to connect to PLC at {_ip}:{_port}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error connecting to PLC: {ex.Message}", ex);
            }
            return IsConnected;
        }
        public bool Close()
        {
            try
            {
                if (IsConnected)
                {
                    IsConnected = !Exit_ETH(NetID);
                }
            }
            catch
            {
                throw;
            }
            return !IsConnected;
        }

        public byte[] ReadBytes(DataType dataType, int nStartAdd, int nCount)
        {
            lock (plc)
            {
                byte[] value = new byte[16000];
                int res = 0;
                switch (plcType)
                {
                    case PlcType.H3U:
                        res = H3u_Read_Soft_Elem(dataType, nStartAdd, nCount, value);
                        if (res != 1) return null;
                        break;
                    case PlcType.H5U:
                        //if (dataType == DataType.REGI_H5U_B)
                        //{
                        //    res = H5u_Read_Device_Block(dataType, nStartAdd, nCount, value);
                        //}
                        //else
                        //{
                        //    res = H5u_Read_Soft_Elem(dataType, nStartAdd, nCount, value);
                        //}
                        res = H5u_Read_Soft_Elem(dataType, nStartAdd, nCount, value);
                        if (res != 1) return null;
                        break;
                    case PlcType.EASY:
                        //if (dataType == DataType.REGI_EASY_B)
                        //{
                        //    res = H5u_Read_Device_Block(dataType, nStartAdd, nCount, value);
                        //}
                        //else
                        //{
                        //    res = H5u_Read_Soft_Elem(dataType, nStartAdd, nCount, value);
                        //}
                        res = H5u_Read_Soft_Elem(dataType, nStartAdd, nCount, value);
                        if (res != 1) return null;
                        break;
                    default:
                        throw new NotSupportedException($"Type {dataType} is not supported.");
                        //break;
                }
                return value;
            }

        }
        public bool WriteBytes(DataType dataType, int nStartAdd, int nCount, byte[] value)
        {
            lock (plc)
            {
                bool res_ = true;
                int res = 0;
                switch (plcType)
                {
                    case PlcType.H3U:
                        res = H3u_Write_Soft_Elem(dataType, nStartAdd, nCount, value);
                        if (res != 1) return !res_;
                        break;
                    case PlcType.H5U:
                        //if (dataType == DataType.REGI_H5U_B)
                        //{
                        //    res = H5u_Write_Device_Block(dataType, nStartAdd, nCount, value);
                        //}
                        //else
                        //{
                        //    res = H5u_Write_Soft_Elem(dataType, nStartAdd, nCount, value);
                        //}
                        res = H5u_Write_Soft_Elem(dataType, nStartAdd, nCount, value);
                        if (res != 1) return !res_;
                        break;
                    case PlcType.EASY:
                        //if (dataType == DataType.REGI_EASY_B)
                        //{
                        //    res = H5u_Write_Device_Block(dataType, nStartAdd, nCount, value);
                        //}
                        //else
                        //{
                        //    res = H5u_Write_Soft_Elem(dataType, nStartAdd, nCount, value);
                        //}
                        res = H5u_Write_Soft_Elem(dataType, nStartAdd, nCount, value);
                        if (res != 1) return !res_;
                        break;
                    default:
                        throw new NotSupportedException($"Type {dataType} is not supported.");
                        //break;
                }
                return res_;
            }

        }
        public T[] ToValue<T>(byte[] data, int index, int count, bool IsWord = false)
        {

            if (data == null || data.Length == 0 || index < 0 || count <= 0 || index + count > data.Length)
            {
                return null;
            }
            object result = null;
            byte[] dataTemp = new byte[count];
            Array.Copy(data, index, dataTemp, 0, count);
            if (IsWord)
            {
                if (typeof(T) == typeof(int))
                {
                    int[] intArray = new int[count / sizeof(int)];
                    for (int i = 0; i < intArray.Length; i++)
                    {
                        intArray[i] = BitConverter.ToInt32(dataTemp, i * sizeof(int));
                    }
                    result = intArray;
                }
                else if (typeof(T) == typeof(short))
                {
                    short[] shortArray = new short[count / sizeof(short)];
                    for (int i = 0; i < shortArray.Length; i++)
                    {
                        shortArray[i] = BitConverter.ToInt16(dataTemp, i * sizeof(short));
                    }
                    result = shortArray;
                }
                else if (typeof(T) == typeof(double))
                {
                    double[] doubleArray = new double[count / sizeof(double)];
                    for (int i = 0; i < doubleArray.Length; i++)
                    {
                        doubleArray[i] = BitConverter.ToDouble(dataTemp, i * sizeof(double));
                    }
                    result = doubleArray;
                }
                else if (typeof(T) == typeof(float))
                {
                    float[] floatArray = new float[count / sizeof(float)];
                    for (int i = 0; i < floatArray.Length; i++)
                    {
                        floatArray[i] = BitConverter.ToSingle(dataTemp, i * sizeof(float));
                    }
                    result = floatArray;
                }
                else if (typeof(T) == typeof(string))
                {
                    result = Encoding.ASCII.GetString(dataTemp,0,dataTemp.Length);
                }
                else
                {
                    throw new NotSupportedException($"Type {typeof(T)} is not supported.");
                }
            }
            else
            {
                if (typeof(T) == typeof(int))
                {
                    int[] intArray = new int[count];
                    for (int i = 0; i < intArray.Length; i++)
                    {
                        intArray[i] = dataTemp[i];
                    }
                    result = intArray;
                }
                else if (typeof(T) == typeof(short))
                {
                    short[] shortArray = new short[count];
                    for (int i = 0; i < shortArray.Length; i++)
                    {
                        shortArray[i] = dataTemp[i];
                    }
                    result = shortArray;
                }
                else if (typeof(T) == typeof(double))
                {
                    double[] doubleArray = new double[count];
                    for (int i = 0; i < doubleArray.Length; i++)
                    {
                        doubleArray[i] = dataTemp[i];
                    }
                    result = doubleArray;
                }
                else if (typeof(T) == typeof(float))
                {
                    float[] floatArray = new float[count];
                    for (int i = 0; i < floatArray.Length; i++)
                    {
                        floatArray[i] = dataTemp[i];
                    }
                    result = floatArray;
                }
                else if (typeof(T) == typeof(bool))
                {
                    bool[] boolArray = new bool[count];
                    for (int i = 0; i < boolArray.Length; i++)
                    {
                        boolArray[i] = dataTemp[i] == 1 ? true: false;
                    }
                    result = boolArray;
                }
                else
                {
                    throw new NotSupportedException($"Type {typeof(T)} is not supported.");
                }
            }
            return result as T[];
        }
        public object[] ToValue (byte[] data, int index, int count, Type type , bool IsWord = false)
        {

            if (data == null || data.Length == 0 || index < 0 || count <= 0 || index + count > data.Length)
            {
                return null;
            }
            byte[] dataTemp = new byte[count];
            Array.Copy(data, index, dataTemp, 0, count);
            if (type == typeof(int))
            {
                object[] intArray;
                if (dataTemp.Length < 4)
                {
                    intArray = new object[1];
                }
                else
                {
                    intArray = new object[count / sizeof(int)];
                }
                for (int i = 0; i < intArray.Length; i++)
                {
                    intArray[i] = BitConverter.ToInt32(dataTemp, i * sizeof(int));
                }
                return intArray;
            }
            else if (type == typeof(short))
            {
                object[] shortArray;
                if (dataTemp.Length < 2)
                {
                    shortArray = new object[1];
                }
                else
                {
                    shortArray = new object[count / sizeof(short)];
                }
                for (int i = 0; i < shortArray.Length; i++)
                {
                    shortArray[i] = BitConverter.ToInt16(dataTemp, i * sizeof(short));
                }
                return shortArray;
            }
            else if (type == typeof(double))
            {
                object[] doubleArray;
                if (dataTemp.Length < 8)
                {
                    doubleArray = new object[1];
                }
                else
                { 
                    doubleArray = new object[count / sizeof(double)];
                }
                for (int i = 0; i < doubleArray.Length; i++)
                {
                    doubleArray[i] = BitConverter.ToDouble(dataTemp, i * sizeof(double));
                }
                return doubleArray;
            }
            else if (type == typeof(float))
            {
                object[] floatArray;
                if(dataTemp.Length < 4)
                {
                    floatArray = new object[1];
                }
                else
                {
                    floatArray = new object[count / sizeof(float)];
                }
                for (int i = 0; i < floatArray.Length; i++)
                {
                    floatArray[i] = BitConverter.ToSingle(dataTemp, i * sizeof(float));
                }
                return floatArray;
            }
            else if (type == typeof(string))
            {
                object[] result = new object[1];
                result[0] = Encoding.ASCII.GetString(dataTemp, 0, dataTemp.Length);
                return result;
            }
            else if (type == typeof(bool) || type == typeof(Boolean))
            {
                object[] boolArray = new object[count];
                for (int i = 0; i < boolArray.Length; i++)
                {
                    boolArray[i] = dataTemp[i] == 1 ? true : false;
                }
                return boolArray;
            }
            else
            {
                throw new NotSupportedException($"Type {type} is not supported.");
            }
        }
        public byte[] ToByteValue(object value, Type type)
        {
            if (value == null)
            {
                return new byte[0];
            }

            int elementSize = type == typeof(bool) ? sizeof(byte) : Marshal.SizeOf(type);
            byte[] result = new byte[elementSize];

            byte[] byteArray = null;

            if (type == typeof(int))
            {
                byteArray = BitConverter.GetBytes(Convert.ToInt32(value));
            }
            else if (type == typeof(short))
            {
                byteArray = BitConverter.GetBytes(Convert.ToInt16(value));
            }
            else if (type == typeof(double))
            {
                byteArray = BitConverter.GetBytes(Convert.ToDouble(value));
            }
            else if (type == typeof(float))
            {
                byteArray = BitConverter.GetBytes(Convert.ToSingle(value));
            }
            else if (type == typeof(bool))
            {
                byteArray = BitConverter.GetBytes(Convert.ToBoolean(value));
            }
            else if (type == typeof(long))
            {
                byteArray = BitConverter.GetBytes(Convert.ToInt64(value));
            }
            else if (type == typeof(byte))
            {
                byteArray = new byte[] { Convert.ToByte(value) };
            }
            else if (type == typeof(char))
            {
                byteArray = BitConverter.GetBytes(Convert.ToChar(value));
            }
            else if (type == typeof(string))
            {
                byteArray = Encoding.ASCII.GetBytes(value.ToString());
            }
            else
            {
                throw new NotSupportedException($"Type {type} is not supported.");
            }

            Array.Copy(byteArray, 0, result, 0, elementSize);

            return result;
        }
        public byte[] ToByteValue<T>(T[] data, int index, int count)
        {
            if (data == null || data.Length == 0 || index < 0 || count <= 0 || index + count > data.Length)
            {
                return new byte[0];
            }

            //int elementSize = Marshal.SizeOf(typeof(T));
            int elementSize = typeof(T) == typeof(bool) ? sizeof(byte) : Marshal.SizeOf(typeof(T));
            byte[] result = new byte[count * elementSize];

            for (int i = 0; i < count; i++)
            {
                byte[] byteArray = null;
                T value = data[index + i];

                if (typeof(T) == typeof(int))
                {
                    byteArray = BitConverter.GetBytes(Convert.ToInt32(value));
                }
                else if (typeof(T) == typeof(short))
                {
                    byteArray = BitConverter.GetBytes(Convert.ToInt16(value));
                }
                else if (typeof(T) == typeof(double))
                {
                    byteArray = BitConverter.GetBytes(Convert.ToDouble(value));
                }
                else if (typeof(T) == typeof(float))
                {
                    byteArray = BitConverter.GetBytes(Convert.ToSingle(value));
                }
                else if (typeof(T) == typeof(bool))
                {
                    byteArray = BitConverter.GetBytes(Convert.ToBoolean(value));
                }
                else
                {
                    throw new NotSupportedException($"Type {typeof(T)} is not supported.");
                }
                Array.Copy(byteArray, 0, result, i * elementSize, elementSize);
            }

            return result;
        }

        public bool WriteSingleValue<T>(DataType dataType ,int startAddress, T value) where T : struct
        {
            //int elementSize = typeof(T) == typeof(bool) ? sizeof(byte) : Marshal.SizeOf(typeof(T));
            byte[] datas = new byte[0];
            if (typeof(T) == typeof(bool))
            {
                datas = BitConverter.GetBytes(Convert.ToBoolean(value));
            }
            else if (typeof(T) == typeof(sbyte))
            {
                datas = new byte[1] { (byte)Convert.ToSByte(value) };
            }
            else if (typeof(T) == typeof(byte))
            {
                datas = new byte[1] { Convert.ToByte(value) };
            }
            else if (typeof(T) == typeof(short))
            {
                datas = BitConverter.GetBytes(Convert.ToInt16(value));
            }
            else if (typeof(T) == typeof(ushort))
            {
                datas = BitConverter.GetBytes(Convert.ToUInt16(value));
            }
            else if (typeof(T) == typeof(int))
            {
                datas = BitConverter.GetBytes(Convert.ToInt32(value));
            }
            else if (typeof(T) == typeof(uint))
            {
                datas = BitConverter.GetBytes(Convert.ToUInt32(value));
            }
            else if (typeof(T) == typeof(long))
            {
                datas = BitConverter.GetBytes(Convert.ToInt64(value));
            }
            else if (typeof(T) == typeof(ulong))
            {
                datas = BitConverter.GetBytes(Convert.ToUInt64(value));
            }
            else if (typeof(T) == typeof(float))
            {
                datas = BitConverter.GetBytes(Convert.ToSingle(value));
            }
            else if (typeof(T) == typeof(double))
            {
                datas = BitConverter.GetBytes(Convert.ToDouble(value));
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T)} is not supported.");
            }
            bool res = WriteBytes(dataType, startAddress, 1, datas);
            return res;
        }

        public T ReadSingleValue<T>(DataType dataType, int Address) where T : struct
        {
            lock (plc)
            {
                try
                {
                    int elementSize = typeof(T) == typeof(bool) ? sizeof(byte) : Marshal.SizeOf(typeof(T));
                    byte[] datas = new byte[0];
                    datas = ReadBytes(dataType, Address, elementSize);
                    if (datas == null) 
                    {
                        datas = ReadBytes(dataType, Address, elementSize);

                    }
                    if (datas == null)
                    {
                        return default(T);
                    }
                    return ToValue<T>(datas, 0, datas.Length)[0];
                }
                catch
                {
                    return default(T);
                }
            }

        }

        // Thêm trường theo dõi trạng thái disposed
        private bool disposed = false;

        // Triển khai IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Giải phóng tài nguyên
                    if (IsConnected)
                    {
                        Close();
                    }
                }
                disposed = true;
            }
        }

        ~PLC()
        {
            Dispose(false);
        }

        #region library
        [DllImport("StandardModbusApi.dll", EntryPoint = "Init_ETH_String", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Init_ETH_String(string sIpAddr, int nNetId = 0, int IpPort = 502);

        [DllImport("StandardModbusApi.dll", EntryPoint = "Exit_ETH", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Exit_ETH(int nNetId = 0); 

        [DllImport("StandardModbusApi.dll", EntryPoint = "H5u_Write_Soft_Elem", CallingConvention = CallingConvention.Cdecl)]
        private static extern int H5u_Write_Soft_Elem(DataType eType, int nStartAddr, int nCount, byte[] pValue, int nNetId = 0);

        [DllImport("StandardModbusApi.dll", EntryPoint = "H5u_Read_Soft_Elem", CallingConvention = CallingConvention.Cdecl)]
        private static extern int H5u_Read_Soft_Elem(DataType eType, int nStartAddr, int nCount, byte[] pValue, int nNetId = 0);

        [DllImport("StandardModbusApi.dll", EntryPoint = "H5u_Read_Device_Block", CallingConvention = CallingConvention.Cdecl)]
        private static extern int H5u_Read_Device_Block(DataType eType, int nStartAddr, int nCount, byte[] pValue, int nNetId = 0);

        [DllImport("StandardModbusApi.dll", EntryPoint = "H5u_Write_Device_Block", CallingConvention = CallingConvention.Cdecl)]
        private static extern int H5u_Write_Device_Block(DataType eType, int nStartAddr, int nCount, byte[] pValue, int nNetId = 0);

        [DllImport("StandardModbusApi.dll", EntryPoint = "H3u_Read_Soft_Elem", CallingConvention = CallingConvention.Cdecl)]
        private static extern int H3u_Read_Soft_Elem(DataType eType, int nStartAddr, int nCount, byte[] pValue, int nNetId = 0);

        [DllImport("StandardModbusApi.dll", EntryPoint = "H3u_Write_Soft_Elem", CallingConvention = CallingConvention.Cdecl)]
        private static extern int H3u_Write_Soft_Elem(DataType eType, int nStartAddr, int nCount, byte[] pValue, int nNetId = 0);
        #endregion
    }
}
