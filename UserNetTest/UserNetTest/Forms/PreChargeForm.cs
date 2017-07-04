using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UserNetTest.Tools;
namespace UserNetTest.Forms
{
    public partial class PreChargeForm : Form
    {

        private SimpleModel preModel;
        public PreChargeForm(SimpleModel model)
        {
            InitializeComponent();
            preModel = model;
        }

        #region 充值
        //进行点击充值
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                int money = int.Parse(this.textBox1.Text);
                ClientNetOperation.UserPreCharge(preModel.manage, preModel.card, UserPreChargeResult, money);
            }
            catch (Exception exc)
            {

            }
        }
        //欲充值回调
        private void UserPreChargeResult(ResultModel result)
        {
            if (result.pack.Cmd != Cmd.CMD_PRECHARGE)
            {
                return;
            }
            preModel.manage.RemoveResultBlock(UserPreChargeResult);
            System.Console.WriteLine("UserPreChargeResult:"+result.pack);

            if (result.pack.Content.MessageType == 1)
            {
                MessageBox.Show("充值成功");
            }
            else
            {
                MessageBox.Show("充值成功");
            }
        }
        #endregion
    }
}
