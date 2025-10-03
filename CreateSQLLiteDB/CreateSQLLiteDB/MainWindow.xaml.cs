using CreateSQLLiteDB.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace CreateSQLLiteDB
{
    public partial class MainWindow : Window
    {
        private string DatabaseFilePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select folder to store SQLite database";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtFolderPath.Text = dialog.SelectedPath;
                }
            }
        }

        // Create Database
        private void CreateDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtDatabaseName.Text))
                {
                    System.Windows.MessageBox.Show("Please enter a database name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtFolderPath.Text))
                {
                    System.Windows.MessageBox.Show("Please select a folder.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Build full path with .db extension
                DatabaseFilePath = Path.Combine(txtFolderPath.Text, $"{txtDatabaseName.Text}.db");

                if (System.IO.File.Exists(DatabaseFilePath))
                {
                    System.Windows.MessageBox.Show("Database already exists!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SQLiteConnection.CreateFile(DatabaseFilePath);

                System.Windows.MessageBox.Show($"Database created successfully at:\n{DatabaseFilePath}",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create Table
        private void CreateTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(DatabaseFilePath) || !System.IO.File.Exists(DatabaseFilePath))
                {
                    System.Windows.MessageBox.Show("Please create the database first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var conn = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
                {
                    conn.Open();

                    // ------------------ Tables ------------------
                    // ------------------ Tables ------------------
                    var tables = new string[]
                    {
                    @"CREATE TABLE IF NOT EXISTS Domain(
                        DomainID INTEGER PRIMARY KEY,
                        DomainName TEXT NOT NULL,
                        CN TEXT,
                        DC1 TEXT,
                        DC2 TEXT,
                        DC3 TEXT,
                        Description TEXT,
                        Created_by TEXT NOT NULL DEFAULT 'System',
                        Created_Date TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                        Updated_by TEXT DEFAULT 'System',
                        Updated_Date TEXT DEFAULT (CURRENT_TIMESTAMP),
                        Status TEXT,
                        DomainNme TEXT
                    );",

                    @" CREATE TABLE IF NOT EXISTS User_tbl(
                        ID INTEGER PRIMARY KEY,
                        UserId TEXT NOT NULL UNIQUE,
                        Empid TEXT,
                        UserName TEXT NOT NULL,
                        Password TEXT,
                        Domain INTEGER NOT NULL,
                        Created_by TEXT NOT NULL DEFAULT 'System',
                        Created_Date TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                        Updated_by TEXT DEFAULT 'System',
                        Updated_Date TEXT DEFAULT (CURRENT_TIMESTAMP),
                        Pwd_Change_Date TEXT,
                        Last_Login_Date TEXT,
                        Logout_Date TEXT,
                        Status TEXT NOT NULL,
                        Remarks TEXT
                    );",

                    @"CREATE TABLE IF NOT EXISTS Role(
                        RoleID INTEGER PRIMARY KEY,
                        Role_Name TEXT NOT NULL UNIQUE,
                        Description TEXT,
                        Created_by TEXT NOT NULL DEFAULT 'System',
                        Created_Date TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                        Updated_by TEXT DEFAULT 'System',
                        Updated_Date TEXT DEFAULT (CURRENT_TIMESTAMP),
                        Status TEXT
                    );",

                    @"CREATE TABLE IF NOT EXISTS MenuItems(
                        MenuID INTEGER PRIMARY KEY,
                        Parent_Menu TEXT NOT NULL,
                        Child_Menu TEXT NOT NULL,
                        Sno INTEGER
                    );",

                    @"CREATE TABLE IF NOT EXISTS RoleControl(
                        RoleId INTEGER NOT NULL,
                        MenuId INTEGER NOT NULL,
                        Created_by TEXT NOT NULL DEFAULT 'System',
                        Created_Date TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                        Updated_by TEXT DEFAULT 'System',
                        Updated_Date TEXT DEFAULT (CURRENT_TIMESTAMP),
                        PRIMARY KEY (RoleId, MenuId)
                    );",

                    @"CREATE TABLE IF NOT EXISTS UserRole(
                        UserId TEXT NOT NULL,
                        RoleId INTEGER NOT NULL,
                        Created_Date TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                        Created_by TEXT NOT NULL DEFAULT 'System',
                        PRIMARY KEY (UserId, RoleId)
                    );",
                 @"CREATE TABLE IF NOT EXISTS Audit (
                    AuditID        INTEGER PRIMARY KEY,
                    AuditDate      TEXT NOT NULL,
                    UserName       TEXT NOT NULL,
                    Detail         TEXT,
                    Created_by     TEXT DEFAULT 'System',
                    Created_Date TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                    Updated_by     TEXT DEFAULT 'System',
                    Updated_Date   TEXT DEFAULT (CURRENT_TIMESTAMP)
                );",
                 @"CREATE TABLE IF NOT EXISTS Alarm (
                    ID               INTEGER PRIMARY KEY,
                    Alarm            TEXT NOT NULL,
                    Alarm_Description TEXT,
                    RaiseTime        TEXT,
                    AcknowledgeTime  TEXT,
                    Created_by       TEXT DEFAULT 'System',
                    Created_Date TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                    Updated_by       TEXT DEFAULT 'System',
                    Updated_Date     TEXT DEFAULT (CURRENT_TIMESTAMP)
                );",

                  @"
                CREATE TABLE IF NOT EXISTS DataAnalytic (
                    ID            INTEGER PRIMARY KEY,
                    Date          TEXT NOT NULL,
                    PanelCountPass     INTEGER DEFAULT 0,
                    PanelCountFail     INTEGER DEFAULT 0,
                    PanelCountRework   INTEGER DEFAULT 0,
                    Created_by    TEXT DEFAULT 'System',
                    Created_Date TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
                    Updated_by    TEXT DEFAULT 'System',
                    Updated_Date   TEXT DEFAULT (CURRENT_TIMESTAMP)
                );"
                };


                    foreach (var sql in tables)
                    {
                        using (var cmd = new SQLiteCommand(sql, conn))
                            cmd.ExecuteNonQuery();
                    }

                    // ------------------ Indexes ------------------
                    // ------------------ Indexes ------------------
                    var indexes = new string[]
                    {
                    // Domain
                    "CREATE INDEX IF NOT EXISTS idx_Domain_Name_Status ON Domain(DomainName, Status);",

                    // User
                    "CREATE INDEX idx_User_All ON User_tbl(UserId, UserName, Empid, Domain, Status);",

                    // Role
                    "CREATE INDEX IF NOT EXISTS idx_Role_Name_Status ON Role(Role_Name, Status);",

                    // MenuItems
                    "CREATE INDEX IF NOT EXISTS idx_Menu_Parent_Child ON MenuItems(Parent_Menu, Child_Menu);",

                    // RoleControl
                    "CREATE INDEX IF NOT EXISTS idx_RoleControl_Role_Menu ON RoleControl(RoleId, MenuId);",

                    // UserRole
                    "CREATE INDEX IF NOT EXISTS idx_UserRole_User_Role ON UserRole(UserId, RoleId);",
                    
                    // Audit
                    "CREATE INDEX IF NOT EXISTS idx_Audit_UserName_AuditDate ON Audit(UserName, AuditDate);",

                    // Alarm
                    "CREATE INDEX IF NOT EXISTS idx_Alarm_Alarm_RaiseTime ON Alarm(Alarm, RaiseTime);",
                    "CREATE INDEX IF NOT EXISTS idx_Alarm_AcknowledgeTime ON Alarm(AknowledgeTime);",
                    // DataAnalytic
                    "CREATE INDEX IF NOT EXISTS idx_DataAnalytic_Date ON DataAnalytic(Date);"
                    };


                    foreach (var sql in indexes)
                    {
                        using (var cmd = new SQLiteCommand(sql, conn))
                            cmd.ExecuteNonQuery();
                    }
                }

                System.Windows.MessageBox.Show("Tables and indexes created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void InsertDefaultData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(DatabaseFilePath) || !System.IO.File.Exists(DatabaseFilePath))
                {
                    System.Windows.MessageBox.Show("Please create the database and table first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                CreateNewDomainName();
                CreateMenuItems();
                CreateNewUser();
                CreateNewRole();
                CreateUserRoles();
                CreateRoleControls();
                System.Windows.MessageBox.Show("Default data inserted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
               
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateUserRoles()
        {
            var userRoles = new List<UserRole>
            {
                new UserRole { UserId = "system", RoleId = 1 }
            };
            using (var conn = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
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
                    catch (Exception ex)
                    {
                        if(transaction!=null)
                        transaction.Rollback();
                    }
                }
            }
        }
        private  void CreateRoleControls()
        {
            var roleControls = new List<RoleControl>();
            int roleId = 1;

            for (int menuId = 1; menuId <= 63; menuId++)
            {
                roleControls.Add(new RoleControl
                {
                    RoleId = roleId,
                    MenuId = menuId,
                    Created_by = "system",
                    Created_Date = DateTime.Now,
                    Updated_by = "system",
                    Updated_Date = DateTime.Now
                });
            }

            using (var conn = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    string sql = @"
                                    INSERT INTO RoleControl (RoleId, MenuId, Created_by, Created_Date, Updated_by, Updated_Date)
                                    VALUES (@RoleId, @MenuId, @Created_by, @Created_Date, @Updated_by, @Updated_Date);
                                    ";
                    try
                    {

                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            foreach (var rc in roleControls)
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
                        if (transaction != null) transaction.Rollback();
                    }
                }
            }
        }

        private  void CreateMenuItems()
        {
            List<Entity.MenuItem> menuList = new List<Entity.MenuItem>();
            int  sno = 1;
            // --- Product Menu ---
            var productMenus = new List<string>
            {
                "Run Product",
                "Edit Product",
                "Load Product",
                "New Product",
                "Save As Product"
            };

        
            foreach (var child in productMenus)
            {
                menuList.Add(new Entity.MenuItem
                {
                    Parent_Menu = "Product",
                    Child_Menu = child,
                    Sno = sno++
                });
            }

            // --- Tool Menu ---
            var toolMenus = new List<string>
            {
                "Edit Tool Info",
                "Reset Tool Life",
                "Tool Life Statistic",
                "Reset Vacuum Filter Data"
            };

            sno = 1;
            foreach (var child in toolMenus)
            {
                menuList.Add(new Entity.MenuItem
                {
                    Parent_Menu = "Tool",
                    Child_Menu = child,
                    Sno = sno++
                });
            }

            // --- Utility Menu ---
            var utilityMenus = new List<string>
            {
                "IO Utility",
                "Home Machine",
                "Robot MotionUtility",
                "Display Alarm File",
                "Camera Spindle Offset",
                "Camera Barcode Offset",
                "Camera Setting",
                "Gripper Camera Setting",
                "Gripper Barcode Setting"
            };

            sno = 1;
            foreach (var child in utilityMenus)
            {
                menuList.Add(new Entity.MenuItem
                {
                    Parent_Menu = "Utility",
                    Child_Menu = child,
                    Sno = sno++
                });
            }

            // --- Library Menu ---
            var libraryMenus = new List<string>
            {
                "Toolbit Station",
                "Gripper Fingers"
            };

            sno = 1;
            foreach (var child in libraryMenus)
            {
                menuList.Add(new Entity.MenuItem
                {
                    Parent_Menu = "Library",
                    Child_Menu = child,
                    Sno = sno++
                });
            }

            // --- Service Menu ---
            var serviceMenus = new List<string>
            {
                "Robot Points",
                "Robot Software Limits",
                "Old Robot Range",
                "Robot Working Range",
                "Right Robot Working Range",
                "Table Calibration",
                "Right Table Calibration",
                "Table Tool Station",
                "Right Table Tool Station",
                "System Option",
                "Motor Calibration",
                "Robot IO Utility",
                "Video Setup",
                "Password Maintenance",
                "Input Barcode ComPort",
                "Machine Activation",
                "Languages Selection",
                "Spindle Inverter ComPort",
                "Server Connection",
                "Modbus Motion Com Port",
                "Modbus IO Com Port",
                "Gripper Calibration",
                "Gripper Jaws Station",
                "Gripper Barcode Com Port",
                "Gripper Table Working Range",
                "Dust Blow Working Range",
                "Dust Blow Reference",
                "Output1 Reference",
                "Output2 Reference",
                "Output1 Working Range",
                "Output2 Working Range"
            };

            sno = 1;
            foreach (var child in serviceMenus)
            {
                menuList.Add(new Entity.MenuItem
                {
                    Parent_Menu = "Service",
                    Child_Menu = child,
                    Sno = sno++
                });
            }

            // --- System Menu ---
            var systemMenus = new List<string>
            {
                "Axis 0 Data",
                "Axis 1 Data",
                "Axis 2 Data",
                "Axis 3 Data",
                "Axis 4 Data",
                "Axis 5 Data",
                "Axis 6 Data",
                "Axis 7 Data",
                "Galil Terminal",
                "System Configuration",
                "Edit Modbus Parameters",
                "Show Modbus Motion Test"
            };

            sno = 1;
            foreach (var child in systemMenus)
            {
                menuList.Add(new Entity.MenuItem
                {
                    Parent_Menu = "System",
                    Child_Menu = child,
                    Sno = sno++
                });
            }

            using (var conn = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction()) // Use transaction for efficiency
                {
                    string sql = @"
                                INSERT INTO MenuItems (Parent_Menu, Child_Menu, Sno)
                                VALUES (@Parent_Menu, @Child_Menu, @Sno);
                                ";
                    try
                    {
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            foreach (var menuItem in menuList)
                            {
                                cmd.Parameters.Clear(); // Clear previous parameters
                                cmd.Parameters.AddWithValue("@Parent_Menu", menuItem.Parent_Menu);
                                cmd.Parameters.AddWithValue("@Child_Menu", menuItem.Child_Menu);
                                cmd.Parameters.AddWithValue("@Sno", menuItem.Sno.HasValue ? (object)menuItem.Sno.Value : DBNull.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit(); 
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null) transaction.Rollback();
                    
                    }
                }
            }


        }
        private  void CreateNewRole()
        {
            var roleList = new List<Role>
            {
                new Role { RoleID = 1, Role_Name = "System Administrator",Description="SYSTEM AUTO CREATION ROLE", Created_by = "system", Created_Date = DateTime.Now,Updated_by="system",Updated_Date=DateTime.Now,Status="Active" },
                new Role { RoleID = 2, Role_Name = "Service",Description=string.Empty, Created_by = "system", Created_Date = DateTime.Now,Updated_by="system",Updated_Date=DateTime.Now ,Status="Active"},
                new Role { RoleID = 3, Role_Name = "Engineer",Description=string.Empty, Created_by = "system", Created_Date = DateTime.Now,Updated_by="system",Updated_Date=DateTime.Now ,Status="Active"},
                new Role { RoleID = 4, Role_Name = "Technician",Description=string.Empty, Created_by = "system", Created_Date = DateTime.Now,Updated_by="system",Updated_Date=DateTime.Now,Status="Active" },
                new Role { RoleID = 5, Role_Name = "Operator",Description=string.Empty, Created_by = "system", Created_Date = DateTime.Now,Updated_by="system",Updated_Date=DateTime.Now,Status="Active" },
            };
            using (var conn = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction()) // Use transaction for efficiency
                {
                    try
                    {
                        string sql = @"
                                   INSERT INTO Role 
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

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null) transaction.Rollback();
                    }
                }
            }

        }
        private  void CreateNewDomainName()
        {
            List<Domain> domainList = new List<Domain>();
            Domain newDomain1 = new Domain
            {
                DomainName = "Local Domain",
                CN = "",
                DC1 = "Local",
                Description = "Getech local domain",
                Created_by = "System",
                Created_Date = DateTime.Now,
                Updated_by = "System",
                Updated_Date = DateTime.Now,
                Status = "Active",
                DomainNme = "Local Domain"
            };
            Domain newDomain2 = new Domain
            {
                DomainName = "Getech External Domain",
                CN = "",
                DC1 = "getecha.com",
                DC2 = "com",
                DC3 = "",
                Description = string.Empty,
                Created_by = "System",
                Created_Date = DateTime.Now,
                Updated_by = "System",
                Updated_Date = DateTime.Now,
                Status = "Active",
                DomainNme = "getecha.com"
            };
            domainList.Add(newDomain1);
            domainList.Add(newDomain2);
            foreach (Domain domain in domainList)
            {
                using (var conn = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
                {
                    conn.Open();

                    string sql = @"
                INSERT INTO Domain 
                (DomainName, CN, DC1, DC2, DC3, Description, Created_by, Created_Date, Updated_by, Updated_Date, Status,DomainNme)
                VALUES
                (@DomainName, @CN, @DC1, @DC2, @DC3, @Description, @Created_by, @Created_Date, @Updated_by, @Updated_Date, @Status,@DomainNme);
            ";

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


        }
        private  void CreateNewUser()
        {
            User user = new User
            {
                UserId = "system",
                Empid = "s123",
                UserName = "system",
                Password = "uebzTpuDgh2pDGf4LTTi7w==",
                Domain = 1,
                Status = "Active",
                Remarks = "GETECH LOCAL USER",
            };
            using (var conn = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;"))
            {
                conn.Open();

                string sql = @"
            INSERT INTO User_tbl 
            (UserId, Empid, UserName, Password, Domain, Status, Remarks)
            VALUES
            (@UserId, @Empid, @UserName, @Password, @Domain, @Status, @Remarks);
            ";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", user.UserId);
                    cmd.Parameters.AddWithValue("@Empid", user.Empid ?? "");
                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@Password", user.Password ?? "");
                    cmd.Parameters.AddWithValue("@Domain", user.Domain);
                    cmd.Parameters.AddWithValue("@Status", user.Status);
                    cmd.Parameters.AddWithValue("@Remarks", user.Remarks ?? "");

                    cmd.ExecuteNonQuery();
                }
            }

        }
        private void txtDatabaseName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Watermark.Visibility = string.IsNullOrEmpty(txtDatabaseName.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

    }
}
