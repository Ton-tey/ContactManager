using ContactsManagerWPF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Text;

namespace Contacts_Manager
{
    public partial class MainWindow : Window
    {
        private string connectionString => "Server=THINKPAD-T495;Database=contactDetails;Trusted_Connection=True;";
        private List<Contact> contacts = new List<Contact>();
        private List<Contact> allContacts = new List<Contact>();

        public MainWindow()
        {
            InitializeComponent();
            LoadContacts();
        }

        private void LoadContacts()
        {
            contacts.Clear();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT ID AS ContactId, FullName AS Name, Email, PhoneNumber AS Phone, Address, DateAdded, IsFavorite FROM Contacts";

                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        contacts.Add(new Contact
                        {
                            ContactId = (int)reader["ContactId"],
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            Address = reader["Address"].ToString(),
                            DateAdded = (DateTime)reader["DateAdded"],
                            IsFavorite = reader["IsFavorite"] != DBNull.Value && (bool)reader["IsFavorite"]
                            

                        });
                        allContacts.Add(new Contact
                        {
                            ContactId = (int)reader["ContactId"],
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            Address = reader["Address"].ToString(),
                            DateAdded = (DateTime)reader["DateAdded"],
                            IsFavorite = reader["IsFavorite"] != DBNull.Value && (bool)reader["IsFavorite"]
                            

                        });
                    }
                    reader.Close();
                }
                ApplyFilter(SearchTextBox.Text);
                ContactsDataGrid.ItemsSource = null;
                ContactsDataGrid.ItemsSource = contacts;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading contacts: " + ex.Message);
            }
        }

        private void EditContact_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int id))
            {
                AddContactWindow editWindow = new AddContactWindow(id);
                editWindow.ShowDialog();
                LoadContacts(); // Refresh after editing
            }
        }


        private void AddContact_Click(object sender, RoutedEventArgs e)
        {
            AddContactWindow addWindow = new AddContactWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadContacts();
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadContacts();
        }
        private void ApplyFilter(string searchText)
        {
            if (contacts == null) return;

            var filteredContacts = contacts.Where(c =>
                (c.Name != null && c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (c.Email != null && c.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (c.Phone != null && c.Phone.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (c.Address != null && c.Address.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            ContactsDataGrid.ItemsSource = filteredContacts;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(SearchTextBox.Text);
        }

        private void DeleteContact_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int contactId))
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();


                        string query = "DELETE FROM Contacts WHERE ID = @id";

                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", contactId);

                        cmd.ExecuteNonQuery();
                    }

                    LoadContacts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting contact: " + ex.Message);
                }
            }
        }


        private void ExportToCsv_Click(object sender, RoutedEventArgs e)
        {
            if (contacts.Count == 0)
            {
                MessageBox.Show("No contacts to export.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV file (*.csv)|*.csv";
            saveFileDialog.FileName = "ContactsExport.csv";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    StringBuilder csvContent = new StringBuilder();
                    csvContent.AppendLine("Name,Email,Phone,Address,DateAdded,IsFavorite");

                    foreach (var contact in contacts)
                    {
                        string line = $"\"{contact.Name}\",\"{contact.Email}\",\"{contact.Phone}\",\"{contact.Address}\",\"{contact.DateAdded:yyyy-MM-dd HH:mm:ss}\",\"{contact.IsFavorite}\"";
                        csvContent.AppendLine(line);
                    }

                    File.WriteAllText(saveFileDialog.FileName, csvContent.ToString());
                    MessageBox.Show("Contacts exported successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting contacts: " + ex.Message);
                }
            }
        }

        public class Contact
        {
            public int ContactId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public DateTime DateAdded { get; set; }
            public bool IsFavorite { get; set; }
            public string GroupKey => string.IsNullOrEmpty(Name) ? "#" : Name.Substring(0, 1).ToUpper();

        }
    }
}
