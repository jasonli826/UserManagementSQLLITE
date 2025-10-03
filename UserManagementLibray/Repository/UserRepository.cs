using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using UserManagementlibrary.Entity;
using UserManagementlibrary.Helpers;
using UserManagementlibrary.Helpers;

namespace UserManagementlibrary.Repository
{


    public static class UserRepository
    {
        private static string dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];
        // Role hierarchy visibility rules
        //private static readonly Dictionary<string, List<string>> roleVisibility = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        //{
        //    { "System Administrator", null }, // null = see all
        //    { "Service", new List<string> { "Engineer", "Technician", "Operator" } },
        //    { "Engineer", new List<string> { "Technician", "Operator" } },
        //    { "Technician", new List<string> { "Operator" } },
        //    { "Operator", new List<string>() }
        //};
        private static List<string> GetAllowedRolesDynamic(List<string> userRoles, List<Role> allRoles)
        {
            if (userRoles == null || userRoles.Count == 0)
                return new List<string>();

            var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 找到用户的最高级角色
            var roleDict = allRoles.ToDictionary(r => r.Role_Name, StringComparer.OrdinalIgnoreCase);

            Role highestRole = null;
            foreach (var roleName in userRoles)
            {
                if (roleDict.TryGetValue(roleName, out var role))
                {
                    if (highestRole == null || role.PriorityIndex < highestRole.PriorityIndex)
                    {
                        highestRole = role;
                    }
                }
            }

            if (highestRole == null)
                return new List<string>();

            // System Admin → 可以看所有
            if (highestRole.Role_Name.Equals("System Administrator", StringComparison.OrdinalIgnoreCase))
                return null;

            // 如果有 Parent → 从 Parent 开始收集可见角色
            int startRoleId = highestRole.ParentRoleID ?? highestRole.RoleID;

            var queue = new Queue<int>();
            queue.Enqueue(startRoleId);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var children = allRoles.Where(r => r.ParentRoleID == currentId).ToList();

                foreach (var child in children)
                {
                    allowedRoles.Add(child.Role_Name);
                    queue.Enqueue(child.RoleID);
                }
            }

