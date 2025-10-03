using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using UserManagementlibrary.Entity;

namespace UserManagementlibrary.Repository
{


    public static class DomainRepository
    {
        private static string dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];

        public static void InsertDomain(Domain domain)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = @"INSERT INTO Domain(DomainName, CN, DC1, DC2, DC3, Description, Created_by, Created_Date, Updated_by, Updated_Date, Status,DomainNme)
                                   VALUES (@DomainName, @CN, @DC1, @DC2, @DC3, @Description, @Created_by, @Created_Date, @Updated_by, @Updated_Date, @Status,@DomainNme);";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DomainName", domain.DomainName);
                        cmd.Parameters.AddWithValue("@CN", domain.CN ?? "");
                        cmd.Parameters.AddWithValue("@DC1", domain.DC1 ?? "");
                        cmd.Parameters.AddWithValue("@DC2", domain.DC2 ?? "");
                        cmd.Parameters.AddWithValue("@DC3", domain.DC3 ?? "");
                        cmd.Parameters.AddWithValue("@Description", domain.Description ?? "");
                        cmd.Parameters.AddWithValue("@Created_by", domain.Created_by);
                        cmd.Parameters.AddWithValue("@Created_Date", domain.Created_Date.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@Updated_by", domain.Updated_by ?? "");
                        cmd.Parameters.AddWithValue("@Updated_Date", domain.Updated_Date?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                        cmd.Parameters.AddWithValue("@Status", domain.Status);
                        cmd.Parameters.AddWithValue("@DomainNme", domain.DomainNme);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            
            }
        }
        public static bool IsDomainExists(string domainName)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = "SELECT COUNT(1) FROM Domain WHERE DomainNme = @DomainName COLLATE NOCASE";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DomainName", domainName);
                        long count = (long)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static bool IsDomainExistsById(string domainName,int domainId)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = "SELECT COUNT(1) FROM Domain WHERE DomainNme = @DomainName and DomainID!=@DomainID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DomainName", domainName);
                        cmd.Parameters.AddWithValue("@DomainID", domainId);
                        long count = (long)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void UpdateDomainStatus(int domainId, string status,string Updated_by)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = "UPDATE Domain SET Status = @Status,Updated_by=@Updated_by, Updated_Date = @Updated_Date WHERE DomainID = @DomainID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@Updated_by", Updated_by);
                        cmd.Parameters.AddWithValue("@Updated_Date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@DomainID", domainId);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            
            }
        }
        public static void UpdateDomain(Domain domain)
        {
            try
            {
                if (domain.DomainID <= 0)
                    throw new ArgumentException("Invalid DomainID for update.");

                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = @"UPDATE Domain SET DomainName = @DomainName,
                            CN = @CN,
                            DC1 = @DC1,
                            DC2 = @DC2,
                            DC3 = @DC3,
                            Description = @Description,
                            Updated_by = @Updated_by,
                            Updated_Date = @Updated_Date,
                            Status = @Status,
                            DomainNme = @DomainNme
                        WHERE DomainID = @DomainID;";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DomainName", domain.DomainName ?? "");
                        cmd.Parameters.AddWithValue("@CN", domain.CN ?? "");
                        cmd.Parameters.AddWithValue("@DC1", domain.DC1 ?? "");
                        cmd.Parameters.AddWithValue("@DC2", domain.DC2 ?? "");
                        cmd.Parameters.AddWithValue("@DC3", domain.DC3 ?? "");
                        cmd.Parameters.AddWithValue("@Description", domain.Description ?? "");
                        cmd.Parameters.AddWithValue("@Status", domain.Status);
                        cmd.Parameters.AddWithValue("@Updated_by", domain.Updated_by);
                        cmd.Parameters.AddWithValue("@Updated_Date", domain.Updated_Date);
                        cmd.Parameters.AddWithValue("@DomainNme", domain.DomainNme ?? "");
                        cmd.Parameters.AddWithValue("@DomainID", domain.DomainID);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new Exception($"Domain with ID '{domain.DomainID}' not found to update.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool IsDomainExists(string domainName,int domainId)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = "SELECT COUNT(1) FROM Domain WHERE DomainNme = @DomainName and DomainID=@DomainID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DomainName", domainName);
                        cmd.Parameters.AddWithValue("@DomainID", domainId);
                        long count = (long)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static List<Domain> GetActiveDomains()
        {
            var list = new List<Domain>();
            try { 

                    using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                    {
                        conn.Open();

                        string sql = "SELECT * FROM Domain where Status=@Status COLLATE NOCASE";

                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Status", "Active");
                            using (var reader = cmd.ExecuteReader())
                            {

                                while (reader.Read())
                                {
                                    list.Add(new Domain
                                    {
                                        DomainID = Convert.ToInt32(reader["DomainID"]),
                                        DomainName = reader["DomainName"].ToString(),
                                        CN = reader["CN"].ToString(),
                                        DC1 = reader["DC1"].ToString(),
                                        DC2 = reader["DC2"].ToString(),
                                        DC3 = reader["DC3"].ToString(),
                                        Description = reader["Description"].ToString(),
                                        Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
                                        Created_by = reader["Created_by"].ToString(),
                                        Updated_Date = Convert.ToDateTime(reader["Updated_Date"].ToString()),
                                        Updated_by = reader["Updated_by"].ToString(),
                                        Status = reader["Status"].ToString(),
                                        DomainNme = reader["DomainNme"].ToString()
                                    });
                                }
                            }
                        }
                    }
            }catch (Exception ex)
            {
                throw ex;
            }

            return list;
        }
        public static List<Domain> GetAllDomains()
        {
            var list = new List<Domain>();
            try
            {

                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = "SELECT * FROM Domain";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Domain
                            {
                                DomainID = Convert.ToInt32(reader["DomainID"]),
                                DomainName = reader["DomainName"].ToString(),
                                CN = reader["CN"].ToString(),
                                DC1 = reader["DC1"].ToString(),
                                DC2 = reader["DC2"].ToString(),
                                DC3 = reader["DC3"].ToString(),
                                Description = reader["Description"].ToString(),
                                Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
                                Created_by = reader["Created_by"].ToString(),
                                Updated_Date = Convert.ToDateTime(reader["Updated_Date"].ToString()),
                                Updated_by = reader["Updated_by"].ToString(),
                                Status = reader["Status"].ToString(),
                                DomainNme = reader["DomainNme"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            
            }

            return list;
        }
    }

}
