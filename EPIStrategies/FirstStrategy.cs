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
    public class FirstStrategy : BaseFT
    {
        public FirstStrategy(string strategyName) : base(strategyName)
        {

        }
        #region 第一个策略
        //------------------------------------------------------------------------
        // 简称: FirstStrategy
        // 类别: Demo
        // 说明：第一个策略
        //------------------------------------------------------------------------
        KDJ kdj;                 //创建KDJ指标变量
        int kdjLength = 9;       //创建KDJ指标Length变量
        int kdjM1 = 3;           //创建KDJ指标M1变量
        int kdjM2 = 3;           //创建KDJ指标M2变量

        /// <summary>
        /// 初始化策略
        /// </summary>
        /// <param name="sender">策略参数,如:（9,3,3）</param>
        /// <returns></returns>
        public override bool InitStrategy(object sender)
        {
            try
            {
                var usrParams = ConvertParams(sender);
                if (usrParams.Length == 3)
                {
                    kdjLength = int.Parse(usrParams[0]);
                    kdjM1 = int.Parse(usrParams[1]);
                    kdjM2 = int.Parse(usrParams[2]);
                    //加载数据次数，对应LoadBarDatas调用次数
                    LoadDataCount = 1;
                    //加载Bar数据，用于初始化指标历史数据（取模拟数据前天数据，不需要过多）
                    var result = LoadBarDatas(Setting.Contract, Setting.Cycle, 100, false, false);
                    return result;
                }
                else
                {
                    Log("初始化策略失败，传入参数错误");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log("初始化策略失败", ex);
                return false;
            }
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
            try
            {
                if (contract == Setting.Contract)
                {
                    kdj = new KDJ(barDatas, kdjLength, kdjM1, kdjM2);
                    //获取合约保证金率及手续情况
                    var cTpDetail = GetContractTpDetail(contract);
                    //获取合约信息
                    Contracts cInfo = GetContract(contract);
                    Log(string.Format("合约:{0},合约乘数:{1},最小变动价位:{2},手续费:{3},保证金:{4}",
                        cInfo.Contract, cInfo.VolumeMultiple, cInfo.PriceTick,
                        cTpDetail.FeeValue, cTpDetail.MarginValue));
                }
                //判断是否最后一笔数据，否则等待后续数据过来
                if (isLast)
                {
                    //至此整个策略初始化完成，启动策略
                    StartStrategy();
                }
            }
            catch (Exception ex)
            {
                Log("处理回报数据错误", ex);
            }
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
            try
            {
                if (barData.Contract == Setting.Contract)
                {
                    //获取当前持仓
                    var position = GetPosition();
                    //添加最新Bar至指标
                    kdj.AddBarData(barData);

                    bool isCurCrossKdjUp = CrossKdjUp();
                    bool isCurCrossKdjDown = CrossKdjDown();

                    if (isCurCrossKdjDown)
                    {
                        Log(string.Format("[{0}] KDJ死叉出现", barData.Contract));
                        Sell(1);
                    }
                    else if (isCurCrossKdjUp)
                    {
                        Log(string.Format("[{0}] KDJ金叉出现", barData.Contract));
                        Buy(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("推动Bar数据时处理过程错误", ex);
            }
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

        /// <summary>
        /// KDJ金叉
        /// </summary>
        /// <returns></returns>
        bool CrossKdjUp()
        {
            //获取前一个D值，由于索引从0开始，所以总长度(kdj.Count)减2即为最后第二个
            var preDValue = kdj.GetDValue(kdj.Count - 2);
            var preJValue = kdj.GetJValue(kdj.Count - 2);
            //获取当前D值
            var dValue = kdj.GetDValue(kdj.Count - 1);
            var jValue = kdj.GetJValue(kdj.Count - 1);
            //判断数据是否有效，JPR为内部属性，可用于判断数值是否为正常数值
            if (!JPR.IsNaN(preDValue) && !JPR.IsNaN(preJValue) && !JPR.IsNaN(dValue) && !JPR.IsNaN(jValue))
            {
                //前J小于D值 并且 当前J大于D值，说明J上穿D
                return preJValue < preDValue && jValue > dValue;
            }
            return false;
        }
        /// <summary>
        /// KDJ死叉
        /// </summary>
        /// <returns></returns>
        bool CrossKdjDown()
        {
            var preDValue = kdj.GetDValue(kdj.Count - 2);
            var preJValue = kdj.GetJValue(kdj.Count - 2);
            var dValue = kdj.GetDValue(kdj.Count - 1);
            var jValue = kdj.GetJValue(kdj.Count - 1);
            if (!JPR.IsNaN(preDValue) && !JPR.IsNaN(preJValue) && !JPR.IsNaN(dValue) && !JPR.IsNaN(jValue))
            {
                return preJValue > preDValue && jValue < dValue;
            }
            return false;
        }
        //------------------------------------------------------------------------
        // 编译版本	EPI2019.11.19
        // 版权所有	EPAI.TECH 2019－2029
        // 更改声明	EPAI.TECH保留对EPI平台每一版本的EPI策略的修改和重写的权利
        //------------------------------------------------------------------------
        #endregion
    }
}
