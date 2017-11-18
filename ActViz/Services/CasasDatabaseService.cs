using ActViz.Helpers;
using ActViz.Models;
using ActViz.ViewModels;
using Npgsql;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.Services
{
    public class CasasDatabaseService
    {
        private Logger appLog = Logger.Instance;
        private SshClient sshClient;
        private ForwardedPortLocal sshTunnel;
        private NpgsqlConnection dbConnection;

        public bool IsSshEnabled { get; set; }
        public string SshServer { get; set; }
        public int SshPort { get; set; }
        public string SshUsername { get; set; }
        public string SshPassword { get; set; }

        public string DbServer { get; set; }
        public int DbPort { get; set; }
        public string DbUsername { get; set; }
        public string DbPassword { get; set; }

        public CasasDatabaseService()
        {
            IsSshEnabled = false;
            SshServer = "127.0.0.1";
            SshPort = 22;
            SshUsername = "";
            SshPassword = "";
            DbServer = "127.0.0.1";
            DbPort = 5432;
            DbUsername = "";
            DbPassword = "";
        }

        public CasasDatabaseService(string dbServer, int dbPort, string dbUsername, string dbPassword)
        {
            IsSshEnabled = false;
            SshServer = "127.0.0.1";
            SshPort = 22;
            SshUsername = "";
            SshPassword = "";
            DbServer = dbServer;
            DbPort = dbPort;
            DbUsername = dbUsername;
            DbPassword = dbPassword;
        }

        public CasasDatabaseService(string dbServer, int dbPort, string dbUsername, string dbPassword,
            string sshServer, int sshPort, string sshUsername, string sshPassword)
        {
            IsSshEnabled = true;
            SshServer = sshServer;
            SshPort = sshPort;
            SshUsername = sshUsername;
            SshPassword = sshPassword;
            DbServer = dbServer;
            DbPort = dbPort;
            DbUsername = dbUsername;
            DbPassword = dbPassword;
        }

        public void Start()
        {
            appLog.Debug(this.GetType().ToString(), "Start CASAS Database Service");
            if (IsSshEnabled)
            {
                appLog.Debug(this.GetType().ToString(), 
                    string.Format("SSH Enabled. Connect to SSH Server {0}, port {1:d}.", SshServer, SshPort));
                // Setup SSH Port Forwarding
                sshClient = new SshClient(SshServer, 22, SshUsername, SshPassword);
                sshClient.Connect();
                sshTunnel = new ForwardedPortLocal("127.0.0.1", (uint)DbPort, DbServer, (uint)DbPort);
                sshClient.AddForwardedPort(sshTunnel);
                sshTunnel.Start();
            }
            appLog.Debug(this.GetType().ToString(),
                    string.Format("Connect to Database Server {0}, port {1:d}.", DbServer, DbPort));
            // Connect to Database
            string sqlConnString = "Host=" + ((IsSshEnabled) ? "127.0.0.1" : DbServer) +
                ";Username=" + DbUsername + ";Password=" + DbPassword + ";Database=smarthomedata;";
            dbConnection = new NpgsqlConnection(sqlConnString);
            dbConnection.Open();
        }

        public void Stop()
        {
            if (dbConnection != null && dbConnection.State != System.Data.ConnectionState.Closed) dbConnection.Close();
            if (sshClient != null && sshClient.IsConnected) sshClient.Disconnect();
        }

        public List<TestBedViewModel> GetTestBedsInfo()
        {
            TimeZoneInfo timeZone;
            DateTime dateTime;
            if (dbConnection.State != System.Data.ConnectionState.Open) return null;
            List<TestBedViewModel> testBeds = new List<TestBedViewModel>();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = dbConnection;
                cmd.CommandText = "select tbname, description, active, created_on, timezone from testbed";
                appLog.Debug(this.GetType().ToString(),
                    string.Format("Executing SQL \"{0}\".", cmd.CommandText));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        timeZone = (reader.IsDBNull(4)) ? TimeZoneInfo.Utc :
                            TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConverter.TZConvert.IanaToWindows(reader.GetString(4)));
                        dateTime = (reader.IsDBNull(3)) ? DateTime.MinValue : reader.GetDateTime(3);
                        testBeds.Add(new TestBedViewModel()
                        {
                            Name = (reader.IsDBNull(0)) ? "" : reader.GetString(0),
                            Description = (reader.IsDBNull(1)) ? "" : reader.GetString(1),
                            Active = (reader.IsDBNull(2)) ? false : reader.GetBoolean(2),
                            CreatedTime = (reader.IsDBNull(3)) ? DateTimeOffset.MinValue :
                                new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified), timeZone.BaseUtcOffset),
                            TimeZone = timeZone
                        });
                    }
                }
                return testBeds;
            }
        }

        public Tuple<DateTime, DateTime> GetStartStopDateTime(TestBedViewModel testBed)
        {
            return GetStartStopDateTime(testBed.Name);
        }

        public Tuple<DateTime, DateTime> GetStartStopDateTime(string testBedName)
        {
            DateTime stopTime;
            DateTime startTime;
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = dbConnection;
                cmd.CommandText = "select stamp from (select stamp, row_number() over (order by stamp desc) as rn, " +
                    "count(*) over () as total_count from detailed_all_events where tbname='" + testBedName + "') t where rn = 1 or rn = total_count;";
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    stopTime = reader.GetDateTime(0);
                    reader.Read();
                    startTime = reader.GetDateTime(0);
                }
            }
            return new Tuple<DateTime, DateTime>(stopTime, startTime);
        }

        public List<string> GetSensorTypeList(TestBedViewModel testBed)
        {
            return GetSensorTypeList(testBed.Name);
        }

        public List<string> GetSensorTypeList(string testBedName)
        {
            List<string> sensorTypeList = new List<string>();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = dbConnection;
                cmd.CommandText = "select distinct sensor_type from detailed_all_sensors where tbname='" + testBedName + "';";
                appLog.Debug(this.GetType().ToString(),
                    string.Format("Executing SQL \"{0}\".", cmd.CommandText));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sensorTypeList.Add(reader.GetString(0));
                    }
                }
            }
            return sensorTypeList;
        }

        public List<string> GetDistinctSensorTargetList(TestBedViewModel testBed)
        {
            return GetDistinctSensorTargetList(testBed.Name);
        }

        public List<string> GetDistinctSensorTargetList(string testBedName)
        {
            List<string> sensorTargetList = new List<string>();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = dbConnection;
                cmd.CommandText = "select distinct sensor_type from detailed_all_sensors where tbname='" + testBedName + "';";
                appLog.Debug(this.GetType().ToString(),
                    string.Format("Executing SQL \"{0}\".", cmd.CommandText));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sensorTargetList.Add(reader.GetString(0));
                    }
                }
            }
            return sensorTargetList;
        }

        public List<Sensor> GetSensors(TestBedViewModel testBed)
        {
            return GetSensors(testBed.Name);
        }

        public List<Sensor> GetSensors(string testBedName)
        {
            List<Sensor> sensorList = new List<Sensor>();
            Dictionary<string, Sensor> sensorDict = new Dictionary<string, Sensor>();
            string targetName;
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = dbConnection;
                cmd.CommandText = "select target, sensor_type from detailed_all_sensors where tbname='" + testBedName + "';";
                appLog.Debug(this.GetType().ToString(),
                    string.Format("Executing SQL \"{0}\".", cmd.CommandText));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        targetName = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        if (sensorDict.ContainsKey(targetName))
                        {
                            sensorDict[targetName].Types.Add(reader.IsDBNull(1) ? "" : reader.GetString(1));
                        }
                        else
                        {
                            Sensor sensor = new Sensor();
                            sensor.Name = targetName;
                            sensor.Types = new List<string>() { reader.IsDBNull(1) ? "" : reader.GetString(1) };
                            sensor.LocX = 0;
                            sensor.LocY = 0;
                            sensor.SizeX = 0.05;
                            sensor.SizeY = 0.02;
                            sensorList.Add(sensor);
                            sensorDict.Add(targetName, sensor);
                        }
                    }
                }
            }
            return sensorList;
        }

        private static readonly HashSet<string> binarySensorMessages = new HashSet<string>()
        {
            "ON", "OFF", "ABSENT", "PRESENT", "OPEN", "CLOSE"
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testBedName"></param>
        /// <param name="startTime"></param>
        /// <param name="stopTime"></param>
        /// <returns>Tuple of motion, item, switch events, temperature events, Light sensing, Radio events, and other events</returns>
        public Tuple<List<string>, List<string>, List<string>, List<string>, List<string>>
            GetSensorEvents(TestBedViewModel testBed, DateTimeOffset startTime, DateTimeOffset stopTime)
        {
            List<string> binaryEvents = new List<string>();
            List<string> temperatureEvents = new List<string>();
            List<string> lightSensorEvents = new List<string>();
            List<string> radioEvents = new List<string>();
            List<string> otherEvents = new List<string>();
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = dbConnection;
                cmd.CommandText = string.Format("select stamp, target, message, sensor_type from detailed_all_events where tbname='{0}' and stamp > '{1:u}' and stamp < '{2:u}';",
                    testBed.Name, startTime, stopTime);
                // Need to increase command timeout.
                cmd.CommandTimeout = 1200;
                appLog.Debug(this.GetType().ToString(),
                    string.Format("Executing SQL \"{0}\".", cmd.CommandText));
                using (var reader = cmd.ExecuteReader())
                {
                    appLog.Debug(this.GetType().ToString(),
                        string.Format("Data retrieved from database."));
                    while (reader.Read())
                    {
                        // Check binary events based on messages
                        string message = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        string sensorTypeString = reader.IsDBNull(3) ? "" : reader.GetString(3);
                        DateTime utcDateTime = reader.IsDBNull(0) ? DateTime.MinValue.ToUniversalTime() : reader.GetDateTime(0);
                        DateTimeOffset utcDateTimeOffset = new DateTimeOffset(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc));
                        string eventString = String.Format("{0:MM/dd/yyyy HH:mm:ss zzz},{1},{2},,,{3}",
                                TimeZoneInfo.ConvertTime(utcDateTimeOffset, testBed.TimeZone),
                                reader.IsDBNull(1) ? "" : reader.GetString(1),
                                message,
                                sensorTypeString
                            );
                        if (binarySensorMessages.Contains(message))
                        {
                            binaryEvents.Add(eventString);
                        }
                        else
                        {
                            switch (SensorType.GetSensorType(sensorTypeString).Category)
                            {
                                case "Light":
                                    lightSensorEvents.Add(eventString);
                                    break;
                                case "Temperature":
                                    temperatureEvents.Add(eventString);
                                    break;
                                case "Radio":
                                    radioEvents.Add(eventString);
                                    break;
                                default:
                                    otherEvents.Add(eventString);
                                    break;
                            }
                        }
                    }
                }
            }
            return new Tuple<List<string>, List<string>, List<string>, List<string>, List<string>>
                (binaryEvents, temperatureEvents, lightSensorEvents, radioEvents, otherEvents);
        }
    }
}
