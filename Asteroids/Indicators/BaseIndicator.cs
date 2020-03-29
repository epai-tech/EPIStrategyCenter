using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace EPI.Asteroids.Indicators
{
    public class BaseIndicator
    {
        private int _count;
        private int _maxCacheCount = 10;
        protected List<BarData> barDatas;
        protected Dictionary<string,List<double>> valueDict;
        protected Dictionary<string, IndicatorGraph> graphDict;
        protected List<UserLine> userLineList;
        public BaseIndicator(List<BarData> bars)
        {
            barDatas = new List<BarData>();
            if (bars != null)
                barDatas.AddRange(bars);
            _count = barDatas.Count;
            valueDict = new Dictionary<string, List<double>>();
            graphDict = new Dictionary<string, IndicatorGraph>();
            userLineList = new List<UserLine>();
        }

        protected virtual void Caculate()
        {
            if (IsSimpleMode)
            {
                if (barDatas.Count > MaxCacheCount)
                {
                    barDatas.RemoveRange(0, barDatas.Count - MaxCacheCount);
                }
                graphDict.Clear();
                GC.Collect();
            }
        }

        #region 公共属性
        /// <summary>
        /// 指标名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 指标描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 是否在主图上显示
        /// </summary>
        public bool IsShowInMain { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// 精简模式
        /// </summary>
        public bool IsSimpleMode { get; set; }
        /// <summary>
        /// 数据字典
        /// </summary>
        public Dictionary<string, List<double>> ValueDict { get { return valueDict; } }
        /// <summary>
        /// 图形字典
        /// </summary>
        public Dictionary<string, IndicatorGraph> GraphDict { get { return graphDict; } }
        /// <summary>
        /// 用户线段
        /// </summary>
        public List<UserLine> UserLineList { get { return userLineList; } }
        /// <summary>
        /// 数量
        /// </summary>
        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
            }
        }
        /// <summary>
        /// 缓存Bar数量
        /// </summary>
        public int MaxCacheCount
        {
            get { return _maxCacheCount; }
            set
            {
                _maxCacheCount = value;
            }
        }
        ///// <summary>
        ///// 最新时间
        ///// </summary>
        //public DateTime LastDateTime { get { return barDatas[barDatas.Count - 1].RealDateTime; } }
        ///// <summary>
        ///// 初始时间
        ///// </summary>
        //public DateTime FirstDateTime { get { return barDatas[0].RealDateTime; } }

        #endregion

        #region 公共方法
        /// <summary>
        /// 添加用户线段
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="title"></param>
        public void AddUserLine(PointF point1, PointF point2, Color color, DashStyle style, float thickness = 1, string title = "")
        {
            userLineList.Add(new UserLine(point1, point2, color, style, thickness, title));
        }
        ///// <summary>
        ///// 更新最后一条线段
        ///// </summary>
        //public void UpdateLastUserLine(PointF updatePoint)
        //{
        //    if (userLineList.Count > 0)
        //    {
        //        var userLine= userLineList[userLineList.Count - 1];
        //        userLine.PointB = updatePoint;
        //    }
        //}
        /// <summary>
        /// 获取用户线段
        /// </summary>
        /// <param name="pIndex">倒数第几个</param>
        /// <returns></returns>
        public UserLine GetUserLine(int pIndex = 0)
        {
            if (userLineList.Count - pIndex - 1 >= 0)
                return userLineList[userLineList.Count - pIndex - 1];
            else return null;
        }

        /// <summary>
        /// 添加参考线
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="color"></param>
        /// <param name="lineStyle"></param>
        public void AddGuidLine(string name,double value,Color color, EnumLineStyle lineStyle = EnumLineStyle.DotLine)
        {
            if (!graphDict.ContainsKey(name))
            {
                var graph = new IndicatorGraph() { Name = name, LineStyle = lineStyle };
                graph.AddValue(value, color);
                graphDict.Add(name, graph);
            }
        }

        /// <summary>
        /// 删除参考线
        /// </summary>
        /// <param name="name"></param>
        public void DelGuidLine(string name)
        {
            graphDict.Remove(name);
        }
        ///// <summary>
        ///// 根据索引获取时间
        ///// </summary>
        ///// <param name="index"></param>
        ///// <returns></returns>
        //public DateTime GetDateTime(int index)
        //{
        //    if (index < Count)
        //        return barDatas[index].RealDateTime;
        //    else
        //        return DateTime.MinValue;
        //}
        ///// <summary>
        ///// 根据时间获取索引
        ///// </summary>
        ///// <param name="dateTime"></param>
        ///// <returns></returns>
        //public int GetIndex(DateTime dateTime)
        //{
        //    var bars = barDatas.Where(d => d.RealDateTime.Equals(dateTime));
        //    if (bars != null && bars.Count() > 0)
        //        return barDatas.IndexOf(bars.ElementAt(0));
        //    else
        //        return -1;
        //}
        /// <summary>
        /// 根据索引获取Bar数据(0->最新一根，1->前一根,以此推类)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public BarData GetBarData(int index = 0)
        {
            if (index < Count)
                return barDatas[barDatas.Count - index - 1];
            else
                return null;
        }
        /// <summary>
        /// 获取最新缓存Bar数据
        /// </summary>
        /// <returns></returns>
        public List<BarData> GetBarDatas()
        {
            return barDatas;
        }
        ///// <summary>
        ///// 根据时间获取Bar数据
        ///// </summary>
        ///// <param name="dateTime"></param>
        ///// <returns></returns>
        //public BarData GetBarData(DateTime dateTime)
        //{
        //    var bars = barDatas.Where(d => d.RealDateTime.Equals(dateTime));
        //    if (bars != null && bars.Count() > 0)
        //        return bars.ElementAt(0);
        //    else
        //        return null;
        //}

        public void BindData(List<BarData> datas)
        {
            barDatas.Clear();
            barDatas.AddRange(datas);
            Caculate();
        }

        /// <summary>
        /// 添加Bar数据至最后
        /// </summary>
        /// <param name="bar"></param>
        public virtual void AddBarData(BarData bar)
        {
            barDatas.Add(new BarData(bar));
            if (IsSimpleMode && barDatas.Count > MaxCacheCount)
            {
                barDatas.RemoveAt(0);
            }
            _count++;
        }
        /// <summary>
        /// 更新Bar数据
        /// </summary>
        /// <param name="bar"></param>
        /// <returns></returns>
        public virtual void UpdateBarData(BarData bar)
        {
            barDatas[barDatas.Count - 1] = new BarData(bar);
        }
        /// <summary>
        /// 插入Bar数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bar"></param>
        public void InsertBar(int index, BarData bar)
        {
            if (index < barDatas.Count)
            {
                barDatas.Insert(index, new BarData(bar));
            }
            else
            {
                barDatas.Add(bar);
            }
            if (barDatas.Count > MaxCacheCount)
            {
                barDatas.RemoveAt(0);
            }
            _count++;
            Caculate();
        }
        /// <summary>
        /// 批量添加Bars
        /// </summary>
        public void AddBars(List<BarData> bars)
        {
            barDatas.AddRange(bars);
            _count += bars.Count;
            Caculate();
        }
        /// <summary>
        /// 批量插入Bars
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bars"></param>
        public void InsertBars(int index, List<BarData> bars)
        {
            if (index < barDatas.Count)
            {
                if (bars[bars.Count - 1].RealDateTime == barDatas[index].RealDateTime)
                {
                    barDatas.RemoveAt(index);//删除第一根实时接收Bar数据
                    _count--;
                }
                _count += bars.Count;
                barDatas.InsertRange(index, bars);
                Caculate();
            }
        }

        public Dictionary<string, double> GetLastValues()
        { 
            Dictionary<string, double> resultDict = new Dictionary<string, double>();
            foreach (var vd in valueDict)
            {
                if (Count != 0)
                    resultDict.Add(vd.Key, vd.Value[Count - 1]);
                else
                    resultDict.Add(vd.Key, JPR.NaN);
            }
            return resultDict;
        }
        ///// <summary>
        ///// 根据索引获取值
        ///// </summary>
        ///// <param name="index"></param>
        ///// <returns></returns>
        //public double GetValue(int index)
        //{
        //    if (index < Count)
        //        return Item[index];
        //    else
        //        return JPR.NaN;
        //}
        ///// <summary>
        ///// 根据时间获取值
        ///// </summary>
        ///// <param name="dateTime"></param>
        ///// <returns></returns>
        //public double GetValue(DateTime dateTime)
        //{
        //    var index = GetIndex(dateTime);
        //    if (index > -1)
        //        return GetValue(index);
        //    else
        //        return JPR.NaN;
        //}
        #endregion
    }
}
