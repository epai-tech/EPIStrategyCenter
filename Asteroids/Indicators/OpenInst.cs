using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class OpenInst : BaseIndicator
    {

        public OpenInst(List<BarData> barDatas, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            Name = "OpenInst";
            Description = "持仓量";
            IsSimpleMode = isSimpleMode;
            valueDict.Add("OI", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("OI", new IndicatorGraph() { Name = "OI", Tag = "", LineStyle = EnumLineStyle.SolidLine });
            }
            IsShowInMain = false;
            Caculate();
        }

        public void SetParameters()
        {
            
        }

        protected override void Caculate()
        {
            valueDict["OI"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["OI"].Clear();
            }
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateVOL(i);
                }
            }
            base.Caculate();
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
            double thisOpenInst = curData.OpenInterest;
            valueDict["OI"].Add(thisOpenInst);
            if (!IsSimpleMode)
            {
                graphDict["OI"].AddValue(thisOpenInst, Color.Yellow);
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
            valueDict["OI"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["OI"].RemoveLast();
            }
            GenerateVOL(Count - 1);
            
            
        }

        public List<double> GetValues()
        {
            return valueDict["OI"];
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["OI"][index];
            else
                return JPR.NaN;
        }

        public double GetLast()
        {
            if (Count != 0)
                return valueDict["OI"][Count - 1];
            else
                return JPR.NaN;
        }
    }
}