            return allowedRoles.ToList();
        }
        private static List<string> GetUserRoles(string userId)
        {
            var roles = new List<string>();
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                string sql = @"SELECT r.Role_Name 
                               FROM UserRole ur 
                               JOIN Role r ON ur.RoleId = r.RoleID 
                               WHERE ur.UserId = @UserId";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(reader["Role_Name"].ToString());
                        }
                    }
                }
            }
            return roles;
        }

        private static List<string> GetAllowedRoles(List<string> userRoles, List<Role> allRoles)
        {
            if (userRoles == null || userRoles.Count == 0)
                return new List<string>();

            var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var roleDict = allRoles.ToDictionary(r => r.Role_Name, StringComparer.OrdinalIgnoreCase);

            // 找到用户的最高层角色（ParentRoleID 为 null 或不存在的角色）
            Role startingRole = null;
            foreach (var roleName in userRoles)
            {
                if (roleDict.TryGetValue(roleName.Trim(), out var role))
                {
                    startingRole = role;
                    break;
                }
            }

            if (startingRole == null)
                return new List<string>();

            // System Administrator → null 表示能看全部
            if (startingRole.Role_Name.Equals("System Administrator", StringComparison.OrdinalIgnoreCase))
                return null;

            // 如果角色有父级，找它的最顶层父级
            while (startingRole.ParentRoleID.HasValue)
            {
                var parentRole = allRoles.FirstOrDefault(r => r.RoleID == startingRole.ParentRoleID.Value);
                if (parentRole == null)
                    break;
                startingRole = parentRole;
            }

            // BFS 遍历所有子角色
            var queue = new Queue<int>();
            queue.Enqueue(startingRole.RoleID);

            while (queue.Count > 0)
            {
                int currentRoleId = queue.Dequeue();
                var children = allRoles
                    .Where(r => r.ParentRoleID == currentRoleId && r.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var child in children)
                {
                    allowedRoles.Add(child.Role_Name);
                    queue.Enqueue(child.RoleID);
                }
            }

            return allowedRoles.ToList();
        }






        public static void InsertUser(User user)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = @"
                    INSERT INTO User_tbl 
                    (UserId, Empid, UserName, Password, Domain, Created_by, Created_Date, Updated_by, Updated_Date, Status, Remarks)
                    VALUES
                    (@UserId, @Empid, @UserName, @Password, @Domain, @Created_by, @Created_Date, @Updated_by, @Updated_Date, @Status, @Remarks);";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", user.UserId);
                        cmd.Parameters.AddWithValue("@Empid", user.Empid ?? "");
                        cmd.Parameters.AddWithValue("@UserName", user.UserName);
                        cmd.Parameters.AddWithValue("@Password", user.Password ?? "");
                        cmd.Parameters.AddWithValue("@Domain", user.Domain);
                        cmd.Parameters.AddWithValue("@Created_by", user.Created_by);
                        cmd.Parameters.AddWithValue("@Created_Date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Updated_by", user.Updated_by ?? "");
                        cmd.Parameters.AddWithValue("@Updated_Date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Status", user.Status);
                        cmd.Parameters.AddWithValue("@Remarks", user.Remarks ?? "");

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static void UpdateLogoutTime(string userId)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string sql = @"
                UPDATE User_tbl 
                SET Logout_Date = @Logout_Date,
                    Updated_by = @Updated_by,
                    Updated_Date = @Updated_Date
                    WHERE UserId = @UserId  COLLATE NOCASE";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {

                        cmd.Parameters.AddWithValue("@Logout_Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@Updated_by", userId);
                        cmd.Parameters.AddWithValue("@Updated_Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        cmd.ExecuteNonQuery();
                    }
                }
            }catch(Exception ex)
                {
                throw ex;
                
                }
        }
        public static void UpdateUser(User user)
        {
            try { 
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();

                // Base SQL without password
                string sql = @"
                UPDATE User_tbl 
                SET Empid = @Empid,
                    UserName = @UserName,
                    Domain = @Domain,
                    Updated_by = @Updated_by,
                    Updated_Date = @Updated_Date,
                    Status = @Status,
                    Remarks = @Remarks";

                // Add password only if provided
                if (!string.IsNullOrEmpty(user.Password))
                {
                    sql += ", Password = @Password";
                }

                sql += " WHERE UserId = @UserId  COLLATE NOCASE;";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Empid", user.Empid ?? "");
                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@Domain", user.Domain);
                    cmd.Parameters.AddWithValue("@Updated_by", user.Updated_by ?? "");
                    cmd.Parameters.AddWithValue("@Updated_Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@Status", user.Status ?? "Active");
                    cmd.Parameters.AddWithValue("@Remarks", user.Remarks ?? "");
                    cmd.Parameters.AddWithValue("@UserId", user.UserId);

                    // Only add password if not empty
                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        cmd.Parameters.AddWithValue("@Password", CryptoHelper.Encrypt(user.Password));
                    }

                    cmd.ExecuteNonQuery();
                }
            }
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        private static List<Role> FilterRolesByUserRole(string userRole, List<Role> allRoles)
        {
            if (string.IsNullOrWhiteSpace(userRole) || allRoles == null || allRoles.Count == 0)
                return new List<Role>();

            // 1) 把用户可能拥有的角色名规范化为小写集合
            var userRoleNames = userRole
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToLower())
                .ToHashSet();

            if (!userRoleNames.Any())
                return new List<Role>();

            // 2) 构建按 ID 的字典，便于上溯查 root
            var byId = allRoles.ToDictionary(r => r.RoleID);

            // 3) 根据 RoleID 缓存 root 计算（避免重复遍历）
            var rootCache = new Dictionary<int, Role>();

            Role GetRoot(Role r)
            {
                if (r == null) return null;
                if (rootCache.TryGetValue(r.RoleID, out var cached)) return cached;

                var cur = r;
                // 上溯直到 ParentRoleID 为 null 或找不到父
                while (cur.ParentRoleID.HasValue && byId.TryGetValue(cur.ParentRoleID.Value, out var parent))
                {
                    cur = parent;
                }

                rootCache[r.RoleID] = cur;
                return cur;
            }

            // 4) 找到用户在 DB 中对应的角色对象
            var userRoles = allRoles
                .Where(r => userRoleNames.Contains(r.Role_Name.Trim().ToLower()))
                .ToList();

            if (!userRoles.Any())
                return new List<Role>(); // 登录角色在 DB 中找不到，返回空（可加日志检查）

            // 5) 如果用户包含 System Administrator（根是 System Administrator），直接返回所有 Active 角色
            foreach (var ur in userRoles)
            {
                var root = GetRoot(ur);
                if (root != null && root.Role_Name.Equals("System Administrator", StringComparison.OrdinalIgnoreCase))
                {
                    return allRoles
                        .Where(r => string.Equals(r.Status, "Active", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(r => r.PriorityIndex)
                        .ThenBy(r => r.Role_Name)
                        .ToList();
                }
            }

            // 6) 收集用户每个角色对应的 rootPriority（可能有多个角色，取每个的 rootPriority 并对每个做允许集合，然后合并）
            var userRootPriorities = new HashSet<int>();
            foreach (var ur in userRoles)
            {
                var root = GetRoot(ur);
                if (root != null) userRootPriorities.Add(root.PriorityIndex);
            }

            // 7) 找出所有 root（ParentRoleID == null）
            var roots = allRoles.Where(r => !r.ParentRoleID.HasValue).ToList();

            // 8) 对于每个用户根优先级，收集 rootPriority > 用户 rootPriority 的那些 root 下所有角色
            var allowedRoleIds = new HashSet<int>();
            foreach (var userRootPriority in userRootPriorities)
            {
                var allowedRoots = roots.Where(rt => rt.PriorityIndex > userRootPriority).ToList();
                foreach (var ar in allowedRoots)
                {
                    // 把属于该 allowed root 的所有角色加入（包括 root 本身与其所有后代）
                    foreach (var role in allRoles)
                    {
                        var roleRoot = GetRoot(role);
                        if (roleRoot != null && roleRoot.RoleID == ar.RoleID)
                        {
                            // 只加入 Active 的
                            if (string.Equals(role.Status, "Active", StringComparison.OrdinalIgnoreCase))
                                allowedRoleIds.Add(role.RoleID);
                        }
                    }
                }
            }

            // 9) 最后去掉用户自己所对应的具体角色（如果你不想用户看到自己）
            foreach (var name in userRoleNames)
            {
                var own = allRoles.FirstOrDefault(r => r.Role_Name.Trim().ToLower() == name);
                if (own != null) allowedRoleIds.Remove(own.RoleID);
            }

            // 10) 将 id 集合映射回 Role 对象并排序返回
            var result = allRoles
                .Where(r => allowedRoleIds.Contains(r.RoleID))
                .OrderBy(r => r.PriorityIndex)
                .ThenBy(r => r.Role_Name)
                .ToList();

            return result;
        }
        public static List<User> GetAllUser(string currentUserRole, List<Role> allRoles,string currentUserId)
        {
            var list = new List<User>();
            try
            {


                // Get allowed roles dynamically
                var allowedRoles = FilterRolesByUserRole(currentUserRole, allRoles).Select(x=>x.Role_Name).ToList();

                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    var sql = new StringBuilder();
                    sql.Append("SELECT a.*, b.DomainName, GROUP_CONCAT(r.Role_Name, '|') AS Roles ");
                    sql.Append("FROM User_tbl a ");
                    sql.Append("LEFT JOIN Domain b ON a.Domain = b.DomainID ");
                    sql.Append("LEFT JOIN UserRole ur ON a.UserId = ur.UserId ");
                    sql.Append("LEFT JOIN Role r ON ur.RoleId = r.RoleID ");
                    sql.Append("WHERE 1=1 ");

                    // Exclude current user
                    sql.Append("AND a.UserId <> @CurrentUserId ");

                    // If allowedRoles is not null → restrict roles
                    if (allowedRoles != null && allowedRoles.Count > 0)
                    {
                        string allowedRolesCsv = string.Join(",", allowedRoles.ConvertAll(r => $"'{r}'"));
                        sql.Append($"AND r.Role_Name IN ({allowedRolesCsv}) ");
                    }
                    else if (allowedRoles != null && allowedRoles.Count == 0)
                    {
                        // No roles allowed → return empty list
                        return list;
                    }
                    // else null means SYSTEM ADMINISTRATOR → no restriction

                    sql.Append("GROUP BY a.UserId ");
                    sql.Append("ORDER BY a.Updated_Date DESC;");

                    using (var cmd = new SQLiteCommand(sql.ToString(), conn))
                    {
                        cmd.Parameters.AddWithValue("@CurrentUserId", currentUserId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new User
                                {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    UserId = reader["UserId"].ToString(),
                                    Empid = reader["Empid"].ToString(),
                                    UserName = reader["UserName"].ToString(),
                                    Remarks = reader["Remarks"].ToString(),
                                    Domain = reader["Domain"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Domain"]),
                                    DomainName = reader["DomainName"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    Created_by = reader["Created_by"].ToString(),
                                    Created_Date = Convert.ToDateTime(reader["Created_Date"]),
                                    Updated_by = reader["Updated_by"].ToString(),
                                    Updated_Date = reader["Updated_Date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Updated_Date"]),
                                    Roles = reader["Roles"] == DBNull.Value ? "" : reader["Roles"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return list;
        }




        public static List<User> SearchUsers(string currentUserRole,
            string currentUserId,
            string userId,
            string userName,
            string empId,
            string domainName,
            string status,
            string remarks,
            List<int> selectedRoleIds)
        {
            var list = new List<User>();

            // 当前用户的角色
            var currentUserRoles = GetUserRoles(currentUserId);

            // 全部角色（带 ParentRoleID, PriorityIndex）
            var allRoles = RoleRepository.GetActiveRoleBasedOnSequence();

            // 动态获取可见角色
            var allowedRoles = FilterRolesByUserRole(currentUserRole, allRoles).Select(x=>x.Role_Name).ToList();

            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();
                    var sql = new StringBuilder();
                    sql.Append("SELECT a.*, b.DomainName, GROUP_CONCAT(r.Role_Name, '|') AS Roles ");
                    sql.Append("FROM User_tbl a ");
                    sql.Append("LEFT JOIN Domain b ON a.Domain = b.DomainID ");
                    sql.Append("LEFT JOIN UserRole ur ON a.UserId = ur.UserId ");
                    sql.Append("LEFT JOIN Role r ON ur.RoleId = r.RoleID ");
                    sql.Append("WHERE 1=1 ");

                    // 🚫 exclude system user
                    sql.Append("AND LOWER(a.UserId) <> 'system' ");

                    // 权限控制
                    if (allowedRoles != null && allowedRoles.Count > 0)
                    {
                        string allowedRolesCsv = string.Join(",", allowedRoles.ConvertAll(r => $"'{r}'"));
                        sql.Append($"AND r.Role_Name IN ({allowedRolesCsv}) ");
                    }
                    else if (allowedRoles != null && allowedRoles.Count == 0)
                    {
                        return list; // 没有权限看任何人
                    }
                    // 如果 allowedRoles == null → System Administrator → 不加过滤，能看所有

                    // 指定角色过滤
                    if (selectedRoleIds != null && selectedRoleIds.Count > 0)
                        sql.Append("AND a.UserId IN (SELECT UserId FROM UserRole WHERE RoleId IN (" + string.Join(",", selectedRoleIds) + ")) ");

                    if (!string.IsNullOrWhiteSpace(userId))
                        sql.Append("AND a.UserId LIKE @UserId COLLATE NOCASE ");
                    if (!string.IsNullOrWhiteSpace(userName))
                        sql.Append("AND a.UserName LIKE @UserName COLLATE NOCASE ");
                    if (!string.IsNullOrWhiteSpace(empId))
                        sql.Append("AND a.Empid LIKE @EmpId COLLATE NOCASE ");
                    if (!string.IsNullOrWhiteSpace(domainName))
                        sql.Append("AND b.DomainName LIKE @DomainName COLLATE NOCASE ");
                    if (!string.IsNullOrWhiteSpace(status))
                        sql.Append("AND a.Status LIKE @Status COLLATE NOCASE ");
                    if (!string.IsNullOrWhiteSpace(remarks))
                        sql.Append("AND a.Remarks LIKE @Remarks COLLATE NOCASE ");

                    sql.Append("GROUP BY a.UserId ");
                    sql.Append("ORDER BY a.Updated_Date DESC;");

                    using (var cmd = new SQLiteCommand(sql.ToString(), conn))
                    {
                        if (!string.IsNullOrWhiteSpace(userId))
                            cmd.Parameters.AddWithValue("@UserId", $"%{userId}%");
                        if (!string.IsNullOrWhiteSpace(userName))
                            cmd.Parameters.AddWithValue("@UserName", $"%{userName}%");
                        if (!string.IsNullOrWhiteSpace(empId))
                            cmd.Parameters.AddWithValue("@EmpId", $"%{empId}%");
                        if (!string.IsNullOrWhiteSpace(domainName))
                            cmd.Parameters.AddWithValue("@DomainName", $"%{domainName}%");
                        if (!string.IsNullOrWhiteSpace(status))
                            cmd.Parameters.AddWithValue("@Status", $"%{status}%");
                        if (!string.IsNullOrWhiteSpace(remarks))
                            cmd.Parameters.AddWithValue("@Remarks", $"%{remarks}%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new User
                                {
                                    ID = Convert.ToInt32(reader["ID"].ToString()),
                                    UserId = reader["UserId"].ToString(),
                                    Empid = reader["Empid"].ToString(),
                                    UserName = reader["UserName"].ToString(),
                                    Remarks = reader["Remarks"].ToString(),
                                    Domain = reader["Domain"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Domain"]),
                                    DomainName = reader["DomainName"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    Created_by = reader["Created_by"].ToString(),
                                    Created_Date = Convert.ToDateTime(reader["Created_Date"]),
                                    Updated_by = reader["Updated_by"].ToString(),
                                    Updated_Date = reader["Updated_Date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Updated_Date"]),
                                    Roles = reader["Roles"] == DBNull.Value ? "" : reader["Roles"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return list;
        }


        //public static User Login(string username, string password)
        //{
        //    string storedPassword = string.Empty;
        //    string decryptedPassword = string.Empty;
        //    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        //        return null;

        //    using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
        //    {
        //        conn.Open();

        //        // Get user info
        //        string sqlUser = "SELECT * FROM User_tbl WHERE UserId = @UserId  COLLATE NOCASE LIMIT 1";
        //        User user = null;

        //        using (var cmd = new SQLiteCommand(sqlUser, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@UserId", username);
        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                if (reader.Read())
        //                {
        //                     storedPassword = reader["Password"].ToString();
        //                     decryptedPassword = CryptoHelper.Decrypt(storedPassword);

        //                    if (decryptedPassword != password)
        //                        return null;


        //                    user = new User
        //                    {
        //                        UserId = reader["UserId"].ToString(),
        //                        Empid = reader["Empid"].ToString(),
        //                        UserName = reader["UserName"].ToString(),
        //                        Password = storedPassword,
        //                        Domain = Convert.ToInt32(reader["Domain"]),
        //                        Status = reader["Status"].ToString(),
        //                        Remarks = reader["Remarks"].ToString(),
        //                        Created_by = reader["Created_by"].ToString(),
        //                        Created_Date = Convert.ToDateTime(reader["Created_Date"]),
        //                        Updated_by = reader["Updated_by"].ToString(),
        //                        Updated_Date = Convert.ToDateTime(reader["Updated_Date"]),
        //                        IsSystemAdmin = false
        //                    };
        //                }
        //                else
        //                {
        //                    return null; // No user found
        //                }
        //            }
        //        }

        //        // Get all roles for this user
        //        string sqlRoles = @"SELECT r.Role_Name 
        //                    FROM UserRole ur 
        //                    JOIN Role r ON ur.RoleId = r.RoleID 
        //                    WHERE ur.UserId = @UserId  COLLATE NOCASE";

        //        using (var cmdRoles = new SQLiteCommand(sqlRoles, conn))
        //        {
        //            cmdRoles.Parameters.AddWithValue("@UserId", username);
        //            using (var readerRoles = cmdRoles.ExecuteReader())
        //            {

        //                while (readerRoles.Read())
        //                {
        //                    if (readerRoles["Role_Name"].ToString().Equals("System Administrator", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        if (user != null)
        //                        {
        //                            user.IsSystemAdmin = true;
        //                            break;
        //                        }
        //                    }
        //                }

        //            }
        //        }

        //        return user;
        //    }
        //}

        public static bool ChangePassword(string userId, string oldPassword, string newPassword)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    // Check if old password matches
                    string sqlCheck = "SELECT COUNT(*) FROM User_tbl WHERE UserId=@UserId AND Password=@OldPassword";
                    using (var cmd = new SQLiteCommand(sqlCheck, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@OldPassword",CryptoHelper.Encrypt(oldPassword));
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count == 0)
                            return false;
                    }

                    string sqlUpdate = "UPDATE User_tbl SET Password=@NewPassword,Pwd_Change_Date=@Pwd_Change_Date, Updated_by=@Updated_by,Updated_Date=@Updated_Date WHERE UserId=@UserId";
                    using (var cmd = new SQLiteCommand(sqlUpdate, conn))
                    {
                        cmd.Parameters.AddWithValue("@NewPassword",CryptoHelper.Encrypt(newPassword));
                        cmd.Parameters.AddWithValue("@Pwd_Change_Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@Updated_Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@Updated_by", userId);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch(Exception ex)
            {
                throw ex;
         
            }
  
        }

        public static List<User> GetAllUser()
        {
            var list = new List<User>();

            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    // Fetch users and domain info
                    string sql = "SELECT a.*,b.DomainName FROM User_tbl a LEFT JOIN Domain b ON a.Domain=b.DomainID where UserId=@UserId";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", "system");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new User
                                {
                                    ID = Convert.ToInt32(reader["ID"].ToString()),
                                    UserId = reader["UserId"].ToString(),
                                    Empid = reader["Empid"].ToString(),
                                    UserName = reader["UserName"].ToString(),
                                    Remarks = reader["Remarks"].ToString(),
                                    Domain = Convert.ToInt32(reader["Domain"].ToString()),
                                    DomainName = reader["DomainName"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    Created_by = reader["Created_by"].ToString(),
                                    Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
                                    Updated_by = reader["Updated_by"].ToString(),
                                    Updated_Date = Convert.ToDateTime(reader["Updated_Date"].ToString()),
                                    Roles = "" // initialize empty, we will fill it next
                                });
                            }
                        }
                    }
                        // Fetch roles for all users
                        string roleSql = @" SELECT ur.UserId, r.Role_Name FROM UserRole ur JOIN Role r ON ur.RoleId = r.RoleID";
                        var roleDict = new Dictionary<string, List<string>>();
                        using (var cmd = new SQLiteCommand(roleSql, conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string userId = reader["UserId"].ToString();
                                string roleName = reader["Role_Name"].ToString();

                                if (!roleDict.ContainsKey(userId))
                                    roleDict[userId] = new List<string>();

                                roleDict[userId].Add(roleName);
                            }
                        }
                        foreach (var user in list)
                        {
                            if (roleDict.ContainsKey(user.UserId))
                                user.Roles = string.Join("|", roleDict[user.UserId]);
                        }
                    }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return list;
        }

        public static bool UserIdExists(string userId)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                string sql = "SELECT COUNT(1) FROM User_tbl WHERE UserId = @UserId  COLLATE NOCASE;";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public static bool EmployeeIdExists(string empId)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                string sql = "SELECT COUNT(1) FROM User_tbl WHERE Empid = @Empid COLLATE NOCASE;";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Empid", empId);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public static bool EmployeeIdExists(string empId,int ID)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                string sql = "SELECT COUNT(1) FROM User_tbl WHERE Empid = @Empid  and ID!=@ID COLLATE NOCASE";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Empid", empId);
                    cmd.Parameters.AddWithValue("@ID", ID);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
        public static bool UserIdExists(string userId, int ID)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                string sql = "SELECT COUNT(1) FROM User_tbl WHERE UserId = @UserId  COLLATE NOCASE and ID!=@ID";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@ID", ID);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

    }

}
