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
        public static void OpenComputer(NetMessageManage manage,string ip,NetMessageManage.DataResultBlock resultBlock)
        {
            //连接服务器
            NetMessageManage.ConnectResultBlock result = new NetMessageManage.ConnectResultBlock(delegate {

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
        public static void CloseComputer(NetMessageManage manage, NetMessageManage.DataResultBlock resultBlock)
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
        public static void UpComputer(NetMessageManage manage,String card, NetMessageManage.DataResultBlock resultBlock)
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
        public static void DownComputer(NetMessageManage manage, String card, NetMessageManage.DataResultBlock resultBlock)
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

        #region 用户充值
        //用户预充值 获取充值二维码
        public static void UserPreCharge(NetMessageManage manage, String card, NetMessageManage.DataResultBlock resultBlock,int amount)
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
        public static void UserRecharge(NetMessageManage manage, NetMessageManage.DataResultBlock resultBlock)
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
        public static void GetSysMessage(NetMessageManage manage, NetMessageManage.DataResultBlock resultBlock)
        {
            MessagePack.Builder pack = new MessagePack.Builder();
            pack.Cmd = Cmd.CMD_CLIENT_SYSMESSAGE;
            manage.SendMsg(pack.Build(), resultBlock);
        }
        #endregion
    }
}
