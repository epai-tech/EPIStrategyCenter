using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class VOL : BaseIndicator
    {
        private int _length;
        private AverageS averageS;
        double minOpenInst, maxOpenInst, minVolRate, maxVol;

        public VOL(List<BarData> barDatas, int length = 20, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", _length);
            Name = string.Format("VOL{0}", paramTag);
            Description = "成交量";
            valueDict.Add("VOL", new List<double>());
            valueDict.Add("MAVOL", new List<double>());
            valueDict.Add("OPID", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("VOL", new IndicatorGraph() { Name = "VOL", Tag = paramTag, LineStyle = EnumLineStyle.Steam });
                graphDict.Add("MAVOL", new IndicatorGraph() { Name = "MAVOL", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("OPID", new IndicatorGraph() { Name = "OPID", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            averageS = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length)
        {
            if (length != _length)
            {
                string paramTag = string.Format("({1})", length);
                Name = string.Format("VOL{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["VOL"].Tag = paramTag;
                    graphDict["MAVOL"].Tag = paramTag;
                    graphDict["OPID"].Tag = paramTag;
                }
                _length = length;
                Caculate();
            }
        }

        int changeDayCount = 0;
        protected override void Caculate()
        {
            valueDict["VOL"].Clear();
            valueDict["MAVOL"].Clear();
            valueDict["OPID"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["VOL"].Clear();
                graphDict["MAVOL"].Clear();
                graphDict["OPID"].Clear();
            }
            averageS.SetParameters(_length);
            ResetRate(0);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (i > 0 && barDatas[i].Cycle[barDatas[i].Cycle.Length - 1] == 'M')
                    {
                        if (barDatas[i].TradingDay != barDatas[i - 1].TradingDay)
                        {
                            if (changeDayCount == 2)
                            {
                                ResetRate(i);
                                changeDayCount = 1;
                            }
                            else
                                changeDayCount ++;
                        }
                    }
                    GenerateVOL(i);
                }
            }
            base.Caculate();
        }

        private void ResetRate(int index)
        {
            if (barDatas.Count > 0)
            {
                maxVol = barDatas.Skip(index).Max(d => d.Volume);
                maxOpenInst = barDatas.Skip(index).Max(d => d.OpenInterest);
                var vdatas = barDatas.Skip(index).Where(d => d.OpenInterest > 0).ToList();
                if (vdatas.Count > 0)
                    minOpenInst = vdatas.Min(d => d.OpenInterest);
                var minRate = (minOpenInst - maxOpenInst) / maxOpenInst;
                minVolRate = (float)(20 - maxVol) / (float)maxVol / (float)minRate;
            }
        }
        

        private void GenerateVOL(int i)
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
            if (!JPR.IsNaN(curData.Close))
            {
                double thisVol = curData.Volume;
                valueDict["VOL"].Add(thisVol);
                if (curData.Close > curData.Open)
                {
                    if (!IsSimpleMode)
                    {
                        graphDict["VOL"].AddValue(thisVol, Color.Red);
                    }
                }
                else if (curData.Close < curData.Open)
                {
                    if (!IsSimpleMode)
                    {
                        graphDict["VOL"].AddValue(thisVol, Color.Green);
                    }
                }
                else
                {
                    if (!IsSimpleMode)
                    {
                        graphDict["VOL"].AddValue(thisVol, Color.White);
                    }
                }

                double maValue = averageS.AddValue(thisVol);
                valueDict["MAVOL"].Add(maValue);
                if (!IsSimpleMode)
                {
                    graphDict["MAVOL"].AddValue(maValue, Color.White);
                }

                var rate = (curData.OpenInterest - maxOpenInst) / maxOpenInst;
                var opidValue = rate * minVolRate * maxVol + maxVol;

                valueDict["OPID"].Add(opidValue);
                if (!IsSimpleMode)
                {
                    graphDict["OPID"].AddValue(opidValue, Color.Yellow);
                }
            }
            else
            {
                valueDict["VOL"].Add(JPR.NaN);
                valueDict["MAVOL"].Add(JPR.NaN);
                valueDict["OPID"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["VOL"].AddValue(JPR.NaN, Color.White);
                    graphDict["MAVOL"].AddValue(JPR.NaN, Color.White);
                    graphDict["OPID"].AddValue(JPR.NaN, Color.Yellow);
                }

            }

        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateVOL(Count - 1);
            
            
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["VOL"].RemoveAt(Count - 1);
            valueDict["MAVOL"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["VOL"].RemoveLast();
                graphDict["MAVOL"].RemoveLast();

                graphDict["OPID"].RemoveLast();
            }
            valueDict["OPID"].RemoveAt(Count - 1);
            averageS.RemoveLast();
            GenerateVOL(Count - 1);
            
            
        }

        public List<double> GetVOLValues()
        {
            return valueDict["VOL"];
        }
        public List<double> GetMAVOLValues()
        {
            return valueDict["MAVOL"];
        }
        public List<double> GetOPIDValues()
        {
            return valueDict["OPID"];
        }

        public double GetVOLValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["VOL"][index];
            else
                return JPR.NaN;
        }

        public double GetMAVOLValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["MAVOL"][index];
            else
                return JPR.NaN;
        }

        public double GetOPIDValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["OPID"][index];
            else
                return JPR.NaN;
        }

        public double GetLastVOL()
        {
            if (Count != 0)
                return valueDict["VOL"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastMAVOL()
        {
            if (Count != 0)
                return valueDict["MAVOL"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastOPID()
        {
            if (Count != 0)
                return valueDict["OPID"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }
    }
}
