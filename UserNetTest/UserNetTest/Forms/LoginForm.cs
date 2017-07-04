using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserNetTest.Forms
{
    public partial class LoginForm : Form
    {
        public delegate void LoginResultHandle(string card);
        public LoginResultHandle loginResultEvent; 

        public LoginForm(LoginResultHandle loginevent)
        {
            InitializeComponent();
            if(loginevent != null)
            {
                this.loginResultEvent += loginevent;
            }
        }

        //登录
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if(this.loginResultEvent != null && !this.textEdit1.Text.Equals(""))
            {
                this.loginResultEvent(this.textEdit1.Text);
                this.Close();
            }



        }
    }
}
