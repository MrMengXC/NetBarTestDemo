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
    public partial class CallServerForm : Form
    {
        private SimpleModel model;

        public CallServerForm(SimpleModel tem)
        {
            InitializeComponent();
            model = tem;
        }

        //呼叫
        private void simpleButton1_Click(object sender, EventArgs e)
        {

            string[] pars = {
                model.card,
                this.textBox1.Text,
            };
            ClientNetOperation.CallServer(model.manage, CallServerResult, pars);
        }

        private void CallServerResult(ResultModel result)
        {
            if(result.pack.Cmd != Cmd.CMD_COMMAND)
            {
                return;
            }

            model.manage.RemoveResultBlock(CallServerResult);
            System.Console.WriteLine("CallServerResult:" + result.pack);
            if(result.pack.Content.MessageType == 1)
            {
                this.Invoke(new UIHandleBlock(delegate {
                    MessageBox.Show("发送成功");

                }));
            }
        }
    }
}
