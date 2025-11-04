CREATE DATABASE BDManualidades
GO

USE BDManualidades
GO

CREATE TABLE [dbo].[Clientes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[NombreCliente] [nvarchar](200) NOT NULL,
	[Telefono] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[Direccion] [nvarchar](300) NULL,
	[TipoCliente] [nvarchar](50) NULL,
	[RUC] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/******************************************************************/


CREATE TABLE [dbo].[HistorialImportacion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FechaImportacion] [datetime] NULL,
	[Usuario] [varchar](100) NOT NULL,
	[RegistrosValidos] [int] NOT NULL,
	[RegistrosRechazados] [int] NOT NULL,
	[RegistrosDuplicados] [int] NOT NULL,
	[NombreArchivo] [varchar](max) NULL,
	[Estado] [varchar](10) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


/*******************************************************************/

CREATE TABLE [dbo].[Productos](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CodigoSKU] [nvarchar](50) NOT NULL,
	[NombreProducto] [nvarchar](200) NOT NULL,
	[PrecioUnitario] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO Productos (CodigoSKU, NombreProducto, PrecioUnitario) VALUES ('P1124555', 'Gaseosa', 10.50);


/*********************************************************************/


CREATE TABLE [dbo].[Ventas](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Serie] [nvarchar](50) NOT NULL,
	[Numero] [nvarchar](50) NOT NULL,
	[FechaDeMision] [datetime] NOT NULL,
	[IdCliente] [int] NOT NULL,
	[MetodoPago] [nvarchar](50) NULL,
	[MontoTotal] [decimal](18, 2) NOT NULL,
	[EstadoPago] [nvarchar](50) NULL,
	[Comentarios] [nvarchar](500) NULL,
	[FechaEntrega] [datetime] NULL,
	[EstadoVenta] [nvarchar](50) NULL,
	[RegionTienda] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Clientes] FOREIGN KEY([IdCliente])
REFERENCES [dbo].[Clientes] ([Id])
GO

ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Clientes]
GO
/**************************************************/



CREATE TABLE [dbo].[Venta_Detalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IdVenta] [int] NOT NULL,
	[IdProducto] [int] NOT NULL,
	[Cantidad] [int] NOT NULL,
	[PrecioUnitario] [decimal](18, 2) NOT NULL,
	[Descuento] [decimal](18, 2) NOT NULL,
	[Subtotal] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Venta_Detalle] ADD  DEFAULT ((0)) FOR [Cantidad]
GO

ALTER TABLE [dbo].[Venta_Detalle] ADD  DEFAULT ((0)) FOR [PrecioUnitario]
GO

ALTER TABLE [dbo].[Venta_Detalle] ADD  DEFAULT ((0)) FOR [Descuento]
GO

ALTER TABLE [dbo].[Venta_Detalle] ADD  DEFAULT ((0)) FOR [Subtotal]
GO

ALTER TABLE [dbo].[Venta_Detalle]  WITH CHECK ADD  CONSTRAINT [FK_VentaDetalle_Productos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[Productos] ([Id])
GO

ALTER TABLE [dbo].[Venta_Detalle] CHECK CONSTRAINT [FK_VentaDetalle_Productos]
GO

ALTER TABLE [dbo].[Venta_Detalle]  WITH CHECK ADD  CONSTRAINT [FK_VentaDetalle_Ventas] FOREIGN KEY([IdVenta])
REFERENCES [dbo].[Ventas] ([Id])
GO

ALTER TABLE [dbo].[Venta_Detalle] CHECK CONSTRAINT [FK_VentaDetalle_Ventas]
GO



/*******************************************************/

create proc [dbo].[sp_ListarHistorial_Importacion]
as
begin
	select * from HistorialImportacion
end
GO


CREATE PROC [dbo].[sp_ListarVentas]
as
begin
	select * from ventas 
end
GO


CREATE   PROCEDURE [dbo].[sp_ListarVentasConCliente]
AS
BEGIN
    SELECT 
        v.Id,
        v.Serie,
        v.Numero,
        v.FechaDeMision,
        c.NombreCliente,          
        v.MetodoPago,
        v.MontoTotal,
        v.EstadoPago,
        v.Comentarios,
        v.FechaEntrega,
        v.EstadoVenta,
        v.RegionTienda
    FROM Ventas v
    LEFT JOIN Clientes c ON v.IdCliente = c.Id
END
GO


