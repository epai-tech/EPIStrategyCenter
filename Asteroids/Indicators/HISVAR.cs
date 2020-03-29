using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class HISVAR : BaseIndicator
    {
        private double std;
        private double rValue;
        private double varValue;
        private double preSTD;

        public HISVAR(List<BarData> barDatas, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("(HIS)");
            Name = string.Format("HISVAR{0}", paramTag);
            Description = "历史潜在最大损失值";
            valueDict.Add("HISVAR", new List<double>());
            graphDict.Add("HISVAR", new IndicatorGraph() { Name = "HISVAR", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters()
        { }

        protected override void Caculate()
        {
            valueDict["HISVAR"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["HISVAR"].Clear();
            }
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateHISVAR(i);
                }
            }
            base.Caculate();
        }

        private void GenerateHISVAR(int i)
        {
            BarData curData = null;
            BarData preData = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
                preData = GetBarData(1);
            }
            else
            {
                curData = barDatas[i];
                if (i > 0 && i < barDatas.Count)
                {
                    preData = barDatas[i - 1];
                }
            }
            if (i == 0)
            {
                preSTD = 0;
                valueDict["HISVAR"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["HISVAR"].AddValue(JPR.NaN, Color.White);
                }
            }
            else if (i == 1)
            {
                rValue = Math.Log(curData.Close) - Math.Log(preData.Close);
                std = Math.Abs(rValue);
                varValue = -1.65 * std * 100;
                valueDict["HISVAR"].Add(varValue);
                if (!IsSimpleMode)
                {
                    graphDict["HISVAR"].AddValue(varValue, Color.White);
                }
            }
            else
            {
                preSTD = std;
                rValue = Math.Log(curData.Close) - Math.Log(preData.Close);
                std = Math.Pow(0.94 * Math.Pow(std, 2) + 0.06 * Math.Pow(rValue, 2), 0.5);
                varValue = -1.65 * std * 100;
                valueDict["HISVAR"].Add(varValue);
                if (!IsSimpleMode)
                {
                    graphDict["HISVAR"].AddValue(varValue, Color.White);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["HISVAR"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["HISVAR"].RemoveLast();
            }
            std = preSTD;
            GenerateHISVAR(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateHISVAR(Count - 1);
            
            
        }

        public List<double> GetHISVARValues()
        {
            return valueDict["HISVAR"];
        }

        public double GetHISVARValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["HISVAR"][index];
            else
                return JPR.NaN;
        }

        public double GetLastHISVAR()
        {
            if (Count != 0)
                return valueDict["HISVAR"][Count - 1];
            else
                return JPR.NaN;
        }
    }
}
