using HelpDesk.Data;
using HelpDesk.Helpers;
using HelpDesk.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace HelpDesk.Views
{
    public partial class MainWindow : Window
    {
        private List<Ticket> tickets;

        public MainWindow()
        {
            InitializeComponent();

            SafeLoadTickets();
            LoadStatuses();

            ShowCurrentUser();
            ApplyRolePermissions();
        }

        private void ShowCurrentUser()
        {
            if (UserSession.CurrentUser == null)
            {
                CurrentUserTextBlock.Text = "Пользователь: не определён";
                return;
            }

            CurrentUserTextBlock.Text =
                "Пользователь: " +
                UserSession.CurrentUser.FullName +
                " (" + UserSession.CurrentUser.RoleName + ")";
        }

        private void ApplyRolePermissions()
        {
            if (UserSession.CurrentUser == null)
                return;

            bool isAdmin = UserSession.CurrentUser.RoleName == "Администратор";

            // Учебное правило: удалять заявки может только администратор.
            DeleteTicketButton.IsEnabled = isAdmin;
        }

        private void SafeLoadTickets()
        {
            try
            {
                LoadTickets();
            }
            catch (SqlException ex)
            {
                TicketsDataGrid.ItemsSource = null;

                MessageBox.Show(
                    "Не удалось загрузить заявки из базы данных.\n\n" +
                    "Проверьте SQL Server, имя базы и строку подключения.\n\n" +
                    ex.Message,
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadTickets()
        {
            tickets = new List<Ticket>();

            string sql = @"
                SELECT
                    t.Id,
                    t.UserId,
                    t.CategoryId,
                    c.Name AS Category,
                    t.Description,
                    t.PriorityId,
                    p.Name AS Priority,
                    t.StatusId,
                    s.Name AS Status,
                    t.AssignedUserId,
                    t.CreatedAt,
                    t.UpdatedAt
                FROM Tickets t
                INNER JOIN Categories c ON t.CategoryId = c.Id
                INNER JOIN Priorities p ON t.PriorityId = p.Id
                INNER JOIN Statuses s ON t.StatusId = s.Id
                ORDER BY t.Id DESC;";

            using (SqlConnection connection = Database.GetConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ticket ticket = new Ticket
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            UserId = Convert.ToInt32(reader["UserId"]),
                            CategoryId = Convert.ToInt32(reader["CategoryId"]),
                            Category = reader["Category"].ToString(),
                            Description = reader["Description"].ToString(),
                            PriorityId = Convert.ToInt32(reader["PriorityId"]),
                            Priority = reader["Priority"].ToString(),
                            StatusId = Convert.ToInt32(reader["StatusId"]),
                            Status = reader["Status"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        };

                        if (reader["AssignedUserId"] != DBNull.Value)
                            ticket.AssignedUserId = Convert.ToInt32(reader["AssignedUserId"]);

                        if (reader["UpdatedAt"] != DBNull.Value)
                            ticket.UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]);

                        tickets.Add(ticket);
                    }
                }
            }

            TicketsDataGrid.ItemsSource = tickets;
        }

        private void LoadStatuses()
        {
            try
            {
                StatusComboBox.Items.Clear();
                StatusComboBox.Items.Add("Все");

                string sql = "SELECT Name FROM Statuses ORDER BY Id";

                using (SqlConnection connection = Database.GetConnection())
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StatusComboBox.Items.Add(reader["Name"].ToString());
                        }
                    }
                }

                StatusComboBox.SelectedIndex = 0;
            }
            catch (SqlException)
            {
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (tickets == null)
                return;

            string searchText = SearchTextBox.Text.Trim().ToLower();

            var result = tickets.Where(ticket =>
                ticket.Description.ToLower().Contains(searchText)
            ).ToList();

            TicketsDataGrid.ItemsSource = result;
        }

        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            if (StatusComboBox.Items.Count > 0)
                StatusComboBox.SelectedIndex = 0;
            TicketsDataGrid.ItemsSource = tickets;
        }

        private void AddTicketButton_Click(object sender, RoutedEventArgs e)
        {
            TicketCreateWindow window = new TicketCreateWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void EditTicketButton_Click(object sender, RoutedEventArgs e)
        {
            if (TicketsDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Сначала выберите заявку.",
                    "Редактирование",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Ticket selectedTicket = TicketsDataGrid.SelectedItem as Ticket;

            if (selectedTicket == null)
            {
                MessageBox.Show(
                    "Не удалось получить данные выбранной заявки.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            TicketEditWindow window = new TicketEditWindow(selectedTicket);
            window.Owner = this;

            bool? result = window.ShowDialog();

            if (result == true)
            {
                TicketsDataGrid.Items.Refresh();
            }
        }

        private void DeleteTicketButton_Click(object sender, RoutedEventArgs e)
        {
            if (TicketsDataGrid.SelectedItem == null)
            {
                MessageBox.Show(
                    "Выберите заявку для удаления.",
                    "Удаление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Вы действительно хотите удалить выбранную заявку?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "На следующих занятиях здесь будет выполняться удаление записи из MS SQL Server.",
                    "HelpDesk",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Выйти из учётной записи?",
                "HelpDesk",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            UserSession.End();

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
