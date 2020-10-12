using System;
using HalconDotNet;

namespace ViewROI
{
    [Serializable]
    public class ROIRegion:ROI
    {
        public HRegion mCurHRegion;

        public ROIRegion(HRegion r)
		{
            mCurHRegion = r;
        }
        public override HRegion getRegion()
        {
            return mCurHRegion;
        }
        public int windowsmallregionwidth = 5;//小矩形的大小
        public override void draw(HalconDotNet.HWindow window)
        {
            //window.SetColor("white");
            //window.SetLineStyle(0);
            //window.SetLineWidth(1);
            window.DispRegion(mCurHRegion);

            if (SizeEnable && ShowRect)
            {
                int hrow, hcol, hw, hh;
                window.GetPart(out hrow, out hcol, out hh, out hw);
                int wrow, wcol, ww, wh;
                window.GetWindowExtents(out wrow, out wcol, out ww, out wh);

                double smallregionwidth = (hw - hcol) * windowsmallregionwidth / ww;
                double smallregionheight = (hh - hrow) * windowsmallregionwidth / wh;

                double midR, midC;
                this.getRegion().AreaCenter(out midR, out midC);

                window.DispRectangle2(midR, midC, 0, smallregionheight, smallregionwidth);
            }
        }
        public override double distToClosestROI(double x, double y)
        {
            HTuple dismax, dismin = 0;
            HOperatorSet.DistancePr(getRegion(), y, x, out dismin, out dismax);
            //System.Diagnostics.Debug.Print(dismin + "," + dismax);
            return dismin;
        }
    }
}
