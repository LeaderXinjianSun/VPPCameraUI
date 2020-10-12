using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using HalconDotNet;
using ViewROI;
/// <summary>
/// 版本V1.11 添加AutoRepaint属性，设定是否自动更新窗口
/// </summary>
namespace HalconViewer
{
    /// <summary>
    /// ImageViewer.xaml 的交互逻辑
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        public ImageViewer()
        {
            InitializeComponent();
            //hv_window = this.WPF_HWindow.HalconWindow;
            viewController = new HWndCtrl(this.WPF_HWindow);
            roiController = new ROIController();



            viewController.useROIController(roiController);
            viewController.setViewState(HWndCtrl.MODE_VIEW_MOVE);
            HObjectList = viewController.HObjList;
            viewController.HObjListChanged += ViewController_HObjListChanged;

            ROIList = roiController.ROIList;
            roiController.ActiveChanged += RoiController_ActiveChanged;
            roiController.ROIChanged += RoiController_ROIChanged;

        }
        /// <summary>
        /// 图形管理器
        /// </summary>
        public HWndCtrl viewController;
        /// <summary>
        /// ROI管理器
        /// </summary>
        public ROIController roiController;

        public IntPtr HWindowHalconID
        {
            get { return this.WPF_HWindow.HalconID; }
        }
        public HWindow hv_window
        {
            get { return WPF_HWindow.HalconWindow; }
        }

