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

namespace VPPCameraUI.View
{
    /// <summary>
    /// MIC1Page3.xaml 的交互逻辑
    /// </summary>
    public partial class MIC1Page3 : UserControl
    {
        public MIC1Page3()
        {
            InitializeComponent();
            Global.MIC1_3ImageViewer = this.MIC1_3ImageViewer;
        }
    }
}
