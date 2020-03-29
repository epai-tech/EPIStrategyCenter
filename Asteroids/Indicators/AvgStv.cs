using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Common;

namespace EPI.Asteroids.Indicators
{
    public class AvgStv:BaseIndicator
    {
        private int _length;
        List<double> list;

        public AvgStv(List<BarData> bars, int length = 20, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(bars)
        {
            Tag = tag;
            _length = length;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", _length);
            Name = string.Format("AvgStv{0}", paramTag);
            Description="均值与标准差";
            valueDict.Add("AVG", new List<double>());
            valueDict.Add("STV", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("AVG", new IndicatorGraph() { Name = "AVG", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("STV", new IndicatorGraph() { Name = "STV", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            list=new List<double>();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length)
        {
            if (length != _length)
            {
                string paramTag = string.Format("({0})", length);
                Name = string.Format("AvgStv{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["AVG"].Tag = paramTag;
                    graphDict["STV"].Tag = paramTag;
                }
                _length = length;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["AVG"].Clear();
            valueDict["STV"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["AVG"].Clear();
                graphDict["STV"].Clear();
            }
            list.Clear();
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateAvgStv(i);
                }
            }
            base.Caculate();
        }

        private void GenerateAvgStv(int i)
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
            if (list.Count == _length)
            { list.RemoveAt(0); }
            else if (list.Count > _length)
            { throw new Exception("计算数量超出范围"); }
            list.Add(curData.Close);
            if (i < _length - 1)
            {
                valueDict["AVG"].Add(JPR.NaN);
                valueDict["STV"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["AVG"].AddValue(JPR.NaN, Color.Yellow);
                    graphDict["STV"].AddValue(JPR.NaN, Color.White);
                }
            }
            else
            {
                double avg=list.Average();
                double stv=MathHelper.Stdev(list);
                valueDict["AVG"].Add(avg);
                valueDict["STV"].Add(stv);
                if (!IsSimpleMode)
                {
                    graphDict["AVG"].AddValue(avg, Color.Yellow);
                    graphDict["STV"].AddValue(stv, Color.White);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["AVG"].RemoveAt(Count - 1);
            valueDict["STV"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["AVG"].RemoveLast();
                graphDict["STV"].RemoveLast();
            }
            list.RemoveAt(list.Count - 1);
            GenerateAvgStv(Count - 1);
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateAvgStv(Count - 1);
        }

        public List<double> GetAvgValues()
        {
            return valueDict["AVG"];
        }
        public List<double> GetStvValues()
        {
            return valueDict["STV"];
        }

        public double GetLastAvg()
        {
            if (Count != 0)
                return valueDict["AVG"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastStv()
        {
            if (Count != 0)
                return valueDict["STV"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }
    }
}
