using Inovance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTcp_Inovance
{
    public class Station1 : StationEx
    {
        public override void NormalRun()
        {
            if (WaitPLC())
            {
                Form1.Instance.txtData.Invoke(new Action(() =>
                {
                    Form1.Instance.txtData.AppendText("Station 1 Run" + "\r\n");
                    if (Form1.Instance.txtData.Text.Length >= 5000) Form1.Instance.txtData.Clear();
                }));
            }
        }
        public bool WaitPLC()
        {
            int flag = 0;
            if (ReadSingleReg(1000, out flag))
            {
                if (flag == 1)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }

        }
        public bool ReadSingleReg(int nAddr, out int bStatus)
        {
            bool ret = false;
            bStatus = Form1.Instance.plc.ReadSingleValue<int>(DataType.REGI_EASY_D,nAddr);
            if (bStatus != default) 
            {
                ret = true;
            }
            return ret;
        }

    }
}
