using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VPPCameraUI.Model;

namespace VPPCameraUI.ViewModel
{
    class MainWindowViewModel : NotificationObject
    {
        #region 属性绑定
        private string version;

        public string Version
        {
            get { return version; }
            set
            {
                version = value;
                this.RaisePropertyChanged("Version");
            }
        }
        private string messageStr;

        public string MessageStr
        {
            get { return messageStr; }
            set
            {
                messageStr = value;
                this.RaisePropertyChanged("MessageStr");
            }
        }
        private string mIC1Page1Visibility;

        public string MIC1Page1Visibility
        {
            get { return mIC1Page1Visibility; }
            set
            {
                mIC1Page1Visibility = value;
                this.RaisePropertyChanged("MIC1Page1Visibility");
            }
        }
        private string axisPageVisibility;

        public string AxisPageVisibility
        {
            get { return axisPageVisibility; }
            set
            {
                axisPageVisibility = value;
                this.RaisePropertyChanged("AxisPageVisibility");
            }
        }
        private double gPos;

        public double GPos
        {
            get { return gPos; }
            set
            {
                gPos = value;
                this.RaisePropertyChanged("GPos");
            }
        }
        private double cPos;

        public double CPos
        {
            get { return cPos; }
            set
            {
                cPos = value;
                this.RaisePropertyChanged("CPos");
            }
        }
        private ObservableCollection<bool> gTS800LimP;

        public ObservableCollection<bool> GTS800LimP
        {
            get { return gTS800LimP; }
            set
            {
                gTS800LimP = value;
                this.RaisePropertyChanged("GTS800LimP");
            }
        }
        private ObservableCollection<bool> gTS800LimN;

        public ObservableCollection<bool> GTS800LimN
        {
            get { return gTS800LimN; }
            set
            {
                gTS800LimN = value;
                this.RaisePropertyChanged("GTS800LimN");
            }
        }
        private ObservableCollection<bool> gTS800Home;

        public ObservableCollection<bool> GTS800Home
        {
            get { return gTS800Home; }
            set
            {
                gTS800Home = value;
                this.RaisePropertyChanged("GTS800Home");
            }
        }
        private ObservableCollection<bool> gTS800Alarm;

        public ObservableCollection<bool> GTS800Alarm
        {
            get { return gTS800Alarm; }
            set
            {
                gTS800Alarm = value;
                this.RaisePropertyChanged("GTS800Alarm");
            }
        }
        private ObservableCollection<bool> gTS800SVN;

        public ObservableCollection<bool> GTS800SVN
        {
            get { return gTS800SVN; }
            set
            {
                gTS800SVN = value;
                this.RaisePropertyChanged("GTS800SVN");
            }
        }
        private ObservableCollection<bool> gTS800RST;

        public ObservableCollection<bool> GTS800RST
        {
            get { return gTS800RST; }
            set
            {
                gTS800RST = value;
                this.RaisePropertyChanged("GTS800RST");
            }
        }
        private ObservableCollection<bool> gTS800Di;

        public ObservableCollection<bool> GTS800Di
        {
            get { return gTS800Di; }
            set
            {
                gTS800Di = value;
                this.RaisePropertyChanged("GTS800Di");
            }
        }
        private ObservableCollection<bool> gTS800Do;

        public ObservableCollection<bool> GTS800Do
        {
            get { return gTS800Do; }
            set
            {
                gTS800Do = value;
                this.RaisePropertyChanged("GTS800Do");
            }
        }
        private double startPos;

        public double StartPos
        {
            get { return startPos; }
            set
            {
                startPos = value;
                this.RaisePropertyChanged("StartPos");
            }
        }
        private double cam1Pos;

        public double Cam1Pos
        {
            get { return cam1Pos; }
            set
            {
                cam1Pos = value;
                this.RaisePropertyChanged("Cam1Pos");
            }
        }
        private double cam2Pos;

