using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class UpDowns:BaseIndicator
    {
        double Ups=0;
        double Downs=0;
        double thisUpDown = 0;
        List<double> closeList;

        public UpDowns(List<BarData> barDatas, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Name = "UpDowns";
            Description = "连续盈亏";
            IsSimpleMode = isSimpleMode;
            valueDict.Add("UpDowns", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("UpDowns", new IndicatorGraph() { Name = "UpDowns", Tag = "UpDowns", LineStyle = EnumLineStyle.Steam });
            }
            closeList = new List<double>();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters()
        {
            Caculate();
        }

        protected override void Caculate()
        {
            valueDict["UpDowns"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["UpDowns"].Clear();
            }
            closeList.Clear();
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateUpDowns(i);
                }
            }
            base.Caculate();
        }

        private void GenerateUpDowns(int i)
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
                valueDict["UpDowns"].Add(0);
                if (!IsSimpleMode)
                {
                    graphDict["UpDowns"].AddValue(0, Color.Red);
                }
                closeList.Add(curData.Close);
            }
            else
            {
                thisUpDown = (curData.Close - preData.Close);
                if (thisUpDown>0)
                {
                    Ups += thisUpDown;
                    Downs = 0;
                    valueDict["UpDowns"].Add(Ups);
                    if (!IsSimpleMode)
                    {
                        graphDict["UpDowns"].AddValue(Ups, Color.Red);
                    }
                }
                else if (thisUpDown < 0)
                {
                    Ups = 0;
                    Downs += thisUpDown;
                    valueDict["UpDowns"].Add(Downs);
                    if (!IsSimpleMode)
                    {
                        graphDict["UpDowns"].AddValue(Downs, Color.Green);
                    }
                }
                else
                {
                    if (Ups != 0)
                    {
                        valueDict["UpDowns"].Add(Ups);
                        if (!IsSimpleMode)
                        {
                            graphDict["UpDowns"].AddValue(Ups, Color.Red);
                        }
                    }
                    else if (Downs != 0)
                    {
                        valueDict["UpDowns"].Add(Downs);
                        if (!IsSimpleMode)
                        {
                            graphDict["UpDowns"].AddValue(Downs, Color.Green);
                        }
                    }
                    else
                    {
                        valueDict["UpDowns"].Add(0);
                        if (!IsSimpleMode)
                        {
                            graphDict["UpDowns"].AddValue(0, Color.Red);
                        }
                    }
                }
                closeList.Add(curData.Close);
            }
        }

        //public override void UpdateBarData(BarData bar)
        //{
        //    base.UpdateBarData(bar);
        //    valueDict["UpDowns"].RemoveAt(Count - 1);
        //    graphDict["UpDowns"].RemoveLast();
        //    closeList.RemoveAt(closeList.Count - 1);
        //    GenerateUpDowns(Count - 1);
        //    Dictionary<string, double> resultDict = new Dictionary<string, double>();
        //    resultDict.Add("UpDowns", GetLast());
        //    
        //}

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateUpDowns(Count - 1);
            
            
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["UpDowns"][index];
            else
                return JPR.NaN;
        }

        public List<double> GetValues()
        {
            return valueDict["UpDowns"];
        }

        public double GetLast()
        {
                if (Count != 0)
                    return valueDict["UpDowns"][Count - 1];
                else
                    return JPR.NaN;
        }

        public double LastClose
        {
            get
            {
                if (closeList.Count != 0)
                    return closeList[closeList.Count - 1];
                else
                    return JPR.NaN;
            }
        }
    }
}
