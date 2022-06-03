using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace Lyh_TcpClientLibrary
{
    public class TcpClient_Lyh
    {
        public delegate void ReceivedMessageHandler(object sender, string message);

        /// <summary>
        /// 接收网络数据事件，不带回车换行
        /// 如果需要赋值给窗体控件，请使用
        /// this.Invoke(new Action(() =>{richTextBox1.Text += message + "\r\n";}));
        /// </summary>
        public event ReceivedMessageHandler ReceivedMessage;

        private TcpClient tcpClient;
        private NetworkStream network;
        private Thread thread1;

        private string hostname;
        private int port;
        private bool isTcpConnected;
        private bool thread60Run = true;
        private Encoding encoding = Encoding.UTF8;
        private System.Timers.Timer timer;

        /// <summary>
        /// 初始化 TcpClient_Lyh 类的新实例。
        /// </summary>
        /// <param name="hostname">要连接到的远程主机的 DNS 名。</param>
        /// <param name="port">要连接到的远程主机的端口号。</param>
        public TcpClient_Lyh(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;
        }

        /// <summary>
        /// 初始化 TcpClient_Lyh 类的新实例。
        /// </summary>
        /// <param name="hostname">要连接到的远程主机的 DNS 名。</param>
        /// <param name="port">要连接到的远程主机的端口号。</param>
        /// <param name="encoding">表示字符编码。</param>
        public TcpClient_Lyh(string hostname, int port, Encoding encoding)
        {
            this.hostname = hostname;
            this.port = port;
            this.encoding = encoding;
        }

        ~TcpClient_Lyh()
        {
            //tcpClient?.GetStream().Close();
            tcpClient?.Close();
            timer.Stop();
            thread60Run = false;
            Dispose();
        }

        /// <summary>
        /// 将客户端连接到指定主机上的指定端口。
        /// </summary>
        public void Connect()
        {
            thread1 = new Thread(ReadTcpThread);
            thread1.IsBackground = true;
            thread1.Start();

            timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 5000; //执行间隔时间,单位为毫秒; 这里实际间隔为10分钟
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(FuncPing);
        }

        public void Close()
        {
            this.isTcpConnected = false;
            timer.Stop();
            thread60Run = false;
            Dispose();
        }

        public void Dispose()
        {
            tcpClient?.Close();
        }

        private void FuncPing(object sender, ElapsedEventArgs e)
        {
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();

            var pingReply1 = ping.Send(this.hostname, this.port);

            if (pingReply1.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                this.isTcpConnected = true;
            }
            else
            {
                this.isTcpConnected = false;
            }
        }

        private void ReadTcpThread()
        {
            while (thread60Run)
            {
                try
                {
                    tcpClient = new TcpClient();
                    tcpClient.Connect(this.hostname, this.port);
                    network = tcpClient.GetStream();
                    this.isTcpConnected = true;
                    while (IsTcpConnected(tcpClient) && isTcpConnected)
                    {
                        if (tcpClient.Available > 0)
                        {
                            Thread.Sleep(20);
                            var buffer = new byte[20480];
                            int rdLength = network.Read(buffer, 0, buffer.Length);
                            if (rdLength > 0)
                            {
                                var rdtxt = this.encoding.GetString(buffer, 0, rdLength).Trim();
                                if (ReceivedMessage != null)
                                {
                                    ReceivedMessage(tcpClient, rdtxt);
                                }
                            }
                            else if (rdLength == 0)
                            {
                                break;
                            }
                        }
                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    tcpClient?.Close();
                }
                if (!thread60Run)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
        }


        /// <summary>
        /// 将数据写入 System.Net.Sockets.NetworkStream。
        /// 若果没有建立连接将抛异常
        /// </summary>
        /// <param name="buffer">类型的数组 System.Byte ，其中包含要写入到数据 System.Net.Sockets.NetworkStream。</param>
        /// <param name="offset">中的位置 buffer 从其开始写入数据。</param>
        /// <param name="size">要写入的字节数 System.Net.Sockets.NetworkStream。</param>
        public void Send(byte[] buffer, int offset, int size)
        {
            network.Write(buffer, offset, size);
        }

        private bool IsTcpConnected(TcpClient tcpClient)
        {
            if (this.isTcpConnected)
            {
                return !((tcpClient.Client.Poll(1000, SelectMode.SelectRead) && tcpClient.Client.Available == 0) || !(tcpClient.Client.Connected));
            }
            return false;
        }
    }
}