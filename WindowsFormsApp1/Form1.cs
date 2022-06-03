using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Lyh_TcpClientLibrary.TcpClient_Lyh tcpClient_Lyh ;

        public Form1()
        {
            InitializeComponent();

            tcpClient_Lyh = new Lyh_TcpClientLibrary.TcpClient_Lyh("192.168.250.124", 8080);
            tcpClient_Lyh.Connect();
            tcpClient_Lyh.ReceivedMessage += TcpClient_Lyh_ReceivedMessage;
        }

        private void TcpClient_Lyh_ReceivedMessage(object sender, string message)
        {
            this.Invoke(new Action(() =>
            {
                richTextBox1.Text += message + "\r\n";
            }));
            //richTextBox1.Text += message + "\r\n";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcpClient_Lyh.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var b = Encoding.UTF8.GetBytes("123azz水电费");
            tcpClient_Lyh.Send(b,0,b.Length);
        }
    }
}
