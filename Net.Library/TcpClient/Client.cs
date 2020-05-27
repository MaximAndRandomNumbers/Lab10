using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace SomeProject.Library.Client
{
    public class Client
    {
        public TcpClient tcpClient;


        public OperationResult ReceiveMessageFromServer()
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8081);
                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];
                NetworkStream stream = tcpClient.GetStream();
                
                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                tcpClient.Close();

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.ToString());
            }
        }

        public OperationResult SendMessageToServer(string message)
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8081);
                using (NetworkStream stream = tcpClient.GetStream())
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    tcpClient.Close();
                }
                return new OperationResult(Result.OK, "") ;
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        /// <summary>
        /// Отправка файла на сервер
        /// </summary>
        /// <param name="file">Путь к файлу</param>
        /// <returns>Результат операции</returns>
        public OperationResult SendFileToServer(string file)
        {
            try
            {
                byte[] bytesOfFile = File.ReadAllBytes(file);
                if (SendMessageToServer(">>file-send>>" + file).Result == Result.OK)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    tcpClient = new TcpClient("127.0.0.1", 8081);
                    using (NetworkStream stream = tcpClient.GetStream())
                    {
                        bf.Serialize(stream, bytesOfFile);
                        tcpClient.Close();
                    }
                   
                }
                return new OperationResult(Result.OK, "");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
    }
}
   
