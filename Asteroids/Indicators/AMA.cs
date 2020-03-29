using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class AMA:BaseIndicator
    {
        private int _length;
        private double _smoothlength;
        List<double> eMaList,noiseList;
        List<double> deMaList1, deMaList2;
        double diff, noise,signal,efratio,smooth;
        List<double> amaefratio;

        public AMA(List<BarData> bars, int length = 5, double smoothlength=3.1, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(bars)
        {
            _length = length;
            _smoothlength = smoothlength;
            IsSimpleMode = isSimpleMode;
            Tag = tag;
            string paramTag = string.Format("({0},{1})", _length,_smoothlength);
            Name = string.Format("AMA{0}", paramTag);
            Description = "佩里·考夫曼自适应移动平均线";
            valueDict.Add("AMA", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("AMA", new IndicatorGraph() { Name = "AMA", Tag = paramTag, LineStyle = EnumLineStyle.Circle });
            }
            eMaList = new List<double>();
            noiseList = new List<double>();
            deMaList1 = new List<double>();
            deMaList2 = new List<double>();
            amaefratio = new List<double>();
            IsShowInMain = isShowInMain;
            MaxCacheCount = length * 2 < 10 ? 10 : length * 2;
            Caculate();
        }

        public void SetParameters(int length, double smoothlength)
        {
            if (length != _length || smoothlength != _smoothlength)
            {
                string paramTag = string.Format("({0},{1})", length, smoothlength);
                Name = string.Format("AMA{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["AMA"].Tag = paramTag;
                }
                _length = length;
                _smoothlength = smoothlength;
                MaxCacheCount = length * 2 < 10 ? 10 : length * 2;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["AMA"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["AMA"].Clear();
            }
            eMaList.Clear();
            noiseList.Clear();
            deMaList1.Clear();
            deMaList2.Clear();
            amaefratio.Clear();
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    Generate(i);
                }
            }
            base.Caculate();
        }

        private void Generate(int i)
        {
            BarData curData = null;
            BarData preData = null;
            double offsetClose = 0;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
                preData = GetBarData(1);
                offsetClose = GetBarData(_length).Close;
            }
            else
            {
                curData = barDatas[i];
                if (i > 0 && i < barDatas.Count)
                {
                    preData = barDatas[i - 1];
                }
                if (i - _length >=0)
                {
                    offsetClose = barDatas[i - _length].Close;
                }
            }
            double thisClose = curData.Close;
            double lasteMa1;
            double lasteMa2;
            double thiseMa1;
            double thiseMa2;
            double thisAMA;
            if (i == 0)
            {
                noiseList.Add(0);
                eMaList.Add(thisClose);
                deMaList1.Add(thisClose);
                deMaList2.Add(thisClose);
                valueDict["AMA"].Add(thisClose);
                if (!IsSimpleMode)
                {
                    graphDict["AMA"].AddValue(thisClose, Color.White);
                }
                amaefratio.Add(JPR.NaN);
            }
            else if (i > 0 && i < _length)
            {
                diff = Math.Abs(curData.Close - preData.Close);
                if (noiseList.Count == _length)
                {
                    noiseList.RemoveAt(0);
                }
                noiseList.Add(diff);
                eMaList.Add(thisClose);
                lasteMa1 = deMaList1[deMaList1.Count - 1];
                lasteMa2 = deMaList2[deMaList2.Count - 1];
                thiseMa1 = lasteMa1 + 2 / 9.0 * (thisClose - lasteMa1);
                thiseMa2 = lasteMa2 + 2 / 9.0 * (thiseMa1 - lasteMa2);
                deMaList1.Add(thiseMa1);
                deMaList2.Add(thiseMa2);
                thisAMA = 2 * thiseMa1 - thiseMa2;
                valueDict["AMA"].Add(thisAMA);
                if (!IsSimpleMode)
                {
                    graphDict["AMA"].AddValue(thisAMA, thisClose > thisAMA ? Color.White : Color.Yellow);
                }
                amaefratio.Add(JPR.NaN);
            }
            else
            {
                diff = Math.Abs(thisClose - preData.Close);
                if (noiseList.Count == _length)
                {
                    noiseList.RemoveAt(0);
                }
                noiseList.Add(diff);
                double thisNoise = noiseList.Sum();
                if (thisNoise != 0)
                {
                    noise = thisNoise;
                }
                signal = Math.Abs(thisClose - offsetClose);
                efratio = noise == 0 ? 0 : signal / noise;
                smooth = Math.Pow(efratio * (0.666 - 0.0645) + 0.0645, _smoothlength);
                double lasteMa=eMaList[eMaList.Count-1];
                double neweMa = lasteMa + smooth * (thisClose - lasteMa);
                eMaList.Add(neweMa);
                lasteMa1 = deMaList1[deMaList1.Count-1];
                lasteMa2 = deMaList2[deMaList2.Count-1];
                thiseMa1 = lasteMa1 + 2 / 9.0 * (neweMa - lasteMa1);
                thiseMa2 = lasteMa2 + 2 / 9.0 * (thiseMa1 - lasteMa2);
                deMaList1.Add(thiseMa1);
                deMaList2.Add(thiseMa2);
                thisAMA = 2 * thiseMa1 - thiseMa2;
                valueDict["AMA"].Add(thisAMA);
                if (!IsSimpleMode)
                {
                    graphDict["AMA"].AddValue(thisAMA, thisClose > thisAMA ? Color.White : Color.Yellow);
                }
                amaefratio.Add(efratio);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["AMA"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["AMA"].RemoveLast();
            }
            eMaList.RemoveAt(eMaList.Count - 1);
            noiseList.RemoveAt(noiseList.Count - 1);
            deMaList1.RemoveAt(deMaList1.Count - 1);
            deMaList2.RemoveAt(deMaList2.Count - 1);
            amaefratio.RemoveAt(amaefratio.Count - 1);
            Generate(Count - 1);
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            Generate(Count - 1);
            
        }

        public List<double> GetAMAValues()
        {
            return valueDict["AMA"];
        }
        public List<double> GetAMAEfratio()
        {
            return amaefratio;
        }

        public double GetAMAValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["AMA"][index];
            else
                return JPR.NaN;
        }
        public double GetEfratioValue(int index)
        {
            if (index >= 0 && index < Count)
                return amaefratio[index];
            else
                return JPR.NaN;
        }

        public double GetLastAMA()
        {
            if (Count != 0)
                return valueDict["AMA"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastEfratio()
        {
            if (Count != 0)
                return amaefratio[Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }

        public double Smoothlength
        {
            get { return _smoothlength; }
        }
    }
}
