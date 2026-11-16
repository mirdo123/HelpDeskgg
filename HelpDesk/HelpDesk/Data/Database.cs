using System.Data.SqlClient;

namespace HelpDesk.Data
{
    public static class Database
    {
        public static string ConnectionString =
            @"Server=DH-R401C08\SQLEXPRESS;
              Database=HelpDesk_Kimaev;
              Trusted_Connection=True;
              TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
