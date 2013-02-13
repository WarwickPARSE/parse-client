using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Speech.Synthesis;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using Microsoft.Kinect;
using HelixToolkit.Wpf;

namespace PARSE
{
    class SkeletonBones
    {
        public JointType joint1 { get; set; }
        public JointType joint2 { get; set; }
        public Point point1 { get; set; }
        public Point point2 { get; set; }

        public SkeletonBones(JointType j1, JointType j2)
        {
            joint1 = j1;
            joint2 = j2;
        }
    }

    public enum ActivityState
    {
        Active,
        Inactive,
        Erased
    }

    class SkeletonFigure
    {
        public String instruction = "hello";
        public bool labelsActive = false;

        private const int JOINT_WIDTH = 4;
        private const int HEAD_WIDTH = 10;
        private const int BONES_THICKNESS = 2;

        private Canvas canvas;
        private double waistDistanceMeasure;

        private IDictionary<JointType, Ellipse> Joints;
        private IDictionary<SkeletonBones, Line> Bones;
        private IDictionary<SkeletonBones, TextBlock> Labels;

        private double shoulder = 0;

        public int TrackingId { get; set; }

        public ActivityState Status { get; set; }

        public SkeletonFigure(Canvas c)
        {
            this.canvas = c;
            this.canvas.Height = 375;
            this.canvas.Width = 600;

            InitJoints();
            InitBones();
            InitLabels();

            System.Diagnostics.Debug.WriteLine("Created the skeleton");
        }

        public void Erase()
        {
            Status = ActivityState.Erased;
            foreach (Ellipse ellipse in Joints.Values)
            {
                canvas.Children.Remove(ellipse);
            }

            foreach (Line line in Bones.Values)
            {
                canvas.Children.Remove(line);
            }

            foreach (TextBlock text in Labels.Values)
            {
                canvas.Children.Remove(text);
            }

            Joints.Clear();
            Bones.Clear();
            Labels.Clear();
        }

        public void Update(JointType jointType, Point point, float distance = 2.0f)
        {
            instruction = "Move your arms into a horizontal position";

            if (Status == ActivityState.Erased)
            {
                InitJoints();
                InitBones();
                InitLabels();
            }

            var ellipse = Joints[jointType];

            // Scale the width
            ellipse.Width = ellipse.Height = JOINT_WIDTH * (2.0f / distance != 0 ? distance : 2.0f);

            Canvas.SetZIndex(ellipse, 5000 - (int)distance * 1000);

            // Center the ellipse to the point
            Canvas.SetTop(Joints[jointType], (point.Y - Joints[jointType].Height / 2));
            Canvas.SetLeft(Joints[jointType], (point.X - Joints[jointType].Width / 2));

            //double shoulder = 0;

            //Set text labels for skeleton from identification subroutine


            foreach (SkeletonBones bones in Labels.Keys.Where(b => b.joint1 == jointType || b.joint2 == jointType))
            {
                try
                {
                    Canvas.SetTop(Labels[bones], point.Y);
                    Canvas.SetLeft(Labels[bones], point.X);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }

            }
            foreach (SkeletonBones bones in Bones.Keys.Where(b => b.joint1 == jointType || b.joint2 == jointType))
            {
                var line = Bones[bones];

                if (bones.joint1 == jointType)
                {
                    line.X1 = point.X;
                    line.Y1 = point.Y;

                    if (bones.joint1 == JointType.WristLeft || bones.joint1 == JointType.WristRight)
                    {
                        if ((point.Y <= shoulder+5) && (point.Y >= shoulder-5) && (shoulder!=0))
                        {
                            //Console.WriteLine("stay there mate" + point.Y);
                            instruction = "Hold this pose!";
                        }
                        
                    }

                    if (bones.joint1 == JointType.ShoulderLeft || bones.joint1 == JointType.ShoulderRight)
                    {
                        shoulder = point.Y;
                        //Console.WriteLine("shoulder " + point.X + " " + point.Y);
                    }
                }
                else
                {
                    line.X2 = point.X;
                    line.Y2 = point.Y;

                }
            }
            
        }

