using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UserManagementlibrary.Entity;
using UserManagementlibrary.Helpers;
using UserManagementlibrary.Log;

namespace UserManagementlibrary.Repository
{
    public class UserAuthenticationRepository
    {
        private static string _dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];

        public static int Login(string username, string password,ref string result)
        {
            int retVal = 1;
            int domainId = 0;
            ApiLogger.Log("UserAuthentication", $"Starting To Call UserRepository Login Method Username={username} Password={password}");

            if (string.IsNullOrWhiteSpace(username))
            {
                result = "Username cannot be empty.";
                return retVal;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                result= "Password cannot be empty.";
                return retVal;
            }

            try
            {
                username = username.ToLower();
                LoginUser user = new LoginUser
                {
                    AccessibleMenus = new List<MenuItem>()
                };

                using (var conn = new SQLiteConnection($"Data Source={_dbFile};Version=3;"))
                {
                    conn.Open();
                    // 1. Get user info
                    string sqlUser = "SELECT * FROM User_tbl WHERE UserId = @UserId LIMIT 1";
                    using (var cmd = new SQLiteCommand(sqlUser, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", username);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                result = "User not found.";
                                ApiLogger.Log("UserAuthentication", result);
                                return retVal;
                            }

                            string storedPassword = reader["Password"].ToString();
                            string decryptedPassword = CryptoHelper.Decrypt(storedPassword);

                            if (decryptedPassword != password)
                            {
                                result = "Incorrect password.";
                                ApiLogger.Log("UserAuthentication", result);
                                return retVal;
                            }

                            if (!reader["Status"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase))
                            {
                                result = "User account is inactive.";
                                ApiLogger.Log("UserAuthentication", result);
                                return retVal;
                            }
                            domainId = Convert.ToInt32(reader["Domain"].ToString());
                        }
                    }
                    // 1.5. Check domain status
                    if (domainId!=0)
                    {
                        string sqlDomain = "SELECT Status FROM Domain WHERE DomainID = @DomainID LIMIT 1";
                        using (var cmdDomain = new SQLiteCommand(sqlDomain, conn))
                        {
                            cmdDomain.Parameters.AddWithValue("@DomainID", domainId);
                            object domainStatus = cmdDomain.ExecuteScalar();

                            if (domainStatus == null)
                            {
                                result = $"User Domain not found.";
                                ApiLogger.Log("UserAuthentication", result);
                                return retVal;
                            }

                            if (!domainStatus.ToString().Equals("Active", StringComparison.OrdinalIgnoreCase))
                            {
                                result = $"This User Domain is inactive. Cannot login.";
                                ApiLogger.Log("UserAuthentication", result);
                                return retVal;
                            }
                        }
                    }

                    // 2. Get role IDs
                    string sqlRoles = @"SELECT r.RoleID, r.Role_Name,r.Status
                                        FROM UserRole ur
                                        JOIN Role r ON ur.RoleId = r.RoleID
                                        WHERE ur.UserId = @UserId COLLATE NOCASE";

                    List<int> roleIds = new List<int>();
                    List<string> userRoles = new List<string>();
    
                    using (var cmdRoles = new SQLiteCommand(sqlRoles, conn))
                    {
                        cmdRoles.Parameters.AddWithValue("@UserId", username);
                        using (var readerRoles = cmdRoles.ExecuteReader())
                        {
                            while (readerRoles.Read())
                            {
                                if (!readerRoles["Status"].ToString().Equals("Active", StringComparison.OrdinalIgnoreCase))
                                {
                                    result = $"Role '{readerRoles["Role_Name"]}' is inactive. Cannot login.";
                                    ApiLogger.Log("UserAuthentication", result);
                                    return retVal;
                                }

                                roleIds.Add(Convert.ToInt32(readerRoles["RoleID"]));
                     
                                userRoles.Add(readerRoles["Role_Name"].ToString());
                            }
                        }
                    }

                    if (!roleIds.Any())
                    {
                        result = "User has no roles assigned.";
                        ApiLogger.Log("UserAuthentication", result);
                        return retVal;
                    }
                    SessionContext.RoleIds = roleIds;
                    string userRolesStr = string.Join("|", userRoles);
                    SessionContext.UserRole = userRolesStr;
                    var roleIdsCsv = string.Join(",", roleIds);
                    ApiLogger.Log("UserAuthentication", $"GetRoles And RoleIds={roleIdsCsv}");
                    retVal = 0;
                    // 3. Get menus, remove duplicates
                    string sqlMenu = $@"SELECT m.MenuID, m.Parent_Menu, m.Child_Menu, m.Sno
                                        FROM RoleControl rc
                                        JOIN MenuItems m ON rc.MenuId = m.MenuID
                                        WHERE rc.RoleId IN ({roleIdsCsv})
                                        ORDER BY m.MenuID";

                    using (var cmdMenu = new SQLiteCommand(sqlMenu, conn))
                    {
                        using (var readerMenu = cmdMenu.ExecuteReader())
                        {
                            var menuSet = new HashSet<int>(); // Track unique MenuID
                            while (readerMenu.Read())
                            {
                                int menuId = Convert.ToInt32(readerMenu["MenuID"]);
                                if (!menuSet.Contains(menuId))
                                {
                                    menuSet.Add(menuId);
                                    user.AccessibleMenus.Add(new MenuItem
                                    {
                                        Parent_Menu = readerMenu["Parent_Menu"].ToString(),
                                        Child_Menu = readerMenu["Child_Menu"].ToString(),
                                        Sno = Convert.ToInt32(readerMenu["Sno"].ToString())
                                    });
                                }
                            }
                        }
                    }
                    if (userRolesStr.ToUpper().Contains("System Administrator".ToUpper()))
                    {
                        user.AccessibleMenus.Add(new MenuItem
                        {
                            Parent_Menu = "User Management",
                            Child_Menu = "User Creation",
                            Sno = 1
                        });
                        user.AccessibleMenus.Add(new MenuItem
                        {
                            Parent_Menu = "User Management",
                            Child_Menu = "RoleAdministration",
                            Sno = 2
                        });
                        user.AccessibleMenus.Add(new MenuItem
                        {
                            Parent_Menu = "User Management",
                            Child_Menu = "DomainControl",
                            Sno = 3
                        });
                        user.AccessibleMenus.Add(new MenuItem
                        {
                            Parent_Menu = "User Management",
                            Child_Menu = "ChangePassword",
                            Sno = 4
                        });
                    }
                    else if (userRolesStr.ToUpper() != "Operator".ToUpper())
                    {
                        user.AccessibleMenus.Add(new MenuItem
                        {
                            Parent_Menu = "User Management",
                            Child_Menu = "User Creation",
                            Sno = 1
                        });
                        user.AccessibleMenus.Add(new MenuItem
                        {
                            Parent_Menu = "User Management",
                            Child_Menu = "ChangePassword",
                            Sno = 2
                        });
                    }
                    else
                    {
                        user.AccessibleMenus.Add(new MenuItem
                        {
                            Parent_Menu = "User Management",
                            Child_Menu = "ChangePassword",
                            Sno = 1
                        });
                    }
                }
        

                ApiLogger.Log("UserAuthentication", "Get MenuList And Starting to convert to xml string");
                XElement xml = new XElement("Menus",
                    user.AccessibleMenus.Select(m =>
                        new XElement("Menu",
                            new XElement("Parent_Menu", m.Parent_Menu),
                            new XElement("Child_Menu", m.Child_Menu),
                            new XElement("Sno", m.Sno)
                        )
                    )
                );

                ApiLogger.Log("UserAuthentication", "MenuList:" + xml.ToString());
                result = xml.ToString();
                return retVal;
            }
            catch (Exception ex)
            {
                retVal = 1;
                result = ex.Message;
                ApiLogger.Log("UserAuthentication", "Get Exception Message:" + result);
                return retVal;
            }
        }
    }
}
