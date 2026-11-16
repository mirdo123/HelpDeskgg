using HelpDesk.Data;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace HelpDesk.Views
{
    public partial class TicketCreateWindow : Window
    {
        public TicketCreateWindow()
        {
            InitializeComponent();

            LoadCategories();
            LoadPriorities();
        }

        private void LoadCategories()
        {
            CategoryComboBox.Items.Clear();

            string sql = "SELECT Id, Name FROM Categories ORDER BY Name";

            using (SqlConnection connection = Database.GetConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = reader["Name"].ToString();
                        item.Tag = Convert.ToInt32(reader["Id"]);

                        CategoryComboBox.Items.Add(item);
                    }
                }
            }
        }

        private void LoadPriorities()
        {
            PriorityComboBox.Items.Clear();

            string sql = @"
                SELECT Id, Name
                FROM Priorities
                ORDER BY SortOrder";

            using (SqlConnection connection = Database.GetConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = reader["Name"].ToString();
                        item.Tag = Convert.ToInt32(reader["Id"]);

                        PriorityComboBox.Items.Add(item);
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите категорию.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show(
                    "Введите описание проблемы.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (PriorityComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите приоритет.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(
                "На следующих занятиях здесь будет выполняться сохранение заявки в MS SQL Server.",
                "HelpDesk",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
