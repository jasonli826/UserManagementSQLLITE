using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using UserManagementlibrary.Entity;

namespace UserManagementOnSQLLite.Repository
{


    public static class MenuItemRepository
    {
        private static string dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];
        public static void InsertMenuItems(List<MenuItem> menuItems)
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
                            string sql = @"INSERT INTO MenuItems (Parent_Menu, Child_Menu, Sno) VALUES (@Parent_Menu, @Child_Menu, @Sno);";

                            using (var cmd = new SQLiteCommand(sql, conn))
                            {
                                foreach (var menuItem in menuItems)
                                {
                                    cmd.Parameters.Clear(); // Clear previous parameters
                                    cmd.Parameters.AddWithValue("@Parent_Menu", menuItem.Parent_Menu);
                                    cmd.Parameters.AddWithValue("@Child_Menu", menuItem.Child_Menu);
                                    cmd.Parameters.AddWithValue("@Sno", menuItem.Sno.HasValue ? (object)menuItem.Sno.Value : DBNull.Value);

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
                MessageBox.Show(ex.Message, "InsertAudit Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        public static List<MenuItem> GetAllMenuItems()
        {
            
            var list = new List<MenuItem>();
            try
            {

                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = "SELECT  * FROM MenuItems";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new MenuItem
                            {
                                MenuID = Convert.ToInt32(reader["MenuID"].ToString()),
                                Parent_Menu = reader["Parent_Menu"].ToString(),
                                Child_Menu = reader["Child_Menu"].ToString(),
                                Sno = Convert.ToInt32(reader["Sno"].ToString())
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
