using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient; // подключаем клиент для работы с БД
using System.Data.Sql;

namespace PostalService
{
    public partial class Form1 : Form
    {
        /* функция скрытия всех форм-вкладок */
        private void hideAllForms(TabControl tc)
        {
            foreach (TabPage t in tc.TabPages)
                tc.TabPages.Remove(t);
        }

        /* функция скрывает все формы-вкладки и показывает нужную */
        private void showForm(TabControl tc, TabPage tp)
        {
            hideAllForms(tc);
            tc.TabPages.Add(tp);
        }

        /* строка для подключения */
        string connectString = @"Data Source=DESKTOP-1Q3VADQ\SQLEXPRESS;Initial Catalog=postalservice;Integrated Security=True";

        bool isStaff;
        string userName;

        private SqlConnection myConnection;

        public Form1()
        {
            InitializeComponent();

            // скрываем все формы, показываем форму приветствия
            hideAllForms(formTabControl);
            showForm(formTabControl, welcomeForm);

            try
            {
                // создаем экземпляр класса SqlConnection
                myConnection = new SqlConnection(connectString);

                // открываем соединение с БД
                myConnection.Open();
            }
            catch (Exception exc)
            {
                // если произошла ошибка при подключении к БД, перехватываем ее и выводим
                MessageBox.Show("Ошибка при подключении к базе данных: " + exc.Message);
                Environment.Exit(0);
            }
        }

        private void loginAsCustomerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            isStaff = false;
            authInfoLabel.Text = "клиента";
            showForm(formTabControl, loginForm);
        }

