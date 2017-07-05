using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UserNetTest.Forms;
using UserNetTest.Tools;
using static UserNetTest.Tools.NetMessageManage;

namespace UserNetTest
{
    public enum COM_STATUS
    {
        CLOSE_STATUS = 0,       //  关机状态
        OPEN_STATUS,            //开机状态
        UP_STATUS,              //上机状态  
        DOWN_STATUS,            //下机状态（电脑开机状态）
        SEL_STATUS,             //选中状态


    }

    public partial class Form1 : Form
    {



        private SimpleButton selectButton = null;
        //  private Dictionary<SimpleButton, COM_STATUS> comDict = new Dictionary<SimpleButton, COM_STATUS>();
        private Dictionary<int, SimpleModel> comDict = new Dictionary<int, SimpleModel>();

        private Dictionary<COM_STATUS, Color> comColor = new Dictionary<COM_STATUS, Color>();


        public Form1()
        {
            InitializeComponent();

            //添加颜色
            comColor.Add(COM_STATUS.CLOSE_STATUS, Color.LightGray);
            comColor.Add(COM_STATUS.SEL_STATUS, Color.Red);
            comColor.Add(COM_STATUS.OPEN_STATUS, Color.FromArgb(192, 255, 192));
            comColor.Add(COM_STATUS.UP_STATUS, Color.FromArgb(192, 255, 255));
            InitUI();
        }

        #region 初始化UI
        //按钮编剧
        const int DISTANCE = 10;
        const int NUM = 5;

        //初始化UI
        private void InitUI()
        {
            //添加电脑、
            this.panel1.AutoScroll = true;
            // this.panel1.SizeChanged += Panel1_SizeChanged ;
            int bth_W = (this.panel1.Width - DISTANCE * (NUM + 1)) / NUM;

            for (int i = 0; i < 80; i++)
            {
                SimpleButton com = new SimpleButton();
                int x = DISTANCE * (i % NUM + 1) + i % NUM * bth_W;
                int y = DISTANCE * (i / NUM + 1) + i / NUM * bth_W;
                com.Location = new Point(x, y);
                com.Size = new Size(bth_W, bth_W);
                com.Text = "192.168.0." + (i + 1);
                com.Click += Com_Click;
                com.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
                Color bgColor = Color.Gray;
                comColor.TryGetValue(COM_STATUS.CLOSE_STATUS, out bgColor);
                com.Appearance.BackColor = bgColor;
                this.panel1.Controls.Add(com);

                int index = this.panel1.Controls.GetChildIndex(com);
                SimpleModel model = new SimpleModel() {
                    button = com,
                    status = COM_STATUS.CLOSE_STATUS,
                    manage = new NetMessageManage(index),
                    ip = "192.168.0." + (i + 1),

                };


                this.comDict.Add(index, model);

            }

        }
        #endregion

        #region 电脑按钮点击事件
        private void Com_Click(object sender, EventArgs e)
        {

            if (sender.Equals(this.selectButton))
            {
                return;
            }

            if (this.selectButton != null)
            {
                //需要获取被选择的按钮状态
                SimpleModel model = this.GetModel(this.panel1.Controls.GetChildIndex(this.selectButton));
                Color btnColor = Color.Gray;
                comColor.TryGetValue(model.status, out btnColor);
                this.selectButton.Appearance.BackColor = btnColor;

            }


            Color bgColor = Color.Gray;
            comColor.TryGetValue(COM_STATUS.SEL_STATUS, out bgColor);
            ((SimpleButton)sender).Appearance.BackColor = bgColor;
            this.selectButton = (SimpleButton)sender;


            //判断当前按钮可以进行的操作

            ComputerOperation();


        }
        //获取Model
        private SimpleModel GetModel(int index)
        {

            SimpleModel model = null;
            this.comDict.TryGetValue(index, out model);
            return model;
        }

        //电脑操作
        private void ComputerOperation()
        {
            //需要获取被选择的按钮状态
            SimpleModel model = this.GetModel(this.panel1.Controls.IndexOf(this.selectButton));
            COM_STATUS status = model.status;

            this.cardLabel.Text = "";
            this.MessageLabel.Text = "";
            this.panel4.Controls.Clear();
            switch (status)
            {
                case COM_STATUS.CLOSE_STATUS:       //关机状态（开机）
                    {
                        SimpleButton open = this.InitOpenButton("开机", OpenComputer);

                        this.panel4.Controls.Add(open);
                    }
                    break;

                case COM_STATUS.OPEN_STATUS:        //开机状态（上机。关机）
                    {
                        SimpleButton up = this.InitOpenButton("上机", UpComputer);
                        this.panel4.Controls.Add(up);

                        SimpleButton close = this.InitOpenButton("关机", CloseComputer);
                        this.panel4.Controls.Add(close);

                    }
                    break;


                case COM_STATUS.UP_STATUS:          //上机状态（下机，关机，充值）
                    {
                        SimpleButton down = this.InitOpenButton("下机", DownComputer);
                        this.panel4.Controls.Add(down);
                        SimpleButton pay = this.InitOpenButton("充值", UserPay);
                        this.panel4.Controls.Add(pay);
                        SimpleButton close = this.InitOpenButton("关机", CloseComputer);
                        this.panel4.Controls.Add(close);
                        this.cardLabel.Text = model.card;
                        this.MessageLabel.Text = "已扣金额 " + model.usebalance + " 元";
                    }
                    break;




            }
        }


