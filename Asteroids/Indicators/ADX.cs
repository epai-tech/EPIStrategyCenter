using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class ADX : BaseIndicator
    {
        private int _length;
        List<double> trList, hdList, ldList;
        private int _maRef;
        private double preValue;
        private double MaADXValue;
        private AverageS MaADX;

        public ADX(List<BarData> bars, int length = 14, int maRef = 14, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(bars)
        {
            _length = length;
            _maRef = maRef;
            Tag = tag;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1})", _length, _maRef);
            Name = string.Format("ADX{0}", paramTag);
            Description = "平均趋向指数";
            valueDict.Add("ADX", new List<double>());
            valueDict.Add("MaADX", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("ADX", new IndicatorGraph() { Name = "ADX", Tag = paramTag, LineStyle = EnumLineStyle.Steam });
                graphDict.Add("MaADX", new IndicatorGraph { Name = "MaADX", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            trList = new List<double>();
            hdList = new List<double>();
            ldList = new List<double>();
            MaADX = new AverageS();
            preValue = 0.0;
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length, int maRef)
        {
            if (length != _length || maRef!=_maRef)
            {
                string paramTag = string.Format("({0},{1})", length, maRef);
                Name = string.Format("ADX{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["ADX"].Tag = paramTag;
                    graphDict["MaADX"].Tag = paramTag;
                }
                _length = length;
                _maRef = maRef;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["ADX"].Clear();
            valueDict["MaADX"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["ADX"].Clear();
                graphDict["MaADX"].Clear();
            }
            trList.Clear();
            hdList.Clear();
            ldList.Clear();
            MaADX.SetParameters(_maRef);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateADX(i);
                }
            }
            base.Caculate();
        }

        private void GenerateADX(int i)
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
            if (trList.Count == _length)
            {
                trList.RemoveAt(0);
                hdList.RemoveAt(0);
                ldList.RemoveAt(0);
            }
            else if (trList.Count > _length)
            { throw new Exception("计算数量超出范围"); }
            //BarData bar = barDatas[i];
            if (trList.Count == 0)
            {
                trList.Add((curData.High - curData.Low));
                hdList.Add(0);
                ldList.Add(0);
            }
            else
            {
                //var preBar = barDatas[i - 1];
                var tr = Math.Max(Math.Max(curData.High - curData.Low, Math.Abs(curData.High - preData.Close)), Math.Abs(preData.Close - curData.Low));
                trList.Add(tr);
                var hd = curData.High - preData.High;
                var ld = preData.Low - curData.Low;
                hdList.Add(hd);
                ldList.Add(ld);
            }
            if (i < _length - 1)
            {
                valueDict["ADX"].Add(JPR.NaN);
                valueDict["MaADX"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["ADX"].AddValue(JPR.NaN, Color.Yellow);
                    graphDict["MaADX"].AddValue(JPR.NaN, Color.Cyan);
                }
            }
            else
            {
                double sumTr = trList.Sum();
                double dmp = 0, dmm = 0;
                for (int j = 0; j < _length; j++)
                {
                    var ohd = hdList[j];
                    var old = ldList[j];
                    dmp += ohd > 0 && ohd > old ? ohd : 0;
                    dmm += old > 0 && old > ohd ? old : 0;
                }
                double value = 0.0;
                if (sumTr == 0)
                {
                    value = preValue;
                }
                else
                {
                    double pdi = dmp * 100 / sumTr;
                    double mdi = dmm * 100 / sumTr;
                    if (mdi + pdi == 0)
                    {
                        value = preValue;
                    }
                    else
                    {
                        value = Math.Abs(mdi - pdi) / (mdi + pdi) * 100;
                    }
                }
                preValue = value;
                valueDict["ADX"].Add(value);
                MaADXValue = MaADX.AddValue(value);
                valueDict["MaADX"].Add(MaADXValue);
                if (!IsSimpleMode)
                {
                    graphDict["ADX"].AddValue(value, Color.Yellow);
                    graphDict["MaADX"].AddValue(MaADXValue, Color.Cyan);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["ADX"].RemoveAt(Count - 1);
            valueDict["MaADX"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["ADX"].RemoveLast();
                graphDict["MaADX"].RemoveLast();
            }
            trList.RemoveAt(trList.Count - 1);
            hdList.RemoveAt(hdList.Count - 1);
            ldList.RemoveAt(hdList.Count - 1);
            MaADX.RemoveLast();
            GenerateADX(Count - 1);
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateADX(Count - 1); 
        }

        public List<double> GetADXValues()
        {
            return valueDict["ADX"];
        }

        public List<double> GetMaADXValues()
        {
            return valueDict["MaADX"];
        }

        public double GetADXValue(int index)
        {
            if (index >= 0 && index < Count)
                 return valueDict["ADX"][index];
            else
                return JPR.NaN;
        }
        public double GetMaADXValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["MaADX"][index];
            else
                return JPR.NaN;
        }

        public double GetLastADX()
        {
            if (Count != 0)
                return valueDict["ADX"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastMaADX()
        {
            if (Count != 0)
                return valueDict["MaADX"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }

        public int MaRef
        {
            get { return _maRef; }
        }
    }
}