        public double Cam2Pos
        {
            get { return cam2Pos; }
            set
            {
                cam2Pos = value;
                this.RaisePropertyChanged("Cam2Pos");
            }
        }
        private double cam3Pos;

        public double Cam3Pos
        {
            get { return cam3Pos; }
            set
            {
                cam3Pos = value;
                this.RaisePropertyChanged("Cam3Pos");
            }
        }
        private double cam4Pos;

        public double Cam4Pos
        {
            get { return cam4Pos; }
            set
            {
                cam4Pos = value;
                this.RaisePropertyChanged("Cam4Pos");
            }
        }
        private double endPos;

        public double EndPos
        {
            get { return endPos; }
            set
            {
                endPos = value;
                this.RaisePropertyChanged("EndPos");
            }
        }


        #endregion
        #region 方法绑定
        public DelegateCommand<object> MenuActionCommand { get; set; }
        public DelegateCommand<object> SvnActionCommand { get; set; }
        public DelegateCommand<object> RstActionCommand { get; set; }
        public DelegateCommand<object> OutActionCommand { get; set; }
        public DelegateCommand AppLoadedEventCommand { get; set; }
        public DelegateCommand Axis_Jog_P_MouseDown_YCommand { get; set; }
        public DelegateCommand Axis_Jog_N_MouseDown_YCommand { get; set; }
        public DelegateCommand Axis_Jog_Stop_YCommand { get; set; }
        public DelegateCommand Axis_Home_YCommand { get; set; }
        public DelegateCommand<object> Axis_TechCommand { get; set; }
        public DelegateCommand<object> Axis_GOCommand { get; set; }
        #endregion
        #region 变量
        private short sRtn;

