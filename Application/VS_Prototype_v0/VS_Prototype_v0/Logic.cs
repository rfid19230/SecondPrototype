using Oracle.ManagedDataAccess.Client;
using System;
using System.Windows.Forms;

namespace VS_Prototype_v0
{
    public partial class MainForm : Form
    {
        // Список используемых команд
        // '0' : Команда подтверждения установки соединения Arduino и компьютера
        // '1' : Команда разрыва соединения между Arduino и компьютером
        // '2' : Команда, получаемая при прикладывании пропуска и свидетельствующая о том, что доступ разрешен
        // '3' : Команда, получаемая при прикладывании пропуска и свидетельствующая о том, что доступ запрещен
        // '4' : Команда, свидетельствующая об окончании работы пользователя с системой
        // '5' : Команда, получаемая при прикладывании ценности и свидетельствующая о ее взятии
        // '6' : Команда, получаемая при прикладывании ценности и свидетельствующая о ее возврате
        // '7' : Команда, получаемая при прикладывании ценности, данные о которой отсутствуют в базе
        const string COMMAND_CONNECT = "0";
        const string COMMAND_DISCONNECT = "1";
        const string COMMAND_VALID_PASS = "2";
        const string COMMAND_INVALID_PASS = "3";
        const string COMMAND_END_SESSION = "4";
        const string COMMAND_VALUE_TAKEN = "5";
        const string COMMAND_VALUE_RETURNED = "6";
        const string COMMAND_INVALID_VALUE = "7";
        // Переменная, используемая для хранения uid пропуска сотрудника, открывшего систему
        public static string currentUid = "";

        /// <summary>
        /// Функция, выполняющая проверку наличия прав доступа у приложившего пропуск
        /// </summary>
        /// <param name="passUid">Uid пропуска</param>
        /// <returns>Возвращает true при наличии прав доступа, в противном случае false</returns>
        private bool isValidPass(string passUid)
        {
            bool result = false;
            // Создание подключения к БД с использованием строки подключения
            using (OracleConnection oracleConnection = new OracleConnection(oracleConnectionString))
            {
                try
                {
                    // Открытие соединения
                    oracleConnection.Open();
                    // Формирование запроса в БД
                    string queryString = "SELECT * FROM users WHERE u_id = " + "'" + passUid + "'";
                    OracleCommand oracleCommand = new OracleCommand(queryString, oracleConnection);

                    try
                    {
                        // Выполнение запроса
                        OracleDataReader reader = oracleCommand.ExecuteReader();
                        if (reader.HasRows)
                        {
                            // Права доступа есть
                            result = true;
                        }
                    }
                    catch (Exception readException)
                    {
                        // При ошибке выполнения запроса
                        toDebugTextBox("Ошибка при выполнении запроса\r\n" + queryString + "\r\n");
                    }
                }
                catch (Exception connectException)
                {
                    // При ошибке подключения к БД
                    toDebugTextBox("Ошибка при подключении к БД\r\n");
                }
            }

            return result;
        }

        /// <summary>
        /// Функция, выполняющая проверку на наличии информации о ценности в БД
        /// </summary>
        /// <param name="valueUid">Uid ценности</param>
        /// <returns>Возвращает true при наличии информации о ценности в БД, в противном случае false</returns>
        private bool isValidValue(string valueUid)
        {
            bool result = false;
            // Создание подключения к БД с использованием строки подключения
            using (OracleConnection oracleConnection = new OracleConnection(oracleConnectionString))
            {
                try
                {
                    // Открытие соединения с БД
                    oracleConnection.Open();
                    // Формирование запроса в БД
                    string queryString = "SELECT * FROM material_values WHERE mv_id = " + "'" + valueUid + "'";
                    OracleCommand oracleCommand = new OracleCommand(queryString, oracleConnection);

                    try
                    {
                        // Выполнение запроса
                        OracleDataReader reader = oracleCommand.ExecuteReader();
                        if (reader.HasRows)
                        {
                            // При наличии информации о ценности
                            result = true;
                        }
                    }
                    catch (Exception readException)
                    {
                        // При ошибке выполнения запроса
                        toDebugTextBox("Ошибка при выполнении запроса\r\n" + queryString + "\r\n");
                    }
                }
                catch (Exception connectException)
                {
                    // ПРи ошибке подключения к БД
                    toDebugTextBox("Ошибка при подключении к БД\r\n");
                }
            }

            return result;
        }

