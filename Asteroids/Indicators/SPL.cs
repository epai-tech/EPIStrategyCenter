using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EPI.Asteroids.Indicators
{
    public class SPL:BaseIndicator
    {
        int _length;
        BarData preUpdateBarData;
        private Color _lineColor;

        public SPL(List<BarData> barDatas, Color lineColor, int length = 1, bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            _lineColor = lineColor;
            
            string paramTag = string.Format("({0})", _length);
            Name = string.Format("SPL{0}", paramTag);
            Description = "分笔分段";
            valueDict.Add("SPLIT", new List<double>());
            valueDict.Add("BLOCK", new List<double>());
            //graphDict.Add("TRA", new IndicatorGraph() { Name = "TRA", Tag = tag, LineStyle = EnumLineStyle.SolidLine });
            IsShowInMain = true;
            Caculate();
        }

        public void SetParameters(Color lineColor, int length = 1)
        {
            if (_length != length || _lineColor != lineColor)
            {
                _lineColor = lineColor;
                string paramTag = string.Format("({0})", length);
                Name = string.Format("SPL{0}", paramTag);
                _length = length;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            UserLineList.Clear();
            ValueDict["SPLIT"].Clear();
            ValueDict["BLOCK"].Clear();
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateSPL(i);
                }
            }
            base.Caculate();
        }

        int trendCount;
        double preHigh, preLow;
        int preHighIndex, preLowIndex;
        double maxLow, maxHigh;
        double minLow, minHigh;
        int preStyleType = 0;//1=顶分型 -1=底分型
        PointF preMarkPoint = new PointF();

        void GenerateSPL(int i)
        {
            BarData curData = null;
            BarData preData = null;
            BarData preData2 = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
                preData = GetBarData(1);
                preData2 = GetBarData(2);
            }
            else
            {
                curData = barDatas[i];
                if (i > 0 && i < barDatas.Count)
                {
                    preData = barDatas[i - 1];
                }
                if (i > 1 && i < barDatas.Count)
                {
                    preData2 = barDatas[i - 2];
                }
            }
            if (i > 0)
            {
                if (trendCount == 2)
                {
                    if (curData.High < maxHigh && curData.Low < maxLow)//顶分型
                    {
                        if (preStyleType == 0)
                        {
                            preMarkPoint.X = i - 1;
                            preMarkPoint.Y = (float)preData.High;
                            preStyleType = 1;
                        }
                        else if (preStyleType == -1)//前一次为底分型
                        {
                            //顶底之间距离大于等于5
                            if (i - preMarkPoint.X >= 5)
                            {
                                //当前分型最低点大于前一次最低点
                                if (preMarkPoint.Y < Math.Min(curData.Low, preData2.Low))
                                {
                                    //满足上升一笔条件
                                    AddUserLine(new PointF(preMarkPoint.X, preMarkPoint.Y),
                                        new PointF(preHighIndex, (float)preHigh), _lineColor, DashStyle.Custom, 1, "SPL");
                                    preMarkPoint.X = preHighIndex;
                                    preMarkPoint.Y = (float)preHigh;
                                    preStyleType = 1;
                                    preHigh = 0;
                                    preLow = 0;
                                }
                            }
                        }
                        //已经被实时的探顶过滤了，故无需考虑
                        //else if (preStyleType == 1)//前一次为顶分型
                        //{
                        //    if (barDatas[i - 1].High > preMarkPoint.Y)//当前顶分型大于前一个，则更新
                        //    {
                        //        preMarkPoint.X = i - 1;
                        //        preMarkPoint.Y = (float)barDatas[i - 1].High;
                        //        UserLine userLine = GetUserLine();
                        //        if (userLine != null)
                        //            userLine.PointB = new PointF(i - 1, preMarkPoint.Y);
                        //        preHigh = 0;
                        //        preLow = 0;
                        //    }
                        //}
                        trendCount = -1;
                    }
                }
                else if (trendCount == -2)
                {
                    if (curData.High > minHigh && curData.Low > minLow)//底分型
                    {
                        if (preStyleType == 0)
                        {
                            preMarkPoint.X = i - 1;
                            preMarkPoint.Y = (float)preData.Low;
                            preStyleType = -1;
                        }
                        else if (preStyleType == 1)//前一次为顶分型
                        {
                            //顶底之间距离大于等于5
                            if (i - preMarkPoint.X >= 5)
                            {
                                //当前分型最高点小于前一次最高点
                                if (preMarkPoint.Y > Math.Max(curData.High, preData2.High))
                                {
                                    //满足下升一笔条件
                                    AddUserLine(new PointF(preMarkPoint.X, preMarkPoint.Y),
                                        new PointF(preLowIndex, (float)preLow), _lineColor, DashStyle.Custom, 1, "SPL");
                                    preMarkPoint.X = preLowIndex;
                                    preMarkPoint.Y = (float)preLow;
                                    preStyleType = -1;
                                    preHigh = 0;
                                    preLow = 0;
                                }
                            }
                        }
                        //已经被实时的探底过滤了，故无需考虑
                        //else if (preStyleType == -1)//前一次为底分型
                        //{
                        //    if (barDatas[i - 1].Low < preMarkPoint.Y)//当前底分型小于前一个，则更新
                        //    {
                        //        preMarkPoint.X = i - 1;
                        //        preMarkPoint.Y = (float)barDatas[i - 1].Low;
                        //        UserLine userLine = GetUserLine();
                        //        if (userLine != null)
                        //            userLine.PointB = new PointF(i - 1, preMarkPoint.Y);
                        //        preHigh = 0;
                        //        preLow = 0;
                        //    }
                        //}
                        trendCount = 1;
                    }
                }
                else
                    CheckTrend(curData, preData);

                CheckContain(i, curData);
            }
        }

        //处理包含关系
        void CheckContain(int i, BarData curData)
        {
            if (trendCount == 0 || trendCount == 1 || trendCount == -1)
            {
                maxHigh = curData.High;
                maxLow = curData.Low;
                minHigh = curData.High;
                minLow = curData.Low;
            }
            else if (trendCount > 0)
            {
                maxHigh = Math.Max(maxHigh, curData.High);
                maxLow = Math.Max(maxLow, curData.Low);
            }
            else if (trendCount < 0)
            {
                minHigh = minHigh > 0 ? Math.Min(minHigh, curData.High) : curData.High;
                minLow = minLow > 0 ? Math.Min(minLow, curData.Low) : curData.Low;

            }

            if (preStyleType == -1 && preMarkPoint.Y > curData.Low)
            {
                preMarkPoint.X = i;
                preMarkPoint.Y = (float)curData.Low;
                UserLine userLine = GetUserLine();
                if (userLine != null)
                    userLine.PointB = new PointF(i, preMarkPoint.Y);
                //两个顶分型中间出现比前一次底分型低的需要更新
                UserLine preUserLine = GetUserLine(1);
                if (preUserLine != null && preUserLine.PointB.Y < preHigh)
                {
                    preUserLine.PointB = new PointF(preHighIndex, (float)preHigh);
                    userLine.PointA = new PointF(preHighIndex, (float)preHigh);
                }

                preHigh = preLow = 0;
            }
            else if (preStyleType == 1 && preMarkPoint.Y < curData.High)
            {
                preMarkPoint.X = i;
                preMarkPoint.Y = (float)curData.High;
                UserLine userLine = GetUserLine();
                if (userLine != null)
                    userLine.PointB = new PointF(i, preMarkPoint.Y);
                //两个顶分型中间出现比前一次底分型低的需要更新
                UserLine preUserLine = GetUserLine(1);
                if (preUserLine != null && preUserLine.PointB.Y > preLow)
                {
                    preUserLine.PointB = new PointF(preLowIndex, (float)preLow);
                    userLine.PointA = new PointF(preLowIndex, (float)preLow);
                }
                
                preHigh = preLow = 0;
            }

            if (preHigh < curData.High)
            {
                preHigh = curData.High;
                preHighIndex = i;
            }
            //preHigh = Math.Max(preHigh, curData.High);
            //preHighIndex = i;
            if (preLow==0||preLow > curData.Low)
            {
                preLow = curData.Low;
                preLowIndex = i;
            }
            //preLow = preLow > 0 ? Math.Min(preLow, curData.Low) : curData.Low;
            //preLowIndex = i;

        }

        /// <summary>
        /// 检查趋势方向
        /// </summary>
        /// <param name="i">2=向上，-2=向下</param>
        /// <returns></returns>
        void CheckTrend(BarData curData, BarData preData)
        {
            if (curData.High > preData.High && curData.Low > preData.Low)
            {
                if (trendCount >= 0)
                    trendCount++;
                else
                    trendCount = 0;
            }
            else if (curData.High < preData.High && curData.Low < preData.Low)
            {
                if (trendCount <= 0)
                    trendCount--;
                else
                    trendCount = 0;
            }
        }


        public override void UpdateBarData(BarData bar)
        {
            //base.UpdateBarData(bar);
            //valueDict["TRA_B"].RemoveAt(Count - 1);
            //valueDict["TRA_S"].RemoveAt(Count - 1);
            //buyLines.RemoveAt(Count - 1);
            //sellLines.RemoveAt(Count - 1);
            if (preUpdateBarData != null && preUpdateBarData.RealDateTime == bar.RealDateTime)
                preUpdateBarData = bar;

            //GenerateTRA(Count - 1);
            Dictionary<string, double> resultDict = new Dictionary<string, double>();
            //resultDict.Add("SPLIT", GetLastSplit());
            //resultDict.Add("BLOCK", GetLastBlock());
            
        }

        public override void AddBarData(BarData bar)
        {
            //base.AddBarData(bar);
            if (preUpdateBarData == null)
                preUpdateBarData = bar;
            if (preUpdateBarData.RealDateTime != bar.RealDateTime)
            {
                base.AddBarData(preUpdateBarData);
                GenerateSPL(Count - 1);
                preUpdateBarData = bar;
            }
            //GenerateTRA(Count - 1);
            Dictionary<string, double> resultDict = new Dictionary<string, double>();
            //resultDict.Add("SPLIT", GetLastSplit());
            //resultDict.Add("BLOCK", GetLastBlock());
            
        }

        public double GetSplitValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["SPLIT"][index];
            else
                return JPR.NaN;
        }

        public double GetBlockValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["BLOCK"][index];
            else
                return JPR.NaN;
        }

        public double GetLastSplit()
        {
            if (Count != 0)
                return valueDict["SPLIT"].Last();
            else
                return JPR.NaN;
        }

        public double GetLastBlock()
        {
            if (Count != 0)
                return valueDict["BLOCK"].Last();
            else
                return JPR.NaN;
        }
        public Color LineColor { get { return _lineColor; } }
        /// <summary>
        /// 长度
        /// </summary>
        public int Length
        {
            get { return _length; }
        }
    }
}
