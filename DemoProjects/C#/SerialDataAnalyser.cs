using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Serial_Data_Analyser
{
    //PLEASE NOTE DELAY IN SERIAL PORT ONE AND TO RECIEVE FUNCTIONS!
    public partial class Form1 : Form
    {

        
       public class Data
        {
            public enum DATADIRECTION { In, Out}
            public Data()
            {
               
            }
            public Data(long time, DateTime Dt, string dev, byte[] arr , DATADIRECTION direction)
            {
                Date = Dt;

                ByteData = string.Join(",", arr);
                HexData = BitConverter.ToString(arr);
                AsciiData = Encoding.Default.GetString(arr);
                TimeSpan = time;
                Direction  = direction;
                // this.dateTime = Dt.ToString("yyyy-MM-dd hh:mm:ss.ff");
                Device = dev;

            }
            public DateTime Date { get; set; }
            public long TimeSpan { get; set; }
            public DATADIRECTION Direction { get; set; }
            //  public string DateTime { get; set; }
            public string Device { get; set; }
            public string ByteData { get; set; }
            public string HexData { get; set; }
            public string AsciiData { get; set; }

        }

        public static List<Data> ListData = new List<Data>();
        public MqttClient TMqttClient;
        public Form1()
        {
            InitializeComponent();

        }
        private void UpdateDataToSendDGVports()
        {
            string[] ar;

            if (CmbPort1Name.Text != "None" && CmbPort2Name.Text != "None")
            {
                ar = new string[] { CmbPort1Name.Text, CmbPort2Name.Text };
            }
            else
            {
                if (CmbPort1Name.Text == "None")
                {
                    ar = new string[] { CmbPort2Name.Text, "Mqtt" };
                }
                else
                {
                    ar = new string[] { CmbPort1Name.Text, "Mqtt"};
                }
            }
           ((DataGridViewComboBoxColumn)DgvDatatoSend.Columns["Ports"]).DataSource = ar;

            for (int i = 0; i < DgvDatatoSend.Rows.Count; i++)
            {
                DgvDatatoSend["Ports", i].Value = ar[0];
            }


        }
        private void SetupDataToSendDGV()
        {
            DgvDatatoSend.Columns.Add(new DataGridViewTextBoxColumn());
            DgvDatatoSend.Columns.Add(new DataGridViewTextBoxColumn());
            DgvDatatoSend.Columns.Add(new DataGridViewComboBoxColumn());
            DgvDatatoSend.Columns.Add(new DataGridViewTextBoxColumn());
            DgvDatatoSend.Columns.Add(new DataGridViewCheckBoxColumn());
            DgvDatatoSend.Columns.Add(new DataGridViewCheckBoxColumn());
            DgvDatatoSend.Columns.Add(new DataGridViewCheckBoxColumn());
            DgvDatatoSend.Columns.Add(new DataGridViewButtonColumn());

            DgvDatatoSend.Columns[0].DataPropertyName = "Description";
            DgvDatatoSend.Columns[1].DataPropertyName = "Data";
            DgvDatatoSend.Columns[2].DataPropertyName = "Ports";
            DgvDatatoSend.Columns[3].DataPropertyName = "Delay";
            DgvDatatoSend.Columns[4].DataPropertyName = "SendAll";
            DgvDatatoSend.Columns[5].DataPropertyName = "CR";
            DgvDatatoSend.Columns[6].DataPropertyName = "NL";
            DgvDatatoSend.Columns[7].DataPropertyName = "Send";

            DgvDatatoSend.Columns[0].Name = "Description";
            DgvDatatoSend.Columns[1].Name = "Data";
            DgvDatatoSend.Columns[2].Name = "Ports";
            DgvDatatoSend.Columns[3].Name = "Delay";
            DgvDatatoSend.Columns[4].Name = "SendAll";
            DgvDatatoSend.Columns[5].Name = "CR";
            DgvDatatoSend.Columns[6].Name = "NL";
            DgvDatatoSend.Columns[7].Name = "Send";

            DgvDatatoSend.Columns[0].FillWeight = 15;
            DgvDatatoSend.Columns[1].FillWeight = 35;
            DgvDatatoSend.Columns[2].FillWeight = 10;
            DgvDatatoSend.Columns[3].FillWeight = 8;
            DgvDatatoSend.Columns[4].FillWeight = 7;
            DgvDatatoSend.Columns[5].FillWeight = 4;
            DgvDatatoSend.Columns[6].FillWeight = 4;
            DgvDatatoSend.Columns[7].FillWeight = 8;


            for (int i = 0; i < 20; i++)
            {
                DgvDatatoSend.Rows.Add(i.ToString(),"", "", 1000, false, false, false, "Send");
            }


            //DgvDatatoSend.Rows.Add("AT+QMTCFG=\"SSL\",1,1,2", "", 0, true, true, true, "Send");
            //// DgvDatatoSend.Rows.Add("AT+QMTOPEN=1,\"18.198.240.106\",1883", "", 0, true,true, true, "Send");
            //DgvDatatoSend.Rows.Add("AT+QMTDISC=1", "", 0, true, true, true, "Send");
            //DgvDatatoSend.Rows.Add("AT+QMTOPEN=1,\"197.234.138.66\",8883", "", 0, true,true, true, "Send");
            //DgvDatatoSend.Rows.Add("AT+QMTCONN=1,\"123\", \"\", \"\"", "", 0, true, true, true, "Send");
            //DgvDatatoSend.Rows.Add("AT+QMTPUB=1,1,2,0,\"Home/Topic\"", "", 0, true, true, true, "Send");
            //string a = File.ReadAllText(@"C:\Users\TJSwanepoel\Desktop\ICEcert\ca.crt");
            //string b = File.ReadAllText(@"C:\Users\TJSwanepoel\Desktop\ICEcert\client.crt");
            //string c = File.ReadAllText(@"C:\Users\TJSwanepoel\Desktop\ICEcert\client.key");
            //DgvDatatoSend.Rows.Add(a, "", 0, true,true, true, "Send");
            //DgvDatatoSend.Rows.Add(b, "", 0, true,true, true, "Send");
            //DgvDatatoSend.Rows.Add(c, "", 0, true,true, true, "Send");


            UpdateDataToSendDGVports();


            // DgvDatatoSend.AutoGenerateColumns = false;
        }

        private void DoDataToSend(string data, string port, bool SendCR, bool SendNl)
        {

            string[] arr = data.Split(',');
            bool SendBytes = false;

            if (arr.Length > 0)
            {
                foreach (string s in arr)
                {
                    if (!byte.TryParse(s, out _))
                    {
                        SendBytes = false;
                        break;
                    }
                    else
                    {
                        SendBytes = true;
                    }

                }
            }


            List<byte> bytedata = new List<byte>();

            if (SendBytes)
            {
                foreach (string s in arr)
                {
                    bytedata.Add(byte.Parse(s));
                }
                if (SendCR)
                {
                    bytedata.Add(13);
                }
                if (SendNl)
                {
                    bytedata.Add(10);
                }
            }
            else
            {
              
                if (SendCR)
                {
                    data += "\r";
                }
                if (SendNl)
                {
                    data += "\n";
                }
                bytedata = Encoding.ASCII.GetBytes(data).ToList();
            }


            if (port != "")
            {
                if (serialPort2.PortName == port)
                {
                    try
                    {

                        if (serialPort2.IsOpen)
                        {
                            if (SendBytes)
                            {
                                
                                long d = 0;
                                if (ListData.Count > 0)
                                {
                                    d = (long)(DateTime.Now - ListData[ListData.Count - 1].Date).TotalMilliseconds;
                                }
                                ListData.Add(new Data(d, DateTime.Now, port, bytedata.ToArray(), Data.DATADIRECTION.Out));
                                Invoke(new EventHandler(UpdateUi));
                                serialPort2.Write(bytedata.ToArray(), 0, bytedata.Count);
                            }
                            else
                            {
                               
                                long d = 0;
                                if (ListData.Count > 0)
                                {
                                    d = (long)(DateTime.Now - ListData[ListData.Count - 1].Date).TotalMilliseconds;
                                }
                                ListData.Add(new Data(d, DateTime.Now, port, bytedata.ToArray(), Data.DATADIRECTION.Out));
                                Invoke(new EventHandler(UpdateUi));
                                serialPort2.Write(data);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Serial Port is closed!");
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Could not parse!");
                    }
                }
                else

                if (serialPort1.PortName == port)
                {
                    try
                    {

                        if (serialPort1.IsOpen)
                        {

                            if (SendBytes)
                            {
                                
                                long d = 0;
                                if (ListData.Count > 0)
                                {
                                    d = (long)(DateTime.Now - ListData[ListData.Count - 1].Date).TotalMilliseconds;
                                }
                                ListData.Add(new Data(d, DateTime.Now, port, bytedata.ToArray(), Data.DATADIRECTION.Out));
                                Invoke(new EventHandler(UpdateUi));
                                serialPort1.Write(bytedata.ToArray(), 0, bytedata.Count);
                            }
                            else
                            {
                               
                                long d = 0;
                                if (ListData.Count > 0)
                                {
                                    d = (long)(DateTime.Now - ListData[ListData.Count - 1].Date).TotalMilliseconds;
                                }

                                ListData.Add(new Data(d, DateTime.Now, port, bytedata.ToArray(), Data.DATADIRECTION.Out));
                                Invoke(new EventHandler(UpdateUi));
                                serialPort1.Write(data);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Serial Port is closed!");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not parse!" + ex.Message);
                    }


                }
                else
                if(port == "Mqtt")
                {
                    if(TMqttClient.IsConnected)
                    {
                        
                        TMqttClient.Publish(CmbPublishTopics.Text, bytedata.ToArray(), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                    }
                }
            }
        }
        private void UpdateUi(object sender, EventArgs e)
        {
            //ListData.Clear();
            // ListData.Add(new Data(200, SourceDevice.Device, new byte[] { 1, 2, 3 }));


            BindingSource Bs = new BindingSource();
            Bs.DataSource = ListData;
            //dgvData.DataSource = null;
            dgvData.DataSource = Bs;
            ///BindingSource Bs = new BindingSource();
            // Bs.DataSource = ListDataToSend;
            // DgvDatatoSend.DataSource = null;
            //DgvDatatoSend.DataSource = Bs;



        }
        private void LoadPorts()
        {

            CmbPort1Name.Items.Clear();
            CmbPort2Name.Items.Clear();
           
           
            CmbPort1Name.Items.Add("None");
            CmbPort2Name.Items.Add("None");
            CmbPort1Name.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            CmbPort2Name.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
          

          
            if (CmbPort1Name.Items.Count > 0)
            {
                CmbPort1Name.SelectedIndex = 0;
            }
            if (CmbPort1Name.Items.Count > 1)
            {
                CmbPort1Name.SelectedIndex = 1;
            }
            if (CmbPort1Name.Items.Count > 2)
            {
                CmbPort1Name.SelectedIndex = 2;
            }
           // CmbPort1Name.SelectedIndex = 5;
            //CmbPort2Name.SelectedIndex = CmbPort2Name.Items.Count - 1;
            CmbPort2Name.Text = "None";

            UpdateDataToSendDGVports();


        }


        private void Form1_Load(object sender, EventArgs e)
        {
            CmbPublishTopics.SelectedIndex = 0;
            CmbSubscribeTopics.SelectedIndex = 0;
            CmbBroker.SelectedIndex = 0;
            CmbPort1Baud.SelectedIndex = 1;
            CmbPort2Baud.SelectedIndex = 1;
            CmbPort1StopBits.SelectedIndex = 0;
            CmbPort2StopBits.SelectedIndex = 0;
            CmbPort1Parity.SelectedIndex = 0;
            CmbPort2Parity.SelectedIndex = 0;


            SetupDataToSendDGV();
            LoadPorts();
            UpdateUi(null, null);
            
            DgvDatatoSend.ClearSelection();
        
            chCol(chByte,null);
            dgvData.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd hh:mm:ss.ff";

            TxtSP1Info.Text = "Port 1: " + CmbPort1Name.Text + " | " + CmbPort1Baud.Text + " | " + CmbPort1Parity.Text + " | " + CmbPort1StopBits.Text;
            TxtSP2Info.Text = "Port 2: " + CmbPort2Name.Text + " | " + CmbPort2Baud.Text + " | " + CmbPort2Parity.Text + " | " + CmbPort2StopBits.Text;
            TxtMqttInfo.Text = "Mqtt: " + CmbBroker.Text;

            for (int i = 0; i <DgvDatatoSend.Rows.Count; i++)
            {
                DgvDatatoSend_CellValueChanged(null, new DataGridViewCellEventArgs(0, i));
            }
           

        }
        private void BtnOpenSerialPort_Click(object sender, EventArgs e)
        {
            if(ChRemote1.Checked||ChRemote2.Checked)
            {
                BtnMqttOpen_Click(null,null);
            }

            if (btnOpenSerialPort.Text == "Open")
            {

                if (CmbPort1Name.SelectedItem.ToString() != CmbPort2Name.SelectedItem.ToString())
                {
                    //try
                    //{ 
                    Invoke(new Action(() =>
                    {
                        System.IO.Ports.StopBits stopBitsP1;
                        System.IO.Ports.StopBits stopBitsP2;
                        Enum.TryParse(CmbPort1StopBits.Text, out stopBitsP1);
                        Enum.TryParse(CmbPort2StopBits.Text, out stopBitsP2);
                        System.IO.Ports.Parity parityP1;
                        System.IO.Ports.Parity parityP2;
                        Enum.TryParse(CmbPort1Parity.Text, out parityP1);
                        Enum.TryParse(CmbPort2Parity.Text, out parityP2);

                        if (serialPort1.IsOpen)
                        {
                            serialPort1.DiscardOutBuffer();
                            serialPort1.DiscardInBuffer();
                            serialPort1.Close();
                        }
                        if (serialPort2.IsOpen)
                        {
                            serialPort2.DiscardOutBuffer();
                            serialPort2.DiscardInBuffer();
                            serialPort2.Close();
                        }

                        if (CmbPort1Name.SelectedItem.ToString() != "None")
                        {
                            serialPort1.PortName = CmbPort1Name.SelectedItem.ToString();
                            serialPort1.BaudRate = int.Parse(CmbPort1Baud.SelectedItem.ToString());
                            serialPort1.ReadTimeout = 500;
                            serialPort1.WriteTimeout = 500;
                            serialPort1.StopBits = stopBitsP1;
                            serialPort1.Parity = parityP1;
                            try
                            {
                                serialPort1.Open();
                            }catch
                            {

                            }
                        }
                        else
                        {
                            serialPort1.PortName = "COMN";
                        }

                        if (CmbPort2Name.SelectedItem.ToString() != "None")
                        {
                            serialPort2.PortName = CmbPort2Name.SelectedItem.ToString();
                            serialPort2.BaudRate = int.Parse(CmbPort2Baud.SelectedItem.ToString());
                            serialPort2.ReadTimeout = 500;
                            serialPort2.WriteTimeout = 500;
                            serialPort2.StopBits = stopBitsP2;
                            serialPort2.Parity = parityP2;
                            
                            try
                            {
                                serialPort2.Open();
                            }catch
                            {

                            }
                            // serialPort2.DtrEnable = true;
                        }
                        else
                        {
                            serialPort2.PortName = "COMN";
                        }
                        //}
                        //catch (Exception ex)
                        //{
                        //    MessageBox.Show(ex.Message);
                        //}
                    }));

                }
                else
                {
                    //try
                    //{
                    Invoke(new Action(() =>
                    {
                        System.IO.Ports.StopBits stopBits;
                        Enum.TryParse(CmbPort1StopBits.Text, out stopBits);
                        System.IO.Ports.Parity parity;
                        Enum.TryParse(CmbPort1Parity.Text, out parity);
                        if (serialPort1.IsOpen)
                        {

                            serialPort1.Close();

                        }
                        if (CmbPort1Name.SelectedItem.ToString() != "None")
                        {
                            serialPort1.PortName = CmbPort1Name.SelectedItem.ToString();
                            serialPort1.BaudRate = int.Parse(CmbPort1Baud.SelectedItem.ToString());
                            serialPort1.Open();
                            serialPort1.StopBits = stopBits;
                            serialPort1.Parity = parity;
                            MessageBox.Show("Only " + serialPort1.PortName + " will be active");

                        }
                    }));
                    //}
                    //catch (Exception ex)
                    //{
                    //    MessageBox.Show(ex.Message);
                    //}
                }
                if (!serialPort1.IsOpen || !serialPort2.IsOpen)
                {
                  //  btnOpenSerialPort.Text = "Close";
                }
            }
            else
            if (btnOpenSerialPort.Text == "Close")
            {
                //try
                //{
                if (serialPort1.IsOpen)
                {
                    Invoke(new Action(() =>
                    {
                        serialPort1.DiscardOutBuffer();
                        serialPort1.DiscardInBuffer();

                        serialPort1.Close();
                        serialPort1.Dispose();
                    }));
                }
                if (serialPort2.IsOpen)
                {
                    Invoke(new Action(() =>
                    {
                        serialPort2.Close();
                    }));
                    btnOpenSerialPort.Text = "Open";
                }



                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.Message);
                //}
            }





            if(serialPort1.IsOpen)
            {
                Port1Status.ForeColor = Color.Lime;
            }
            else
            {
                Port1Status.ForeColor = Color.Red;
            }

            if (serialPort2.IsOpen)
            {
                Port2Status.ForeColor = Color.Lime;
            }
            else
            {
                Port2Status.ForeColor = Color.Red;
            }


            if (serialPort1.IsOpen || serialPort2.IsOpen)
            {
               // btnOpenSerialPort.Text = "Close";
            }
            else
            {
                btnOpenSerialPort.Text = "Open";
            }

            if (serialPort1.IsOpen)
            {
                serialPort1.DtrEnable = TSMIdtr1.Checked;
            }
            if (serialPort2.IsOpen)
            {
                serialPort2.DtrEnable = TSMIdtr2.Checked;
            }


        }
      
        private void SerialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                /*if (first == true)
                {
                    this.Invoke(new EventHandler( Label2_Click));
                    first = false;
                }*/

                if (chRxDelay.Checked)
                {
                    System.Threading.Thread.Sleep((int)nudRxDelay.Value);
                }
                // sw1.Restart();
                // sw2.Stop();
                byte[] arrRx = new byte[serialPort1.BytesToRead];
                serialPort1.Read(arrRx, 0, arrRx.Length);

                if (!ChRemote1.Checked)
                {
                    if (ChLinkToPort2.Checked)
                    {
                        if (serialPort2.IsOpen)
                        {
                            try
                            {
                                serialPort2.Write(arrRx, 0, arrRx.Length);
                                Invoke(new Action(() => { TxtErrorInfo.Text = "Error Information will be displayed here"; }));
                            }
                            catch (TimeoutException)
                            {
                                Invoke(new Action(() => { TxtErrorInfo.Text = serialPort2.PortName + ": a WRITE TIMEOUT Has occured"; }));
                            }
                            catch (Exception es)
                            {
                                MessageBox.Show(es.Message);
                            }
                        }
                    }
                }
                else
                {
                    if(TMqttClient!=null)
                    {
                        if(TMqttClient.IsConnected)
                        {
                            TMqttClient.Publish(CmbPublishTopics.Text, arrRx);
                        }
                    }
                }
                if (ListData.Count > 0)
                {
                    long d = (long)(DateTime.Now - ListData[ListData.Count - 1].Date).TotalMilliseconds;
                    ListData.Add(new Data(d, DateTime.Now, serialPort1.PortName, arrRx, Data.DATADIRECTION.In));
                }
                else
                {
                    ListData.Add(new Data(0, DateTime.Now, serialPort1.PortName, arrRx, Data.DATADIRECTION.In));
                }
                // ListData.Add(new Data(sw2.ElapsedMilliseconds,DateTime.Now, SourceDevice.PC, arrRx));
                this.Invoke(new EventHandler(UpdateUi));
            }
            catch
            { }



        }
        private void SerialPort2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {

                if (chRxDelay.Checked)
                {
                    System.Threading.Thread.Sleep((int)nudRxDelay.Value);
                }
                //sw2.Restart();
                //sw1.Stop();

                byte[] arrRx = new byte[serialPort2.BytesToRead];
                serialPort2.Read(arrRx, 0, arrRx.Length);

                if (!ChRemote2.Checked)
                {
                    if (ChLinkToPort1.Checked)
                    {
                        if (serialPort1.IsOpen)
                        {
                            try
                            {
                                serialPort1.Write(arrRx, 0, arrRx.Length);
                                Invoke(new Action(() => { TxtErrorInfo.Text = "Error Information will be displayed here"; }));
                            }
                            catch (TimeoutException)
                            {
                                Invoke(new Action(() => { TxtErrorInfo.Text = serialPort1.PortName + ": a WRITE TIMEOUT Has occured"; }));
                            }
                            catch (Exception es)
                            {
                                MessageBox.Show(es.Message);
                            }
                        }
                    }
                }
                else
                {
                    if (TMqttClient != null)
                    {
                        if (TMqttClient.IsConnected)
                        {
                            TMqttClient.Publish(CmbPublishTopics.Text, arrRx);
                        }
                    }
                }


                if (ListData.Count > 0)
                {
                    long d = (long)(DateTime.Now - ListData[ListData.Count - 1].Date).TotalMilliseconds;
                    ListData.Add(new Data(d, DateTime.Now, serialPort2.PortName, arrRx, Data.DATADIRECTION.In));
                }
                else
                {
                    ListData.Add(new Data(0, DateTime.Now, serialPort2.PortName, arrRx, Data.DATADIRECTION.In));
                }
                this.Invoke(new EventHandler(UpdateUi));
            }
            catch { }

        }
        private void BntClear_Click(object sender, EventArgs e)
        {
            ListData.Clear();
            // ListData.Add(new Data(200, SourceDevice.Device, new byte[] { 1, 2, 3 }));
            BindingSource Bs = new BindingSource();
            Bs.DataSource = ListData;
            dgvData.DataSource = Bs;
           

        }
       
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(TMqttClient!=null)
            {
                TMqttClient.Disconnect();
             
            }


            if (serialPort1.IsOpen)
            {
                Invoke(new Action(() =>
                {
                    serialPort1.Close();
                }));
            }
            if (serialPort2.IsOpen)
            {
                Invoke(new Action(() =>
                {
                    serialPort2.Close();
                }));
            }
        }
       
        void chCol(object sender, EventArgs e)
        {

            int i = 0;

            dgvData.Columns["Date"].Visible = chShowDate.Checked;
            dgvData.Columns["ByteData"].Visible = chByte.Checked;
            dgvData.Columns["HexData"].Visible = chHex.Checked;
            dgvData.Columns["AsciiData"].Visible = chAscii.Checked;
     

            if (chAscii.Checked)
            {
                i++;
            }
            if (chByte.Checked)
            {
                i++;
            }
            if (chHex.Checked)
            {
                i++;
            }


            if (i == 3)
            {
                dgvData.Columns["Date"].FillWeight = 40;
                dgvData.Columns["TimeSpan"].FillWeight = 20;
                dgvData.Columns["Direction"].FillWeight = 20;
                dgvData.Columns["Device"].FillWeight = 20;
            }
            if (i == 2)
            {
                dgvData.Columns["Date"].FillWeight = 30;
                dgvData.Columns["Direction"].FillWeight = 23;
                dgvData.Columns["TimeSpan"].FillWeight = 23;
                dgvData.Columns["Device"].FillWeight = 23;
            }
            if (i == 1)
            {
                dgvData.Columns["Date"].FillWeight = 20;
                dgvData.Columns["Direction"].FillWeight = 7;
                dgvData.Columns["TimeSpan"].FillWeight = 7;
                dgvData.Columns["Device"].FillWeight = 7;
            }
            if (i == 0)
            {
                dgvData.Columns["Date"].FillWeight = 100;
                dgvData.Columns["Direction"].FillWeight = 100;
                dgvData.Columns["TimeSpan"].FillWeight = 100;
                dgvData.Columns["Device"].FillWeight = 100;
            }
            //dgvData.Columns["Date"].FillWeight = 10;
            //dgvData.Columns["TimeSpan"].FillWeight = 5;
            //dgvData.Columns["Device"].FillWeight = 5;
           
            //dgvData.Columns["ByteData"].FillWeight = 25;
            //dgvData.Columns["HexData"].FillWeight = 25;
            //dgvData.Columns["AsciiData"].FillWeight= 25;
        }

        private void DgvDatatoSend_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            Console.WriteLine();
        }

        private void DgvDatatoSend_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == DgvDatatoSend.Columns["Send"].Index)
            {
             
                 DoDataToSend(DgvDatatoSend["Data", e.RowIndex].Value.ToString(),
                 DgvDatatoSend["Ports", e.RowIndex].Value.ToString(),
                 bool.Parse(DgvDatatoSend["CR", e.RowIndex].Value.ToString()),
                 bool.Parse(DgvDatatoSend["NL", e.RowIndex].Value.ToString()));
                
            }
         
           
        }
        private void DgvDatatoSend_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == DgvDatatoSend.Columns["Data"].Index)
            {
                try
                {
                    if (DgvDatatoSend[e.ColumnIndex, e.RowIndex].Value != null)
                    {
                        DgvDatatoSend[e.ColumnIndex, e.RowIndex].ToolTipText =
                          DgvDatatoSend[e.ColumnIndex, e.RowIndex].Value.ToString().Length.ToString() + "|" + DgvDatatoSend[e.ColumnIndex, e.RowIndex].Value.ToString().Split(',').Length.ToString() + " | " + DgvDatatoSend[e.ColumnIndex, e.RowIndex].Value.ToString();
                    }
                }
                catch { }
            }
            //if (e.ColumnIndex ==1 && e.RowIndex==0)
            //{
            //    Console.WriteLine();
            //}
        }
        private void DgvDatatoSend_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.Button == MouseButtons.Right)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    DgvDatatoSend[e.ColumnIndex, e.RowIndex].Value = File.ReadAllText(openFileDialog.FileName);
                }
            }
        }
        private void BtnSaveAllData_Click(object sender, EventArgs e)
        {
            SaveForm SF = new SaveForm();
            SF.Activate();
            SF.Show();
        }

        private void BtnUpdateSerialPorts_Click(object sender, EventArgs e)
        {
            LoadPorts();
        }
        private void TSMIdtr1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.DtrEnable = TSMIdtr1.Checked;
            }
        }
        private void TSMIdtr2_Click(object sender, EventArgs e)
        {
            if (serialPort2.IsOpen)
            {
                serialPort2.DtrEnable = TSMIdtr2.Checked;
            }

        }

        private void CmbPort1Baud_TextChanged(object sender, EventArgs e)
        {
            CmbPort2Baud.SelectedIndex = CmbPort1Baud.SelectedIndex;
        }

        private void TSMIport1_DropDownClosed(object sender, EventArgs e)
        {
           // BtnOpenSerialPort_Click(null, null);

            TxtSP1Info.Text = "Port 1: "+ CmbPort1Name.Text + " | " + CmbPort1Baud.Text + " | " + CmbPort1Parity.Text + " | " + CmbPort1StopBits.Text;
            TxtSP2Info.Text = "Port 2: "+ CmbPort2Name.Text + " | " + CmbPort2Baud.Text + " | " + CmbPort2Parity.Text + " | " + CmbPort2StopBits.Text;
            
            TxtMqttInfo.Text = "Mqtt: " + CmbBroker.Text;
           
           
            UpdateDataToSendDGVports();
        }

        private void BtnSendSerOutData_Click(object sender, EventArgs e)
        {
            for(int i=0;i<DgvDatatoSend.Rows.Count;i++)
            {
                if(DgvDatatoSend["Data", i].Value!=null)
                {
                    if (bool.Parse(DgvDatatoSend["SendAll", i].Value.ToString()))
                    {
                        DoDataToSend(DgvDatatoSend["Data", i].Value.ToString(),
                        DgvDatatoSend["Ports", i].Value.ToString(),
                        bool.Parse(DgvDatatoSend["CR", i].Value.ToString()),
                        bool.Parse(DgvDatatoSend["NL", i].Value.ToString()));

                        System.Threading.Thread.Sleep(int.Parse(DgvDatatoSend["Delay", i].Value.ToString()));
                    }
                }
                
            }
            
        }

      

        private void dgvData_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
           
                dgvData[6, e.RowIndex].ToolTipText = dgvData[6, e.RowIndex].Value.ToString().Length.ToString() + " | "+ dgvData[6, e.RowIndex].Value.ToString();
           
        }

     

        private void TSMIOpenDataToSend_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            if(openFileDialog.ShowDialog()==DialogResult.OK)
            {
                string[] arr = File.ReadAllLines(openFileDialog.FileName);
                DgvDatatoSend.Rows.Clear();
                foreach(string s in arr)
                {
                    string[] marr = s.Split(new string[] { "|Split|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (marr.Length>1 )
                    {
                        DgvDatatoSend.Rows.Add(marr[0], marr[1], "", 1000, false, false, false, "Send");
                    }
                    else
                    {
                        DgvDatatoSend.Rows.Add("", s.Replace("|Split|", ""), "", 1000, false, false, false, "Send");

                    }
                }
                UpdateDataToSendDGVports();
            }

        }

        private void DgvDatatoSend_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.F1)
            {
                if (DgvDatatoSend.SelectedRows[0].Index>0)
                {
                    
                    List<string> li = new List<string>();
                    int sindex = DgvDatatoSend.SelectedRows[0].Index - 1;
                    for (int i = 0; i < DgvDatatoSend.Rows.Count; i++)
                    {
                        li.Add(DgvDatatoSend.Rows[i].Cells[0].Value.ToString());
                    }
                    string hold = li[DgvDatatoSend.SelectedRows[0].Index];
                    li.RemoveAt(DgvDatatoSend.SelectedRows[0].Index);
                    li.Insert(sindex, hold);
                  
                    DgvDatatoSend.Rows.Clear();
                    foreach (string s in li)
                    {
                        DgvDatatoSend.Rows.Add(s, "", 1000, false, true, true, "Send");
                    }
                    DgvDatatoSend.Rows[sindex].Selected = true;
                    DgvDatatoSend.FirstDisplayedScrollingRowIndex = sindex;
                    UpdateDataToSendDGVports();
                }
                else
                {
                    Console.WriteLine();

                }

               
            }
            if (e.KeyCode == Keys.F2)
            {
                if (DgvDatatoSend.SelectedRows[0].Index < DgvDatatoSend.Rows.Count - 1)
                {

                    List<string> li = new List<string>();
                    int sindex = DgvDatatoSend.SelectedRows[0].Index + 1;
                    for (int i = 0; i < DgvDatatoSend.Rows.Count; i++)
                    {
                        li.Add(DgvDatatoSend.Rows[i].Cells[0].Value.ToString());
                    }
                    string hold = li[DgvDatatoSend.SelectedRows[0].Index];
                    li.RemoveAt(DgvDatatoSend.SelectedRows[0].Index);
                    li.Insert(sindex, hold);

                    DgvDatatoSend.Rows.Clear();
                    foreach (string s in li)
                    {
                        DgvDatatoSend.Rows.Add(s, "", 1000, false, true, true, "Send");
                    }
                  
                    DgvDatatoSend.Rows[sindex].Selected = true;
                    DgvDatatoSend.FirstDisplayedScrollingRowIndex = sindex;
                    e.Handled = true;
                    UpdateDataToSendDGVports();

                }
                else
                {
                    Console.WriteLine();

                }

            }

        }

        private void BtnSaveDataTosend_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            saveFileDialog.Filter = "|*.txt";
           

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                List<string> ls = new List<string>();
                for(int i=0;i<DgvDatatoSend.Rows.Count;i++)
                {
                    if (DgvDatatoSend["Data", i].Value.ToString() != "")
                    {
                        ls.Add(DgvDatatoSend["Description", i].Value.ToString() + "|Split|" + DgvDatatoSend["Data", i].Value.ToString());
                    }
                }

                File.WriteAllLines(saveFileDialog.FileName,ls.ToArray());
            }
        }

        private void BtnMqttOpen_Click(object sender, EventArgs e)
        {
            if (TMqttClient == null)
            {
                TMqttClient = new MqttClient(CmbBroker.Text);
                TMqttClient.MqttMsgPublishReceived += TMqttClient_MqttMsgPublishReceived;
                TMqttClient.Connect(Guid.NewGuid().ToString()+"123", true);
                TMqttClient.Subscribe(new string[] {CmbSubscribeTopics.Text }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            }
            else
            {
                TMqttClient.MqttMsgPublishReceived -= TMqttClient_MqttMsgPublishReceived;
                TMqttClient.Disconnect();
                
                TMqttClient = new MqttClient(CmbBroker.Text);
                TMqttClient.MqttMsgPublishReceived += TMqttClient_MqttMsgPublishReceived;
                TMqttClient.Connect(Guid.NewGuid().ToString() + "123", true);
                TMqttClient.Subscribe(new string[] { CmbSubscribeTopics.Text }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                
            }


            if(TMqttClient!=null)
            {
                if (TMqttClient.IsConnected)
                {
                    MqttStatus.ForeColor = Color.Lime;
                }else
                {
                    MqttStatus.ForeColor = Color.Red;
                }
            }
            else
            {
                MqttStatus.ForeColor = Color.Red;
            }
        }

        private void TMqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if(ChRemote1.Checked && serialPort1.IsOpen)
            {
                serialPort1.Write(e.Message, 0, e.Message.Length);
                if (ListData.Count > 0)
                {
                    long d = (long)(DateTime.Now - ListData[ListData.Count - 1].Date).TotalMilliseconds;
                    ListData.Add(new Data(d, DateTime.Now, serialPort1.PortName + "rem", e.Message, Data.DATADIRECTION.Out));
                }
                else
                {
                    ListData.Add(new Data(0, DateTime.Now, serialPort1.PortName + "rem", e.Message, Data.DATADIRECTION.Out));
                }
                Invoke(new EventHandler(UpdateUi));
            }
            else
            if (ChRemote2.Checked && serialPort2.IsOpen)
            {
                serialPort2.Write(e.Message, 0, e.Message.Length);
                if (ListData.Count > 0)
                {
                    long d = (long)(DateTime.Now - ListData[ListData.Count - 1].Date).TotalMilliseconds;
                    ListData.Add(new Data(d, DateTime.Now, serialPort2.PortName + "rem", e.Message, Data.DATADIRECTION.Out));
                }
                else
                {
                    ListData.Add(new Data(0, DateTime.Now, serialPort2.PortName+"rem", e.Message, Data.DATADIRECTION.Out));
                }
                Invoke(new EventHandler(UpdateUi));
            }else
            {
                if (ListData.Count > 0)
                {
                    long d = (long)(DateTime.Now - ListData[ListData.Count - 1].Date).TotalMilliseconds;
                    ListData.Add(new Data(d, DateTime.Now, serialPort1.PortName + "rem", e.Message, Data.DATADIRECTION.Out));
                }
                else
                {
                    ListData.Add(new Data(0, DateTime.Now, serialPort1.PortName + "rem", e.Message, Data.DATADIRECTION.Out));
                }
                Invoke(new EventHandler(UpdateUi));
            }


           
        }

        private void ChRemote2_CheckedChanged(object sender, EventArgs e)
        {
            if(ChRemote2.Checked)
            {
                ChLinkToPort1.Checked = false;
            }
        }

        private void ChRemote1_CheckedChanged(object sender, EventArgs e)
        {
            if (ChRemote1.Checked)
            {
                ChLinkToPort2.Checked = false;
            }
        }

        private void TSMIclosePort1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.DiscardOutBuffer();
                serialPort1.DiscardInBuffer();
                serialPort1.Close();
            }

            if (serialPort1.IsOpen)
            {
                Port1Status.ForeColor = Color.Lime;
            }
            else
            {
                Port1Status.ForeColor = Color.Red;
            }

           

        }

        private void TSMIclosePort2_Click(object sender, EventArgs e)
        {
           
            if (serialPort2.IsOpen)
            {
                serialPort2.DiscardOutBuffer();
                serialPort2.DiscardInBuffer();
                serialPort2.Close();
            }

            if (serialPort2.IsOpen)
            {
                Port2Status.ForeColor = Color.Lime;
            }
            else
            {
                Port2Status.ForeColor = Color.Red;
            }
        }
    }
}
