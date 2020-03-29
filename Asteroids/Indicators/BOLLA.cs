using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using EPI.Common;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class BOLLA : BaseIndicator
    {
        public enum EnumBollLine
        { 
            Mid=0,
            Up=1,
            Down=2,
            M1=3
        }

        private int _length;
        private double _offset;
        List<double> valList;
        private int _theme;
        private int _rate;
        Color midColor;
        Color upColor;
        Color downColor;
        Color mColor;
        bool isPreTop;
        bool isPreBottom;
        double m1Value;
        double m2Value;

        public BOLLA(List<BarData> bars, int length = 26, double offset = 2, int rate = 20, int theme = 1
            , bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(bars)
        {
            m1Value = m2Value = JPR.NaN;
            IsSimpleMode = isSimpleMode;
            isPreTop = false;
            if (rate > 0)
                _rate = rate;
            else
                _rate = 20;
            Tag = tag;
            _length = length;
            _offset = offset;
            string paramTag = string.Format("({0},{1},{2})", _length, _offset,_rate);
            Name = string.Format("BOLLA{0}", tag);
            Description = "布林通道A";
            ChangeTheme(theme);
            valueDict.Add(EnumBollLine.Mid.ToString(), new List<double>());
            valueDict.Add(EnumBollLine.Up.ToString(), new List<double>());
            valueDict.Add(EnumBollLine.Down.ToString(), new List<double>());
            valueDict.Add(EnumBollLine.M1.ToString(), new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add(EnumBollLine.Mid.ToString(), new IndicatorGraph() { Name = EnumBollLine.Mid.ToString(), Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add(EnumBollLine.Up.ToString(), new IndicatorGraph() { Name = EnumBollLine.Up.ToString(), Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add(EnumBollLine.Down.ToString(), new IndicatorGraph() { Name = EnumBollLine.Down.ToString(), Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add(EnumBollLine.M1.ToString(), new IndicatorGraph() { Name = EnumBollLine.M1.ToString(), Tag = paramTag, LineStyle = EnumLineStyle.DotLine });
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
            mColor = Color.Gold;
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

        public void SetParameters(int length, double offset,int rate,int theme)
        {
            if (length != _length || offset != _offset||_rate!=rate || theme != _theme)
            {
                ChangeTheme(theme);
                string paramTag = string.Format("({0},{1})", length, offset);
                Name = string.Format("BOLLA{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict[EnumBollLine.Mid.ToString()].Tag = paramTag;
                    graphDict[EnumBollLine.Up.ToString()].Tag = paramTag;
                    graphDict[EnumBollLine.Down.ToString()].Tag = paramTag;
                    graphDict[EnumBollLine.M1.ToString()].Tag = paramTag;
                }
                _length = length;
                _offset = offset;
                if (rate > 0)
                    _rate = rate;
                else
                    _rate = 20;
                Caculate();
            }
        }

        protected override void Caculate()
        { 
            valueDict[EnumBollLine.Mid.ToString()].Clear();
            valueDict[EnumBollLine.Up.ToString()].Clear();
            valueDict[EnumBollLine.Down.ToString()].Clear();
            valueDict[EnumBollLine.M1.ToString()].Clear();
            if (!IsSimpleMode)
            {
                graphDict[EnumBollLine.Mid.ToString()].Clear();
                graphDict[EnumBollLine.Up.ToString()].Clear();
                graphDict[EnumBollLine.Down.ToString()].Clear();
                graphDict[EnumBollLine.M1.ToString()].Clear();
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
                valueDict[EnumBollLine.M1.ToString()].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict[EnumBollLine.Mid.ToString()].AddValue(JPR.NaN, midColor);
                    graphDict[EnumBollLine.Up.ToString()].AddValue(JPR.NaN, upColor);
                    graphDict[EnumBollLine.Down.ToString()].AddValue(JPR.NaN, downColor);
                    graphDict[EnumBollLine.M1.ToString()].AddValue(JPR.NaN, mColor);
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
                int midCount = valueDict[EnumBollLine.Mid.ToString()].Count;
                //mValue = ma + Offset * md / 5;
                if (valueDict[EnumBollLine.Up.ToString()][midCount - 2] > valueDict[EnumBollLine.Up.ToString()][midCount - 3] &&
                    valueDict[EnumBollLine.Up.ToString()][midCount - 2] > valueDict[EnumBollLine.Up.ToString()][midCount - 1])
                {
                    m1Value = Offset * md / (100f / (float)_rate);
                    isPreTop = true;
                }
                else if (valueDict[EnumBollLine.Up.ToString()][midCount - 2] < valueDict[EnumBollLine.Up.ToString()][midCount - 3] &&
                    valueDict[EnumBollLine.Up.ToString()][midCount - 2] < valueDict[EnumBollLine.Up.ToString()][midCount - 1])
                {
                    m1Value = Offset * md / (100f / (float)_rate);
                    isPreTop = false;
                }

                if (valueDict[EnumBollLine.Down.ToString()][midCount - 2] > valueDict[EnumBollLine.Down.ToString()][midCount - 3] &&
                    valueDict[EnumBollLine.Down.ToString()][midCount - 2] > valueDict[EnumBollLine.Down.ToString()][midCount - 1])
                {
                    m2Value = Offset * md / (100f / (float)_rate);
                    isPreBottom = false;
                }
                else if (valueDict[EnumBollLine.Down.ToString()][midCount - 2] < valueDict[EnumBollLine.Down.ToString()][midCount - 3] &&
                    valueDict[EnumBollLine.Down.ToString()][midCount - 2] < valueDict[EnumBollLine.Down.ToString()][midCount - 1])
                {
                    m2Value = Offset * md / (100f / (float)_rate);
                    isPreBottom = true;
                }
                if (!isPreTop)
                {
                    m1Value = Offset * md / (100f / (float)_rate);
                }
                if (!isPreBottom)
                {
                    m2Value = Offset * md / (100f / (float)_rate);
                }
                
                if (!JPR.IsNaN(m1Value))
                {
                    if (curData.Close < ma)
                    {
                        //graphDict[EnumBollLine.M1.ToString()].AddValue(ma + mValue, mColor);
                        valueDict[EnumBollLine.M1.ToString()].Add(ma - m2Value);
                        if (!IsSimpleMode)
                        {
                            graphDict[EnumBollLine.M1.ToString()].AddValue(ma - m2Value, mColor);
                        }
                    }
                    else
                    {
                        valueDict[EnumBollLine.M1.ToString()].Add(ma + m1Value);
                        if (!IsSimpleMode)
                        {
                            graphDict[EnumBollLine.M1.ToString()].AddValue(ma + m1Value, mColor);
                        }
                        //graphDict[EnumBollLine.M2.ToString()].AddValue(ma - Offset * md/2, mColor);
                    }
                }
                else
                {
                    valueDict[EnumBollLine.M1.ToString()].Add(ma - Offset * md / 2);
                    if (!IsSimpleMode)
                    {
                        graphDict[EnumBollLine.M1.ToString()].AddValue(ma - Offset * md / 2, mColor);
                    }
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict[EnumBollLine.Mid.ToString()].RemoveAt(Count - 1);
            valueDict[EnumBollLine.Up.ToString()].RemoveAt(Count - 1);
            valueDict[EnumBollLine.Down.ToString()].RemoveAt(Count - 1);
            valueDict[EnumBollLine.M1.ToString()].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict[EnumBollLine.Mid.ToString()].RemoveLast();
                graphDict[EnumBollLine.Up.ToString()].RemoveLast();
                graphDict[EnumBollLine.Down.ToString()].RemoveLast();
                graphDict[EnumBollLine.M1.ToString()].RemoveLast();
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
        /// 偏离率
        /// </summary>
        public int Rate
        {
            get { return _rate; }
        }

        /// <summary>
        /// 主题颜色
        /// </summary>
        public int Theme { get { return _theme; } }
    }
}
