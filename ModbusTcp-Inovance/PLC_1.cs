using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Inovance1
{
    /// <summary>
    /// 汇川PLC读写地址表
    /// 斯内科 20210907
    /// </summary>
    public class InovanceTcp
    {
        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected;
        /// <summary>
        /// 字节存放类型，汇川PLC默认为CDAB
        /// </summary>
        public StoreByteCategory StoreByteCategory { get; set; }
        /// <summary>
        /// 是否字符串颠倒，相邻两个字符交换位置【比如：ABCD存放是BADC】
        /// </summary>
        public bool IsStringReverse = true;
        string ip;
        int port;
        /// <summary>
        /// TCP客户端对象
        /// </summary>
        TcpClient msender;
        /// <summary>
        /// TCP连接成功后生成的发送和接收Session会话对象
        /// </summary>
        Socket msock;
        /// <summary>
        /// 记录Modbus的发送和接收数据包事件
        /// 第一个参数是发送的数据包，第二个参数是响应的数据包，第三个参数的获取到响应数据包所花费的时间（ms）
        /// </summary>
        public event Action<byte[], byte[], double> RecordDataEvent;

        /// <summary>
        /// 汇川PLC的Modbus协议Tcp连接的构造函数（PLC的IP地址、PLC端口号、本机节点）
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号，默认502</param>
        /// <returns>实例化一个Modbus协议Tcp连接</returns>
        public InovanceTcp(string ip, int port, StoreByteCategory storeByteCategory = StoreByteCategory.CDAB, bool isStringReverse = true)
        {
            this.ip = ip;
            this.port = port;
            this.StoreByteCategory = storeByteCategory;
            this.IsStringReverse = isStringReverse;
            IsConnected = false;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisConnect()
        {
            msender?.Close();
            msock?.Close();
            msock?.Dispose();
            IsConnected = false;
        }

        /// <summary>
        /// 连接PLC
        /// </summary>
        public bool Connect()
        {
            if (!IsConnected)
            {
                msender = new TcpClient(ip, port);
                msock = msender.Client;
                msock.ReceiveTimeout = 3000;
                IsConnected = true;
            }
            return IsConnected;

        }

        /// <summary>
        /// 写基本数据类型到PLC，如bool,short,int,uint,float等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startAddress">起始寄存器地址，以字（WORD）为单位，形如MW32000</param>
        /// <param name="value"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int WriteValue<T>(int startAddress, T value, out string msg) where T : struct
        {
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
                //暂不考虑 char(就是ushort，两个字节)，decimal（十六个字节） 等类型
                msg = $"写Modbus数据暂不支持其他类型：{value.GetType()}";
                return -1;
            }
            byte[] rcvBuffer = new byte[0];
            return SendByte(startAddress, false, 0, ref rcvBuffer, out msg, datas);
        }

        /// <summary>
        /// 写入连续的字节数组
        /// </summary>
        /// <param name="startAddress">起始寄存器地址，以字（WORD）为单位，形如MW32000</param>
        /// <param name="buffer"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int WriteValue(int startAddress, byte[] buffer, out string msg)
        {
            byte[] rcvBuffer = new byte[0];
            return SendByte(startAddress, false, 0, ref rcvBuffer, out msg, buffer);
        }
        /// <summary>
        /// 写指定长度的字符串(比如条码)到汇川PLC，最大240个字符
        /// 将字符串填充满length个，不足时用 空字符'\0'填充，相当于清空所有字符后重新填充
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length">字符串的最大长度【范围1~240】,超过240个字符将直接返回错误</param>
        /// <param name="barcode">实际字符串，长度可能小于最大长度length</param>
        /// <returns></returns>
        public int WriteString(int startAddress, int length, string barcode, out string msg)
        {
            if (barcode == null)
            {
                barcode = string.Empty;
            }
            //将字符串填充满length个，不足时用 空字符'\0'填充，相当于清空所有字符后填充
            //防止出现 上一次写 【ABCD】，本次写 【12】， 读取字符串 结果是【12CD】 的问题
            barcode = barcode.PadRight(length, '\0');
            return WriteValue(startAddress, Encoding.ASCII.GetBytes(barcode), out msg);
        }

        /// <summary>
        /// 写长字符串
        /// 一个寄存器地址可以存放两个字节【两个字符】，设定每次最多写入200个字符，
        /// 分多次写入，每一次写入的起始寄存器地址偏移100
        /// </summary>
        /// <param name="startAddress">起始地址</param>
        /// <param name="longString">长字符串</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int WriteLongString(int startAddress, string longString, out string msg)
        {
            msg = string.Empty;
            if (longString == null)
            {
                longString = string.Empty;
            }
            int cycleCount = 200;//每20个字符就进行分段
            int maxLength = longString.Length;
            int pageSize = (maxLength + cycleCount - 1) / cycleCount;
            int errorCode = -1;
            for (int i = 0; i < pageSize; i++)
            {
                int writeLength = cycleCount;
                if (i == pageSize - 1)
                {
                    //最后一次
                    writeLength = maxLength - i * cycleCount;
                }
                //分段字符串，每次最多200个
                string segment = longString.Substring(i * cycleCount, writeLength);
                //寄存器地址是字，所以需要 字节个数 除以2
                errorCode = WriteString(startAddress + (i * cycleCount / 2), writeLength, segment, out msg);
                if (errorCode != 0)
                {
                    return errorCode;
                }
            }
            return errorCode;
        }

        /// <summary>
        /// 读取基本数据类型
        /// </summary>
        /// <typeparam name="T">基本的数据类型，如short，int，double等</typeparam>
        /// <param name="startAddress"></param>
        /// <param name="value"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int ReadValue<T>(int startAddress, out T value, out string msg) where T : struct
        {
            value = default(T);
            int length = 0;//读取的连续字节个数
            if (typeof(T) == typeof(bool) || typeof(T) == typeof(sbyte) || typeof(T) == typeof(byte))
            {
                length = 1;
            }
            else if (typeof(T) == typeof(short) || typeof(T) == typeof(ushort))
            {
                length = 2;
            }
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(float))
            {
                length = 4;
            }
            else if (typeof(T) == typeof(long) || typeof(T) == typeof(ulong) || typeof(T) == typeof(double))
            {
                length = 8;
            }
            else
            {
                //暂不考虑 char(就是ushort，两个字节)，decimal（十六个字节） 等类型
                msg = $"读Modbus数据暂不支持其他类型：{value.GetType()}";
                return -1;
            }
            byte[] rcvBuffer = new byte[0];
            int errorCode = SendByte(startAddress, true, length, ref rcvBuffer, out msg);
            if (errorCode != 0)
            {
                return errorCode;
            }
            if (typeof(T) == typeof(bool))
            {
                value = (T)(object)Convert.ToBoolean(rcvBuffer[0]);
            }
            else if (typeof(T) == typeof(sbyte))
            {
                value = (T)(object)(sbyte)rcvBuffer[0];
            }
            else if (typeof(T) == typeof(byte))
            {
                value = (T)(object)rcvBuffer[0];
            }
            else if (typeof(T) == typeof(short))
            {
                value = (T)(object)BitConverter.ToInt16(rcvBuffer, 0);
            }
            else if (typeof(T) == typeof(ushort))
            {
                value = (T)(object)BitConverter.ToUInt16(rcvBuffer, 0);
            }
            else if (typeof(T) == typeof(int))
            {
                value = (T)(object)BitConverter.ToInt32(rcvBuffer, 0);
            }
            else if (typeof(T) == typeof(uint))
            {
                value = (T)(object)BitConverter.ToUInt32(rcvBuffer, 0);
            }
            else if (typeof(T) == typeof(long))
            {
                value = (T)(object)BitConverter.ToInt64(rcvBuffer, 0);
            }
            else if (typeof(T) == typeof(ulong))
            {
                value = (T)(object)BitConverter.ToUInt64(rcvBuffer, 0);
            }
            else if (typeof(T) == typeof(float))
            {
                value = (T)(object)BitConverter.ToSingle(rcvBuffer, 0);
            }
            else if (typeof(T) == typeof(double))
            {
                value = (T)(object)BitConverter.ToDouble(rcvBuffer, 0);
            }
            return 0;
        }

        /// <summary>
        /// 从起始地址开始，读取一段字节流
        /// </summary>
        /// <param name="startAddress">起始寄存器地址</param>
        /// <param name="length">读取的字节个数,最多250个字节，最多125个寄存器</param>
        /// <param name="values">返回的字节流数据</param>
        /// <returns>true：读取成功 false：读取失败</returns>
        public int ReadValue(int startAddress, int length, out byte[] values, out string msg)
        {
            values = new byte[length];
            return SendByte(startAddress, true, length, ref values, out msg);
        }

        /// <summary>
        /// 读取（长）字符串，设定每次最多读取200个字符，
        /// 分多次读取，每一次读取的起始寄存器地址偏移100
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <param name="longString"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int ReadLongString(int startAddress, int length, out string longString, out string msg)
        {
            msg = string.Empty;
            longString = string.Empty;
            int cycleCount = 200;
            int pageSize = (length + cycleCount - 1) / cycleCount;
            int errorCode = -1;
            for (int i = 0; i < pageSize; i++)
            {
                int readLength = cycleCount;
                if (i == pageSize - 1)
                {
                    //最后一次
                    readLength = length - i * cycleCount;
                }
                byte[] values;
                errorCode = ReadValue(startAddress + (i * cycleCount / 2), readLength, out values, out msg);
                if (errorCode != 0)
                {
                    return errorCode;
                }
                longString += Encoding.ASCII.GetString(values);
            }
            return errorCode;
        }

        /// <summary>
        /// 用于排他锁,确保在多线程调用该接口时，不会同时调用。确保在处理当前命令时，其他命令请等待
        /// </summary>
        static int lockedValue = 0;

        #region 关键的Modbus报文处理
        /// <summary>
        /// 报文处理：发送命令并等待响应结果
        /// 确保不能同时读写同一个地址，需要加一个锁对象
        /// </summary>
        /// <param name="startAddress">要读写的汇川PLC的内存寄存器地址，只考虑M区的MW起始寄存器地址，如果是MB请除以2后传入；如果是MD请乘以2后传入</param>
        /// <param name="isRead">读(true)还是写(false)</param>
        /// <param name="length">读取的字节个数,最多250个字节，最多125个寄存器。如果是写入，该参数无意义，直接按datas.Length处理</param>
        /// <param name="rev">返回的字节流数据,直接是用户所需的字节流数据。写寄存器时，此参数无意义</param>
        /// <param name="msg">异常信息描述</param>
        /// <param name="datas">要写入的字节流数据，读取时【isRead=true】忽略该参数</param>
        /// <returns>返回错误号，0为处理成功</returns>
        private int SendByte(int startAddress, bool isRead, int length, ref byte[] rev, out string msg, byte[] datas = null)
        {
            msg = string.Empty;
            if (!IsConnected)
            {
                msg = $"【未建立连接】尚未连接汇川PLC成功【{ip}:{port}】,请检查配置和网络，当前起始地址【{startAddress}】";
                return 1000;
            }
            if (startAddress < 0 || startAddress > 65535)
            {
                msg = $"【参数非法】汇川PLC的Modbus协议的起始地址必须在0~65535之间,当前起始地址【{startAddress}】";
                return 1001;
            }
            //读保持寄存器0x03读取的寄存器数量的范围为 1~125。因一个寄存器【一个Word】存放两个字节，因此 字节数组的长度范围 为 1~250
            if (isRead && (length < 1 || length > 250))
            {
                msg = $"【参数非法】读取的字节数组的长度范围为1~250，当前起始地址【{startAddress}】，读取长度【{length}】";
                return 1002;
            }
            if (!isRead && (datas == null || datas.Length < 1 || datas.Length > 240))
            {
                msg = $"【参数非法】写入的字节数组的不能为空，也不能写入超过120个寄存器【240个字节】，当前起始地址【{startAddress}】";
                return 1003;
            }
            //添加锁
            while (Interlocked.Exchange(ref lockedValue, 1) != 0)
            {
                //此循环用于等待当前捕获current的线程执行结束
                //Thread.Sleep(50);
            }
            byte[] addrArray = BitConverter.GetBytes((ushort)startAddress);
            byte[] SendByte = new byte[12];
            //读取的寄存器个数： 如果length为偶数 则为 length/2 如果length为奇数，则为(length+1)/2。因整数相除，结果不考虑余数，所以如下通用：
            byte registerCount = (byte)((length + 1) / 2);
            if (isRead)
            {
                /*
                 * 发送的请求【读多个保持寄存器 0x03】Modbus字节流说明：
            * byte[0] byte[1] 随便指定，PLC返回的前两个字节完全一致
            * byte[2]=0 byte[3]=0 固定为0 代表Modbus标识
            * byte[4] byte[5] 排在byte[5]后面所有字节的个数，也就是总长度6
            * byte[6] 站号(从站标识)，随便指定，00--FF都可以,PLC返回的保持一致
            * byte[7] 功能码，读保持寄存器 0x03：
            * byte[8] byte[9] 起始地址，如起始地址为整数20 则为 0x00 0x14，再如起始地址为整数1000，则为 0x03 0xE8
            * byte[10] byte[11] 寄存器个数【Word】，读取的数据长度【以字Word为单位】，读取Int32或Float就是两个字，读取byte或short就是一个字 范围【1~125 即 0x0001~0x007D】
                */
                SendByte = new byte[12] { 0x02, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, addrArray[1], addrArray[0], 0x00, registerCount };
            }
            else
            {
                //生成写入寄存器命令
                SendByte = GenerateWriteCommand(addrArray, datas);
            }
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                msock.Send(SendByte, SocketFlags.None);
            }
            catch (SocketException ex)
            {
                IsConnected = false;
                msg = $"【网络异常】发送命令失败，当前起始地址【{startAddress}】,套接字错误【{ex.SocketErrorCode}】,【{ex.Message}】";
                //释放锁
                Interlocked.Exchange(ref lockedValue, 0);//将current重置为0
                return 1004;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                msg = $"【处理异常】发送命令失败，当前起始地址【{startAddress}】【{ex.Message}】";
                //释放锁
                Interlocked.Exchange(ref lockedValue, 0);//将current重置为0
                return 1005;
            }

            byte[] buffer = new byte[2048];
            int rcvCount = 0;//收到的字节数
            try
            {
                rcvCount = msock.Receive(buffer);
                RecordDataEvent?.Invoke(SendByte, new ArraySegment<byte>(buffer, 0, rcvCount).ToArray(), stopwatch.Elapsed.TotalMilliseconds);
            }
            catch (SocketException ex)
            {
                IsConnected = false;
                msg = $"【网络异常】接收数据失败，当前起始地址【{startAddress}】,套接字错误【{ex.SocketErrorCode}】,【{ex.Message}】";
                //释放锁
                Interlocked.Exchange(ref lockedValue, 0);//将current重置为0
                return 1006;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                msg = $"【处理异常】接收数据失败，当前起始地址【{startAddress}】【{ex.Message}】";
                //释放锁
                Interlocked.Exchange(ref lockedValue, 0);//将current重置为0
                return 1007;
            }
            if (rcvCount < 9 || buffer[0] != SendByte[0] || buffer[1] != SendByte[1] || buffer[2] != SendByte[2] || buffer[3] != SendByte[3] || buffer[7] != SendByte[7])
            {
                msg = $"【数据非法】接收数据非法或者处理失败，当前起始地址【{startAddress}】接收的数据【{string.Join(",", new ArraySegment<byte>(buffer, 0, rcvCount))}】";
                Interlocked.Exchange(ref lockedValue, 0);//将current重置为0
                return 1008;
            }
            if (isRead)
            {
                /*
                 * * 接收的内容解析：【读多个保持寄存器 0x03】【Modbus响应】
            * byte[0] byte[1] 与发送的一致
            * byte[2]=0 byte[3]=0 固定为0 代表Modbus标识
            * byte[4] byte[5] 排在byte[5]后面所有字节的个数
            * byte[6] 站号，与发送的一致
            * byte[7] 功能码，与发送的一致
            * byte[8] 表示byte[8]后面跟随的字节数【发送的寄存器个数 * 2】
            * byte[9] byte[10] byte[11] byte[12] byte[...] 真实数据的字节流，字节流的总个数就是byte[8]
                */
                int receiveLength = buffer[8];
                if (receiveLength != registerCount * 2)
                {
                    //接收到的实际数据字节个数:buffer[8]
                    msg = $"【数据非法】解析接收数据非法，读取后接收的实际数据长度【{receiveLength}】不是读取寄存器数量【{registerCount}】的2倍.当前起始地址【{startAddress}】接收的数据【{string.Join(",", new ArraySegment<byte>(buffer, 0, rcvCount))}】";
                    Interlocked.Exchange(ref lockedValue, 0);//将current重置为0
                    return 1009;
                }
                rev = new byte[receiveLength];
                //相邻两个数据交换 【索引0和索引1交换，索引2与索引3交换，...】
                for (int i = 0; i < receiveLength; i++)
                {
                    if (i % 2 == 0)
                    {
                        rev[i] = buffer[9 + i + 1];
                    }
                    else
                    {
                        rev[i] = buffer[9 + i - 1];
                    }
                }
            }
            else
            {
                if (buffer[7] == 0x10 && rcvCount > 11 && buffer[11] != SendByte[11])
                {
                    //如果是写多个寄存器 并且 写入的寄存器数量不匹配
                    msg = $"【数据非法】解析接收数据非法，写多个寄存器时，返回的写入寄存器数量【{buffer[11]}】与请求的寄存器数量【{SendByte[11]}】不一致，地址【{startAddress}】接收的数据【{string.Join(",", new ArraySegment<byte>(buffer, 0, rcvCount))}】";
                    Interlocked.Exchange(ref lockedValue, 0);//将current重置为0
                    return 1010;
                }
            }
            //释放锁
            Interlocked.Exchange(ref lockedValue, 0);//将current重置为0
            return 0;
        }
        #endregion

        /// <summary>
        /// 生成写入寄存器命令
        /// </summary>
        /// <param name="addrArray">起始地址字节数组，其实就是ushort地址转化后的两个字节</param>
        /// <param name="datas">要写入的字节数组</param>
        /// <returns></returns>
        private byte[] GenerateWriteCommand(byte[] addrArray, byte[] datas)
        {
            byte[] SendByte = new byte[12];
            //如果是写PLC寄存器地址
            byte registerCount = (byte)((datas.Length + 1) / 2);
            //实际写入的字节个数：注意buffer数组的长度为奇数时 需要将最后一个寄存器的高位设置为0
            byte writeByteCount = (byte)(registerCount * 2);
            if (registerCount == 1)
            {
                //如果只写入一个寄存器，可以使用0x06命令【写单个保持寄存器】，用于sbyte,byte,short,ushort                    
                SendByte = new byte[12] { 0x02, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x06, addrArray[1], addrArray[0], 0x00, 0x00 };
                if (datas.Length == 1)
                {
                    SendByte[11] = datas[0];
                }
                else //写入的字节数组长度为2
                {
                    SendByte[10] = datas[1];
                    SendByte[11] = datas[0];
                }
                return SendByte;
            }
            //如果2个或多个寄存器，使用命令0x10【写多个保持寄存器】，用于int,uint,float,long,ulong,double等
            //实际写入的字节个数：注意buffer数组的长度为奇数时 需要将最后一个寄存器的高位设置为0
            /*
             * 发送的请求【写多个保持寄存器 0x10】Modbus字节流说明：【写Int32，UInt32，Float需要两个寄存器 写Int64，UInt64，Double需要四个寄存器】
             * byte[0] byte[1] 随便指定，PLC返回的前两个字节完全一致
             * byte[2]=0 byte[3]=0 固定为0 代表Modbus标识 随便指定也可以
             * byte[4] byte[5] 排在byte[5]后面所有字节的个数
             * byte[6] 站号，随便指定，00--FF都可以,PLC返回的保持一致
             * byte[7] 功能码，0x10：写多个保持寄存器
             * byte[8] byte[9] 起始地址，如起始地址为整数20 则为 0x00 0x14，再如起始地址为整数1000，则为 0x03 0xE8
             * byte[10] byte[11] 寄存器数量【设置长度】，范围1~120【0x78】，因此byte[10]=0, byte[11]为寄存器数量
             * byte[12] 字节个数 也就是【寄存器数量*2】，范围【2~240】
             * byte[13] byte[14] byte[15] byte[16] byte[...]  具体的数据内容 对应 数据一高位 数据一低位 数据二高位 数据二低位
            */
            SendByte = new byte[13 + writeByteCount];
            SendByte[0] = 0x02;
            SendByte[1] = 0x01;
            SendByte[5] = (byte)(7 + writeByteCount);
            SendByte[6] = 0x01;
            SendByte[7] = 0x10;//写多个寄存器标记：0x10
            SendByte[8] = addrArray[1];
            SendByte[9] = addrArray[0];
            SendByte[11] = registerCount;
            SendByte[12] = writeByteCount;
            //交换相邻两个字节 【索引0和索引1交换，索引2与索引3交换，...需考虑datas.Length是奇数时的特殊处理】
            for (int i = 0; i < writeByteCount; i++)
            {
                if (i % 2 == 0)
                {
                    if (i + 1 == datas.Length)
                    {
                        //如果是写入奇数个字节，需要将最后一个寄存器的高位设置为0
                        SendByte[13 + i] = 0;
                    }
                    else
                    {
                        SendByte[13 + i] = datas[i + 1];
                    }
                }
                else
                {
                    SendByte[13 + i] = datas[i - 1];
                }
            }
            return SendByte;
        }
    }

    /// <summary>
    /// 字节存储类型，C#中字节存放顺序是DCBA【低字节在前】
    /// 汇川PLC默认存储是CDAB
    /// </summary>
    public enum StoreByteCategory
    {
        /// <summary>
        /// 顺序，高字节在前,低字节在后
        /// </summary>
        ABCD = 0,
        /// <summary>
        /// 字正序，字节颠倒
        /// </summary>
        BADC = 1,
        /// <summary>
        /// 字颠倒，字节正序
        /// </summary>
        CDAB = 2,
        /// <summary>
        /// 逆序，低字节在前,高字节在后
        /// </summary>
        DCBA = 3
    }
}