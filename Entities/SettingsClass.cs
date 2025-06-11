using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities;

public class SettingsClass
{
    public List<Log> Logs { get; set; }
    public List<Cookie> Cookies { get; set; }
    public bool IsDebug { get; set; }
    public float CalculateDynamicVariablesInterval { get; set; }
    public float RealoadPendingCodeReferenceInterval { get; set; }
    public float ReferenceCodeExpirationTime { get; set; }
    public int ReferenceCodeLength { get; set; }
    public Databases DatabaseConection { get; set; }


    public class Cookie
    {
        public string CookieName { get; set; }
        public CookieTypes CookieType { get; set; }
        public enum CookieTypes
        {
            LanguageId
        }
    }
    public class Log
    {
        public string LogPath { get; set; }
        public LogTypes logtype { get; set; }

        public enum LogTypes
        {
            Logger
        }
    }
    public string? GetLogsRoot(Log.LogTypes LogType)
    {
        return Logs.Where(log => log.logtype == LogType).FirstOrDefault()?.LogPath;
    }

    public class Databases
    {
        public string DatabaseName { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseHost { get; set; }
        public int DatabasePort { get; set; }
        public string GetConnectionString()
        {
            var port = DatabasePort == 0 || DatabasePort == default ? "3306" : $"Port={DatabasePort}";
            return $"Server={DatabaseHost};Database={DatabaseName};User ID={DatabaseUser};Password={DatabasePassword};Port={port};";
        }
    }
}
