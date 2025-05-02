using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using DynamicChartApp.Models;


namespace DynamicChartApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChartController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public ChartController(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        // GET: api/chart/static
        [HttpGet("static")]
        public IActionResult GetStaticChartData()
        {
            var data = new ChartData
            {
                Labels = new List<string> { "Jan", "Feb", "Mar" },
                Values = new List<int> { 100, 150, 200 }
            };
            return Ok(data);
        }

        // POST: api/chart/connect
        [HttpPost("connect")]
        public IActionResult ConnectAndFetch([FromBody] ChartRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request details are required");
            }

            var connStr = $"Server={request.Server};Database={request.Database};User Id={request.Username};Password={request.Password};TrustServerCertificate=True;";
            var data = new ChartData
            {
                Labels = new List<string>(),
                Values = new List<int>()
            };

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand("SELECT Label, Value FROM ChartView", conn); // Ensure ChartView exists
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        data.Labels.Add(reader["Label"].ToString());
                        data.Values.Add(Convert.ToInt32(reader["Value"]));
                    }
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection([FromBody] DatabaseConnection connection)
        {
            try
            {
                if (connection == null)
                {
                    return BadRequest("Connection details are required");
                }

                var (success, message) = await _databaseService.TestConnection(connection);
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost("database-objects")]
        public async Task<IActionResult> GetDatabaseObjects([FromBody] DatabaseConnection connection)
        {
            try
            {
                if (connection == null)
                {
                    return BadRequest("Connection details are required");
                }

                var objects = await _databaseService.GetDatabaseObjects(connection);
                return Ok(objects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve database objects", details = ex.Message });
            }
        }

        [HttpPost("execute-query")]
        public async Task<IActionResult> ExecuteQuery(
            [FromBody] DatabaseConnection connection,
            [FromQuery] string objectName,
            [FromQuery] string schema)
        {
            try
            {
                if (connection == null)
                {
                    return BadRequest("Connection details are required");
                }

                if (string.IsNullOrWhiteSpace(objectName) || string.IsNullOrWhiteSpace(schema))
                {
                    return BadRequest("Both schema and object name are required");
                }

                // Validate object name and schema to prevent SQL injection
                if (!IsValidIdentifier(objectName) || !IsValidIdentifier(schema))
                {
                    return BadRequest("Invalid schema or object name");
                }

                var (columns, data) = await _databaseService.ExecuteQuery(connection, objectName, schema);
                
                // Convert DataTable to array of objects for better JSON serialization
                var rows = new List<Dictionary<string, object>>();
                foreach (DataRow row in data.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in data.Columns)
                    {
                        dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                    }
                    rows.Add(dict);
                }

                return Ok(new { columns, data = rows });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = "Database error", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred", details = ex.Message });
            }
        }

        private bool IsValidIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return false;

            // Basic validation for SQL identifiers
            // Only allows letters, numbers, and underscores
            // Must start with a letter or underscore
            return System.Text.RegularExpressions.Regex.IsMatch(identifier, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }
    }
}