        #region Private methods

        private void InitBones()
        {
            Bones = new Dictionary<SkeletonBones, Line>
            {
                { new SkeletonBones(JointType.Head,JointType.ShoulderCenter), GenerateLine()},
                { new SkeletonBones(JointType.ShoulderCenter,JointType.ShoulderLeft), GenerateLine()},
                { new SkeletonBones(JointType.ShoulderCenter,JointType.ShoulderRight), GenerateLine()},
                { new SkeletonBones(JointType.ShoulderCenter,JointType.Spine), GenerateLine()},
                { new SkeletonBones(JointType.Spine,JointType.ShoulderLeft), GenerateLine()},
                { new SkeletonBones(JointType.Spine,JointType.ShoulderRight), GenerateLine()},
                { new SkeletonBones(JointType.Spine,JointType.HipCenter), GenerateLine()},
                { new SkeletonBones(JointType.Spine,JointType.HipLeft), GenerateLine()},
                { new SkeletonBones(JointType.Spine,JointType.HipRight), GenerateLine()},
                { new SkeletonBones(JointType.HipCenter,JointType.HipLeft), GenerateLine()},
                { new SkeletonBones(JointType.HipCenter,JointType.HipRight), GenerateLine()},
                
                { new SkeletonBones(JointType.ShoulderLeft,JointType.ElbowLeft), GenerateLine()},
                { new SkeletonBones(JointType.ElbowLeft,JointType.WristLeft), GenerateLine()},
                { new SkeletonBones(JointType.WristLeft,JointType.HandLeft), GenerateLine()},
                
                { new SkeletonBones(JointType.ShoulderRight,JointType.ElbowRight), GenerateLine()},
                { new SkeletonBones(JointType.ElbowRight,JointType.WristRight), GenerateLine()},
                { new SkeletonBones(JointType.WristRight,JointType.HandRight), GenerateLine()},
                
                { new SkeletonBones(JointType.HipRight,JointType.KneeRight), GenerateLine()},
                { new SkeletonBones(JointType.KneeRight,JointType.AnkleRight), GenerateLine()},
                { new SkeletonBones(JointType.AnkleRight,JointType.FootRight), GenerateLine()},
                
                { new SkeletonBones(JointType.HipLeft,JointType.KneeLeft), GenerateLine()},
                { new SkeletonBones(JointType.KneeLeft,JointType.AnkleLeft), GenerateLine()},
                { new SkeletonBones(JointType.AnkleLeft,JointType.FootLeft), GenerateLine()},
            };

            //foreach (Line line in Bones.Values)
            //canvas.Children.Add(line);
        }

        private void InitJoints()
        {
            Joints = new Dictionary<JointType, Ellipse>()
            {
                  { JointType.Head,             GenerateEllipse(50)},
                  { JointType.AnkleLeft,        GenerateEllipse()},
                  { JointType.AnkleRight,       GenerateEllipse()},
                  { JointType.ElbowLeft,        GenerateEllipse()},
                  { JointType.ElbowRight,       GenerateEllipse()},
                  { JointType.FootLeft,         GenerateEllipse()},
                  { JointType.FootRight,        GenerateEllipse()},
                  { JointType.HandLeft,         GenerateEllipse()},
                  { JointType.HandRight,        GenerateEllipse()},
                  { JointType.HipCenter,        GenerateEllipse()},
                  { JointType.HipLeft,          GenerateEllipse()},
                  { JointType.HipRight,         GenerateEllipse()},
                  { JointType.KneeLeft,         GenerateEllipse()},
                  { JointType.KneeRight,        GenerateEllipse()},
                  { JointType.ShoulderCenter,   GenerateEllipse()},
                  { JointType.ShoulderRight,    GenerateEllipse()},
                  { JointType.ShoulderLeft,     GenerateEllipse()},
                  { JointType.Spine,            GenerateEllipse()},
                  { JointType.WristLeft,        GenerateEllipse()},
                  { JointType.WristRight,       GenerateEllipse()},
            };

            //foreach (Ellipse ellipse in Joints.Values)
            //    canvas.Children.Add(ellipse);
        }

