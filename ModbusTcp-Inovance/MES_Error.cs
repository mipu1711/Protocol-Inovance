//using ModbusTcp_Inovance;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Security.Policy;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BoTech
//{
//    public class MES_Error
//    {
//        public static MES_Error mes_Error = new MES_Error(Machine_State.idle);

//        //Machine_State old_state;

//        int state;

//        DateTime downTime;

//        public MES_Error(Machine_State state)
//        {

//            this.state = (int)Machine_State.running;



//            StateAndSend = (int)Machine_State.idle;


//        }
//        public int StateAndSend
//        {
//            get
//            {
//                return state;
//            }
//            set
//            {
//                //if (!mFunction.mParList[(int)ParName.ChkPar.开启MES].CheckSts)
//                //{
//                //    return;
//                //}
//                lock (this)
//                {
//                    if (state == value)
//                    {
//                        return;
//                    }
//                    if (state == (int)Machine_State.down && value == (int)Machine_State.idle)
//                    {
//                        return;
//                    }
//                    if (state == (int)Machine_State.planned_downtime && value == (int)Machine_State.down)
//                    {
//                        return;
//                    }
//                    //if (state == (int)Machine_State.down)
//                    //{
//                    //    Send();
//                    //}
//                    if (value == (int)Machine_State.down)
//                    {
//                        downTime = DateTime.Now;

//                        Send();

//                    }

//                    state = value;
//                }
//            }
//        }
//        public string Send()
//        {
//            string url = @"https://httpbin.org/post";
//            JObject dataClass = new JObject();
//            dataClass.Add("empNo", "AUTOH37");
//            dataClass.Add("terminalName", "Hung_DZ");
//            dataClass.Add("occurrence_time", $"{downTime.ToString("yyyy-MM-ddTHH:mm:ss.ff")}");


//            string json = dataClass.ToString();
//            //string jsonData = JsonConvert.SerializeObject(dataClass, Formatting.Indented);//设置json显示格式

//            string mes = string.Empty;
//            WriteLog("PC-->MES SendError " + " [" + url + "] " + json.Replace("\\r", "").Replace("\\n", ""));
//            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

//            try
//            {
//                request.Timeout = 2000;
//                request.Method = "POST";
//                request.ContentType = "application/json";
//                request.Accept = "text/plain";
//                //request.Accept = "application/json";
//                //request.ContentType = "application/x-www-form-urlencoded";


//                //request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
//                Stream myRequestStream = request.GetRequestStream();
//                //StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
//                StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("utf-8"));
//                myStreamWriter.Write(json);
//                myStreamWriter.Close();

//                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

//                Stream myResponseStream = response.GetResponseStream();
//                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
//                mes = myStreamReader.ReadToEnd();
//                myStreamReader.Close();
//                myResponseStream.Close();
//                if (request != null)
//                {
//                    request.Abort();
//                }

//            }
//            catch (Exception ex)
//            {
//                if (request != null)
//                {
//                    request.Abort();
//                }
//                mes = ex.ToString();
//            }
//            //Form1.Instance.txtData.Invoke(new Action(() => 
//            //{
//            //    Form1.Instance.txtData.AppendText(mes + "\r");
               
//            //}));
//            WriteLog("MES-->PC SendError " + " " + mes + "\r\n");
//            return mes;
//        }
//        private static void WriteLog(string log)
//        {
//            //string filePath = FilePath.mFilePath.BZ_MachineLogPath + DateTime.Now.ToString("yyyyMMdd");
//            //string filePath = FilePath.mFilePath.MES_log + DateTime.Now.ToString("yyyyMMdd");
//            //string fileName = "\\MesError_Hour" + DateTime.Now.ToString("HH") + ".txt";
//            //logServer.Instance.WriteLine(filePath, fileName, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff ") + log);
//        }
//        public enum Machine_State
//        {
//            running = 1,
//            idle,
//            engineering,
//            planned_downtime,
//            down
//        }
//    }
//}
