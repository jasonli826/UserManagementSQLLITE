using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using UserManagementlibrary.Entity;
using UserManagementLibray.Entity;

namespace UserManagementlibrary.Repository
{
    public static class AlarmRepository
    {
        private static string dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];

        public static void InsertAlarm(AlarmMessage alarm)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();
                    string query = @"INSERT INTO Alarm (Alarm, Alarm_Description, RaiseTime, AcknowledgeTime, Created_by, Created_Date)
                                 VALUES (@Alarm, @Desc, @RaiseTime, @AckTime, @Created_by, datetime('now'))";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Alarm", alarm.Alarm);
                        cmd.Parameters.AddWithValue("@Desc", alarm.Alarm_Description);
                        cmd.Parameters.AddWithValue("@RaiseTime", alarm.RaiseTime);
                        cmd.Parameters.AddWithValue("@AckTime", alarm.AcknowledgeTime);
                        cmd.Parameters.AddWithValue("@Created_by", alarm.Created_by);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<AlarmMessage> GetAllAlarms()
        {
            var alarms = new List<AlarmMessage>();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();
                    string query = "SELECT * FROM Alarm ORDER BY RaiseTime DESC";

                    using (var cmd = new SQLiteCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            alarms.Add(new AlarmMessage
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Alarm = reader["Alarm"].ToString(),
                                Alarm_Description = reader["Alarm_Description"].ToString(),
                                RaiseTime = Convert.ToDateTime(reader["RaiseTime"].ToString()),
                                AcknowledgeTime = Convert.ToDateTime(reader["AcknowledgeTime"].ToString()),
                                Created_by = reader["Created_by"].ToString(),
                                Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
                                Updated_by = reader["Updated_by"].ToString(),
                                Updated_Date = Convert.ToDateTime(reader["Updated_Date"].ToString())
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return alarms;
        }
        public static List<AlarmMessage> GetFilteredAlarms(DateTime? startDate, DateTime? endDate, string keyword)
        {
            var alarms = new List<AlarmMessage>();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();
                    string query = "SELECT * FROM Alarm WHERE 1=1";

                    if (startDate.HasValue)
                        query += " AND DATE(RaiseTime) >= @StartDate";

                    if (endDate.HasValue)
                        query += " AND DATE(RaiseTime) <= @EndDate";

                    if (!string.IsNullOrEmpty(keyword))
                        query += " AND (LOWER(Alarm) LIKE @Keyword OR LOWER(Alarm_Description) LIKE @Keyword)";

                    query += " Order by Created_Date DESC";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        if (startDate.HasValue) cmd.Parameters.AddWithValue("@StartDate", startDate.Value.ToString("yyyy-MM-dd"));
                        if (endDate.HasValue) cmd.Parameters.AddWithValue("@EndDate", endDate.Value.ToString("yyyy-MM-dd"));
                        if (!string.IsNullOrEmpty(keyword)) cmd.Parameters.AddWithValue("@Keyword", $"%{keyword.ToLower()}%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                alarms.Add(new AlarmMessage
                                {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    Alarm = reader["Alarm"].ToString(),
                                    Alarm_Description = reader["Alarm_Description"].ToString(),
                                    RaiseTime = Convert.ToDateTime(reader["RaiseTime"].ToString()),
                                    AcknowledgeTime = Convert.ToDateTime(reader["AcknowledgeTime"].ToString()),
                                    Created_by = reader["Created_by"].ToString(),
                                    Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
                                    Updated_by = reader["Updated_by"].ToString(),
                                    Updated_Date = Convert.ToDateTime(reader["Updated_Date"].ToString()),
                                    AlarmNumeric = Convert.ToInt32((reader["Alarm"].ToString().ToUpper().Replace("ALARM ",string.Empty)))
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
            return alarms;
        }

        public static List<AlarmGroupDto> GetAlarmsGrouped(DateTime? startDate, DateTime? endDate, string groupBy)
        {
            var results = new List<AlarmGroupDto>();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string query = $@"
                        SELECT {groupBy} AS GroupedTime,
                               COUNT(*) AS AlarmCount
                        FROM Alarm
                        WHERE 1=1";

                    if (startDate.HasValue)
                        query += " AND RaiseTime >= @StartDate";

                    if (endDate.HasValue)
                        query += " AND RaiseTime <= @EndDate";

                    query += $" GROUP BY GroupedTime ORDER BY GroupedTime ASC";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        if (startDate.HasValue)
                            cmd.Parameters.AddWithValue("@StartDate", startDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        if (endDate.HasValue)
                            cmd.Parameters.AddWithValue("@EndDate", endDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime? groupedTime = null;

                                if (reader["GroupedTime"] != DBNull.Value)
                                {
                                    if (DateTime.TryParse(reader["GroupedTime"].ToString(), out var parsedDate))
                                        groupedTime = parsedDate;
                                }

                                results.Add(new AlarmGroupDto
                                {
                                    GroupedTime = groupedTime,
                                    Count = reader["AlarmCount"] != DBNull.Value
                                        ? Convert.ToInt32(reader["AlarmCount"])
                                        : 0
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
            return results;
        }



    }
}
