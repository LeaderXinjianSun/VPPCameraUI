using System;
using HalconDotNet;

namespace ViewROI
{
	/// <summary>
	/// This class demonstrates one of the possible implementations for a 
	/// (simple) rectangularly shaped ROI. ROIRectangle1 inherits 
	/// from the base class ROI and implements (besides other auxiliary
	/// methods) all virtual methods defined in ROI.cs.
	/// Since a simple rectangle is defined by two data points, by the upper 
	/// left corner and the lower right corner, we use four values (row1/col1) 
	/// and (row2/col2) as class members to hold these positions at 
	/// any time of the program. The four corners of the rectangle can be taken
	/// as handles, which the user can use to manipulate the size of the ROI. 
	/// Furthermore, we define a midpoint as an additional handle, with which
	/// the user can grab and drag the ROI. Therefore, we declare NumHandles
	/// to be 5 and set the activeHandle to be 0, which will be the upper left
	/// corner of our ROI.
	/// </summary>
    [Serializable]
	public class ROIRectangle1 : ROI
	{
        //public string ROIName { get; set; }
        //public int ROIId { get; set; }
		public  double row1, col1;   // upper left
		public  double row2, col2;   // lower right 
		public  double midR, midC;   // midpoint 
        public void Move(double _Offset_Row, double _Offset_Col)
        {
            row1 = row1 + _Offset_Row;
            col1 = col1 + _Offset_Col;
            row2 = row2 + _Offset_Row;
            col2 = col2 + _Offset_Col;

            midR = ((row2 - row1) / 2) + row1;
            midC = ((col2 - col1) / 2) + col1;
        }
        //public bool SizeEnable=true;
        /// <summary>Constructor</summary>
        public ROIRectangle1()
		{

			NumHandles = 5; // 4 corner points + midpoint
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

			row1 = midR - 50;
			col1 = midC - 50;
			row2 = midR + 50;
			col2 = midC + 50;
		}
        public void createROI(double mrow1, double mcolumn1, double mrow2, double mcolumn2)
        {
            row1 = mrow1;
            row2 = mrow2;
            col1 = mcolumn1;
            col2 = mcolumn2;
            midC = ((mcolumn2 - mcolumn1) / 2) + mcolumn1;
            midR = ((mrow2 - mrow1) / 2) + mrow1;
        }
        public int windowsmallregionwidth = 5;//4边小矩形的大小
		/// <summary>Paints the ROI into the supplied window</summary>
		/// <param name="window">HALCON window</param>
		public override void draw(HalconDotNet.HWindow window)
		{
			window.DispRectangle1(row1, col1, row2, col2);
            if (SizeEnable && ShowRect)
            {
                int hrow, hcol, hw, hh;
                window.GetPart(out hrow, out hcol, out hh, out hw);
                int wrow, wcol, ww, wh;
                window.GetWindowExtents(out wrow, out wcol, out ww, out wh);

                double smallregionwidth = (hw-hcol) * windowsmallregionwidth / ww;
                double smallregionheight = (hh-hrow) * windowsmallregionwidth / wh;
                //焦点小矩形最小为5
                if (smallregionwidth < 5)
                    smallregionwidth = 5;
                if (smallregionheight < 5)
                    smallregionheight = 5;

                window.DispRectangle2(row1, col1, 0, smallregionheight, smallregionwidth);
                window.DispRectangle2(row1, col2, 0, smallregionheight, smallregionwidth);
                window.DispRectangle2(row2, col2, 0, smallregionheight, smallregionwidth);
                window.DispRectangle2(row2, col1, 0, smallregionheight, smallregionwidth);
                window.DispRectangle2(midR, midC, 0, smallregionheight, smallregionwidth);
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

			midR = ((row2 - row1) / 2) + row1;
			midC = ((col2 - col1) / 2) + col1;

			val[0] = HMisc.DistancePp(y, x, row1, col1); // upper left 
			val[1] = HMisc.DistancePp(y, x, row1, col2); // upper right 
			val[2] = HMisc.DistancePp(y, x, row2, col2); // lower right 
			val[3] = HMisc.DistancePp(y, x, row2, col1); // lower left 
			val[4] = HMisc.DistancePp(y, x, midR, midC); // midpoint 

			for (int i=0; i < NumHandles; i++)
			{
				if (val[i] < max)
				{
					max = val[i];
					activeHandleIdx = i;
				}
			}// end of for 
			return val[activeHandleIdx];
		}
        public override double distToClosestROI(double x, double y)
        {
            HTuple dismax, dismin = 0;
            HOperatorSet.DistancePr(getRegion(), y, x, out dismin, out dismax);
            //System.Diagnostics.Debug.Print(dismin + "," + dismax);
            return dismin;

            //HTuple dismax, dismin = 0;
            ////这算法根本不对啊！还不如自己算的
            ////HOperatorSet.DistancePr(getRegion(), y, x, out dismin, out dismax);
            //if (y >= row1 && y <= row2 && x >= col1 && x <= col2)
            //    dismin = 0;
            //else
            //    dismin = -1;
            ////System.Diagnostics.Debug.Print(dismin + "," + dismax);
            ////Console.WriteLine("r1:" + row1 + "c1:" + col1 + "r2:" + row2 + "c2:" + col2 + "y:" + y + "x:" + x+"dismin:"+dismin.D);
            //return dismin;
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
            //焦点小矩形最小为5
            if (smallregionwidth < 5)
                smallregionwidth = 5;
            if (smallregionheight < 5)
                smallregionheight = 5;

            switch (activeHandleIdx)
			{
				case 0:
                    window.DispRectangle2(row1, col1, 0, smallregionheight, smallregionwidth);
					break;
				case 1:
                    window.DispRectangle2(row1, col2, 0, smallregionheight, smallregionwidth);
					break;
				case 2:
                    window.DispRectangle2(row2, col2, 0, smallregionheight, smallregionwidth);
					break;
				case 3:
                    window.DispRectangle2(row2, col1, 0, smallregionheight, smallregionwidth);
					break;
				case 4:
                    window.DispRectangle2(midR, midC, 0, smallregionheight, smallregionwidth);
					break;
			}
		}

		/// <summary>Gets the HALCON region described by the ROI</summary>
		public override HRegion getRegion()
		{
			HRegion region = new HRegion();
			region.GenRectangle1(row1, col1, row2, col2);
			return region;
		}

		/// <summary>
		/// Gets the model information described by 
		/// the interactive ROI
		/// </summary> 
		public override HTuple getModelData()
		{
			return new HTuple(new double[] { row1, col1, row2, col2 });
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
			double len1, len2;
			double tmp;

			switch (activeHandleIdx)
			{
				case 0: // upper left 
					row1 = newY;
					col1 = newX;
					break;
				case 1: // upper right 
					row1 = newY;
					col2 = newX;
					break;
				case 2: // lower right 
					row2 = newY;
					col2 = newX;
					break;
				case 3: // lower left
					row2 = newY;
					col1 = newX;
					break;
				case 4: // midpoint 
					len1 = ((row2 - row1) / 2);
					len2 = ((col2 - col1) / 2);

					row1 = newY - len1;
					row2 = newY + len1;

					col1 = newX - len2;
					col2 = newX + len2;

					break;
			}

			if (row2 <= row1)
			{
				tmp = row1;
				row1 = row2;
				row2 = tmp;
			}

			if (col2 <= col1)
			{
				tmp = col1;
				col1 = col2;
				col2 = tmp;
			}

			midR = ((row2 - row1) / 2) + row1;
			midC = ((col2 - col1) / 2) + col1;

		}//end of method

        public override void show()
        {
            System.Diagnostics.Debug.Print(midR+","+midC+","+row1+","+col1+","+row2+","+col2); 
        }
        
	}//end of class
}//end of namespace
