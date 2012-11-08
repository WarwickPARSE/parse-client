using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

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
        private const int JOINT_WIDTH = 4;
        private const int HEAD_WIDTH = 10;
        private const int BONES_THICKNESS = 2;

        private Canvas canvas;

        private IDictionary<JointType, Ellipse> Joints;
        private IDictionary<SkeletonBones, Line> Bones;

        public int TrackingId { get; set; }

        public ActivityState Status { get; set; }

        public SkeletonFigure(Canvas c)
        {
            this.canvas = c;
            this.canvas.Height = 426;
            this.canvas.Width = 543;

            InitJoints();
            InitBones();
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

            Joints.Clear();
            Bones.Clear();
        }

        public void Update(JointType jointType, Point point, float distance = 2.0f)
        {
            if (Status == ActivityState.Erased)
            {
                InitJoints();
                InitBones();
            }

            var ellipse = Joints[jointType];

            // Scale the width
            ellipse.Width = ellipse.Height = JOINT_WIDTH * (2.0f / distance != 0 ? distance : 2.0f);

            Canvas.SetZIndex(ellipse, 5000 - (int)distance * 1000);

            // Center the ellipse to the point
            Canvas.SetTop(Joints[jointType], (point.Y - Joints[jointType].Height / 2));
            Canvas.SetLeft(Joints[jointType], (point.X - Joints[jointType].Width / 2));

            foreach (SkeletonBones bones in Bones.Keys.Where(b => b.joint1 == jointType || b.joint2 == jointType))
            {
                var line = Bones[bones];
                if (bones.joint1 == jointType)
                {
                    line.X1 = point.X;
                    line.Y1 = point.Y;
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
            //    canvas.Children.Add(line);
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

        #endregion
    }
}
