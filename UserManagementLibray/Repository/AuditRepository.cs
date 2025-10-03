using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UserManagementLibray.Entity;

namespace UserManagementLibray.Repository
{
    public static class AuditRepository
    {
        private static string dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];

        public static void InsertAudit(Audit audit)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    connection.Open();
                    string query = @"INSERT INTO Audit (AuditDate, UserName, Detail, Created_by, Created_Date)
                                 VALUES (@AuditDate, @UserName, @Detail, @Created_by, datetime('now'))";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@AuditDate", audit.AuditDate);
                        cmd.Parameters.AddWithValue("@UserName", audit.UserName);
                        cmd.Parameters.AddWithValue("@Detail", audit.Detail);
                        cmd.Parameters.AddWithValue("@Created_by", audit.Created_by);
                        cmd.Parameters.AddWithValue("@Created_by", audit.Created_by);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "InsertAudit Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static List<Audit> GetAllAudits()
        {
            var audits = new List<Audit>();

            using (var connection = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                connection.Open();
                string query = "SELECT * FROM Audit ORDER BY AuditDate DESC";

                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        audits.Add(new Audit
                        {
                            AuditID = Convert.ToInt32(reader["AuditID"]),
                            AuditDate = Convert.ToDateTime(reader["AuditDate"].ToString()),
                            UserName = reader["UserName"].ToString(),
                            Detail = reader["Detail"].ToString(),
                            Created_by = reader["Created_by"].ToString(),
                            Created_Date =Convert.ToDateTime( reader["Created_Date"].ToString()),
                            Updated_by = reader["Updated_by"].ToString(),
                            Updated_Date = Convert.ToDateTime(reader["Updated_Date"].ToString())
                        });
                    }
                }
            }
            return audits;
        }
        public static List<Audit> GetFilteredAudit(DateTime? selectDate, string keyword)
        {
            
            var auditLog = new List<Audit>();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();
                    string query = "SELECT * FROM Audit WHERE 1=1";

                    if (selectDate.HasValue)
                        query += " AND DATE(AuditDate) =@selectDate ";


                    if (!string.IsNullOrEmpty(keyword))
                        query += " AND (LOWER(UserName) LIKE @Keyword OR LOWER(Detail) LIKE @Keyword)";

                    query += " Order by Created_Date DESC";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        if (selectDate.HasValue) cmd.Parameters.AddWithValue("@selectDate", selectDate.Value.ToString("yyyy-MM-dd"));
                        if (!string.IsNullOrEmpty(keyword)) cmd.Parameters.AddWithValue("@Keyword", $"%{keyword.ToLower()}%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                auditLog.Add(new Audit
                                {
                                    AuditID = Convert.ToInt32(reader["AuditID"]),
                                    AuditDate = Convert.ToDateTime(reader["AuditDate"].ToString()),
                                    UserName = reader["UserName"].ToString(),
                                    Detail = reader["Detail"].ToString(),
                                    Created_by = reader["Created_by"].ToString(),
                                    Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
                                    Updated_by = reader["Updated_by"].ToString(),
                                    Updated_Date = Convert.ToDateTime(reader["Updated_Date"].ToString())
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return auditLog;
        }
    }
}