        public short SRtn
        {
            get { return sRtn; }
            set
            {
                sRtn = value;
                switch (value)
                {
                    case 1:
                        AddMessage("指令执行错误");
                        break;
                    case 2:
                        AddMessage("license 不支持");
                        break;
                    case 7:
                        AddMessage("指令参数错误");
                        break;
                    case -1:
                        AddMessage("主机和运动控制器通讯失败");
                        break;
                    case -6:
                        AddMessage("打开控制器失败");
                        break;
                    case -7:
                        AddMessage("运动控制器没有响应");
                        break;
                    default:
                        break;
                }
            }
        }
        const double MaxSpeed_Y = 100;
        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        uint pClock;
        double _GPos, _CPos;
        bool PauseFlag = false, StopFlag = false, HomeStateY = false;
        #endregion
        #region 构造函数
        public MainWindowViewModel()
        {
            #region 初始化参数
            Version = "20201012";
            MessageStr = "";
            MIC1Page1Visibility = "Visible"; AxisPageVisibility = "Collapsed";
            GTS800LimP = new ObservableCollection<bool>();
            GTS800LimN = new ObservableCollection<bool>();
            GTS800Home = new ObservableCollection<bool>();
            GTS800Alarm = new ObservableCollection<bool>();
            GTS800SVN = new ObservableCollection<bool>();
            GTS800RST = new ObservableCollection<bool>();
            GTS800Di = new ObservableCollection<bool>();
            GTS800Do = new ObservableCollection<bool>();
            for (int i = 0; i < 8; i++)
            {
                GTS800LimP.Add(false);
                GTS800LimN.Add(false);
                GTS800Home.Add(false);
                GTS800Alarm.Add(false);
                GTS800SVN.Add(false);
                GTS800RST.Add(false);

            }
            for (int i = 0; i < 16; i++)
            {
                GTS800Di.Add(false);
                GTS800Do.Add(false);
            }
            try
            {
                StartPos = double.Parse(Inifile.INIGetStringValue(iniParameterPath, "Position", "StartPos", "0"));
                Cam1Pos = double.Parse(Inifile.INIGetStringValue(iniParameterPath, "Position", "Cam1Pos", "0"));
                Cam2Pos = double.Parse(Inifile.INIGetStringValue(iniParameterPath, "Position", "Cam2Pos", "0"));
                Cam3Pos = double.Parse(Inifile.INIGetStringValue(iniParameterPath, "Position", "Cam3Pos", "0"));
                Cam4Pos = double.Parse(Inifile.INIGetStringValue(iniParameterPath, "Position", "Cam4Pos", "0"));
                EndPos = double.Parse(Inifile.INIGetStringValue(iniParameterPath, "Position", "EndPos", "0"));
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }

            #endregion
            #region 初始化轴卡
            int pValue;
            try
            {
                SRtn = gts.mc.GT_Open(0, 0, 1);
                SRtn = gts.mc.GT_Reset(0);
                SRtn = gts.mc.GT_LoadConfig(0, "GTS800.cfg");
                SRtn = gts.mc.GT_ClrSts(0, 1, 8);
                SRtn = gts.mc.GT_GetDi(0, gts.mc.MC_GPI, out pValue);
                for (int i = 0; i < 16; i++)
                {
                    GTS800Di[i] = (pValue & (1 << i)) == 0;
                }
                SRtn = gts.mc.GT_GetDo(0, gts.mc.MC_GPO, out pValue);
                for (int i = 0; i < 16; i++)
                {
                    GTS800Do[i] = (pValue & (1 << i)) == 0;
                }
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
            #endregion
            MenuActionCommand = new DelegateCommand<object>(new Action<object>(this.MenuActionCommandExecute));
            SvnActionCommand = new DelegateCommand<object>(new Action<object>(this.SvnActionCommandExecute));
            RstActionCommand = new DelegateCommand<object>(new Action<object>(this.RstActionCommandExecute));
            OutActionCommand = new DelegateCommand<object>(new Action<object>(this.OutActionCommandExecute));
            AppLoadedEventCommand = new DelegateCommand(new Action(this.AppLoadedEventCommandExecute));
            Axis_Jog_P_MouseDown_YCommand = new DelegateCommand(new Action(this.Axis_Jog_P_MouseDown_YCommandExecute));
            Axis_Jog_N_MouseDown_YCommand = new DelegateCommand(new Action(this.Axis_Jog_N_MouseDown_YCommandExecute));
            Axis_Jog_Stop_YCommand = new DelegateCommand(new Action(this.Axis_Jog_Stop_YCommandExecute));
            Axis_TechCommand = new DelegateCommand<object>(new Action<object>(this.Axis_TechCommandExecute));
            Axis_GOCommand = new DelegateCommand<object>(new Action<object>(this.Axis_GOCommandExecute));
            Axis_Home_YCommand = new DelegateCommand(new Action(this.Axis_Home_YCommandExecute));
        }

        private void Axis_Home_YCommandExecute()
        {
            if (MessageBox.Show("是否回原点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Task.Run(()=> {
                    GTS800SVN[0] = true;
                    SRtn = gts.mc.GT_ClrSts(0, 1, 1);
                    SRtn = gts.mc.GT_AxisOn(0, 1);
                    HomeY(); 
                });
            }
        }