        #region 图像与ROI

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(HImage), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    var image = e.NewValue as HImage;
                    if (image != null)
                    {
                        imageViewer.viewController.addIconicVar(image);
                        if(imageViewer.AutoRepaint)
                            imageViewer.viewController.repaint();
                        GC.Collect();//垃圾回收
                    }
                    else
                    {
                        imageViewer.viewController.clearList();
                        if (imageViewer.AutoRepaint)
                            imageViewer.viewController.repaint();
                        GC.Collect();//垃圾回收
                    }
                }
                  )
                ));
        public HImage Image
        {
            get { return (HImage)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public static readonly DependencyProperty AutoFitProperty =
            DependencyProperty.Register("AutoFit", typeof(bool), typeof(ImageViewer), new PropertyMetadata(true,
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    var mAutoFit = (bool)e.NewValue;
                    imageViewer.viewController.Image_Auto_Fit = mAutoFit;
                }
                  )
                ));
        public bool AutoFit
        {
            get { return (bool)GetValue(AutoFitProperty); }
            set { SetValue(AutoFitProperty, value); }
        }
        public static readonly DependencyProperty ROIListProperty =
            DependencyProperty.Register("ROIList", typeof(ObservableCollection<ROI>), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    var ROIList = e.NewValue as ObservableCollection<ROI>;
                    if (ROIList == null)
                        imageViewer.roiController.ROIList.Clear();
                    else
                        imageViewer.roiController.ROIList = ROIList;
                    if (imageViewer.AutoRepaint)
                        imageViewer.viewController.repaint();
                })));
        public ObservableCollection<ROI> ROIList
        {
            get { return (ObservableCollection<ROI>)GetValue(ROIListProperty); }
            set { SetValue(ROIListProperty, value); }
        }
        public static DependencyProperty ActiveIndexProperty =
            DependencyProperty.Register("ActiveIndex", typeof(int), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    var mActiveIndex = (int)e.NewValue;
                    imageViewer.roiController.activeROIidx = mActiveIndex;
                })));
        public int ActiveIndex
        {
            get { return (int)GetValue(ActiveIndexProperty); }
            set { SetValue(ActiveIndexProperty, value); }
        }

        public bool ROIChanged
        {
            get { return (bool)GetValue(ROIChangedProperty); }
            set { SetValue(ROIChangedProperty, value); }
        }
        public static readonly DependencyProperty ROIChangedProperty =
            DependencyProperty.Register("ROIChanged", typeof(bool), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    imageViewer.ROIChanged = (bool)e.NewValue;
                })));
        public static readonly DependencyProperty SizeEnableProperty =
            DependencyProperty.Register("SizeEnable", typeof(bool), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    imageViewer.viewController.setViewState(HWndCtrl.MODE_VIEW_MOVE);
                    if (imageViewer.AutoRepaint)
                        imageViewer.viewController.repaint();
                })));
        public bool SizeEnable
        {
            get { return (bool)GetValue(SizeEnableProperty); }
            set { SetValue(SizeEnableProperty, value); }
        }

        public static readonly DependencyProperty RepaintProperty =
            DependencyProperty.Register("Repaint", typeof(bool), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    imageViewer.viewController.repaint();
                })));
        public bool Repaint
        {
            get { return (bool)GetValue(RepaintProperty); }
            set { SetValue(RepaintProperty, value); }
        }
        public static readonly DependencyProperty AutoRepaintProperty =
            DependencyProperty.Register("AutoRepaint", typeof(bool), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    imageViewer.AutoRepaint = (bool)e.NewValue;
                })));
        /// <summary>
        /// 判断是否自动Repaint刷新窗口
        /// </summary>
        public bool AutoRepaint
        {
            get { return (bool)GetValue(AutoRepaintProperty); }
            set { SetValue(RepaintProperty, value); }
        }
        #endregion

        #region 图形样式和HRegion

        public static readonly DependencyProperty GCStyleProperty =
            DependencyProperty.Register("GCStyle", typeof(Tuple<string, object>), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    var GCStyle = e.NewValue as Tuple<string, object>;
                    if (GCStyle == null)//传空值，清空当前图形样式
                        imageViewer.viewController.clearGraphicContext();
                    else
                    {
                        dynamic val = GCStyle.Item2;
                        imageViewer.viewController.changeGraphicSettings(GCStyle.Item1, val);
                    }

                })));
        /// <summary>
        /// 图形样式，可设置Color DrawMode LineWidth 等，下次绘图有效
        /// </summary>
        public Tuple<string, object> GCStyle
        {
            get { return (Tuple<string, object>)GetValue(GCStyleProperty); }
            set { SetValue(GCStyleProperty, value); }
        }

        public static readonly DependencyProperty AppendHObjectProperty =
            DependencyProperty.Register("AppendHObject", typeof(HObject), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    var AppendHObject = e.NewValue as HObject;
                    if (AppendHObject == null)
                    {
                        for (int i = imageViewer.HObjectList.Count - 1; i > 0; i--)
                        {
                            HObjectEntry h = imageViewer.HObjectList[i];
                            if (!(h.HObj is HImage))
                            {
                                imageViewer.HObjectList.RemoveAt(i);
                            }
                        }
                        if (imageViewer.AutoRepaint)
                            imageViewer.viewController.repaint();
                    }
                    else
                    {
                        imageViewer.viewController.addIconicVar(AppendHObject);
                        if (imageViewer.AutoRepaint)
                            imageViewer.viewController.repaint();
                    }
                })));
        /// <summary>
        /// 新增的HObject,赋值null，清空所有除了图像
        /// </summary>
        public HObject AppendHObject
        {
            get { return (HObject)GetValue(AppendHObjectProperty); }
            set { SetValue(AppendHObjectProperty, value); }
        }

        public static readonly DependencyProperty HObjectListProperty =
            DependencyProperty.Register("HObjectList", typeof(ObservableCollection<HObjectEntry>), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    var HObjectList = e.NewValue as ObservableCollection<HObjectEntry>;
                    if (HObjectList == null)
                    {
                        imageViewer.viewController.clearList();
                        if (imageViewer.AutoRepaint)
                            imageViewer.viewController.repaint();
                    }
                })));
        /// <summary>
        /// 所有HObject,赋值null，清空所有包括图像
        /// </summary>
        public ObservableCollection<HObjectEntry> HObjectList
        {
            get { return (ObservableCollection<HObjectEntry>)GetValue(HObjectListProperty); }
            set { SetValue(HObjectListProperty, value); }
        }

        public static readonly DependencyProperty AppendHMessageProperty =
            DependencyProperty.Register("AppendHMessage", typeof(HMsgEntry), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    var AppendHMessage = e.NewValue as HMsgEntry;
                    if (AppendHMessage == null)
                    {
                        imageViewer.viewController.clearMsgList();
                        if (imageViewer.AutoRepaint)
                            imageViewer.viewController.repaint();
                    }
                    else
                    {
                        imageViewer.viewController.addHMsgVar(AppendHMessage);
                        if (imageViewer.AutoRepaint)
                            imageViewer.viewController.repaint();
                    }
                })));
        /// <summary>
        /// 新增的HObject,赋值null，清空所有除了图像
        /// </summary>
        public HMsgEntry AppendHMessage
        {
            get { return (HMsgEntry)GetValue(AppendHMessageProperty); }
            set { SetValue(AppendHMessageProperty, value); }
        }
        public static readonly DependencyProperty HMessageListProperty =
            DependencyProperty.Register("HMessageList", typeof(ObservableCollection<HMsgEntry>), typeof(ImageViewer), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as ImageViewer;
                    var HMessageList = e.NewValue as ObservableCollection<HMsgEntry>;
                    if (HMessageList == null)
                    {
                        imageViewer.viewController.clearMsgList();
                        if (imageViewer.AutoRepaint)
                            imageViewer.viewController.repaint();
                    }
                })));
        public ObservableCollection<HMsgEntry> HMessageList
        {
            get { return (ObservableCollection<HMsgEntry>)GetValue(HMessageListProperty); }
            set { SetValue(HMessageListProperty, value); }
        }

        #endregion

        #region Halcon通知事件
        private void ViewController_HObjListChanged(object sender, EventArgs e)
        {
            HObjectList = viewController.HObjList;
        }

        void RoiController_ROIChanged(object sender, EventArgs e)
        {
            ROIChanged = !ROIChanged;
        }

        void RoiController_ActiveChanged(object sender, EventArgs e)
        {
            ActiveIndex = roiController.activeROIidx;
        }
        #endregion

        #region 右击菜单 保存图像、保存窗口和适应窗口

        private void MenuItem_Fit_Click(object sender, RoutedEventArgs e)
        {
            viewController.FitImage();
        }
        private void MenuItem_Auto_Fit_Click(object sender, RoutedEventArgs e)
        {
            MenuItem_Auto_Fit.IsChecked = !MenuItem_Auto_Fit.IsChecked;
            this.AutoFit = MenuItem_Auto_Fit.IsChecked;
        }
        private void MenuItem_SaveImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Image != null)
                {
                    Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                    string imgFileName;

                    sfd.Filter = "BMP图像|*.bmp|PNG图像|*.png|JPEG图像|*.jpg|所有文件|*.*";

                    if (sfd.ShowDialog() == true)
                    {
                        if (String.IsNullOrEmpty(sfd.FileName))
                            return;

                        imgFileName = sfd.FileName;
                        Image.WriteImage("bmp", 0, imgFileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void MenuItem_SaveWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                string imgFileName;

                sfd.Filter = "BMP图像|*.bmp|PNG图像|*.png|JPEG图像|*.jpg|所有文件|*.*";

                if (sfd.ShowDialog() == true)
                {
                    if (String.IsNullOrEmpty(sfd.FileName))
                        return;

                    imgFileName = sfd.FileName;
                    WPF_HWindow.HalconWindow.DumpWindow("bmp", imgFileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        #endregion

        string mActiveTool = "Move";
        /// <summary>
        /// 当前鼠标工具
        /// </summary>
        public string ActiveTool
        {
            get { return mActiveTool; }
            set
            {
                mActiveTool = value;
            }
        }
        
        /// <summary>
        /// 指定ROI类型画ROI
        /// </summary>
        /// <param name="ROI_TYPE">ROI.ROI_TYPE</param>
        /// <returns></returns>
        public ROI DrawROI(int ROI_TYPE)
        {
            ROI NewROI = null;
            viewController.setViewState(HWndCtrl.MODE_ROI_Create);
            WPF_HWindow.HalconWindow.SetColor("red");
            
            if (ROI_TYPE == ROI.ROI_TYPE_LINE)
            {
                double row1, row2, column1, column2;
                hv_window.DrawLine(out row1, out column1, out row2, out column2);
                if (row1 == 0 && row2 == 0 && column1 == 0 && column2 == 0)
                {
                    Console.WriteLine("空的ROI");
                }
                else
                {
                    ROILine _ROI = new ROILine();
                    _ROI.createROI(row1, column1, row2, column2);
                    NewROI = _ROI;
                }
            }
            else if (ROI_TYPE == ROI.ROI_TYPE_RECTANGLE1)
            {//如果type是Rectangle1
                double row1, row2, column1, column2;
                hv_window.DrawRectangle1(out row1, out column1, out row2, out column2);

                if (row1 == 0 && row2 == 0 && column1 == 0 && column2 == 0)
                {
                    Console.WriteLine("空的ROI");
                }
                else
                { 
                    ROIRectangle1 _ROI = new ROIRectangle1();
                    _ROI.createROI(row1, column1, row2, column2);
                    NewROI = _ROI;
                }
            }
            else if(ROI_TYPE==ROI.ROI_TYPE_RECTANGLE2)
            {//
                double row, column, phi, length1, length2;
                hv_window.DrawRectangle2(out row, out column, out phi, out length1, out length2);

                if(row ==0 && column==0&&length1==0&&length2==0)
                {
                    Console.WriteLine("空的ROI");
                }
                else
                {
                    ROIRectangle2 _ROI = new ROIRectangle2();
                    _ROI.createROI(row, column, phi, length1, length2);
                    NewROI = _ROI;
                }
            }
            else if(ROI_TYPE == ROI.ROI_TYPE_REGION)
            {//如果type是Region
                HRegion hr = hv_window.DrawRegion();
                if (hr.Area.L == 0)
                {
                    Console.WriteLine("空的ROI");
                }
                else
                {
                    NewROI = new ROIRegion(hr);
                }
            }
            else if(ROI_TYPE==ROI.ROI_TYPE_CIRCLE)
            {
                double row, column, radius;
                hv_window.DrawCircle(out row, out column, out radius);
                if(row==0&&column==0&&radius==0)
                {
                    Console.WriteLine("空的ROI");
                }
                else
                {
                    ROICircle _ROI = new ROICircle();
                    _ROI.createROI(column, row, radius);
                    NewROI = _ROI;
                }
            }

            DelaySetViewState(HWndCtrl.MODE_VIEW_MOVE);
            //hv_window.DispObj(hr);
            return NewROI;
        }
        public async void DelaySetViewState(int _mode)
        {
            await Task.Delay(500);
            viewController.setViewState(_mode);
        }


        private void WPF_HWindow_HMouseDown(object sender, HalconDotNet.HMouseEventArgsWPF e)
        {
            //Console.WriteLine("col:" + e.Column + ",row:" + e.Row);
            //Console.WriteLine("X:" + e.X + ",Y:" + e.Y);
            //Console.WriteLine("1");
        }

        private void WPF_HWindow_HMouseMove(object sender, HMouseEventArgsWPF e)
        {
            //Console.WriteLine("col:" + e.Column + ",row:" + e.Row);
            //Console.WriteLine("X:" + e.X + ",Y:" + e.Y);
            //coordinate.Content = "(" + e.Column + "," + e.Row + ")";
        }
        long MouseTicks = 0;
        private void WPF_HWindow_HMouseUp(object sender, HMouseEventArgsWPF e)
        {
            long _CurTicks = DateTime.Now.Ticks;
            long _Interval = _CurTicks - MouseTicks;
            //Console.WriteLine(_s);
            if (_Interval < 5000000)//500ms
            {
                MouseTicks = 0;
                if(_DoubleClickEvent!=null)
                    _DoubleClickEvent(sender, e);
            }
            else
                MouseTicks = _CurTicks;

            if (e.Button == MouseButton.Right && viewController.getViewState() != HWndCtrl.MODE_ROI_Create)
                WPF_HWindow_ContextMenu.IsOpen = true;
        }


        public delegate void DoubleClickDelegate(object sender, HMouseEventArgsWPF e);
        private event DoubleClickDelegate _DoubleClickEvent;

        public event DoubleClickDelegate DoubleClickEvent
        {
            add
            {
                _DoubleClickEvent += value;
            }
            remove
            {
                _DoubleClickEvent -= value;
            }
        }
    }
}