        private void InitLabels()
        {
            Labels = new Dictionary<SkeletonBones, TextBlock>
            {
                { new SkeletonBones(JointType.Head,JointType.ShoulderCenter), GenerateLabel("Neck")},
                { new SkeletonBones(JointType.ShoulderCenter,JointType.ShoulderLeft), GenerateLabel("")},
                { new SkeletonBones(JointType.ShoulderCenter,JointType.ShoulderRight), GenerateLabel("")},
                { new SkeletonBones(JointType.ShoulderCenter,JointType.Spine), GenerateLabel("Chest")},
                { new SkeletonBones(JointType.Spine,JointType.ShoulderLeft), GenerateLabel("")},
                { new SkeletonBones(JointType.Spine,JointType.ShoulderRight), GenerateLabel("")},
                { new SkeletonBones(JointType.Spine,JointType.HipCenter), GenerateLabel("Waist")},
                { new SkeletonBones(JointType.Spine,JointType.HipLeft), GenerateLabel("")},
                { new SkeletonBones(JointType.Spine,JointType.HipRight), GenerateLabel("")},
                { new SkeletonBones(JointType.HipCenter,JointType.HipLeft), GenerateLabel("")},
                { new SkeletonBones(JointType.HipCenter,JointType.HipRight), GenerateLabel("")},
                
                { new SkeletonBones(JointType.ShoulderLeft,JointType.ElbowLeft), GenerateLabel("Upper Left Arm")},
                { new SkeletonBones(JointType.ElbowLeft,JointType.WristLeft), GenerateLabel("Forearm")},
                { new SkeletonBones(JointType.WristLeft,JointType.HandLeft), GenerateLabel("Forearm left")},
                
                { new SkeletonBones(JointType.ShoulderRight,JointType.ElbowRight), GenerateLabel("Upper Right Arm")},
                { new SkeletonBones(JointType.ElbowRight,JointType.WristRight), GenerateLabel("Forearm right")},
                { new SkeletonBones(JointType.WristRight,JointType.HandRight), GenerateLabel("")},
                
                { new SkeletonBones(JointType.HipRight,JointType.KneeRight), GenerateLabel("Right Thigh")},
                { new SkeletonBones(JointType.KneeRight,JointType.AnkleRight), GenerateLabel("Right Calf")},
                { new SkeletonBones(JointType.AnkleRight,JointType.FootRight), GenerateLabel("")},
                
                { new SkeletonBones(JointType.HipLeft,JointType.KneeLeft), GenerateLabel("Left Thigh")},
                { new SkeletonBones(JointType.KneeLeft,JointType.AnkleLeft), GenerateLabel("Left Calf")},
                { new SkeletonBones(JointType.AnkleLeft,JointType.FootLeft), GenerateLabel("")},
            };

            //foreach (Ellipse ellipse in Joints.Values)
            //    canvas.Children.Add(ellipse);
        }

        private Ellipse GenerateEllipse(int size = JOINT_WIDTH)
        {
            var ellipse = new Ellipse() { Width = size, Height = size, Fill = Brushes.LightBlue };
            canvas.Children.Add(ellipse);
            return ellipse;
        }

        private Line GenerateLine()
        {
            var line = new Line() { StrokeThickness = BONES_THICKNESS, Fill = Brushes.Blue, Stroke = Brushes.Blue };
            canvas.Children.Add(line);
            return line;
        }

        private TextBlock GenerateLabel(String bonelabel)
        {
            var text = new TextBlock() { Text = bonelabel, Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#000000"), FontSize = 14 };
            canvas.Children.Add(text);
            text.MouseDown += new MouseButtonEventHandler(text_MouseDown);
            text.MouseUp += new MouseButtonEventHandler(text_MouseUp);
            return text;
        }

        void text_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.Write("Text clicked");
            ((TextBlock)sender).Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#73B1B7");
        }

        void text_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        #endregion
    }
}
