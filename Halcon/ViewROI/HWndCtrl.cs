using System;
using ViewROI;
using System.Collections;
using HalconDotNet;
using System.Diagnostics;
using System.Windows.Controls;
using System.Security.Permissions;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace ViewROI
{
    public delegate void IconicDelegate(int val);
    public delegate void FuncDelegate();

    /// <summary>
    /// This class works as a wrapper class for the HALCON window
    /// HWindow. HWndCtrl is in charge of the visualization.
    /// You can move and zoom the visible image part by using GUI component 
    /// inputs or with the mouse. The class HWndCtrl uses a graphics stack 
    /// to manage the iconic objects for the display. Each object is linked 
    /// to a graphical context, which determines how the object is to be drawn.
    /// The context can be changed by calling changeGraphicSettings().
    /// The graphical "modes" are defined by the class GraphicsContext and 
    /// map most of the dev_set_* operators provided in HDevelop.
    /// </summary>
    public class HWndCtrl
    {
        /// <summary>No action is performed on mouse events</summary>
        public const int MODE_VIEW_NONE = 10;

        /// <summary>Zoom is performed on mouse events</summary>
        public const int MODE_VIEW_ZOOM = 11;

        /// <summary>Move is performed on mouse events</summary>
        public const int MODE_VIEW_MOVE = 12;

        /// <summary>Magnification is performed on mouse events</summary>
        public const int MODE_VIEW_ZOOMWINDOW = 13;

        /// <summary>
        /// 画ROI时，屏蔽所有移动
        /// </summary>
        public const int MODE_ROI_Create = 14;
        /// <summary>
        /// 显示图像时是否自适应窗口
        /// </summary>
        public bool Image_Auto_Fit = true;

        public const int MODE_INCLUDE_ROI = 1;

        public const int MODE_EXCLUDE_ROI = 2;


        /// <summary>
        /// Constant describes delegate message to signal new image
        /// </summary>
        public const int EVENT_UPDATE_IMAGE = 31;
        /// <summary>
        /// Constant describes delegate message to signal error
        /// when reading an image from file
        /// </summary>
        public const int ERR_READING_IMG = 32;
        /// <summary> 
        /// Constant describes delegate message to signal errorq
        /// when defining a graphical context
        /// </summary>
        public const int ERR_DEFINING_GC = 33;

        /// <summary> 
        /// Maximum number of HALCON objects that can be put on the graphics 
        /// stack without loss. For each additional object, the first entry 
        /// is removed from the stack again.
        /// </summary>
        private const int MAXNUMOBJLIST = 50;


        private int stateView;
        private bool mousePressed = false;
        private double startX, startY;

        /// <summary>HALCON window</summary>
        public HWindowControlWPF viewPort;

        /// <summary>
        /// Instance of ROIController, which manages ROI interaction
        /// </summary>
        private ROIController roiManager;

        /* dispROI is a flag to know when to add the ROI models to the 
		   paint routine and whether or not to respond to mouse events for 
		   ROI objects */
        private int dispROI;


        /* Basic parameters, like dimension of window and displayed image part */
        private double windowWidth;
        private double windowHeight;
        private int imageWidth;
        private int imageHeight;

        private int[] CompRangeX;
        private int[] CompRangeY;


        private int prevCompX, prevCompY;
        private double stepSizeX, stepSizeY;


        /* Image coordinates, which describe the image part that is displayed  
		   in the HALCON window */
        private double ImgRow1, ImgCol1, ImgRow2, ImgCol2;

        /// <summary>Error message when an exception is thrown</summary>
        public string exceptionText = "";


        /* Delegates to send notification messages to other classes */
        /// <summary>
        /// Delegate to add information to the HALCON window after 
        /// the paint routine has finished
        /// </summary>
        public FuncDelegate addInfoDelegate;

        /// <summary>
        /// Delegate to notify about failed tasks of the HWndCtrl instance
        /// </summary>
        public IconicDelegate NotifyIconObserver;


        private HWindow ZoomWindow;
        private double zoomWndFactor;
        private double zoomAddOn;
        private int zoomWndSize;


        /// <summary> 
        /// List of HALCON objects to be drawn into the HALCON window. 
        /// The list shouldn't contain more than MAXNUMOBJLIST objects, 
        /// otherwise the first entry is removed from the list.
        /// </summary>
        public ObservableCollection<HObjectEntry> HObjList;

        public ObservableCollection<HMsgEntry> HMsgList;

        public event EventHandler HObjListChanged;
        
        /// <summary>
        /// Instance that describes the graphical context for the
        /// HALCON window. According on the graphical settings
        /// attached to each HALCON object, this graphical context list 
        /// is updated constantly.
        /// </summary>
        private GraphicsContext mGC;


        /// <summary> 
        /// Initializes the image dimension, mouse delegation, and the 
        /// graphical context setup of the instance.
        /// </summary>
        /// <param name="view"> HALCON window </param>
        public HWndCtrl(HWindowControlWPF view)
        {
            viewPort = view;
            stateView = MODE_VIEW_NONE;
            windowWidth = viewPort.ActualWidth;
            windowHeight = viewPort.ActualHeight;

            zoomWndFactor = (double)imageWidth / viewPort.ActualWidth;
            zoomAddOn = Math.Pow(0.9, 5);
            zoomWndSize = 150;

            /*default*/
            CompRangeX = new int[] { 0, 100 };
            CompRangeY = new int[] { 0, 100 };

            prevCompX = prevCompY = 0;

            dispROI = MODE_INCLUDE_ROI;//1;

            viewPort.HMouseUp += ViewPort_HMouseUp;
            viewPort.HMouseDown += ViewPort_HMouseDown;
            viewPort.HMouseMove += ViewPort_HMouseMove;
            viewPort.HMouseWheel += ViewPort_HMouseWheel;

            viewPort.SizeChanged += ViewPort_SizeChanged;


            addInfoDelegate = new FuncDelegate(dummyV);
            NotifyIconObserver = new IconicDelegate(dummy);

            // graphical stack 
            HObjList = new ObservableCollection<HObjectEntry>();
            HMsgList = new ObservableCollection<HMsgEntry>();
            mGC = new GraphicsContext();
            mGC.gcNotification = new GCDelegate(exceptionGC);
        }
        private void ViewPort_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            DelayRepaint();
        }
        private async void DelayRepaint()
        {
            await System.Threading.Tasks.Task.Delay(1);
            FitImage();
            repaint();


            //Console.WriteLine("W:" + viewPort.ImagePart.Width + "H:" + viewPort.ImagePart.Height);
            //Console.WriteLine("w:" + viewPort.ActualWidth + "h:" + viewPort.ActualHeight);
            //double regionw = viewPort.ImagePart.Width * 10 / viewPort.ActualWidth;
            //double regionh = viewPort.ImagePart.Height * 10 / viewPort.ActualHeight;

            //Console.WriteLine("ww:" + regionw + "hh:" + regionh);

            //int hrow, hcol, hw, hh;
            //viewPort.HalconWindow.GetPart(out hrow, out hcol, out hw, out hh);
            //Console.WriteLine("hrow:" + hrow + "hcol:" + hcol + "hw:" + hw + "hh:" + hh);


            //int wrow, wcol, ww, wh;
            //viewPort.HalconWindow.GetWindowExtents(out wrow, out wcol, out ww, out wh);
            //Console.WriteLine("wrow:" + wrow + "wcol:" + wcol + "ww:" + ww + "wh:" + wh);


            //SetEpsilon();
        }
        public void SetEpsilon()
        {
            try
            {
                int hrow, hcol, hw, hh;
                viewPort.HalconWindow.GetPart(out hrow, out hcol, out hh, out hw);
                int wrow, wcol, ww, wh;
                viewPort.HalconWindow.GetWindowExtents(out wrow, out wcol, out ww, out wh);

                double regionw = (hw - hcol) * 10 / ww;
                double regionh = (hh - hrow) * 10 / wh;

                double _epsilon = regionw;
                if (regionh > regionw)
                    _epsilon = regionh;
                if (_epsilon == 0)
                    _epsilon = 1;
                roiManager.epsilon = _epsilon;
            }
            catch { }
        }
        private void ViewPort_HMouseWheel(object sender, HMouseEventArgsWPF e)
        {
            if (stateView == MODE_ROI_Create)
                return;

            if (e.Delta > 0)
            {
                zoomImage(e.Column, e.Row, 0.9);
            }
            else
            {
                zoomImage(e.Column, e.Row, 1 / 0.9);
            }
        }

        private void ViewPort_HMouseMove(object sender, HMouseEventArgsWPF e)
        {
            if (stateView == MODE_ROI_Create)
                return;

            double motionX, motionY;
            double posX, posY;
            double zoomZone;
            int a = roiManager.mouseMoveROI(e.Column, e.Row);//鼠标经过的index
            if (!mousePressed)
                return;

            if (roiManager != null && (roiManager.activeROIidx != -1) && (dispROI == MODE_INCLUDE_ROI))
            {
                roiManager.mouseMoveAction(e.Column, e.Row);
            }
            else if (stateView == MODE_VIEW_MOVE)
            {
                motionX = ((e.Column - startX));
                motionY = ((e.Row - startY));

                if (((int)motionX != 0) || ((int)motionY != 0))
                {
                    moveImage(motionX, motionY);
                    startX = e.Column - motionX;
                    startY = e.Row - motionY;
                }
            }
            else if (stateView == MODE_VIEW_ZOOMWINDOW)
            {
                HSystem.SetSystem("flush_graphic", "false");
                ZoomWindow.ClearWindow();


                posX = ((e.Column - ImgCol1) / (ImgCol2 - ImgCol1)) * viewPort.ActualWidth;
                posY = ((e.Row - ImgRow1) / (ImgRow2 - ImgRow1)) * viewPort.ActualHeight;
                zoomZone = (zoomWndSize / 2) * zoomWndFactor * zoomAddOn;

                ZoomWindow.SetWindowExtents((int)posY - (zoomWndSize / 2),
                                            (int)posX - (zoomWndSize / 2),
                                            zoomWndSize, zoomWndSize);
                ZoomWindow.SetPart((int)(e.Row - zoomZone), (int)(e.Column - zoomZone),
                                   (int)(e.Row + zoomZone), (int)(e.Column + zoomZone));
                repaint(ZoomWindow);

                HSystem.SetSystem("flush_graphic", "true");
                ZoomWindow.DispLine(-100.0, -100.0, -100.0, -100.0);
            }
        }

        private void ViewPort_HMouseDown(object sender, HMouseEventArgsWPF e)
        {
            if (stateView == MODE_ROI_Create)
                return;

            mousePressed = true;
            int activeROIidx = -1;
            double scale;

            if (roiManager != null && (dispROI == MODE_INCLUDE_ROI))
            {
                activeROIidx = roiManager.mouseDownAction(e.Column, e.Row);
            }

            if (activeROIidx == -1)
            {
                switch (stateView)
                {
                    case MODE_VIEW_MOVE:
                        startX = e.Column;
                        startY = e.Row;
                        break;
                    case MODE_VIEW_ZOOM:
                        if (e.Button == System.Windows.Input.MouseButton.Left)
                            scale = 0.9;
                        else
                            scale = 1 / 0.9;
                        zoomImage(e.Column, e.Row, scale);
                        break;
                    case MODE_VIEW_NONE:
                        break;
                    case MODE_VIEW_ZOOMWINDOW:
                        activateZoomWindow((int)e.Column, (int)e.Row);
                        break;
                    default:
                        break;
                }
            }
        }

        private void ViewPort_HMouseUp(object sender, HMouseEventArgsWPF e)
        {
            if (stateView == MODE_ROI_Create)
                return;

            mousePressed = false;

            if (roiManager != null
                && (roiManager.activeROIidx != -1)
                && (dispROI == MODE_INCLUDE_ROI))
            {
                roiManager.NotifyRCObserver(ROIController.EVENT_UPDATE_ROI);
            }
            else if (stateView == MODE_VIEW_ZOOMWINDOW)
            {
                ZoomWindow.Dispose();
            }
        }
        /// <summary>
        /// Registers an instance of an ROIController with this window 
        /// controller (and vice versa).
        /// </summary>
        /// <param name="rC"> 
        /// Controller that manages interactive ROIs for the HALCON window 
        /// </param>
        public void useROIController(ROIController rC)
        {
            roiManager = rC;
            rC.setViewController(this);
        }


        /// <summary>
        /// Read dimensions of the image to adjust own window settings
        /// </summary>
        /// <param name="image">HALCON image</param>
        private void setImagePart(HImage image)
        {
            string s;
            int w, h;

            image.GetImagePointer1(out s, out w, out h);
            setImagePart(0, 0, h, w);
        }


        /// <summary>
        /// Adjust window settings by the values supplied for the left 
        /// upper corner and the right lower corner
        /// </summary>
        /// <param name="r1">y coordinate of left upper corner</param>
        /// <param name="c1">x coordinate of left upper corner</param>
        /// <param name="r2">y coordinate of right lower corner</param>
        /// <param name="c2">x coordinate of right lower corner</param>
        private void setImagePart(int r1, int c1, int r2, int c2)
        {
            ImgRow1 = r1;
            ImgCol1 = c1;
            ImgRow2 = imageHeight = r2;
            ImgCol2 = imageWidth = c2;

            System.Windows.Rect rect = viewPort.ImagePart;
            rect.X = (int)ImgCol1;
            rect.Y = (int)ImgRow1;
            rect.Height = (int)imageHeight;
            rect.Width = (int)imageWidth;
            viewPort.ImagePart = rect;

            SetEpsilon();
        }


        /// <summary>
        /// Sets the view mode for mouse events in the HALCON window
        /// (zoom, move, magnify or none).
        /// </summary>
        /// <param name="mode">One of the MODE_VIEW_* constants</param>
        public void setViewState(int mode)
        {
            stateView = mode;

            if (roiManager != null)
                roiManager.resetROI();
        }
        /// <summary>
        /// 获取当前stateView
        /// </summary>
        /// <returns></returns>
        public int getViewState()
        {
            return stateView;
        }

        /********************************************************************/
        private void dummy(int val)
        {
        }

        private void dummyV()
        {
        }

        /*******************************************************************/
        private void exceptionGC(string message)
        {
            exceptionText = message;
            NotifyIconObserver(ERR_DEFINING_GC);
        }

        /// <summary>
        /// Paint or don't paint the ROIs into the HALCON window by 
        /// defining the parameter to be equal to 1 or not equal to 1.
        /// </summary>
        public void setDispLevel(int mode)
        {
            dispROI = mode;
        }

        /****************************************************************************/
        /*                          graphical element                               */
        /****************************************************************************/
        private void zoomImage(double x, double y, double scale)
        {
            double lengthC, lengthR;
            double percentC, percentR;
            int lenC, lenR;

            percentC = (x - ImgCol1) / (ImgCol2 - ImgCol1);
            percentR = (y - ImgRow1) / (ImgRow2 - ImgRow1);

            lengthC = (ImgCol2 - ImgCol1) * scale;
            lengthR = (ImgRow2 - ImgRow1) * scale;

            ImgCol1 = x - lengthC * percentC;
            ImgCol2 = x + lengthC * (1 - percentC);

            ImgRow1 = y - lengthR * percentR;
            ImgRow2 = y + lengthR * (1 - percentR);

            lenC = (int)Math.Round(lengthC);
            lenR = (int)Math.Round(lengthR);

            System.Windows.Rect rect = viewPort.ImagePart;
            rect.X = (int)Math.Round(ImgCol1);
            rect.Y = (int)Math.Round(ImgRow1);
            rect.Width = (lenC > 0) ? lenC : 1;
            rect.Height = (lenR > 0) ? lenR : 1;
            viewPort.ImagePart = rect;

            SetEpsilon();

            zoomWndFactor *= scale;
            repaint();
        }

        /// <summary>
        /// Scales the image in the HALCON window according to the 
        /// value scaleFactor
        /// </summary>
        public void zoomImage(double scaleFactor)
        {
            double midPointX, midPointY;

            if (((ImgRow2 - ImgRow1) == scaleFactor * imageHeight) &&
                ((ImgCol2 - ImgCol1) == scaleFactor * imageWidth))
            {
                repaint();
                return;
            }

            ImgRow2 = ImgRow1 + imageHeight;
            ImgCol2 = ImgCol1 + imageWidth;

            midPointX = ImgCol1;
            midPointY = ImgRow1;

            zoomWndFactor = (double)imageWidth / viewPort.ActualWidth;
            zoomImage(midPointX, midPointY, scaleFactor);
        }


        /// <summary>
        /// Scales the HALCON window according to the value scale
        /// </summary>
        public void scaleWindow(double scale)
        {
            ImgRow1 = 0;
            ImgCol1 = 0;

            ImgRow2 = imageHeight;
            ImgCol2 = imageWidth;

            viewPort.Width = (int)(ImgCol2 * scale);
            viewPort.Height = (int)(ImgRow2 * scale);

            zoomWndFactor = ((double)imageWidth / viewPort.ActualWidth);
        }

        /// <summary>
        /// Recalculates the image-window-factor, which needs to be added to 
        /// the scale factor for zooming an image. This way the zoom gets 
        /// adjusted to the window-image relation, expressed by the equation 
        /// imageWidth/viewPort.Width.
        /// </summary>
        public void setZoomWndFactor()
        {
            zoomWndFactor = ((double)imageWidth / viewPort.ActualWidth);
        }

        /// <summary>
        /// Sets the image-window-factor to the value zoomF
        /// </summary>
        public void setZoomWndFactor(double zoomF)
        {
            zoomWndFactor = zoomF;
        }

        /*******************************************************************/
        private void moveImage(double motionX, double motionY)
        {
            ImgRow1 += -motionY;
            ImgRow2 += -motionY;

            ImgCol1 += -motionX;
            ImgCol2 += -motionX;

            System.Windows.Rect rect = viewPort.ImagePart;
            rect.X = (int)Math.Round(ImgCol1);
            rect.Y = (int)Math.Round(ImgRow1);
            viewPort.ImagePart = rect;

            repaint();
        }


        /// <summary>
        /// Resets all parameters that concern the HALCON window display 
        /// setup to their initial values and clears the ROI list.
        /// </summary>
        public void resetAll()
        {
            ImgRow1 = 0;
            ImgCol1 = 0;
            ImgRow2 = imageHeight;
            ImgCol2 = imageWidth;

            zoomWndFactor = (double)imageWidth / viewPort.ActualWidth;

            System.Windows.Rect rect = viewPort.ImagePart;
            rect.X = (int)ImgCol1;
            rect.Y = (int)ImgRow1;
            rect.Width = (int)imageWidth;
            rect.Height = (int)imageHeight;
            viewPort.ImagePart = rect;

            SetEpsilon();

            if (roiManager != null)
                roiManager.reset();
        }

        public void resetWindow()
        {
            ImgRow1 = 0;
            ImgCol1 = 0;
            ImgRow2 = imageHeight;
            ImgCol2 = imageWidth;

            zoomWndFactor = (double)imageWidth / viewPort.ActualWidth;

            System.Windows.Rect rect = viewPort.ImagePart;
            rect.X = (int)ImgCol1;
            rect.Y = (int)ImgRow1;
            rect.Width = (int)imageWidth;
            rect.Height = (int)imageHeight;
            viewPort.ImagePart = rect;

            SetEpsilon();
        }

        /*******************************************************************/
        private void activateZoomWindow(int X, int Y)
        {
            double posX, posY;
            int zoomZone;

            if (ZoomWindow != null)
                ZoomWindow.Dispose();

            HOperatorSet.SetSystem("border_width", 10);
            ZoomWindow = new HWindow();

            posX = ((X - ImgCol1) / (ImgCol2 - ImgCol1)) * viewPort.ActualWidth;
            posY = ((Y - ImgRow1) / (ImgRow2 - ImgRow1)) * viewPort.ActualHeight;

            zoomZone = (int)((zoomWndSize / 2) * zoomWndFactor * zoomAddOn);
            ZoomWindow.OpenWindow((int)posY - (zoomWndSize / 2), (int)posX - (zoomWndSize / 2),
                                   zoomWndSize, zoomWndSize,
                                   viewPort.HalconID, "visible", "");
            ZoomWindow.SetPart(Y - zoomZone, X - zoomZone, Y + zoomZone, X + zoomZone);
            repaint(ZoomWindow);
            ZoomWindow.SetColor("black");
        }



        /// <summary>
        /// To initialize the move function using a GUI component, the HWndCtrl
        /// first needs to know the range supplied by the GUI component. 
        /// For the x direction it is specified by xRange, which is 
        /// calculated as follows: GuiComponentX.Max()-GuiComponentX.Min().
        /// The starting value of the GUI component has to be supplied 
        /// by the parameter Init
        /// </summary>
        public void setGUICompRangeX(int[] xRange, int Init)
        {
            int cRangeX;

            CompRangeX = xRange;
            cRangeX = xRange[1] - xRange[0];
            prevCompX = Init;
            stepSizeX = ((double)imageWidth / cRangeX) * (imageWidth / windowWidth);

        }

        /// <summary>
        /// To initialize the move function using a GUI component, the HWndCtrl
        /// first needs to know the range supplied by the GUI component. 
        /// For the y direction it is specified by yRange, which is 
        /// calculated as follows: GuiComponentY.Max()-GuiComponentY.Min().
        /// The starting value of the GUI component has to be supplied 
        /// by the parameter Init
        /// </summary>
        public void setGUICompRangeY(int[] yRange, int Init)
        {
            int cRangeY;

            CompRangeY = yRange;
            cRangeY = yRange[1] - yRange[0];
            prevCompY = Init;
            stepSizeY = ((double)imageHeight / cRangeY) * (imageHeight / windowHeight);
        }


        /// <summary>
        /// Resets to the starting value of the GUI component.
        /// </summary>
        public void resetGUIInitValues(int xVal, int yVal)
        {
            prevCompX = xVal;
            prevCompY = yVal;
        }

        /// <summary>
        /// Moves the image by the value valX supplied by the GUI component
        /// </summary>
        public void moveXByGUIHandle(int valX)
        {
            double motionX;

            motionX = (valX - prevCompX) * stepSizeX;

            if (motionX == 0)
                return;

            moveImage(motionX, 0.0);
            prevCompX = valX;
        }


        /// <summary>
        /// Moves the image by the value valY supplied by the GUI component
        /// </summary>
        public void moveYByGUIHandle(int valY)
        {
            double motionY;

            motionY = (valY - prevCompY) * stepSizeY;

            if (motionY == 0)
                return;

            moveImage(0.0, motionY);
            prevCompY = valY;
        }

        /// <summary>
        /// Zooms the image by the value valF supplied by the GUI component
        /// </summary>
        public void zoomByGUIHandle(double valF)
        {
            double x, y, scale;
            double prevScaleC;



            x = (ImgCol1 + (ImgCol2 - ImgCol1) / 2);
            y = (ImgRow1 + (ImgRow2 - ImgRow1) / 2);

            prevScaleC = (double)((ImgCol2 - ImgCol1) / imageWidth);
            scale = ((double)1.0 / prevScaleC * (100.0 / valF));

            zoomImage(x, y, scale);
        }

        /// <summary>
        /// Triggers a repaint of the HALCON window
        /// </summary>
        public void repaint()
        {
            repaint(viewPort.HalconWindow);
        }

        /// <summary>
        /// Repaints the HALCON window 'window'
        /// </summary>
        public void repaint(HalconDotNet.HWindow window)
        {
            int h = imageHeight;
            if (window.IsInitialized() == false || viewPort.HalconID.ToInt32() == -1 || viewPort.ImagePart.Width <= 1 || viewPort.ImagePart.Height <= 1)
                return;
            int count = HObjList.Count;
            HObjectEntry entry;

            HSystem.SetSystem("flush_graphic", "false");
            window.ClearWindow();
            mGC.stateOfSettings.Clear();

            for (int i = 0; i < count; i++)
            {
                entry = ((HObjectEntry)HObjList[i]);
                mGC.applyContext(window, entry.gContext);
                window.DispObj(entry.HObj);
            }

            addInfoDelegate();

            if (roiManager != null && (dispROI == MODE_INCLUDE_ROI))
                roiManager.paintData(window);

            HSystem.SetSystem("flush_graphic", "true");

            window.SetColor("black");
            window.DispLine(-100.0, -100.0, -101.0, -101.0);
            
            for (int i=0;i<HMsgList.Count;i++)
            {
                if (HMsgList[i].Size != 0)
                {
                    set_display_font(viewPort.HalconID, HMsgList[i].Size, HMsgList[i].Font, HMsgList[i].Bold, HMsgList[i].Slant);
                }
                window.DispText(HMsgList[i].HMsg, HMsgList[i].coordSystem, HMsgList[i].row, HMsgList[i].column, HMsgList[i].color, HMsgList[i].genParamName, HMsgList[i].genParamValue);
            }
        }

        public void set_display_font(HTuple hv_WindowHandle, HTuple hv_Size, HTuple hv_Font,
      HTuple hv_Bold, HTuple hv_Slant)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_OS = null, hv_Fonts = new HTuple();
            HTuple hv_Style = null, hv_Exception = new HTuple(), hv_AvailableFonts = null;
            HTuple hv_Fdx = null, hv_Indices = new HTuple();
            HTuple hv_Font_COPY_INP_TMP = hv_Font.Clone();
            HTuple hv_Size_COPY_INP_TMP = hv_Size.Clone();

            // Initialize local and output iconic variables 
            //This procedure sets the text font of the current window with
            //the specified attributes.
            //
            //Input parameters:
            //WindowHandle: The graphics window for which the font will be set
            //Size: The font size. If Size=-1, the default of 16 is used.
            //Bold: If set to 'true', a bold font is used
            //Slant: If set to 'true', a slanted font is used
            //
            HOperatorSet.GetSystem("operating_system", out hv_OS);
            // dev_get_preferences(...); only in hdevelop
            // dev_set_preferences(...); only in hdevelop
            if ((int)((new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(-1)))) != 0)
            {
                hv_Size_COPY_INP_TMP = 16;
            }
            if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
            {
                //Restore previous behaviour
                hv_Size_COPY_INP_TMP = ((1.13677 * hv_Size_COPY_INP_TMP)).TupleInt();
            }
            if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("Courier"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Courier";
                hv_Fonts[1] = "Courier 10 Pitch";
                hv_Fonts[2] = "Courier New";
                hv_Fonts[3] = "CourierNew";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Consolas";
                hv_Fonts[1] = "Menlo";
                hv_Fonts[2] = "Courier";
                hv_Fonts[3] = "Courier 10 Pitch";
                hv_Fonts[4] = "FreeMono";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Luxi Sans";
                hv_Fonts[1] = "DejaVu Sans";
                hv_Fonts[2] = "FreeSans";
                hv_Fonts[3] = "Arial";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Times New Roman";
                hv_Fonts[1] = "Luxi Serif";
                hv_Fonts[2] = "DejaVu Serif";
                hv_Fonts[3] = "FreeSerif";
                hv_Fonts[4] = "Utopia";
            }
            else
            {
                hv_Fonts = hv_Font_COPY_INP_TMP.Clone();
            }
            hv_Style = "";
            if ((int)(new HTuple(hv_Bold.TupleEqual("true"))) != 0)
            {
                hv_Style = hv_Style + "Bold";
            }
            else if ((int)(new HTuple(hv_Bold.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Bold";
                throw new HalconException(hv_Exception);
            }
            if ((int)(new HTuple(hv_Slant.TupleEqual("true"))) != 0)
            {
                hv_Style = hv_Style + "Italic";
            }
            else if ((int)(new HTuple(hv_Slant.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Slant";
                throw new HalconException(hv_Exception);
            }
            if ((int)(new HTuple(hv_Style.TupleEqual(""))) != 0)
            {
                hv_Style = "Normal";
            }
            HOperatorSet.QueryFont(hv_WindowHandle, out hv_AvailableFonts);
            hv_Font_COPY_INP_TMP = "";
            for (hv_Fdx = 0; (int)hv_Fdx <= (int)((new HTuple(hv_Fonts.TupleLength())) - 1); hv_Fdx = (int)hv_Fdx + 1)
            {
                hv_Indices = hv_AvailableFonts.TupleFind(hv_Fonts.TupleSelect(hv_Fdx));
                if ((int)(new HTuple((new HTuple(hv_Indices.TupleLength())).TupleGreater(0))) != 0)
                {
                    if ((int)(new HTuple(((hv_Indices.TupleSelect(0))).TupleGreaterEqual(0))) != 0)
                    {
                        hv_Font_COPY_INP_TMP = hv_Fonts.TupleSelect(hv_Fdx);
                        break;
                    }
                }
            }
            if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual(""))) != 0)
            {
                throw new HalconException("Wrong value of control parameter Font");
            }
            hv_Font_COPY_INP_TMP = (((hv_Font_COPY_INP_TMP + "-") + hv_Style) + "-") + hv_Size_COPY_INP_TMP;
            HOperatorSet.SetFont(hv_WindowHandle, hv_Font_COPY_INP_TMP);
            // dev_set_preferences(...); only in hdevelop

            return;
        }

        /********************************************************************/
        /*                      GRAPHICSSTACK                               */
        /********************************************************************/

        /// <summary>
        /// Adds an iconic object to the graphics stack similar to the way
        /// it is defined for the HDevelop graphics stack.
        /// </summary>
        /// <param name="obj">Iconic object</param>
        public void addIconicVar(HObject obj)
        {
            HObjectEntry entry;

            if (obj == null)
                return;

            if (obj is HImage)
            {
                //double r, c;
                //int area;
                int h, w;
                string s;

                //area = ((HImage)obj).GetDomain().AreaCenter(out r, out c);
                ((HImage)obj).GetImagePointer1(out s, out w, out h);

                HTuple width, height;
                HOperatorSet.GetSystem("width", out width);
                HOperatorSet.GetSystem("height", out height);
                //在某种情况下，Get的值正确的也未生效，需要再设置一遍，原因未知
                HOperatorSet.SetSystem("width", width);
                HOperatorSet.SetSystem("height", height);

                if (Image_Auto_Fit)//area == (w * h))//大小不同也清除原来的
                {
                    clearList();
                    clearMsgList();

                    if ((h != imageHeight) || (w != imageWidth))
                    {
                        int _beginRow, _begin_Col, _endRow, _endCol;
                        double ratio_win = (double)viewPort.ActualWidth / (double)viewPort.ActualHeight;
                        double ratio_img = (double)w / (double)h;
                        imageHeight = h;
                        imageWidth = w;
                        if (ratio_win >= ratio_img)
                        {
                            _beginRow = 0;
                            _endRow = h - 1;
                            _begin_Col = (int)(-w * (ratio_win / ratio_img - 1d) / 2d);
                            _endCol = (int)(w + w * (ratio_win / ratio_img - 1d) / 2d);
                            zoomWndFactor = (double)h / viewPort.ActualHeight;
                        }
                        else
                        {
                            _begin_Col = 0;
                            _endCol = w - 1;
                            _beginRow = (int)(-h * (ratio_img / ratio_win - 1d) / 2d);
                            _endRow = (int)(h + h * (ratio_img / ratio_win - 1d) / 2d);
                            zoomWndFactor = (double)w / viewPort.ActualWidth;
                        }
                        //viewPort.HalconWindow.SetPart(_beginRow, _begin_Col, _endRow, _endCol);
                        setImagePart(_beginRow, _begin_Col, (int)viewPort.ActualHeight, (int)viewPort.ActualWidth);
                        //setImagePart(_beginRow, _begin_Col, _endRow-_beginRow, _endCol-_begin_Col);
                        //clearList();

                        zoomImage(zoomWndFactor);
                    }
                }//if
            }//if

            entry = new HObjectEntry(obj, mGC.copyContextList());

            HObjList.Add(entry);

            if (HObjList.Count > MAXNUMOBJLIST)
                HObjList.RemoveAt(1);

            if (HObjListChanged != null)
                HObjListChanged(null, null);
        }
        public void addHMsgVar(HMsgEntry msg)
        {
            HMsgList.Add(msg);
        }
        public void FitImage()
        {
            if (HObjList.Count == 0)
                return;

            HObjectEntry entry = HObjList[0] as HObjectEntry;
            if (entry.HObj is HImage)
            {
                double r, c;
                int h, w, area;
                string s;

                area = ((HImage)entry.HObj).GetDomain().AreaCenter(out r, out c);
                ((HImage)entry.HObj).GetImagePointer1(out s, out w, out h);


                if ((h != imageHeight) || (w != imageWidth))
                {
                    int _beginRow, _begin_Col, _endRow, _endCol;
                    double ratio_win = (double)viewPort.ActualWidth / (double)viewPort.ActualHeight;
                    double ratio_img = (double)w / (double)h;
                    imageHeight = h;
                    imageWidth = w;
                    if (ratio_win >= ratio_img)
                    {
                        _beginRow = 0;
                        _endRow = h - 1;
                        _begin_Col = (int)(-w * (ratio_win / ratio_img - 1d) / 2d);
                        _endCol = (int)(w + w * (ratio_win / ratio_img - 1d) / 2d);
                        zoomWndFactor = (double)h / viewPort.ActualHeight;
                    }
                    else
                    {
                        _begin_Col = 0;
                        _endCol = w - 1;
                        _beginRow = (int)(-h * (ratio_img / ratio_win - 1d) / 2d);
                        _endRow = (int)(h + h * (ratio_img / ratio_win - 1d) / 2d);
                        zoomWndFactor = (double)w / viewPort.ActualWidth;
                    }
                    //viewPort.HalconWindow.SetPart(_beginRow, _begin_Col, _endRow, _endCol);
                    setImagePart(_beginRow, _begin_Col, (int)viewPort.ActualHeight, (int)viewPort.ActualWidth);
                    //setImagePart(_beginRow, _begin_Col, _endRow-_beginRow, _endCol-_begin_Col);


                    zoomImage(zoomWndFactor);
                }
            }
        }


        /// <summary>
        /// Clears all entries from the graphics stack 
        /// </summary>
        public void clearList()
        {
            HObjList.Clear();

            if (HObjListChanged != null)
                HObjListChanged(null, null);
        }
        public void clearMsgList()
        {
            HMsgList.Clear();
        }

        /// <summary>
        /// Returns the number of items on the graphics stack
        /// </summary>
        public int getListCount()
        {
            return HObjList.Count;
        }

        /// <summary>
        /// Changes the current graphical context by setting the specified mode
        /// (constant starting by GC_*) to the specified value.
        /// </summary>
        /// <param name="mode">
        /// Constant that is provided by the class GraphicsContext
        /// and describes the mode that has to be changed, 
        /// e.g., GraphicsContext.GC_COLOR
        /// </param>
        /// <param name="val">
        /// Value, provided as a string, 
        /// the mode is to be changed to, e.g., "blue" 
        /// </param>
        public void changeGraphicSettings(string mode, string val)
        {
            switch (mode)
            {
                case GraphicsContext.GC_COLOR:
                    mGC.setColorAttribute(val);
                    break;
                case GraphicsContext.GC_DRAWMODE:
                    mGC.setDrawModeAttribute(val);
                    break;
                case GraphicsContext.GC_LUT:
                    mGC.setLutAttribute(val);
                    break;
                case GraphicsContext.GC_PAINT:
                    mGC.setPaintAttribute(val);
                    break;
                case GraphicsContext.GC_SHAPE:
                    mGC.setShapeAttribute(val);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Changes the current graphical context by setting the specified mode
        /// (constant starting by GC_*) to the specified value.
        /// </summary>
        /// <param name="mode">
        /// Constant that is provided by the class GraphicsContext
        /// and describes the mode that has to be changed, 
        /// e.g., GraphicsContext.GC_LINEWIDTH
        /// </param>
        /// <param name="val">
        /// Value, provided as an integer, the mode is to be changed to, 
        /// e.g., 5 
        /// </param>
        public void changeGraphicSettings(string mode, int val)
        {
            switch (mode)
            {
                case GraphicsContext.GC_COLORED:
                    mGC.setColoredAttribute(val);
                    break;
                case GraphicsContext.GC_LINEWIDTH:
                    mGC.setLineWidthAttribute(val);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Changes the current graphical context by setting the specified mode
        /// (constant starting by GC_*) to the specified value.
        /// </summary>
        /// <param name="mode">
        /// Constant that is provided by the class GraphicsContext
        /// and describes the mode that has to be changed, 
        /// e.g.,  GraphicsContext.GC_LINESTYLE
        /// </param>
        /// <param name="val">
        /// Value, provided as an HTuple instance, the mode is 
        /// to be changed to, e.g., new HTuple(new int[]{2,2})
        /// </param>
        public void changeGraphicSettings(string mode, HTuple val)
        {
            switch (mode)
            {
                case GraphicsContext.GC_LINESTYLE:
                    mGC.setLineStyleAttribute(val);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Clears all entries from the graphical context list
        /// </summary>
        public void clearGraphicContext()
        {
            mGC.clear();
        }

        /// <summary>
        /// Returns a clone of the graphical context list (hashtable)
        /// </summary>
        public Hashtable getGraphicContext()
        {
            return mGC.copyContextList();
        }

    }//end of class
}//end of namespace
