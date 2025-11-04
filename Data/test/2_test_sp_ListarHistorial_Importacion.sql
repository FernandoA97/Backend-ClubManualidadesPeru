
CREATE OR ALTER PROCEDURE [Tests_Procedures].[test_sp_ListarHistorial_Importacion]
AS
BEGIN
    -- Aislar completamente la tabla real
    EXEC tSQLt.FakeTable 'dbo', 'HistorialImportacion';

    -- Insertar data simulada (no afecta a la real)
    INSERT INTO dbo.HistorialImportacion 
        (FechaImportacion, Usuario, RegistrosValidos, RegistrosRechazados, RegistrosDuplicados, NombreArchivo, Estado)
    VALUES 
        ('2025-10-01', 'Admin', 120, 2, 0, 'import_octubre.xlsx', 'OK'),
        ('2025-10-15', 'Soporte', 85, 10, 3, 'import_parcial.xlsx', 'ERR'),
        ('2025-11-01', 'Cesar', 150, 0, 0, 'import_full.xlsx', 'OK');

    CREATE TABLE #resultado (
        Id INT,
        FechaImportacion DATETIME,
        Usuario VARCHAR(100),
        RegistrosValidos INT,
        RegistrosRechazados INT,
        RegistrosDuplicados INT,
        NombreArchivo VARCHAR(MAX),
        Estado VARCHAR(10)
    );

    INSERT INTO #resultado
    EXEC dbo.sp_ListarHistorial_Importacion;

    -- Validaciones
    DECLARE @esperado INT = (SELECT COUNT(*) FROM dbo.HistorialImportacion);
    DECLARE @obtenido INT = (SELECT COUNT(*) FROM #resultado);
    EXEC tSQLt.AssertEquals @esperado, @obtenido;

    DECLARE @usuario NVARCHAR(100) = (SELECT TOP 1 Usuario FROM #resultado WHERE Estado = 'OK');
    EXEC tSQLt.AssertEqualsString 'Admin', @usuario;
END
GO