using System.Configuration;
using System.Data.Client;
namespace Phonestore.WinForms.Database
{
   public static class DbHelper
   {
      public static SqlConnection GetConnection()
      {
         return new SqlConnection(_connectionString);
      }
   }
}
