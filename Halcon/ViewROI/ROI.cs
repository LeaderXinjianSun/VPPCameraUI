using System;
using HalconDotNet;

namespace ViewROI
{

    /// <summary>
    /// This class is a base class containing virtual methods for handling
    /// ROIs. Therefore, an inheriting class needs to define/override these
    /// methods to provide the ROIController with the necessary information on
    /// its (= the ROIs) shape and position. The example project provides 
    /// derived ROI shapes for rectangles, lines, circles, and circular arcs.
    /// To use other shapes you must derive a new class from the base class 
    /// ROI and implement its methods.
    /// </summary>    
    [Serializable]
    public class ROI
    {
        public HRegion CurRegion { get; set; }
        public string ROIName { get; set; }
        [NonSerialized]
        public string mROIResult;
        public string ROIResult { get { return mROIResult; } set { mROIResult = value; } }
        //[NonSerialized]
        public string mROIInfo;
        public string ROIInfo { get { return mROIInfo; } set { mROIInfo = value; } }
        [NonSerialized]
        public string mROIColor;
        public string ROIColor
        {
            get { return mROIColor; }
            set { mROIColor = value; }
        }
        public int ROIId { get; set; }
        private bool mSizeEnable = true;
        public bool SizeEnable { get { return mSizeEnable; } set { mSizeEnable = value; } }
        private bool mShowRect = false;
        public bool ShowRect { get { return mShowRect; } set { mShowRect = value; } }
        public virtual void show() { }
		// class members of inheriting ROI classes
		protected int   NumHandles;
		protected int	activeHandleIdx;

		/// <summary>
		/// Flag to define the ROI to be 'positive' or 'negative'.
		/// </summary>
		protected int     OperatorFlag;

        /// <summary>Parameter to define the line style of the ROI.</summary>
        //[NonSerialized]
        public HTuple flagLineStyle = new HTuple();//halcon13修改=new htuple(),否则报null错误

		/// <summary>Constant for a positive ROI flag.</summary>
		public const int  POSITIVE_FLAG	= ROIController.MODE_ROI_POS;

		/// <summary>Constant for a negative ROI flag.</summary>
		public const int  NEGATIVE_FLAG	= ROIController.MODE_ROI_NEG;

		public const int  ROI_TYPE_LINE       = 10;
		public const int  ROI_TYPE_CIRCLE     = 11;
		public const int  ROI_TYPE_CIRCLEARC  = 12;
		public const int  ROI_TYPE_RECTANGLE1 = 13;
		public const int  ROI_TYPE_RECTANGLE2 = 14;
        public const int  ROI_TYPE_REGION     = 15;

        [NonSerialized]
		protected HTuple  posOperation = new HTuple();
        [NonSerialized]
		protected HTuple  negOperation = new HTuple(new int[] { 2, 2 });

		/// <summary>Constructor of abstract ROI class.</summary>
		public ROI() { }

		/// <summary>Creates a new ROI instance at the mouse position.</summary>
		/// <param name="midX">
		/// x (=column) coordinate for ROI
		/// </param>
		/// <param name="midY">
		/// y (=row) coordinate for ROI
		/// </param>
		public virtual void createROI(double midX, double midY) { }

		/// <summary>Paints the ROI into the supplied window.</summary>
		/// <param name="window">HALCON window</param>
		public virtual void draw(HalconDotNet.HWindow window) { }

		/// <summary> 
		/// Returns the distance of the ROI handle being
		/// closest to the image point(x,y)
		/// </summary>
		/// <param name="x">x (=column) coordinate</param>
		/// <param name="y">y (=row) coordinate</param>
		/// <returns> 
		/// Distance of the closest ROI handle.
		/// </returns>
		public virtual double distToClosestHandle(double x, double y)
		{
			return 0.0;
		}
        //判断鼠标是否在ROI内
        public virtual double distToClosestROI(double x,double y)
        {
            return -1.0;
        }
		/// <summary> 
		/// Paints the active handle of the ROI object into the supplied window. 
		/// </summary>
		/// <param name="window">HALCON window</param>
		public virtual void displayActive(HalconDotNet.HWindow window) { }

		/// <summary> 
		/// Recalculates the shape of the ROI. Translation is 
		/// performed at the active handle of the ROI object 
		/// for the image coordinate (x,y).
		/// </summary>
		/// <param name="x">x (=column) coordinate</param>
		/// <param name="y">y (=row) coordinate</param>
		public virtual void moveByHandle(double x, double y) { }

		/// <summary>Gets the HALCON region described by the ROI.</summary>
		public virtual HRegion getRegion()
		{
			return null;
		}

		public virtual double getDistanceFromStartPoint(double row, double col)
		{
			return 0.0;
		}
		/// <summary>
		/// Gets the model information described by 
		/// the ROI.
		/// </summary> 
		public virtual HTuple getModelData()
		{
			return null;
		}

		/// <summary>Number of handles defined for the ROI.</summary>
		/// <returns>Number of handles</returns>
		public int getNumHandles()
		{
			return NumHandles;
		}

		/// <summary>Gets the active handle of the ROI.</summary>
		/// <returns>Index of the active handle (from the handle list)</returns>
		public int getActHandleIdx()
		{
			return activeHandleIdx;
		}

		/// <summary>
		/// Gets the sign of the ROI object, being either 
		/// 'positive' or 'negative'. This sign is used when creating a model
		/// region for matching applications from a list of ROIs.
		/// </summary>
		public int getOperatorFlag()
		{
			return OperatorFlag;
		}

		/// <summary>
		/// Sets the sign of a ROI object to be positive or negative. 
		/// The sign is used when creating a model region for matching
		/// applications by summing up all positive and negative ROI models
		/// created so far.
		/// </summary>
		/// <param name="flag">Sign of ROI object</param>
		public void setOperatorFlag(int flag)
		{
			OperatorFlag = flag;

			switch (OperatorFlag)
			{
				case ROI.POSITIVE_FLAG:
					flagLineStyle = posOperation;
					break;
				case ROI.NEGATIVE_FLAG:
					flagLineStyle = negOperation;
					break;
				default:
					flagLineStyle = posOperation;
					break;
			}
		}
	}//end of class
}//end of namespace
