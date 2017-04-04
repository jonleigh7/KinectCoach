using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }
    /// <summary>
    /// Interaction logic for realtime.xaml
    /// </summary>
    public partial class realtime : Window
    {

        StringTable wally;
        bool isInRealTime = false;
        long count = 0;
        KinectSensor sensor;
        ColorFrameReader reader;
        WriteableBitmap colorBitmap;


        //body
        BodyFrameReader bodyFrameReader;
        Body[] bodies;
        //read up to six different bodies 

        String fullData;
        public realtime()
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

                        


                        if (joints[JointType.SpineMid].TrackingState == TrackingState.Tracked
                            && joints[JointType.SpineBase].TrackingState == TrackingState.Tracked
                            && joints[JointType.HipRight].TrackingState == TrackingState.Tracked
                            && joints[JointType.HipLeft].TrackingState == TrackingState.Tracked
                            && joints[JointType.SpineShoulder].TrackingState == TrackingState.Tracked
                            && joints[JointType.Neck].TrackingState == TrackingState.Tracked
                            && joints[JointType.ShoulderLeft].TrackingState == TrackingState.Tracked
                            && joints[JointType.ShoulderRight].TrackingState == TrackingState.Tracked
                            && joints[JointType.KneeLeft].TrackingState == TrackingState.Tracked
                            && joints[JointType.KneeRight].TrackingState == TrackingState.Tracked
                            && joints[JointType.AnkleLeft].TrackingState == TrackingState.Tracked
                            && joints[JointType.AnkleRight].TrackingState == TrackingState.Tracked )//look at joints and look at jointed enums 
                        {

                            count++;


                            String timeStamp = DateTime.Now.ToString("mm:ss.ffff");

                            /*  
                              String yLeft = joints[JointType.HandLeft].Position.Y.ToString();
                              String xLeft = joints[JointType.HandLeft].Position.X.ToString();
                              String zLeft = joints[JointType.HandLeft].Position.Z.ToString();
                              String yRight = joints[JointType.HandRight].Position.Y.ToString();
                              String xRight = joints[JointType.HandRight].Position.X.ToString();
                              String zRight = joints[JointType.HandRight].Position.Z.ToString();
                              */

                            String spineMid = joints[JointType.SpineMid].Position.Z.ToString();
                            String spineBase = joints[JointType.SpineBase].Position.Z.ToString();
                            String hipRight = joints[JointType.HipRight].Position.Z.ToString();
                            String hipLeft = joints[JointType.HipLeft].Position.Z.ToString();
                            String spineShoulder = joints[JointType.SpineShoulder].Position.Z.ToString();
                            String neck = joints[JointType.Neck].Position.Z.ToString();
                            String head = joints[JointType.Head].Position.Z.ToString();
                            String shoulderRight = joints[JointType.ShoulderRight].Position.Z.ToString();
                            String shoulderLeft = joints[JointType.ShoulderLeft].Position.Z.ToString();
                            String kneeRight = joints[JointType.KneeRight].Position.Z.ToString();
                            String kneeLeft = joints[JointType.KneeLeft].Position.Z.ToString();
                            String ankleRight = joints[JointType.AnkleRight].Position.Z.ToString();
                            String ankleLeft = joints[JointType.AnkleLeft].Position.Z.ToString();

                            //  txtleft.Text = "x: " + xLeft + ", y:" + yLeft + ", z: " + zLeft;
                            txtright.Text = "Tracked";


                            if (isInRealTime&&(count == 0 || count % 10 == 0))
                                InvokeRequestResponseService();

                            String complete = String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13} ", timeStamp, spineMid, spineBase, hipRight, hipLeft, spineShoulder, neck, head, shoulderRight, shoulderLeft, kneeRight, kneeLeft, ankleRight, ankleLeft);

                            

                            wally = new StringTable()
                            {
                                ColumnNames = new string[] { "Col1", "Col2", "Col3", "Col4", "Col5", "Col6", "Col7", "Col8", "Col9", "Col10", "Col11", "Col12", "Col13", "Col14", "Col15" },
                                Values = new string[,] { { "value", spineMid, spineBase, hipRight, hipLeft, spineShoulder, neck, head, shoulderRight, shoulderLeft, shoulderLeft, kneeLeft, ankleRight, ankleLeft, "0" }, }
                            };
                            
     

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
                        txtleft.Text = "Score: " + (Double.Parse(first.Substring(124, 8)) - 10) + ""; 



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
