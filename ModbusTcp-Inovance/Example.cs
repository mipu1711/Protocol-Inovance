using Inovance;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModbusTcp_Inovance
{
    public partial class Example : Form
    {
        public Example()
        {
            InitializeComponent();
            plc = new PLC(PlcType.EASY, txtIP.Text.Trim());

        }
        public PLC plc;


        private void Example_Load(object sender, EventArgs e)
        {
            cmbElemType.SelectedIndex = 5;
            cbType.SelectedIndex = 2;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!plc.IsConnected)
            {
                plc = new PLC(PlcType.EASY, txtIP.Text.Trim());
                plc.Connect();

                if (plc.IsConnected)
                {
                    btnConnect.Text = "DisConnect";
                    btnConnect.BackColor = Color.Green;
                    txtIP.Enabled = false;
                    thread = new Thread(readIO);
                    thread.IsBackground = true;
                    thread.Start();
                }
            }
            else
            {
                plc.Close();
                btnConnect.Text = "Connect";
                btnConnect.BackColor = Color.Gainsboro;
                txtIP.Enabled = true;
            }
        }
        Thread thread;

        private void readIO()
        {
            while (plc.IsConnected)
            {
                if (plc.IsConnected)
                {
                    try
                    {
                        int i = 0;
                        byte[] data = plc.ReadBytes(DataType.REGI_EASY_X, 0, 8);
                        byte[] data1 = plc.ReadBytes(DataType.REGI_EASY_Y, 0, 8);

                        bool[] X_val = plc.ToValue<bool>(data, 0, 8);
                        bool[] Y_val = plc.ToValue<bool>(data1, 0, 8);
                        if (X_val != null)
                        {
                            foreach (var item in X_val)
                            {
                                if (i < 8)
                                {
                                    if (item)
                                    {
                                        groupBox1.Invoke((MethodInvoker)delegate
                                        {

                                            groupBox1.Controls.Find("X" + i.ToString(), true)[0].BackColor = Color.Green;
                                        });
                                        //this.Controls.Find("X" + i.ToString(), true)[0].BackColor = Color.Green;
                                    }
                                    else
                                    {
                                        groupBox1.Invoke((MethodInvoker)delegate
                                        {
                                            groupBox1.Controls.Find("X" + i.ToString(), true)[0].BackColor = Color.DarkGray;
                                        });
                                        //this.Controls.Find("X" + i.ToString(), true)[0].BackColor = Color.DarkGray;
                                    }
                                }
                                i++;
                            }
                        }
                        if (Y_val != null)
                        {
                            i = 0;
                            foreach (var item in Y_val)
                            {
                                if (i < 8)
                                {
                                    if (item)
                                    {
                                        groupBox1.Invoke((MethodInvoker)delegate
                                        {
                                            groupBox1.Controls.Find("Y" + i.ToString(), true)[0].BackColor = Color.Green;
                                        });
                                        //this.Controls.Find("Y" + i.ToString(), true)[0].BackColor = Color.Green;
                                    }
                                    else
                                    {
                                        groupBox1.Invoke((MethodInvoker)delegate
                                        {
                                            groupBox1.Controls.Find("Y" + i.ToString(), true)[0].BackColor = Color.DarkGray;
                                        });
                                        //this.Controls.Find("Y" + i.ToString(), true)[0].BackColor = Color.DarkGray;
                                    }
                                }
                                i++;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("PLC is not connected");
                }
                Thread.Sleep(5);
            }
        }

        private void btn_read_Click(object sender, EventArgs e)
        {
            txtData.Clear();
            if (!plc.IsConnected) return;
            DataType ElemType = DataType.REGI_H5U_Y;
            if (cmbElemType.Text == "Y")
            {
                ElemType = DataType.REGI_H5U_Y;
            }
            else if (cmbElemType.Text == "X")
            {
                ElemType = DataType.REGI_H5U_X;
            }
            else if (cmbElemType.Text == "S")
            {
                ElemType = DataType.REGI_H5U_S;
            }
            else if (cmbElemType.Text == "M")
            {
                ElemType = DataType.REGI_H5U_M;
            }
            else if (cmbElemType.Text == "B")
            {
                ElemType = DataType.REGI_H5U_B;
            }
            else if (cmbElemType.Text == "D")
            {
                if (int.Parse(txtCount.Text) < 2)
                {
                    txtCount.Text = "2";
                }
                ElemType = DataType.REGI_H5U_D;//REGI_H3U_DW,REGI_H5U_D
            }
            else if (cmbElemType.Text == "R")
            {
                if (int.Parse(txtCount.Text) < 2)
                {
                    txtCount.Text = "2";
                }
                ElemType = DataType.REGI_H5U_R;
            }
            byte[] result = plc.ReadBytes(ElemType, int.Parse(txtStartAddr.Text), int.Parse(txtCount.Text));
            Type type = GetTypeFromString(cbType.Text);
            object[] val = plc.ToValue(result, 0, int.Parse(txtCount.Text), type);
            for (int i = 0; i < val.Length; i++)
            {
                txtData.AppendText($"{val[i]} ");
            }
        }



        public static Type GetTypeFromString(string typeName)
        {
            switch (typeName.ToLower())
            {
                case "bool":
                case "boolean":
                    return typeof(bool);
                case "byte":
                    return typeof(byte);
                case "sbyte":
                    return typeof(sbyte);
                case "char":
                    return typeof(char);
                case "decimal":
                    return typeof(decimal);
                case "double":
                    return typeof(double);
                case "float":
                case "single":
                    return typeof(float);
                case "int":
                case "int32":
                    return typeof(int);
                case "uint":
                case "uint32":
                    return typeof(uint);
                case "long":
                case "int64":
                    return typeof(long);
                case "ulong":
                case "uint64":
                    return typeof(ulong);
                case "short":
                case "int16":
                    return typeof(short);
                case "ushort":
                case "uint16":
                    return typeof(ushort);
                case "string":
                    return typeof(string);
                // Add more cases as needed
                default:
                    throw new ArgumentException("Unsupported type");
            }
        }

        private void btn_write_Click(object sender, EventArgs e)
        {
            DataType ElemType = DataType.REGI_H5U_Y;
            if (cmbElemType.Text == "Y")
            {
                ElemType = DataType.REGI_H5U_Y;
            }
            else if (cmbElemType.Text == "X")
            {
                ElemType = DataType.REGI_H5U_X;
            }
            else if (cmbElemType.Text == "S")
            {
                ElemType = DataType.REGI_H5U_S;
            }
            else if (cmbElemType.Text == "M")
            {
                ElemType = DataType.REGI_H5U_M;
            }
            else if (cmbElemType.Text == "B")
            {
                ElemType = DataType.REGI_H5U_B;
            }
            else if (cmbElemType.Text == "D")
            {
                if (int.Parse(txtCount.Text) < 2)
                {
                    txtCount.Text = "2";
                }
                ElemType = DataType.REGI_H5U_D;//REGI_H3U_DW,REGI_H5U_D
            }
            else if (cmbElemType.Text == "R")
            {
                if (int.Parse(txtCount.Text) < 2)
                {
                    txtCount.Text = "2";
                }
                ElemType = DataType.REGI_H5U_R;
            }
            Type type = GetTypeFromString(cbType.Text);
            if (type == typeof(bool) || type == typeof(Boolean))
            {
                int val__ = 0;
                int.TryParse(txtValue.Text.Trim(), out val__);
                object val = val__ > 0 ? true : false;
                byte[] val_ = plc.ToByteValue(val, type);
                plc.WriteBytes(ElemType, int.Parse(txtStartAddr.Text), val_.Length, val_);
            }
            else if (type == typeof(string))
            {
                byte[] val_ = Encoding.ASCII.GetBytes(txtValue.Text.Trim());
                plc.WriteBytes(ElemType, int.Parse(txtStartAddr.Text), val_.Length, val_);
            }
            else
            {
                object val = Convert.ChangeType(txtValue.Text.Trim(), type);
                byte[] val_ = plc.ToByteValue(val, type);
                plc.WriteBytes(ElemType, int.Parse(txtStartAddr.Text), val_.Length, val_);
            }

        }

        private void x7_Click(object sender, EventArgs e)
        {
            Label label = sender as Label;
            Match match = Regex.Match(label.Name, @"\d+");
            short index;
            if (match.Success && plc.IsConnected)
            {
                index = short.Parse(match.Value);
                bool val = label.BackColor == Color.Green ? false : true;
                DataType dataType = label.Text.Contains("X")? DataType.REGI_EASY_X:DataType.REGI_EASY_Y;
                bool rs = plc.WriteSingleValue<bool>(dataType, int.Parse(match.Value),val);
                if (rs)
                {
                    label.BackColor = val ? Color.Green : Color.DarkGray;
                }
            }

        }
    }
}
