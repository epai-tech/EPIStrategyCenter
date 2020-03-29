using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Common;

namespace EPI.Asteroids.Indicators
{
    public class Volatility : BaseIndicator
    {
        private int _length1;
        private int _length2;
        private int _length3;
        private int _showLength;
        private List<BarData> barList1;
        private List<double> list1;
        private List<BarData> barList2;
        private List<double> list2;
        private List<BarData> barList3;
        private List<double> list3;

        public Volatility(List<BarData> barDatas, int length1 = 5, int length2 = 10, int length3 = 20, int showLength = 1, bool isSimpleMode = true,
            bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length1 = length1;
            _length2 = length2;
            _length3 = length3;
            IsSimpleMode = isSimpleMode;
            _showLength = showLength;
            string paramTag = string.Format("({0},{1},{2},{3})", _length1, _length2, _length3, _showLength);
            Name = string.Format("Volatility{0}", paramTag);
            Description = "波动率";
            valueDict.Add("Volatility1", new List<double>());
            valueDict.Add("Volatility2", new List<double>());
            valueDict.Add("Volatility3", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("Volatility1", new IndicatorGraph() { Name = "Volatility1", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("Volatility2", new IndicatorGraph() { Name = "Volatility2", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("Volatility3", new IndicatorGraph() { Name = "Volatility3", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            barList1 = new List<BarData>();
            list1 = new List<double>();
            barList2 = new List<BarData>();
            list2 = new List<double>();
            barList3 = new List<BarData>();
            list3 = new List<double>();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length1 , int length2 , int length3 , int showLength)
        {
            if (length1 != _length1 || length2 != _length2 ||length3 != _length3  || showLength != _showLength)
            {
                string paramTag = string.Format("({0},{1},{2},{3})", length1, length2, length3, showLength);
                Name = string.Format("Volatility{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["Volatility1"].Tag = paramTag;
                    graphDict["Volatility2"].Tag = paramTag;
                    graphDict["Volatility3"].Tag = paramTag;
                }
                _length1 = length1;
                _length2 = length2;
                _length3 = length3;
                _showLength = showLength;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["Volatility1"].Clear();
            valueDict["Volatility2"].Clear();
            valueDict["Volatility3"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["Volatility1"].Clear();
                graphDict["Volatility2"].Clear();
                graphDict["Volatility3"].Clear();
            }
            barList1.Clear();
            list1.Clear();
            barList2.Clear();
            list2.Clear();
            barList3.Clear();
            list3.Clear();
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateVolatility(i);
                }
            }
            base.Caculate();
        }

        private void GenerateVolatility(int i)
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
            if (barList1.Count == 2)
            { barList1.RemoveAt(0); }
            barList1.Add(curData);
            if (barList2.Count == 2)
            { barList2.RemoveAt(0); }
            barList2.Add(curData);
            if (barList3.Count == 2)
            { barList3.RemoveAt(0); }
            barList3.Add(curData);
            if (list1.Count == _length1)
            { list1.RemoveAt(0); }
            if (list2.Count == _length2)
            { list2.RemoveAt(0); }
            if (list3.Count == _length3)
            { list3.RemoveAt(0); }
            //第一条
            if (barList1.Count == 1)
            {
                if (barList1[0].Open != 0)
                    list1.Add(Math.Log(barList1[0].Close / barList1[0].Open));
                else
                    list1.Add(0);
            }
            else
            {
                if (barList1[0].Close != 0)
                    list1.Add(Math.Log(barList1[1].Close / barList1[0].Close));
                else
                    list1.Add(0);
            }
            if (i < _length1 - 1)
            {
                valueDict["Volatility1"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["Volatility1"].AddValue(JPR.NaN, Color.White);
                }
            }
            else
            {
                double stv = MathHelper.Stdev(list1) * Math.Sqrt(_showLength)*10000;
                valueDict["Volatility1"].Add(stv);
                if (!IsSimpleMode)
                {
                    graphDict["Volatility1"].AddValue(stv, Color.White);
                }
            }
            //第二条
            if (barList2.Count == 1)
            {
                if (barList2[0].Open != 0)
                    list2.Add(Math.Log(barList2[0].Close / barList2[0].Open));
                else
                    list2.Add(0);
            }
            else
            {
                if (barList2[0].Close != 0)
                    list2.Add(Math.Log(barList2[1].Close / barList2[0].Close));
                else
                    list2.Add(0);
            }
            if (i < _length2 - 1)
            {
                valueDict["Volatility2"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["Volatility2"].AddValue(JPR.NaN, Color.Yellow);
                }
            }
            else
            {
                double stv = MathHelper.Stdev(list2) * Math.Sqrt(_showLength) * 10000;
                valueDict["Volatility2"].Add(stv);
                if (!IsSimpleMode)
                {
                    graphDict["Volatility2"].AddValue(stv, Color.White);
                }
            }
            //第三条
            if (barList3.Count == 1)
            {
                if (barList3[0].Open != 0)
                    list3.Add(Math.Log(barList3[0].Close / barList3[0].Open));
                else
                    list3.Add(0);
            }
            else
            {
                if (barList3[0].Close != 0)
                    list3.Add(Math.Log(barList3[1].Close / barList3[0].Close));
                else
                    list3.Add(0);
            }
            if (i < _length3 - 1)
            {
                valueDict["Volatility3"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["Volatility3"].AddValue(JPR.NaN, Color.Purple);
                }
            }
            else
            {
                double stv = MathHelper.Stdev(list3) * Math.Sqrt(_showLength) * 10000;
                valueDict["Volatility3"].Add(stv);
                if (!IsSimpleMode)
                {
                    graphDict["Volatility3"].AddValue(stv, Color.Purple);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["Volatility1"].RemoveAt(Count - 1);
            valueDict["Volatility2"].RemoveAt(Count - 1);
            valueDict["Volatility3"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["Volatility1"].RemoveLast();
                graphDict["Volatility2"].RemoveLast();
                graphDict["Volatility3"].RemoveLast();
            }
            list1.RemoveAt(list1.Count - 1);
            barList1.RemoveAt(1);
            list2.RemoveAt(list2.Count - 1);
            barList2.RemoveAt(1);
            list3.RemoveAt(list3.Count - 1);
            barList3.RemoveAt(1);
            GenerateVolatility(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateVolatility(Count - 1);
           
            
        }

        public List<double> GetValues1()
        {
            return valueDict["Volatility1"];
        }
        public List<double> GetValues2()
        {
            return valueDict["Volatility2"];
        }
        public List<double> GetValues3()
        {
            return valueDict["Volatility3"];
        }

        public double GetValue1(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Volatility1"][index];
            else
                return JPR.NaN;
        }
        public double GetValue2(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Volatility2"][index];
            else
                return JPR.NaN;
        }
        public double GetValue3(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Volatility3"][index];
            else
                return JPR.NaN;
        }

        public double GetLast1()
        {
            if (Count != 0)
                return valueDict["Volatility1"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLast2()
        {
            if (Count != 0)
                return valueDict["Volatility2"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLast3()
        {
            if (Count != 0)
                return valueDict["Volatility3"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length1
        {
            get { return _length1; }
        }
        public int Length2
        {
            get { return _length2; }
        }
        public int Length3
        {
            get { return _length3; }
        }
        public int ShowLength
        {
            get { return _showLength; }
        }
    }
}
