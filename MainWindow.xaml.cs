//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using Microsoft.Samples.Kinect.ControlsBasics;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Globalization;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;
        private bool player1=true;

        private List<SkeletonPoint> m_skeletonCalibPoints = new List<SkeletonPoint>(); //3d skeleton points

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        private Skeleton[] skeletons;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;
        private TicTacToe ticTacToe = new TicTacToe();
        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);



        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        //data for calibration
        private bool InCalibration = false;
        private bool gestureActive = false; //detect if the gesture is still active or if it's new.
        private PartialCalibrationClass callibrator;
        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }
        private void drawPlayfield(TicTacToe tic,DrawingContext drawingContext)
        {
            
            for (int i = 1; i < 3; i++)
            {
                drawingContext.DrawLine(new Pen(Brushes.Red, 2), new Point(0, RenderHeight*i / 3), new Point(RenderWidth, RenderHeight*i / 3));
                drawingContext.DrawLine(new Pen(Brushes.Red, 2), new Point(RenderWidth*i/3,0), new Point(RenderWidth*i/3, RenderHeight));
                
            }
           

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (tic.getField(i, j) == Field.X)
                    {
                        drawingContext.DrawLine(new Pen(Brushes.Red, 2), new Point(RenderWidth * i / 3, RenderHeight * j / 3), new Point(RenderWidth * (i + 1) / 3, RenderHeight * (j + 1) / 3));
                        drawingContext.DrawLine(new Pen(Brushes.Red, 2), new Point(RenderWidth * (i + 1) / 3, RenderHeight * j / 3), new Point(RenderWidth * i / 3, RenderHeight * (j + 1) / 3));
                    }
                    else if (tic.getField(i, j) == Field.O)
                    {
                        drawingContext.DrawEllipse(null, new Pen(Brushes.Red,2), new Point(RenderWidth * (i + 0.5) / 3, RenderHeight * (j + 0.5) / 3), RenderWidth / 6, RenderHeight / 6);
                    }
                }
              
               

            }
        }


      
        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            /*Debug.Print(ticTacToe.setField(0, 0, Field.X).ToString()+" wuuut" );
          Debug.Print(ticTacToe.setField(0, 2, Field.O).ToString());
          Debug.Print(ticTacToe.setField(0, 1, Field.O).ToString());
          Debug.Print(ticTacToe.setField(1, 1, Field.X).ToString());
          Debug.Print(ticTacToe.setField(0, 0, Field.O).ToString());
          Debug.Print(ticTacToe.setField(1, 0, Field.O).ToString());
            
            Debug.Print(ticTacToe.setField(2, 2, Field.X).ToString());*/
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;

                    break;
                }

            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    this.skeletons = new Skeleton[0];
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
               
                if (skeletons.Length != 0)
                {

                    //when callibrating we need to check for the gesture for calibration.
                    if (callibrator == null)
                    {
                        calibPointDetection();
                        drawCalibPoint(dc);
                    }
                    else {
                        drawPlayfield(ticTacToe, dc);
                    }

                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);
                     
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            if (callibrator != null)    //if calibrated.
                            {
                                Point player = callibrator.kinectToProjectionPoint(skel.Joints[JointType.Spine].Position);

                                //Debug.Print("x: " + player.X.ToString() + " y: " + player.Y.ToString());
                                dc.DrawEllipse(centerPointBrush, trackedBonePen, player, 5, 5);
                            }
                            else {  //we only draw bones while calibrating
                                this.DrawBonesAndJoints(skel, dc);
                            }
                            ///Debug.WriteLineIf(true,skel.Joints[JointType.HipCenter].Position.Z);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                
            }
            
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }


        //this function detects if the left hand is above the left shoulder. 
        private SkeletonGesture detectLeftHandAboveShoulder() {
            bool hasGesture = false;
            int skeletonindex = 0;
            bool status = false;
            foreach (Skeleton skeleton in skeletons) { 
                SkeletonPoint shoulderLeft = skeleton.Joints[JointType.ShoulderLeft].Position;
                SkeletonPoint handLeft = skeleton.Joints[JointType.HandLeft].Position;

                bool inGesture = shoulderLeft.Y < handLeft.Y;
                if (inGesture)
                {
                    hasGesture = true;
                    break;
                }
                skeletonindex++;
                
            }
            if (hasGesture && !gestureActive)
            {
                Debug.WriteLine("New gesture found");
                gestureActive = true;
                status = true;
            }
            else if (!hasGesture && gestureActive){
                Debug.WriteLine("gesture ended");
                gestureActive = false;
            }
            return new SkeletonGesture(status, skeletonindex);
        }




        private void drawCalibPoint(DrawingContext dc) {
            int pointSize = 30;
            dc.DrawText(
       new FormattedText("Stand on the yellow square and \nput your left hand up \nto calibrate point",
          CultureInfo.GetCultureInfo("en-us"),FlowDirection,
          new Typeface("Verdana"),
          36, System.Windows.Media.Brushes.LightBlue),
          new System.Windows.Point(20, RenderHeight/3));
            Point p = new Point(0,0);
            if (m_skeletonCalibPoints.Count == 0) {
                p.X = 0;
                p.Y = 0;
            }
            else if (m_skeletonCalibPoints.Count == 1)
            {
                p.X = 0;
                p.Y = RenderHeight- pointSize;
            }
            else if (m_skeletonCalibPoints.Count == 2)
            {
                p.X = RenderWidth - pointSize;
                p.Y = RenderHeight - pointSize;
            }
            else if (m_skeletonCalibPoints.Count == 3)
            {
                p.X = RenderWidth - pointSize;
                p.Y = 0;
            }

            dc.DrawRectangle(Brushes.Yellow, null, new Rect(p.X, p.Y, pointSize, pointSize));
          

        }



        //resets calibration 
        private void startCalibration(object sender, RoutedEventArgs e) {
            callibrator = null; //set the callibrator to 0
            m_skeletonCalibPoints = new List<SkeletonPoint>();  //reset the calibration points
        }
        
        //in this function we check if a skeleton is making a gesture, if so we add it's position to the calib points.
        private void calibPointDetection()
        {
            if (m_skeletonCalibPoints.Count < 4)
            {
                if (skeletons.Length != 0)
                {
                    SkeletonGesture gesture = detectLeftHandAboveShoulder();
                    if (gesture.hasGesture()) { //check if user is making a gesture
                        m_skeletonCalibPoints.Add(skeletons[gesture.getSkeletonIndex()].Joints[JointType.Spine].Position);
                        Debug.WriteLine(m_skeletonCalibPoints.Count);
                        //Debug.Print("aaaaaaa");
                    }
                }
            }
            //if we have 4 points make calibration
            if (m_skeletonCalibPoints.Count == 4)
            {
                List<Point> m_calibpoints = new List<Point>();
                m_calibpoints.Add(new Point(0, 0));
                m_calibpoints.Add(new Point(0, RenderHeight));
                m_calibpoints.Add(new Point(RenderWidth, RenderHeight));
                m_calibpoints.Add(new Point(RenderWidth, 0));
                callibrator = new PartialCalibrationClass(sensor, m_skeletonCalibPoints, m_calibpoints);
                //Debug.Print("Calibration done");
            }
            
        }





        private void makeMove(object sender, RoutedEventArgs e)
        {
            if (skeletons.Length != 0)
            {
                if (callibrator != null)
                {
                    Point p = callibrator.kinectToProjectionPoint(skeletons[0].Joints[JointType.Spine].Position);
                    int x = (int)(p.X *3/ RenderWidth);
                    int y = (int)(p.Y *3/ RenderHeight);
                    Debug.Print(" x: " + x + " y: " + y);
                    if (player1) {
                        if (ticTacToe.setField(x, y, Field.X).Item2)
                        {
                            player1 = false;
                            
                        }

                    }
                    else
                    {
                        if (ticTacToe.setField(x, y, Field.O).Item2)
                        {
                            player1 = true;

                        }
                    }
                    //Debug.Print("bbbbbbb");}
                }
            }
        }
    }
}