using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Google.ProtocolBuffers;
using System.Windows.Forms;
using UserNetTest.Tools;

namespace UserNetTest.Tools
{
    //UI回调代理
    public delegate void UIHandleBlock();
    // 连接结果回调
    public delegate void ConnectResultBlock();
    // 接受回调代理
    public delegate void DataResultBlock(ResultModel result);

    public class NetMessageManage
    {
        public int index;
        private Socket clientSocket;
        //private static NetMessageManage _instance;
        private const string ipString = "jorkenw.gnway.org";
        private const int port = 8465;

        #region
       
        public event DataResultBlock ResultBlockHandle;

       
        public event ConnectResultBlock ConnectBlockHandle;
        
       
        #endregion

        public NetMessageManage(int tem)
        {
            index = tem;
        }
        #region 连接服务器
        //连接服务器
        public void ConnectServer(ConnectResultBlock connect)
        {
            this.ConnectBlockHandle += connect;
            this.ConnectSever();
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        private void ConnectSever()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.BeginConnect(ipString, port, new AsyncCallback(ConnectCallback), clientSocket);
        }
        //关闭服务器连接
        public void CloseServerConnect()
        {
            clientSocket.Close();
        }
        // 连接服务器回调
         private void ConnectCallback(IAsyncResult asyncConnect)
        {
            //这里做一个超时的监测，当连接超过5秒还没成功表示超时  
            bool success = asyncConnect.AsyncWaitHandle.WaitOne(100, true);
            if (!success)
            {
                //超时  
                //clientSocket.Close();
                Console.WriteLine("connect Time Out");
            }
            else
            {
                System.Console.WriteLine("Connect success");
                Thread thread = new Thread(ReceiveData);
                thread.Start();
                //发送连接成功回调
                if(this.ConnectBlockHandle!=null)
                {
                    this.ConnectBlockHandle();
                }
            }

        }
        #endregion
        #region 接收数据
        //循环接收数据
        private void ReceiveData()
        {
            //不断接收服务器发来的数据
            while (true)
            {

                if (!clientSocket.Connected)
                {
                    Console.WriteLine("断开连接");
                    break;
                }
                try
                {
                    // Console.WriteLine("Time:" + DateTime.Now);
                    byte[] receiveBytes = new byte[1024];
                    //存储数据头的所有字节数 varint32:1419 1417
                    Int32 len = clientSocket.Receive(receiveBytes, 0);
                    Console.WriteLine("len:" + len);
                    if (len > 0)
                    {
                        //处理接受到的数据
                        HandleReceveBytes(receiveBytes, len);
                    }
                }
                catch(Exception exc)
                {
                   // System.Console.WriteLine("接收服务器数据出错");

                }





            }
        }
        //处理粘包(递归) 接收的长度
        private void HandleReceveBytes(byte[] recveBytes, Int32 recelen)
        {
            try
            {
                CodedInputStream inputStream = CodedInputStream.CreateInstance(recveBytes);
                //数据所有长度
                Int32 varint32 = (Int32)inputStream.ReadRawVarint32();
                //获取消息头长度
                Int32 headlength = CodedOutputStream.ComputeRawVarint32Size((uint)varint32);
                //   System.Console.WriteLine("len:" + recelen + "\nvarint32:" + varint32 + "\nlen:" + headlength);

                //如果所有有长度大于接收的长度。断包了
                if (varint32 > recelen - headlength)
                {

                    //需要的长度
                    Int32 needlen = varint32 + headlength - recelen;
                    //已经的长度
                    Int32 haslen = recelen;
                    byte[] resArr = new byte[varint32 + headlength];
                    recveBytes.CopyTo(resArr, 0);

                    while (true)
                    {
                        //
                        byte[] newResult = new byte[needlen];
                        int len = clientSocket.Receive(newResult, 0, needlen, SocketFlags.None);
                        //System.Console.WriteLine("len:" + len + "\nhaslen:" + haslen + "\nneedlen:" + needlen);

                        newResult.CopyTo(resArr, haslen);
                        haslen += len;
                        needlen -= len;

                        if (needlen == 0)
                        {
                            break;
                        }
                    }

                    Thread thread = new Thread(new ParameterizedThreadStart(ReceiveDataHandle));
                    thread.Start(resArr);

                }
                else
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(ReceiveDataHandle));
                    thread.Start(recveBytes);
                    //  ReceiveDataHandle(recveBytes);
                    //如果实际接受的长度大于包体长度+包头长（粘包）
                    if (recelen > (varint32 + headlength))
                    {
                        //剩下的进行解决
                        byte[] res = recveBytes.Skip<byte>(varint32 + headlength).ToArray<byte>();
                        HandleReceveBytes(res, recelen - varint32 - headlength);
                    }
                }

            }
            catch (Exception exc)
            {
                System.Console.WriteLine("接受数据出问题：" + exc);
            }

        }


        //接收数据处理
        private void ReceiveDataHandle(object obj)
        {
            byte[] result = (byte[])obj;
            try
            {
                CodedInputStream inputStream = CodedInputStream.CreateInstance(result);
                int varint32 = (int)inputStream.ReadRawVarint32();
                byte[] body = inputStream.ReadRawBytes(varint32);
                MessagePack pack = MessagePack.ParseFrom(body);
                //  System.Console.WriteLine("pack:"+ pack);
                if (ResultBlockHandle != null)
                {
                    ResultBlockHandle(new ResultModel()
                    {
                        pack = pack,
                        index = this.index,
                        error = 0,
                    });

                }

            }
            catch (Exception exp)
            {
                System.Console.WriteLine("ReceiveMessage Exception:" + exp.ToString());
            }

        }
        #endregion

        #region 发送数据
        // 发送数据1
        private void SendMsg(IMessageLite value)
        {

            //		resultBlock = result;

            if (!IsConnected())
            {
                System.Console.WriteLine("服务器未连接");
                return;
            }
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    CodedOutputStream outputStream = CodedOutputStream.CreateInstance(stream);
                    //一定要去看它的代码实现，
                    outputStream.WriteMessageNoTag(value);
                    /**
                    * WriteMessageNoTag 等价于 WriteVarint32, WriteByte(byte[])
                    * 也就是：变长消息头 + 消息体
                    */
                    outputStream.Flush();

                    byte[] data = stream.ToArray();

                    if (!IsConnected())
                    {
                        //					clientSocket.Close();  
                        return;
                    }
                    //发送数据
                    clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), clientSocket);
                }

            }
            catch (Exception exp)
            {
                //Debug.Log("send message error" + exp.ToString());

            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="resultBlock">Result block.</param>
        public void SendMsg(IMessageLite value, DataResultBlock resultBlock)
        {
            ResultBlockHandle += resultBlock;
            SendMsg(value);
        }

        //发送数据结果回调
        private void SendCallback(IAsyncResult asyncConnect)
        {

            bool success = asyncConnect.AsyncWaitHandle.WaitOne(5000, true);
            if (!success)
            {
                //			clientSocket.Close ();  
                System.Console.WriteLine("Failed to SendMessage server.");
            }
            else
            {
                System.Console.WriteLine("SendSuccess");

            }
        }
        #endregion

        //是否连接中
        private bool IsConnected()
        {
            return clientSocket.Connected;
        }

        //再次连接服务器
        public void AgainConnect()
        {


        }
        /// <summary>
        /// 移除结果回调
        /// </summary>
        /// <param name="result"></param>
        public void RemoveResultBlock(DataResultBlock result)
        {
            this.ResultBlockHandle -= result;

        }
        /// <summary>
        /// 添加方法
        /// </summary>
        /// <param name="result"></param>
        public void AddResultBlock(DataResultBlock result)
        {
            this.ResultBlockHandle += result;

        }


    }

    /// <summary>
    /// 结果Model
    /// </summary>
    public class ResultModel
    {
        public int error = 0;   // 0/1 无错误、有错误
        public MessagePack pack;
        public int index;

    }
}
