using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace VS_Prototype_v0
{
    public partial class MainForm : Form
    {
        // Порт и ip-адрес сервера
        const int srv_port = 19230;
        const string srv_ip = "172.18.160.26";

        public TcpListener srv = null;
        public static bool srv_stop;

        /// <summary>
        /// Действия, выполняемые при нажатии на кнопку "Запустить сервер"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startServerButton_Click(object sender, EventArgs e)
        {
            // Создание нового потока, играющего роль сервера
            Thread s = new Thread(() =>
            {
                // Описание действий, выполняемых в серверном потоке
                // Настройка некоторых элементов интерфейса
                Invoke(new Action(() =>
                {
                    startServerButton.Enabled = false;
                    stopServerButton.Enabled = true;
                    serverStateLabel.BackColor = Color.Lime;
                }));
                toDebugTextBox("Сервер запущен\r\n");
                // Установка флага состояния сервера
                srv_stop = false;
                try
                {
                    // Запуск сервера
                    srv = new TcpListener(IPAddress.Parse(srv_ip), srv_port);
                    srv.Start();

                    // Цикл ожидания подключений с проверкой флага состояния сервера
                    while (!srv_stop)
                    {
                        // Непосредственно ожидание подключения
                        while (!srv_stop && !srv.Pending())
                        {
                            Thread.Sleep(100);
                        }
                        if (!srv_stop)
                        {
                            // При входящем подключении
                            //                            Thread c = new Thread(() =>
                            //                            {
                            NetworkStream stream = null;
                            TcpClient client = null;
                            try
                            {
                                // Установка соединения с клиентом
                                client = srv.AcceptTcpClient();
                                Thread.Sleep(100);
                                toDebugTextBox("Зафиксировано входящее подключение\r\n");

                                // Считывание полученного сообщения
                                stream = client.GetStream();
                                byte[] data = new byte[64]; // буфер для получаемых данных

                                // получаем uid
                                StringBuilder builder = new StringBuilder();
                                int bytes = 0;
                                do
                                {
                                    bytes = stream.Read(data, 0, data.Length);
                                    builder.Append(Encoding.ASCII.GetString(data, 0, bytes));
                                }
                                while (stream.DataAvailable);

                                // Обработка полученного uid
                                string uid = builder.ToString();
                                toDebugTextBox("Получен считанный uid = " + uid + "\r\n");
                                if (uid != "00000000")
                                {
                                    string command = onUidReceived(uid);
                                    toDebugTextBox("Код сгенерированной команды = " + command + "\r\n");

                                    // отправляем обратно команду
                                    data = Encoding.ASCII.GetBytes(command);
                                    stream.Write(data, 0, data.Length);
                                }
                                else
                                {
                                    toDebugTextBox("Получено сообщение проверки соединения\r\n");
                                }
                            }
                            catch (Exception ex)
                            {
                                // При ошибке
                                toDebugTextBox(ex.Message + "\r\nОшибка в клиентском потоке\r\n");
                            }
                            finally
                            {
                                // При завершении работы
                                if (stream != null)
                                    stream.Close();
                                if (client != null)
                                {
                                    client.Close();
                                    toDebugTextBox("Завершено обслуживание входящего подключения\r\n");
                                }
                            }
                            //                            });
                            //                            c.IsBackground = true;
                            //                            c.Start();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // При ошибке
                    toDebugTextBox(ex.Message + "\r\nОшибка сервера\r\n");
                }
                finally
                {
                    // При ошибке завершения работы
                    if (srv != null)
                    {
                        srv.Stop();
                    }
                    Invoke(new Action(() =>
                    {
                        startServerButton.Enabled = true;
                        stopServerButton.Enabled = false;
                        serverStateLabel.BackColor = Color.Red;
                    }));
                    toDebugTextBox("Сервер остановлен\r\n");
                }
            });
            s.IsBackground = true;
            s.Start();
        }

        /// <summary>
        /// Действия, выполняемые при нажатии на кнопку "Остановить сервер"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopServerButton_Click(object sender, EventArgs e)
        {
            // Установка флага остановки сервера
            srv_stop = true;
        }
    }
}
