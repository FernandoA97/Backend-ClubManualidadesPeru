CREATE OR ALTER PROCEDURE [Tests_Procedures].[test_sp_ListarVentasConCliente]
AS
BEGIN
    EXEC tSQLt.FakeTable 'dbo', 'Clientes';
    EXEC tSQLt.FakeTable 'dbo', 'Ventas';

    INSERT INTO dbo.Clientes (Id, NombreCliente, Telefono, Email, Direccion, TipoCliente, RUC)
VALUES 
    (1, 'Cesar Aguilar', '999888777', 'cesar@correo.com', 'Lima', 'Regular', '12345678901'),
    (2, 'Lucía Torres', '999777666', 'lucia@correo.com', 'Cusco', 'Premium', '10987654321'),
    (3, 'Mario Castro', '999555444', 'mario@correo.com', 'Arequipa', 'Regular', '10293847561');

    INSERT INTO dbo.Ventas 
        (Serie, Numero, FechaDeMision, IdCliente, MetodoPago, MontoTotal, EstadoPago, Comentarios, FechaEntrega, EstadoVenta, RegionTienda)
    VALUES 
        ('B001', '0001', '2025-11-01', 1, 'Efectivo', 120.00, 'Pagado', 'Entrega hoy', '2025-11-02', 'Completada', 'Lima'),
        ('B001', '0002', '2025-11-02', 2, 'Tarjeta', 250.00, 'Pendiente', 'Pago parcial', '2025-11-03', 'Pendiente', 'Cusco'),
        ('B001', '0003', '2025-11-03', 3, 'Plin', 95.00, 'Pagado', 'Compra online', '2025-11-04', 'Completada', 'Arequipa'),
        ('B001', '0004', '2025-11-04', 999, 'Efectivo', 99.00, 'Pagado', 'Cliente sin registro', NULL, 'Completada', 'Tacna');

    CREATE TABLE #resultado (
        Id INT,
        Serie NVARCHAR(50),
        Numero NVARCHAR(50),
        FechaDeMision DATETIME,
        NombreCliente NVARCHAR(200),
        MetodoPago NVARCHAR(50),
        MontoTotal DECIMAL(18,2),
        EstadoPago NVARCHAR(50),
        Comentarios NVARCHAR(500),
        FechaEntrega DATETIME,
        EstadoVenta NVARCHAR(50),
        RegionTienda NVARCHAR(100)
    );

    INSERT INTO #resultado
    EXEC dbo.sp_ListarVentasConCliente;

    DECLARE @esperado INT = (SELECT COUNT(*) FROM dbo.Ventas);
    DECLARE @obtenido INT = (SELECT COUNT(*) FROM #resultado);
    EXEC tSQLt.AssertEquals @esperado, @obtenido;

    DECLARE @cliente1 NVARCHAR(200) = (SELECT TOP 1 NombreCliente FROM #resultado WHERE Numero = '0001');
    EXEC tSQLt.AssertEqualsString 'Cesar Aguilar', @cliente1;

    DECLARE @clienteInexistente NVARCHAR(200) = (SELECT TOP 1 NombreCliente FROM #resultado WHERE Numero = '0004');
    EXEC tSQLt.AssertEqualsString NULL, @clienteInexistente;

    DECLARE @totalRegiones INT = (SELECT COUNT(DISTINCT RegionTienda) FROM #resultado);
    DECLARE @esperadoRegion BIT = 1;
    DECLARE @condicion BIT = CASE WHEN @totalRegiones >= 3 THEN 1 ELSE 0 END;
    EXEC tSQLt.AssertEquals @esperadoRegion, @condicion;

END
GO
