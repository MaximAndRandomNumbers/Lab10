using System;
using System.Windows.Forms;
using SomeProject.Library.Client;
using SomeProject.Library;

namespace SomeProject.TcpClient
{
    public partial class ClientMainWindow : Form
    {
        public ClientMainWindow()
        {
            InitializeComponent();
        }

        private void OnMsgBtnClick(object sender, EventArgs e)
        {
            Client client = new Client();
            Result res = client.SendMessageToServer(textBox.Text).Result;

            if(res == Result.OK)
            {
                textBox.Text = client.ReceiveMessageFromServer().Message;
                labelRes.Text = "Message was sent succefully!";
            }
            else
            {
                labelRes.Text = "Cannot send the message to the server.";
            }
            timer.Interval = 2000;
            timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            labelRes.Text = "";
            timer.Stop();
        }

        /// <summary>
        /// Обработка нажатия кнопки отправки файла
        /// Отправляет выбранный файл на сервер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendFileBtn_Click(object sender, EventArgs e)
        {
            Client client = new Client();
            String fileName;
            if(fileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = fileDialog.FileName;
            } else
            {
                return;
            }
            Result res = client.SendFileToServer(fileName).Result;
            if (res == Result.OK)
            {
                textBox.Text = client.ReceiveMessageFromServer().Message;
                labelRes.Text = "File was sent succefully!";
            }
            else
            {
                labelRes.Text = "Cannot send the file to the server.";
            }
            timer.Interval = 2000;
            timer.Start();
        }
    }
}
