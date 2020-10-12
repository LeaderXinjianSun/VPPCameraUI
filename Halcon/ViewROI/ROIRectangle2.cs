using System;
using HalconDotNet;



namespace ViewROI
{
    [Serializable]

    /// <summary>
    /// This class demonstrates one of the possible implementations for a 
    /// (simple) rectangularly shaped ROI. To create this rectangle we use 
    /// a center point (midR, midC), an orientation 'phi' and the half 
    /// edge lengths 'length1' and 'length2', similar to the HALCON 
    /// operator gen_rectangle2(). 
    /// The class ROIRectangle2 inherits from the base class ROI and 
    /// implements (besides other auxiliary methods) all virtual methods 
    /// defined in ROI.cs.
    /// </summary>
    public class ROIRectangle2 : ROI
	{

		/// <summary>Half length of the rectangle side, perpendicular to phi</summary>
		public double length1;

		/// <summary>Half length of the rectangle side, in direction of phi</summary>
        public double length2;

		/// <summary>Row coordinate of midpoint of the rectangle</summary>
        public double midR;

		/// <summary>Column coordinate of midpoint of the rectangle</summary>
        public double midC;

		/// <summary>Orientation of rectangle defined in radians.</summary>
        public double phi;

		//auxiliary variables
		HTuple rowsInit;
		HTuple colsInit;
		HTuple rows;
		HTuple cols;

		HHomMat2D hom2D, tmp;

		/// <summary>Constructor</summary>
		public ROIRectangle2()
		{
			NumHandles = 6; // 4 corners +  1 midpoint + 1 rotationpoint			
			activeHandleIdx = 4;
		}

		/// <summary>Creates a new ROI instance at the mouse position</summary>
		/// <param name="midX">
		/// x (=column) coordinate for interactive ROI
		/// </param>
		/// <param name="midY">
		/// y (=row) coordinate for interactive ROI
		/// </param>
		public override void createROI(double midX, double midY)
		{
			midR = midY;
			midC = midX;

			length1 = 100;
			length2 = 50;

			phi = 0.0;

			rowsInit = new HTuple(new double[] {-1.0, -1.0, 1.0, 
												   1.0,  0.0, 0.0 });
			colsInit = new HTuple(new double[] {-1.0, 1.0,  1.0, 
												  -1.0, 0.0, 0.6 });
			//order        ul ,  ur,   lr,  ll,   mp, arrowMidpoint
			hom2D = new HHomMat2D();
			tmp = new HHomMat2D();

			updateHandlePos();
		}
        public void createROI(double mrow,double mcolumn,double mphi,double mlength1,double mlength2)
        {
            midR = mrow;
            midC = mcolumn;

            length1 = mlength1;
            length2 = mlength2;

            phi = mphi;

            rowsInit = new HTuple(new double[] {-1.0, -1.0, 1.0, 
												   1.0,  0.0, 0.0 });
            colsInit = new HTuple(new double[] {-1.0, 1.0,  1.0, 
												  -1.0, 0.0, 0.6 });
            //order        ul ,  ur,   lr,  ll,   mp, arrowMidpoint
            hom2D = new HHomMat2D();
            tmp = new HHomMat2D();

            updateHandlePos();
        }
        public int windowsmallregionwidth = 5;//4边小矩形的大小
		/// <summary>Paints the ROI into the supplied window</summary>
		/// <param name="window">HALCON window</param>
		public override void draw(HalconDotNet.HWindow window)
		{
			window.DispRectangle2(midR, midC, phi, length1, length2);
            if (SizeEnable && ShowRect)
            {
                int hrow, hcol, hw, hh;
                window.GetPart(out hrow, out hcol, out hh, out hw);
                int wrow, wcol, ww, wh;
                window.GetWindowExtents(out wrow, out wcol, out ww, out wh);

                double smallregionwidth = (hw - hcol) * windowsmallregionwidth / ww;
                double smallregionheight = (hh - hrow) * windowsmallregionwidth / wh;

                for (int i = 0; i < NumHandles; i++)
                    window.DispRectangle2(rows[i].D, cols[i].D, phi, smallregionwidth, smallregionheight);

                window.DispArrow(midR, midC, midR + (Math.Sin(-phi) * length1 * 1.2),
                    midC + (Math.Cos(-phi) * length1 * 1.2), 2.0);
            }
		}

