// Необходимые зависимости
using Oracle.ManagedDataAccess.Client;
using System;
using System.Windows.Forms;

namespace VS_Prototype_v0
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Конструктор главной формы
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // заполнение списка доступных COM портов
            string[] serialPorts = new string[255];
            for (int i = 1; i < 256; ++i)
            {
                serialPorts[i - 1] = "COM" + i.ToString();
            }
            chosenPortComboBox.Items.AddRange(serialPorts);
            int defaultChosenPort = 3;
            chosenPortComboBox.SelectedIndex = defaultChosenPort - 1;
            // Вывод информации о работе программы
            string info = "Приложение запущено: Успешно\r\n";
            toDebugTextBox(info);
        }

        /// <summary>
        /// Действия, выполняемые при закрытии формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Если СОМ порт открыт
            if (serialPort.IsOpen)
            {
                // Отправка на Arduino комнады разрыва соединения
                serialPort.Write(COMMAND_DISCONNECT);
                currentUid = "";
                // Закрытие порта
                serialPort.Close();
                // Вывод информации о работе программы
                string info = "Отключение порта " + serialPort.PortName + ": Успешно\r\n";
                toDebugTextBox(info);
            }

            string text = "Приложение закрыто: Успешно";
            toDebugTextBox(text);
        }

        /// <summary>
        /// Функция, выполняющая вывод информации о работе програамы в специальное окно
        /// </summary>
        /// <param name="info">Строка с информацией, которую необходимо вывести</param>
        void toDebugTextBox(string info)
        {
            // Проверка, из какого потока вызывается функция
            if (InvokeRequired)
            {
                // Если не из главного
                Invoke(new Action(() => debugTextBox.Text += info));

                if (debugTextBox.Text.Length >= 0.95 * debugTextBox.MaxLength)
                {
                    Invoke(new Action(() => debugTextBox.Clear()));
                    string text = "debugTextBox был очищен во избежание переполнения\r\n";
                    Invoke(new Action(() => debugTextBox.Text += text));
                }
            }
            else
            {
                // Если из главного
                debugTextBox.Text += info;

                if (debugTextBox.Text.Length >= 0.95 * debugTextBox.MaxLength)
                {
                    debugTextBox.Clear();
                    string text = "debugTextBox был очищен во избежание переполнения\r\n";
                    debugTextBox.Text += text;
                }
            }
        }

        /// <summary>
        /// Действия, выполняемые при загрузке главной формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // Вывод содержимого всех таблиц на всех вкладках
            string[] usersConditions = { "", "", "", "" };
            selectFromTable("users", usersConditions);

            string[] closetsConditions = { "", "", "", "" };
            selectFromTable("closets", closetsConditions);

            string[] materialValuesConditions = { "", "", "" };
            selectFromTable("material_values", materialValuesConditions);

            string[] takenValuesInfoViewConditions = { "", "", "", "", "", "", "", "", "", "", "" };
            selectFromTable("takenvalues_info", takenValuesInfoViewConditions);
        }

        /// <summary>
        /// Функция проверяет возможность подключения к БД
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkDbConnectionButton_Click(object sender, EventArgs e)
        {
            // Создание подключения к БД с помощью строки подключения
            using (OracleConnection oracleConnection = new OracleConnection(oracleConnectionString))
            {
                try
                {
                    // Открытие соединения
                    oracleConnection.Open();
                    toDebugTextBox("БД на связи\r\n");
                }
                catch (Exception connectException)
                {
                    // При ошибке открытия соединения
                    toDebugTextBox("Ошибка при подключении к БД\r\n");
                }
            }
        }

        /// <summary>
        /// Действия при нажатии на кнопку ручной отправки запросов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void executeQueryButton_Click(object sender, EventArgs e)
        {
            // Создание соединения с БД с использованием строки подключения
            using (OracleConnection oracleConnection = new OracleConnection(oracleConnectionString))
            {
                try
                {
                    // Открытие соединения
                    oracleConnection.Open();
                    // Считывание введенного запроса
                    string queryString = queryTextBox.Text;
                    OracleCommand oracleCommand = new OracleCommand(queryString, oracleConnection);

                    try
                    {
                        // Выполнение запроса
                        oracleCommand.ExecuteNonQuery();
                        toDebugTextBox("Запрос успешно выполнен\r\n");
                    }
                    catch (Exception exception)
                    {
                        // При ошибке выполнения запроса
                        toDebugTextBox("Ошибка при выполнении запроса\r\n" + queryString + "\r\n" + exception.Message + "\r\n");
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
        /// Действия при нажатии на кнопку очистки окна вывода информации о работе программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearDebugTextBoxButton_Click(object sender, EventArgs e)
        {
            debugTextBox.Clear();
        }

        /// <summary>
        /// Действия при нажатии на кнопку очистки окна ручного ввода запросов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearQueryTextBox_Click(object sender, EventArgs e)
        {
            queryTextBox.Clear();
        }

        /// <summary>
        /// Действия при нажатии на кнопку ОК на вкладке Взятые вещи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void takenValuesInfoCommitButton_Click(object sender, EventArgs e)
        {
            // Если выбрана опция Просмотр
            if (takenValuesInfoSelectRadioButton.Checked)
            {
                selectFromTable("takenvalues_info", getTakenValuesInfoFields());
            }
            // Если выбрана опция Удаление
            if (takenValuesInfoDeleteRadioButton.Checked)
            {
                deleteFromTable("taken_values", takenValuesInfoView.SelectedRows);
                if (autoRefreshTakenValuesInfoCheckBox.Checked) selectFromTable("takenvalues_info", getTakenValuesInfoFields());
            }
        }

        /// <summary>
        /// Действия при нажатии на кнопку Обновить на вкладке взятые вещи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshTakenValuesInfoViewButton_Click(object sender, EventArgs e)
        {
            selectFromTable("takenvalues_info", getTakenValuesInfoFields());
        }

        /// <summary>
        /// Действия при выборе опции Автообновление на вкладке Взятые вещи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoRefreshTakenValuesInfoCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (autoRefreshTakenValuesInfoCheckBox.Checked)
            {
                refreshTakenValuesInfoViewButton.Enabled = false;
                selectFromTable("takenvalues_info", getTakenValuesInfoFields());
            }
            else
            {
                refreshTakenValuesInfoViewButton.Enabled = true;
            }
        }

        /// <summary>
        /// Действия при нажатии на кнопку ОК на вкладке Шкафы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closetsCommitButton_Click(object sender, EventArgs e)
        {
            // Если выбрана опция Просмотр
            if (closetsSelectRadioButton.Checked)
            {
                selectFromTable("closets", getClosetsFields());
            }
            // Если выбрана опция Добавление
            if (closetsInsertRadioButton.Checked)
            {
                insertIntoTable("closets", getClosetsFields());

                if (autoRefreshClosetsCheckBox.Checked)
                {
                    selectFromTable("closets", getClosetsFields());
                }
            }
            // Если выбрана опция Редактирование
            if (closetsUpdateRadioButton.Checked)
            {
                int currentRow = closetsView.CurrentRow.Index;
                if (currentRow != -1)
                {
                    string[] oldValues = {closetsView.Rows[currentRow].Cells[0].Value.ToString(), closetsView.Rows[currentRow].Cells[1].Value.ToString(),
                                          closetsView.Rows[currentRow].Cells[2].Value.ToString(),closetsView.Rows[currentRow].Cells[3].Value.ToString()};
                    string[] newValues = getClosetsFields();
                    updateTable("closets", oldValues, newValues);

                    if (autoRefreshClosetsCheckBox.Checked) selectFromTable("closets", getClosetsFields());
                    if (autoRefreshMaterialValuesCheckBox.Checked) selectFromTable("material_values", getMaterialValuesFields());
                    if (autoRefreshTakenValuesInfoCheckBox.Checked) selectFromTable("takenvalues_info", getTakenValuesInfoFields());
                }
            }
            // Если выбрана опция удаление
            if (closetsDeleteRadioButton.Checked)
            {
                deleteFromTable("closets", closetsView.SelectedRows);
                if (autoRefreshClosetsCheckBox.Checked) selectFromTable("closets", getClosetsFields());
                if (autoRefreshMaterialValuesCheckBox.Checked) selectFromTable("material_values", getMaterialValuesFields());
                if (autoRefreshTakenValuesInfoCheckBox.Checked) selectFromTable("takenvalues_info", getTakenValuesInfoFields());
            }
        }

        /// <summary>
        /// Действия при нажатии на кнопку Обновить на вкладке Шкафы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshClosetsButton_Click(object sender, EventArgs e)
        {
            selectFromTable("closets", getClosetsFields());
        }

        /// <summary>
        /// Действия при выборе опции Автообновление на вкладке Шкафы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoRefreshClosetsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (autoRefreshClosetsCheckBox.Checked)
            {
                refreshClosetsButton.Enabled = false;
                selectFromTable("closets", getClosetsFields());
            }
            else
            {
                refreshClosetsButton.Enabled = true;
            }
        }

        /// <summary>
        /// Действия при нажатии на кнопку ОК на вкладке Ценности
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void materialValuesCommitButton_Click(object sender, EventArgs e)
        {
            // Если выбрана опция Просмотр
            if (materialValuesSelectRadioButton.Checked)
            {
                selectFromTable("material_values", getMaterialValuesFields());
            }
            // Если выбрано опция Добавление
            if (materialValuesInsertRadioButton.Checked)
            {
                insertIntoTable("material_values", getMaterialValuesFields());

                if (autoRefreshMaterialValuesCheckBox.Checked)
                {
                    selectFromTable("material_values", getMaterialValuesFields());
                }
            }
            // Если выбрана опция Редактирование
            if (materialValuesUpdateRadioButton.Checked)
            {
                int currentRow = materialValuesView.CurrentRow.Index;
                if (currentRow != -1)
                {
                    string[] oldValues = {materialValuesView.Rows[currentRow].Cells[0].Value.ToString(), materialValuesView.Rows[currentRow].Cells[1].Value.ToString(),
                                      materialValuesView.Rows[currentRow].Cells[2].Value.ToString()};
                    string[] newValues = getMaterialValuesFields();
                    updateTable("material_values", oldValues, newValues);

                    if (autoRefreshMaterialValuesCheckBox.Checked) selectFromTable("material_values", getMaterialValuesFields());
                    if (autoRefreshTakenValuesInfoCheckBox.Checked) selectFromTable("takenvalues_info", getTakenValuesInfoFields());
                }
            }
            // Если выбрана опция Удаление
            if (materialValuesDeleteRadioButton.Checked)
            {
                deleteFromTable("material_values", materialValuesView.SelectedRows);
                if (autoRefreshMaterialValuesCheckBox.Checked) selectFromTable("material_values", getMaterialValuesFields());
                if (autoRefreshTakenValuesInfoCheckBox.Checked) selectFromTable("takenvalues_info", getTakenValuesInfoFields());
            }
        }

        /// <summary>
        /// Функция для получения значений, введенных в покна ввода текста на вкладке Пользователи
        /// </summary>
        /// <returns>Возвращает массив строк, содержащих значения окон для ввода текста</returns>
        private string[] getUsersFields()
        {
            string[] fields = { usersIdTextBox.Text, usersFTextBox.Text, usersIoTextBox.Text, usersPostTextBox.Text };
            return fields;
        }

        /// <summary>
        /// Функция для получения значений, введенных в покна ввода текста на вкладке Шкафы
        /// </summary>
        /// <returns>Возвращает массив строк, содержащих значения окон для ввода текста</returns>
        private string[] getClosetsFields()
        {
            string[] fields = {closetsIdTextBox.Text, closetsBuildingTextBox.Text,
                                       closetsFloorTextBox.Text, closetsDescriptionTextBox.Text};
            return fields;
        }

        /// <summary>
        /// Функция для получения значений, введенных в покна ввода текста на вкладке Ценности
        /// </summary>
        /// <returns>Возвращает массив строк, содержащих значения окон для ввода текста</returns>
        private string[] getMaterialValuesFields()
        {
            string[] fields = { materialValuesIdTextBox.Text, materialValuesDescriptionTextBox.Text, materialValuesClosetIdTextBox.Text };
            return fields;
        }

        /// <summary>
        /// Функция для получения значений, введенных в покна ввода текста на вкладке Взятые вещи
        /// </summary>
        /// <returns>Возвращает массив строк, содержащих значения окон для ввода текста</returns>
        private string[] getTakenValuesInfoFields()
        {
            string[] fields = {takenValuesInfoUidTextBox.Text, takenValuesInfoFTextBox.Text, takenValuesInfoIoTextBox.Text,
                                   takenValuesInfoPostTextBox.Text, takenValuesInfoVidTextBox.Text, takenValuesInfoVdescriptionTextBox.Text,
                                   takenValuesInfoCidTextBox.Text, takenValuesInfoTakeFromTextBox.Text, takenValuesInfoTakeToTextBox.Text,
                                   takenValuesInfoReturnFromTextBox.Text, takenValuesInfoReturnToTextBox.Text};
            return fields;
        }

        /// <summary>
        /// Действия при нажатии кнопки Обновить на вкладке Ценности
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshMaterialValuesButton_Click(object sender, EventArgs e)
        {
            selectFromTable("material_values", getMaterialValuesFields());
        }

        /// <summary>
        /// Действия при выборе опции Автообновление на вкладке Ценности
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoRefreshMaterialValuesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (autoRefreshMaterialValuesCheckBox.Checked)
            {
                refreshMaterialValuesButton.Enabled = false;
                selectFromTable("material_values", getMaterialValuesFields());
            }
            else
            {
                refreshMaterialValuesButton.Enabled = true;
            }
        }

        /// <summary>
        /// Действия при нажатии кнопки ОК на вкладке Пользователи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void usersCommitButton_Click(object sender, EventArgs e)
        {
            // Если выбрана опция Просмотр
            if (usersSelectRadioButton.Checked)
            {
                selectFromTable("users", getUsersFields());
            }
            // Если выбрана опция Добавление
            if (usersInsertRadioButton.Checked)
            {
                insertIntoTable("users", getUsersFields());

                if (autoRefreshUsersView.Checked)
                {
                    selectFromTable("users", getUsersFields());
                }
            }
            // Если выбрана опция Редактирование
            if (usersUpdateRadioButton.Checked)
            {
                int currentRow = usersView.CurrentRow.Index;
                if (currentRow != -1)
                {
                    string[] oldValues = {usersView.Rows[currentRow].Cells[0].Value.ToString(), usersView.Rows[currentRow].Cells[1].Value.ToString(),
                                      usersView.Rows[currentRow].Cells[2].Value.ToString(),usersView.Rows[currentRow].Cells[3].Value.ToString()};
                    string[] newValues = getUsersFields();
                    updateTable("users", oldValues, newValues);

                    if (autoRefreshUsersView.Checked) selectFromTable("users", getUsersFields());
                    if (autoRefreshTakenValuesInfoCheckBox.Checked) selectFromTable("takenvalues_info", getTakenValuesInfoFields());
                }
            }
            // Если выбрана опция Удаление
            if (usersDeleteRadioButton.Checked)
            {
                deleteFromTable("users", usersView.SelectedRows);
                if (autoRefreshUsersView.Checked) selectFromTable("users", getUsersFields());
                if (autoRefreshTakenValuesInfoCheckBox.Checked) selectFromTable("takenvalues_info", getTakenValuesInfoFields());
            }
        }

        /// <summary>
        /// Действия при нажатии кнопки Обновить на вкладке Пользователи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshUsersView_Click(object sender, EventArgs e)
        {
            selectFromTable("users", getUsersFields());
        }

        /// <summary>
        ///  Действия при выборе опции Автообновление на вкладке Пользователи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoRefreshUsersView_CheckedChanged(object sender, EventArgs e)
        {
            if (autoRefreshUsersView.Checked)
            {
                refreshUsersView.Enabled = false;
                selectFromTable("users", getUsersFields());
            }
            else
            {
                refreshUsersView.Enabled = true;
            }
        }

        // Серия функций, описывающих действия при выборе опций Просмотр, Добавление, Редактирование, Удаление на вкладке Шкафы
        private void closetsSelectRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            closetsIdTextBox.Clear();
            closetsBuildingTextBox.Clear();
            closetsFloorTextBox.Clear();
            closetsDescriptionTextBox.Clear();
        }

        private void closetsInsertRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            closetsIdTextBox.Clear();
            closetsBuildingTextBox.Clear();
            closetsFloorTextBox.Clear();
            closetsDescriptionTextBox.Clear();
        }

        private void closetsUpdateRadioButton_CheckedChanged(object sender, EventArgs e)
        {                   
            if (closetsUpdateRadioButton.Checked)
            {
                closetsView.MultiSelect = false;
                closetsView.Rows[closetsView.CurrentRow.Index].Selected = false;
            }
            else
            {
                closetsView.MultiSelect = true;
            }

            closetsIdTextBox.Clear();
            closetsBuildingTextBox.Clear();
            closetsFloorTextBox.Clear();
            closetsDescriptionTextBox.Clear();
        }

        private void closetsDeleteRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            closetsIdTextBox.Clear();
            closetsBuildingTextBox.Clear();
            closetsFloorTextBox.Clear();
            closetsDescriptionTextBox.Clear();
        }

        // Серия функций, описывающих действия при выборе опций Просмотр, Добавление, Редактирование, Удаление на вкладке Ценности
        private void materialValuesSelectRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            materialValuesIdTextBox.Clear();
            materialValuesDescriptionTextBox.Clear();
            materialValuesClosetIdTextBox.Clear();
        }

        private void materialValuesInsertRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            materialValuesIdTextBox.Clear();
            materialValuesDescriptionTextBox.Clear();
            materialValuesClosetIdTextBox.Clear();
        }

        private void materialValuesUpdateRadioButton_CheckedChanged(object sender, EventArgs e)
        {          
            if (materialValuesUpdateRadioButton.Checked)
            {
                materialValuesView.MultiSelect = false;
                materialValuesView.Rows[materialValuesView.CurrentRow.Index].Selected = false;
            }
            else
            {
                materialValuesView.MultiSelect = true;
            }

            materialValuesIdTextBox.Clear();
            materialValuesDescriptionTextBox.Clear();
            materialValuesClosetIdTextBox.Clear();
        }

        private void materialValuesDeleteRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            materialValuesIdTextBox.Clear();
            materialValuesDescriptionTextBox.Clear();
            materialValuesClosetIdTextBox.Clear();
        }

        // Серия функций, описывающих действия при выборе опций Просмотр, Добавление, Редактирование, Удаление на вкладке Пользователи
        private void usersSelectRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            usersIdTextBox.Clear();
            usersFTextBox.Clear();
            usersIoTextBox.Clear();
            usersPostTextBox.Clear();
        }

        private void usersInsertRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            usersIdTextBox.Clear();
            usersFTextBox.Clear();
            usersIoTextBox.Clear();
            usersPostTextBox.Clear();
        }

        private void usersUpdateRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (usersUpdateRadioButton.Checked)
            {
                usersView.MultiSelect = false;
                usersView.Rows[usersView.CurrentRow.Index].Selected = false;
            }
            else
            {
                usersView.MultiSelect = true;
            }

            usersIdTextBox.Clear();
            usersFTextBox.Clear();
            usersIoTextBox.Clear();
            usersPostTextBox.Clear();
        }

        private void usersDeleteRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            usersIdTextBox.Clear();
            usersFTextBox.Clear();
            usersIoTextBox.Clear();
            usersPostTextBox.Clear();
        }

        // Серия функций, описывающих действия при выборе опций Просмотр, Добавление, Редактирование, Удаление на вкладке Взятые вещи
        private void materialValuesView_SelectionChanged(object sender, EventArgs e)
        {
            if (materialValuesUpdateRadioButton.Checked)
            {
                try
                {
                    int currentRow = materialValuesView.CurrentRow.Index;
                    if (currentRow != -1)
                    {
                        materialValuesIdTextBox.Text = materialValuesView.Rows[currentRow].Cells[0].Value.ToString();
                        materialValuesDescriptionTextBox.Text = materialValuesView.Rows[currentRow].Cells[1].Value.ToString();
                        materialValuesClosetIdTextBox.Text = materialValuesView.Rows[currentRow].Cells[2].Value.ToString();
                    }
                }
                catch (Exception exception)
                {

                }
            }
        }

        private void usersView_SelectionChanged(object sender, EventArgs e)
        {
            if (usersUpdateRadioButton.Checked)
            {
                try
                {
                    int currentRow = usersView.CurrentRow.Index;
                    if (currentRow != -1)
                    {
                        usersIdTextBox.Text = usersView.Rows[currentRow].Cells[0].Value.ToString();
                        usersFTextBox.Text = usersView.Rows[currentRow].Cells[1].Value.ToString();
                        usersIoTextBox.Text = usersView.Rows[currentRow].Cells[2].Value.ToString();
                        usersPostTextBox.Text = usersView.Rows[currentRow].Cells[3].Value.ToString();
                    }
                }
                catch (Exception exception)
                {

                }
            }
        }

        private void closetsView_SelectionChanged(object sender, EventArgs e)
        {
            if (closetsUpdateRadioButton.Checked)
            {
                try
                {
                    int currentRow = closetsView.CurrentRow.Index;
                    if (currentRow != -1)
                    {
                            closetsIdTextBox.Text = closetsView.Rows[currentRow].Cells[0].Value.ToString();
                            closetsBuildingTextBox.Text = closetsView.Rows[currentRow].Cells[1].Value.ToString();
                            closetsFloorTextBox.Text = closetsView.Rows[currentRow].Cells[2].Value.ToString();
                            closetsDescriptionTextBox.Text = closetsView.Rows[currentRow].Cells[3].Value.ToString();
                    }
                }
                catch (Exception exception)
                {

                }
            }
        }
    }
}