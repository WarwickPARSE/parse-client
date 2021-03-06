﻿using System;
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
        public String instruction = "hello";

        private const int JOINT_WIDTH = 4;
        private const int HEAD_WIDTH = 10;
        private const int BONES_THICKNESS = 2;

        private Canvas canvas;

        private IDictionary<JointType, Ellipse> Joints;
        private IDictionary<SkeletonBones, Line> Bones;

        private double shoulder = 0;

        public int TrackingId { get; set; }

        public ActivityState Status { get; set; }

        private Brush color;

        public SkeletonFigure(Canvas c, Brush color)
        {
            this.canvas = c;
            this.canvas.Height = 426;
            this.canvas.Width = 543;

            this.color = color;

            InitJoints(color);
            InitBones(color);
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

        public void setColor(Brush c)
        {
            this.color = c;
            this.Erase();
            InitJoints(color);
            InitBones(color);
        }
        
        public void Update(JointType jointType, Point point, float distance = 2.0f)
        {
            instruction = "Move your arms into a horizontal position";

            if (Status == ActivityState.Erased)
            {
                InitJoints(color);
                InitBones(color);
            }

            var ellipse = Joints[jointType];

            // Scale the width
            ellipse.Width = ellipse.Height = JOINT_WIDTH * (2.0f / distance != 0 ? distance : 2.0f);

            Canvas.SetZIndex(ellipse, 5000 - (int)distance * 1000);

            // Center the ellipse to the point
            Canvas.SetTop(Joints[jointType], (point.Y - Joints[jointType].Height / 2));
            Canvas.SetLeft(Joints[jointType], (point.X - Joints[jointType].Width / 2));

            //double shoulder = 0;

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
                        //x`ZConsole.WriteLine(point.X + " " + point.Y);
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

        private void InitBones(Brush color)
        {
            Bones = new Dictionary<SkeletonBones, Line>
            {
                { new SkeletonBones(JointType.Head,JointType.ShoulderCenter), GenerateLine(color)},
                { new SkeletonBones(JointType.ShoulderCenter,JointType.ShoulderLeft), GenerateLine(color)},
                { new SkeletonBones(JointType.ShoulderCenter,JointType.ShoulderRight), GenerateLine(color)},
                { new SkeletonBones(JointType.ShoulderCenter,JointType.Spine), GenerateLine(color)},
                { new SkeletonBones(JointType.Spine,JointType.ShoulderLeft), GenerateLine(color)},
                { new SkeletonBones(JointType.Spine,JointType.ShoulderRight), GenerateLine(color)},
                { new SkeletonBones(JointType.Spine,JointType.HipCenter), GenerateLine(color)},
                { new SkeletonBones(JointType.Spine,JointType.HipLeft), GenerateLine(color)},
                { new SkeletonBones(JointType.Spine,JointType.HipRight), GenerateLine(color)},
                { new SkeletonBones(JointType.HipCenter,JointType.HipLeft), GenerateLine(color)},
                { new SkeletonBones(JointType.HipCenter,JointType.HipRight), GenerateLine(color)},

                { new SkeletonBones(JointType.ShoulderLeft,JointType.ElbowLeft), GenerateLine(color)},
                { new SkeletonBones(JointType.ElbowLeft,JointType.WristLeft), GenerateLine(color)},
                { new SkeletonBones(JointType.WristLeft,JointType.HandLeft), GenerateLine(color)},

                { new SkeletonBones(JointType.ShoulderRight,JointType.ElbowRight), GenerateLine(color)},
                { new SkeletonBones(JointType.ElbowRight,JointType.WristRight), GenerateLine(color)},
                { new SkeletonBones(JointType.WristRight,JointType.HandRight), GenerateLine(color)},

                { new SkeletonBones(JointType.HipRight,JointType.KneeRight), GenerateLine(color)},
                { new SkeletonBones(JointType.KneeRight,JointType.AnkleRight), GenerateLine(color)},
                { new SkeletonBones(JointType.AnkleRight,JointType.FootRight), GenerateLine(color)},

                { new SkeletonBones(JointType.HipLeft,JointType.KneeLeft), GenerateLine(color)},
                { new SkeletonBones(JointType.KneeLeft,JointType.AnkleLeft), GenerateLine(color)},
                { new SkeletonBones(JointType.AnkleLeft,JointType.FootLeft), GenerateLine(color)},
            };

            //foreach (Line line in Bones.Values)
            //    canvas.Children.Add(line);
        }

        private void InitJoints(Brush color)
        {
            Joints = new Dictionary<JointType, Ellipse>()
            {
                  { JointType.Head,             GenerateEllipse(color)},
                  { JointType.AnkleLeft,        GenerateEllipse(color)},
                  { JointType.AnkleRight,       GenerateEllipse(color)},
                  { JointType.ElbowLeft,        GenerateEllipse(color)},
                  { JointType.ElbowRight,       GenerateEllipse(color)},
                  { JointType.FootLeft,         GenerateEllipse(color)},
                  { JointType.FootRight,        GenerateEllipse(color)},
                  { JointType.HandLeft,         GenerateEllipse(color)},
                  { JointType.HandRight,        GenerateEllipse(color)},
                  { JointType.HipCenter,        GenerateEllipse(color)},
                  { JointType.HipLeft,          GenerateEllipse(color)},
                  { JointType.HipRight,         GenerateEllipse(color)},
                  { JointType.KneeLeft,         GenerateEllipse(color)},
                  { JointType.KneeRight,        GenerateEllipse(color)},
                  { JointType.ShoulderCenter,   GenerateEllipse(color)},
                  { JointType.ShoulderRight,    GenerateEllipse(color)},
                  { JointType.ShoulderLeft,     GenerateEllipse(color)},
                  { JointType.Spine,            GenerateEllipse(color)},
                  { JointType.WristLeft,        GenerateEllipse(color)},
                  { JointType.WristRight,       GenerateEllipse(color)},
            };

            //foreach (Ellipse ellipse in Joints.Values)
            //    canvas.Children.Add(ellipse);
        }

        private double skelDepth = 0;

        public void setDepth(double depth)
        {
            this.skelDepth = depth;
        }

        private Ellipse GenerateEllipse(Brush color)
        {
            int size = JOINT_WIDTH;
            
            var ellipse = new Ellipse() { Width = size, Height = size, Fill = color };
            canvas.Children.Add(ellipse);
            return ellipse;
        }

        private Line GenerateLine(Brush color)
        {
            var line = new Line() { StrokeThickness = BONES_THICKNESS, Fill = color, Stroke = color };
            canvas.Children.Add(line);
            return line;
        }

        #endregion
    }
}