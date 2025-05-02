USE master;
GO

IF DB_ID('ChartTestDB') IS NULL
    CREATE DATABASE ChartTestDB;
GO

USE ChartTestDB;
GO

-- Drop objects in the correct order
IF OBJECT_ID('sp_SalesByPerson', 'P') IS NOT NULL DROP PROCEDURE sp_SalesByPerson;
GO
IF OBJECT_ID('fn_SalesByDate', 'IF') IS NOT NULL DROP FUNCTION fn_SalesByDate;
GO
IF OBJECT_ID('fn_TotalSalesForPerson', 'FN') IS NOT NULL DROP FUNCTION fn_TotalSalesForPerson;
GO
IF OBJECT_ID('vw_SalesByRegion', 'V') IS NOT NULL DROP VIEW vw_SalesByRegion;
GO
IF OBJECT_ID('SalesReport', 'U') IS NOT NULL DROP TABLE SalesReport;
GO

-- Create the table
CREATE TABLE SalesReport (
    Id INT PRIMARY KEY IDENTITY,
    SaleDate DATE,
    SalesPerson NVARCHAR(50),
    Amount DECIMAL(10,2),
    Region NVARCHAR(50)
);
GO

-- Insert data
INSERT INTO SalesReport (SaleDate, SalesPerson, Amount, Region) VALUES
('2024-01-01', 'Alice', 1200, 'East'),
('2024-01-02', 'Bob', 900, 'West'),
('2024-01-03', 'Charlie', 1500, 'East'),
('2024-01-04', 'Alice', 1100, 'North'),
('2024-01-05', 'Bob', 1300, 'South');
GO

-- Create the view (must be first in batch)
CREATE VIEW vw_SalesByRegion AS
SELECT Region, SUM(Amount) AS TotalSales
FROM SalesReport
GROUP BY Region;
GO

-- Create the scalar function (must be first in batch)
CREATE FUNCTION fn_TotalSalesForPerson(@person NVARCHAR(50))
RETURNS DECIMAL(10,2)
AS
BEGIN
    RETURN (SELECT SUM(Amount) FROM SalesReport WHERE SalesPerson = @person)
END
GO

-- Create the table-valued function (must be first in batch)
CREATE FUNCTION fn_SalesByDate(@startDate DATE, @endDate DATE)
RETURNS TABLE
AS
RETURN (
    SELECT SaleDate, SUM(Amount) AS TotalSales
    FROM SalesReport
    WHERE SaleDate BETWEEN @startDate AND @endDate
    GROUP BY SaleDate
)
GO

-- Create the stored procedure (must be first in batch)
CREATE PROCEDURE sp_SalesByPerson
    @person NVARCHAR(50)
AS
BEGIN
    SELECT SaleDate, Amount
    FROM SalesReport
    WHERE SalesPerson = @person
END
GO