        //初始化一个操作按钮
        private SimpleButton InitOpenButton(string text, EventHandler click)
        {
            int bth_W = this.panel4.Width;

            SimpleButton open = new SimpleButton();
            open.Size = new Size(bth_W, 40);
            open.Text = text;
            open.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            open.Dock = DockStyle.Top;
            open.Click += click;
            return open;
        }

        #endregion

        #region 按钮操作

        #region 进行开机（连接服务器）
        private void OpenComputer(object sender, EventArgs e)
        {
            SimpleModel model = this.GetModel(this.panel1.Controls.GetChildIndex(this.selectButton));

            //System.Console.WriteLine("OpenComputerIndex:" + model.manage.index);
            ClientNetOperation.OpenComputer(model.manage, model.ip, OpenComputerResult);
        }
        //开机回调
        private void OpenComputerResult(ResultModel result)
        {
            if (result.pack.Cmd != Cmd.CMD_CLIENT_OPEN)
            {
                return;
            }

            SimpleModel model = this.GetModel(result.index);
            model.manage.RemoveResultBlock(OpenComputerResult);
           // System.Console.WriteLine("OpenComputerResult:" + result.pack +"\nIndex:"+ result.index);

            if (result.pack.Content.MessageType == 1)
            {
               
                this.Invoke(new UIHandleBlock(delegate
                {
                    //成功回调
                    model.status = COM_STATUS.OPEN_STATUS;
                    //判断是否是当前按钮，是的话当前按钮可以进行的操作
                    SimpleButton button = (SimpleButton)this.panel1.Controls[result.index];
                    if (button.Equals(this.selectButton))
                    {
                        ComputerOperation();
                    }

                }));
            }
            else
            {
                this.Invoke(new UIHandleBlock(delegate
                {
                    MessageBox.Show("开机失败");
                }));
            }
        }

        #endregion

        #region 进行关机（服务器断开）
        private void CloseComputer(object sender, EventArgs e)
        {
            SimpleModel model = this.GetModel(this.panel1.Controls.GetChildIndex(this.selectButton));
            ClientNetOperation.CloseComputer(model.manage, CloseComputerResult);

        }
        //关机回调
        private void CloseComputerResult(ResultModel result)
        {
            if (result.pack.Cmd != Cmd.CMD_CLIENT_CLOSE)
            {
                return;
            }
            SimpleModel model = this.GetModel(result.index);
            model.manage.RemoveResultBlock(CloseComputerResult);
            System.Console.WriteLine("CloseComputerResult:" + result.pack);

            if (result.pack.Content.MessageType == 1)
            {
                //关闭服务器连接
                model.manage.CloseServerConnect();
                this.Invoke(new UIHandleBlock(delegate
                {
                    //成功回调
                    model.status = COM_STATUS.CLOSE_STATUS;
                    //判断是否是当前按钮，是的话当前按钮可以进行的操作
                    SimpleButton button = (SimpleButton)this.panel1.Controls[result.index];
                    if (button.Equals(this.selectButton))
                    {
                        ComputerOperation();
                    }

                }));


            }
            else
            {
                this.Invoke(new UIHandleBlock(delegate
                {
                    MessageBox.Show("关机失败");
                }));
            }
        }
        #endregion

        #region 进行上机（用户登录）
        private void UpComputer(object sender, EventArgs e)
        {
           
            LoginForm.LoginResultHandle login = new LoginForm.LoginResultHandle(delegate (string text) {
                int index = this.panel1.Controls.GetChildIndex(this.selectButton);
                SimpleModel model = this.GetModel(index);
                //获取信息回调
                GetSysMessage(model.manage);
                //输入身份证号
                model.card = text;
                model.usebalance = 0;
                ClientNetOperation.UpComputer(model.manage, text, UpComputerResult);
            });
            LoginForm form = new LoginForm(login);
            form.ShowInTaskbar = false;
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
        }
        //上机回调
        private void UpComputerResult(ResultModel result)
        {

            if (result.pack.Cmd != Cmd.CMD_CLIENT_LOGON)
            {
                return;
            }
            SimpleModel model = this.GetModel(result.index);
            model.manage.RemoveResultBlock(UpComputerResult);
            System.Console.WriteLine("UpComputerResult:" + result.pack);

            if (result.pack.Content.MessageType == 1)
            {
              
                this.Invoke(new UIHandleBlock(delegate
                {
                    //成功回调
                    model.status = COM_STATUS.UP_STATUS;
                    //判断是否是当前按钮，是的话当前按钮可以进行的操作
                    SimpleButton button = (SimpleButton)this.panel1.Controls[result.index];
                    if (button.Equals(this.selectButton))
                    {
                        ComputerOperation();
                    }
                }));
            }
            else
            {
                this.Invoke(new UIHandleBlock(delegate
                {
                    MessageBox.Show("上机失败");
                }));
            }
        }
        #endregion

