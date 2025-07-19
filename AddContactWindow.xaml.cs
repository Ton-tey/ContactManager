using Contacts_Manager;
using System;
using System.Data.SqlClient;
using System.Windows;

namespace ContactsManagerWPF
{
    public partial class AddContactWindow : Window
    {
        private int? contactId = null; // Nullable - null means "Add", otherwise "Edit"

        public AddContactWindow()
        {
            InitializeComponent();
        }

        // Constructor for Edit mode
        public AddContactWindow(int id)
        {
            InitializeComponent();
            contactId = id;
            LoadContactData(id);
        }

        private void LoadContactData(int id)
        {
            using (SqlConnection conn = new SqlConnection(AppConfig.ConnectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Contacts WHERE ID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        NameTextBox.Text = reader["FullName"].ToString();
                        EmailTextBox.Text = reader["Email"].ToString();
                        PhoneTextBox.Text = reader["PhoneNumber"].ToString();
                        AddressTextBox.Text = reader["Address"].ToString();
                        FavoriteCheckBox.IsChecked = Convert.ToBoolean(reader["IsFavorite"]);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading contact: " + ex.Message);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string email = EmailTextBox.Text;
            string phone = PhoneTextBox.Text;
            string address = AddressTextBox.Text;
            bool isFavorite = FavoriteCheckBox.IsChecked ?? false;

            using (SqlConnection conn = new SqlConnection(AppConfig.ConnectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd;

                    if (contactId == null) // ADD NEW
                    {
                        cmd = new SqlCommand("INSERT INTO Contacts (FullName, Email, PhoneNumber, Address, IsFavorite) VALUES (@name, @mail, @phone, @address, @fav)", conn);
                    }
                    else // UPDATE EXISTING
                    {
                        cmd = new SqlCommand("UPDATE Contacts SET FullName = @name, Email = @mail, PhoneNumber = @phone, Address = @address, IsFavorite = @fav WHERE ID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", contactId);
                    }

                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@mail", email);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@address", address);
                    cmd.Parameters.AddWithValue("@fav", isFavorite);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show(contactId == null ? "Contact added!" : "Contact updated!");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving contact: " + ex.Message);
                }
            }
        }
    }
}
