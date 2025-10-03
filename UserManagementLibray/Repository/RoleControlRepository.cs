using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using UserManagementlibrary.Entity;

namespace UserManagementlibrary.Repository
{


    public static class RoleControlRepository
    {
        //private static string dbFile = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName,"users.db");
        private static string dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];
        public static void InsertRoleControls(List<RoleControl> rcList)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string sql = @"INSERT INTO RoleControl (RoleId, MenuId, Created_by, Created_Date, Updated_by, Updated_Date) VALUES (@RoleId, @MenuId, @Created_by, @Created_Date, @Updated_by, @Updated_Date);";

                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                foreach (var rc in rcList)
                                {
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("@RoleId", rc.RoleId);
                                    cmd.Parameters.AddWithValue("@MenuId", rc.MenuId);
                                    cmd.Parameters.AddWithValue("@Created_by", rc.Created_by);
                                    cmd.Parameters.AddWithValue("@Created_Date", rc.Created_Date.ToString("yyyy-MM-dd HH:mm:ss"));
                                    cmd.Parameters.AddWithValue("@Updated_by", rc.Updated_by ?? "");
                                    cmd.Parameters.AddWithValue("@Updated_Date", rc.Updated_Date?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
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
                MessageBox.Show(ex.Message, "InsertRoleControls Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        public static List<RoleControl> GetRoleControls()
        {
            var list = new List<RoleControl>();

            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();

                string sql = "SELECT  * FROM RoleControl";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new RoleControl
                        {
                            RoleId = Convert.ToInt32(reader["RoleId"].ToString()),
                            MenuId= Convert.ToInt32(reader["MenuId"].ToString())
                        });
                    }
                }
            }

            return list;
        }
        public static List<RoleControl> GetRoleControlsByRoleId(int roleId)
        {
            var list = new List<RoleControl>();
            try
            {

                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = "SELECT * FROM RoleControl WHERE RoleId = @RoleId";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleId", roleId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new RoleControl
                                {
                                    RoleId = Convert.ToInt32(reader["RoleId"]),
                                    MenuId = Convert.ToInt32(reader["MenuId"]),
                                    Created_by = reader["Created_by"].ToString(),
                                    Created_Date = Convert.ToDateTime(reader["Created_Date"]),
                                    Updated_by = reader["Updated_by"] != DBNull.Value ? reader["Updated_by"].ToString() : null,
                                    Updated_Date = reader["Updated_Date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["Updated_Date"]) : null
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
        public static List<RoleControl> GetRoleControlsByRoleIds(List<int> roleList,string menuName)
        {
            var list = new List<RoleControl>();

            if (roleList == null || roleList.Count == 0)
                return list; // No roles provided

            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                var paramNames = new List<string>();
                for (int i = 0; i < roleList.Count; i++)
                    paramNames.Add($"@RoleId{i}");

                string sql = $"SELECT * FROM RoleControl WHERE RoleId IN ({string.Join(",", paramNames)})";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    for (int i = 0; i < roleList.Count; i++)
                        cmd.Parameters.AddWithValue(paramNames[i], roleList[i]);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new RoleControl
                            {
                                RoleId = Convert.ToInt32(reader["RoleId"]),
                                MenuId = Convert.ToInt32(reader["MenuId"]),
                                Created_by = reader["Created_by"].ToString(),
                                Created_Date = Convert.ToDateTime(reader["Created_Date"]),
                                Updated_by = reader["Updated_by"] != DBNull.Value ? reader["Updated_by"].ToString() : null,
                                Updated_Date = reader["Updated_Date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["Updated_Date"]) : null
                            });
                        }
                    }
                }
            }

            return list;
        }
        public static bool HasMonitoringAuditLogAccess(List<int> roleList, string parentMenu, string childMenu)
        {
            try
            {
                if (roleList == null || roleList.Count == 0)
                    return false;

                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    // Create placeholders for each role id
                    var paramNames = new List<string>();
                    for (int i = 0; i < roleList.Count; i++)
                        paramNames.Add($"@RoleId{i}");

                    string sql = $@"SELECT 1 FROM RoleControl rc INNER JOIN MenuItems mi ON rc.MenuId = mi.MenuID WHERE rc.RoleId IN ({string.Join(",", paramNames)})
                                  AND mi.Parent_Menu COLLATE NOCASE = @ParentMenu
                                  AND mi.Child_Menu COLLATE NOCASE = @ChildMenu
                                  LIMIT 1";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        // Add role id parameters
                        for (int i = 0; i < roleList.Count; i++)
                            cmd.Parameters.AddWithValue(paramNames[i], roleList[i]);

                        // Add menu filter parameters
                        cmd.Parameters.AddWithValue("@ParentMenu", parentMenu);
                        cmd.Parameters.AddWithValue("@ChildMenu", childMenu);

                        var result = cmd.ExecuteScalar();
                        return result != null; // True if found
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        public static void UpdateRoleControlsByMenuIds(int roleId, List<int> menuIdsToDelete,List<int> menuIdsToUpdate, string createdBy)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string deleteSql = $"DELETE FROM RoleControl WHERE RoleId = @RoleId AND MenuId IN ({string.Join(",", menuIdsToDelete)})";
                            using (var deleteCmd = new SQLiteCommand(deleteSql, conn))
                            {
                                deleteCmd.Parameters.AddWithValue("@RoleId", roleId);
                                deleteCmd.ExecuteNonQuery();
                            }

                            foreach (var menuId in menuIdsToUpdate)
                            {
                                using (var insertCmd = new SQLiteCommand(
                                    "INSERT INTO RoleControl (RoleId, MenuId, Created_by, Created_Date) VALUES (@RoleId, @MenuId, @Created_by, @Created_Date)", conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@RoleId", roleId);
                                    insertCmd.Parameters.AddWithValue("@MenuId", menuId);
                                    insertCmd.Parameters.AddWithValue("@Created_by", createdBy);
                                    insertCmd.Parameters.AddWithValue("@Created_Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    insertCmd.Parameters.AddWithValue("@Updated_by", createdBy);
                                    insertCmd.Parameters.AddWithValue("@Updated_Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    insertCmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
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


    }

}
