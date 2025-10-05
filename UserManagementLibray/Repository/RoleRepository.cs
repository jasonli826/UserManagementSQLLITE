using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using UserManagementlibrary.Entity;

namespace UserManagementlibrary.Repository
{


    public static class RoleRepository
    {

        private static string dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];
        public static void InsertRole(Role role)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();
                    // Get the current max PriorityIndex
                    int maxPriority = 0;
                    string sqlMax = "SELECT MAX(PriorityIndex) FROM Role";
                    using (var cmdMax = new SQLiteCommand(sqlMax, conn))
                    {
                        var result = cmdMax.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                            maxPriority = Convert.ToInt32(result);
                    }
                    string sql = @"INSERT INTO Role (Role_Name, Description, Created_by, Created_Date, Updated_by, Updated_Date, Status,PriorityIndex) VALUES (@Role_Name, @Description, @Created_by, @Created_Date, @Updated_by, @Updated_Date, @Status,@PriorityIndex);";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Role_Name", role.Role_Name);
                        cmd.Parameters.AddWithValue("@Description", role.Description ?? "");
                        cmd.Parameters.AddWithValue("@Created_by", role.Created_by);
                        cmd.Parameters.AddWithValue("@Created_Date", role.Created_Date.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@Updated_by", role.Updated_by ?? "");
                        cmd.Parameters.AddWithValue("@Updated_Date", role.Updated_Date?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                        cmd.Parameters.AddWithValue("@Status", role.Status);
                        cmd.Parameters.AddWithValue("@PriorityIndex", maxPriority + 1); 

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void UpdatePriorityIndex(int roleId, int priorityIndex)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("UPDATE Role SET PriorityIndex = @PriorityIndex WHERE RoleID = @RoleID", conn);
                cmd.Parameters.AddWithValue("@PriorityIndex", priorityIndex);
                cmd.Parameters.AddWithValue("@RoleID", roleId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateParentRole(int roleId, int? parentRoleId)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("UPDATE Role SET ParentRoleID = @ParentRoleID WHERE RoleID = @RoleID", conn))
                {
                    cmd.Parameters.AddWithValue("@ParentRoleID", parentRoleId.HasValue ? (object)parentRoleId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoleID", roleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void ClearChildren(int roleId)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("UPDATE Role SET ParentRoleID = NULL WHERE ParentRoleID = @RoleID", conn))
                {
                    cmd.Parameters.AddWithValue("@RoleID", roleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public static void UpdateRole(Role role)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = @"UPDATE Role SET Role_Name   = @Role_Name,
                            Description = @Description,
                            Updated_by  = @Updated_by,
                            Updated_Date= @Updated_Date
                            WHERE RoleID   = @RoleID";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleID", role.RoleID);
                        cmd.Parameters.AddWithValue("@Role_Name", role.Role_Name);
                        cmd.Parameters.AddWithValue("@Description", role.Description ?? "");
                        cmd.Parameters.AddWithValue("@Updated_by", role.Updated_by ?? "");
                        cmd.Parameters.AddWithValue("@Updated_Date", role.Updated_Date);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new Exception($"No role found with ID {role.RoleID}. Update failed.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void UpdateRoleStatus(int roleId, string status)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = "UPDATE Role SET Status = @Status, Updated_Date = @UpdatedDate WHERE RoleID = @RoleID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@RoleID", roleId);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            
            }
        }

        public static void InsertMultipleRoles(List<Role> roleList)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction()) // Use transaction for efficiency
                    {
                        try
                        {
                            string sql = @"INSERT INTO Role 
                                (Role_Name, Description, Created_by, Created_Date, Updated_by, Updated_Date, Status)
                                VALUES
                                (@Role_Name, @Description, @Created_by, @Created_Date, @Updated_by, @Updated_Date, @Status);
                                ";

                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                foreach (var role in roleList)
                                {
                                    cmd.Parameters.Clear(); // Clear previous parameters
                                    cmd.Parameters.AddWithValue("@RoleID", role.RoleID);
                                    cmd.Parameters.AddWithValue("@Role_Name", role.Role_Name);
                                    cmd.Parameters.AddWithValue("@Description", role.Description ?? "");
                                    cmd.Parameters.AddWithValue("@Created_by", role.Created_by);
                                    cmd.Parameters.AddWithValue("@Created_Date", role.Created_Date.ToString("yyyy-MM-dd HH:mm:ss"));
                                    cmd.Parameters.AddWithValue("@Updated_by", role.Updated_by ?? "");
                                    cmd.Parameters.AddWithValue("@Updated_Date", role.Updated_Date?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                                    cmd.Parameters.AddWithValue("@Status", role.Status);

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit(); // Commit all inserts at once
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static Role GetRoleById(int roleId)
        {
            Role role = null;
            try
            {

                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = "SELECT * FROM Role WHERE RoleID = @RoleID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleID", roleId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // only read the first record
                            {
                                role = new Role
                                {
                                    RoleID = Convert.ToInt32(reader["RoleID"].ToString()),
                                    Role_Name = reader["Role_Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
                                    Created_by = reader["Created_by"].ToString(),
                                    Updated_Date = Convert.ToDateTime(reader["Updated_Date"].ToString()),
                                    Updated_by = reader["Updated_by"].ToString(),
                                    Status = reader["Status"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            
            }
            return role; // will return null if no record found
        }

        //public static List<Role> GetAllRole()
        //{
        //    var list = new List<Role>();
        //    try
        //    {
        //        using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
        //        {
        //            conn.Open();

        //            string sql = @"SELECT  * FROM Role Where Role_Name<>'System Administrator'";

        //            using (var cmd = new SQLiteCommand(sql, conn))
        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    list.Add(new Role
        //                    {
        //                        RoleID = Convert.ToInt32(reader["RoleID"].ToString()),
        //                        Role_Name = reader["Role_Name"].ToString(),
        //                        Description = reader["Description"].ToString(),
        //                        Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
        //                        Created_by = reader["Created_by"].ToString(),
        //                        Updated_Date = Convert.ToDateTime(reader["Updated_Date"].ToString()),
        //                        Updated_by = reader["Updated_by"].ToString(),
        //                        Status = reader["Status"].ToString()
        //                    });
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    list= list.OrderBy(r => r.ParentRoleID ?? r.RoleID).ThenBy(r => r.ParentRoleID).ThenBy(r => r.PriorityIndex).ToList();
        //    return list;
        //}
        public static List<Role> GetAllRole()
        {
            var list = new List<Role>();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = @"SELECT  * FROM Role Where Role_Name<>'System Administrator'";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Role
                            {
                                RoleID = Convert.ToInt32(reader["RoleID"].ToString()),
                                Role_Name = reader["Role_Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
                                Created_by = reader["Created_by"].ToString(),
                                Updated_Date = reader["Updated_Date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Updated_Date"]),
                                Updated_by = reader["Updated_by"].ToString(),
                                Status = reader["Status"].ToString(),
                                ParentRoleID = reader["ParentRoleID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ParentRoleID"]),
                                PriorityIndex = reader["PriorityIndex"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PriorityIndex"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // build a dictionary for quick lookup
            var byId = list.ToDictionary(r => r.RoleID);

            // cache roots to avoid repeated walks
            var rootCache = new Dictionary<int, Role>();

            Role GetRoot(Role r)
            {
                if (r == null) return null;
                if (rootCache.TryGetValue(r.RoleID, out var cached)) return cached;

                var cur = r;
                while (cur.ParentRoleID.HasValue && byId.TryGetValue(cur.ParentRoleID.Value, out var parent))
                {
                    cur = parent;
                }

                rootCache[r.RoleID] = cur;
                return cur;
            }

            // order by root.PriorityIndex (so root ordering follows PriorityIndex),
            // then group by parent (COALESCE(ParentRoleID, RoleID)), then by PriorityIndex and RoleName
            var ordered = list
                .OrderBy(r => {
                    var root = GetRoot(r);
                    return root != null ? root.PriorityIndex : int.MaxValue;
                })
                .ThenBy(r => r.ParentRoleID ?? r.RoleID)    // COALESCE(ParentRoleID, RoleID)
                .ThenBy(r => r.ParentRoleID)                // ParentRoleID (nullable)
                .ThenBy(r => r.PriorityIndex)               // PriorityIndex
                .ThenBy(r => r.Role_Name)
                .ToList();

            return ordered;
        }

        public static List<Role> GetActiveRoles()
        {

            var list = new List<Role>();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = "SELECT  * FROM Role where Status=@Status";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", "Active");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new Role
                                {
                                    RoleID = Convert.ToInt32(reader["RoleID"].ToString()),
                                    Role_Name = reader["Role_Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
                                    Created_by = reader["Created_by"].ToString(),
                                    Updated_Date = Convert.ToDateTime(reader["Updated_Date"].ToString()),
                                    Updated_by = reader["Updated_by"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    PriorityIndex = Convert.ToInt32(reader["PriorityIndex"].ToString())
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

            return list;
        }
        public static List<Role> GetActiveRoleBasedOnSequence()
        {
            var roles = new List<Role>();

            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    //    string sql = @"
                    //SELECT *
                    //FROM Role
                    //WHERE Status = 'Active'
                    //ORDER BY COALESCE(ParentRoleID, RoleID), ParentRoleID, PriorityIndex;";

                    string sql = @"
                SELECT *
                FROM Role
                ORDER BY COALESCE(ParentRoleID, RoleID), ParentRoleID, PriorityIndex;";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                roles.Add(new Role
                                {
                                    RoleID = Convert.ToInt32(reader["RoleID"]),
                                    Role_Name = reader["Role_Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
                                    Created_by = reader["Created_by"].ToString(),
                                    Updated_Date = Convert.ToDateTime(reader["Updated_Date"].ToString()),
                                    Updated_by = reader["Updated_by"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    ParentRoleID = reader["ParentRoleID"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["ParentRoleID"]),
                                    PriorityIndex = reader["PriorityIndex"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PriorityIndex"])
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

            return roles;
        }

        public static bool RoleNameExists(string roleName)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(1) FROM Role WHERE Role_Name = @RoleName  COLLATE NOCASE";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleName", roleName);
                        var count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static bool RoleNameExists(string roleName,int RoleId)
        {
            try { 
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(1) FROM Role WHERE Role_Name = @RoleName COLLATE NOCASE and RoleID!=@RoleID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleName", roleName);
                        cmd.Parameters.AddWithValue("@RoleID", RoleId);
                        var count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                    }
            }catch (Exception ex)
            {
                throw ex;
            }
}
    }

}
