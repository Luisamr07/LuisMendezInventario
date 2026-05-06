USE DBSISTEMA_INVENTARIO

GO

create table ROL (
IdRol int primary key identity,
Descripcion varchar(50),
FechaRegistro datetime default getdate()
)

go

create table PERMISO (
IdPermiso int primary key identity,
IdRol int references ROL(IdRol),
NombreMenu varchar(100),
FechaRegistro datetime default getdate()
)

go

create table PROVEEDOR (
Documento varchar(50) primary key,
RazonSocial varchar(50),
Correo varchar(50),
Telefono varchar(50),
Estado bit,
FechaRegistro datetime default getdate()
)

go

create table CLIENTE (
Documento varchar(50) primary key,
NombreCompleto varchar(50),
Correo varchar(50),
Telefono varchar(50),
Estado bit,
FechaRegistro datetime default getdate()
)

go

create table USUARIO (
Documento varchar(50) primary key,
NombreCompleto varchar(50),
Correo varchar(50),
Clave varchar(50),
IdRol int references ROL(IdRol),
Estado bit,
FechaRegistro datetime default getdate()
)

go

create table CATEGORIA (
IdCategoria int primary key identity,
Descripcion varchar(100),
Estado bit,
FechaRegistro datetime default getdate()
)


go

create table PRODUCTO (
Codigo varchar(50) primary key,
Nombre varchar(50),
Descripcion varchar(50),
IdCategoria int references CATEGORIA(IdCategoria),
Stock int not null default 0,
PrecioCompra decimal(10,2) default 0,
PrecioVenta decimal(10,2) default 0,
Estado bit,
FechaRegistro datetime default getdate()
)

go

create table COMPRA (
IdCompra int primary key identity,
IdUsuario varchar(50) references USUARIO(Documento),
IdProveedor varchar(50) references PROVEEDOR(Documento),
TipoDocumento varchar(50),
NumeroDocumento varchar(50),
MontoTotal decimal(10,2),
FechaRegistro datetime default getdate()
)

go

create table DETALLE_COMPRA (
IdDetalleCompra int primary key identity,
IdCompra int references COMPRA(IdCompra),
IdProducto varchar(50) references PRODUCTO(Codigo),
PrecioCompra decimal(10,2) default 0,
PrecioVenta decimal(10,2) default 0,
Cantidad int,
MontoTotal decimal(10,2),
FechaRegistro datetime default getdate()
)

go

create table VENTA (
IdVenta int primary key identity,
IdUsuario varchar(50) references USUARIO(Documento),
TipoDocumento varchar(50),
NumeroDocumento varchar(50),
DocumentoCliente varchar(50),
NombreCliente varchar(100),
MontoPago decimal(10,2),
MontoCambio decimal(10,2),
MontoTotal decimal(10,2),
FechaRegistro datetime default getdate()
)

go

create table DETALLE_VENTA (
IdDetalleVenta int primary key identity,
IdVenta int references VENTA(IdVenta),
IdProducto varchar(50) references PRODUCTO(Codigo),
PrecioVenta decimal(10,2),
Cantidad int,
SubTotal decimal(10,2),
FechaRegistro datetime default getdate()
)

go

create table NEGOCIO(
IdNegocio int primary key,
Nombre varchar(60),
RIF varchar(60),
Direccion varchar(60),
Logo varbinary(max) NULL
)

go
/* ====================================

    Procedimientos almacenados 

=====================================*/

CREATE PROCEDURE sp_verificar_stock
    @IdProducto VARCHAR(50),
    @Cantidad INT,
    @Operacion VARCHAR(10) -- 'compra' o 'venta'
AS
BEGIN
    IF @Operacion = 'compra'
    BEGIN
        IF EXISTS (SELECT 1 FROM PRODUCTO WHERE Codigo = @IdProducto AND Stock >= 0)
        BEGIN
            RETURN 1; -- Stock disponible
        END
        ELSE
        BEGIN
            RETURN 0; -- Sin stock
        END
    END
    ELSE IF @Operacion = 'venta'
    BEGIN
        IF EXISTS (SELECT 1 FROM PRODUCTO WHERE Codigo = @IdProducto AND Stock >= @Cantidad)
        BEGIN
            RETURN 1; -- Stock suficiente para la venta
        END
        ELSE
        BEGIN
            RETURN 0; -- Stock insuficiente
        END
    END
END;

go

CREATE TRIGGER trg_after_insert_detalle_compra
ON DETALLE_COMPRA
AFTER INSERT
AS
BEGIN
    DECLARE @IdProducto VARCHAR(50), @Cantidad INT, @PrecioCompra DECIMAL(10,2), @PrecioVenta DECIMAL(10,2);

    SELECT @IdProducto = IdProducto, @Cantidad = Cantidad, @PrecioCompra = PrecioCompra, @PrecioVenta = PrecioVenta
    FROM INSERTED;

    IF EXISTS (SELECT 1 FROM PRODUCTO WHERE Codigo = @IdProducto AND Stock >= 0)
    BEGIN
        UPDATE PRODUCTO
        SET Stock = Stock + @Cantidad,
            PrecioCompra = @PrecioCompra,
            PrecioVenta = @PrecioVenta
        WHERE Codigo = @IdProducto;
    END
END;

go

CREATE TRIGGER trg_after_insert_detalle_venta
ON DETALLE_VENTA
AFTER INSERT
AS
BEGIN
    DECLARE @IdProducto VARCHAR(50), @Cantidad INT;

    SELECT @IdProducto = IdProducto, @Cantidad = Cantidad
    FROM INSERTED;

    IF EXISTS (SELECT 1 FROM PRODUCTO WHERE Codigo = @IdProducto AND Stock >= @Cantidad)
    BEGIN
        UPDATE PRODUCTO
        SET Stock = Stock - @Cantidad
        WHERE Codigo = @IdProducto;
    END
END;

go

/* ====================================

    Creando registros

=====================================*/

GO

 insert into ROL (Descripcion)
 values('ADMINISTRADOR')

 GO

  insert into ROL (Descripcion)
 values('EMPLEADO')

 GO

 insert into USUARIO(Documento,NombreCompleto,Correo,Clave,IdRol,Estado)
 values 
 ('12345678','ADMIN','@GMAIL.COM','123456',1,1)

 GO


 insert into USUARIO(Documento,NombreCompleto,Correo,Clave,IdRol,Estado)
 values 
 ('98765432','EMPLEADO','@GMAIL.COM','123456',2,1)

 GO

  insert into PERMISO(IdRol,NombreMenu) values
  (1,'menuusuarios'),
  (1,'menumantenedor'),
  (1,'menuventas'),
  (1,'menucompras'),
  (1,'menuclientes'),
  (1,'menuproveedores'),
  (1,'menuinventario'),
  (1,'menureportes'),
  (1,'menuayuda'),
  (1,'menuacercade')

  GO

  insert into PERMISO(IdRol,NombreMenu) values
  (2,'menuventas'),
  (2,'menucompras'),
  (2,'menuclientes'),
  (2,'menuproveedores'),
  (2,'menuinventario'),
  (2,'menuayuda'),
  (2,'menuacercade')

  GO

  insert into NEGOCIO(IdNegocio,Nombre,RIF,Direccion,Logo) values
  (1,'COMECA','20202020','av intercomunal, puente miranda',null)