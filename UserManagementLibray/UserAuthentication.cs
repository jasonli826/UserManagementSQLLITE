using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UserManagementlibrary.Entity;
using UserManagementlibrary.Log;
using UserManagementlibrary.Repository;
using UserManagementLibray;
using UserManagementLibray.Entity;
using UserManagementLibray.Repository;

namespace UserManagementlibrary
{
    public class UserAuthentication
    {
        public UserAuthentication() { }

        public static int Authenticated(string userName, string Password, ref string result)
        {
            int retVal = 0;
            ApiLogger.Log("UserAuthentication", "Calling UserAuthentication API");
            retVal = UserAuthenticationRepository.Login(userName, Password, ref result);
            ApiLogger.Log("UserAuthentication", "Receive Authentication API Message: " + result);

                SessionContext.UserId = userName;
                AuditRepository.InsertAudit(new Audit
                {
                    AuditDate = DateTime.Now,
                    UserName = userName,
                    Detail = (retVal == 0 ? "Login success" : "Login failed due to error message "+result),
                    Created_by = userName,
                    Created_Date = DateTime.Now,
                    Updated_by = userName,
                    Updated_Date = DateTime.Now
                });
            
            



            return retVal;
        }
        public static void OpenAuditLog()
        {
            List<int> RoleList = SessionContext.RoleIds;
            if (!RoleControlRepository.HasMonitoringAuditLogAccess(RoleList, "Monitoring", "Audit Log"))
            {
                MessageBox.Show("Access Denied. You dont have permission to open this window.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }
            var AuditLog = new AuditLog();
            UserManagementlibrary.Form.PopupWindow popupWindow = new UserManagementlibrary.Form.PopupWindow(AuditLog, "Audit Log");
            popupWindow.ShowDialog();
            // 🔹 Audit action
            AuditRepository.InsertAudit(new Audit
            {
                AuditDate = DateTime.Now,
                UserName = SessionContext.UserId,
                Detail = "Opened Audit Log",
                Created_by = SessionContext.UserId,
                Created_Date = DateTime.Now,
                Updated_by = SessionContext.UserId,
                Updated_Date = DateTime.Now
            });

        }
        public static void OpenAlarmWindow()
        {
            List<int> RoleList = SessionContext.RoleIds;
            if (!RoleControlRepository.HasMonitoringAuditLogAccess(RoleList, "Monitoring", "Alarm"))
            {
                MessageBox.Show("Access Denied. You dont have permission to open this window.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }
            var Alarm = new Alarm();
            UserManagementlibrary.Form.PopupWindow popupWindow = new UserManagementlibrary.Form.PopupWindow(Alarm, "Alarm Window");
            popupWindow.ShowDialog();
            // 🔹 Audit action
            AuditRepository.InsertAudit(new Audit
            {
                AuditDate = DateTime.Now,
                UserName = SessionContext.UserId,
                Detail = "Opened Alarm Window",
                Created_by = SessionContext.UserId,
                Created_Date = DateTime.Now,
                Updated_by = SessionContext.UserId,
                Updated_Date = DateTime.Now
            });

        }
        public static void OpenLogWindow()
        {
            var Log = new LogUserControl();
            UserManagementlibrary.Form.PopupWindow popupWindow = new UserManagementlibrary.Form.PopupWindow(Log, "Log Window");
            popupWindow.Width = 900;
            popupWindow.Height = 600;
            popupWindow.ShowDialog();
            // 🔹 Audit action
            AuditRepository.InsertAudit(new Audit
            {
                AuditDate = DateTime.Now,
                UserName = SessionContext.UserId,
                Detail = "Opened Alarm Window",
                Created_by = SessionContext.UserId,
                Created_Date = DateTime.Now,
                Updated_by = SessionContext.UserId,
                Updated_Date = DateTime.Now
            });

        }
        public static void OpenUserManagemeWindow()
        {
           if (SessionContext.UserRole.ToUpper() == "OPERATOR")
            {
                MessageBox.Show("Access Denied. Only System Admin can access this page.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var userManagementControl = new UserManagement();
            UserManagementlibrary.Form.PopupWindow popupWindow = new UserManagementlibrary.Form.PopupWindow(userManagementControl, "User Management");
            popupWindow.ShowDialog();

            // 🔹 Audit action
            AuditRepository.InsertAudit(new Audit
            {
                AuditDate = DateTime.Now,
                UserName = SessionContext.UserId,
                Detail = "Opened User Management window",
                Created_by = SessionContext.UserId,
                Created_Date= DateTime.Now,
                Updated_by = SessionContext.UserId,
                Updated_Date = DateTime.Now
            });
        }

        public static void OpenRoleAdministrationWindow()
        {
            if (!SessionContext.UserRole.ToUpper().Contains("SYSTEM ADMINISTRATOR"))
            {
                MessageBox.Show("Access Denied. Only System Administrator can access this page.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var RoleAdministration = new RoleAdministration();
            UserManagementlibrary.Form.PopupWindow popupWindow = new UserManagementlibrary.Form.PopupWindow(RoleAdministration, "Role Administration");
            popupWindow.ShowDialog();

            // 🔹 Audit action
            AuditRepository.InsertAudit(new Audit
            {
                AuditDate = DateTime.Now,
                UserName = SessionContext.UserId,
                Detail = "Opened Role Administration window",
                Created_by = SessionContext.UserId,
                Created_Date = DateTime.Now,
                Updated_by = SessionContext.UserId,
                Updated_Date = DateTime.Now
            });
        }

        public static void OpenDomainWindow()
        {
            if (!SessionContext.UserRole.ToUpper().Contains("SYSTEM ADMINISTRATOR"))
            {
                MessageBox.Show("Access Denied. Only System Administrator can access this page.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var domainWindow = new DomainUserControl();
            UserManagementlibrary.Form.PopupWindow popupWindow = new UserManagementlibrary.Form.PopupWindow(domainWindow, "Domain");
            popupWindow.ShowDialog();

            // 🔹 Audit action
            AuditRepository.InsertAudit(new Audit
            {
                AuditDate = DateTime.Now,
                UserName = SessionContext.UserId,
                Detail = "Opened Domain Management window",
                Created_by = SessionContext.UserId,
                Created_Date = DateTime.Now,
                Updated_by = SessionContext.UserId,
                Updated_Date = DateTime.Now
            });
        }

        public static void OpenDataAnalyticsWindow()
        {
            List<int> RoleList = SessionContext.RoleIds;
            if (!RoleControlRepository.HasMonitoringAuditLogAccess(RoleList, "Monitoring", "Data Analytics"))
            {
                MessageBox.Show("Access Denied. You dont have permission to open this window.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }
            var DataAnalytics = new DataAnalytics();
            UserManagementlibrary.Form.PopupWindow popupWindow = new UserManagementlibrary.Form.PopupWindow(DataAnalytics, "DataAnalytics");

            popupWindow.ShowDialog();
            AuditRepository.InsertAudit(new Audit
            {
                AuditDate = DateTime.Now,
                UserName = SessionContext.UserId,
                Detail = "Opened DataAnalytics window",
                Created_by = SessionContext.UserId,
                Created_Date = DateTime.Now,
                Updated_by = SessionContext.UserId,
                Updated_Date = DateTime.Now
            });
        }
        public static void OpenChangePasswordWindow()
        {
            var ChangePassword = new ChangePassword();
            UserManagementlibrary.Form.PopupWindow popupWindow = new UserManagementlibrary.Form.PopupWindow(ChangePassword, "Change Password");
           
            popupWindow.ShowDialog();

            // 🔹 Audit action
            AuditRepository.InsertAudit(new Audit
            {
                AuditDate = DateTime.Now,
                UserName = SessionContext.UserId,
                Detail = "Opened Change Password window",
                Created_by = SessionContext.UserId,
                Created_Date = DateTime.Now,
                Updated_by = SessionContext.UserId,
                Updated_Date = DateTime.Now
            });
        }

        public static int Logout()
        {
            UserManagementlibrary.Repository.UserRepository.UpdateLogoutTime(SessionContext.UserId);
            AuditRepository.InsertAudit(new Audit
            {
                AuditDate = DateTime.Now,
                UserName = SessionContext.UserId,
                Detail = "Logged Out",
                Created_by = SessionContext.UserId,
                Created_Date= DateTime.Now,
                Updated_by = SessionContext.UserId,
                Updated_Date = DateTime.Now

            });

            return 0;
        }
        public static void OpenRoleSequenceControWindow()
        {
            var RoleSequenceControl = new RoleSequenceControl();
            UserManagementlibrary.Form.PopupWindow popupWindow = new UserManagementlibrary.Form.PopupWindow(RoleSequenceControl, "RoleSequenceControl");

            popupWindow.ShowDialog();


        }
        public static void RaiseAlarm(string alarmName, string description)
        {
            try
            {
                AlarmRepository.InsertAlarm(new AlarmMessage
                {
                    Alarm = alarmName,
                    Alarm_Description = description,
                    RaiseTime = DateTime.Now,
                    Created_by = SessionContext.UserId,
                    Created_Date = DateTime.Now,
                    Updated_by = SessionContext.UserId,
                    Updated_Date = DateTime.Now
                });

                ApiLogger.Log("UserAuthentication", $"Alarm Raised: {alarmName} - {description}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "RaiseAlarm Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        public static void WriteAuditLog(string message)
        {
            AuditRepository.InsertAudit(new Audit
            {
                AuditDate = DateTime.Now,
                UserName = SessionContext.UserId,
                Detail = message,
                Created_by = "System",
                Created_Date = DateTime.Now,
                Updated_by = SessionContext.UserId,
                Updated_Date = DateTime.Now
            });
        }
    }
}