        private void Axis_GOCommandExecute(object obj)
        {
            int pSts;uint pClock; 
            switch (obj.ToString())
            {
                case "0":
                    if (MessageBox.Show("是否运动到起始点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        SRtn = gts.mc.GT_GetSts(0, 1, out pSts, (short)1, out pClock);
                        if (!(System.Convert.ToBoolean(pSts & 0x400) == true))//不在驱动中
                        {
                            AbsMotion(0, 1, (int)(StartPos * 100), MaxSpeed_Y * 0.1);
                        }
                    }
                    break;
                case "1":
                    if (MessageBox.Show("是否运动到拍照1点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        SRtn = gts.mc.GT_GetSts(0, 1, out pSts, (short)1, out pClock);
                        if (!(System.Convert.ToBoolean(pSts & 0x400) == true))//不在驱动中
                        {
                            AbsMotion(0, 1, (int)(Cam1Pos * 100), MaxSpeed_Y * 0.1);
                        }
                    }
                    break;
                case "2":
                    if (MessageBox.Show("是否运动到拍照2点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        SRtn = gts.mc.GT_GetSts(0, 1, out pSts, (short)1, out pClock);
                        if (!(System.Convert.ToBoolean(pSts & 0x400) == true))//不在驱动中
                        {
                            AbsMotion(0, 1, (int)(Cam2Pos * 100), MaxSpeed_Y * 0.1);
                        }
                    }
                    break;
                case "3":
                    if (MessageBox.Show("是否运动到拍照3点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        SRtn = gts.mc.GT_GetSts(0, 1, out pSts, (short)1, out pClock);
                        if (!(System.Convert.ToBoolean(pSts & 0x400) == true))//不在驱动中
                        {
                            AbsMotion(0, 1, (int)(Cam3Pos * 100), MaxSpeed_Y * 0.1);
                        }
                    }
                    break;
                case "4":
                    if (MessageBox.Show("是否运动到拍照4点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        SRtn = gts.mc.GT_GetSts(0, 1, out pSts, (short)1, out pClock);
                        if (!(System.Convert.ToBoolean(pSts & 0x400) == true))//不在驱动中
                        {
                            AbsMotion(0, 1, (int)(Cam4Pos * 100), MaxSpeed_Y * 0.1);
                        }
                    }
                    break;
                case "5":
                    if (MessageBox.Show("是否运动到结束点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        SRtn = gts.mc.GT_GetSts(0, 1, out pSts, (short)1, out pClock);
                        if (!(System.Convert.ToBoolean(pSts & 0x400) == true))//不在驱动中
                        {
                            AbsMotion(0, 1, (int)(EndPos * 100), MaxSpeed_Y * 0.1);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void Axis_TechCommandExecute(object obj)
        {
            switch (obj.ToString())
            {
                case "0":
                    if (MessageBox.Show("是否示教起始点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        StartPos = CPos;
                        Inifile.INIWriteValue(iniParameterPath, "Position", "StartPos", StartPos.ToString());
                    }
                    break;
                case "1":
                    if (MessageBox.Show("是否示教拍照1点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        Cam1Pos = CPos;
                        Inifile.INIWriteValue(iniParameterPath, "Position", "Cam1Pos", Cam1Pos.ToString());
                    }
                    break;
                case "2":
                    if (MessageBox.Show("是否示教拍照2点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        Cam2Pos = CPos;
                        Inifile.INIWriteValue(iniParameterPath, "Position", "Cam2Pos", Cam2Pos.ToString());
                    }
                    break;
                case "3":
                    if (MessageBox.Show("是否示教拍照3点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        Cam3Pos = CPos;
                        Inifile.INIWriteValue(iniParameterPath, "Position", "Cam3Pos", Cam3Pos.ToString());
                    }
                    break;
                case "4":
                    if (MessageBox.Show("是否示教拍照4点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        Cam4Pos = CPos;
                        Inifile.INIWriteValue(iniParameterPath, "Position", "Cam4Pos", Cam4Pos.ToString());
                    }
                    break;
                case "5":
                    if (MessageBox.Show("是否示教结束点？", "确认", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        EndPos = CPos;
                        Inifile.INIWriteValue(iniParameterPath, "Position", "EndPos", EndPos.ToString());
                    }
                    break;
                default:
                    break;
            }
        }

        private async void Axis_Jog_Stop_YCommandExecute()
        {
            SRtn = gts.mc.GT_Stop(0, 1 << (1 - 1), 1 << (1 - 1));
            StopFlag = true;
            await Task.Delay(100);
            StopFlag = false;
        }

        private void Axis_Jog_N_MouseDown_YCommandExecute()
        {
            GTS800SVN[0] = true;
            SRtn = gts.mc.GT_ClrSts(0, 1, 1);
            SRtn = gts.mc.GT_AxisOn(0, 1);
            gts.mc.TJogPrm JogPrm = new gts.mc.TJogPrm();
            SRtn = gts.mc.GT_PrfJog(0, 1);
            SRtn = gts.mc.GT_GetJogPrm(0, 1, out JogPrm);
            JogPrm.acc = 1;
            JogPrm.dec = 1;
            SRtn = gts.mc.GT_SetJogPrm(0, 1, ref JogPrm);
            SRtn = gts.mc.GT_SetVel(0, 1, MaxSpeed_Y * 0.1 * -1); //设置当前轴的目标速度
            SRtn = gts.mc.GT_Update(0, (1 << (1 - 1))); //启动当前轴运动
        }

        private void Axis_Jog_P_MouseDown_YCommandExecute()
        {
            GTS800SVN[0] = true;
            SRtn = gts.mc.GT_ClrSts(0, 1, 1);
            SRtn = gts.mc.GT_AxisOn(0, 1);
            gts.mc.TJogPrm JogPrm = new gts.mc.TJogPrm();
            SRtn = gts.mc.GT_PrfJog(0, 1);
            SRtn = gts.mc.GT_GetJogPrm(0, 1, out JogPrm);
            JogPrm.acc = 1;
            JogPrm.dec = 1;
            SRtn = gts.mc.GT_SetJogPrm(0, 1, ref JogPrm);
            SRtn = gts.mc.GT_SetVel(0, 1, MaxSpeed_Y * 0.1); //设置当前轴的目标速度
            SRtn = gts.mc.GT_Update(0, (1 << (1 - 1))); //启动当前轴运动
        }

        private void AppLoadedEventCommandExecute()
        {
            AddMessage("软件加载完成");
            Run();
            UIRun();
        }

        private void OutActionCommandExecute(object obj)
        {
            short AxisNum = (short)(int.Parse(obj.ToString()) + 1);
            if (GTS800Do[AxisNum - 1])
            {
                SRtn = gts.mc.GT_SetDoBit(0, gts.mc.MC_GPO, AxisNum, 0);
            }
            else
            {
                SRtn = gts.mc.GT_SetDoBit(0, gts.mc.MC_GPO, AxisNum, 1);
            }
        }

        private void RstActionCommandExecute(object obj)
        {
            short AxisNum = (short)(int.Parse(obj.ToString()) + 1);
            if (GTS800RST[AxisNum - 1])
            {
                SRtn = gts.mc.GT_SetDoBit(0, gts.mc.MC_CLEAR, AxisNum, 0);
            }
            else
            {
                SRtn = gts.mc.GT_SetDoBit(0, gts.mc.MC_CLEAR, AxisNum, 1);
            }
        }

        private void SvnActionCommandExecute(object obj)
        {
            short AxisNum = (short)(int.Parse(obj.ToString()) + 1);
            if (GTS800SVN[AxisNum - 1])
            {
                SRtn = gts.mc.GT_AxisOn(0, AxisNum);
            }
            else
            {
                SRtn = gts.mc.GT_AxisOff(0, AxisNum);
            }
        }

        private void MenuActionCommandExecute(object obj)
        {
            switch (obj.ToString())
            {
                case "0":
                    MIC1Page1Visibility = "Visible";
                    AxisPageVisibility = "Collapsed";
                    break;
                case "6":
                    MIC1Page1Visibility = "Collapsed";
                    AxisPageVisibility = "Visible";
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region 自定义函数
        private void AddMessage(string str)
        {
            string[] s = MessageStr.Split('\n');
            if (s.Length > 1000)
            {
                MessageStr = "";
            }
            if (MessageStr != "")
            {
                MessageStr += "\n";
            }
            MessageStr += System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + str;
        }
        private async void Run()
        {
            int pValue;
            while (true)
            {
                try
                {
                    #region IO
                    gts.mc.GT_GetDi(0, gts.mc.MC_GPI, out pValue);
                    for (int i = 0; i < 16; i++)
                    {
                        bool b = (pValue & (1 << i)) == 0;
                        GTS800Di[i] = b;
                    }
                    gts.mc.GT_GetDi(0, gts.mc.MC_LIMIT_NEGATIVE, out pValue);
                    for (int i = 0; i < 8; i++)
                    {
                        GTS800LimN[i] = (pValue & (1 << i)) == 0;
                    }
                    gts.mc.GT_GetDi(0, gts.mc.MC_LIMIT_POSITIVE, out pValue);
                    for (int i = 0; i < 8; i++)
                    {
                        GTS800LimP[i] = (pValue & (1 << i)) == 0;
                    }
                    gts.mc.GT_GetDi(0, gts.mc.MC_HOME, out pValue);
                    for (int i = 0; i < 8; i++)
                    {
                        GTS800Home[i] = (pValue & (1 << i)) == 0;
                    }
                    gts.mc.GT_GetDi(0, gts.mc.MC_ALARM, out pValue);
                    for (int i = 0; i < 8; i++)
                    {
                        GTS800Alarm[i] = (pValue & (1 << i)) == 0;
                    }
                    #endregion
                    #region 轴状态
                    gts.mc.GT_GetPrfPos(0, 1, out _GPos, 1, out pClock);
                    GPos = Math.Round(_GPos / 100, 2);
                    gts.mc.GT_GetEncPos(0, 1, out _CPos, 1, out pClock);
                    CPos = Math.Round(_CPos / 100, 2);

                    #endregion
                }
                catch (Exception ex)
                {
                    AddMessage(ex.Message);
                }
                await Task.Delay(10);
            }
        }
        private async void UIRun()
        {
            bool[] _GTS800SVN = new bool[8];
            while (true)
            {
                try
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (_GTS800SVN[i] != GTS800SVN[i])
                        {
                            if (GTS800SVN[i])
                            {
                                var aa = GetEncPos(0, (short)(i + 1));
                                SRtn = gts.mc.GT_SetPrfPos(0, (short)(i + 1), aa);
                            }
                            _GTS800SVN[i] = GTS800SVN[i];
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddMessage(ex.Message);
                }

                await Task.Delay(100);
            }
        }
        #region 回原点
        void HomeY()
        {
            //if (!GTS800LimN[0])
            //{
            //    SRtn = gts.mc.GT_ZeroPos(0, 1, 1);
            //    AbsMotion(0, 1, -9999999, MaxSpeed_Y * 0.02);
            //    do
            //    {
            //        System.Threading.Thread.Sleep(10);
            //        if (PauseFlag || StopFlag)
            //        {
            //            return;
            //        }
            //    } while (!GTS800LimN[0]);
            //    AxisStop(0, 1);
            //}
            gts.mc.THomePrm tHomePrm;
            SRtn = gts.mc.GT_GetHomePrm(0, 1, out tHomePrm);
            tHomePrm.mode = gts.mc.HOME_MODE_LIMIT_HOME_INDEX;
            tHomePrm.moveDir = -1;
            tHomePrm.indexDir = 1;
            tHomePrm.edge = 1;
            tHomePrm.triggerIndex = -1;
            tHomePrm.velHigh = MaxSpeed_Y * 0.02;
            tHomePrm.velLow = MaxSpeed_Y * 0.002;
            tHomePrm.acc = 0.25;
            tHomePrm.dec = 0.125;
            tHomePrm.searchHomeDistance = 100000;
            tHomePrm.searchIndexDistance = 50000;
            tHomePrm.escapeStep = 250;
            sRtn = gts.mc.GT_GoHome(0, 1, ref tHomePrm);//启动 Smart Home 回原点
            gts.mc.THomeStatus tHomeSts;
            do
            {
                sRtn = gts.mc.GT_GetHomeStatus(0, 1, out tHomeSts);//获取回原点状态
                if (PauseFlag || StopFlag)
                {
                    return;
                }
            } while (tHomeSts.run == 1);
            System.Threading.Thread.Sleep(1000);
            if (PauseFlag || StopFlag)
            {
                return;
            }
            if (tHomeSts.stage == gts.mc.HOME_STAGE_END)
            {
                sRtn = gts.mc.GT_ZeroPos(0, 1, 1);
            }
            HomeStateY = true;
        }
        #endregion
        #region 绝对位置运动
        //ABS绝对运动模式子程序，参数1：轴号；参数2：速度；参数3：相对于原点的位移距离
        /// <summary>
        /// 参数:[卡号0-4],[轴号1-8],[相对于原点的位移距离],[速度]
        /// </summary>
        /// <param name="aCardNum"></param>
        /// <param name="aAxis"></param>
        /// <param name="aPosition"></param>
        /// <param name="Vel"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        void AbsMotion(short aCardNum, short aAxis, int aPosition, double Vel)
        {
            //System.Threading.Thread.Sleep(1);
            gts.mc.TTrapPrm ATrapPrm = new gts.mc.TTrapPrm();
            double Vel_ASpeed = 0;
            gts.mc.GT_PrfTrap(aCardNum, aAxis); //设置指定轴为点位模式
            gts.mc.GT_GetTrapPrm(aCardNum, aAxis, out ATrapPrm); //读取点位模式运动参数
            ATrapPrm.acc = 0.5;//点位运动的加速度。正数，单位：pulse/ms2。
            ATrapPrm.dec = 0.5;//点位运动的减速度。正数，单位：pulse/ms2。
            ATrapPrm.smoothTime = (short)25;

            gts.mc.GT_SetTrapPrm(aCardNum, aAxis, ref ATrapPrm); //设置点位模式运动参数
            Vel_ASpeed = Vel;
            gts.mc.GT_SetPos(aCardNum, aAxis, aPosition); //设置目标位置
            gts.mc.GT_SetVel(aCardNum, aAxis, Vel_ASpeed); //设置目标速度
            gts.mc.GT_Update(aCardNum, (1 << (aAxis - 1))); //启动当前轴运动
        }
        /// <summary>
        /// 判断马达是否停止运动【目标位置和编码器位置差在0.2mm内】
        /// </summary>
        /// <param name="CardNum"></param>
        /// <param name="Axis"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool ZSPD(short CardNum, short Axis, int AxisTmplPos) //'''判断马达是否停止运动
        {
            bool returnValue = false;
            int AxisStatus = 0;
            uint temp_pClock = 0;
            gts.mc.GT_GetSts(CardNum, Axis, out AxisStatus, (short)1, out temp_pClock);
            if (!(System.Convert.ToBoolean(AxisStatus & 0x400) == true))
            {
                if (System.Math.Abs(AxisTmplPos - GetEncPos(CardNum, Axis)) > 10)
                {
                    returnValue = false;
                }
                else
                {
                    returnValue = true;
                }

            }
            else
            {
                returnValue = false;
            }
            return returnValue;
        }
        /// <summary>
        /// 获取编码器位置,单位plus
        /// 参数:[卡号0-4],[轴号1-8]
        /// </summary>
        /// <param name="gCardNum"></param>
        /// <param name="gAxis"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static int GetEncPos(short gCardNum, short gAxis)
        {
            double CarEncPos = 0;
            try
            {
                uint temp_pClock = 0;
                gts.mc.GT_GetEncPos(gCardNum, gAxis, out CarEncPos, 1, out temp_pClock); //读取编码器位置
                return (int)CarEncPos; //强制类型转换
            }
            catch (Exception)
            {
                return 0;
            }
        }
        /// <summary>
        /// 停止轴运动（设置运动速度为0模式）
        /// </summary>
        /// <param name="CardNum"></param>
        /// <param name="mAxis"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public void AxisStop(short CardNum, short mAxis)
        {
            gts.mc.GT_Stop(CardNum, 1 << (mAxis - 1), 1 << (mAxis - 1));
        }
        #endregion
        #endregion
    }
}
