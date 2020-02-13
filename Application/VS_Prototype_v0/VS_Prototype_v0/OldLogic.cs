using System;
using System.Drawing;
using System.Windows.Forms;

namespace VS_Prototype_v0
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Действия, выполняемые при нажатии кнопки "Подключение"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectButton_Click(object sender, EventArgs e)
        {
            // Выбор номера используемого порта
            serialPort.PortName = chosenPortComboBox.Text;
            // Открытие порта с обработкой исключений, возникающих при невозможности открытия
            try
            {
                serialPort.Open();
            }
            catch (Exception exception)
            {
                string info = "Подключение к порту " + serialPort.PortName + ": Не удалось\r\n";
                toDebugTextBox(info);
            }
            // При успешном открытии 
            if (serialPort.IsOpen)
            {
                // Отправка команды подключния на Arduino
                serialPort.Write(COMMAND_DISCONNECT);
                serialPort.Write(COMMAND_CONNECT);
                // Вывод информации о работе программы
                string info = "Подключение к порту " + serialPort.PortName + ": Успешно\r\n";
                toDebugTextBox(info);
                connectionStateLabel.BackColor = Color.Lime;
                connectionStateLabel.Text = "Подключено";
                connectButton.Enabled = false;
                disconnectButton.Enabled = true;
                chosenPortComboBox.Enabled = false;
            }
        }

        /// <summary>
        /// Действия, выполняемые при нажатии кнопки "Отключение"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void disconnectButton_Click(object sender, EventArgs e)
        {
            // Отправка на Arduino команды разрыва соединения
            serialPort.Write(COMMAND_DISCONNECT);
            // Закрытие порта
            try
            {
                serialPort.Close();
            }
            catch (Exception exception)
            {

            }
            // Вывод информации о работе программы
            string info = "Отключение порта " + serialPort.PortName + ": Успешно\r\n";
            toDebugTextBox(info);
            connectionStateLabel.BackColor = System.Drawing.Color.Red;
            connectionStateLabel.Text = "Отключено";
            connectButton.Enabled = true;
            disconnectButton.Enabled = false;
            chosenPortComboBox.Enabled = true;

            currentUid = "";
        }

        /// <summary>
        /// Действия, выполняемые при получении данных от Arduino. 
        /// Основная функция программы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Считывание размера uid прочитанной метки
            int uidSize = Convert.ToInt32(serialPort.ReadLine());
            // Считывание uid метки
            string uid = "";
            for (int i = 0; i < uidSize; ++i)
            {
                // Обработка очередного байта uid
                string rawUidByte = serialPort.ReadLine();
                string uidByte = rawUidByte.Substring(0, rawUidByte.Length - 1);
                if (uidByte.Length == 1)
                {
                    uidByte = "0" + uidByte;
                }
                uid += uidByte;
            }

            // Вывод информации о работе программы
            string uidInfo = "Считана RFID-метка: uidSize = " + uidSize.ToString() + ", uid = " + uid + "\r\n";
            toDebugTextBox(uidInfo);

            // Анализ прочитанной метки и выбор дальнейших действий
            if (currentUid == "")
            {
                // Считанный uid рассматривается как пропуск
                string passInfo = "Рассматривается как пропуск\r\n";
                toDebugTextBox(passInfo);

                // Обращение в БД для проверки прав доступа
                if (isValidPass(uid))
                {
                    // Если доступ разрешен - отправка команды на открытие замка, запоминание текущего id пропуска
                    currentUid = uid;
                    serialPort.Write(COMMAND_VALID_PASS);
                    // Вывод информации о работе программы
                    string info = "Выполнен запрос в БД. Доступ разрешен, замок открыт\r\n";
                    toDebugTextBox(info);
                }
                else
                {
                    // Если доступ запрещен
                    serialPort.Write(COMMAND_INVALID_PASS);
                    // Вывод информации о работе программы
                    string info = "Выполнен запрос в БД. Доступ не разрешен, замок не открыт\r\n";
                    toDebugTextBox(info);
                }
            }
            else
            {
                // Идет процесс взятия и возврата вещей
                // Проверка на завершение процесса взятия и возврата вещей
                if (currentUid == uid)
                {
                    // Повторное прикладывание пропуска - признак завершения процесса взятия вещей
                    // Сброс текущего uid
                    currentUid = "";
                    // Отправка команды завершения сеанса взятия и возврата вещей
                    serialPort.Write(COMMAND_END_SESSION);
                    // Вывод информации о работе программы
                    string info = "Повторное прикладывание пропуска. Конец взятия и возврата вещей\r\n";
                    toDebugTextBox(info);
                }
                else
                {
                    // Считанный uid рассматривается как ценность
                    string valueInfo = "Рассматривается как ценность\r\n";
                    toDebugTextBox(valueInfo);

                    // Проверка ценности
                    if (isValidValue(uid))
                    {
                        // Информация о ценности есть в БД
                        string validValueInfo = "Выполнен запрос в БД. Ценность найдена в списке\r\n";
                        toDebugTextBox(validValueInfo);

                        // Проверка, взяте или возврат
                        string action = actionWithValue(currentUid, uid);
                        //
                        if (action == "taking")
                        {
                            // Если возврат, отправка соответствующей команды на Arduino
                            serialPort.Write(COMMAND_VALUE_TAKEN);
                            // Вывод информации о работе программы
                            string info = "Выполнен запрос в БД. Действие - взятие\r\n";
                            toDebugTextBox(info);
                            // Внесение изменений в БД
                            changeTakenValues(currentUid, uid, action);
                            // Отображение изменений
                            if (autoRefreshTakenValuesInfoCheckBox.Checked)
                            {
                                selectFromTable("takenvalues_info", getTakenValuesInfoFields());
                            }
                        }
                        else
                        {
                            if (action == "returning")
                            {
                                // Если озврат, отправка соответствующей команды на Arduino
                                serialPort.Write(COMMAND_VALUE_RETURNED);
                                // Вывод информации о работе программы
                                string info = "Выполнен запрос в БД. Действие - возврат\r\n";
                                toDebugTextBox(info);
                                // Внесение изменений в БД
                                changeTakenValues(currentUid, uid, action);
                                // Отображение изменений
                                if (autoRefreshTakenValuesInfoCheckBox.Checked)
                                {
                                    Invoke(new Action(() => selectFromTable("takenvalues_info", getTakenValuesInfoFields())));
                                }
                            }
                        }
                    }
                    else
                    {
                        // Информация о ценности отсутствует в БД
                        serialPort.Write(COMMAND_INVALID_VALUE);
                        // Вывод информации о работе программы
                        string info = "Выполнен запрос в БД. Ценность не найдена в списке\r\n";
                        toDebugTextBox(info);
                    }
                }
            }
        }

    }
}
