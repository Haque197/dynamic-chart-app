namespace DynamicChartApp.Models
{
    public class DatabaseConnection
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IntegratedSecurity { get; set; }
    }
}

public class DatabaseObject
{
    public string Name { get; set; }
    public string Type { get; set; } // "Function", "StoredProcedure", "View"
    public string Schema { get; set; }
}

public class ColumnInfo
{
    public string Name { get; set; }
    public string DataType { get; set; }
} 