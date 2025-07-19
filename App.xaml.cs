using System.Configuration;
using System.Data;
using System.Windows;

namespace Contacts_Manager
{
    public static class AppConfig
    {
        public static string ConnectionString => "Server=THINKPAD-T495;Database=contactDetails;Trusted_Connection=True;";
    }

    public partial class App : Application
    {
    }

}
