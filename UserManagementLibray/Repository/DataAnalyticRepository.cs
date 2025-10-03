using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Windows;
using UserManagementlibrary.Entity;
using UserManagementLibray.Entity;

namespace UserManagementlibrary.Repository
{
    public static class DataAnalyticRepository
    {
        private static string dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];

        public static void InsertDataAnalytic(DataAnalytic data)
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();
                    string query = @"INSERT INTO DataAnalytic (Date, PanelCountPass, PanelCountFail, PanelCountRework, Created_by, Created_Date)
                                 VALUES (@Date, @Pass, @Reject, @Rework, @Created_by, datetime('now','localtime'),@Updated_by,datetime('now','localtime'))";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Date", data.Date);
                        cmd.Parameters.AddWithValue("@Pass", data.PanelCountPass);
                        cmd.Parameters.AddWithValue("@Reject", data.PanelCountReject);
                        cmd.Parameters.AddWithValue("@Rework", data.PanelCountRework);
                        cmd.Parameters.AddWithValue("@Created_by", data.Created_by);
                        cmd.Parameters.AddWithValue("@Updated_by", data.Updated_by);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "InsertDataAnalytic Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        public static List<DataAnalytic> GetAllDataAnalytics()
        {
            var analytics = new List<DataAnalytic>();

            using (var conn =  new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                conn.Open();
                string query = "SELECT * FROM DataAnalytic ORDER BY Date DESC";

                using (var cmd = new SQLiteCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        analytics.Add(new DataAnalytic
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            Date = Convert.ToDateTime(reader["Date"].ToString()),
                            PanelCountPass = Convert.ToDouble(reader["PanelCountPass"]),
                            PanelCountReject = Convert.ToDouble(reader["PanelCountReject"]),
                            PanelCountRework = Convert.ToDouble(reader["PanelCountRework"]),
                            Created_by = reader["Created_by"].ToString(),
                            Created_Date = Convert.ToDateTime(reader["Created_Date"].ToString()),
                            Updated_by = reader["Updated_by"].ToString(),
                            Updated_Date = Convert.ToDateTime( reader["Updated_Date"].ToString())
                        });
                    }
                }
            }
            return analytics;
        }
        public static List<PanelCountSummaryDto> GetPanelCountSummaryForUPH(DateTime startDate, DateTime endDate, bool hourly = false)
        {
          
            var results = new List<PanelCountSummaryDto>();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string groupClause = hourly ? "STRFTIME('%Y-%m-%d %H:00:00', Date)" : "DATE(Date)";

                    string query = $@"SELECT {groupClause} AS DateGroup,SUM(PanelCountPass) AS TotalPass,SUM(PanelCountFail) AS TotalReject,SUM(PanelCountRework) AS TotalRework
                                    FROM DataAnalytic
                                    WHERE Date BETWEEN @StartDate AND @EndDate
                                    GROUP BY DateGroup
                                    ORDER BY DateGroup ASC";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd HH:mm:ss"));

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var totalPass = reader["TotalPass"] != DBNull.Value ? Convert.ToDouble(reader["TotalPass"]) : 0;
                                var totalReject = reader["TotalReject"] != DBNull.Value ? Convert.ToDouble(reader["TotalReject"]) : 0;
                                var totalRework = reader["TotalRework"] != DBNull.Value ? Convert.ToDouble(reader["TotalRework"]) : 0;

                                results.Add(new PanelCountSummaryDto
                                {
                                    Date = Convert.ToDateTime(reader["DateGroup"]),
                                    TotalPass = totalPass,
                                    TotalReject = totalReject,
                                    TotalRework = totalRework,
                                    UPH = hourly
                                        ? (totalPass + totalReject + totalRework)  // hourly: panels in that hour
                                        : (totalPass + totalReject + totalRework) / 24.0 // daily: average per hour
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

        public static List<PanelCountSummaryDto> GetPanelCountSummaryDaily(DateTime startDate, DateTime endDate)
        {
            var results = new List<PanelCountSummaryDto>();
            try
            {

                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string query = @"SELECT DATE(Date) AS DateOnly,SUM(PanelCountPass) AS TotalPass,SUM(PanelCountFail) AS TotalReject,SUM(PanelCountRework) AS TotalRework
                                    FROM DataAnalytic
                                    WHERE Date BETWEEN @StartDate AND @EndDate
                                    GROUP BY DateOnly
                                    ORDER BY DateOnly DESC";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd HH:mm:ss"));

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new PanelCountSummaryDto
                                {
                                    Date = Convert.ToDateTime(reader["DateOnly"]),
                                    TotalPass = reader["TotalPass"] != DBNull.Value ? Convert.ToDouble(reader["TotalPass"]) : 0,
                                    TotalReject = reader["TotalReject"] != DBNull.Value ? Convert.ToDouble(reader["TotalReject"]) : 0,
                                    TotalRework = reader["TotalRework"] != DBNull.Value ? Convert.ToDouble(reader["TotalRework"]) : 0
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
        public static List<PanelCountSummaryDto> GetPanelCountSummaryHourly(DateTime startDate, DateTime endDate)
        {
            var results = new List<PanelCountSummaryDto>();
            try
            {

                using (var conn = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
                {
                    conn.Open();

                    string query = @"SELECT strftime('%Y-%m-%d %H:00:00', Date) AS HourBlock,SUM(PanelCountPass) AS TotalPass,SUM(PanelCountFail) AS TotalReject,SUM(PanelCountRework) AS TotalRework
                                    FROM DataAnalytic
                                    WHERE Date BETWEEN @StartDate AND @EndDate
                                    GROUP BY HourBlock
                                    ORDER BY HourBlock ASC";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd HH:mm:ss"));

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new PanelCountSummaryDto
                                {
                                    Date = Convert.ToDateTime(reader["HourBlock"]),
                                    TotalPass = reader["TotalPass"] != DBNull.Value ? Convert.ToDouble(reader["TotalPass"]) : 0,
                                    TotalReject = reader["TotalReject"] != DBNull.Value ? Convert.ToDouble(reader["TotalReject"]) : 0,
                                    TotalRework = reader["TotalRework"] != DBNull.Value ? Convert.ToDouble(reader["TotalRework"]) : 0
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
