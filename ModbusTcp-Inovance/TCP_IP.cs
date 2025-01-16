using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace ModbusTcp_Inovance
{
    public class TCP_IP
    {

        private event BarcodeReceivedHandler barcodeReceived;

        // Định nghĩa delegate và sự kiện
        public delegate void BarcodeReceivedHandler(string barcode);

        public event BarcodeReceivedHandler BarcodeReceived
        {
            add
            {
                lock (this)
                {
                    barcodeReceived += value;
                }
            }
            remove
            {
                lock (this)
                {
                    barcodeReceived -= value;
                }
            }
        }

        private Socket BarcodeScan;
        public IPEndPoint endPoint
        {
            get;set;
        }
        public int TimeOut
        {
            get ; set;
        }
        private bool AcceptRead = false;
        public TCP_IP(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
            BarcodeScan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TimeOut = 3000;

        }
        public void Open()
        {
            try
            {
                BarcodeScan.Connect(endPoint);
            }
            catch
            {
                throw;
                //this.Close();
            }
        }
        public void Close()
        {
            try
            {
                BarcodeScan.Close();
            }
            catch
            {
                throw;
                //this.Close();
            }
        }
        Stopwatch readTimeOut = new Stopwatch();
        public void Listen()
        {
            Thread.Sleep(1000);
            byte[] arrMsgRec = new byte[1024 * 1024 * 2];
            int length = -1;
            while (Connected)
            {
                try
                {
                    if (AcceptRead)
                    {
                        length = BarcodeScan.Receive(arrMsgRec);
                        if (length > 0 && length < 30)
                        {
                            string strMsg = Encoding.ASCII.GetString(arrMsgRec, 0, length);
                            int EndIndex = strMsg.IndexOf("\r\n");
                            string temp = EndIndex > -1 ? strMsg.Substring(0, EndIndex) : strMsg;
                            OnBarcodeReceived(temp);
                            AcceptRead = false;
                            readTimeOut.Stop();
                        }
                        else if(readTimeOut.ElapsedMilliseconds >= TimeOut)
                        {
                            OnBarcodeReceived("NR");
                            AcceptRead = false;
                            readTimeOut.Stop();

                        }
                    }
                }
                catch (SocketException se)
                {
                    if (!BarcodeScan.Connected) BarcodeScan.Connect(endPoint);

                }
                catch (Exception ex)
                {
                    if (!BarcodeScan.Connected) BarcodeScan.Connect(endPoint);
                }
                Thread.Sleep(10);
            }
            if (!BarcodeScan.Connected)
            {
                BarcodeScan.Close();
            }
        }
        public bool SendMess(string msg)
        {
            if (Connected) 
            {
                AcceptRead = true;
                readTimeOut.Reset();
                readTimeOut.Start();
                byte[] buffer = Encoding.ASCII.GetBytes(msg);
                return BarcodeScan.Send(buffer) >0;
            }
            else
            {
                return false;
            }
        }
        public bool Connected
        {
            get { return BarcodeScan.Connected;}
        }
        // Phương thức kích hoạt sự kiện
        protected virtual void OnBarcodeReceived(string barcode)
        {
            barcodeReceived?.Invoke(barcode);
        }
    }
    #region Client
    public class BarcodeListener
    {
        private event BarcodeReceivedHandler barcodeReceived;

        // Định nghĩa delegate và sự kiện
        public delegate void BarcodeReceivedHandler(string barcode);

        public event BarcodeReceivedHandler BarcodeReceived
        {
            add
            {
                lock (this)
                {
                    barcodeReceived += value;
                }
            }
            remove
            {
                lock (this)
                {
                    barcodeReceived -= value;
                }
            }
        }

        private TcpClient tcpClient;
        private NetworkStream networkStream;

        public IPEndPoint endPoint { get; set; }

        public BarcodeListener(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
            tcpClient = new TcpClient();
        }

        public void Open()
        {
            try
            {
                tcpClient.Connect(endPoint);
                networkStream = tcpClient.GetStream();
            }
            catch (SocketException se)
            {
                // Handle exception
                return;
            }
        }

        public void Close()
        {
            try
            {
                networkStream?.Close();
                tcpClient?.Close();
            }
            catch (SocketException se)
            {
                // Handle exception
                return;
            }
        }

        public void Listen()
        {
            Thread.Sleep(1000);
            byte[] arrMsgRec = new byte[1024 * 1024 * 2];
            int length = -1;

            while (Connected)
            {
                try
                {
                    length = networkStream.Read(arrMsgRec, 0, arrMsgRec.Length);
                    if (length > 0 && length < 30)
                    {
                        string strMsg = Encoding.ASCII.GetString(arrMsgRec, 0, length);
                        int EndIndex = strMsg.IndexOf("\r\n");
                        string temp = EndIndex > -1 ? strMsg.Substring(0, EndIndex) : strMsg;
                        OnBarcodeReceived(temp);
                    }
                }
                catch (SocketException se)
                {
                    Close();
                }
                catch (Exception ex)
                {
                    Close();
                }
                Thread.Sleep(200);
            }
            if (!tcpClient.Connected)
            {
                Close();
            }
        }

        public void SendMess(string msg)
        {
            if (Connected)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(msg);
                networkStream.Write(buffer, 0, buffer.Length);
            }
        }

        public bool Connected
        {
            get { return tcpClient.Connected; }
        }

        // Phương thức kích hoạt sự kiện
        protected virtual void OnBarcodeReceived(string barcode)
        {
            barcodeReceived?.Invoke(barcode);
        }
    }
    #endregion
}
