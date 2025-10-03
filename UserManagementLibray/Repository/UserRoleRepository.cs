using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementlibrary.Entity;

namespace UserManagementlibrary.Repository
{
    public static class UserRoleRepository
    {
        //private static string dbFile = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName,"users.db");
        private static string dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];
        public static void InsertUserRole(UserRole ur)
        {
            InsertUserRoles(new List<UserRole> { ur });
        }

        public static void InsertUserRoles(List<UserRole> userRoles)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    string sql = @"
                INSERT INTO UserRole (UserId, RoleId)
                VALUES (@UserId, @RoleId);
                ";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        foreach (var ur in userRoles)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@UserId", ur.UserId);
                            cmd.Parameters.AddWithValue("@RoleId", ur.RoleId);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }
        }
        public static void DeleteUserRolesByUserId(string userId)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();

                string sql = "DELETE FROM UserRole WHERE UserId = @UserId";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<UserRole> GetUserRoles()
        {
            var list = new List<UserRole>();

            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();

                string sql = "SELECT  * FROM UserRole";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new UserRole
                        {
                            RoleId = Convert.ToInt32(reader["RoleId"].ToString()),
                            UserId = reader["UserId"].ToString()
                        });
                    }
                }
            }

            return list;
        }
        public static List<UserRole> GetUserRolesById(string userId)
        {
            var list = new List<UserRole>();

            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();

                string sql = "SELECT ur.* FROM UserRole ur Left Join Role r on ur.RoleId=r.RoleID  WHERE UserId = @UserId  COLLATE NOCASE";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    // Add parameter to prevent SQL injection
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new UserRole
                            {
                                RoleId = Convert.ToInt32(reader["RoleId"].ToString()),
                                UserId = reader["UserId"].ToString() // or keep string if your class uses string
                            });
                        }
                    }
                }
            }

            return list;
        }

    }

}
