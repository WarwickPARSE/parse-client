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
        private Boolean HasMatches;
        private int Matches;
        private Image<Bgr, Byte> MappedImage;
        private string MappedImageUrl;
        private Image<Gray, Byte> OriginalImage;
        private string OriginalImageUrl;
        private long MatchTime;

        public SurfResults(Boolean match, int matches, Image<Gray, byte> Original, Image<Bgr, byte> Mapped, long Time)
        {
            HasMatches = match;
            Matches = matches;
            OriginalImage = Original;
            MappedImage = Mapped;
            MatchTime = Time;
        }

        public SurfResults(Boolean match, int matches, string OriginalURI, Image<Gray, byte> Original, string MappedURI, Image<Bgr, byte> Mapped, long Time)
        {
            HasMatches = match;
            Matches = matches;
            OriginalImage = Original;
            OriginalImageUrl = OriginalURI;
            MappedImage = Mapped;
            MappedImageUrl = MappedURI;
            MatchTime = Time;
        }

        public Boolean hasMatches()
        {
            return HasMatches;
        }

        public int getMatches()
        {
            return Matches;
        }

        public Image<Bgr,Byte> getMappedImage()
        {
            return MappedImage;
        }

        public string getMappedImageUrl()
        {
            return MappedImageUrl;
        }

        public Image<Gray, Byte> getOriginalImage()
        {
            return OriginalImage;
        }

        public string getOriginalImageUrl()
        {
            return OriginalImageUrl;
        }

        public long getMatchTime()
        {
            return MatchTime;
        }

        public void setURLs(string Model_Url, string Target_Url)
        {
            MappedImageUrl = Model_Url;
            OriginalImageUrl = Target_Url;
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("Target image: " + OriginalImageUrl);
            output.AppendLine("Model image: " + MappedImageUrl);
            output.AppendLine("Match? " + HasMatches);
            output.AppendLine("Number of matches: " + Matches);
            output.AppendLine("Time taken: " + MatchTime + "ms");
            return output.ToString();
        }
    }
}
