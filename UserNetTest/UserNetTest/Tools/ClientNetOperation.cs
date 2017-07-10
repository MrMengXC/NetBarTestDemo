using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserNetTest.Tools
{
    class ClientNetOperation
    {

        #region 打开电脑
        public static void OpenComputer(NetMessageManage manage,string ip,DataResultBlock resultBlock)
        {
            //连接服务器
            ConnectResultBlock result = new ConnectResultBlock(delegate {

                //请求服务器开机

                CSClientOpen.Builder open = new CSClientOpen.Builder()
                {
                    Text = ip,
                };

                MessageContent.Builder content = new MessageContent.Builder()
                {
                    MessageType = 1,
                    CsClientOpen = open.Build(),

                };

                
                MessagePack.Builder pack = new MessagePack.Builder()
                {
                    Cmd = Cmd.CMD_CLIENT_OPEN,
                    Content = content.Build(),
                };
                manage.SendMsg(pack.Build(), resultBlock);

            });
            manage.ConnectServer(result);

           
        }
        #endregion

        #region 关闭电脑
        public static void CloseComputer(NetMessageManage manage, DataResultBlock resultBlock)
        {
            CSClientClose.Builder close = new CSClientClose.Builder();
            MessageContent.Builder content = new MessageContent.Builder()
            {
                MessageType = 1,
                CsClientClose = close.Build(),
            };

            MessagePack.Builder pack = new MessagePack.Builder()
            {
                Cmd = Cmd.CMD_CLIENT_CLOSE,
                Content = content.Build(),
            };
            manage.SendMsg(pack.Build(), resultBlock);
        }
        #endregion

        #region 用户上机
        public static void UpComputer(NetMessageManage manage,String card,DataResultBlock resultBlock)
        {
            CSLogon.Builder logon = new CSLogon.Builder()
            {
                Cardnumber = card,
            };
            MessageContent.Builder content = new MessageContent.Builder()
            {
                MessageType = 1,
                CsLogon = logon.Build(),
            };
            MessagePack.Builder pack = new MessagePack.Builder()
            {
                Cmd = Cmd.CMD_CLIENT_LOGON,
                Content = content.Build(),
            };
            manage.SendMsg(pack.Build(), resultBlock);
        }
        #endregion

        #region 用户下机
        public static void DownComputer(NetMessageManage manage, String card,DataResultBlock resultBlock)
        {
            CSLogoff.Builder off = new CSLogoff.Builder()
            {
                Cardnumber = card,
            };
            MessageContent.Builder content = new MessageContent.Builder()
            {
                MessageType = 1,
                CsLogoff = off.Build(),
            };

            MessagePack.Builder pack = new MessagePack.Builder()
            {
                Cmd = Cmd.CMD_CLIENT_LOGOFF,
                Content = content.Build(),
            };

            manage.SendMsg(pack.Build(), resultBlock);
        }
        #endregion

        #region 呼叫服务器
        public static void CallServer(NetMessageManage manage, DataResultBlock resultBlock,string[] pars)
        {
            CSCommand.Builder command = new CSCommand.Builder()
            {
                 Cmd = 9,
               
            };
            foreach(string par in pars)
            {
                command.AddParams(par);
            }
            MessageContent.Builder content = new MessageContent.Builder()
            {
                MessageType = 1,
                CsCommand = command.Build(),
            };

            MessagePack.Builder pack = new MessagePack.Builder()
            {
                Cmd = Cmd.CMD_COMMAND, 
                Content = content.Build(),
            };

            manage.SendMsg(pack.Build(), resultBlock);
        }
        #endregion

        #region 用户充值
        //用户预充值 获取充值二维码
        public static void UserPreCharge(NetMessageManage manage, String card, DataResultBlock resultBlock,int amount)
        {
            CSPreCharge.Builder charge = new CSPreCharge.Builder()
            {
                Cardnumber = card,
                Amount = amount,
                Paymode = 1,
                Offical = 0,
            };
            MessageContent.Builder content = new MessageContent.Builder()
            {
                MessageType = 1,
                CsPreCharge = charge.Build(),
            };
            MessagePack.Builder pack = new MessagePack.Builder()
            {
                Cmd = Cmd.CMD_PRECHARGE,
                Content = content.Build(),
            };
            manage.SendMsg(pack.Build(), resultBlock);
        }
        //获取充值结果
        public static void UserRecharge(NetMessageManage manage, DataResultBlock resultBlock)
        {           
             
            MessagePack.Builder pack = new MessagePack.Builder()
            {
               Cmd = Cmd.CMD_TOCHARGE,
            };
            manage.SendMsg(pack.Build(), resultBlock);
        }

        #endregion

        #region 获取系统消息反馈
        //获取系统消息
        public static void GetSysMessage(NetMessageManage manage, DataResultBlock resultBlock)
        {
            MessagePack.Builder pack = new MessagePack.Builder();
            pack.Cmd = Cmd.CMD_CLIENT_SYSMESSAGE;
            manage.SendMsg(pack.Build(), resultBlock);
        }
        #endregion


        #region 进入商店获取商品列表
        public static void GoShop(NetMessageManage manage, DataResultBlock resultBlock, int category, string key)
        {
            StructPage.Builder page = new StructPage.Builder()
            {
                Pagebegin = 0,
                Pagesize = 15,
                Order = 0,
                Fieldname = 0,
            };

            CSGoodsFind.Builder find = new CSGoodsFind.Builder();
            find.Page = page.Build();
            find.Category = category;
            if(key != null && !key.Equals(""))
            {
                find.Keywords = key;
            }
        

            MessageContent.Builder content = new MessageContent.Builder();
            content.MessageType = 1;
            content.CsGoodsFind = find.Build();

            MessagePack.Builder pack = new MessagePack.Builder();
            pack.Cmd = Cmd.CMD_GOODS_FIND;
            pack.Content = content.Build();
            manage.SendMsg(pack.Build(), resultBlock);
        }
        #endregion

        #region 进行购买某个物品
     
        public static void PreBuyProduct(NetMessageManage manage, DataResultBlock resultBlock,string card,int goodid,int num)
        {
            CSPreBuy.Builder buy = new CSPreBuy.Builder();
            buy.Cardnumber = card;
            buy.Goodsid = goodid;
            buy.Goodsnum = num;

            MessageContent.Builder content = new MessageContent.Builder();
            content.MessageType = 1;
            content.CsPreBuy = buy.Build();

            MessagePack.Builder pack = new MessagePack.Builder();
            pack.Cmd = Cmd.CMD_PREBUY;
            pack.Content = content.Build();
            manage.SendMsg(pack.Build(), resultBlock);
        }
        #endregion


        public static void ToBuyProduct(NetMessageManage manage, DataResultBlock resultBlock)
        {
            MessagePack.Builder pack = new MessagePack.Builder();
            pack.Cmd = Cmd.CMD_TOBUY;
            manage.SendMsg(pack.Build(), resultBlock);
        }
    }
}
