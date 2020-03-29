using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using EPI.Common;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class BOLL : BaseIndicator
    {
        public enum EnumBollLine
        { 
            Mid=0,
            Up=1,
            Down=2
        }

        private int _length;
        private double _offset;
        List<double> valList;
        private int _theme;
        Color midColor;
        Color upColor;
        Color downColor;

        public BOLL(List<BarData> bars, int length = 26, double offset = 2, int theme = 1
            , bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(bars)
        {
            Tag = tag;
            _length = length;
            _offset = offset;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1})", _length, _offset);
            Name = string.Format("BOLL{0}", tag);
            Description = "布林通道";
            ChangeTheme(theme);
            valueDict.Add(EnumBollLine.Mid.ToString(), new List<double>());
            valueDict.Add(EnumBollLine.Up.ToString(), new List<double>());
            valueDict.Add(EnumBollLine.Down.ToString(), new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add(EnumBollLine.Mid.ToString(), new IndicatorGraph() { Name = EnumBollLine.Mid.ToString(), Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add(EnumBollLine.Up.ToString(), new IndicatorGraph() { Name = EnumBollLine.Up.ToString(), Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add(EnumBollLine.Down.ToString(), new IndicatorGraph() { Name = EnumBollLine.Down.ToString(), Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            valList = new List<double>();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        private void ChangeTheme(int theme)
        {
            _theme = theme;
            midColor = Color.Yellow;
            upColor = Color.Magenta;
            downColor = Color.SpringGreen;
            if (theme == 2)
            {
                midColor = Color.OrangeRed;
                upColor = Color.OrangeRed;
                downColor = Color.OrangeRed;
            }
            else if (theme == 3)
            {
                midColor = Color.Cyan;
                upColor = Color.Cyan;
                downColor = Color.Cyan;
            }
        }

        public void SetParameters(int length, double offset,int theme)
        {
            if (length != _length || offset != _offset || theme != _theme)
            {
                ChangeTheme(theme);
                string paramTag = string.Format("({0},{1})", length, offset);
                Name = string.Format("BOLL{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict[EnumBollLine.Mid.ToString()].Tag = paramTag;
                    graphDict[EnumBollLine.Up.ToString()].Tag = paramTag;
                    graphDict[EnumBollLine.Down.ToString()].Tag = paramTag;
                }
                _length = length;
                _offset = offset;
                Caculate();
            }
        }

        protected override void Caculate()
        { 
            valueDict[EnumBollLine.Mid.ToString()].Clear();
            valueDict[EnumBollLine.Up.ToString()].Clear();
            valueDict[EnumBollLine.Down.ToString()].Clear();
            if (!IsSimpleMode)
            {
                graphDict[EnumBollLine.Mid.ToString()].Clear();
                graphDict[EnumBollLine.Up.ToString()].Clear();
                graphDict[EnumBollLine.Down.ToString()].Clear();
            }
            valList.Clear();
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateBOLL(i);
                }
            }
            base.Caculate();
        }

        private void GenerateBOLL(int i)
        {
            BarData curData = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
            }
            else
            {
                curData = barDatas[i];
            }
            if (valList.Count == _length)
            { valList.RemoveAt(0); }
            else if(valList.Count>_length)
            { throw new Exception("计算数量超出范围"); }
            valList.Add(curData.Close);
            if (i < _length-1)
            {
                valueDict[EnumBollLine.Mid.ToString()].Add(JPR.NaN);
                valueDict[EnumBollLine.Up.ToString()].Add(JPR.NaN);
                valueDict[EnumBollLine.Down.ToString()].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict[EnumBollLine.Mid.ToString()].AddValue(JPR.NaN, midColor);
                    graphDict[EnumBollLine.Up.ToString()].AddValue(JPR.NaN, upColor);
                    graphDict[EnumBollLine.Down.ToString()].AddValue(JPR.NaN, downColor);
                }
            }
            else
            {
                double ma = valList.Average();
                double md = MathHelper.Stdev(valList);
                valueDict[EnumBollLine.Mid.ToString()].Add(ma);
                valueDict[EnumBollLine.Up.ToString()].Add(ma + Offset * md);
                valueDict[EnumBollLine.Down.ToString()].Add(ma - Offset * md);
                if (!IsSimpleMode)
                {
                    graphDict[EnumBollLine.Mid.ToString()].AddValue(ma, midColor);
                    graphDict[EnumBollLine.Up.ToString()].AddValue(ma + Offset * md, upColor);
                    graphDict[EnumBollLine.Down.ToString()].AddValue(ma - Offset * md, downColor);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict[EnumBollLine.Mid.ToString()].RemoveAt(Count - 1);
            valueDict[EnumBollLine.Up.ToString()].RemoveAt(Count - 1);
            valueDict[EnumBollLine.Down.ToString()].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict[EnumBollLine.Mid.ToString()].RemoveLast();
                graphDict[EnumBollLine.Up.ToString()].RemoveLast();
                graphDict[EnumBollLine.Down.ToString()].RemoveLast();
            }
            valList.RemoveAt(valList.Count - 1);
            GenerateBOLL(Count - 1);
            
            
        }


        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateBOLL(Count - 1);
        }
        /// <summary>
        /// 获取所有Boll值
        /// </summary>
        /// <param name="bl">Boll线类型</param>
        /// <returns></returns>
        public List<double> GetValues(EnumBollLine bl)
        {
            return valueDict[bl.ToString()];
        }
        ///// <summary>
        ///// 获取Boll值
        ///// </summary>
        ///// <param name="bl">Boll线类型</param>
        ///// <param name="dateTime">时间点</param>
        ///// <returns></returns>
        //public double GetValue(EnumBollLine bl, DateTime dateTime)
        //{
        //    var data = barDatas.Find(d => d.RealDateTime.Equals(dateTime));
        //    if (data != null)
        //    {
        //        int index = barDatas.IndexOf(data);
        //        if (valueDict[bl.ToString()].Count > index)
        //            return valueDict[bl.ToString()][index];
        //    }
        //    return JPR.NaN;
        //}
        /// <summary>
        /// 获取Boll值
        /// </summary>
        /// <param name="bl">Boll线类型</param>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public double GetValue(EnumBollLine bl, int index)
        {
            if (index >= 0 && index < valueDict[bl.ToString()].Count)
                return valueDict[bl.ToString()][index];
            else
                return JPR.NaN;
        }

        public double GetUpValue(int index)
        {
            if (index >= 0 && index < valueDict[EnumBollLine.Up.ToString()].Count)
                return valueDict[EnumBollLine.Up.ToString()][index];
            else
                return JPR.NaN;
        }

        public double GetDownValue(int index)
        {
            if (index >= 0 && index < valueDict[EnumBollLine.Down.ToString()].Count)
                return valueDict[EnumBollLine.Down.ToString()][index];
            else
                return JPR.NaN;
        }

        public double GetMidValue(int index)
        {
            if (index >= 0 && index < valueDict[EnumBollLine.Mid.ToString()].Count)
                return valueDict[EnumBollLine.Mid.ToString()][index];
            else
                return JPR.NaN;
        }

        /// <summary>
        /// 获取最后一个Boll值
        /// </summary>
        /// <param name="bl">Boll线类型</param>
        /// <returns></returns>
        public double GetLast(EnumBollLine bl)
        {
            if (Count != 0)
                return valueDict[bl.ToString()][Count - 1];
            else
                return JPR.NaN;
        }
        /// <summary>
        /// 周期长度
        /// </summary>
        public int Length 
        { 
            get { return _length; } 
        }
        /// <summary>
        /// 偏移量
        /// </summary>
        public double Offset 
        { 
            get { return _offset; } 
        }

        /// <summary>
        /// 主题颜色
        /// </summary>
        public int Theme { get { return _theme; } }
    }
}
