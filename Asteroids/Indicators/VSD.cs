using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Common;

namespace EPI.Asteroids.Indicators
{
    public class VSD:BaseIndicator
    {
        private int _length;
        private int _showPec;
        List<double> list;

        public VSD(List<BarData> barDatas, int length = 20,int showPec=0, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            _showPec = showPec;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1})", _length, _showPec);
            Name = string.Format("VSD{0}", paramTag);
            Description = "波动标准差";
            if (_showPec > 0) valueDict.Add("Pec", new List<double>());
            valueDict.Add("Avg", new List<double>());
            valueDict.Add("Std", new List<double>());
            if (_showPec > 0)
                if (!IsSimpleMode)
                {
                    graphDict.Add("Pec", new IndicatorGraph() { Name = "Pec", Tag = paramTag, LineStyle = EnumLineStyle.Steam });
                }
            if (!IsSimpleMode)
            {
                graphDict.Add("Avg", new IndicatorGraph() { Name = "Avg", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("Std", new IndicatorGraph() { Name = "Std", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            list = new List<double>();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length, int showPec = 0)
        {
            if (length != _length)
            {
                string paramTag = string.Format("({0},{1})", length,showPec);
                Name = string.Format("VSD{0}", paramTag);
                if (_showPec > 0)
                    if (!IsSimpleMode)
                    {
                        graphDict["Pec"].Tag = paramTag;
                    }
                if (!IsSimpleMode)
                {
                    graphDict["Avg"].Tag = paramTag;
                    graphDict["Std"].Tag = paramTag;
                }
                _length = length;
                _showPec = showPec;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            if (_showPec > 0) valueDict["Pec"].Clear();
            valueDict["Avg"].Clear();
            valueDict["Std"].Clear();
            if (_showPec > 0) graphDict["Pec"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["Avg"].Clear();
                graphDict["Std"].Clear();
            }
            list.Clear();
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                { 
                    GenerateVSD(i);
                }
            }
            base.Caculate();
        }

        private void GenerateVSD(int i)
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
            double value=0;
            if (list.Count == _length)
            { list.RemoveAt(0); }
            else if (list.Count > _length)
            { throw new Exception("计算数量超出范围"); }
            if (curData.Open != 0)
            {
                value = (curData.Close / curData.Open - 1) * 10000;
                list.Add(value);
            }
            else
            {
                list.Add(0);
            }
            if (i < _length - 1)
            {
                if (_showPec > 0) valueDict["Pec"].Add(JPR.NaN); ;
                valueDict["Avg"].Add(JPR.NaN); ;
                valueDict["Std"].Add(JPR.NaN); ;
                if (_showPec > 0) graphDict["Pec"].AddValue(JPR.NaN, Color.Red);
                if (!IsSimpleMode)
                {
                    graphDict["Avg"].AddValue(JPR.NaN, Color.White);
                    graphDict["Std"].AddValue(JPR.NaN, Color.Yellow);
                }
            }
            else
            {
                double avg = list.Average();
                double std = MathHelper.Stdev(list);
                if (_showPec > 0) valueDict["Pec"].Add(value);
                valueDict["Avg"].Add(avg);
                valueDict["Std"].Add(std);
                if (_showPec > 0)
                    if (!IsSimpleMode)
                    {
                        graphDict["Pec"].AddValue(value, value >= 0 ? Color.Red : Color.Cyan);
                    }
                if (!IsSimpleMode)
                {
                    graphDict["Avg"].AddValue(avg, Color.White);
                    graphDict["Std"].AddValue(std, Color.Yellow);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            if (_showPec > 0) valueDict["Pec"].RemoveAt(Count - 1);
            valueDict["Avg"].RemoveAt(Count - 1);
            valueDict["Std"].RemoveAt(Count - 1);
            if (_showPec > 0)
                if (!IsSimpleMode)
                {
                    graphDict["Pec"].RemoveLast();
                }
            if (!IsSimpleMode)
            {
                graphDict["Avg"].RemoveLast();
                graphDict["Std"].RemoveLast();
            }
            list.RemoveAt(list.Count - 1);
            GenerateVSD(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateVSD(Count - 1);
            
            
        }

        public List<double> GetPecValues()
        {
            if (_showPec > 0) 
                return valueDict["Pec"];
            else
                return null;
        }
        public List<double> GetAvgValues()
        {
            return valueDict["Avg"];
        }
        public List<double> GetStdValues()
        {
            return valueDict["Std"];
        }

        public double GetPecValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Pec"][index];
            else
                return JPR.NaN;
        }

        public double GetAvgValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Avg"][index];
            else
                return JPR.NaN;
        }

        public double GetStdValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Std"][index];
            else
                return JPR.NaN;
        }

        public double GetLastPec()
        {
            if (Count != 0 && _showPec > 0)
                return valueDict["Pec"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastAvg()
        {
            if (Count != 0)
                return valueDict["Avg"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastStd()
        {
            if (Count != 0)
                return valueDict["Std"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }
        public int ShowPec
        {
            get { return _showPec; }
        }
    }
}
