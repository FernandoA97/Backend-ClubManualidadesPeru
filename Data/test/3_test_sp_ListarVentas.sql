CREATE OR ALTER PROCEDURE [Tests_Procedures].[test_sp_ListarVentas]
AS
BEGIN
    -- Aislar tabla de ventas
    EXEC tSQLt.FakeTable 'dbo', 'Ventas';

    INSERT INTO dbo.Ventas 
        (Serie, Numero, FechaDeMision, IdCliente, MetodoPago, MontoTotal, EstadoPago, Comentarios, FechaEntrega, EstadoVenta, RegionTienda)
    VALUES 
        ('F001', '0001', '2025-11-01', 1, 'Efectivo', 150.00, 'Pagado', 'Entrega inmediata', '2025-11-02', 'Completada', 'Lima'),
        ('F001', '0002', '2025-11-02', 2, 'Tarjeta', 200.00, 'Pendiente', 'Pago con tarjeta', NULL, 'Pendiente', 'Cusco'),
        ('F001', '0003', '2025-11-03', 3, 'Yape', 120.00, 'Pagado', 'Venta por app', '2025-11-04', 'Completada', 'Arequipa');

    CREATE TABLE #resultado (
        Id INT,
        Serie NVARCHAR(50),
        Numero NVARCHAR(50),
        FechaDeMision DATETIME,
        IdCliente INT,
        MetodoPago NVARCHAR(50),
        MontoTotal DECIMAL(18,2),
        EstadoPago NVARCHAR(50),
        Comentarios NVARCHAR(500),
        FechaEntrega DATETIME,
        EstadoVenta NVARCHAR(50),
        RegionTienda NVARCHAR(100)
    );

    INSERT INTO #resultado
    EXEC dbo.sp_ListarVentas;

    -- Validaciones
    EXEC tSQLt.AssertEqualsTable 'dbo.Ventas', '#resultado';
END
GO