		/// <summary> 
		/// Returns the distance of the ROI handle being
		/// closest to the image point(x,y)
		/// </summary>
		/// <param name="x">x (=column) coordinate</param>
		/// <param name="y">y (=row) coordinate</param>
		/// <returns> 
		/// Distance of the closest ROI handle.
		/// </returns>
		public override double distToClosestHandle(double x, double y)
		{
			double max = 10000;
			double [] val = new double[NumHandles];


			for (int i=0; i < NumHandles; i++)
				val[i] = HMisc.DistancePp(y, x, rows[i].D, cols[i].D);

			for (int i=0; i < NumHandles; i++)
			{
				if (val[i] < max)
				{
					max = val[i];
					activeHandleIdx = i;
				}
			}
			return val[activeHandleIdx];
		}
        public override double distToClosestROI(double x, double y)
        {
            HTuple dismax, dismin = 0;
            HOperatorSet.DistancePr(getRegion(), y, x, out dismin, out dismax);
            //System.Diagnostics.Debug.Print(dismin + "," + dismax);
            return dismin;
        }
		/// <summary> 
		/// Paints the active handle of the ROI object into the supplied window
		/// </summary>
		/// <param name="window">HALCON window</param>
		public override void displayActive(HalconDotNet.HWindow window)
        {
            if (!SizeEnable || !ShowRect)
                return;

            int hrow, hcol, hw, hh;
            window.GetPart(out hrow, out hcol, out hh, out hw);
            int wrow, wcol, ww, wh;
            window.GetWindowExtents(out wrow, out wcol, out ww, out wh);

            double smallregionwidth = (hw - hcol) * windowsmallregionwidth / ww;
            double smallregionheight = (hh - hrow) * windowsmallregionwidth / wh;

            window.DispRectangle2(rows[activeHandleIdx].D,
								  cols[activeHandleIdx].D,
								  phi, smallregionwidth, smallregionheight);

			if (activeHandleIdx == 5)
				window.DispArrow(midR, midC,
								 midR + (Math.Sin(-phi) * length1 * 1.2),
								 midC + (Math.Cos(-phi) * length1 * 1.2),
								 2.0);
		}


		/// <summary>Gets the HALCON region described by the ROI</summary>
		public override HRegion getRegion()
		{
			HRegion region = new HRegion();
			region.GenRectangle2(midR, midC, phi, length1, length2);
			return region;
		}

		/// <summary>
		/// Gets the model information described by 
		/// the interactive ROI
		/// </summary> 
		public override HTuple getModelData()
		{
			return new HTuple(new double[] { midR, midC, phi, length1, length2 });
		}

		/// <summary> 
		/// Recalculates the shape of the ROI instance. Translation is 
		/// performed at the active handle of the ROI object 
		/// for the image coordinate (x,y)
		/// </summary>
		/// <param name="newX">x mouse coordinate</param>
		/// <param name="newY">y mouse coordinate</param>
		public override void moveByHandle(double newX, double newY)
        {
            if (SizeEnable == false)
                return;
			double vX, vY, x=0, y=0;

			switch (activeHandleIdx)
			{
				case 0:
				case 1:
				case 2:
				case 3:
					tmp = hom2D.HomMat2dInvert();
					x = tmp.AffineTransPoint2d(newX, newY, out y);

					length2 = Math.Abs(y);
					length1 = Math.Abs(x);

					checkForRange(x, y);
					break;
				case 4:
					midC = newX;
					midR = newY;
					break;
				case 5:
					vY = newY - rows[4].D;
					vX = newX - cols[4].D;
					phi = -Math.Atan2(vY, vX);
					break;
			}
			updateHandlePos();
		}//end of method


		/// <summary>
		/// Auxiliary method to recalculate the contour points of 
		/// the rectangle by transforming the initial row and 
		/// column coordinates (rowsInit, colsInit) by the updated
		/// homography hom2D
		/// </summary>
		private void updateHandlePos()
		{
			hom2D.HomMat2dIdentity();
			hom2D = hom2D.HomMat2dTranslate(midC, midR);
			hom2D = hom2D.HomMat2dRotateLocal(-phi);
			tmp = hom2D.HomMat2dScaleLocal(length1, length2);
			cols = tmp.AffineTransPoint2d(colsInit, rowsInit, out rows);
		}


		/* This auxiliary method checks the half lengths 
		 * (length1, length2) using the coordinates (x,y) of the four 
		 * rectangle corners (handles 0 to 3) to avoid 'bending' of 
		 * the rectangular ROI at its midpoint, when it comes to a
		 * 'collapse' of the rectangle for length1=length2=0.
		 * */
		private void checkForRange(double x, double y)
		{
			switch (activeHandleIdx)
			{
				case 0:
					if ((x < 0) && (y < 0))
						return;
					if (x >= 0) length1 = 0.01;
					if (y >= 0) length2 = 0.01;
					break;
				case 1:
					if ((x > 0) && (y < 0))
						return;
					if (x <= 0) length1 = 0.01;
					if (y >= 0) length2 = 0.01;
					break;
				case 2:
					if ((x > 0) && (y > 0))
						return;
					if (x <= 0) length1 = 0.01;
					if (y <= 0) length2 = 0.01;
					break;
				case 3:
					if ((x < 0) && (y > 0))
						return;
					if (x >= 0) length1 = 0.01;
					if (y <= 0) length2 = 0.01;
					break;
				default:
					break;
			}
		}
	}//end of class
}//end of namespace
