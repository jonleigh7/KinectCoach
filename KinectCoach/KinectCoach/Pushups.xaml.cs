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


    /// <summary>
    /// Interaction logic for realtime.xaml
    /// </summary>
    public partial class Pushups : Window
    {

        double[] wally2 = new double[3];
        StringTable pushupRequestData;
        bool isInRealTime = true;
        // data from prev frames
        Boolean up = false;
        Boolean down = false;
        Boolean atLeastOne = false;
        double lowestCDiff = 500000000000000000;
        double highestHip = -999;
        double yInitial = 0.0;
        double y = 0;
        // the y value from the frame before

        // storage
        double averageBack = 0;
        double averageLeg = 0;
        int countBack = 0;
        int countLeg = 0;

        KinectSensor pushupSensor;
        ColorFrameReader pushupReader;
        WriteableBitmap pushupColorBitmap;
        double[] data = new double[3];
        String timestamp;


        //body
        BodyFrameReader pushupBodyReader;
        Body[] pushupBodies;
        //read up to six different bodies 

        //String fullData;
        public Pushups()
        {
            pushupSensor = KinectSensor.GetDefault();
            pushupSensor.Open();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //set variable to set up data reader
            pushupReader = pushupSensor.ColorFrameSource.OpenReader();
            pushupReader.FrameArrived += this.Reader_ColorFrameArrived;

            pushupBodyReader = pushupSensor.BodyFrameSource.OpenReader();
            pushupBodyReader.FrameArrived += this.Reader_BodyFrameArrrived;

            InitializeComponent();


            //display bitmap 
            pushupColorBitmap = new WriteableBitmap(1920, 1080, 96.0, 96.0, PixelFormats.Bgr32, null);
            //96 ppi, kinect is full hd, the kinect sends back data in bgr data
            ColorImage.Source = pushupColorBitmap;

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
                if (pushupBodies == null)
                {
                    pushupBodies = new Body[bodyFrame.BodyCount];

                }
                bodyFrame.GetAndRefreshBodyData(pushupBodies);
                //refresh body data for the frame 
                foreach (Body body in pushupBodies)//do some work
                {
                    if (body.IsTracked)
                    {
                        var joints = body.Joints;




                        if (joints[JointType.SpineMid].TrackingState == TrackingState.Tracked
                            && joints[JointType.SpineBase].TrackingState == TrackingState.Tracked
                            && joints[JointType.Neck].TrackingState == TrackingState.Tracked
                            && joints[JointType.Head].TrackingState == TrackingState.Tracked
                            && joints[JointType.ShoulderLeft].TrackingState == TrackingState.Tracked)//look at joints and look at jointed enums 
                        {
                            //Head, neck spine
                            double backLine = 0;
                            double legLine = 0;
                            double cee = 0;
                            double expectedCee = 0;
                            double ceeDiff = 0;


                            String timeStamp = DateTime.Now.ToString("mm:ss.ffff");

                            //Straight back
                            double backAvgy = 0;
                            double backAvgx = 0;
                            double[] straightBackX = new double[5];
                            double[] straightBackY = new double[5];

                            String neck = joints[JointType.Neck].Position.Y.ToString();
                            String head = joints[JointType.Head].Position.Y.ToString();
                            String spineShoulder = joints[JointType.SpineShoulder].Position.Y.ToString();
                            String spineMid = joints[JointType.SpineMid].Position.Y.ToString();
                            String spineBase = joints[JointType.SpineBase].Position.Y.ToString();

                            String neckx = joints[JointType.Neck].Position.X.ToString();
                            String headx = joints[JointType.Head].Position.X.ToString();
                            String spineShoulderx = joints[JointType.SpineShoulder].Position.X.ToString();
                            String spineMidx = joints[JointType.SpineMid].Position.X.ToString();
                            String spineBasex = joints[JointType.SpineBase].Position.X.ToString();

                            straightBackX[0] = Double.Parse(neckx);
                            straightBackX[1] = Double.Parse(headx);
                            straightBackX[2] = Double.Parse(spineShoulderx);
                            straightBackX[3] = Double.Parse(spineMidx);
                            straightBackX[4] = Double.Parse(spineBasex);

                            straightBackY[0] = Double.Parse(neck);
                            straightBackY[1] = Double.Parse(head);
                            straightBackY[2] = Double.Parse(spineShoulder);
                            straightBackY[3] = Double.Parse(spineMid);
                            straightBackY[4] = Double.Parse(spineBase);

                            backAvgx = average(straightBackX);
                            backAvgy = average(straightBackY);

                            double backDevx = standev(straightBackX, backAvgx);
                            double backDevY = standev(straightBackY, backAvgy);
                            double finalthingBack = 0;
                            for (int i = 0; i < 5; i++)
                            {
                                finalthingBack += ((straightBackX[i] - backAvgx) / backDevx) * (((straightBackY[i] - backAvgy) / backDevY));
                            }


                            //important data
                            backLine = finalthingBack / 4.0;

                            // Calc c
                            String wristRight = joints[JointType.WristRight].Position.Y.ToString();
                            String wristLeft = joints[JointType.WristLeft].Position.Y.ToString();
                            String wristRightx = joints[JointType.WristRight].Position.X.ToString();
                            String wristLeftx = joints[JointType.WristLeft].Position.X.ToString();

                            String elbowRight = joints[JointType.ElbowRight].Position.Y.ToString();
                            String elbowRightX = joints[JointType.ElbowRight].Position.X.ToString();
                            String elbowLeft = joints[JointType.ElbowLeft].Position.Y.ToString();
                            String elbowLeftX = joints[JointType.ElbowLeft].Position.X.ToString();
                            double distWrist = (Math.Sqrt(Math.Abs(Math.Pow(Double.Parse(elbowLeft) - Double.Parse(wristLeftx), 2) + Math.Pow(Double.Parse(elbowLeftX) - Double.Parse(wristLeftx), 2)) + Math.Sqrt(Math.Pow(Double.Parse(elbowRight) - Double.Parse(wristRightx), 2) + Math.Pow(Double.Parse(elbowRightX) - Double.Parse(wristRightx), 2)))) / 2;
                            String shoulderRight = joints[JointType.ShoulderRight].Position.Y.ToString();
                            String shoulderLeft = joints[JointType.ShoulderLeft].Position.Y.ToString();
                            String shoulderRightx = joints[JointType.ShoulderRight].Position.X.ToString();
                            String shoulderLeftx = joints[JointType.ShoulderLeft].Position.X.ToString();
                            double distShoulder = (distance(shoulderRight, elbowRight, shoulderRightx, elbowRightX) + distance(shoulderLeftx, elbowLeftX, shoulderLeft, elbowLeft)) / 2;
                            //impotant data
                            expectedCee = Math.Sqrt(Math.Abs(Math.Pow(distWrist, 2) + Math.Pow(distShoulder, 2)));

                            cee = Math.Sqrt(Math.Abs(Math.Pow(Double.Parse(wristRight) - Double.Parse(shoulderRight), 2) + Math.Pow(Double.Parse(wristRightx) - Double.Parse(shoulderRightx), 2)));
                            cee *= 10;
                            ceeDiff = Math.Abs(cee - expectedCee);





                            //Leg straight

                            double legAvgy = 0;
                            double legAvgx = 0;
                            double[] straightlegX = new double[7];
                            double[] straightlegY = new double[7];

                            String hipRight = joints[JointType.HipRight].Position.Y.ToString();
                            String hipLeft = joints[JointType.HipLeft].Position.Y.ToString();
                            Console.WriteLine("Hip Left: " + hipLeft);
                            String xhipRight = joints[JointType.HipRight].Position.X.ToString();
                            String xhipLeft = joints[JointType.HipLeft].Position.X.ToString();

                            String kneeRight = joints[JointType.KneeRight].Position.Y.ToString();
                            String kneeLeft = joints[JointType.KneeLeft].Position.Y.ToString();
                            String kneeRightx = joints[JointType.KneeRight].Position.X.ToString();
                            String kneeLeftx = joints[JointType.KneeLeft].Position.X.ToString();

                            String ankleRight = joints[JointType.AnkleRight].Position.Y.ToString();
                            String ankleLeft = joints[JointType.AnkleLeft].Position.Y.ToString();
                            String ankleRightx = joints[JointType.AnkleRight].Position.X.ToString();
                            String ankleLeftx = joints[JointType.AnkleLeft].Position.X.ToString();

                            straightlegX[0] = Double.Parse(xhipRight);
                            straightlegX[1] = Double.Parse(xhipLeft);
                            straightlegX[2] = Double.Parse(kneeRightx);
                            straightlegX[3] = Double.Parse(kneeLeftx);
                            straightlegX[4] = Double.Parse(ankleRightx);
                            straightlegX[5] = Double.Parse(ankleLeftx);
                            straightlegX[6] = Double.Parse(spineBasex);

                            straightlegY[0] = Double.Parse(hipRight);
                            straightlegY[1] = Double.Parse(hipLeft);
                            straightlegY[2] = Double.Parse(kneeRight);
                            straightlegY[3] = Double.Parse(kneeLeft);
                            straightlegY[4] = Double.Parse(ankleRight);
                            straightlegY[5] = Double.Parse(ankleLeft);
                            straightlegY[6] = Double.Parse(spineBase);
                            legAvgx = average(straightlegX);
                            legAvgy = average(straightlegY);

                            double legDevx = standev(straightlegX, legAvgx);
                            double legDevY = standev(straightlegY, legAvgy);
                            double finalthingLeg = 0;
                            for (int i = 0; i < 5; i++)
                            {
                                finalthingLeg += ((straightlegX[i] - legAvgx) / legDevx) * (((straightlegY[i] - legAvgy) / legDevY));
                            }
                            //important data
                            legLine = finalthingLeg / 5.0;








                            //  txtleft.Text = "x: " + xLeft + ", y:" + yLeft + ", z: " + zLeft;
                            txtright.Text = "Tracked";

                            //find the change in y 

                            double yCurrent = Double.Parse(joints[JointType.ShoulderRight].Position.Y.ToString());

                            y = Double.Parse(head);






                            timestamp = timeStamp;
                            // new code
                            if (Double.Parse(hipLeft) > highestHip)
                            {
                                highestHip = Double.Parse(hipLeft);
                            }
                            if (Double.Parse(hipRight) > highestHip)
                            {
                                highestHip = Double.Parse(hipRight);
                            }
                            double errorFactor = 0.5;
                            Console.WriteLine("Y val: " + (y - yInitial));

                            if (Double.Parse(head) < highestHip)
                                Console.WriteLine("head works");

                            Console.WriteLine(y - yInitial - errorFactor);
                            if (Double.Parse(head) < highestHip && (y - yInitial - errorFactor) < -0.55)
                            {
                                down = true;
                                Console.WriteLine(Double.Parse(head) - highestHip);
                                if (ceeDiff < lowestCDiff)
                                {
                                    lowestCDiff = ceeDiff;
                                }
                                averageBack += backLine;
                                countBack++; 


                                averageLeg += legLine;
                                countLeg++;


                            }
                            if (Double.Parse(head) < highestHip && down && (y - yInitial + errorFactor) > 0.55)
                            {
                                Console.WriteLine(Double.Parse(head) - highestHip);
                                up = true;
                                if (ceeDiff < lowestCDiff)
                                {
                                    lowestCDiff = ceeDiff;
                                }
                                averageBack += backLine;
                                countBack++;

                                averageLeg += legLine;
                                countLeg++;

                            }
                            // Console.WriteLine("Lowest CDIFF:" + lowestCDiff);
                            if (atLeastOne && Double.Parse(head) > highestHip)
                            {
                                averageBack = averageBack / countBack;
                                averageLeg = averageLeg / countLeg;
                                //write to file averageBack,averageLeg,lowestCDiff
                                String complete = String.Format("{0}, {1}, {2}", averageBack, averageLeg, lowestCDiff);
                                Console.WriteLine(complete);
                                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\jonle\Desktop\pushups-new.txt", true))
                                {
                                    file.WriteLine(complete + "lastup&down");
                                }
                               

                                wally2[0] = averageBack * 100;
                                wally2[1] = averageLeg * 100;
                                wally2[2] = lowestCDiff * 10;

                                Console.WriteLine("back:" + wally2[0] + ", leg: " + wally2[1] + ", cdiff: " + wally2[2]);

                                if (isInRealTime)
                                {
                                    InvokeRequestResponseService();
                                }



                                pushupSensor.Close();
                               // MainWindow window = new MainWindow();
                               // window.Show();
                                //this.Close();
                                //end
                            }

                            Console.WriteLine(Double.Parse(head) + " | " + highestHip);


                            if (up && down)
                            {
                                averageBack = averageBack / countBack;
                                averageLeg = averageLeg / countLeg;
                                //write 
                                String complete = String.Format("{0}, {1}, {2}", averageBack, averageLeg, lowestCDiff);
                                Console.WriteLine(complete);


                                wally2[0] = averageBack * 100;
                                wally2[1] = averageLeg * 100;
                                wally2[2] = lowestCDiff * 10;

                                Console.WriteLine("back:" + wally2[0] + ", leg: " + wally2[1] + ", cdiff: " + wally2[2]);
                                if (isInRealTime)
                                {
                                    InvokeRequestResponseService();
                                }


                                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\jonle\Desktop\pushups-data.txt", true))
                                {
                                    file.WriteLine(complete + "up&down2");
                                }

                                averageBack = 0;
                                averageLeg = 0;
                                countBack = 0;
                                countLeg = 0;
                                lowestCDiff = 500000;
                                up = false;
                                down = false;
                                atLeastOne = true;
                                //write to file averageBack,averageLeg,lowestCDiff

                            }

                        }
                        else
                        {
                            txtright.Text = "Untracked";
                        }
                        yInitial = y;
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
                    pushupColorBitmap.Lock();

                    //let application to know where the image is being stored 
                    frame.CopyConvertedFrameDataToIntPtr(pushupColorBitmap.BackBuffer, (uint)(1920 * 1080 * 4), ColorImageFormat.Bgra);
                    //the number is width*height*bytes per pixel

                    //redraw the screen in this area
                    pushupColorBitmap.AddDirtyRect(new Int32Rect(0, 0, pushupColorBitmap.PixelWidth, pushupColorBitmap.PixelHeight));

                    //unlock
                    pushupColorBitmap.Unlock();
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
                    Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "Col1", wally2[0]+""
                                            },
                                            {
                                                "Col2", wally2[1]+""
                                            },
                                            {
                                                "Col3", wally2[2]+""
                                            },
                                            {
                                                "Col4", "1"
                                            },
                                            {
                                                "Col5", "1"
                                            },
                                            {
                                                "Col6", "1"
                                            },
                                            {
                                                "Col7", "1"
                                            },
                                }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                const string apiKey = "W8kruMnFXfcNiSfJ+yY2pIlomp5PLTtYNWIBfiQQKBWfO8bLc0kTD2xiOH+FW9/d/qKJuB9pert9XnLL8/msXA=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/1924d8d702784c8a9c4bccc4a2d57dd2/services/565490a8abaa467f95950f6d2589d2ba/execute?api-version=2.0&format=swagger");

                // WARNING: The 'await' statement below can result in a deadlock
                // if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false)
                // so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)


                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);


                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    this.Dispatcher.Invoke(() =>
                    {
                        String first = string.Format("Result: {0}", result);
                       
                        txtleft.Text = "Score: " + (Double.Parse(first.Substring(49, 8))) + ""; 

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
            pushupSensor.Close();

        }
        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            MainWindow subWindow = new MainWindow();
            subWindow.Show();
            
        }
        private void ReturnToMain(object sender, RoutedEventArgs e)
        {
            MainWindow subWindow = new MainWindow();
            subWindow.Show();
            this.Close();
        }

    }
}