        private void loginAsStaffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isStaff = true;
            authInfoLabel.Text = "сотрудника";
            showForm(formTabControl, loginForm);
        }

        private void regToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // получаем из БД список почтовых участков

            // текст запроса
            string query = "SELECT DepCode, DepAdress FROM Departments ORDER BY DepCode";

            try
            {
                // иницилизируем адаптер
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connectString);
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                // Заполняем DataTable данными из базы
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                // очищаем regDepBox
                regDepBox.Items.Clear();

                // заполняем comboBox
                regDepBox.DataSource = dataTable;
                regDepBox.ValueMember = dataTable.Columns[0].ColumnName;
                regDepBox.DisplayMember = dataTable.Columns[1].ColumnName;

                // показываем форму
                showForm(formTabControl, regForm);
                regDepBox.SelectedIndex = -1;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка получения данных из БД: " + exc.Message);
            }
        }


        private void loginButton_Click(object sender, EventArgs e)
        {
            // авторизуемся

            // текст запроса
            string query = "SELECT ";
            if (isStaff)
                query += "StaffName FROM Staff";
            else
                query += "CustomerName FROM Customers";
            query += " WHERE Login = '" + loginBox.Text + "' AND Password = '" + passwordBox.Text + "'";

            try
            {
                // создаем объект SqlCommand для выполнения запроса к БД
                SqlCommand command = new SqlCommand(query, myConnection);

                try
                {
                    // выполняем запрос и выводим результат в textBox1
                    userName = command.ExecuteScalar().ToString();
                }
                catch
                {
                    // если ошибка авторизации (такого пользователя нет), то порождаем исключение
                    throw new Exception("Неверный логин или пароль!");
                }

                // при успешной авторизации включаем нужные элементы управления и отключаем ненужные
                packagesToolStripMenuItem.Visible = true;
                loginAsCustomerToolStripMenuItem1.Visible = false;
                loginAsStaffToolStripMenuItem.Visible = false;
                regToolStripMenuItem.Visible = false;
                logoutToolStripMenuItem.Visible = true;
                this.Text = "Почтовая служба - " + userName;

                // если пользователь сотрудник, то он может редактировать, добавлять и удалять отправления
                if (isStaff)
                {
                    addPackageToolStripMenuItem.Visible = true;
                    editPackageToolStripMenuItem.Visible = true;
                    this.Text += " (сотрудник)";
                }

                // выводим приветствие
                MessageBox.Show("Здравствуйте, " + userName + "! Вы успешно авторизовались.");

                // скрываем формы, вновь показываем приветствие
                hideAllForms(formTabControl);
                showForm(formTabControl, welcomeForm);
                welcomeHintLabel.Text = "Спасибо, что пользуетесь нашими услугами,\n" + userName;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка авторизации: " + exc.Message);
            }
        }

        private void regButton_Click(object sender, EventArgs e)
        {
            // текст запроса
            string query = "INSERT INTO Customers ([Login], [CustomerName], [DepCode], [BirthDate], [Adress], [Phone], [Passport], [Password]) " +
                "VALUES ('" + regLoginBox.Text + "', '" + regNameBox.Text + "', " + regDepBox.SelectedValue + ", '" + regBirthDateBox.Text +
                "', '" + regAdressBox.Text + "', '" + regPhoneBox.Text + "', '" + regPassportBox.Text + "', '" + regPasswordBox.Text + "')";

            try
            {
                // создаем объект SqlCommand для выполнения запроса к БД
                SqlCommand command = new SqlCommand(query, myConnection);

                // выполняем запрос
                command.ExecuteNonQuery();

                // если регистрация успешна, то скрываем форму и выводим сообщение
                MessageBox.Show("Вы успешно зарегистрировались!");
                hideAllForms(formTabControl);
                showForm(formTabControl, welcomeForm);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка регистрации в БД: " + exc.Message);
            }
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            userName = null;

            // при выходе скрываем ненужные элементы управления и показываем нужные
            packagesToolStripMenuItem.Visible = false;
            loginAsCustomerToolStripMenuItem1.Visible = true;
            loginAsStaffToolStripMenuItem.Visible = true;
            regToolStripMenuItem.Visible = true;
            logoutToolStripMenuItem.Visible = false;
            addPackageToolStripMenuItem.Visible = false;
            editPackageToolStripMenuItem.Visible = false;
            this.Text = "Почтовая служба";

            // скрываем формы
            hideAllForms(formTabControl);
            showForm(formTabControl, welcomeForm);
            welcomeHintLabel.Text = "Пожалуйста, авторизуйтесь";

            // выводим сообщение
            MessageBox.Show("Вы успешно вышли из системы.");
        }

        private void viewPackagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // получаем из БД список почтовых отправлений

            // текст запроса
            string query = "SELECT PackageCode, CustomerName, PackageType, Weight, PackageDate " +
                "FROM Packages, Customers WHERE CustomerLogin = Login ";
                
            // если пользователь не сотрудник, то получаем только свои отправления
            if (!isStaff) query += "AND CustomerName = '" + userName + "' ";
                    
            query += "ORDER BY PackageCode";

            try
            {
                // иницилизируем адаптер
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connectString);
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                // Заполняем DataTable данными из базы
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                // указываем как источник данных packagesGrid
                packagesGrid.DataSource = dataTable;

                // показываем форму
                showForm(formTabControl, viewPackagesForm);

                // задаем свойства столбцам
                packagesGrid.Columns[0].HeaderText = "Код";
                packagesGrid.Columns[1].HeaderText = "Клиент";
                packagesGrid.Columns[2].HeaderText = "Тип отправления";
                packagesGrid.Columns[3].HeaderText = "Вес";
                packagesGrid.Columns[4].HeaderText = "Дата";

                packagesGrid.Columns[0].Width = 50;
                packagesGrid.Columns[1].Width = 280;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка получения данных из БД: " + exc.Message);
            }
        }

        private void findPackageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // получаем из БД список почтовых участков

            // текст запроса
            string query = "SELECT PackageType FROM Package_types";

            try
            {
                // создаем объект SqlCommand для выполнения запроса к БД
                SqlCommand command = new SqlCommand(query, myConnection);

                // получаем объект SqlDataReader для чтения табличного результата запроса SELECT
                SqlDataReader reader = command.ExecuteReader();

                // очищаем regDepBox
                findPackageTypeBox.Items.Clear();

                // в цикле построчно читаем ответ от БД
                while (reader.Read())
                {
                    // выводим данные столбцов текущей строки
                    findPackageTypeBox.Items.Add(reader[0].ToString());
                }

                // закрываем SqlDataReader
                reader.Close();

                // показываем форму
                showForm(formTabControl, findPackageForm);
                findPackagesGrid.Visible = false;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка получения данных из БД: " + exc.Message);
            }            
        }

        private void addPackageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // получаем из БД список клиентов и типов отправления

            // текст запроса

            string query = "SELECT CustomerName FROM Customers ORDER BY CustomerName";
            string query2 = "SELECT PackageType FROM Package_types";

            try
            {
                // создаем объект SqlCommand для выполнения запроса к БД
                SqlCommand command = new SqlCommand(query, myConnection);

                // получаем объект SqlDataReader для чтения табличного результата запроса SELECT
                SqlDataReader reader = command.ExecuteReader();

                // очищаем списки
                addPackageCustomerNameBox.Items.Clear();
                addPackageTypeBox.Items.Clear();

                // в цикле построчно читаем ответ от БД
                while (reader.Read())
                {
                    // выводим данные столбцов текущей строки
                    addPackageCustomerNameBox.Items.Add(reader[0].ToString());
                }

                // закрываем SqlDataReader
                reader.Close();

                // повторяем для типов отправлений
                command = new SqlCommand(query2, myConnection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    addPackageTypeBox.Items.Add(reader[0].ToString());
                }
                reader.Close();

                // показываем форму
                showForm(formTabControl, addPackageForm);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка получения данных из БД: " + exc.Message);
            }
        }

        private void addPackageButton_Click(object sender, EventArgs e)
        {
            // текст запроса на нахождение логина пользователя
            string query = "SELECT Login FROM Customers WHERE CustomerName = '" + addPackageCustomerNameBox.Text + "'";

            try
            {
                // создаем объект SqlCommand для выполнения запроса к БД
                SqlCommand command = new SqlCommand(query, myConnection);

                // выполняем запрос и сохраняем результат
                string CustomerLogin = command.ExecuteScalar().ToString();

                string query2 = "INSERT INTO Packages ([CustomerLogin], [PackageType], [Weight], [PackageDate]) " +
                "VALUES ('" + CustomerLogin + "', '" +
                addPackageTypeBox.Text + "', " + addPackageWeightBox.Text + ", '" + addPackageDateBox.Text + "')";

                // создаем объект SqlCommand для выполнения запроса к БД
                command = new SqlCommand(query2, myConnection);

                // выполняем запрос
                command.ExecuteNonQuery();

                // если регистрация успешна, то скрываем форму и выводим сообщение
                MessageBox.Show("Отправление успешно добавлено!");
                addPackageWeightBox.Clear();
                addPackageDateBox.Clear();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка добавления записи в БД: " + exc.Message);
            }
        }

        private void editPackageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // получаем из БД список кодов почтовых отправлений, типов отправлений и клиентов

            // текст запроса на коды отправлений и их типы
            string query = "SELECT PackageCode FROM Packages ORDER BY PackageCode";
            string query2 = "SELECT PackageType FROM Package_types";
            string query3 = "SELECT Login, CustomerName FROM Customers ORDER BY CustomerName";

            try
            {
                // создаем объект SqlCommand для выполнения запроса к БД
                SqlCommand command = new SqlCommand(query, myConnection);

                // получаем объект SqlDataReader для чтения табличного результата запроса SELECT
                SqlDataReader reader = command.ExecuteReader();

                // очищаем regDepBox
                editPackagesIds.Items.Clear();
                editPackagesTypeBox.Items.Clear();

                // в цикле построчно читаем ответ от БД
                while (reader.Read())
                {
                    // выводим данные столбцов текущей строки
                    editPackagesIds.Items.Add(reader[0].ToString());
                }

                // закрываем SqlDataReader
                reader.Close();

                // повторяем для типов отправлений
                command = new SqlCommand(query2, myConnection);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    editPackagesTypeBox.Items.Add(reader[0].ToString());
                }
                reader.Close();

                // повторяем для клиентов
                // иницилизируем адаптер
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query3, connectString);
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                // Заполняем DataTable данными из базы
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                // очищаем regDepBox
                editPackagesCustomerNameBox.DataSource = null;
                editPackagesCustomerNameBox.Items.Clear();

                // заполняем comboBox
                editPackagesCustomerNameBox.DataSource = dataTable;
                editPackagesCustomerNameBox.ValueMember = dataTable.Columns[0].ColumnName;
                editPackagesCustomerNameBox.DisplayMember = dataTable.Columns[1].ColumnName;

                // показываем форму
                showForm(formTabControl, editPackageForm);
                editPackagesCustomerNameBox.SelectedIndex = -1;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка получения данных из БД: " + exc.Message);
            }
        }

        private void deletePackageToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // закрываем соединение с БД
            myConnection.Close();
        }

        private void findPackageButton_Click(object sender, EventArgs e)
        {
            // запрос к БД на поиск
            string query = "SELECT PackageCode, CustomerName, PackageType, Weight, PackageDate " +
                "FROM Packages, Customers WHERE CustomerLogin = Login ";

            // в зависимости от условий поиска составляем необходимый запрос
            if (!string.IsNullOrWhiteSpace(findPackageNameBox.Text))
                query += "AND CustomerLogin = (SELECT Login FROM Customers WHERE CustomerName LIKE '%" +
                    findPackageNameBox.Text + "%') ";

            if (findPackageTypeBox.SelectedIndex != -1)
                query += "AND PackageType LIKE '%" + findPackageTypeBox.Text + "%'";

            try { 
                // иницилизируем адаптер
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connectString);
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                // Заполняем DataTable данными из базы
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                findPackagesGrid.DataSource = dataTable; // указываем как источник данных packagesGrid

                // очистим поле поиска
                findPackageNameBox.Clear();

                // показать таблицу с результатами поиска
                findPackagesGrid.Visible = true;

                // задаем свойства столбцам
                findPackagesGrid.Columns[0].HeaderText = "Код";
                findPackagesGrid.Columns[1].HeaderText = "Клиент";
                findPackagesGrid.Columns[2].HeaderText = "Тип отправления";
                findPackagesGrid.Columns[3].HeaderText = "Вес";
                findPackagesGrid.Columns[4].HeaderText = "Дата";

                findPackagesGrid.Columns[0].Width = 50;
                findPackagesGrid.Columns[1].Width = 280;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка получения данных из БД: " + exc.Message);
            }
        }

        private void findPackageTimer_Tick(object sender, EventArgs e)
        {
            // проверка, что все поля заполнены. Если да, то кнопка поиска активна
            if (string.IsNullOrWhiteSpace(findPackageNameBox.Text)
                && findPackageTypeBox.SelectedIndex == -1)
            {
                findPackageButton.Enabled = false;
            }
            else findPackageButton.Enabled = true;
        }

        private void regCheckTimer_Tick(object sender, EventArgs e)
        {
            // проверка, что все поля заполнены. Если да, то кнопка регистрации активна
            if (!regBirthDateBox.MaskCompleted || !regPhoneBox.MaskCompleted
                || string.IsNullOrWhiteSpace(regLoginBox.Text)
                || string.IsNullOrWhiteSpace(regNameBox.Text)
                || regDepBox.SelectedIndex == -1
                || string.IsNullOrWhiteSpace(regAdressBox.Text)
                || !regPassportBox.MaskCompleted
                || string.IsNullOrWhiteSpace(regPasswordBox.Text))
            {
                regButton.Enabled = false;
                return;
            }
            else regButton.Enabled = true;
        }

        private void loginCheckTimer_Tick(object sender, EventArgs e)
        {
            // проверка, что все поля заполнены. Если да, то кнопка авторизации активна 
            if (string.IsNullOrWhiteSpace(loginBox.Text)
                || string.IsNullOrWhiteSpace(passwordBox.Text))
            {
                loginButton.Enabled = false;
            }
            else loginButton.Enabled = true;
        }

        private void addPackageTimer_Tick(object sender, EventArgs e)
        {
            // проверка, что все поля заполнены. Если да, то кнопка добавления активна 
            if (addPackageCustomerNameBox.SelectedIndex == -1
                || addPackageTypeBox.SelectedIndex == -1
                || string.IsNullOrWhiteSpace(addPackageWeightBox.Text)
                || !addPackageDateBox.MaskCompleted)
            {
                addPackageButton.Enabled = false;
            }
            else addPackageButton.Enabled = true;
        }

        private void editPackagesIds_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (editPackagesIds.SelectedIndex != -1)
            {
                // текст запроса на получение информации о выбранном отправлении
                string query = "SELECT CustomerLogin, PackageType, Weight, PackageDate " +
                "FROM Packages WHERE PackageCode = " + editPackagesIds.SelectedItem.ToString();

                try
                {
                    // создаем объект SqlCommand для выполнения запроса к БД
                    SqlCommand command = new SqlCommand(query, myConnection);

                    // получаем объект SqlDataReader для чтения табличного результата запроса SELECT
                    SqlDataReader reader = command.ExecuteReader();

                    // в цикле построчно читаем ответ от БД
                    while (reader.Read())
                    {
                        // заполняем поля данными
                        foreach (DataRowView item in editPackagesCustomerNameBox.Items)
                        {
                            if (item.Row.ItemArray[0].ToString() == reader[0].ToString())
                                editPackagesCustomerNameBox.SelectedValue = item.Row.ItemArray[0];
                        }

                        foreach (string item in editPackagesTypeBox.Items)
                        {
                            if (item == reader[1].ToString())
                                editPackagesTypeBox.SelectedItem = item;
                        }

                        editPackagesWeightBox.Text = reader[2].ToString();
                        editPackagesDateBox.Text = reader[3].ToString();
                    }

                    // закрываем SqlDataReader
                    reader.Close();
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Ошибка получения данных из БД: " + exc.Message);
                }
            }
        }

        private void deletePackageButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы действительно хотите удалить отправление?", "Внимание", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                // текст запроса на удаление записи
                string query = "DELETE FROM Packages WHERE PackageCode = " + editPackagesIds.SelectedItem.ToString();

                try
                {
                    // создаем объект SqlCommand для выполнения запроса к БД
                    SqlCommand command = new SqlCommand(query, myConnection);

                    // выполняем запрос
                    command.ExecuteScalar();

                    // удаляем код из списка и очищаем поля
                    editPackagesIds.Items.Remove(editPackagesIds.SelectedItem);
                    editPackagesCustomerNameBox.SelectedIndex = -1;
                    editPackagesTypeBox.SelectedIndex = -1;
                    editPackagesWeightBox.Clear();
                    editPackagesDateBox.Clear();

                }
                catch (Exception exc)
                {
                    MessageBox.Show("Ошибка удаления записи из БД: " + exc.Message);
                }
            }
        }

        private void editPackageTimer_Tick(object sender, EventArgs e)
        {
            // проверка, что отправление выбрано. Если выбрано, то кнопки сохранения изменений и удаления активны
            if (editPackagesIds.SelectedIndex != - 1)
            {
                savePackageButton.Enabled = true;
                deletePackageButton.Enabled = true;
            }
            else
            {
                savePackageButton.Enabled = false;
                deletePackageButton.Enabled = false;
            }
        }

        private void savePackageButton_Click(object sender, EventArgs e)
        {
            // текст запроса на сохранение изменений
            string query = "UPDATE Packages SET CustomerLogin = '" + editPackagesCustomerNameBox.SelectedValue +
                "', PackageType = '" + editPackagesTypeBox.Text + "', Weight = '" + editPackagesWeightBox.Text +
                "', PackageDate = '" + editPackagesDateBox.Text + "' WHERE PackageCode = " + editPackagesIds.SelectedItem.ToString();

            try
            {
                // создаем объект SqlCommand для выполнения запроса к БД
                SqlCommand command = new SqlCommand(query, myConnection);

                // выполняем запрос
                command.ExecuteNonQuery();                

                // если сохранение изменений успешно, то выводим сообщение
                MessageBox.Show("Изменения сохранены!");
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка редактирования записи в БД: " + exc.Message);
            }
        }

        private void queriesTimer_Tick(object sender, EventArgs e)
        {
            //если запрос не выбран, то кнопка выполнения запроса неактивна, иначе активна
            if (queriesBox.SelectedIndex != -1)
                executeQueryButton.Enabled = true;
            else
                executeQueryButton.Enabled = false;
        }

        private void queriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //показываем нужную форму
            hideAllForms(formTabControl);
            showForm(formTabControl, queriesForm);

            //создаем таблицу с запросами
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("QueryText");
            dataTable.Columns.Add("QueryDescription");
            dataTable.Rows.Add(new Object[] { "SELECT CustomerName AS ФИО, Adress AS Адрес, BirthDate AS ДатаРождения, " +
                "Phone AS Телефон, Passport AS Паспорт " +
                "FROM Customers WHERE Adress LIKE '%Коммунаров%'",
                "Клиенты, проживающие на Коммунаров" });
            dataTable.Rows.Add(new Object[] { "SELECT CustomerName AS ФИО, Adress AS Адрес, BirthDate AS ДатаРождения, " +
                "Phone AS Телефон, Passport AS Паспорт " +
                "FROM Customers WHERE CustomerName LIKE 'К%'",
                "Клиенты с фамилией на К" });
            dataTable.Rows.Add(new Object[] { "SELECT CustomerName AS ФИО, Adress AS Адрес, BirthDate AS ДатаРождения, " +
                "Phone AS Телефон, Passport AS Паспорт " +
                "FROM Customers WHERE BirthDate LIKE '%1990%'",
                "Клиенты 1990 года рождения" });
            dataTable.Rows.Add(new Object[] { "SELECT CustomerName AS ФИО, PackageType AS Тип, Weight AS Вес, " +
                "PackageDate AS Дата " +
                "FROM Packages, Customers WHERE Packages.CustomerLogin = Customers.Login AND PackageType = 'Письмо' AND Weight > 10",
                "Письма весом более 10 грамм" });
            dataTable.Rows.Add(new Object[] { "SELECT CustomerName AS ФИО, PackageType AS Тип, Weight AS Вес, " +
                "PackageDate AS Дата " +
                "FROM Packages, Customers WHERE Packages.CustomerLogin = Customers.Login AND PackageType = 'Посылка'",
                "Все посылки" });

            //привязываем таблицу с запросами к выпадающему списку
            queriesBox.DataSource = dataTable;
            queriesBox.ValueMember = dataTable.Columns[0].ColumnName;
            queriesBox.DisplayMember = dataTable.Columns[1].ColumnName;
        }

        private void executeQueryButton_Click(object sender, EventArgs e)
        {
            // запрос к БД
            string query = queriesBox.SelectedValue.ToString();

            try
            {
                // иницилизируем адаптер
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connectString);
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                // Заполняем DataTable данными из базы
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                // указываем как источник данных queriesGrid
                queriesGrid.DataSource = dataTable;

                // показать таблицу с результатами поиска
                queriesGrid.Visible = true;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка получения данных из БД: " + exc.Message);
            }
        }
    }
}
