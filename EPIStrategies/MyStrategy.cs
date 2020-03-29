using EPI.Comet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPI.Model;
using EPI.Asteroids.Indicators;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Integration;

namespace EPIStrategies
{
    public class MyStrategy : BaseFT
    {
        public MyStrategy(string strategyName) : base(strategyName)
        {

        }
        #region 我的策略
        //------------------------------------------------------------------------
        // 简称: MyStrategy
        // 类别: 模版
        // 说明：我的策略
        //------------------------------------------------------------------------
        /// <summary>
        /// 初始化策略
        /// </summary>
        /// <param name="sender">策略参数</param>
        /// <returns></returns>
        public override bool InitStrategy(object sender)
        {
            return base.InitStrategy(sender);
        }
        /// <summary>
        /// 数据加载完成
        /// </summary>
        /// <param name="contract">合约</param>
        /// <param name="cycle">周期</param>
        /// <param name="barDatas">Bar数据</param>
        /// <param name="isLast">是否最后一个数据</param>
        public override void FinishedLoadData(string contract, string cycle, List<BarData> barDatas, bool isLast)
        {
            base.FinishedLoadData(contract, cycle, barDatas, isLast);
        }
        /// <summary>
        /// Tick行情
        /// </summary>
        /// <param name="tickData"></param>
        public override void RtnTickData(TickData tickData)
        {
            base.RtnTickData(tickData);
        }
        /// <summary>
        /// Bar行情
        /// </summary>
        /// <param name="barData">最新Bar数据</param>
        /// <param name="isNewBar">是否下一周期的Bar（Tick实时刷新Bar时用于判断是否下一周期的Bar）</param>
        public override void RtnBarData(BarData barData, bool isNewBar)
        {
            base.RtnBarData(barData, isNewBar);
        }
        /// <summary>
        /// 委托回报
        /// </summary>
        /// <param name="rOrder">委托单</param>
        public override void RtnOrder(RspOrders rOrder)
        {
            base.RtnOrder(rOrder);
        }
        /// <summary>
        /// 成交回报
        /// </summary>
        /// <param name="rTrade">成交单</param>
        public override void RtnTrade(RspTrades rTrade)
        {
            base.RtnTrade(rTrade);
        }
        /// <summary>
        /// 消息回报
        /// </summary>
        /// <param name="rMsg">消息</param>
        public override void RtnMessage(JRspMessage rMsg)
        {
            base.RtnMessage(rMsg);
        }
        //------------------------------------------------------------------------
        // 编译版本	EPI2019.11.19
        // 版权所有	EPAI.TECH 2019－2029
        // 更改声明	EPAI.TECH保留对EPI平台每一版本的EPI策略的修改和重写的权利
        //------------------------------------------------------------------------
        #endregion
    }
}