        /// <summary>
        /// Функция, определяющая тип действия с ценностью
        /// </summary>
        /// <param name="passUid">Uid пропуска</param>
        /// <param name="valueUid">Uid ценности</param>
        /// <returns>Возвращает строку "taking" при взятии, строку "returning" при возвртае, "error" при ошибке</returns>
        private string actionWithValue(string passUid, string valueUid)
        {
            string result = "";
            // Создание подключения к БД с помошью строки подключения
            using (OracleConnection oracleConnection = new OracleConnection(oracleConnectionString))
            {
                try
                {
                    // Открытие соединения с БД
                    oracleConnection.Open();
                    // Формирование запроса в БД
                    string queryString = "SELECT * FROM taken_values WHERE tv_return IS NULL and tv_value = " + "'" + valueUid + "'" + " and tv_user = " + "'" + passUid + "'";
                    OracleCommand oracleCommand = new OracleCommand(queryString, oracleConnection);

                    try
                    {
                        // Выполнение запроса
                        OracleDataReader reader = oracleCommand.ExecuteReader();
                        if (reader.HasRows)
                        {
                            // Взятие
                            result = "returning";
                        }
                        else
                        {
                            // Возврат
                            result = "taking";
                        }
                    }
                    catch (Exception readException)
                    {
                        // При ошибке выполения вопроса
                        toDebugTextBox("Ошибка при выполнении запроса\r\n" + queryString + "\r\n");
                        result = "error";
                    }
                }
                catch (Exception connectException)
                {
                    // При ошибке подключения
                    toDebugTextBox("Ошибка при подключении к БД\r\n");
                    result = "error";
                }
            }
            return result;
        }

        /// <summary>
        /// Функция вносит изменения в БД при взятии и возврате ценностей
        /// </summary>
        /// <param name="passUid">Uid пропуска</param>
        /// <param name="valueUid">Uid ценности</param>
        /// <param name="action">Действие</param>
        private void changeTakenValues(string passUid, string valueUid, string action)
        {
            // Создание соединения с БД с использованием строки подключения
            using (OracleConnection oracleConnection = new OracleConnection(oracleConnectionString))
            {
                try
                {
                    // Открытие соединения
                    oracleConnection.Open();
                    // Формирование запроса в зависимости от действия
                    string queryString = "";
                    if (action == "taking")
                    {
                        // При взятии
                        queryString = "INSERT INTO taken_values (tv_user, tv_value, tv_take, tv_return) VALUES (:1, :2, sysdate, NULL)";
                    }
                    if (action == "returning")
                    {
                        // При возврате
                        queryString = "UPDATE taken_values SET tv_return = sysdate WHERE tv_return IS NULL and tv_user = :1 and tv_value = :2";
                    }
                    OracleCommand oracleCommand = new OracleCommand(queryString, oracleConnection);
                    oracleCommand.Parameters.Add(":1", OracleDbType.Varchar2, 20).Value = passUid;
                    oracleCommand.Parameters.Add(":2", OracleDbType.Varchar2, 20).Value = valueUid;

                    try
                    {
                        // Выполнение запроса
                        oracleCommand.ExecuteNonQuery();
                        toDebugTextBox("Выполнен запрос в БД. Обновлена таблица taken_values\r\n");
                    }
                    catch (Exception readException)
                    {
                        // При ошибке выполнения запроса
                        toDebugTextBox("Ошибка при выполнении запроса\r\n" + queryString + "\r\n");
                    }
                }
                catch (Exception connectException)
                {
                    // При ошибке подключения
                    toDebugTextBox("Ошибка при подключении к БД\r\n");
                }
            }
        }

        /// <summary>
        /// Функция, реализующая логику работы системы. По полученному uid определяет дальнейшие действия.
        /// </summary>
        /// <param name="uid">Uid считанной метки</param>
        /// <returns>Следующая команда</returns>
        string onUidReceived(string uid)
        {
            string res = "";
            // Вывод информации о работе программы
            string uidInfo = "Считана RFID-метка: uid = " + uid + "\r\n";
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
                    res = COMMAND_VALID_PASS;
                    // Вывод информации о работе программы
                    string info = "Выполнен запрос в БД. Доступ разрешен, замок открыт\r\n";
                    toDebugTextBox(info);
                }
                else
                {
                    // Если доступ запрещен
                    res = COMMAND_INVALID_PASS;
                    // Вывод информации о работе программы
                    string info = "Выполнен запрос в БД. Доступ не разрешен, замок не открыт\r\n";
                    toDebugTextBox(info);
                }
            }
            else
            {
                // Идет процесс взятия и возврата вещей
                // Проверка на завершение процесса взятия и возврата вещей
                if ("FFFFFFFF" == uid)
                {
                    // Получен признак завершения процесса взятия вещей
                    // Сброс текущего uid
                    currentUid = "";
                    // Отправка команды завершения сеанса взятия и возврата вещей
                    res = COMMAND_END_SESSION;
                    // Вывод информации о работе программы
                    string info = "Конец взятия и возврата вещей\r\n";
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
                            res = COMMAND_VALUE_TAKEN;
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
                                res = COMMAND_VALUE_RETURNED;
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
                        res = COMMAND_INVALID_VALUE;
                        // Вывод информации о работе программы
                        string info = "Выполнен запрос в БД. Ценность не найдена в списке\r\n";
                        toDebugTextBox(info);
                    }
                }
            }
            return res;
        }
    }
}
