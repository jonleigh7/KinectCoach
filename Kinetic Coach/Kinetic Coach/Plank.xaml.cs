using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Plank.xaml
    /// </summary>
    public partial class Plank : Window
    {
        bool isInRealTime = false;
       

        StringTable wally;
        int initSeconds = 0;
        long count = 0;
        KinectSensor sensor;
        ColorFrameReader reader;
        WriteableBitmap colorBitmap;


        //body
        BodyFrameReader bodyFrameReader;
        Body[] bodies;
        //read up to six different bodies 

        String fullData;
        private bool inGame = true;

        public Plank()
        {
            sensor = KinectSensor.GetDefault();
            sensor.Open();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //set variable to set up data reader
            reader = sensor.ColorFrameSource.OpenReader();
            reader.FrameArrived += this.Reader_ColorFrameArrived;

            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrrived;

            InitializeComponent();


            //display bitmap 
            colorBitmap = new WriteableBitmap(1920, 1080, 96.0, 96.0, PixelFormats.Bgr32, null);
            //96 ppi, kinect is full hd, the kinect sends back data in bgr data
            ColorImage.Source = colorBitmap;









        }
        private double standev(double[] a, double men)
        {
            double ret = 0;
            for (int i = 0; i < a.Length; i++)
            {
                ret += Math.Pow(a[i] - men, 2);
            }
            return Math.Sqrt(Math.Abs(ret / a.Length - 1));
        }

        private double distance(String x1, String y1, String x2, String y2)
        {
            return Math.Sqrt(Math.Abs(Math.Pow(Double.Parse(x1) - Double.Parse(x2), 2) + Math.Pow(Double.Parse(y1) - Double.Parse(y2), 2)));
        }

        private double average(double[] array)
        {
            double sum = 0;
            for (int i = 0; i < array.Length; i++)
            {

                sum += array[i];
            }
            return sum / array.Length;
        }
        private void Reader_BodyFrameArrrived(object sender, BodyFrameArrivedEventArgs e)
        {




            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame == null)
                {
                    return;
                }
                if (bodies == null)
                {
                    bodies = new Body[bodyFrame.BodyCount];

                }
                bodyFrame.GetAndRefreshBodyData(bodies);
                //refresh body data for the frame 
                foreach (Body body in bodies)//do some work
                {
                    if (body.IsTracked)
                    {
                        var joints = body.Joints;


                        if (joints[JointType.SpineMid].TrackingState == TrackingState.Tracked)
                        {

                            String spineMid = joints[JointType.SpineMid].Position.Y.ToString();
                            String spineBase = joints[JointType.SpineBase].Position.Y.ToString();
                            String hipRight = joints[JointType.HipRight].Position.Y.ToString();
                            String hipLeft = joints[JointType.HipLeft].Position.Y.ToString();
                            String spineShoulder = joints[JointType.SpineShoulder].Position.Y.ToString();
                            String neck = joints[JointType.Neck].Position.Y.ToString();
                            String head = joints[JointType.Head].Position.Y.ToString();
                            String shoulderRight = joints[JointType.ShoulderRight].Position.Y.ToString();
                            String shoulderLeft = joints[JointType.ShoulderLeft].Position.Y.ToString();
                            String kneeRight = joints[JointType.KneeRight].Position.Y.ToString();
                            String kneeLeft = joints[JointType.KneeLeft].Position.Y.ToString();
                            String ankleRight = joints[JointType.AnkleRight].Position.Y.ToString();
                            String ankleLeft = joints[JointType.AnkleLeft].Position.Y.ToString();

                            count++;

                            
                            String seconds = DateTime.Now.ToString("ss");
                            String minutes = DateTime.Now.ToString("mm");
                            int totalSeconds = int.Parse(seconds) + (int.Parse(minutes) * 60);

                            if (count == 1)
                            {
                                initSeconds = totalSeconds;
                            }

                            if(inGame)
                            txtleft.Text = totalSeconds - initSeconds+""; 

                            if(Double.Parse(txtleft.Text) < 1)
                            {
                                inGame = false;
                            }


                            //  txtleft.Text = "x: " + xLeft + ", y:" + yLeft + ", z: " + zLeft;
                            txtright.Text = "Tracked";

                            //find the change in y 

                            if (Double.Parse(joints[JointType.KneeLeft].Position.Y.ToString())- Double.Parse(joints[JointType.AnkleLeft].Position.Y.ToString()) -.2<0)
                            {
                                sensor.Close();
                                inGame = false;
                            }

                            //  txtleft.Text = "x: " + xLeft + ", y:" + yLeft + ", z: " + zLeft;

                            
                            txtright.Text = "Tracked"; //first back*100 ,2nd = leg*100, 3rd = angle*10


                            if (isInRealTime && (count == 0 || count % 10 == 0))
                                InvokeRequestResponseService();

                            String complete = String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, ",  spineMid, spineBase, hipRight, hipLeft, spineShoulder, neck, head, shoulderRight, shoulderLeft, kneeRight, kneeLeft, ankleRight, ankleLeft);



                            wally = new StringTable()
                            {
                                ColumnNames = new string[] { "Col1", "Col2", "Col3", "Col4", "Col5", "Col6", "Col7", "Col8", "Col9", "Col10", "Col11", "Col12", "Col13", "Col14", "Col15" },
                                Values = new string[,] { { "value", spineMid, spineBase, hipRight, hipLeft, spineShoulder, neck, head, shoulderRight, shoulderLeft, shoulderLeft, kneeLeft, ankleRight, ankleLeft, "0" }, }
                            };



                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\jonle\Desktop\bad_posture.txt", true))
                            {

                                file.WriteLine(complete);
                            }


                        }
                        else
                        {
                            txtright.Text = "Untracked";
                        }
                    }
                }
            }
        }

        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            //use the current image frame in a memory safe manner 
            using (ColorFrame frame = e.FrameReference.AcquireFrame())
            {
                //sometimes this frame isn't available 
                //defensive programming 
                if (frame == null)
                    return;
                using (KinectBuffer colorBuffer = frame.LockRawImageBuffer())
                {
                    //threadsafe lock on this data so it doesn't get modified
                    colorBitmap.Lock();

                    //let application to know where the image is being stored 
                    frame.CopyConvertedFrameDataToIntPtr(colorBitmap.BackBuffer, (uint)(1920 * 1080 * 4), ColorImageFormat.Bgra);
                    //the number is width*height*bytes per pixel

                    //redraw the screen in this area
                    colorBitmap.AddDirtyRect(new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));

                    //unlock
                    colorBitmap.Unlock();
                }
            }

        }

        private void Api_Click(object sender, RoutedEventArgs e)
        {
            InvokeRequestResponseService();
        }
        async Task InvokeRequestResponseService()
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            wally
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                const string apiKey = "8dNrtbfZT3i3736W6Tc0UORAoU7dYbAcUq84LpSl1wSGrc498scRu66NDF1eVpgzctslTroIVRg72ucoSUMnhA=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/1924d8d702784c8a9c4bccc4a2d57dd2/services/77513e7599dd48a5971d8c5a89323df6/execute?api-version=2.0&details=true");

                // WARNING: The 'await' statement below can result in a deadlock if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false) so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)


                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    this.Dispatcher.Invoke(() =>
                    {
                        String first = string.Format("Result: {0}", result);
                        txtleft.Text = "Score: " + ( Double.Parse(first.Substring(124, 8)) - 10) + ""; // change this


                    });

                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        txtleft.Text = string.Format("The request failed with status code: {0}", response.StatusCode);
                    });
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Console.WriteLine(responseContent);
                }
            }
        }


        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sensor.Close();
        }
      
    private void BeginRealTime(object sender, RoutedEventArgs e)
        {
            isInRealTime = true;
            apicallbutton.IsEnabled = false;
            button1.IsEnabled = false;
            button.IsEnabled = true;
        }
        private void EndRealTime(object sender, RoutedEventArgs e)
        {
            isInRealTime = false;
            apicallbutton.IsEnabled = true;
            button1.IsEnabled = true;
            button.IsEnabled = false;
        }
        private void ReturnToMain(object sender, RoutedEventArgs e)
        {
            MainWindow subWindow = new MainWindow();
            subWindow.Show();
            this.Close();
        }
    }
}
