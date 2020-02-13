using Oracle.ManagedDataAccess.Client;
using System;
using System.Windows.Forms;

namespace VS_Prototype_v0
{
    public partial class MainForm : Form
    {
        // Строка подключения к Oracle
        const string oracleConnectionString = @"DATA SOURCE=localhost:1521/XE;PASSWORD=19230;PERSIST SECURITY INFO=True;USER ID=rfid";

        /// <summary>
        /// Функция, выполняющая выборку информации из таблиц БД и вывод выбранных данных
        /// </summary>
        /// <param name="table">Имя таблицы</param>
        /// <param name="conditions">Условия выбора в виде массива строк, каждая строка отвечает за условие на одно поле таблицы</param>
        private void selectFromTable(string table, string[] conditions)
        {
            // Создание соединения с БД с использованием строки подключения
            using (OracleConnection oracleConnection = new OracleConnection(oracleConnectionString))
            {
                try
                {
                    // Открытие соединения
                    oracleConnection.Open();
                    // Начало формирования запроса
                    string queryString = "SELECT * FROM " + table;
                    // Проверка, есть ли условия и на какие конкретно поля
                    bool noConditions = true;
                    for (int i = 0; i < conditions.Length && noConditions; ++i)
                    {
                        if (conditions[i] != "")
                        {
                            noConditions = false;
                        }
                    }

                    // При налачии условий
                    if (!(noConditions))
                    {
                        queryString += " WHERE";
                        bool andRequired = false;
                        switch (table)
                        {
                            // Добавление условий на поля таблиц в зависимости от таблиц и полей, на которые есть условия
                            case "users":
                                {
                                    if (conditions[0] != "")
                                    {
                                        queryString += " u_id = '" + conditions[0] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[1] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " u_f = '" + conditions[1] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[2] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " u_io = '" + conditions[2] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[3] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " u_post = '" + conditions[3] + "'";
                                    }

                                    break;
                                }

                            case "closets":
                                {
                                    if (conditions[0] != "")
                                    {
                                        queryString += " c_id = '" + conditions[0] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[1] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " c_building = '" + conditions[1] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[2] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " c_floor = '" + conditions[2] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[3] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " c_description = '" + conditions[3] + "'";
                                    }

                                    break;
                                }

                            case "material_values":
                                {
                                    if (conditions[0] != "")
                                    {
                                        queryString += " mv_id = '" + conditions[0] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[1] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " mv_description = '" + conditions[1] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[2] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " mv_closet = '" + conditions[2] + "'";
                                        andRequired = true;
                                    }

                                    break;
                                }

                            case "takenvalues_info":
                                {
                                    if (conditions[0] != "")
                                    {
                                        queryString += " u_id = '" + conditions[0] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[1] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " u_f = '" + conditions[1] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[2] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " u_io = '" + conditions[2] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[3] != "")
                                    {
                                        queryString += " u_post = '" + conditions[3] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[4] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " mv_id = '" + conditions[4] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[5] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " mv_description = '" + conditions[5] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[6] != "")
                                    {
                                        queryString += " mv_closet = '" + conditions[6] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[7] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " tv_take > '" + conditions[7] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[8] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " tv_take < '" + conditions[8] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[9] != "")
                                    {
                                        queryString += " tv_return > '" + conditions[9] + "'";
                                        andRequired = true;
                                    }

                                    if (conditions[10] != "")
                                    {
                                        if (andRequired) queryString += " AND";
                                        queryString += " tv_return < '" + conditions[10] + "'";
                                        andRequired = true;
                                    }

                                    break;
                                }
                        }
                    }
                    // Окончание формирования запроса
                    OracleCommand oracleCommand = new OracleCommand(queryString, oracleConnection);

                    try
                    {
                        // Выполнение запроса
                        OracleDataReader reader = oracleCommand.ExecuteReader();

                        switch (table)
                        {
                            // Изменение отображения информации в таблице в зависимости от введенных условий
                            case "users":
                                {
                                    while (usersView.Rows.Count != 0)
                                    {
                                        Invoke(new Action(() => usersView.Rows.Remove(usersView.Rows[usersView.Rows.Count - 1])));
                                    }

                                    break;
                                }

                            case "closets":
                                {
                                    while (closetsView.Rows.Count != 0)
                                    {
                                        Invoke(new Action(() => closetsView.Rows.Remove(closetsView.Rows[closetsView.Rows.Count - 1])));
                                    }

                                    break;
                                }

                            case "material_values":
                                {
                                    while (materialValuesView.Rows.Count != 0)
                                    {
                                        Invoke(new Action(() => materialValuesView.Rows.Remove(materialValuesView.Rows[materialValuesView.Rows.Count - 1])));
                                    }

                                    break;
                                }

                            case "takenvalues_info":
                                {
                                    while (takenValuesInfoView.Rows.Count != 0)
                                    {
                                        Invoke(new Action(() => takenValuesInfoView.Rows.Remove(takenValuesInfoView.Rows[takenValuesInfoView.Rows.Count - 1])));
                                    }

                                    break;
                                }
                        }

                        if (reader.HasRows)
                        {
                            // Заполнение нужной таблицы
                            while (reader.Read())
                            {
                                DataGridViewRow row = new DataGridViewRow();

                                switch (table)
                                {
                                    case "users":
                                        {
                                            row.CreateCells(usersView);

                                            row.Cells[0].Value = reader.GetValue(0);
                                            row.Cells[1].Value = reader.GetValue(1);
                                            row.Cells[2].Value = reader.GetValue(2);
                                            row.Cells[3].Value = reader.GetValue(3);

                                            Invoke(new Action(() => usersView.Rows.Add(row)));

                                            break;
                                        }

                                    case "closets":
                                        {
                                            row.CreateCells(closetsView);

                                            row.Cells[0].Value = reader.GetValue(0);
                                            row.Cells[1].Value = reader.GetValue(1);
                                            row.Cells[2].Value = reader.GetValue(2);
                                            row.Cells[3].Value = reader.GetValue(3);

                                            Invoke(new Action(() => closetsView.Rows.Add(row)));

                                            break;
                                        }

                                    case "material_values":
                                        {
                                            row.CreateCells(materialValuesView);

                                            row.Cells[0].Value = reader.GetValue(0);
                                            row.Cells[1].Value = reader.GetValue(1);
                                            row.Cells[2].Value = reader.GetValue(2);

                                            Invoke(new Action(() => materialValuesView.Rows.Add(row)));

                                            break;
                                        }

                                    case "takenvalues_info":
                                        {
                                            row.CreateCells(takenValuesInfoView);

                                            row.Cells[0].Value = reader.GetValue(0);
                                            row.Cells[1].Value = reader.GetValue(1);
                                            row.Cells[2].Value = reader.GetValue(2);
                                            row.Cells[3].Value = reader.GetValue(3);
                                            row.Cells[4].Value = reader.GetValue(4);
                                            row.Cells[5].Value = reader.GetValue(5);
                                            row.Cells[6].Value = reader.GetValue(6);
                                            row.Cells[7].Value = reader.GetValue(7);
                                            row.Cells[8].Value = reader.GetValue(8);

                                            Invoke(new Action(() => takenValuesInfoView.Rows.Add(row)));

                                            break;
                                        }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        // При ошибке выполнения запроса
                        toDebugTextBox("Ошибка при выполнении запроса\r\n" + queryString + "\r\n" + exception.Message + "\r\n");
                    }
                }
                catch (Exception connectException)
                {
                    // При ошибке подключения к БД
                    toDebugTextBox("Ошибка при подключении к БД\r\n");
                }
            }
        }

        /// <summary>
        /// Функция, выполняющая добавление информации в таблицу
        /// </summary>
        /// <param name="table">Имя таблицы</param>
        /// <param name="values">Данные для добавления в виде массива строк, каждая строка отвечает за одно поле таблицы</param>
        private void insertIntoTable(string table, string[] values)
        {
            // Создание подключения с БД с использованием строки подключения
            using (OracleConnection oracleConnection = new OracleConnection(oracleConnectionString))
            {
                try
                {
                    // Открытие соединения
                    oracleConnection.Open();
                    // Формирование запроса
                    string queryString = "INSERT INTO " + table + " VALUES (";
                    for (int i = 0; i < values.Length; ++i)
                    {
                        if (i != 0)
                        {
                            queryString += ", ";
                        }
                        queryString += "'" + values[i] + "'";
                    }
                    queryString += ")";

                    OracleCommand oracleCommand = new OracleCommand(queryString, oracleConnection);

                    try
                    {
                        // Выполнение запроса
                        oracleCommand.ExecuteNonQuery();
                    }
                    catch (Exception exception)
                    {
                        // При ошибке выполнения запроса
                        toDebugTextBox("Ошибка при выполнении запроса\r\n" + queryString + "\r\n" + exception.Message + "\r\n");
                    }
                }
                catch (Exception connectException)
                {
                    // При ошибке подключения к БД
                    toDebugTextBox("Ошибка при подключении к БД\r\n");
                }
            }
        }

        /// <summary>
        /// Функция, выполняющая изменение информации в таблице
        /// </summary>
        /// <param name="table">Имя таблицы</param>
        /// <param name="oldValues">Старые значения полей в виде массива строк</param>
        /// <param name="newValues">Новые значения полей в виде массива строк</param>
        private void updateTable(string table, string[] oldValues, string[] newValues)
        {
            // Проверка на наличие изменений
            bool noChanges = true;
            bool[] changes = new bool[oldValues.Length];

            for (int i = 0; i < oldValues.Length; ++i)
            {
                if (oldValues[i] == newValues[i])
                {
                    changes[i] = false;
                }
                else
                {
                    changes[i] = true;
                    noChanges = false;
                }
            }

            if (!(noChanges))
            {
                // Создание соединения с БД с использованием строки подключения
                using (OracleConnection oracleConnection = new OracleConnection(oracleConnectionString))
                {
                    try
                    {
                        // Открытие соединения
                        oracleConnection.Open();
                        OracleCommand cmd;

                        switch (table)
                        {
                            // Формирование запроса в зависимости от таблицы и изменившихся полей
                            case "users":
                                {
                                    if (changes[0])
                                    {
                                        insertIntoTable("users", newValues);
                                        cmd = new OracleCommand("UPDATE taken_values SET tv_user = '" + newValues[0] + "' WHERE tv_user = '" + oldValues[0] + "'", oracleConnection);
                                        cmd.ExecuteNonQuery();
                                        cmd = new OracleCommand("DELETE FROM users WHERE u_id = '" + oldValues[0] + "'", oracleConnection);
                                        cmd.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        cmd = new OracleCommand("UPDATE users set u_f = '" + newValues[1] + "', u_io = '" + newValues[2] + "', u_post = '" + newValues[3] + "' WHERE u_id = '" + oldValues[0] + "'", oracleConnection);
                                        cmd.ExecuteNonQuery();
                                    }

                                    break;
                                }

                            case "closets":
                                {
                                    if (changes[0])
                                    {
                                        insertIntoTable("closets", newValues);
                                        cmd = new OracleCommand("UPDATE material_values SET mv_closet = '" + newValues[0] + "' WHERE mv_closet = '" + oldValues[0] + "'", oracleConnection);
                                        cmd.ExecuteNonQuery();
                                        cmd = new OracleCommand("DELETE FROM closets WHERE c_id = '" + oldValues[0] + "'", oracleConnection);
                                        cmd.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        cmd = new OracleCommand("UPDATE closets SET c_building = '" + newValues[1] + "', c_floor = '" + newValues[2] + "', c_description = '" + newValues[3] + "' WHERE c_id = '" + oldValues[0] + "'", oracleConnection);
                                        cmd.ExecuteNonQuery();
                                    }

                                    break;
                                }

                            case "material_values":
                                {
                                    if (changes[0])
                                    {
                                        insertIntoTable("material_values", newValues);
                                        cmd = new OracleCommand("UPDATE taken_values SET tv_value = '" + newValues[0] + "' WHERE tv_value = '" + oldValues[0] + "'", oracleConnection);
                                        cmd.ExecuteNonQuery();
                                        cmd = new OracleCommand("DELETE FROM material_values WHERE mv_id = '" + oldValues[0] + "'", oracleConnection);
                                        cmd.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        cmd = new OracleCommand("UPDATE material_values SET mv_description = '" + newValues[1] + "', mv_closet = '" + newValues[2] + "' WHERE mv_id = '" + oldValues[0] + "'", oracleConnection);
                                        cmd.ExecuteNonQuery();
                                    }

                                    break;
                                }

                            case "taken_values":
                                {
                                    cmd = new OracleCommand("UPDATE taken_values SET tv_return = sysdate WHERE tv_return IS NULL and tv_user = '" + oldValues[0] + "' and tv_value = '" + oldValues[1] + "'", oracleConnection);
                                    cmd.ExecuteNonQuery();
                                    break;
                                }
                        }
                    }
                    catch (Exception connectException)
                    {
                        // При ошибке выполнения
                        toDebugTextBox("Ошибка при подключении к БД или выполнении запроса типа UPDATE\r\n");
                    }
                }
            }
        }

        /// <summary>
        /// Функция, выполняющая удаление строк из таблиц
        /// </summary>
        /// <param name="table">Имя таблицы</param>
        /// <param name="rows">Коллекция строк, которые нужно удалить</param>
        private void deleteFromTable(string table, DataGridViewSelectedRowCollection rows)
        {
            // Создание подключения к БД с использованием строки подключения
            using (OracleConnection oracleConnection = new OracleConnection(oracleConnectionString))
            {
                try
                {
                    // Открытие соединения с БД
                    oracleConnection.Open();
                    OracleCommand cmd;
                    // Последовательное удаление указанных строк указанной таблицы
                    foreach (DataGridViewRow row in rows)
                    {
                        switch (table)
                        {
                            case "users":
                                {
                                    cmd = new OracleCommand("DELETE FROM taken_values WHERE tv_user = '" + row.Cells[0].Value.ToString() + "'");
                                    cmd.ExecuteNonQuery();
                                    cmd = new OracleCommand("DELETE FROM users WHERE u_id = '" + row.Cells[0].Value.ToString() + "'");
                                    cmd.ExecuteNonQuery();
                                    break;
                                }

                            case "closets":
                                {
                                    cmd = new OracleCommand("DELETE FROM material_values WHERE mv_closet = '" + row.Cells[0].Value.ToString() + "'");
                                    cmd.ExecuteNonQuery();
                                    cmd = new OracleCommand("DELETE FROM closets WHERE c_id = '" + row.Cells[0].Value.ToString() + "'");
                                    cmd.ExecuteNonQuery();
                                    break;
                                }

                            case "material_values":
                                {
                                    cmd = new OracleCommand("DELETE FROM taken_values WHERE tv_value = '" + row.Cells[0].Value.ToString() + "'");
                                    cmd.ExecuteNonQuery();
                                    cmd = new OracleCommand("DELETE FROM material_values WHERE mv_id = '" + row.Cells[0].Value.ToString() + "'");
                                    cmd.ExecuteNonQuery();
                                    break;
                                }

                            case "taken_values":
                                {
                                    if (row.Cells[8].Value.ToString() != "")
                                    {
                                        string queryString = "DELETE FROM taken_values WHERE" +
                                                             " tv_user = '" + row.Cells[0].Value.ToString() + "'" +
                                                             " and tv_value = '" + row.Cells[4].Value.ToString() + "'" +
                                                             " and to_char(tv_take) = '" + row.Cells[7].Value.ToString() + "'" +
                                                             " and to_char(tv_return) = '" + row.Cells[8].Value.ToString() + "'";
                                        cmd = new OracleCommand(queryString, oracleConnection);
                                        cmd.ExecuteNonQuery();
                                    }
                                    break;
                                }
                        }
                    }
                }
                catch (Exception connectException)
                {
                    // При ошибке
                    toDebugTextBox("Ошибка при подключении к БД или выполнении запроса типа DELETE\r\n");
                }
            }
        }
    }
}
