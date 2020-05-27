using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeProject.Library.Server
{
    public class Server
    {
        /// <summary>
        /// Текущее количество соединений
        /// </summary>
        private int NumberOfConnections;

        /// <summary>
        /// Максимально возможное количество соединений
        /// </summary>
        private const int  MAX_CONNECTIONS = 4;

        TcpListener serverListener;

        /// <summary>
        /// Порядковый номер файла
        /// </summary>
        int fileNumber = 0;

        /// <summary>
        /// Команда, которая говорит о том, что после сообщения  будет отправлен файл
        /// </summary>
        const String fileSendCommand = ">>file-send>>";

        public Server()
        {
            serverListener = new TcpListener(IPAddress.Loopback, 8081);
            NumberOfConnections = 0;
        }

        public bool TurnOffListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Stop();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn off listener: " + e.Message);
                return false;
            }
        }

        public async Task TurnOnListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Start();
                while (true)
                {
                    OperationResult result = await ReceiveMessageFromClient();
                    if (result.Result == Result.Fail)
                    {
                        SendMessageToClient(result.Message);
                        Console.WriteLine("Unexpected error: " + result.Message);
                    }
                       
                    else
                    {
                        if (result.Message.StartsWith(fileSendCommand))
                        {
                            OperationResult res = await RecieveFileFromClient(getExtension(result.Message));
                            Console.WriteLine(res.Message);
                            SendMessageToClient(res.Message);
                        }
                        else
                        {
                            SendMessageToClient("Сообщение получено!");
                            Console.WriteLine("New message from client: " + result.Message);
                        }
                    }
                       
                   
                }
            }
            catch (Exception e)
            {
                SendMessageToClient("Ошибка сервера!");
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }

        
        public async Task<OperationResult> ReceiveMessageFromClient()
        {
            try
            {
                Console.WriteLine("Waiting for connections...");
                TcpClient client = null;
                if (NumberOfConnections < MAX_CONNECTIONS)
                {
                    client = serverListener.AcceptTcpClient();
                    Interlocked.Increment(ref NumberOfConnections);
                }
                if (client == null)
                {
                    return new OperationResult(Result.Fail, "Too many connections");
                }

                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];
                NetworkStream stream = client.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                String message = recievedMessage.ToString();
                client.Close();

                Interlocked.Decrement(ref NumberOfConnections);
               

                return new OperationResult(Result.OK, message);
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }
        /// <summary>
        /// Получение расширения файла из сообщения 
        /// </summary>
        /// <param name="message">сообщение вида ">>file-send>>path\\filename.ext"</param>
        /// <returns>Расширение файла</returns>
        private String getExtension(string message)
        {
            int fromIndex = message.IndexOf('.');

            return message.Substring(fromIndex+1);
        }

        /// <summary>
        /// Генерирует путь, по которому сохранится переданный файл
        /// </summary>
        /// <param name="ext">Расширение файла</param>
        /// <returns>Путь</returns>
        private String generatePath(string ext)
        {
            String dirName = DateTime.Now.ToString("yyyy-MM-dd");
            Directory.CreateDirectory(dirName);
            return dirName + "\\File" + fileNumber + "." + ext;
        }
        /// <summary>
        /// Сохраняет полученный от клиента файл на сервере
        /// </summary>
        /// <param name="extension">расширение файла</param>
        /// <returns>Результат операции</returns>
        public async Task<OperationResult> RecieveFileFromClient(string extension)
        {
            try
            {
                TcpClient client = null;
                if(NumberOfConnections < MAX_CONNECTIONS)
                {
                    client = serverListener.AcceptTcpClient();
                    Interlocked.Increment(ref NumberOfConnections);
                }
                if(client == null)
                {
                    return new OperationResult(Result.Fail, "Too many connections");
                }
                BinaryFormatter bf = new BinaryFormatter();               
                NetworkStream ns = client.GetStream();             
                File.WriteAllBytes(generatePath(extension), (byte[])bf.Deserialize(ns));
                ns.Close();
                client.Close();
                Interlocked.Decrement(ref NumberOfConnections);
                Interlocked.Increment(ref fileNumber);
                return new OperationResult(Result.OK, "Файл получен");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        public OperationResult SendMessageToClient(string message)
        {
            try
            {
                TcpClient client = serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
            return new OperationResult(Result.OK, "");
        }
    }
}