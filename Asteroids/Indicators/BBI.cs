using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Asteroids.Functions;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class BBI:BaseIndicator
    {
        private int _num1;
        private int _num2;
        private int _num3;
        private int _num4;
        private AverageS MA1;
        private AverageS MA2;
        private AverageS MA3;
        private AverageS MA4;
        private double Ma1Value;
        private double Ma2Value;
        private double Ma3Value;
        private double Ma4Value;
        private double BBIValue;
        private Color _lineColor;

        public BBI(List<BarData> barDatas, Color lineColor, int num1 = 3, int num2 = 6, int num3 = 12, int num4 = 24, bool isSimpleMode = true,
            bool isShowInMain = true, string tag = "1")
            : base(barDatas) 
        {
            Tag = tag;
            _lineColor = lineColor;
            _num1 = num1;
            _num2 = num2;
            _num3 = num3;
            _num4 = num4;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1},{2},{3})", _num1, _num2, _num3, _num4);
            Name = string.Format("BBI{0}", paramTag);
            Description = "牛熊指数";
            valueDict.Add("BBI", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("BBI", new IndicatorGraph() { Name = "BBI", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            MA1 = new AverageS();
            MA2 = new AverageS();
            MA3 = new AverageS();
            MA4 = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(Color lineColor, int num1, int num2, int num3, int num4)
        {
            if (num1 != _num1 || num2 != _num2 || num3 != _num3 || num4 != _num4||_lineColor!=lineColor)
            {
                string paramTag = string.Format("({0},{1},{2},{3})", num1, num2, num3, num4);
                Name = string.Format("BBI{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["BBI"].Tag = paramTag;
                }
                _lineColor = lineColor;
                _num1 = num1;
                _num2 = num2;
                _num3 = num3;
                _num4 = num4;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["BBI"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["BBI"].Clear();
            }
            MA1.SetParameters(_num1);
            MA2.SetParameters(_num2);
            MA3.SetParameters(_num3);
            MA4.SetParameters(_num4);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateBBI(i);
                }
            }
            base.Caculate();
        }

        private void GenerateBBI(int i)
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
            Ma1Value = MA1.AddValue(curData.Close);
            Ma2Value = MA2.AddValue(curData.Close);
            Ma3Value = MA3.AddValue(curData.Close);
            Ma4Value = MA4.AddValue(curData.Close);
            if (Ma1Value != JPR.NaN && Ma2Value != JPR.NaN && Ma3Value != JPR.NaN && Ma4Value != JPR.NaN)
            {
                BBIValue = (Ma1Value + Ma2Value + Ma3Value + Ma4Value) / 4;
            }
            else
            {
                BBIValue = JPR.NaN;
            }
            valueDict["BBI"].Add(BBIValue);
            if (!IsSimpleMode)
            {
                graphDict["BBI"].AddValue(BBIValue, _lineColor);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["BBI"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["BBI"].RemoveLast();
            }
            MA1.RemoveLast();
            MA2.RemoveLast();
            MA3.RemoveLast();
            MA4.RemoveLast();
            GenerateBBI(Count - 1);
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateBBI(Count - 1);
            
        }

        public List<double> GetValues()
        {
            return valueDict["BBI"];
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["BBI"][index];
            else
                return JPR.NaN;
        }

        public double GetLast()
        {
            if (Count != 0)
                return valueDict["BBI"][Count - 1];
            else
                return JPR.NaN;
        }

        public Color LineColor { get { return _lineColor; } }

        public int Num1
        { 
            get { return _num1; } 
        }
        public int Num2
        {
            get { return _num2; }
        }
        public int Num3
        {
            get { return _num3; }
        }
        public int Num4
        {
            get { return _num4; }
        }
    }
}
