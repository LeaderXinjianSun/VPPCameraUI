using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;
using System.Diagnostics;
using HalconViewer;
using HalconDotNet;
using ViewROI;

namespace HalconViewer
{
    //为了统一，从GW137项目（2019/12/1）开始，图像Row为X，Col为Y（之前是反的）
    public static class VisionManager
    {
        /*
        //公用的模板ID
        public static HTuple ShapeModelID = null;
        /// <summary>
        /// 公用的创建识别模板函数
        /// </summary>
        /// <param name="_ImageViewer">指定的图像窗口，显示模板结果</param>
        public static void CreateShapeModel(ImageViewer _ImageViewer)
        {
            _ImageViewer.AppendHObject = null;

            ROIRegion r = (ROIRegion)_ImageViewer.DrawROI(ROI.ROI_TYPE_REGION);

            if (r == null)
                return;

            try
            {
                if (ShapeModelID != null)
                    HOperatorSet.ClearShapeModel(ShapeModelID);
                ShapeModelID = null;
            }
            catch { }

            HObject mReduceImage, mShapeModel;
            HOperatorSet.ReduceDomain(_ImageViewer.Image, r.getRegion(), out mReduceImage);
            HOperatorSet.CreateShapeModel(mReduceImage, 5, (new HTuple(0)).TupleRad()
                , (new HTuple(360)).TupleRad(), (new HTuple(2)).TupleRad(), (new HTuple("none")).TupleConcat(
                "no_pregeneration"), "use_polarity", "auto", "auto", out ShapeModelID);
            HOperatorSet.GetShapeModelContours(out mShapeModel, ShapeModelID, 1);


            HTuple area, RefRow, RefColumn, HomMat2D;
            HObject TransContours;
            HOperatorSet.AreaCenter(r.getRegion(), out area, out RefRow, out RefColumn);
            HOperatorSet.VectorAngleToRigid(0, 0, 0, RefRow, RefColumn, 0, out HomMat2D);
            HOperatorSet.AffineTransContourXld(mShapeModel, out TransContours, HomMat2D);


            Tuple<string, object> t = new Tuple<string, object>("Colored", 12);
            Tuple<string, object> t1 = new Tuple<string, object>("DrawMode", "fill");

            _ImageViewer.GCStyle = t;
            _ImageViewer.GCStyle = t1;

            _ImageViewer.AppendHObject = TransContours;



            return;
        }
        /// <summary>
        /// 公用的保存模板函数
        /// </summary>
        /// <param name="_ShapeModelPath">指定的模板路径，保存模板用</param>
        public static void SaveShapeModel(string _ShapeModelPath)
        {
            try
            {
                System.IO.FileInfo _File = new System.IO.FileInfo(_ShapeModelPath);
                System.IO.Directory.CreateDirectory(_File.DirectoryName);
                HOperatorSet.WriteShapeModel(ShapeModelID, _ShapeModelPath);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 查找产品圆心，已过时
        /// </summary>
        /// <param name="_RegionPath"></param>
        /// <param name="_Image"></param>
        /// <param name="_ImageViewer"></param>
        /// <returns></returns>
        public static double[] FindShapeModel(string _RegionPath, HImage _Image, ImageViewer _ImageViewer = null)
        {
            if (_ImageViewer != null)
                _ImageViewer.AppendHObject = null;

            double[] xy = new double[3];
            xy[0] = 0; xy[1] = 0; xy[2] = 0;
            try
            {
                HObject img;
                HOperatorSet.ReduceDomain(_Image, ReadROI(_RegionPath).getRegion(), out img);
                HObject region1, region2;
                HOperatorSet.Threshold(img, out region2, 230, 255);

                HOperatorSet.FillUp(region2, out region1);

                HObject regionopen;
                HOperatorSet.OpeningCircle(region1, out regionopen, 200);
                HObject regionconnect;
                HOperatorSet.Connection(regionopen, out regionconnect);


                HTuple area, centerRow, centerCol;
                HOperatorSet.AreaCenter(regionconnect, out area, out centerRow, out centerCol);


                if (_ImageViewer != null)
                {
                    Tuple<string, object> t = new Tuple<string, object>("Colored", 12);
                    Tuple<string, object> t1 = new Tuple<string, object>("DrawMode", "fill");

                    _ImageViewer.GCStyle = t;
                    _ImageViewer.GCStyle = t1;

                    _ImageViewer.AppendHObject = regionconnect;
                }

                xy[0] = centerRow.D;
                xy[1] = centerCol.D;

                Console.WriteLine(xy[0] + " , " + xy[1]);
            }
            catch
            {
            }


            return xy;
        }
        public static double[] FindShapeModel_o(string _ShapModelPath, string _RegionPath, HImage _Image, ImageViewer _ImageViewer = null, bool _Msg = false)
        {
            if (_ImageViewer != null)
                _ImageViewer.AppendHObject = null;
            //CalibLocation = findCalib(mImageViewer1.Image);
            //Console.WriteLine("X:" + CalibLocation[0] + "Y:" + CalibLocation[1] + "R:" + CalibLocation[2]);


            double[] xy = new double[3];
            xy[0] = 0; xy[1] = 0; xy[2] = 0;
            try
            {
                HObject calibShapeModel, calibModelAtNewPosition;
                HTuple mModelID, calibRowCheck, calibColumnCheck, calibAngleCheck, calibScore, calibMovementOfObject;


                HOperatorSet.ReadShapeModel(_ShapModelPath, out mModelID);

                HObject image;
                if (string.IsNullOrEmpty(_RegionPath))
                    image = _Image;
                else
                    HOperatorSet.ReduceDomain(_Image, ReadROI(_RegionPath).getRegion(), out image);

                HOperatorSet.FindShapeModel(image, mModelID, (new HTuple(-30)).TupleRad(), (new HTuple(60)).TupleRad(), 0.4, 1, 0.5, "least_squares",
       0, 0.5, out calibRowCheck, out calibColumnCheck, out calibAngleCheck, out calibScore);

                HOperatorSet.GetShapeModelContours(out calibShapeModel, mModelID, 1);
                HOperatorSet.VectorAngleToRigid(0, 0, 0, calibRowCheck.TupleSelect(0), calibColumnCheck.TupleSelect(
                            0), calibAngleCheck.TupleSelect(0), out calibMovementOfObject);
                if (_ImageViewer != null)
                {
                    HOperatorSet.AffineTransContourXld(calibShapeModel, out calibModelAtNewPosition, calibMovementOfObject);

                    Tuple<string, object> t = new Tuple<string, object>("Colored", 12);
                    Tuple<string, object> t1 = new Tuple<string, object>("DrawMode", "fill");

                    _ImageViewer.GCStyle = t;
                    _ImageViewer.GCStyle = t1;

                    _ImageViewer.AppendHObject = calibModelAtNewPosition;
                }

                xy[1] = calibColumnCheck.TupleSelect(0);
                xy[0] = calibRowCheck.TupleSelect(0);
                xy[2] = calibAngleCheck.TupleSelect(0);

                xy[2] = (new HTuple(xy[2])).TupleDeg().D;
                xy[2] = Angle_Calc2(xy[2]);

                Console.WriteLine(xy[0] + " , " + xy[1] + "," + xy[2]);
            }
            catch
            {
                if (_Msg)
                    System.Windows.MessageBox.Show("查找失败!");
            }


            return xy;
        }
        /// <summary>
        /// 公用的识别模板函数
        /// </summary>
        /// <param name="_ShapModelPath">使用的模板文件路径</param>
        /// <param name="_RegionPath">使用的ROI文件路径</param>
        /// <param name="_Image">要识别的图像</param>
        /// <param name="_ImageViewer">指定的图像窗口，显示识别结果用</param>
        /// <param name="_Msg">是否弹出MessageBox</param>
        /// <returns></returns>
        public static double[] FindShapeModel(string _ShapModelPath, string _RegionPath, HObject _Image, ImageViewer _ImageViewer = null, bool _Msg = false, double _Score = 0.5)
        {
            if (_ImageViewer != null)
            {
                //_ImageViewer.AppendHObject = null;
                ROI mROI = ReadROI(_RegionPath);
                if (mROI != null)
                    _ImageViewer.ROIList.Add(mROI);
                else
                    _RegionPath = "";
            }
            //CalibLocation = findCalib(mImageViewer1.Image);
            //Console.WriteLine("X:" + CalibLocation[0] + "Y:" + CalibLocation[1] + "R:" + CalibLocation[2]);

            double[] xy = new double[3];
            xy[0] = 0; xy[1] = 0; xy[2] = 0;
            try
            {
                HObject calibShapeModel, calibModelAtNewPosition;
                HTuple mModelID, calibRowCheck, calibColumnCheck, calibAngleCheck, calibScore, calibMovementOfObject;


                HOperatorSet.ReadShapeModel(_ShapModelPath, out mModelID);

                HObject image;
                if (string.IsNullOrEmpty(_RegionPath))
                    image = _Image;
                else
                    HOperatorSet.ReduceDomain(_Image, ReadROI(_RegionPath).getRegion(), out image);

                HOperatorSet.FindShapeModel(image, mModelID, (new HTuple(-30)).TupleRad(), (new HTuple(60)).TupleRad(), _Score, 1, 0.5, "least_squares",
       0, 0.5, out calibRowCheck, out calibColumnCheck, out calibAngleCheck, out calibScore);

                HOperatorSet.GetShapeModelContours(out calibShapeModel, mModelID, 1);
                HOperatorSet.VectorAngleToRigid(0, 0, 0, calibRowCheck.TupleSelect(0), calibColumnCheck.TupleSelect(
                            0), calibAngleCheck.TupleSelect(0), out calibMovementOfObject);
                if (_ImageViewer != null)
                {
                    HOperatorSet.AffineTransContourXld(calibShapeModel, out calibModelAtNewPosition, calibMovementOfObject);

                    Tuple<string, object> t = new Tuple<string, object>("Colored", 12);
                    Tuple<string, object> t1 = new Tuple<string, object>("DrawMode", "fill");

                    _ImageViewer.GCStyle = t;
                    _ImageViewer.GCStyle = t1;

                    _ImageViewer.AppendHObject = calibModelAtNewPosition;
                }

                xy[1] = calibColumnCheck.TupleSelect(0);
                xy[0] = calibRowCheck.TupleSelect(0);
                xy[2] = calibAngleCheck.TupleSelect(0);

                xy[2] = (new HTuple(xy[2])).TupleDeg().D;
                xy[2] = Angle_Calc2(xy[2]);

                Console.WriteLine(xy[0] + " , " + xy[1] + "," + xy[2]);
            }
            catch (Exception ex)
            {
                if (_Msg)
                    System.Windows.MessageBox.Show("查找失败!" + ex.Message);
                else
                    Console.WriteLine("查找失败!" + ex.Message);
            }


            return xy;
        }

        /// <summary>
        /// 查找实心圆，返回中心
        /// </summary>
        /// <param name="_MinGray">最小灰度值</param>
        /// <param name="_MaxGray">最大灰度值</param>
        /// <param name="_MinRadius">最小半径</param>
        /// <param name="_MaxRadius">最大半径</param>
        /// <param name="_RegionPath">识别区域</param>
        /// <param name="_Image">识别图像</param>
        /// <param name="_ImageViewer">图像窗口</param>
        /// <param name="_Msg">是否弹出信息</param>
        /// <returns></returns>
        public static double[] FindCircle(int _MinGray, int _MaxGray, double _MinRadius, double _MaxRadius, string _RegionPath, HObject _Image, ImageViewer _ImageViewer = null, bool _Msg = false)
        {
            if (_ImageViewer != null)
            {
                //_ImageViewer.AppendHObject = null;
                ROI mROI = ReadROI(_RegionPath);
                if (mROI != null)
                    _ImageViewer.ROIList.Add(mROI);
                else
                    _RegionPath = "";
            }
            double[] xy = new double[3];
            xy[0] = 0; xy[1] = 0; xy[2] = 0;

            HObject image = null;
            try
            {
                if (string.IsNullOrEmpty(_RegionPath))
                    image = _Image;
                else
                    HOperatorSet.ReduceDomain(_Image, ReadROI(_RegionPath).getRegion(), out image);
            }
            catch { }

            HObject mRegions = null;
            HObject mConnectedRegions = null;
            HObject mSelectedRegions = null;
            HObject mRegionTrans = null;
            HObject mRegionOpening = null;
            HObject mContours = null;
            HTuple mRadius = null; HTuple mRow = null; HTuple mColumn = null; HTuple mStartPhi = null; HTuple mEndPhi = null; HTuple mPointOrder = null;
            try
            {
                HOperatorSet.Threshold(image, out mRegions, _MinGray, _MaxGray);
                HOperatorSet.Connection(mRegions, out mConnectedRegions);
                HOperatorSet.SelectShape(mConnectedRegions, out mSelectedRegions, new HTuple("outer_radius").TupleConcat("anisometry").TupleConcat("area"), "and", new HTuple(_MinRadius).TupleConcat(1).TupleConcat(Math.PI * _MinRadius * _MinRadius), new HTuple(_MaxRadius).TupleConcat(1.8).TupleConcat(Math.PI * _MaxRadius * _MaxRadius));
                // ['outer_radius','anisometry'], 'and', [50,1], [70,1.8]
                HOperatorSet.ShapeTrans(mSelectedRegions, out mRegionTrans, "outer_circle");
                HOperatorSet.OpeningCircle(mRegionTrans, out mRegionOpening, _MinRadius);
                HOperatorSet.GenContourRegionXld(mRegionOpening, out mContours, "border");
                HOperatorSet.FitCircleContourXld(mContours, "algebraic", -1, 0, 0, 3, 2, out mRow, out mColumn, out mRadius, out mStartPhi, out mEndPhi, out mPointOrder);
            }
            catch { }
            try
            {
                if (mRow.Length != 1)
                {
                    mRegions = null;
                    mConnectedRegions = null;
                    mSelectedRegions = null;
                    mRegionTrans = null;
                    mRegionOpening = null;
                    mContours = null;
                    mRadius = null; mRow = null; mColumn = null; mStartPhi = null; mEndPhi = null; mPointOrder = null;

                    HOperatorSet.Threshold(image, out mRegions, _MinGray - 10, _MaxGray);
                    HOperatorSet.Connection(mRegions, out mConnectedRegions);
                    HOperatorSet.SelectShape(mConnectedRegions, out mSelectedRegions, new HTuple("outer_radius").TupleConcat("anisometry").TupleConcat("area"), "and", new HTuple(_MinRadius).TupleConcat(1).TupleConcat(Math.PI * _MinRadius * _MinRadius), new HTuple(_MaxRadius).TupleConcat(1.8).TupleConcat(Math.PI * _MaxRadius * _MaxRadius));
                    // ['outer_radius','anisometry'], 'and', [50,1], [70,1.8]
                    HOperatorSet.ShapeTrans(mSelectedRegions, out mRegionTrans, "outer_circle");
                    HOperatorSet.OpeningCircle(mRegionTrans, out mRegionOpening, _MinRadius);
                    HOperatorSet.GenContourRegionXld(mRegionOpening, out mContours, "border");
                    HOperatorSet.FitCircleContourXld(mContours, "algebraic", -1, 0, 0, 3, 2, out mRow, out mColumn, out mRadius, out mStartPhi, out mEndPhi, out mPointOrder);
                }
            }
            catch { }
            try
            {
                if (mRow.Length != 2)
                {
                    mRegions = null;
                    mConnectedRegions = null;
                    mSelectedRegions = null;
                    mRegionTrans = null;
                    mRegionOpening = null;
                    mContours = null;
                    mRadius = null; mRow = null; mColumn = null; mStartPhi = null; mEndPhi = null; mPointOrder = null;

                    HOperatorSet.Threshold(image, out mRegions, _MinGray + 10, _MaxGray);
                    HOperatorSet.Connection(mRegions, out mConnectedRegions);
                    HOperatorSet.SelectShape(mConnectedRegions, out mSelectedRegions, new HTuple("outer_radius").TupleConcat("anisometry").TupleConcat("area"), "and", new HTuple(_MinRadius).TupleConcat(1).TupleConcat(Math.PI * _MinRadius * _MinRadius), new HTuple(_MaxRadius).TupleConcat(1.8).TupleConcat(Math.PI * _MaxRadius * _MaxRadius));
                    // ['outer_radius','anisometry'], 'and', [50,1], [70,1.8]
                    HOperatorSet.ShapeTrans(mSelectedRegions, out mRegionTrans, "outer_circle");
                    HOperatorSet.OpeningCircle(mRegionTrans, out mRegionOpening, _MinRadius);
                    HOperatorSet.GenContourRegionXld(mRegionOpening, out mContours, "border");
                    HOperatorSet.FitCircleContourXld(mContours, "algebraic", -1, 0, 0, 3, 2, out mRow, out mColumn, out mRadius, out mStartPhi, out mEndPhi, out mPointOrder);
                }
            }
            catch { }
            try
            {
                if (mRow.Length != 1)
                {
                    mRegions = null;
                    mConnectedRegions = null;
                    mSelectedRegions = null;
                    mRegionTrans = null;
                    mRegionOpening = null;
                    mContours = null;
                    mRadius = null; mRow = null; mColumn = null; mStartPhi = null; mEndPhi = null; mPointOrder = null;

                    HOperatorSet.Threshold(image, out mRegions, _MinGray - 20, _MaxGray);
                    HOperatorSet.Connection(mRegions, out mConnectedRegions);
                    HOperatorSet.SelectShape(mConnectedRegions, out mSelectedRegions, new HTuple("outer_radius").TupleConcat("anisometry").TupleConcat("area"), "and", new HTuple(_MinRadius).TupleConcat(1).TupleConcat(Math.PI * _MinRadius * _MinRadius), new HTuple(_MaxRadius).TupleConcat(1.8).TupleConcat(Math.PI * _MaxRadius * _MaxRadius));
                    // ['outer_radius','anisometry'], 'and', [50,1], [70,1.8]
                    HOperatorSet.ShapeTrans(mSelectedRegions, out mRegionTrans, "outer_circle");
                    HOperatorSet.OpeningCircle(mRegionTrans, out mRegionOpening, _MinRadius);
                    HOperatorSet.GenContourRegionXld(mRegionOpening, out mContours, "border");
                    HOperatorSet.FitCircleContourXld(mContours, "algebraic", -1, 0, 0, 3, 2, out mRow, out mColumn, out mRadius, out mStartPhi, out mEndPhi, out mPointOrder);
                }
            }
            catch { }
            try
            {
                if (mRow.Length != 1)
                {
                    mRegions = null;
                    mConnectedRegions = null;
                    mSelectedRegions = null;
                    mRegionTrans = null;
                    mRegionOpening = null;
                    mContours = null;
                    mRadius = null; mRow = null; mColumn = null; mStartPhi = null; mEndPhi = null; mPointOrder = null;

                    HOperatorSet.Threshold(image, out mRegions, _MinGray + 20, _MaxGray);
                    HOperatorSet.Connection(mRegions, out mConnectedRegions);
                    HOperatorSet.SelectShape(mConnectedRegions, out mSelectedRegions, new HTuple("outer_radius").TupleConcat("anisometry").TupleConcat("area"), "and", new HTuple(_MinRadius).TupleConcat(1).TupleConcat(Math.PI * _MinRadius * _MinRadius), new HTuple(_MaxRadius).TupleConcat(1.8).TupleConcat(Math.PI * _MaxRadius * _MaxRadius));
                    // ['outer_radius','anisometry'], 'and', [50,1], [70,1.8]
                    HOperatorSet.ShapeTrans(mSelectedRegions, out mRegionTrans, "outer_circle");
                    HOperatorSet.OpeningCircle(mRegionTrans, out mRegionOpening, _MinRadius);
                    HOperatorSet.GenContourRegionXld(mRegionOpening, out mContours, "border");
                    HOperatorSet.FitCircleContourXld(mContours, "algebraic", -1, 0, 0, 3, 2, out mRow, out mColumn, out mRadius, out mStartPhi, out mEndPhi, out mPointOrder);
                }
            }
            catch { }
            try
            {
                if (mRow.Length != 0)
                {
                    if (_ImageViewer != null)
                    {
                        Tuple<string, object> t = new Tuple<string, object>("Colored", 12);
                        Tuple<string, object> t1 = new Tuple<string, object>("DrawMode", "fill");

                        _ImageViewer.GCStyle = t;
                        _ImageViewer.GCStyle = t1;

                        _ImageViewer.AppendHObject = mContours;
                    }
                }
                if (mRow.Length == 1)
                {
                    xy[1] = mColumn[0];
                    xy[0] = mRow[0];
                    xy[2] = mRadius[0];
                }
                else
                {
                    if (_Msg)
                        System.Windows.MessageBox.Show("查找失败!圆的数量为" + mRow.Length);
                    else
                        Console.WriteLine("查找失败!圆的数量为" + mRow.Length);
                }

            }
            catch (Exception ex)
            {
                if (_Msg)
                    System.Windows.MessageBox.Show("查找失败!" + ex.Message);
                else
                    Console.WriteLine("查找失败!" + ex.Message);
            }

            return xy;
        }
        //左右一起识别
        public static double[] FindShapeModel(string _ShapModelPath, string _RegionPath, string _ShapModelPath_Right, string _RegionPath_Right, HImage _Image, ImageViewer _ImageViewer = null, bool _Msg = false)
        {
            if (_ImageViewer != null)
            {
                _ImageViewer.AppendHObject = null;
                _ImageViewer.ROIList.Clear();

                _ImageViewer.ROIList.Add(ReadROI(_RegionPath));
                _ImageViewer.ROIList.Add(ReadROI(_RegionPath_Right));
            }
            //CalibLocation = findCalib(mImageViewer1.Image);
            //Console.WriteLine("X:" + CalibLocation[0] + "Y:" + CalibLocation[1] + "R:" + CalibLocation[2]);


            double[] xy = new double[6];

            try
            {
                HObject calibShapeModel, calibModelAtNewPosition;
                HTuple mModelID, calibRowCheck, calibColumnCheck, calibAngleCheck, calibScore, calibMovementOfObject;

                HOperatorSet.ReadShapeModel(_ShapModelPath, out mModelID);


                HObject image;
                if (string.IsNullOrEmpty(_RegionPath))
                    image = _Image;
                else
                    HOperatorSet.ReduceDomain(_Image, ReadROI(_RegionPath).getRegion(), out image);

                HOperatorSet.FindShapeModel(image, mModelID, (new HTuple(-30)).TupleRad(), (new HTuple(60)).TupleRad(), 0.4, 1, 0.5, "least_squares",
       0, 0.5, out calibRowCheck, out calibColumnCheck, out calibAngleCheck, out calibScore);

                HOperatorSet.GetShapeModelContours(out calibShapeModel, mModelID, 1);
                HOperatorSet.VectorAngleToRigid(0, 0, 0, calibRowCheck.TupleSelect(0), calibColumnCheck.TupleSelect(
                            0), calibAngleCheck.TupleSelect(0), out calibMovementOfObject);


                //右
                HObject calibShapeModel_Right, calibModelAtNewPosition_Right;
                HTuple mModelID_Right, calibRowCheck_Right, calibColumnCheck_Right, calibAngleCheck_Right, calibScore_Right, calibMovementOfObject_Right;

                HOperatorSet.ReadShapeModel(_ShapModelPath_Right, out mModelID_Right);

                HObject image_Right;
                if (string.IsNullOrEmpty(_RegionPath_Right))
                    image_Right = _Image;
                else
                    HOperatorSet.ReduceDomain(_Image, ReadROI(_RegionPath_Right).getRegion(), out image_Right);

                HOperatorSet.FindShapeModel(image_Right, mModelID_Right, (new HTuple(-30)).TupleRad(), (new HTuple(60)).TupleRad(), 0.4, 1, 0.5, "least_squares",
       0, 0.5, out calibRowCheck_Right, out calibColumnCheck_Right, out calibAngleCheck_Right, out calibScore_Right);

                HOperatorSet.GetShapeModelContours(out calibShapeModel_Right, mModelID_Right, 1);
                HOperatorSet.VectorAngleToRigid(0, 0, 0, calibRowCheck_Right.TupleSelect(0), calibColumnCheck_Right.TupleSelect(
                            0), calibAngleCheck_Right.TupleSelect(0), out calibMovementOfObject_Right);
                //

                if (_ImageViewer != null)
                {
                    HOperatorSet.AffineTransContourXld(calibShapeModel, out calibModelAtNewPosition, calibMovementOfObject);
                    //右
                    HOperatorSet.AffineTransContourXld(calibShapeModel_Right, out calibModelAtNewPosition_Right, calibMovementOfObject_Right);
                    //

                    Tuple<string, object> t = new Tuple<string, object>("Colored", 12);
                    Tuple<string, object> t1 = new Tuple<string, object>("DrawMode", "fill");

                    _ImageViewer.GCStyle = t;
                    _ImageViewer.GCStyle = t1;

                    _ImageViewer.AppendHObject = calibModelAtNewPosition;
                    //右
                    _ImageViewer.AppendHObject = calibModelAtNewPosition_Right;
                    //
                }

                xy[1] = calibColumnCheck.TupleSelect(0);
                xy[0] = calibRowCheck.TupleSelect(0);
                xy[2] = calibAngleCheck.TupleSelect(0);

                xy[2] = (new HTuple(xy[2])).TupleDeg().D;
                xy[2] = Angle_Calc2(xy[2]);

                Console.WriteLine(xy[0] + " , " + xy[1] + "," + xy[2]);


                //右

                xy[4] = calibColumnCheck_Right.TupleSelect(0);
                xy[3] = calibRowCheck_Right.TupleSelect(0);
                xy[5] = calibAngleCheck_Right.TupleSelect(0);

                xy[5] = (new HTuple(xy[5])).TupleDeg().D;
                xy[5] = Angle_Calc2(xy[5]);

                Console.WriteLine(xy[3] + " , " + xy[4] + "," + xy[5]);
                //
            }
            catch
            {
                if (_Msg)
                    System.Windows.MessageBox.Show("查找失败!");
            }


            return xy;
        }
        /// <summary>
        /// 通用扫码函数
        /// </summary>
        /// <param name="_ScanRegionPath">扫码ROI</param>
        /// <param name="_Image">要识别的图像</param>
        /// <param name="_ImageViewer">指定的图像窗口，显示扫码结果</param>
        /// <returns></returns>
        public static string ScanBarcode(string _ScanRegionPath, HObject _Image, ImageViewer _ImageViewer = null)
        {
            ROI mROI = ReadROI(_ScanRegionPath);
            if (_ImageViewer != null)
            {
                if (mROI != null)
                    _ImageViewer.ROIList.Add(mROI);
            }

            try
            {
                HObject image;
                if (mROI == null)
                    image = _Image;
                else
                    HOperatorSet.ReduceDomain(_Image, mROI.getRegion(), out image);

                HObject xld = null;
                string result = BarCode.GetBarCode(image, ref xld);
                Console.WriteLine("bar:" + result);

                if (_ImageViewer != null)
                {

                    Tuple<string, object> t = new Tuple<string, object>("Colored", 12);
                    Tuple<string, object> t1 = new Tuple<string, object>("DrawMode", "fill");

                    _ImageViewer.GCStyle = t;
                    _ImageViewer.GCStyle = t1;

                    _ImageViewer.AppendHObject = xld;

                    int r1 = 0;
                    int c1 = 0;
                    if (mROI != null)
                    {
                        r1 = (int)(mROI as ROIRectangle1).row1 - 120;
                        if (r1 < 0)
                            r1 = 0;
                        c1 = (int)(mROI as ROIRectangle1).col1 - 0;
                        if (c1 < 0)
                            c1 = 0;
                    }
                    HMsgEntry mhms = new HMsgEntry(result, r1, c1, "green", "image", new HTuple().TupleConcat("box"), new HTuple().TupleConcat("false"));
                    _ImageViewer.AppendHMessage = mhms;
                }
                return result;
            }
            catch (Exception ex) { Console.WriteLine("ScanBarcode:" + ex.Message); }
            return "";
        }

        /// <summary>
        /// 像素坐标转换成实际坐标
        /// </summary>
        /// <param name="_CalibDataPath"></param>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="pr"></param>
        /// <returns></returns>
        public static double[] AffineTransPoint2d(string _CalibDataPath, double px, double py, double pr)
        {
            double[] xy = new double[3];
            xy[0] = 0; xy[1] = 0; xy[2] = 0;
            HTuple CalibData;
            try
            {
                HOperatorSet.ReadTuple(_CalibDataPath, out CalibData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return xy;
            }
            HTuple qx, qy;

            HOperatorSet.AffineTransPoint2d(CalibData, px, py, out qx, out qy);

            xy[0] = qx.D;
            xy[1] = qy.D;
            xy[2] = pr;

            return xy;
        }
        /// <summary>
        /// 创建ROI区域并保存文件
        /// </summary>
        /// <param name="_ROIPath">保存ROI文件的路径</param>
        /// <param name="_ImageViewer">指定的图像窗口</param>
        public static void CreateRegion(string _ROIPath, ImageViewer _ImageViewer, int _ROI_Type = ROI.ROI_TYPE_RECTANGLE1)
        {
            _ImageViewer.ROIList.Clear();
            _ImageViewer.Repaint = !_ImageViewer.Repaint;

            ROI r = _ImageViewer.DrawROI(_ROI_Type);

            if (r == null)
                return;
            r.SizeEnable = false;

            WriteROI(r, _ROIPath);

            if (_ImageViewer.ROIList == null)
                _ImageViewer.ROIList = new System.Collections.ObjectModel.ObservableCollection<ROI>();
            _ImageViewer.ROIList.Add(r);
            _ImageViewer.Repaint = !_ImageViewer.Repaint;

        }
        /// <summary>
        /// 从文件读取ROI区域
        /// </summary>
        /// <param name="_ROIPath">读取ROI文件的路径</param>
        /// <returns>返回ROI</returns>
        public static ROI ReadROI(string _ROIPath)
        {
            ROI mROI = null;
            try
            {
                FileStream fileStream = new FileStream(_ROIPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryFormatter mBinFmat = new BinaryFormatter();
                mROI = (ROI)mBinFmat.Deserialize(fileStream);
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return mROI;
        }
        /// <summary>
        /// 将ROI区域保存到文件
        /// </summary>
        /// <param name="_ROI">要保存的ROI</param>
        /// <param name="_ROIPath">保存ROI文件的路径</param>
        public static void WriteROI(ROI _ROI, string _ROIPath)
        {
            try
            {
                FileStream fileStream = new FileStream(_ROIPath, FileMode.Create);
                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(fileStream, _ROI);
                fileStream.Close();
            }
            catch { }
        }

        public static string CalibContent { get; set; }
        static bool mHasStartCalib = false;
        public static bool HasStartCalib
        {
            get { return mHasStartCalib; }
            set
            {
                mHasStartCalib = value;
                if (mHasStartCalib)
                    CalibContent = "停止标定";
                else
                    CalibContent = "开始标定";
                //this.OnPropertyChanged("CalibContent");
            }
        }

        /// <summary>
        /// 开始标定函数
        /// </summary>
        /// <param name="_ShapeModelPath">标定时的模板文件路径</param>
        /// <param name="_RegionPath">标定时的ROI目录</param>
        /// <param name="_CalibMode">标定模式 9:9点标定;12:9点加旋转标定</param>
        /// <param name="_CalibDataPath">标定文件保存目录</param>
        /// <param name="_CameraName">相机类型</param>
        /// <param name="_ImageViewer">图像窗口</param>
        /// <param name="_Active_Calib_Link">标定时使用的机械手连接</param>
        /// <param name="_CalibAxisX">标定时X坐标</param>
        /// <param name="_CalibAxisY">标定时Y坐标</param>
        public static async void StartCalibration(string _ShapeModelPath, string _RegionPath, int _CalibMode, string _CalibDataPath, string _CameraName, ImageViewer _ImageViewer, DXH.Net.DXHTCPClient _Active_Calib_Link, double _CalibAxisX = 0, double _CalibAxisY = 0)
        {
            if (!HasStartCalib)
                HasStartCalib = true;
            else
            {
                HasStartCalib = false;
                return;
            }

            if (_Active_Calib_Link.ConnectState != "Connected")
                _Active_Calib_Link.StartTCPConnect();

            if (_Active_Calib_Link.ConnectState != "Connected")
            {
                Console.WriteLine("标定程序未连接.");
                await Task.Delay(1000);
            }
            if (_Active_Calib_Link.ConnectState != "Connected")
            {
                Console.WriteLine("标定程序未连接..");
                await Task.Delay(1000);
            }
            if (_Active_Calib_Link.ConnectState != "Connected")
            {
                Console.WriteLine("标定程序未连接...");
                await Task.Delay(1000);
            }
            if (_Active_Calib_Link.ConnectState != "Connected")
            {
                Console.WriteLine("标定程序未连接....");
                await Task.Delay(1000);
            }
            if (_Active_Calib_Link.ConnectState != "Connected")
            {
                Console.WriteLine("标定程序未连接.....");
                Console.WriteLine("标定流程退出");
                HasStartCalib = false;
                return;
            }

            Console.WriteLine("标定程序连接成功");

            string str = _Active_Calib_Link.TCPSend("START\r\n");
            Console.WriteLine(str);
            if (str != "OK\r\n")
            {
                System.Windows.MessageBox.Show("连接机械手失败!");
                HasStartCalib = false;
                return;
            }

            List<double> Px = new List<double>();
            List<double> Py = new List<double>();
            List<double> Qx = new List<double>();
            List<double> Qy = new List<double>();
            List<double> Rx = new List<double>();
            List<double> Ry = new List<double>();

            Px.Clear();
            Py.Clear();
            Qx.Clear();
            Qy.Clear();
            Rx.Clear();
            Ry.Clear();

            HTuple calibHomMat2D;

            int step = 0;
            int curstep = -1;
            double CalibAxisX = 0;
            double CalibAxisY = 0;
            double CalibAxisR = 0;

            double TargetX = 0;
            double TargetY = 0;
            double TargetR = 0;

            while (HasStartCalib)
            {
                bool mMoveResult = true;
                Task WritePLC_Task = Task.Run(() =>
                {
                    if (curstep != step)
                    {
                        curstep = step;
                        if (curstep == 0)
                        {
                            TargetX = CalibAxisX;
                            TargetY = CalibAxisY;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 1)
                        {
                            TargetX = CalibAxisX + 5;
                            TargetY = CalibAxisY + 0;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 2)
                        {
                            TargetX = CalibAxisX + 5;
                            TargetY = CalibAxisY + 5;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 3)
                        {
                            TargetX = CalibAxisX + 0;
                            TargetY = CalibAxisY + 5;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 4)
                        {
                            TargetX = CalibAxisX - 5;
                            TargetY = CalibAxisY + 5;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 5)
                        {
                            TargetX = CalibAxisX - 5;
                            TargetY = CalibAxisY - 0;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 6)
                        {
                            TargetX = CalibAxisX - 5;
                            TargetY = CalibAxisY - 5;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 7)
                        {
                            TargetX = CalibAxisX - 0;
                            TargetY = CalibAxisY - 5;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 8)
                        {
                            TargetX = CalibAxisX + 5;
                            TargetY = CalibAxisY - 5;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 9)
                        {
                            TargetX = CalibAxisX;
                            TargetY = CalibAxisY;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 10)
                        {
                            TargetX = CalibAxisX;
                            TargetY = CalibAxisY;
                            TargetR = CalibAxisR + 5;
                        }
                        else if (curstep == 11)
                        {
                            TargetX = CalibAxisX;
                            TargetY = CalibAxisY;
                            TargetR = CalibAxisR - 5;
                        }
                        Console.WriteLine("X:" + TargetX + " Y:" + TargetY + " R:" + TargetR);

                        string mMotionStr = _Active_Calib_Link.TCPSend(TargetX + "," + TargetY + "," + TargetR + "\r\n", true, 3000);
                        Console.WriteLine(mMotionStr);

                    }
                });
                await WritePLC_Task;

                await Task.Delay(3000);
                if (mMoveResult)
                {
                    //Task Task_AcquireImage = Task.Run(() =>
                    //{
                    if (_CameraName == "上相机")
                    {
                        try
                        {
                            CameraManager.Camera_1_AcquireImage(_ImageViewer);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("Camera_1_AcquireImage:" + ex.Message);
                        }
                    }
                    else if (_CameraName == "下相机")
                    {
                        try
                        {
                            CameraManager.Camera_2_AcquireImage(_ImageViewer);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("Camera_2_AcquireImage:" + ex.Message);
                        }
                    }
                    //});
                    //await Task_AcquireImage;

                    DXH.WPF.DXHWPF.DoEvents();
                    await Task.Delay(1);

                    Task Task_SaveImage = Task.Run(() =>
                    {
                        if (_CameraName == "上相机")
                        {
                            try
                            {
                                CameraManager.Camera_1_SaveImage(_ShapeModelPath.Remove(_ShapeModelPath.LastIndexOf("\\")) + "\\Calib_1\\" + step + ".png");
                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show("Camera_1_SaveImage:" + ex.Message);
                            }
                        }
                        else if (_CameraName == "下相机")
                        {
                            try
                            {
                                CameraManager.Camera_2_SaveImage(_ShapeModelPath.Remove(_ShapeModelPath.LastIndexOf("\\")) + "\\Calib_2\\" + step + ".png");
                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show("Camera_2_SaveImage:" + ex.Message);
                            }
                        }
                    });
                    await Task_SaveImage;

                    double[] xy = FindShapeModel(_ShapeModelPath, _RegionPath, _ImageViewer.Image, _ImageViewer, true);

                    Console.WriteLine("第" + (step + 1) + "步: ");
                    step++;
                    DXH.WPF.DXHWPF.DoEvents();
                    await Task.Delay(1);

                    if (xy[0] == 0 && xy[1] == 0)
                    {
                        Console.WriteLine("相机标定失败!");
                        Console.WriteLine("相机标定失败!");
                        HasStartCalib = false;
                        break;
                    }
                    if (step <= 9)
                    {
                        Px.Add(xy[0]);
                        Py.Add(xy[1]);

                        Qx.Add(TargetX + _CalibAxisX);
                        Qy.Add(TargetY + _CalibAxisY);
                    }
                    else
                    {
                        Rx.Add(xy[0]);
                        Ry.Add(xy[1]);
                    }

                    //await Task.Delay(500);

                    //mImageViewer1.AppendHObject = null;

                    if (step >= _CalibMode)
                    {
                        if (_CalibMode > 9)
                            HOperatorSet.VectorToHomMat2d(
                                    ((((((new HTuple(Px[0])).TupleConcat(Px[1])).TupleConcat(Px[2])).TupleConcat(Px[3])).TupleConcat(Px[4])).TupleConcat(Px[5])).TupleConcat(Px[6]).TupleConcat(Px[7]).TupleConcat(Px[8]),
                                    ((((((new HTuple(Py[0])).TupleConcat(Py[1])).TupleConcat(Py[2])).TupleConcat(Py[3])).TupleConcat(Py[4])).TupleConcat(Py[5])).TupleConcat(Py[6]).TupleConcat(Py[7]).TupleConcat(Py[8]),
                                    ((((((new HTuple(Qx[0])).TupleConcat(Qx[1])).TupleConcat(Qx[2])).TupleConcat(Qx[3])).TupleConcat(Qx[4])).TupleConcat(Qx[5])).TupleConcat(Qx[6]).TupleConcat(Qx[7]).TupleConcat(Qx[8]),
                                    ((((((new HTuple(Qy[0])).TupleConcat(Qy[1])).TupleConcat(Qy[2])).TupleConcat(Qy[3])).TupleConcat(Qy[4])).TupleConcat(Qy[5])).TupleConcat(Qy[6]).TupleConcat(Qy[7]).TupleConcat(Qy[8]), out calibHomMat2D);
                        else//移动的相机9点标定时，XY移动方向要取反
                            HOperatorSet.VectorToHomMat2d(
                                    ((((((new HTuple(Px[0])).TupleConcat(Px[1])).TupleConcat(Px[2])).TupleConcat(Px[3])).TupleConcat(Px[4])).TupleConcat(Px[5])).TupleConcat(Px[6]).TupleConcat(Px[7]).TupleConcat(Px[8]),
                                    ((((((new HTuple(Py[0])).TupleConcat(Py[1])).TupleConcat(Py[2])).TupleConcat(Py[3])).TupleConcat(Py[4])).TupleConcat(Py[5])).TupleConcat(Py[6]).TupleConcat(Py[7]).TupleConcat(Py[8]),
                                    ((((((new HTuple(Qx[0])).TupleConcat(Qx[5])).TupleConcat(Qx[6])).TupleConcat(Qx[7])).TupleConcat(Qx[8])).TupleConcat(Qx[1])).TupleConcat(Qx[2]).TupleConcat(Qx[3]).TupleConcat(Qx[4]),
                                    ((((((new HTuple(Qy[0])).TupleConcat(Qy[5])).TupleConcat(Qy[6])).TupleConcat(Qy[7])).TupleConcat(Qy[8])).TupleConcat(Qy[1])).TupleConcat(Qy[2]).TupleConcat(Qy[3]).TupleConcat(Qy[4]), out calibHomMat2D);


                        if (_CalibMode > 9)
                        {
                            var roxy = rotateCenter(Rx[0], Ry[0], Rx[1], Ry[1], Rx[2], Ry[2]);

                            HTuple hv_Qx, hv_Qy;
                            HOperatorSet.AffineTransPoint2d(calibHomMat2D, roxy[0], roxy[1], out hv_Qx, out hv_Qy);
                            double bx = Qx[0] - hv_Qx.D;
                            double by = Qy[0] - hv_Qy.D;

                            HOperatorSet.VectorToHomMat2d(
                                    ((((((new HTuple(Px[0])).TupleConcat(Px[1])).TupleConcat(Px[2])).TupleConcat(Px[3])).TupleConcat(Px[4])).TupleConcat(Px[5])).TupleConcat(Px[6]).TupleConcat(Px[7]).TupleConcat(Px[8]),
                                    ((((((new HTuple(Py[0])).TupleConcat(Py[1])).TupleConcat(Py[2])).TupleConcat(Py[3])).TupleConcat(Py[4])).TupleConcat(Py[5])).TupleConcat(Py[6]).TupleConcat(Py[7]).TupleConcat(Py[8]),
                                    ((((((new HTuple(Qx[0] + bx)).TupleConcat(Qx[1] + bx)).TupleConcat(Qx[2] + bx)).TupleConcat(Qx[3] + bx)).TupleConcat(Qx[4] + bx)).TupleConcat(Qx[5] + bx)).TupleConcat(Qx[6] + bx).TupleConcat(Qx[7] + bx).TupleConcat(Qx[8] + bx),
                                    ((((((new HTuple(Qy[0] + by)).TupleConcat(Qy[1] + by)).TupleConcat(Qy[2] + by)).TupleConcat(Qy[3] + by)).TupleConcat(Qy[4] + by)).TupleConcat(Qy[5] + by)).TupleConcat(Qy[6] + by).TupleConcat(Qy[7] + by).TupleConcat(Qy[8] + by), out calibHomMat2D);


                        }
                        HOperatorSet.WriteTuple(calibHomMat2D, _CalibDataPath);

                        Console.WriteLine("相机标定成功!");
                        Console.WriteLine("相机标定成功!");
                        System.Windows.MessageBox.Show("相机标定成功!");
                        HasStartCalib = false;

                        if (_CalibMode == 9)
                        {//固定的相机保存标定时的轴坐标
                            Console.WriteLine("开始保存标定时机械坐标..");
                            string mCalibAxisXPath = _CalibDataPath.Replace("Data.tup", "AxisX.tup");
                            string mCalibAxisYPath = _CalibDataPath.Replace("Data.tup", "AxisY.tup");
                            HTuple mCalibAxisX = new HTuple(CalibAxisX);
                            HTuple mCalibAxisY = new HTuple(CalibAxisY);
                            Console.WriteLine("标定时坐标:" + CalibAxisX + "," + CalibAxisY + "...");
                            HOperatorSet.WriteTuple(mCalibAxisX, mCalibAxisXPath);
                            HOperatorSet.WriteTuple(mCalibAxisY, mCalibAxisYPath);
                            Console.WriteLine("标定时坐标保存完成");
                        }
                        else
                        {//固定的相机保存标定时的轴坐标
                            Console.WriteLine("开始保存标定时机械坐标..");
                            string mCalibAxisXPath = _CalibDataPath.Replace("Data.tup", "AxisX.tup");
                            string mCalibAxisYPath = _CalibDataPath.Replace("Data.tup", "AxisY.tup");
                            HTuple mCalibAxisX = new HTuple(CalibAxisX);
                            HTuple mCalibAxisY = new HTuple(CalibAxisY);
                            Console.WriteLine("标定时坐标:" + CalibAxisX + "," + CalibAxisY + "...");
                            HOperatorSet.WriteTuple(mCalibAxisX, mCalibAxisXPath);
                            HOperatorSet.WriteTuple(mCalibAxisY, mCalibAxisYPath);
                            Console.WriteLine("标定时坐标保存完成");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("轴没有到位");
                    Console.WriteLine("结束");
                    HasStartCalib = false;
                    return;
                }
            }
            Console.WriteLine("结束");
            HasStartCalib = false;

            Task FinishPLC_Task = Task.Run(() =>
            {
                string mMotionStr = _Active_Calib_Link.TCPSend(0 + "," + 0 + "," + 0 + "\r\n", true, 3000);
                Console.WriteLine(mMotionStr);
                System.Threading.Thread.Sleep(3000);
            });
            await FinishPLC_Task;
            if (_CameraName == "上相机")
            {
                try
                {
                    CameraManager.Camera_1_AcquireImage(_ImageViewer);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Camera_1_AcquireImage:" + ex.Message);
                }
            }
            else if (_CameraName == "下相机")
            {
                try
                {
                    CameraManager.Camera_2_AcquireImage(_ImageViewer);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Camera_2_AcquireImage:" + ex.Message);
                }
            }

            try
            {
                _Active_Calib_Link.Close();
            }
            catch { }
        }

        /// <summary>
        /// 读取本地图像标定相机
        /// </summary>
        /// <param name="_CalibImagePath">标定时的图像文件目录</param>
        /// <param name="_ShapeModelPath">标定时的模板文件目录</param>
        /// <param name="_RegionPath">标定时的ROI目录</param>
        /// <param name="_CalibMode">标定模式 9:9点标定;12:9点加旋转标定</param>
        /// <param name="_CalibDataPath">标定文件保存目录</param>
        /// <param name="_CameraName">相机类型</param>
        /// <param name="_ImageViewer">图像窗口</param>
        /// <param name="_CalibAxisX">拍照时X坐标</param>
        /// <param name="_CalibAxisY">拍照时Y坐标</param>
        /// <param name="_CalibAxisR">拍照时R坐标</param>
        public async static void Calibration_WithLocalImage(string _CalibImagePath, string _ShapeModelPath, string _RegionPath, int _CalibMode, string _CalibDataPath, string _CameraName, ImageViewer _ImageViewer, double _CalibAxisX = 0, double _CalibAxisY = 0, double _CalibAxisR = 0, double _CalibOffset = 5)
        {
            if (!HasStartCalib)
                HasStartCalib = true;
            else
            {
                HasStartCalib = false;
                return;
            }

            List<double> Px = new List<double>();
            List<double> Py = new List<double>();
            List<double> Qx = new List<double>();
            List<double> Qy = new List<double>();
            List<double> Rx = new List<double>();
            List<double> Ry = new List<double>();

            Px.Clear();
            Py.Clear();
            Qx.Clear();
            Qy.Clear();
            Rx.Clear();
            Ry.Clear();

            HTuple calibHomMat2D;

            int step = 0;
            int curstep = -1;
            double CalibAxisX = _CalibAxisX;
            double CalibAxisY = _CalibAxisY;
            double CalibAxisR = _CalibAxisR;

            double TargetX = 0;
            double TargetY = 0;
            double TargetR = 0;

            while (HasStartCalib)
            {
                bool mMoveResult = true;
                Task WritePLC_Task = Task.Run(() =>
                {
                    if (curstep != step)
                    {
                        curstep = step;
                        if (curstep == 0)
                        {
                            TargetX = CalibAxisX;
                            TargetY = CalibAxisY;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 1)
                        {
                            TargetX = CalibAxisX + _CalibOffset;
                            TargetY = CalibAxisY + 0;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 2)
                        {
                            TargetX = CalibAxisX + _CalibOffset;
                            TargetY = CalibAxisY + _CalibOffset;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 3)
                        {
                            TargetX = CalibAxisX + 0;
                            TargetY = CalibAxisY + _CalibOffset;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 4)
                        {
                            TargetX = CalibAxisX - _CalibOffset;
                            TargetY = CalibAxisY + _CalibOffset;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 5)
                        {
                            TargetX = CalibAxisX - _CalibOffset;
                            TargetY = CalibAxisY - 0;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 6)
                        {
                            TargetX = CalibAxisX - _CalibOffset;
                            TargetY = CalibAxisY - _CalibOffset;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 7)
                        {
                            TargetX = CalibAxisX - 0;
                            TargetY = CalibAxisY - _CalibOffset;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 8)
                        {
                            TargetX = CalibAxisX + _CalibOffset;
                            TargetY = CalibAxisY - _CalibOffset;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 9)
                        {
                            TargetX = CalibAxisX;
                            TargetY = CalibAxisY;
                            TargetR = CalibAxisR;
                        }
                        else if (curstep == 10)
                        {
                            TargetX = CalibAxisX;
                            TargetY = CalibAxisY;
                            TargetR = CalibAxisR + 5;
                        }
                        else if (curstep == 11)
                        {
                            TargetX = CalibAxisX;
                            TargetY = CalibAxisY;
                            TargetR = CalibAxisR - 5;
                        }
                        Console.WriteLine("X:" + TargetX + " Y:" + TargetY + " R:" + TargetR);
                    }
                });
                await WritePLC_Task;

                await Task.Delay(1000);
                if (mMoveResult)
                {
                    if (_CameraName == "上相机")
                    {
                        try
                        {
                            CameraManager.Camera_1_ReadImage(_ImageViewer, _CalibImagePath + curstep);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("Camera_1_ReadImage:" + ex.Message);
                        }
                    }
                    else if (_CameraName == "下相机")
                    {
                        try
                        {
                            CameraManager.Camera_2_ReadImage(_ImageViewer, _CalibImagePath + curstep);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("Camera_2_ReadImage:" + ex.Message);
                        }
                    }

                    DXH.WPF.DXHWPF.DoEvents();
                    await Task.Delay(1);

                    double[] xy = FindShapeModel(_ShapeModelPath, _RegionPath, _ImageViewer.Image, _ImageViewer, true);

                    Console.WriteLine("第" + (step + 1) + "步: ");
                    step++;
                    DXH.WPF.DXHWPF.DoEvents();
                    await Task.Delay(1);

                    if (xy[0] == 0 && xy[1] == 0)
                    {
                        Console.WriteLine("相机标定失败!");
                        Console.WriteLine("相机标定失败!");
                        HasStartCalib = false;
                        break;
                    }
                    if (step <= 9)
                    {
                        Px.Add(xy[0]);
                        Py.Add(xy[1]);

                        Qx.Add(TargetX);
                        Qy.Add(TargetY);
                    }
                    else
                    {
                        Rx.Add(xy[0]);
                        Ry.Add(xy[1]);
                    }

                    if (step >= _CalibMode)
                    {
                        if (_CalibMode > 9)
                            HOperatorSet.VectorToHomMat2d(
                                    ((((((new HTuple(Px[0])).TupleConcat(Px[1])).TupleConcat(Px[2])).TupleConcat(Px[3])).TupleConcat(Px[4])).TupleConcat(Px[5])).TupleConcat(Px[6]).TupleConcat(Px[7]).TupleConcat(Px[8]),
                                    ((((((new HTuple(Py[0])).TupleConcat(Py[1])).TupleConcat(Py[2])).TupleConcat(Py[3])).TupleConcat(Py[4])).TupleConcat(Py[5])).TupleConcat(Py[6]).TupleConcat(Py[7]).TupleConcat(Py[8]),
                                    ((((((new HTuple(Qx[0])).TupleConcat(Qx[1])).TupleConcat(Qx[2])).TupleConcat(Qx[3])).TupleConcat(Qx[4])).TupleConcat(Qx[5])).TupleConcat(Qx[6]).TupleConcat(Qx[7]).TupleConcat(Qx[8]),
                                    ((((((new HTuple(Qy[0])).TupleConcat(Qy[1])).TupleConcat(Qy[2])).TupleConcat(Qy[3])).TupleConcat(Qy[4])).TupleConcat(Qy[5])).TupleConcat(Qy[6]).TupleConcat(Qy[7]).TupleConcat(Qy[8]), out calibHomMat2D);
                        else//移动的相机9点标定时，XY移动方向要取反
                            HOperatorSet.VectorToHomMat2d(
                                    ((((((new HTuple(Px[0])).TupleConcat(Px[1])).TupleConcat(Px[2])).TupleConcat(Px[3])).TupleConcat(Px[4])).TupleConcat(Px[5])).TupleConcat(Px[6]).TupleConcat(Px[7]).TupleConcat(Px[8]),
                                    ((((((new HTuple(Py[0])).TupleConcat(Py[1])).TupleConcat(Py[2])).TupleConcat(Py[3])).TupleConcat(Py[4])).TupleConcat(Py[5])).TupleConcat(Py[6]).TupleConcat(Py[7]).TupleConcat(Py[8]),
                                    ((((((new HTuple(Qx[0])).TupleConcat(Qx[5])).TupleConcat(Qx[6])).TupleConcat(Qx[7])).TupleConcat(Qx[8])).TupleConcat(Qx[1])).TupleConcat(Qx[2]).TupleConcat(Qx[3]).TupleConcat(Qx[4]),
                                    ((((((new HTuple(Qy[0])).TupleConcat(Qy[5])).TupleConcat(Qy[6])).TupleConcat(Qy[7])).TupleConcat(Qy[8])).TupleConcat(Qy[1])).TupleConcat(Qy[2]).TupleConcat(Qy[3]).TupleConcat(Qy[4]), out calibHomMat2D);


                        if (_CalibMode > 9)
                        {
                            var roxy = rotateCenter(Rx[0], Ry[0], Rx[1], Ry[1], Rx[2], Ry[2]);

                            HTuple hv_Qx, hv_Qy;
                            HOperatorSet.AffineTransPoint2d(calibHomMat2D, roxy[0], roxy[1], out hv_Qx, out hv_Qy);
                            double bx = Qx[0] - hv_Qx.D;
                            double by = Qy[0] - hv_Qy.D;

                            HOperatorSet.VectorToHomMat2d(
                                    ((((((new HTuple(Px[0])).TupleConcat(Px[1])).TupleConcat(Px[2])).TupleConcat(Px[3])).TupleConcat(Px[4])).TupleConcat(Px[5])).TupleConcat(Px[6]).TupleConcat(Px[7]).TupleConcat(Px[8]),
                                    ((((((new HTuple(Py[0])).TupleConcat(Py[1])).TupleConcat(Py[2])).TupleConcat(Py[3])).TupleConcat(Py[4])).TupleConcat(Py[5])).TupleConcat(Py[6]).TupleConcat(Py[7]).TupleConcat(Py[8]),
                                    ((((((new HTuple(Qx[0] + bx)).TupleConcat(Qx[1] + bx)).TupleConcat(Qx[2] + bx)).TupleConcat(Qx[3] + bx)).TupleConcat(Qx[4] + bx)).TupleConcat(Qx[5] + bx)).TupleConcat(Qx[6] + bx).TupleConcat(Qx[7] + bx).TupleConcat(Qx[8] + bx),
                                    ((((((new HTuple(Qy[0] + by)).TupleConcat(Qy[1] + by)).TupleConcat(Qy[2] + by)).TupleConcat(Qy[3] + by)).TupleConcat(Qy[4] + by)).TupleConcat(Qy[5] + by)).TupleConcat(Qy[6] + by).TupleConcat(Qy[7] + by).TupleConcat(Qy[8] + by), out calibHomMat2D);


                        }
                        HOperatorSet.WriteTuple(calibHomMat2D, _CalibDataPath);

                        Console.WriteLine("相机标定成功!");
                        Console.WriteLine("相机标定成功!");
                        System.Windows.MessageBox.Show("相机标定成功!");
                        HasStartCalib = false;

                        if (_CalibMode == 9)
                        {
                            Console.WriteLine("开始保存标定时机械坐标..");
                            string mCalibAxisXPath = _CalibDataPath.Replace("Data.tup", "AxisX.tup");
                            string mCalibAxisYPath = _CalibDataPath.Replace("Data.tup", "AxisY.tup");
                            HTuple mCalibAxisX = new HTuple(CalibAxisX);
                            HTuple mCalibAxisY = new HTuple(CalibAxisY);
                            Console.WriteLine("标定时坐标:" + CalibAxisX + "," + CalibAxisY + "...");
                            HOperatorSet.WriteTuple(mCalibAxisX, mCalibAxisXPath);
                            HOperatorSet.WriteTuple(mCalibAxisY, mCalibAxisYPath);
                            Console.WriteLine("标定时坐标保存完成");
                        }
                        else
                        {//固定的相机保存标定时的轴坐标
                            Console.WriteLine("开始保存标定时机械坐标..");
                            string mCalibAxisXPath = _CalibDataPath.Replace("Data.tup", "AxisX.tup");
                            string mCalibAxisYPath = _CalibDataPath.Replace("Data.tup", "AxisY.tup");
                            HTuple mCalibAxisX = new HTuple(CalibAxisX);
                            HTuple mCalibAxisY = new HTuple(CalibAxisY);
                            Console.WriteLine("标定时坐标:" + CalibAxisX + "," + CalibAxisY + "...");
                            HOperatorSet.WriteTuple(mCalibAxisX, mCalibAxisXPath);
                            HOperatorSet.WriteTuple(mCalibAxisY, mCalibAxisYPath);
                            Console.WriteLine("标定时坐标保存完成");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("轴没有到位");
                    Console.WriteLine("结束");
                    HasStartCalib = false;
                    return;
                }
            }
            Console.WriteLine("结束");
            HasStartCalib = false;

            Task FinishPLC_Task = Task.Run(() =>
            {
                System.Threading.Thread.Sleep(1000);
            });
            await FinishPLC_Task;
            if (_CameraName == "上相机")
            {
                try
                {
                    CameraManager.Camera_1_ReadImage(_ImageViewer, _CalibImagePath + "0");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Camera_1_ReadImage:" + ex.Message);
                }
            }
            else if (_CameraName == "下相机")
            {
                try
                {
                    CameraManager.Camera_2_ReadImage(_ImageViewer, _CalibImagePath + "0");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Camera_2_ReadImage:" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 三点确定圆心
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        /// <returns></returns>
        private static double[] rotateCenter(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double a, b, c, d, e, f;
            a = 2 * (x2 - x1);
            b = 2 * (y2 - y1);
            c = x2 * x2 + y2 * y2 - x1 * x1 - y1 * y1;
            d = 2 * (x3 - x2);
            e = 2 * (y3 - y2);
            f = x3 * x3 + y3 * y3 - x2 * x2 - y2 * y2;

            double x = (b * f - e * c) / (b * d - e * a);
            double y = (d * c - a * f) / (b * d - e * a);
            double[] xy = new double[2];
            xy[0] = x;
            xy[1] = y;
            return xy;
        }

        /// <summary>
        /// 返回点P围绕点A旋转弧度rad后的坐标
        /// </summary>
        /// <param name="P">待旋转点坐标</param>
        /// <param name="A">旋转中心坐标</param>
        /// <param name="rad">旋转弧度</param>
        /// <param name="isClockwise">true:顺时针/false:逆时针</param>
        /// <returns>旋转后坐标</returns>
        public static double[] RotatePoint(double[] P, double[] A,
            double rad, bool isClockwise = true)
        {
            //点Temp1
            double[] Temp1 = new double[] { P[0] - A[0], P[1] - A[1] };
            //点Temp1到原点的长度
            double lenO2Temp1 = DistanceTo(new double[2] { 0, 0 }, Temp1);
            //∠T1OX弧度
            double angT1OX = radPOX(Temp1[0], Temp1[1]);
            //∠T2OX弧度（T2为T1以O为圆心旋转弧度rad）
            double angT2OX = angT1OX - (isClockwise ? 1 : -1) * rad;
            //点Temp2
            double[] Temp2 = new double[] { lenO2Temp1 * Math.Cos(angT2OX), lenO2Temp1 * Math.Sin(angT2OX) };
            //点Q
            return new double[] { Temp2[0] + A[0], Temp2[1] + A[1] };
        }
        /// <summary>
        /// 计算点P(x,y)与X轴正方向的夹角
        /// </summary>
        /// <param name="x">横坐标</param>
        /// <param name="y">纵坐标</param>
        /// <returns>夹角弧度</returns>
        public static double radPOX(double x, double y)
        {
            //P在(0,0)的情况
            if (x == 0 && y == 0) return 0;

            //P在四个坐标轴上的情况:x正、x负、y正、y负
            if (y == 0 && x > 0) return 0;
            if (y == 0 && x < 0) return Math.PI;
            if (x == 0 && y > 0) return Math.PI / 2;
            if (x == 0 && y < 0) return Math.PI / 2 * 3;

            //点在第一、二、三、四象限时的情况
            if (x > 0 && y > 0) return Math.Atan(y / x);
            if (x < 0 && y > 0) return Math.PI - Math.Atan(y / -x);
            if (x < 0 && y < 0) return Math.PI + Math.Atan(-y / -x);
            if (x > 0 && y < 0) return Math.PI * 2 - Math.Atan(-y / x);

            return 0;
        }
        public static double DistanceTo(double[] P, double[] A)
        {
            return Math.Sqrt((P[0] - A[0]) * (P[0] - A[0]) + (P[1] - A[1]) * (P[1] - A[1]));
        }
        /// <summary>
        /// 将角度转换到-360到360的范围
        /// </summary>
        /// <param name="_Angle"></param>
        /// <returns></returns>
        public static double Angle_Calc(double _Angle)
        {
            int multiple = (int)_Angle / 360;
            double mA = _Angle - (multiple * 360.0);
            return mA;
        }
        /// <summary>
        /// 将角度转换到-180到180的范围
        /// </summary>
        /// <param name="_Angle"></param>
        /// <returns></returns>
        public static double Angle_Calc2(double _Angle)
        {
            int multiple = (int)_Angle / 360;
            double mA = _Angle - (multiple * 360.0);
            if (mA < -180)
                mA = mA + 360;
            else if (mA > 180)
                mA = mA - 360;
            return mA;
        }
        /// <summary>
        /// 返回起始点到终点相对X轴的弧度
        /// </summary>
        /// <param name="_Start_X">起始点X</param>
        /// <param name="_Start_Y">起始点Y</param>
        /// <param name="_End_X">终点X</param>
        /// <param name="_End_Y">终点Y</param>
        /// <returns></returns>
        public static double radRay(double _Start_X, double _Start_Y, double _End_X, double _End_Y)
        {
            double mRay_R = Math.Atan((_Start_Y - _End_Y) / (_Start_X - _End_X));
            return mRay_R;
        }

        /// <summary>
        /// 查找直线
        /// </summary>
        /// <param name="ho_Image"></param>
        /// <param name="hv_Row1"></param>
        /// <param name="hv_Col1"></param>
        /// <param name="hv_Row2"></param>
        /// <param name="hv_Col2"></param>
        /// <param name="hv_Transition"></param>
        /// <param name="hv_Elements"></param>
        /// <param name="hv_ActiveElements"></param>
        /// <param name="hv_DetectHeight"></param>
        /// <param name="hv_Select"></param>
        /// <param name="hv_Sigma"></param>
        /// <param name="hv_Threshold"></param>
        /// <param name="hv_RowL1"></param>
        /// <param name="hv_ColL1"></param>
        /// <param name="hv_RowL2"></param>
        /// <param name="hv_ColL2"></param>
        public static void Fit_Line(HObject ho_Image, HTuple hv_Row1, HTuple hv_Col1, HTuple hv_Row2,
      HTuple hv_Col2, HTuple hv_Transition, HTuple hv_Elements, HTuple hv_ActiveElements,
      HTuple hv_DetectHeight, HTuple hv_Select, HTuple hv_Sigma, HTuple hv_Threshold,
      out HTuple hv_RowL1, out HTuple hv_ColL1, out HTuple hv_RowL2, out HTuple hv_ColL2)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Regions, ho_Rectangle = null, ho_Arrow1 = null;
            HObject ho_Line, ho_Contour = null;

            // Local control variables 

            HTuple hv_Width = null, hv_Height = null, hv_ResultRow = null;
            HTuple hv_ResultColumn = null, hv_ATan = null, hv_Deg1 = null;
            HTuple hv_Deg = null, hv_i = null, hv_RowC = new HTuple();
            HTuple hv_ColC = new HTuple(), hv_Distance = new HTuple();
            HTuple hv_MsrHandle_Measure = new HTuple(), hv_RowEdge = new HTuple();
            HTuple hv_ColEdge = new HTuple(), hv_Amplitude = new HTuple();
            HTuple hv_tRow = new HTuple(), hv_tCol = new HTuple();
            HTuple hv_t = new HTuple(), hv_Number = new HTuple(), hv_j = new HTuple();
            HTuple hv_Length = null, hv_Nr = new HTuple(), hv_Nc = new HTuple();
            HTuple hv_Dist = new HTuple(), hv_Length1 = new HTuple();
            HTuple hv_Select_COPY_INP_TMP = hv_Select.Clone();
            HTuple hv_Transition_COPY_INP_TMP = hv_Transition.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_Arrow1);
            HOperatorSet.GenEmptyObj(out ho_Line);
            HOperatorSet.GenEmptyObj(out ho_Contour);
            hv_RowL1 = new HTuple();
            hv_ColL1 = new HTuple();
            hv_RowL2 = new HTuple();
            hv_ColL2 = new HTuple();
            //Bing
            //20170705
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            ho_Regions.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Regions);
            hv_ResultRow = new HTuple();
            hv_ResultColumn = new HTuple();
            HOperatorSet.TupleAtan2((-hv_Row2) + hv_Row1, hv_Col2 - hv_Col1, out hv_ATan);
            HOperatorSet.TupleDeg(hv_ATan, out hv_Deg1);
            hv_ATan = hv_ATan + ((new HTuple(90)).TupleRad());
            HOperatorSet.TupleDeg(hv_ATan, out hv_Deg);

            HTuple end_val11 = hv_Elements;
            HTuple step_val11 = 1;
            for (hv_i = 1; hv_i.Continue(end_val11, step_val11); hv_i = hv_i.TupleAdd(step_val11))
            {
                hv_RowC = hv_Row1 + (((hv_Row2 - hv_Row1) * hv_i) / (hv_Elements + 1));
                hv_ColC = hv_Col1 + (((hv_Col2 - hv_Col1) * hv_i) / (hv_Elements + 1));
                if ((int)((new HTuple((new HTuple((new HTuple(hv_RowC.TupleGreater(hv_Height - 1))).TupleOr(
                    new HTuple(hv_RowC.TupleLess(0))))).TupleOr(new HTuple(hv_ColC.TupleGreater(
                    hv_Width - 1))))).TupleOr(new HTuple(hv_ColC.TupleLess(0)))) != 0)
                {
                    continue;
                }
                HOperatorSet.DistancePp(hv_Row1, hv_Col1, hv_Row2, hv_Col2, out hv_Distance);
                if ((int)(new HTuple(hv_Elements.TupleEqual(1))) != 0)
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle2ContourXld(out ho_Rectangle, hv_RowC, hv_ColC,
                        hv_Deg.TupleRad(), hv_DetectHeight / 2, hv_Distance / 2);
                }
                else
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle2ContourXld(out ho_Rectangle, hv_RowC, hv_ColC,
                        hv_Deg.TupleRad(), hv_DetectHeight / 2, hv_Distance / hv_Elements);
                }

                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Regions, ho_Rectangle, out ExpTmpOutVar_0);
                    ho_Regions.Dispose();
                    ho_Regions = ExpTmpOutVar_0;
                }
                if ((int)(new HTuple(hv_i.TupleEqual(1))) != 0)
                {
                    hv_RowL2 = hv_RowC + ((hv_DetectHeight / 2) * (((-hv_ATan)).TupleSin()));
                    hv_RowL1 = hv_RowC - ((hv_DetectHeight / 2) * (((-hv_ATan)).TupleSin()));
                    hv_ColL2 = hv_ColC + ((hv_DetectHeight / 2) * (((-hv_ATan)).TupleCos()));
                    hv_ColL1 = hv_ColC - ((hv_DetectHeight / 2) * (((-hv_ATan)).TupleCos()));
                    ho_Arrow1.Dispose();
                    gen_arrow_contour_xld(out ho_Arrow1, hv_RowL1, hv_ColL1, hv_RowL2, hv_ColL2,
                        25, 25);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_Regions, ho_Arrow1, out ExpTmpOutVar_0);
                        ho_Regions.Dispose();
                        ho_Regions = ExpTmpOutVar_0;
                    }
                }
                HOperatorSet.GenMeasureRectangle2(hv_RowC, hv_ColC, hv_Deg.TupleRad(), hv_DetectHeight / 2,
                    (hv_Distance / hv_Elements) / 2, hv_Width, hv_Height, "nearest_neighbor", out hv_MsrHandle_Measure);

                if ((int)(new HTuple(hv_Transition_COPY_INP_TMP.TupleEqual("negative"))) != 0)
                {
                    hv_Transition_COPY_INP_TMP = "negative";
                }
                else
                {
                    if ((int)(new HTuple(hv_Transition_COPY_INP_TMP.TupleEqual("positive"))) != 0)
                    {
                        hv_Transition_COPY_INP_TMP = "positive";
                    }
                    else
                    {
                        hv_Transition_COPY_INP_TMP = "all";
                    }
                }

                if ((int)(new HTuple(hv_Select_COPY_INP_TMP.TupleEqual("first"))) != 0)
                {
                    hv_Select_COPY_INP_TMP = "first";
                }
                else
                {
                    if ((int)(new HTuple(hv_Select_COPY_INP_TMP.TupleEqual("last"))) != 0)
                    {
                        hv_Select_COPY_INP_TMP = "last";
                    }
                    else
                    {
                        hv_Select_COPY_INP_TMP = "all";
                    }
                }

                HOperatorSet.MeasurePos(ho_Image, hv_MsrHandle_Measure, hv_Sigma, hv_Threshold,
                    hv_Transition_COPY_INP_TMP, hv_Select_COPY_INP_TMP, out hv_RowEdge, out hv_ColEdge,
                    out hv_Amplitude, out hv_Distance);

                HOperatorSet.CloseMeasure(hv_MsrHandle_Measure);
                hv_tRow = 0;
                hv_tCol = 0;
                hv_t = 0;
                HOperatorSet.TupleLength(hv_RowEdge, out hv_Number);
                if ((int)(new HTuple(hv_Number.TupleLess(1))) != 0)
                {
                    continue;
                }
                HTuple end_val65 = hv_Number - 1;
                HTuple step_val65 = 1;
                for (hv_j = 0; hv_j.Continue(end_val65, step_val65); hv_j = hv_j.TupleAdd(step_val65))
                {
                    if ((int)(new HTuple(((((hv_Amplitude.TupleSelect(hv_j))).TupleAbs())).TupleGreater(
                        hv_t))) != 0)
                    {

                        hv_tRow = hv_RowEdge.TupleSelect(hv_j);
                        hv_tCol = hv_ColEdge.TupleSelect(hv_j);
                        hv_t = ((hv_Amplitude.TupleSelect(hv_j))).TupleAbs();
                    }
                }
                if ((int)(new HTuple(hv_t.TupleGreater(0))) != 0)
                {

                    hv_ResultRow = hv_ResultRow.TupleConcat(hv_tRow);
                    hv_ResultColumn = hv_ResultColumn.TupleConcat(hv_tCol);
                }
            }
            HOperatorSet.TupleLength(hv_ResultRow, out hv_Number);

            hv_RowL1 = 0;
            hv_ColL1 = 0;
            hv_RowL2 = 0;
            hv_ColL2 = 0;
            ho_Line.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Line);
            HOperatorSet.TupleLength(hv_ResultColumn, out hv_Length);

            if ((int)((new HTuple(hv_Length.TupleGreaterEqual(hv_ActiveElements))).TupleAnd(
                new HTuple(hv_ActiveElements.TupleGreater(1)))) != 0)
            {

                ho_Contour.Dispose();
                HOperatorSet.GenContourPolygonXld(out ho_Contour, hv_ResultRow, hv_ResultColumn);
                HOperatorSet.FitLineContourXld(ho_Contour, "tukey", hv_ActiveElements, 0, 5,
                    2, out hv_RowL1, out hv_ColL1, out hv_RowL2, out hv_ColL2, out hv_Nr, out hv_Nc,
                    out hv_Dist);
                HOperatorSet.TupleLength(hv_Dist, out hv_Length1);
                if ((int)(new HTuple(hv_Length1.TupleLess(1))) != 0)
                {
                    ho_Regions.Dispose();
                    ho_Rectangle.Dispose();
                    ho_Arrow1.Dispose();
                    ho_Line.Dispose();
                    ho_Contour.Dispose();

                    return;
                }
                ho_Line.Dispose();
                HOperatorSet.GenContourPolygonXld(out ho_Line, hv_Row1.TupleConcat(hv_Row2),
                    hv_Col1.TupleConcat(hv_Col2));

            }
            ho_Regions.Dispose();
            ho_Rectangle.Dispose();
            ho_Arrow1.Dispose();
            ho_Line.Dispose();
            ho_Contour.Dispose();

            return;
        }

        public static void gen_arrow_contour_xld(out HObject ho_Arrow, HTuple hv_Row1, HTuple hv_Column1,
      HTuple hv_Row2, HTuple hv_Column2, HTuple hv_HeadLength, HTuple hv_HeadWidth)
        {



            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_TempArrow = null;

            // Local control variables 

            HTuple hv_Length = null, hv_ZeroLengthIndices = null;
            HTuple hv_DR = null, hv_DC = null, hv_HalfHeadWidth = null;
            HTuple hv_RowP1 = null, hv_ColP1 = null, hv_RowP2 = null;
            HTuple hv_ColP2 = null, hv_Index = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Arrow);
            HOperatorSet.GenEmptyObj(out ho_TempArrow);
            //This procedure generates arrow shaped XLD contours,
            //pointing from (Row1, Column1) to (Row2, Column2).
            //If starting and end point are identical, a contour consisting
            //of a single point is returned.
            //
            //input parameteres:
            //Row1, Column1: Coordinates of the arrows' starting points
            //Row2, Column2: Coordinates of the arrows' end points
            //HeadLength, HeadWidth: Size of the arrow heads in pixels
            //
            //output parameter:
            //Arrow: The resulting XLD contour
            //
            //The input tuples Row1, Column1, Row2, and Column2 have to be of
            //the same length.
            //HeadLength and HeadWidth either have to be of the same length as
            //Row1, Column1, Row2, and Column2 or have to be a single element.
            //If one of the above restrictions is violated, an error will occur.
            //
            //
            //Init
            ho_Arrow.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Arrow);
            //
            //Calculate the arrow length
            HOperatorSet.DistancePp(hv_Row1, hv_Column1, hv_Row2, hv_Column2, out hv_Length);
            //
            //Mark arrows with identical start and end point
            //(set Length to -1 to avoid division-by-zero exception)
            hv_ZeroLengthIndices = hv_Length.TupleFind(0);
            if ((int)(new HTuple(hv_ZeroLengthIndices.TupleNotEqual(-1))) != 0)
            {
                if (hv_Length == null)
                    hv_Length = new HTuple();
                hv_Length[hv_ZeroLengthIndices] = -1;
            }
            //
            //Calculate auxiliary variables.
            hv_DR = (1.0 * (hv_Row2 - hv_Row1)) / hv_Length;
            hv_DC = (1.0 * (hv_Column2 - hv_Column1)) / hv_Length;
            hv_HalfHeadWidth = hv_HeadWidth / 2.0;
            //
            //Calculate end points of the arrow head.
            hv_RowP1 = (hv_Row1 + ((hv_Length - hv_HeadLength) * hv_DR)) + (hv_HalfHeadWidth * hv_DC);
            hv_ColP1 = (hv_Column1 + ((hv_Length - hv_HeadLength) * hv_DC)) - (hv_HalfHeadWidth * hv_DR);
            hv_RowP2 = (hv_Row1 + ((hv_Length - hv_HeadLength) * hv_DR)) - (hv_HalfHeadWidth * hv_DC);
            hv_ColP2 = (hv_Column1 + ((hv_Length - hv_HeadLength) * hv_DC)) + (hv_HalfHeadWidth * hv_DR);
            //
            //Finally create output XLD contour for each input point pair
            for (hv_Index = 0; (int)hv_Index <= (int)((new HTuple(hv_Length.TupleLength())) - 1); hv_Index = (int)hv_Index + 1)
            {
                if ((int)(new HTuple(((hv_Length.TupleSelect(hv_Index))).TupleEqual(-1))) != 0)
                {
                    //Create_ single points for arrows with identical start and end point
                    ho_TempArrow.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_TempArrow, hv_Row1.TupleSelect(hv_Index),
                        hv_Column1.TupleSelect(hv_Index));
                }
                else
                {
                    //Create arrow contour
                    ho_TempArrow.Dispose();
                    HOperatorSet.GenContourPolygonXld(out ho_TempArrow, ((((((((((hv_Row1.TupleSelect(
                        hv_Index))).TupleConcat(hv_Row2.TupleSelect(hv_Index)))).TupleConcat(
                        hv_RowP1.TupleSelect(hv_Index)))).TupleConcat(hv_Row2.TupleSelect(hv_Index)))).TupleConcat(
                        hv_RowP2.TupleSelect(hv_Index)))).TupleConcat(hv_Row2.TupleSelect(hv_Index)),
                        ((((((((((hv_Column1.TupleSelect(hv_Index))).TupleConcat(hv_Column2.TupleSelect(
                        hv_Index)))).TupleConcat(hv_ColP1.TupleSelect(hv_Index)))).TupleConcat(
                        hv_Column2.TupleSelect(hv_Index)))).TupleConcat(hv_ColP2.TupleSelect(
                        hv_Index)))).TupleConcat(hv_Column2.TupleSelect(hv_Index)));
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Arrow, ho_TempArrow, out ExpTmpOutVar_0);
                    ho_Arrow.Dispose();
                    ho_Arrow = ExpTmpOutVar_0;
                }
            }
            ho_TempArrow.Dispose();

            return;
        }


        public static double[] FindRectangle(HObject _Image, HObject _HRegion)
        {
            double[] results = new double[3];
            int[] maxGrays = new int[] { 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95 };

            HObject mImage = null;
            HOperatorSet.ReduceDomain(_Image, _HRegion, out mImage);


            for (int i = 0; i < maxGrays.Length; i++)
            {//使用多个灰度值阈值
                HObject mRegions = null;
                HOperatorSet.Threshold(mImage, out mRegions, 0, maxGrays[i]);
                HObject mConnectedRegions = null;
                HOperatorSet.Connection(mRegions, out mConnectedRegions);
                HObject mSelectedRegions = null;
                HOperatorSet.SelectShape(mConnectedRegions, out mSelectedRegions, new HTuple("outer_radius").TupleConcat("anisometry"), "and", new HTuple(50).TupleConcat(1), new HTuple(70).TupleConcat(1.8));
                // ['outer_radius','anisometry'], 'and', [50,1], [70,1.8]
                HObject mRegionTrans = null;
                HOperatorSet.ShapeTrans(mSelectedRegions, out mRegionTrans, "outer_circle");
                HObject mRegionOpening = null;
                HOperatorSet.OpeningCircle(mRegionTrans, out mRegionOpening, 50);
                HObject mContours = null;
                HOperatorSet.GenContourRegionXld(mRegionOpening, out mContours, "border");
                HTuple mRadius, mRow, mColumn, mStartPhi, mEndPhi, mPointOrder;
                HOperatorSet.FitCircleContourXld(mContours, "algebraic", -1, 0, 0, 3, 2, out mRow, out mColumn, out mRadius, out mStartPhi, out mEndPhi, out mPointOrder);


                try
                {
                    results[0] = mRow;
                    results[1] = mColumn;
                    results[2] = mRadius;
                    Console.WriteLine(maxGrays[i]);
                    //找到了就返回
                    break;
                }
                catch { }
            }
            return results;
        }

        public static double[] FindRectangle2(HObject _Image, HObject _HRegion)
        {
            double[] results = new double[3];
            int[] minGrays = new int[] { 30, 35, 40, 45, 50 };

            HObject mImage = null;
            HOperatorSet.ReduceDomain(_Image, _HRegion, out mImage);


            for (int i = 0; i < minGrays.Length; i++)
            {//使用多个灰度值阈值
                HObject mRegions = null;
                HOperatorSet.Threshold(mImage, out mRegions, minGrays[i], 255);
                HObject mConnectedRegions = null;
                HOperatorSet.Connection(mRegions, out mConnectedRegions);
                HObject mSelectedRegions = null;
                HOperatorSet.SelectShape(mConnectedRegions, out mSelectedRegions, new HTuple("outer_radius").TupleConcat("anisometry"), "and", new HTuple(45).TupleConcat(1), new HTuple(70).TupleConcat(1.8));
                // ['outer_radius','anisometry'], 'and', [50,1], [70,1.8]
                HObject mRegionTrans = null;
                HOperatorSet.ShapeTrans(mSelectedRegions, out mRegionTrans, "outer_circle");
                HObject mRegionOpening = null;
                HOperatorSet.OpeningCircle(mRegionTrans, out mRegionOpening, 45);
                HObject mContours = null;
                HOperatorSet.GenContourRegionXld(mRegionOpening, out mContours, "border");
                HTuple mRadius, mRow, mColumn, mStartPhi, mEndPhi, mPointOrder;
                HOperatorSet.FitCircleContourXld(mContours, "algebraic", -1, 0, 0, 3, 2, out mRow, out mColumn, out mRadius, out mStartPhi, out mEndPhi, out mPointOrder);


                try
                {
                    results[0] = mRow;
                    results[1] = mColumn;
                    results[2] = mRadius;
                    Console.WriteLine(minGrays[i]);
                    //找到了就返回
                    break;
                }
                catch { }
            }
            return results;
        }

        /// <summary>
        /// 找直线
        /// </summary>
        /// <param name="image">当前图像</param>
        /// <param name="Row1">检测直线起点</param>
        /// <param name="Col1">检测直线起点</param>
        /// <param name="Row2">检测直线终点</param>
        /// <param name="Col2">检测直线终点</param>
        /// <param name="Transition">检测变换，从直线的右侧往左侧识别，positive(黑色到白色),negative(白色到黑色),all(所有)</param>
        /// <param name="Elements">检测元素个数</param>
        /// <param name="ActiveElements">生效的元素个数</param>
        /// <param name="DetectHeight">检测范围高度</param>
        /// <param name="Select">选择直线，first,last,max</param>
        /// <param name="Sigma">Sigma,默认设置1即可</param>
        /// <param name="Threshold">阈值，对比度高则高，反之则低，经验值30</param>
        /// <returns></returns>
        public static double[] FindLine(HObject image, double Row1, double Col1, double Row2, double Col2, string Transition, int Elements, int ActiveElements, double DetectHeight, string Select, double Sigma, int Threshold)
        {
            double[] results = new double[5];

            // Stack for temporary objects
            HObject[] OTemp = new HObject[20];

            // Local iconic variables

            HObject ho_Regions, ho_Rectangle = null, ho_Arrow1 = null;
            HObject ho_Line, ho_Contour = null;

            // Local control variables

            HTuple hv_Width = null, hv_Height = null, hv_ResultRow = null;
            HTuple hv_ResultColumn = null, hv_ATan = null, hv_Deg1 = null;
            HTuple hv_Deg = null, hv_i = null, hv_RowC = new HTuple();
            HTuple hv_ColC = new HTuple(), hv_Distance = new HTuple();
            HTuple hv_MsrHandle_Measure = new HTuple(), hv_RowEdge = new HTuple();
            HTuple hv_ColEdge = new HTuple(), hv_Amplitude = new HTuple();
            HTuple hv_tRow = new HTuple(), hv_tCol = new HTuple();
            HTuple hv_t = new HTuple(), hv_Number = new HTuple(), hv_j = new HTuple();
            HTuple hv_Length = null, hv_Nr = new HTuple(), hv_Nc = new HTuple();
            HTuple hv_Dist = new HTuple(), hv_Length1 = new HTuple();
            HTuple hv_Select_COPY_INP_TMP = Select;
            HTuple hv_Transition_COPY_INP_TMP = Transition;

            // Initialize local and output iconic variables
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_Arrow1);
            HOperatorSet.GenEmptyObj(out ho_Line);
            HOperatorSet.GenEmptyObj(out ho_Contour);
            HTuple hv_RowL1 = new HTuple();
            HTuple hv_ColL1 = new HTuple();
            HTuple hv_RowL2 = new HTuple();
            HTuple hv_ColL2 = new HTuple();
            //Bing
            //20170705
            HOperatorSet.GetImageSize(image, out hv_Width, out hv_Height);
            ho_Regions.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Regions);
            hv_ResultRow = new HTuple();
            hv_ResultColumn = new HTuple();
            HOperatorSet.TupleAtan2((-Row2) + Row1, Col2 - Col1, out hv_ATan);
            HOperatorSet.TupleDeg(hv_ATan, out hv_Deg1);
            hv_ATan = hv_ATan + ((new HTuple(90)).TupleRad());
            HOperatorSet.TupleDeg(hv_ATan, out hv_Deg);

            HTuple end_val11 = Elements;
            HTuple step_val11 = 1;
            for (hv_i = 1; hv_i.Continue(end_val11, step_val11); hv_i = hv_i.TupleAdd(step_val11))
            {
                hv_RowC = Row1 + (((Row2 - Row1) * hv_i) / (Elements + 1));
                hv_ColC = Col1 + (((Col2 - Col1) * hv_i) / (Elements + 1));
                if ((int)((new HTuple((new HTuple((new HTuple(hv_RowC.TupleGreater(hv_Height - 1))).TupleOr(
                    new HTuple(hv_RowC.TupleLess(0))))).TupleOr(new HTuple(hv_ColC.TupleGreater(
                    hv_Width - 1))))).TupleOr(new HTuple(hv_ColC.TupleLess(0)))) != 0)
                {
                    continue;
                }
                HOperatorSet.DistancePp(Row1, Col1, Row2, Col2, out hv_Distance);
                if (Elements != 0)
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle2ContourXld(out ho_Rectangle, hv_RowC, hv_ColC,
                        hv_Deg.TupleRad(), DetectHeight / 2, hv_Distance / 2);
                }
                else
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle2ContourXld(out ho_Rectangle, hv_RowC, hv_ColC,
                        hv_Deg.TupleRad(), DetectHeight / 2, hv_Distance / Elements);
                }

                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Regions, ho_Rectangle, out ExpTmpOutVar_0);
                    ho_Regions.Dispose();
                    ho_Regions = ExpTmpOutVar_0;
                }
                if ((int)(new HTuple(hv_i.TupleEqual(1))) != 0)
                {
                    hv_RowL2 = hv_RowC + ((DetectHeight / 2) * (((-hv_ATan)).TupleSin()));
                    hv_RowL1 = hv_RowC - ((DetectHeight / 2) * (((-hv_ATan)).TupleSin()));
                    hv_ColL2 = hv_ColC + ((DetectHeight / 2) * (((-hv_ATan)).TupleCos()));
                    hv_ColL1 = hv_ColC - ((DetectHeight / 2) * (((-hv_ATan)).TupleCos()));
                    ho_Arrow1.Dispose();
                    gen_arrow_contour_xld(out ho_Arrow1, hv_RowL1, hv_ColL1, hv_RowL2, hv_ColL2,
                        25, 25);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_Regions, ho_Arrow1, out ExpTmpOutVar_0);
                        ho_Regions.Dispose();
                        ho_Regions = ExpTmpOutVar_0;
                    }
                }
                HOperatorSet.GenMeasureRectangle2(hv_RowC, hv_ColC, hv_Deg.TupleRad(), DetectHeight / 2,
                    (hv_Distance / Elements) / 2, hv_Width, hv_Height, "nearest_neighbor", out hv_MsrHandle_Measure);

                if ((int)(new HTuple(hv_Transition_COPY_INP_TMP.TupleEqual("negative"))) != 0)
                {
                    hv_Transition_COPY_INP_TMP = "negative";
                }
                else
                {
                    if ((int)(new HTuple(hv_Transition_COPY_INP_TMP.TupleEqual("positive"))) != 0)
                    {
                        hv_Transition_COPY_INP_TMP = "positive";
                    }
                    else
                    {
                        hv_Transition_COPY_INP_TMP = "all";
                    }
                }

                if ((int)(new HTuple(hv_Select_COPY_INP_TMP.TupleEqual("first"))) != 0)
                {
                    hv_Select_COPY_INP_TMP = "first";
                }
                else
                {
                    if ((int)(new HTuple(hv_Select_COPY_INP_TMP.TupleEqual("last"))) != 0)
                    {
                        hv_Select_COPY_INP_TMP = "last";
                    }
                    else
                    {
                        hv_Select_COPY_INP_TMP = "all";
                    }
                }

                HOperatorSet.MeasurePos(image, hv_MsrHandle_Measure, Sigma, Threshold,
                    hv_Transition_COPY_INP_TMP, hv_Select_COPY_INP_TMP, out hv_RowEdge, out hv_ColEdge,
                    out hv_Amplitude, out hv_Distance);

                HOperatorSet.CloseMeasure(hv_MsrHandle_Measure);
                hv_tRow = 0;
                hv_tCol = 0;
                hv_t = 0;
                HOperatorSet.TupleLength(hv_RowEdge, out hv_Number);
                if ((int)(new HTuple(hv_Number.TupleLess(1))) != 0)
                {
                    continue;
                }
                HTuple end_val65 = hv_Number - 1;
                HTuple step_val65 = 1;
                for (hv_j = 0; hv_j.Continue(end_val65, step_val65); hv_j = hv_j.TupleAdd(step_val65))
                {
                    if ((int)(new HTuple(((((hv_Amplitude.TupleSelect(hv_j))).TupleAbs())).TupleGreater(
                        hv_t))) != 0)
                    {
                        hv_tRow = hv_RowEdge.TupleSelect(hv_j);
                        hv_tCol = hv_ColEdge.TupleSelect(hv_j);
                        hv_t = ((hv_Amplitude.TupleSelect(hv_j))).TupleAbs();
                    }
                }
                if ((int)(new HTuple(hv_t.TupleGreater(0))) != 0)
                {
                    hv_ResultRow = hv_ResultRow.TupleConcat(hv_tRow);
                    hv_ResultColumn = hv_ResultColumn.TupleConcat(hv_tCol);
                }
            }
            HOperatorSet.TupleLength(hv_ResultRow, out hv_Number);

            hv_RowL1 = 0;
            hv_ColL1 = 0;
            hv_RowL2 = 0;
            hv_ColL2 = 0;
            ho_Line.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Line);
            HOperatorSet.TupleLength(hv_ResultColumn, out hv_Length);

            if (ActiveElements != 0)
            {
                ho_Contour.Dispose();
                HOperatorSet.GenContourPolygonXld(out ho_Contour, hv_ResultRow, hv_ResultColumn);
                HOperatorSet.FitLineContourXld(ho_Contour, "tukey", ActiveElements, 0, 5,
                    2, out hv_RowL1, out hv_ColL1, out hv_RowL2, out hv_ColL2, out hv_Nr, out hv_Nc,
                    out hv_Dist);
                HOperatorSet.TupleLength(hv_Dist, out hv_Length1);
                if ((int)(new HTuple(hv_Length1.TupleLess(1))) != 0)
                {
                    ho_Regions.Dispose();
                    ho_Rectangle.Dispose();
                    ho_Arrow1.Dispose();
                    ho_Line.Dispose();
                    ho_Contour.Dispose();
                }
                ho_Line.Dispose();
            }
            ho_Regions.Dispose();
            ho_Rectangle.Dispose();
            ho_Arrow1.Dispose();
            ho_Line.Dispose();
            ho_Contour.Dispose();
            HTuple hv_Angle;
            HOperatorSet.AngleLx(hv_RowL1, hv_ColL1, hv_RowL2, hv_ColL2, out hv_Angle);

            results[0] = hv_RowL1.D;
            results[1] = hv_ColL1.D;
            results[2] = hv_RowL2.D;
            results[3] = hv_ColL2.D;
            results[4] = hv_Angle.D;
            return results;
        }

        /// <summary>
        /// 找圆
        /// </summary>
        /// <param name="Image">当前图像</param>
        /// <param name="CRow">检测圆的圆心</param>
        /// <param name="CColumn">检测圆的圆心</param>
        /// <param name="CRadius">检测圆的半径</param>
        /// <param name="Transition">检测变换，positive,negative,all</param>
        /// <param name="Direct">检测方向，inner,outer</param>
        /// <param name="Elements">检测元素个数</param>
        /// <param name="ActiveElements">生效的元素个数</param>
        /// <param name="DetectHeight">检测范围高度</param>
        /// <param name="Select">选择圆，first,last,max</param>
        /// <param name="Sigma">Sigma,默认设1即可</param>
        /// <param name="Threshold">阈值，对比度高则高，反之则低，经验值30</param>
        /// <param name="ArcType">类型，圆弧或圆，arc,circle</param>
        /// <returns></returns>
        public static double[] FindCircle(HObject Image, double CRow, double CColumn, double CRadius, string Transition, string Direct, int Elements, int ActiveElements, double DetectHeight, string Select, double Sigma, int Threshold, string ArcType = "circle", ImageViewer _ImageViewer = null)
        {
            double[] results = new double[3];

            // Stack for temporary objects
            HObject[] OTemp = new HObject[20];

            // Local iconic variables

            HObject ho_Regions, ho_ContCircle, ho_Arrow1 = null;
            HObject ho_Circle, ho_Contour = null;

            // Local control variables

            HTuple hv_Width = null, hv_Height = null;
            HTuple hv_ResultRow = null, hv_ResultColumn = null, hv_RowXLD = null;
            HTuple hv_ColXLD = null, hv_Length = null, hv_Length2 = null;
            HTuple hv_DetectWidth = null, hv_i = null, hv_j = new HTuple();
            HTuple hv_RowE = new HTuple(), hv_ColE = new HTuple();
            HTuple hv_ATan = new HTuple(), hv_RowL2 = new HTuple();
            HTuple hv_RowL1 = new HTuple(), hv_ColL2 = new HTuple();
            HTuple hv_ColL1 = new HTuple(), hv_MsrHandle_Measure = new HTuple();
            HTuple hv_RowEdge = new HTuple(), hv_ColEdge = new HTuple();
            HTuple hv_Amplitude = new HTuple(), hv_Distance = new HTuple();
            HTuple hv_tRow = new HTuple(), hv_tCol = new HTuple();
            HTuple hv_t = new HTuple(), hv_Number = new HTuple(), hv_k = new HTuple();
            HTuple hv_StartPhi = new HTuple(), hv_EndPhi = new HTuple();
            HTuple hv_PointOrder = new HTuple(), hv_Length1 = new HTuple();
            HTuple hv_ArcType_COPY_INP_TMP = ArcType;
            HTuple hv_Elements_COPY_INP_TMP = Elements;
            HTuple hv_Select_COPY_INP_TMP = Select;
            HTuple hv_Transition_COPY_INP_TMP = Transition;

            // Initialize local and output iconic variables
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_ContCircle);
            HOperatorSet.GenEmptyObj(out ho_Arrow1);
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.GenEmptyObj(out ho_Contour);
            HTuple hv_RowCenter = new HTuple();
            HTuple hv_ColCenter = new HTuple();
            HTuple hv_Radius = new HTuple();
            //Bing
            //20170705
            HOperatorSet.GetImageSize(Image, out hv_Width, out hv_Height);
            ho_Regions.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Regions);
            hv_ResultRow = new HTuple();
            hv_ResultColumn = new HTuple();

            ho_ContCircle.Dispose();
            HOperatorSet.GenCircleContourXld(out ho_ContCircle, CRow, CColumn, CRadius,
                0, (new HTuple(360)).TupleRad(), "positive", 3);
            HOperatorSet.GetContourXld(ho_ContCircle, out hv_RowXLD, out hv_ColXLD);
            HOperatorSet.LengthXld(ho_ContCircle, out hv_Length);
            HOperatorSet.TupleLength(hv_ColXLD, out hv_Length2);
            if ((int)(new HTuple(hv_Elements_COPY_INP_TMP.TupleLess(1))) != 0)
            {
                ho_Regions.Dispose();
                ho_ContCircle.Dispose();
                ho_Arrow1.Dispose();
                ho_Circle.Dispose();
                ho_Contour.Dispose();
            }
            hv_DetectWidth = (((new HTuple(360)).TupleRad()) * CRadius) / hv_Elements_COPY_INP_TMP;
            HTuple end_val15 = hv_Elements_COPY_INP_TMP - 1;
            HTuple step_val15 = 1;
            for (hv_i = 0; hv_i.Continue(end_val15, step_val15); hv_i = hv_i.TupleAdd(step_val15))
            {
                if ((int)(new HTuple(((hv_RowXLD.TupleSelect(0))).TupleEqual(hv_RowXLD.TupleSelect(
                    hv_Length2 - 1)))) != 0)
                {
                    HOperatorSet.TupleInt(((1.0 * hv_Length2) / (hv_Elements_COPY_INP_TMP - 1)) * hv_i,
                        out hv_j);
                    hv_ArcType_COPY_INP_TMP = "circle";
                }
                else
                {
                    HOperatorSet.TupleInt(((1.0 * hv_Length2) / (hv_Elements_COPY_INP_TMP - 1)) * hv_i,
                        out hv_j);
                    hv_ArcType_COPY_INP_TMP = "arc";
                }
                if ((int)(new HTuple(hv_j.TupleGreaterEqual(hv_Length2))) != 0)
                {
                    hv_j = hv_Length2 - 1;
                    //continue
                }
                hv_RowE = hv_RowXLD.TupleSelect(hv_j);
                hv_ColE = hv_ColXLD.TupleSelect(hv_j);

                //超出图像区域，不检测，否则容易报异常
                if ((int)((new HTuple((new HTuple((new HTuple(hv_RowE.TupleGreater(hv_Height - 1))).TupleOr(
                    new HTuple(hv_RowE.TupleLess(0))))).TupleOr(new HTuple(hv_ColE.TupleGreater(
                    hv_Width - 1))))).TupleOr(new HTuple(hv_ColE.TupleLess(0)))) != 0)
                {
                    continue;
                }
                if (Direct == "inner")
                {
                    HOperatorSet.TupleAtan2((-hv_RowE) + CRow, hv_ColE - CColumn, out hv_ATan);
                    hv_ATan = ((new HTuple(180)).TupleRad()) + hv_ATan;
                }
                else
                {
                    HOperatorSet.TupleAtan2((-hv_RowE) + CRow, hv_ColE - CColumn, out hv_ATan);
                }

                //gen_rectangle2 (Rectangle1, RowE, ColE, ATan, DetectHeight/2, DetectWidth/2)
                //concat_obj (Regions, Rectangle1, Regions)
                if ((int)(new HTuple(hv_i.TupleEqual(0))) != 0)
                {
                    hv_RowL2 = hv_RowE + ((DetectHeight / 2) * (((-hv_ATan)).TupleSin()));
                    hv_RowL1 = hv_RowE - ((DetectHeight / 2) * (((-hv_ATan)).TupleSin()));
                    hv_ColL2 = hv_ColE + ((DetectHeight / 2) * (((-hv_ATan)).TupleCos()));
                    hv_ColL1 = hv_ColE - ((DetectHeight / 2) * (((-hv_ATan)).TupleCos()));
                    ho_Arrow1.Dispose();
                    gen_arrow_contour_xld(out ho_Arrow1, hv_RowL1, hv_ColL1, hv_RowL2, hv_ColL2,
                        25, 25);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_Regions, ho_Arrow1, out ExpTmpOutVar_0);
                        ho_Regions.Dispose();
                        ho_Regions = ExpTmpOutVar_0;
                    }
                }
                HOperatorSet.GenMeasureRectangle2(hv_RowE, hv_ColE, hv_ATan, DetectHeight / 2,
                    hv_DetectWidth / 2, hv_Width, hv_Height, "nearest_neighbor", out hv_MsrHandle_Measure);

                if ((int)(new HTuple(hv_Transition_COPY_INP_TMP.TupleEqual("negative"))) != 0)
                {
                    hv_Transition_COPY_INP_TMP = "negative";
                }
                else
                {
                    if ((int)(new HTuple(hv_Transition_COPY_INP_TMP.TupleEqual("positive"))) != 0)
                    {
                        hv_Transition_COPY_INP_TMP = "positive";
                    }
                    else
                    {
                        hv_Transition_COPY_INP_TMP = "all";
                    }
                }

                if ((int)(new HTuple(hv_Select_COPY_INP_TMP.TupleEqual("first"))) != 0)
                {
                    hv_Select_COPY_INP_TMP = "first";
                }
                else
                {
                    if ((int)(new HTuple(hv_Select_COPY_INP_TMP.TupleEqual("last"))) != 0)
                    {
                        hv_Select_COPY_INP_TMP = "last";
                    }
                    else
                    {
                        hv_Select_COPY_INP_TMP = "all";
                    }
                }

                HOperatorSet.MeasurePos(Image, hv_MsrHandle_Measure, Sigma, Threshold,
                    hv_Transition_COPY_INP_TMP, hv_Select_COPY_INP_TMP, out hv_RowEdge, out hv_ColEdge,
                    out hv_Amplitude, out hv_Distance);
                HOperatorSet.CloseMeasure(hv_MsrHandle_Measure);
                hv_tRow = 0;
                hv_tCol = 0;
                hv_t = 0;
                HOperatorSet.TupleLength(hv_RowEdge, out hv_Number);
                if ((int)(new HTuple(hv_Number.TupleLess(1))) != 0)
                {
                    continue;
                }
                HTuple end_val83 = hv_Number - 1;
                HTuple step_val83 = 1;
                for (hv_k = 0; hv_k.Continue(end_val83, step_val83); hv_k = hv_k.TupleAdd(step_val83))
                {
                    if ((int)(new HTuple(((((hv_Amplitude.TupleSelect(hv_k))).TupleAbs())).TupleGreater(
                        hv_t))) != 0)
                    {
                        hv_tRow = hv_RowEdge.TupleSelect(hv_k);
                        hv_tCol = hv_ColEdge.TupleSelect(hv_k);
                        hv_t = ((hv_Amplitude.TupleSelect(hv_k))).TupleAbs();
                    }
                }
                if ((int)(new HTuple(hv_t.TupleGreater(0))) != 0)
                {
                    hv_ResultRow = hv_ResultRow.TupleConcat(hv_tRow);
                    hv_ResultColumn = hv_ResultColumn.TupleConcat(hv_tCol);
                }
            }

            hv_RowCenter = 0;
            hv_ColCenter = 0;
            hv_Radius = 0;
            hv_Elements_COPY_INP_TMP = ActiveElements;//15;
            ho_Circle.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.TupleLength(hv_ResultColumn, out hv_Length);

            if ((int)((new HTuple(hv_Length.TupleGreaterEqual(hv_Elements_COPY_INP_TMP))).TupleAnd(
                new HTuple(hv_Elements_COPY_INP_TMP.TupleGreater(2)))) != 0)
            {
                ho_Contour.Dispose();
                HOperatorSet.GenContourPolygonXld(out ho_Contour, hv_ResultRow, hv_ResultColumn);
                HOperatorSet.FitCircleContourXld(ho_Contour, "geotukey", -1, 0, 0, 3, 2, out hv_RowCenter,
                    out hv_ColCenter, out hv_Radius, out hv_StartPhi, out hv_EndPhi, out hv_PointOrder);

                if (_ImageViewer != null)
                {
                    _ImageViewer.GCStyle = new Tuple<string, object>("Color", "green");
                    _ImageViewer.AppendHObject = ho_Contour;
                }

                HOperatorSet.TupleLength(hv_StartPhi, out hv_Length1);
                if ((int)(new HTuple(hv_Length1.TupleLess(1))) != 0)
                {
                    ho_Regions.Dispose();
                    ho_ContCircle.Dispose();
                    ho_Arrow1.Dispose();
                    ho_Circle.Dispose();
                    //ho_Contour.Dispose();
                }
                if ((int)(new HTuple(hv_ArcType_COPY_INP_TMP.TupleEqual("arc"))) != 0)
                {
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_Circle, hv_RowCenter, hv_ColCenter,
                        hv_Radius, hv_StartPhi, hv_EndPhi, hv_PointOrder, 1);
                }
                else
                {
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_Circle, hv_RowCenter, hv_ColCenter,
                        hv_Radius, 0, (new HTuple(360)).TupleRad(), hv_PointOrder, 1);
                }
            }

            ho_Regions.Dispose();
            ho_ContCircle.Dispose();
            ho_Arrow1.Dispose();
            ho_Circle.Dispose();
            //ho_Contour.Dispose();
            results[0] = hv_RowCenter.D;
            results[1] = hv_ColCenter.D;
            results[2] = hv_Radius.D;
            return results;
        }


        public static void Fit_Circle(HObject ho_Image, HTuple hv_CRow, HTuple hv_CColumn, HTuple hv_CRadius,
      HTuple hv_Transition, HTuple hv_Direct, HTuple hv_Elements, HTuple hv_ActiveElements,
      HTuple hv_DetectHeight, HTuple hv_Select, HTuple hv_Sigma, HTuple hv_Threshold,
      HTuple hv_ArcType, out HTuple hv_RowCenter, out HTuple hv_ColCenter, out HTuple hv_Radius)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_Regions, ho_ContCircle, ho_Arrow1 = null;
            HObject ho_Circle, ho_Contour = null;

            // Local control variables 

            HTuple hv_Width = null, hv_Height = null;
            HTuple hv_ResultRow = null, hv_ResultColumn = null, hv_RowXLD = null;
            HTuple hv_ColXLD = null, hv_Length = null, hv_Length2 = null;
            HTuple hv_DetectWidth = null, hv_i = null, hv_j = new HTuple();
            HTuple hv_RowE = new HTuple(), hv_ColE = new HTuple();
            HTuple hv_ATan = new HTuple(), hv_RowL2 = new HTuple();
            HTuple hv_RowL1 = new HTuple(), hv_ColL2 = new HTuple();
            HTuple hv_ColL1 = new HTuple(), hv_MsrHandle_Measure = new HTuple();
            HTuple hv_RowEdge = new HTuple(), hv_ColEdge = new HTuple();
            HTuple hv_Amplitude = new HTuple(), hv_Distance = new HTuple();
            HTuple hv_tRow = new HTuple(), hv_tCol = new HTuple();
            HTuple hv_t = new HTuple(), hv_Number = new HTuple(), hv_k = new HTuple();
            HTuple hv_StartPhi = new HTuple(), hv_EndPhi = new HTuple();
            HTuple hv_PointOrder = new HTuple(), hv_Length1 = new HTuple();
            HTuple hv_ArcType_COPY_INP_TMP = hv_ArcType.Clone();
            HTuple hv_Elements_COPY_INP_TMP = hv_Elements.Clone();
            HTuple hv_Select_COPY_INP_TMP = hv_Select.Clone();
            HTuple hv_Transition_COPY_INP_TMP = hv_Transition.Clone();

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_ContCircle);
            HOperatorSet.GenEmptyObj(out ho_Arrow1);
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.GenEmptyObj(out ho_Contour);
            hv_RowCenter = new HTuple();
            hv_ColCenter = new HTuple();
            hv_Radius = new HTuple();
            //Bing
            //20170705
            HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            ho_Regions.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Regions);
            hv_ResultRow = new HTuple();
            hv_ResultColumn = new HTuple();

            ho_ContCircle.Dispose();
            HOperatorSet.GenCircleContourXld(out ho_ContCircle, hv_CRow, hv_CColumn, hv_CRadius,
                0, (new HTuple(360)).TupleRad(), "positive", 3);
            HOperatorSet.GetContourXld(ho_ContCircle, out hv_RowXLD, out hv_ColXLD);
            HOperatorSet.LengthXld(ho_ContCircle, out hv_Length);
            HOperatorSet.TupleLength(hv_ColXLD, out hv_Length2);
            if ((int)(new HTuple(hv_Elements_COPY_INP_TMP.TupleLess(1))) != 0)
            {
                ho_Regions.Dispose();
                ho_ContCircle.Dispose();
                ho_Arrow1.Dispose();
                ho_Circle.Dispose();
                ho_Contour.Dispose();

                return;
            }
            hv_DetectWidth = (((new HTuple(360)).TupleRad()) * hv_CRadius) / hv_Elements_COPY_INP_TMP;
            HTuple end_val15 = hv_Elements_COPY_INP_TMP - 1;
            HTuple step_val15 = 1;
            for (hv_i = 0; hv_i.Continue(end_val15, step_val15); hv_i = hv_i.TupleAdd(step_val15))
            {
                if ((int)(new HTuple(((hv_RowXLD.TupleSelect(0))).TupleEqual(hv_RowXLD.TupleSelect(
                    hv_Length2 - 1)))) != 0)
                {
                    HOperatorSet.TupleInt(((1.0 * hv_Length2) / (hv_Elements_COPY_INP_TMP - 1)) * hv_i,
                        out hv_j);
                    hv_ArcType_COPY_INP_TMP = "circle";
                }
                else
                {
                    HOperatorSet.TupleInt(((1.0 * hv_Length2) / (hv_Elements_COPY_INP_TMP - 1)) * hv_i,
                        out hv_j);
                    hv_ArcType_COPY_INP_TMP = "arc";
                }
                if ((int)(new HTuple(hv_j.TupleGreaterEqual(hv_Length2))) != 0)
                {
                    hv_j = hv_Length2 - 1;
                    //continue
                }
                hv_RowE = hv_RowXLD.TupleSelect(hv_j);
                hv_ColE = hv_ColXLD.TupleSelect(hv_j);

                //超出图像区域，不检测，否则容易报异常
                if ((int)((new HTuple((new HTuple((new HTuple(hv_RowE.TupleGreater(hv_Height - 1))).TupleOr(
                    new HTuple(hv_RowE.TupleLess(0))))).TupleOr(new HTuple(hv_ColE.TupleGreater(
                    hv_Width - 1))))).TupleOr(new HTuple(hv_ColE.TupleLess(0)))) != 0)
                {
                    continue;
                }
                if ((int)(new HTuple(hv_Direct.TupleEqual("inner"))) != 0)
                {
                    HOperatorSet.TupleAtan2((-hv_RowE) + hv_CRow, hv_ColE - hv_CColumn, out hv_ATan);
                    hv_ATan = ((new HTuple(180)).TupleRad()) + hv_ATan;
                }
                else
                {
                    HOperatorSet.TupleAtan2((-hv_RowE) + hv_CRow, hv_ColE - hv_CColumn, out hv_ATan);
                }

                //gen_rectangle2 (Rectangle1, RowE, ColE, ATan, DetectHeight/2, DetectWidth/2)
                //concat_obj (Regions, Rectangle1, Regions)
                if ((int)(new HTuple(hv_i.TupleEqual(0))) != 0)
                {
                    hv_RowL2 = hv_RowE + ((hv_DetectHeight / 2) * (((-hv_ATan)).TupleSin()));
                    hv_RowL1 = hv_RowE - ((hv_DetectHeight / 2) * (((-hv_ATan)).TupleSin()));
                    hv_ColL2 = hv_ColE + ((hv_DetectHeight / 2) * (((-hv_ATan)).TupleCos()));
                    hv_ColL1 = hv_ColE - ((hv_DetectHeight / 2) * (((-hv_ATan)).TupleCos()));
                    ho_Arrow1.Dispose();
                    gen_arrow_contour_xld(out ho_Arrow1, hv_RowL1, hv_ColL1, hv_RowL2, hv_ColL2,
                        25, 25);
                    {
                        HObject ExpTmpOutVar_0;
                        HOperatorSet.ConcatObj(ho_Regions, ho_Arrow1, out ExpTmpOutVar_0);
                        ho_Regions.Dispose();
                        ho_Regions = ExpTmpOutVar_0;
                    }
                }
                HOperatorSet.GenMeasureRectangle2(hv_RowE, hv_ColE, hv_ATan, hv_DetectHeight / 2,
                    hv_DetectWidth / 2, hv_Width, hv_Height, "nearest_neighbor", out hv_MsrHandle_Measure);


                if ((int)(new HTuple(hv_Transition_COPY_INP_TMP.TupleEqual("negative"))) != 0)
                {
                    hv_Transition_COPY_INP_TMP = "negative";
                }
                else
                {
                    if ((int)(new HTuple(hv_Transition_COPY_INP_TMP.TupleEqual("positive"))) != 0)
                    {
                        hv_Transition_COPY_INP_TMP = "positive";
                    }
                    else
                    {
                        hv_Transition_COPY_INP_TMP = "all";
                    }
                }

                if ((int)(new HTuple(hv_Select_COPY_INP_TMP.TupleEqual("first"))) != 0)
                {
                    hv_Select_COPY_INP_TMP = "first";
                }
                else
                {
                    if ((int)(new HTuple(hv_Select_COPY_INP_TMP.TupleEqual("last"))) != 0)
                    {
                        hv_Select_COPY_INP_TMP = "last";
                    }
                    else
                    {
                        hv_Select_COPY_INP_TMP = "all";
                    }
                }

                HOperatorSet.MeasurePos(ho_Image, hv_MsrHandle_Measure, hv_Sigma, hv_Threshold,
                    hv_Transition_COPY_INP_TMP, hv_Select_COPY_INP_TMP, out hv_RowEdge, out hv_ColEdge,
                    out hv_Amplitude, out hv_Distance);
                HOperatorSet.CloseMeasure(hv_MsrHandle_Measure);
                hv_tRow = 0;
                hv_tCol = 0;
                hv_t = 0;
                HOperatorSet.TupleLength(hv_RowEdge, out hv_Number);
                if ((int)(new HTuple(hv_Number.TupleLess(1))) != 0)
                {
                    continue;
                }
                HTuple end_val83 = hv_Number - 1;
                HTuple step_val83 = 1;
                for (hv_k = 0; hv_k.Continue(end_val83, step_val83); hv_k = hv_k.TupleAdd(step_val83))
                {
                    if ((int)(new HTuple(((((hv_Amplitude.TupleSelect(hv_k))).TupleAbs())).TupleGreater(
                        hv_t))) != 0)
                    {
                        hv_tRow = hv_RowEdge.TupleSelect(hv_k);
                        hv_tCol = hv_ColEdge.TupleSelect(hv_k);
                        hv_t = ((hv_Amplitude.TupleSelect(hv_k))).TupleAbs();
                    }
                }
                if ((int)(new HTuple(hv_t.TupleGreater(0))) != 0)
                {

                    hv_ResultRow = hv_ResultRow.TupleConcat(hv_tRow);
                    hv_ResultColumn = hv_ResultColumn.TupleConcat(hv_tCol);
                }
            }



            hv_RowCenter = 0;
            hv_ColCenter = 0;
            hv_Radius = 0;
            hv_Elements_COPY_INP_TMP = 15;
            ho_Circle.Dispose();
            HOperatorSet.GenEmptyObj(out ho_Circle);
            HOperatorSet.TupleLength(hv_ResultColumn, out hv_Length);

            if ((int)((new HTuple(hv_Length.TupleGreaterEqual(hv_Elements_COPY_INP_TMP))).TupleAnd(
                new HTuple(hv_Elements_COPY_INP_TMP.TupleGreater(2)))) != 0)
            {

                ho_Contour.Dispose();
                HOperatorSet.GenContourPolygonXld(out ho_Contour, hv_ResultRow, hv_ResultColumn);
                HOperatorSet.FitCircleContourXld(ho_Contour, "geotukey", -1, 0, 0, 3, 2, out hv_RowCenter,
                    out hv_ColCenter, out hv_Radius, out hv_StartPhi, out hv_EndPhi, out hv_PointOrder);

                HOperatorSet.TupleLength(hv_StartPhi, out hv_Length1);
                if ((int)(new HTuple(hv_Length1.TupleLess(1))) != 0)
                {
                    ho_Regions.Dispose();
                    ho_ContCircle.Dispose();
                    ho_Arrow1.Dispose();
                    ho_Circle.Dispose();
                    ho_Contour.Dispose();

                    return;
                }
                if ((int)(new HTuple(hv_ArcType_COPY_INP_TMP.TupleEqual("arc"))) != 0)
                {
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_Circle, hv_RowCenter, hv_ColCenter,
                        hv_Radius, hv_StartPhi, hv_EndPhi, hv_PointOrder, 1);
                }
                else
                {
                    ho_Circle.Dispose();
                    HOperatorSet.GenCircleContourXld(out ho_Circle, hv_RowCenter, hv_ColCenter,
                        hv_Radius, 0, (new HTuple(360)).TupleRad(), hv_PointOrder, 1);
                }
            }


            ho_Regions.Dispose();
            ho_ContCircle.Dispose();
            ho_Arrow1.Dispose();
            ho_Circle.Dispose();
            ho_Contour.Dispose();

            return;
        }

    */
    }
}
