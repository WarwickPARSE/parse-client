using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.GPU;
namespace PARSE.Recognition
{
    class SurfResults
    {
        private Boolean Match;
        private int Matches;
        private Image<Bgr, Byte> MappedImage;
        private Image<Gray, Byte> OriginalImage;
        private long MatchTime;

        public SurfResults(Boolean match, int matches, Image<Gray, byte> Original, Image<Bgr, byte> Mapped, long Time)
        {
            Match = match;
            Matches = matches;
            OriginalImage = Original;
            MappedImage = Mapped;
            MatchTime = Time;
        }

        public Boolean isMatch()
        {
            return Match;
        }

        public int getMatches()
        {
            return Matches;
        }

        public Image<Bgr,Byte> getMappedImage()
        {
            return MappedImage;
        }

        public Image<Gray, Byte> getOriginalImage()
        {
            return OriginalImage;
        }

        public long getMatchTime()
        {
            return MatchTime;
        }

    }
}
