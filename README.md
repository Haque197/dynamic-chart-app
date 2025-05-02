# Dynamic Chart Application

A full-stack ASP.NET Core web app that lets users connect to any SQL Server database, select a view, stored procedure, or function, and visualise the results as dynamic charts (Line, Bar, Radar) using Chart.js.

---

## Features

- **Dynamic SQL Connection:** Enter your own SQL Server connection details.
- **Object Discovery:** Lists all views, stored procedures, and table-valued functions in the selected database.
- **Parameter Support:** Enter parameters for stored procedures and functions.
- **Flexible Charting:** Visualize data as Line, Bar, or Radar charts.
- **Field Mapping:** Map any returned columns to chart axes.
- **Modern UI:** Built with Bootstrap, jQuery, and Chart.js.

---

## Getting Started

### 1. Prerequisites

- [.NET 6 SDK or later](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for local SQL Server)
- Modern web browser

---

### 2. Run SQL Server in Docker

```bash
docker run --name azuresqledge -e 'ACCEPT_EULA=1' -e 'MSSQL_SA_PASSWORD=Password123!' -p 1434:1433 -d mcr.microsoft.com/azure-sql-edge
```

---

### 3. Initialize the Database

1. Save the provided `setup.sql` file in your project directory.
2. Copy and run it in the container:

```bash
docker cp setup.sql azuresqledge:/setup.sql
docker exec -it azuresqledge /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Password123! -i /setup.sql
```

---

### 4. Run the Application

```bash
dotnet clean
dotnet build
dotnet run
```

Open your browser to [https://localhost:7235](https://localhost:7235) (or the port shown in your terminal).

---

### 5. Connect and Visualize

- **Server:** `localhost,1434`
- **Database:** `ChartTestDB`
- **Username:** `sa`
- **Password:** `Password123!`
- Uncheck "Use Windows Authentication"

Click **Connect**.  
Select a view, stored procedure, or function.  
Map fields to chart axes and generate your chart!

---

## Example SQL Objects & Outputs

### 1. **View: `vw_SalesByRegion`**

| Region | TotalSales |
|--------|-----------:|
| East   |      2700  |
| West   |       900  |
| North  |      1100  |
| South  |      1300  |

**Best Chart Types:** Bar, Radar

---

### 2. **Function: `fn_SalesByDate`**

| SaleDate   | TotalSales |
|------------|-----------:|
| 2024-01-01 |      1200  |
| 2024-01-02 |       900  |
| 2024-01-03 |      1500  |
| 2024-01-04 |      1100  |
| 2024-01-05 |      1300  |

**Best Chart Type:** Line

---

### 3. **Stored Procedure: `sp_SalesByPerson`**

| SaleDate   | Amount |
|------------|-------:|
| 2024-01-01 |  1200  |
| 2024-01-04 |  1100  |

**Best Chart Types:** Bar, Line

---

## Troubleshooting

- **No containers running?**  
  Run the Docker command above to start SQL Server.
- **Connection failed?**  
  Double-check your server, port, username, and password.
- **No data in chart?**  
  Make sure you select the correct fields for X and Y axes.


## Author

Arif Haque
