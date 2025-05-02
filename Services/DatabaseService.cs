using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using DynamicChartApp.Models;

namespace DynamicChartApp.Services
{
    public class DatabaseService
    {
        public async Task<(bool success, string message)> TestConnection(DatabaseConnection connection)
        {
            var connectionString = BuildConnectionString(connection);
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    return (true, "Connection successful");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Connection failed: {ex.Message}");
            }
        }

        public async Task<List<DatabaseObject>> GetDatabaseObjects(DatabaseConnection connection)
        {
            var objects = new List<DatabaseObject>();
            var connectionString = BuildConnectionString(connection);

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT name, 'View' as Type, SCHEMA_NAME(schema_id) as SchemaName
                        FROM sys.views
                        UNION ALL
                        SELECT name, 'StoredProcedure', SCHEMA_NAME(schema_id)
                        FROM sys.procedures
                        UNION ALL
                        SELECT name, 'Function', SCHEMA_NAME(schema_id)
                        FROM sys.objects
                        WHERE type IN ('FN', 'TF', 'IF')";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            objects.Add(new DatabaseObject
                            {
                                Name = reader.GetString(0),
                                Type = reader.GetString(1),
                                Schema = reader.GetString(2)
                            });
                        }
                    }
                }
            }
            return objects;
        }

        public async Task<(List<ColumnInfo> Columns, DataTable Data)> ExecuteQuery(DatabaseConnection connection, string objectName, string schema)
        {
            var connectionString = BuildConnectionString(connection);
            var columns = new List<ColumnInfo>();
            var dataTable = new DataTable();

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT * FROM [{schema}].[{objectName}]";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var schemaTable = reader.GetSchemaTable();
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            columns.Add(new ColumnInfo
                            {
                                Name = row["ColumnName"].ToString(),
                                DataType = row["DataType"].ToString()
                            });
                        }
                        dataTable.Load(reader);
                    }
                }
            }
            return (columns, dataTable);
        }

        public async Task<(List<ColumnInfo> Columns, DataTable Data)> ExecuteObject(
            DatabaseConnection connection, string objectType, string schema, string name, Dictionary<string, object> parameters)
        {
            var connectionString = BuildConnectionString(connection);
            var columns = new List<ColumnInfo>();
            var dataTable = new DataTable();

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    if (objectType == "View")
                        cmd.CommandText = $"SELECT * FROM [{schema}].[{name}]";
                    else if (objectType == "StoredProcedure")
                    {
                        cmd.CommandText = $"[{schema}].[{name}]";
                        cmd.CommandType = CommandType.StoredProcedure;
                        foreach (var param in parameters)
                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }
                    else if (objectType == "Function")
                    {
                        // For table-valued functions with parameters
                        var paramList = string.Join(",", parameters.Select(p => $"@{p.Key}"));
                        cmd.CommandText = $"SELECT * FROM [{schema}].[{name}]({paramList})";
                        foreach (var param in parameters)
                            cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var schemaTable = reader.GetSchemaTable();
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            columns.Add(new ColumnInfo
                            {
                                Name = row["ColumnName"].ToString(),
                                DataType = row["DataType"].ToString()
                            });
                        }
                        dataTable.Load(reader);
                    }
                }
            }
            return (columns, dataTable);
        }

        private string BuildConnectionString(DatabaseConnection connection)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = connection.Server,
                InitialCatalog = connection.Database,
                UserID = connection.Username,
                Password = connection.Password,
                TrustServerCertificate = true,
                IntegratedSecurity = connection.IntegratedSecurity
            };
            return builder.ConnectionString;
        }
    }
} 