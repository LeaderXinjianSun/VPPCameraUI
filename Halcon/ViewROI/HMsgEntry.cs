using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;

namespace ViewROI
{
    /// <summary>
    /// 显示字符包含字体设置
    /// </summary>
    public class HMsgEntry
    {
        public string HMsg;
        public string coordSystem;
        public int row;
        public int column;
        public string color;
        public HTuple genParamName;
        public HTuple genParamValue;
        /// <summary>
        /// FontSize,-1为默认值16,建议值: [9, 11, 14, 16, 20, 27],实际默认值11,为0不设置
        /// </summary>
        public int Size = -1;
        /// <summary>
        /// 字体,建议值: 'mono', 'sans', 'serif'
        /// </summary>
        public string Font = "mono";
        /// <summary>
        /// 值的列表: 'true', 'false'
        /// </summary>
        public string Bold = "true";
        /// <summary>
        /// 值的列表: 'true', 'false'
        /// </summary>
        public string Slant = "true";
        public HMsgEntry(string _stringVal, int _row, int _column, string _color = "black", string _coordSystem = "image", HTuple _genParamName = null, HTuple _genParamValue = null, int _Size = 0, string _Font = "mono", string _Bold = "false", string _Slant = "false")
        {
            HMsg = _stringVal;
            coordSystem = _coordSystem;
            row = _row;
            column = _column;
            color = _color;
            if (_genParamName == null)
                genParamName = new HTuple();
            else
                genParamName = _genParamName;
            if (_genParamValue == null)
                genParamValue = new HTuple();
            else
                genParamValue = _genParamValue;

            Size = _Size;
            Font = _Font;
            Bold = _Bold;
            Slant = _Slant;
        }
    }
}
