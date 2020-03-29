using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Asteroids.Functions;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class JSJ : BaseIndicator
    {
        private bool _isCFFEX;
        private double _volumeMultiple;
        private BarData preBarData;
        private double SumAmount;
        private double SumVolum;

        public JSJ(List<BarData> barDatas, bool isCFFEX,double volumeMultiple, bool isSimpleMode = true, bool isShowInMain = true)
            : base(barDatas)
        {
            string tag = string.Format("({0},{1})", isCFFEX?"CFFEX":"NotCFFEX",volumeMultiple);
            Name = string.Format("JSJ{0}", tag);
            Description = "结算价";
            IsSimpleMode = isSimpleMode;
            valueDict.Add("JSJ", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("JSJ", new IndicatorGraph() { Name = "JSJ", Tag = tag, LineStyle = EnumLineStyle.SolidLine });
            }
            _volumeMultiple = volumeMultiple;
            _isCFFEX = isCFFEX;
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(bool isCFFEX,double volumeMultiple)
        {
            if (isCFFEX != _isCFFEX || volumeMultiple!=_volumeMultiple)
            {
                string tag = string.Format("({0},{1})", isCFFEX ? "CFFEX" : "NotCFFEX", volumeMultiple);
                Name = string.Format("JSJ{0}", tag);
                if (!IsSimpleMode)
                {
                    graphDict["JSJ"].Tag = tag;
                }
                _volumeMultiple = volumeMultiple;
                _isCFFEX = isCFFEX;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["JSJ"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["JSJ"].Clear();
            }
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateJSJ(i);
                }
            }
            base.Caculate();
        }

        private void GenerateJSJ(int i)
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
            if (_isCFFEX)
            {
                if (preBarData != null && curData.UpdateTime.CompareTo("14:15:00") >= 0)
                {
                    if (preBarData.UpdateTime.CompareTo("14:15:00") >= 0)
                    {
                        SumAmount += curData.Amount;
                        SumVolum += curData.Volume;
                    }
                    else
                    {
                        SumAmount = curData.Amount;
                        SumVolum = curData.Volume;
                    }
                }
            }
            else
            {
                if (preBarData == null)
                {
                    SumAmount = curData.Amount;
                    SumVolum = curData.Volume;
                }
                else
                {
                    if (curData.TradingDay == preBarData.TradingDay)
                    {
                        SumAmount += curData.Amount;
                        SumVolum += curData.Volume;
                    }
                    else
                    {
                        SumAmount = curData.Amount;
                        SumVolum = curData.Volume;
                    }
                }
            }
            preBarData = curData;
            if (SumVolum != 0)
            {
                double result = SumAmount / SumVolum / _volumeMultiple;
                valueDict["JSJ"].Add(result);
                if (!IsSimpleMode)
                {
                    graphDict["JSJ"].AddValue(result, Color.LightGoldenrodYellow);
                }
            }
            else
            {
                valueDict["JSJ"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["JSJ"].AddValue(JPR.NaN, Color.LightGoldenrodYellow);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["JSJ"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["JSJ"].RemoveLast();
            }
            if (preBarData != null)
            {
                if (_isCFFEX)
                {
                    if (preBarData.UpdateTime.CompareTo("14:15:00") >= 0)
                    {
                        SumAmount -= preBarData.Amount;
                        SumVolum -= preBarData.Volume;
                    }
                }
                else
                {
                    if (bar.TradingDay == preBarData.TradingDay)
                    {
                        SumAmount -= preBarData.Amount;
                        SumVolum -= preBarData.Volume;
                    }
                }
            }
            GenerateJSJ(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateJSJ(Count - 1);
           
            
        }

        public List<double> GetValues()
        {
            return valueDict["JSJ"];
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["JSJ"][index];
            else
                return JPR.NaN;
        }

        public double GetLastValue()
        {
            if (Count != 0)
                return valueDict["JSJ"][Count - 1];
            else
                return JPR.NaN;
        }

        public bool IsCFFEX
        {
            get { return _isCFFEX; }
        }
        public double VolumeMultiple
        {
            get { return _volumeMultiple; }
        }
    }
}