        #region 获取系统信息反馈
        private void GetSysMessage(NetMessageManage manage)
        {
            manage.RemoveResultBlock(GetSysMessageResult);
            manage.AddResultBlock(GetSysMessageResult);
        }
        //获取系统信息反馈结果回调
        private void GetSysMessageResult(ResultModel result)
        {

            if (result.pack.Cmd != Cmd.CMD_CLIENT_SYSMESSAGE)
            {
                return;
            }
            SimpleModel model = this.GetModel(result.index);
            System.Console.WriteLine("GetSysMessageResult:" + result.pack);

            if (result.pack.Content.MessageType == 1)
            {
                
                SCSysMessage message = result.pack.Content.ScSysMessage;
                IList<string> paramslist = message.ParamsList;

                if (message.Cmd == 6)
                {
                    int blance = int.Parse(message.ParamsList[2]);
                    //扣费时间    下次扣费时间    扣费金额
                    model.usebalance += blance;
                    this.Invoke(new UIHandleBlock(delegate
                    {
                        //判断是否是当前按钮，是的话当前按钮可以进行的操作
                        SimpleButton button = (SimpleButton)this.panel1.Controls[result.index];
                        if (button.Equals(this.selectButton))
                        {
                            this.MessageLabel.Text = "已扣金额 " + model.usebalance + " 元";
                        }


                    }));

                }
                else if(message.Cmd == 2)
                {

                }

              
            }
            else
            {
               

            }
        }
        #endregion

        #region 进行下机（用户登出）
        private void DownComputer(object sender, EventArgs e)
        {
            int index = this.panel1.Controls.GetChildIndex(this.selectButton);
            SimpleModel model = this.GetModel(index);
            //model.manage.RemoveResultBlock

            ClientNetOperation.DownComputer(model.manage, model.card, DownComputerResult);
        }
        //下机回调
        private void DownComputerResult(ResultModel result)
        {
            if (result.pack.Cmd != Cmd.CMD_CLIENT_LOGOFF)
            {
                return;
            }
            SimpleModel model = this.GetModel(result.index);
            model.manage.RemoveResultBlock(DownComputerResult);
            System.Console.WriteLine("DownComputerResult:" + result.pack);
            if (result.pack.Content.MessageType == 1)
            {
               

                this.Invoke(new UIHandleBlock(delegate
                {
                    //成功回调
                    model.status = COM_STATUS.OPEN_STATUS;
                    //判断是否是当前按钮，是的话当前按钮可以进行的操作
                    SimpleButton button = (SimpleButton)this.panel1.Controls[result.index];
                    if (button.Equals(this.selectButton))
                    {
                        ComputerOperation();
                    }
                }));
            }
            else
            {
                this.Invoke(new UIHandleBlock(delegate
                {
                    MessageBox.Show("下机失败");
                }));
            }


        }
        #endregion

        #region 进行充值（用户充值）
        private void UserPay(object sender, EventArgs e)
        {
            int index = this.panel1.Controls.GetChildIndex(this.selectButton);
            SimpleModel model = this.GetModel(index);
            PreChargeForm form = new PreChargeForm(model);
            form.ShowInTaskbar = false;
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();


        }
        #endregion
        #endregion

        #region Panel Size 改变
        private void Panel1_SizeChanged(object sender, EventArgs e)
        {
            int bth_W = (this.panel1.Width - DISTANCE * (NUM + 1)) / NUM;
            this.panel1.AutoScroll = true;

            foreach (SimpleButton com in this.panel1.Controls)
            {
                int i = this.panel1.Controls.IndexOf(com);
              

                int x = DISTANCE * (i % NUM + 1) + i % NUM * bth_W;
                int y = DISTANCE * (i / NUM + 1) + i / NUM * bth_W;
                com.Location = new Point(x, y);
                com.Size = new Size(bth_W, bth_W);

            }
        }
        #endregion

    }

    #region SimpleModel
    public class SimpleModel
    {
        public SimpleButton button;
        public NetMessageManage manage;
        public COM_STATUS status;               
        public string card;                     //身份证号
        public string ip;                       //所使用的IP
        public int usebalance;                  //使用的费用
    }
    #endregion

}
