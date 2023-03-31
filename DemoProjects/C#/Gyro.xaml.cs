using System;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using HelixToolkit.Wpf;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;
using System.Linq;



namespace Acc3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
       
        //COMS
        TcpListener server_socket = new TcpListener(IPAddress.Parse("192.168.43.191"), 8221);
        TcpClient clsckt = default(TcpClient);
        SerialPort sp = new SerialPort("COM11", 115200, Parity.None, 8, StopBits.One);
        

        //3D
        Transform3DGroup tgr = new Transform3DGroup();
        AxisAngleRotation3D x = new AxisAngleRotation3D();
        AxisAngleRotation3D y = new AxisAngleRotation3D();
        AxisAngleRotation3D z = new AxisAngleRotation3D();
        Vector3D Vx = new Vector3D(1, 0, 0);
        Vector3D Vy = new Vector3D(0, 1, 0);
        Vector3D Vz = new Vector3D(0, 0, 1);
        RotateTransform3D rotx = new RotateTransform3D();
        RotateTransform3D roty = new RotateTransform3D();
        RotateTransform3D rotz = new RotateTransform3D();
        HelixViewport3D hvp = new HelixViewport3D();
        BoxVisual3D box = new BoxVisual3D();

        //DATA
        byte[] RXData = new byte[206];
        string data = "";
        int filter = 1;
        double[] accArr = new double[3];
        double[] GyroArr = new double[3];
        double[] FilteredArr = new double[3];
        double accXave = 0;
        double accYave = 0;
        double accZave = 0;

        //Normal
        char b = 'n';
        int j = 1;
        string[] arr = new string[10000];
        int i = 0;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

                     
            
            sp.DataReceived+=sp_DataReceived;

            var lights = new DefaultLights();

            Material m = new DiffuseMaterial(new SolidColorBrush(Colors.Beige));

            box.Material = m;
            box.BackMaterial = m;
            box.Width = 2;
            box.Height = 0.5;
            box.Length = 5;


            hvp.Height = cnv3Dcontent.ActualHeight;
            hvp.Width = cnv3Dcontent.ActualWidth;
            hvp.Children.Add(box);
            hvp.Children.Add(lights);
            hvp.ShowCameraInfo = true;
            hvp.CameraRotationMode = CameraRotationMode.Turntable;


            hvp.Camera.LookDirection = new Vector3D(25.673, -0.056, -0.952);
            hvp.Camera.UpDirection = new Vector3D(-0.755, 0.002, 0.656);
            hvp.Camera.Position = new Point3D(-25.673, 0.056, 0.952);
            // box.Transform = new TranslateTransform3D(1, 1, 1);
            cnv3Dcontent.Children.Add(hvp);
            //  server.Connect(remote_ep);

           

        }
       
        public  void Update_Layout_Orientation_Android()
        {
           // MessageBox.Show(data);
            if (data != "" && data != null)
            {
                txtValues.Text = data.Split(',')[0] + "|" + data.Split(',')[1] + "|" + data.Split(',')[2];
                //txtValues.Text = data;
                tgr.Children.Clear();
                
                rotx.Rotation = x;
               // x.Angle = Convert.ToDouble(data.Split(',')[0]);
                x.Angle = double.Parse(data.Split(',')[0],CultureInfo.InvariantCulture);
                x.Axis = Vx;

                roty.Rotation = y;
               // y.Angle = Convert.ToDouble(data.Split(',')[1]);
                y.Angle = double.Parse(data.Split(',')[1], CultureInfo.InvariantCulture);
                y.Axis = Vy;

                rotz.Rotation = z;
                //z.Angle = 360- Convert.ToDouble(data.Split(',')[2])-360;
                z.Angle =360- double.Parse(data.Split(',')[2], CultureInfo.InvariantCulture)-360;
                z.Axis = Vz;



                tgr.Children.Add(rotx);
                tgr.Children.Add(roty);
                tgr.Children.Add(rotz);

                box.Transform = tgr;




                   // box.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), Convert.ToDouble(data.Split(',')[0])));
                  // box.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), Convert.ToDouble(data.Split(',')[1])));
                  // box.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), Convert.ToDouble(data.Split(',')[2])));
   
            }
            
        }
        public void Update_Layout_Acc_Android()
        {
            double xAcc = 0;
            double yAcc = 0;
            double zAcc = 0;
            double R = 0;
        
            if (data != "" && data != null)
            {
         
                xAcc = Convert.ToDouble(data.Split(',')[0]);
                yAcc = Convert.ToDouble(data.Split(',')[1]);
                zAcc = Convert.ToDouble(data.Split(',')[2]);

                R = Math.Sqrt(Math.Pow(xAcc, 2) + Math.Pow(yAcc, 2) + Math.Pow(zAcc, 2));

               

                txtValues.Text = data;
                tgr.Children.Clear();
                //MessageBox.Show((180 * Math.Acos(0) / Math.PI).ToString());
                rotx.Rotation = x;
                x.Angle = (180* Math.Acos(xAcc / R)/Math.PI)-90;
                x.Axis = Vx;

                roty.Rotation = y;
                y.Angle = (180 * Math.Acos(yAcc / R) / Math.PI) - 90;
                y.Axis = Vy;

                rotz.Rotation = z;
                z.Angle = (180 * Math.Acos(zAcc / R) / Math.PI);
                z.Axis = Vz;



                tgr.Children.Add(rotx);
                tgr.Children.Add(roty);
                tgr.Children.Add(rotz);

                box.Transform = tgr;
                txtValues.Text = ((int)(x.Angle)).ToString() + "|" + ((int)(y.Angle)).ToString() + "|" + ((int)(z.Angle)).ToString();
            }

        }
        
        
        public void Update_Layout_Acc_Mpu9250_Test()
        {
           
            double R = 0;
           // MessageBox.Show(data.Split('|')[0].ToString() + "|" + data.Split('|')[1].ToString() + "|" + data.Split('|')[2].ToString());
            
                

                R = Math.Sqrt(Math.Pow(accArr[0], 2) + Math.Pow(accArr[1], 2) + Math.Pow(accArr[2], 2));

                txtValues.Text = data;
                tgr.Children.Clear();
                
                rotx.Rotation = x;
                x.Angle = (180 * Math.Acos(accArr[0] / R) / Math.PI) - 90;
                x.Axis = Vx;

                roty.Rotation = y;
                y.Angle = (180 * Math.Acos(accArr[1] / R) / Math.PI) - 90;
                y.Axis = Vy;


               /* rotz.Rotation = z;
                z.Angle = (180 * Math.Acos(zAcc / R) / Math.PI);
                z.Axis = Vz;*/

                tgr.Children.Add(rotx);
                tgr.Children.Add(roty);
                tgr.Children.Add(rotz);

                box.Transform = tgr;
                try
                {
                    txtValues.Text = ((int)(x.Angle)).ToString() + "|" + ((int)(y.Angle)).ToString() + "|" + ((int)(z.Angle)).ToString();
                }
                catch { }
            

        }
        private void filterACC()
        {



            accXave += accArr[0];
            accYave += accArr[1];
            accZave += accArr[2];



            if (filter == 5)
            {


                try
                {
                    accArr[0] = accXave / 5;
                    accArr[1] = accYave / 5;
                    accArr[2] = accZave / 5;

                }
                catch { MessageBox.Show("Err:" + data + "||" + data.Split('|')[3] + "|" + data.Split('|')[4] + "|" + data.Split('|')[5]); }

                accXave = 0;
                accYave = 0;
                accZave = 0;
                filter = 0;
                try
                {
                    this.Dispatcher.Invoke(new Action(() => Update_Layout_Acc_Mpu9250()), DispatcherPriority.Render);
                }
                catch { }


            }
            filter++;


        }


        public void Update_Layout_Acc_Mpu9250()
        {

            rotx.Rotation = x;
            x.Angle = (accArr[0]);
            x.Axis = Vx;


            roty.Rotation = y;
            y.Angle = (accArr[1]);
            y.Axis = Vy;

            rotz.Rotation = z;
            z.Angle = (accArr[2]);
            z.Axis = Vz;

            txtValues.Text = data;
            tgr.Children.Clear();



            tgr.Children.Add(rotx);
            tgr.Children.Add(roty);
            tgr.Children.Add(rotz);

            box.Transform = tgr;
            try
            {
                txtValues.Text = ((int)(x.Angle)).ToString() + "|" + ((int)(y.Angle)).ToString() + "|" + ((int)(z.Angle)).ToString();
            }
            catch { }


        }
        public void Update_Layout_Gyro_Mpu9250()
        {
          
                rotx.Rotation = x;
                x.Angle = (GyroArr[0]);
                x.Axis = Vx;
                    

                roty.Rotation = y;
                y.Angle =(GyroArr[1]);
                y.Axis = Vy;

                rotz.Rotation = z;
                z.Angle = (GyroArr[2]);
                z.Axis = Vz;

                txtValues.Text = data;
                tgr.Children.Clear();

                

                tgr.Children.Add(rotx);
                tgr.Children.Add(roty);
                tgr.Children.Add(rotz);

                box.Transform = tgr;
                try
                {
                    txtValues.Text = ((int)(x.Angle)).ToString() + "|" + ((int)(y.Angle)).ToString() + "|" + ((int)(z.Angle)).ToString();
                }
                catch { }
           
               
        }
        public void Update_Layout_Filtered_Mpu9250()
        {

            rotx.Rotation = x;
            x.Angle = (FilteredArr[0]);
            x.Axis = Vx;


            roty.Rotation = y;
            y.Angle = (FilteredArr[1]);
            y.Axis = Vy;

            rotz.Rotation = z;
            z.Angle = (FilteredArr[2]);
            z.Axis = Vz;

            txtValues.Text = data;
            tgr.Children.Clear();



            tgr.Children.Add(rotx);
            tgr.Children.Add(roty);
           // tgr.Children.Add(rotz);

            box.Transform = tgr;
            try
            {
                txtValues.Text = ((int)(x.Angle)).ToString() + "|" + ((int)(y.Angle)).ToString() + "|" + ((int)(z.Angle)).ToString();
            }
            catch { }


        }
        public void Update_Display_Mpu9250()
        {
            if (chAcc.IsChecked == true)
            {
                this.Dispatcher.Invoke(new Action(() => Update_Layout_Acc_Mpu9250()), DispatcherPriority.Render);
            }
            if (chGyro.IsChecked == true)
            {
                this.Dispatcher.Invoke(new Action(() => Update_Layout_Gyro_Mpu9250()), DispatcherPriority.Render);
            }
            if (chFilter.IsChecked == true)
            {
                this.Dispatcher.Invoke(new Action(() => Update_Layout_Filtered_Mpu9250()), DispatcherPriority.Render);
            }
        }
        private void Close_SerialPort()
        {
           
            
        }
   

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
                data = sp.ReadLine();

                if (data.Split('|').Length == 9)
                {
                    //MessageBox.Show(data);
                    try
                    {
                        accArr[0] = Convert.ToDouble(data.Split('|')[0]);
                        accArr[1] = Convert.ToDouble(data.Split('|')[1]);
                        accArr[2] = Convert.ToDouble(data.Split('|')[2]);
                        GyroArr[0] = Convert.ToDouble(data.Split('|')[3]);
                        GyroArr[1] = Convert.ToDouble(data.Split('|')[4]);
                        GyroArr[2] = Convert.ToDouble(data.Split('|')[5]);
                        FilteredArr[0] = Convert.ToDouble(data.Split('|')[6]);
                        FilteredArr[1] = Convert.ToDouble(data.Split('|')[7]);
                        FilteredArr[2] = Convert.ToDouble(data.Split('|')[8]);

                   
                        Dispatcher.Invoke(new Action(() => Update_Display_Mpu9250()), DispatcherPriority.Render);
                    }
                    catch { }


                }
                 
        }
        private void UDP_DataReceived_DoWork()
        {

            while (true)
            {

                if (server_socket.Pending() == true)
                {
                    clsckt = server_socket.AcceptTcpClient();
                    b = 'y';
                    //Dispatcher.Invoke(new Action(() => Update_Layout(sender, e)), DispatcherPriority.Render);
                }
                if (b == 'y')
                {
                    //i++;
                    if (clsckt.Connected == true)
                    {
                        if (clsckt.GetStream().DataAvailable)
                        {
                            StreamReader sr = new System.IO.StreamReader(clsckt.GetStream());

                            data = sr.ReadLine();
                            //sr.Close();
                            //sr.Dispose();
                        }
                        //sr.Close();
                    }
                    try
                    {
                        //this.Dispatcher.Invoke(new Action(() => Update_Layout_Orientation()), DispatcherPriority.Render);
                        this.Dispatcher.Invoke(new Action(() => Update_Layout_Acc_Android()), DispatcherPriority.Render);
                    }
                    catch { }

                }

            }

         
        }
        private void AnimateBox()
        {

            RotateTransform3D rotateTransform = new RotateTransform3D();
            rotateTransform.CenterX = 0;
            rotateTransform.CenterY = 0;
            rotateTransform.CenterZ = 0;
           
            AxisAngleRotation3D rotateAxisFrom = new AxisAngleRotation3D(new Vector3D(Convert.ToDouble(arr[j - 1].Split(',')[0]), Convert.ToDouble(arr[j - 1].Split(',')[1]), Convert.ToDouble(arr[j - 1].Split(',')[2])), Convert.ToDouble(arr[j - 1].Split(',')[3]));
            AxisAngleRotation3D rotateAxisTo = new AxisAngleRotation3D(new Vector3D(Convert.ToDouble(arr[j].Split(',')[0]), Convert.ToDouble(arr[j].Split(',')[1]), Convert.ToDouble(arr[j].Split(',')[2])), Convert.ToDouble(arr[j].Split(',')[3]));
            Rotation3DAnimation rotateAnimation = new Rotation3DAnimation(rotateAxisFrom,rotateAxisTo, TimeSpan.FromSeconds(1));

            if (j <i-1 )
            {
                rotateAnimation.Completed += rotateAnimation_Completed;
                j++;
            }

            rotateTransform.Rotation = rotateAxisFrom; 
            box.Transform = rotateTransform;
          
        
            rotateTransform.BeginAnimation(RotateTransform3D.RotationProperty, rotateAnimation);
            
    
        }
        private void rotateAnimation_Completed(object sender, EventArgs e)
        {
            AnimateBox();
        }
      

        private void slX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            box.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), e.NewValue));
        }
        private void slY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            box.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), e.NewValue));
        }
        private void slZ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            box.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), e.NewValue));
        }
        private void chGyro_Checked(object sender, RoutedEventArgs e)
        {
            if (chGyro.IsChecked == true)
            {
                chAcc.IsChecked = false;
                chFilter.IsChecked = false;
            }
        }
        private void chAcc_Checked(object sender, RoutedEventArgs e)
        {
            if (chAcc.IsChecked == true)
            {
                chGyro.IsChecked = false;
                chFilter.IsChecked = false;
            }
        }
        private void chFilter_Checked(object sender, RoutedEventArgs e)
        {
            if (chFilter.IsChecked == true)
            {
                chGyro.IsChecked = false;
                chAcc.IsChecked = false;
            }
        }
      
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           Dispatcher.Invoke(new Action(() => Close_SerialPort()), DispatcherPriority.Render);
        }
        private void Window_Closed(object sender, EventArgs e)
        {


            //try closing socket on a different thread by invoking
            /*try
            {
                if(server_socket.Server.IsBound)
                server_socket.Server.Disconnect(false);
            }
            catch { }
            server_socket.Server.Close();*/
        }

        private void btnArduino_Click(object sender, RoutedEventArgs e)
        {
            if (SerialPort.GetPortNames().Contains("COM11"))
            {
                sp.Open();
            }
            else
            {
                MessageBox.Show("Could Not Found COM11");
            }
        }

        private void btnAndroid_Click(object sender, RoutedEventArgs e)
        {
             server_socket.Start();
             Thread th = new Thread(UDP_DataReceived_DoWork);
             th.IsBackground = true;
             th.Start();
        }

        

     
    }
}
