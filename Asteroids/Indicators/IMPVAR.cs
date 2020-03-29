using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class IMPVAR : BaseIndicator
    {
        private double std;
        private double rValue;
        private double varValue;
        private int _length;

        public IMPVAR(List<BarData> barDatas, int length = 20, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", _length);
            Name = string.Format("IMPVAR{0}", paramTag);
            Description = "近期潜在最大损失值";
            valueDict.Add("IMPVAR", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("IMPVAR", new IndicatorGraph() { Name = "IMPVAR", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            IsShowInMain = isShowInMain;
            MaxCacheCount = length;
            Caculate();
        }

        public void SetParameters(int length)
        {
            if (length != _length)
            {
                string paramTag = string.Format("({0})", length);
                Name = string.Format("IMPVAR{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["IMPVAR"].Tag = paramTag;
                }
                _length = length;
                MaxCacheCount = length;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["IMPVAR"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["IMPVAR"].Clear();
            }
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateIMPVAR(i);
                }
            }
            base.Caculate();
        }

        private void GenerateIMPVAR(int i)
        {
            BarData curData = null;
            BarData preData = null;
            if (i <= _length)
            {
                valueDict["IMPVAR"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["IMPVAR"].AddValue(JPR.NaN, Color.White);
                }
            }
            else
            {
                for (int j = i - _length; j < i; j++)
                {
                    if (i >= barDatas.Count)
                    {
                        curData = GetBarData(i - j);
                        preData = GetBarData(i - j - 1);
                    }
                    else {
                        curData = barDatas[j];
                        preData = barDatas[j - 1];
                    }

                    rValue = Math.Log(curData.Close) - Math.Log(preData.Close);
                    if (j == i - _length)
                    {
                        std = Math.Pow(rValue, 2);
                    }
                    else
                    {
                        std = 0.94 * std + 0.06 * Math.Pow(rValue, 2);
                    }
                }
                std = Math.Pow(std, 0.5);
                varValue = -1.65 * std * 100;
                valueDict["IMPVAR"].Add(varValue);
                if (!IsSimpleMode)
                {
                    graphDict["IMPVAR"].AddValue(varValue, Color.White);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            //valueDict["IMPVAR"].RemoveAt(Count - 1);
            //graphDict["IMPVAR"].RemoveLast();
            //GenerateIMPVAR(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateIMPVAR(Count - 1);
           
            
        }

        public List<double> GetIMPVARValues()
        {
            return valueDict["IMPVAR"];
        }

        public double GetIMPVARValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["IMPVAR"][index];
            else
                return JPR.NaN;
        }

        public double GetLastIMPVAR()
        {
            if (Count != 0)
                return valueDict["IMPVAR"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }
    }
}
