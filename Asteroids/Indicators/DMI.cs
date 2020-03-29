using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Asteroids.Functions;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class DMI:BaseIndicator
    {
        private int _length;
        private TrueRange tr;
        private AverageS atr;
        private AverageS ADX;
        private AverageS PDM;
        private AverageS MDM;
        private double prePDI;
        private double preMDI;
        private double preDX;
        private double ADXValue;
        private double perADXValue;

        public DMI(List<BarData> barDatas, int length = 12, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            :base(barDatas)
        {
            Tag = tag;
            _length = length;
            string paramTag = string.Format("({0})", _length);
            Name = string.Format("DMI{0}", paramTag);
            IsSimpleMode = isSimpleMode;
            Description = "趋向指标";
            valueDict.Add("+DI", new List<double>());
            valueDict.Add("-DI", new List<double>());
            valueDict.Add("ADX", new List<double>());
            valueDict.Add("ADXR", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("+DI", new IndicatorGraph() { Name = "+DI", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("-DI", new IndicatorGraph() { Name = "-DI", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("ADX", new IndicatorGraph() { Name = "ADX", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("ADXR", new IndicatorGraph() { Name = "ADXR", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            tr = new TrueRange();
            atr = new AverageS();
            ADX = new AverageS();
            PDM = new AverageS();
            MDM = new AverageS();
            prePDI = 0.0;
            preMDI = 0.0;
            preDX = 0.0;
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length)
        {
            if (length != _length)
            {
                string paramTag = string.Format("({0})", length);
                Name = string.Format("DMI{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["+DI"].Tag = paramTag;
                    graphDict["-DI"].Tag = paramTag;
                    graphDict["ADX"].Tag = paramTag;
                    graphDict["ADXR"].Tag = paramTag;
                }
                _length = length;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["+DI"].Clear();
            valueDict["-DI"].Clear();
            valueDict["ADX"].Clear();
            valueDict["ADXR"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["+DI"].Clear();
                graphDict["-DI"].Clear();
                graphDict["ADX"].Clear();
                graphDict["ADXR"].Clear();
            }
            atr.SetParameters(_length);
            ADX.SetParameters(_length);
            PDM.SetParameters(_length);
            MDM.SetParameters(_length);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateDMI(i);
                }
            }
            base.Caculate();
        }

        private void GenerateDMI(int i)
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
            double trValue;
            double atrValue;
            double PDMValue;
            double MDMValue;
            double avePDM;
            double aveMDM;
            double PDI;
            double MDI;
            double DX;
            double ADXRValue;
            if (i == 0)
            {
                trValue = (curData.High - curData.Low);
                PDMValue = 0;
                MDMValue = 0;
            }
            else
            {
                trValue = tr.Caculate(preData.Close, curData.High, curData.Low);
                PDMValue = curData.High - preData.High > 0 ? curData.High - preData.High : 0;
                MDMValue = preData.Low - curData.Low > 0 ? preData.Low - curData.Low : 0;
                if (PDMValue >= MDMValue)
                { MDMValue = 0; }
                else
                { PDMValue = 0; }
            }
            atrValue = atr.AddValue(trValue);
            avePDM = PDM.AddValue(PDMValue);
            aveMDM = MDM.AddValue(MDMValue);
            if (avePDM != JPR.NaN)
            {
                if (atrValue == 0)
                {
                    PDI = prePDI;
                    MDI = preMDI;
                    DX = preDX;
                }
                else
                {
                    PDI = avePDM * 100 / atrValue;
                    MDI = aveMDM * 100 / atrValue;
                    if (PDI + MDI == 0)
                    {
                        PDI = prePDI;
                        MDI = preMDI;
                        DX = preDX;
                    }
                    else
                    {
                        DX = Math.Abs(PDI - MDI) * 100 / (PDI + MDI);
                    }
                }
                prePDI = PDI;
                preMDI = MDI;
                preDX = DX;
                ADXValue = ADX.AddValue(DX);
                if (perADXValue != JPR.NaN)
                {
                    ADXRValue = perADXValue / 2 + ADXValue / 2;
                    perADXValue = ADXValue;
                }
                else
                {
                    ADXRValue = JPR.NaN;
                    perADXValue = ADXValue;
                }
            }
            else
            {
                PDI = JPR.NaN;
                MDI = JPR.NaN;
                ADXValue = JPR.NaN;
                ADXRValue = JPR.NaN;
            }
            valueDict["+DI"].Add(PDI);
            valueDict["-DI"].Add(MDI);
            valueDict["ADX"].Add(ADXValue);
            valueDict["ADXR"].Add(ADXRValue);
            if (!IsSimpleMode)
            {
                graphDict["+DI"].AddValue(PDI, Color.Red);
                graphDict["-DI"].AddValue(MDI, Color.Green);
                graphDict["ADX"].AddValue(ADXValue, Color.White);
                graphDict["ADXR"].AddValue(ADXRValue, Color.Yellow);
            }
        }

        //public override void UpdateBarData(BarData bar)
        //{
        //    base.UpdateBarData(bar);
        //    valueDict["+DI"].RemoveAt(Count-1);
        //    valueDict["-DI"].RemoveAt(Count-1);
        //    valueDict["ADX"].RemoveAt(Count-1);
        //    valueDict["ADXR"].RemoveAt(Count-1);
        //    graphDict["+DI"].RemoveLast();
        //    graphDict["-DI"].RemoveLast();
        //    graphDict["ADX"].RemoveLast();
        //    graphDict["ADXR"].RemoveLast();
        //    atr.RemoveLast();
        //    ADX.RemoveLast();
        //    PDM.RemoveLast();
        //    MDM.RemoveLast();
        //    GenerateDMI(Count-1);
        //    Dictionary<string,double>resultDict=new Dictionary<string,double>();
        //    resultDict.Add("+DI", GetLastPDI());
        //    resultDict.Add("-DI", GetLastMDI());
        //    resultDict.Add("ADX", GetLastADX());
        //    resultDict.Add("ADXR", GetLastADXR());
        //    
        //}

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateDMI(Count - 1);
            
            
        }

        public List<double> GetPDIValues()
        { 
            return valueDict["+DI"]; 
        }

        public List<double> GetMDIValues()
        {
            return valueDict["-DI"];
        }

        public List<double> GetADXValues()
        {
            return valueDict["ADX"];
        }

        public List<double> GetADXRValues()
        {
            return valueDict["ADXR"];
        }

        public double GetPDIValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["+DI"][index];
            else
                return JPR.NaN;
        }

        public double GetMDIValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["-DI"][index];
            else
                return JPR.NaN;
        }

        public double GetADXValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["ADX"][index];
            else
                return JPR.NaN;
        }

        public double GetADXRValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["ADXR"][index];
            else
                return JPR.NaN;
        }

        public double GetLastPDI()
        {
            if (Count != 0)
                return valueDict["+DI"][Count - 1];
            else
                return JPR.NaN;
        }

        public double GetLastMDI()
        {
            if (Count != 0)
                return valueDict["-DI"][Count - 1];
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

        public double GetLastADXR()
        {
            if (Count != 0)
                return valueDict["ADXR"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }
    }
}
