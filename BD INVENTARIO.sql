go
use master
go
IF NOT EXISTS(SELECT name FROM master.dbo.sysdatabases WHERE NAME = 'INVENTARIO')
CREATE DATABASE INVENTARIO
go
USE INVENTARIO
GO
--(1) TABLA MENU
if not exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'MENU')
create table MENU(
IdMenu int primary key identity(1,1),
Nombre varchar(60),
Icono varchar(60),
Activo bit default 1,
FechaRegistro datetime default getdate()
);
GO
--()REGISTROS EN TABLA MENU
INSERT INTO MENU(Nombre,Icono) VALUES
('Seguridad','fas fa-users-cog'),
('Mantenedor','fas fa-tools'),
('Gestión de Activos', 'fas fa-boxes'),
('Proveedores', 'fas fa-boxes'),
('Contribuyente','fas fa-book-reader'),
('Responsables','fas fa-users-cog'),
('Reportes','far fa-clipboard')

GO
--(2) TABLA SUBMENU
if not exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SUBMENU')
create table SUBMENU(
IdSubMenu int primary key identity(1,1),
IdMenu int references MENU(IdMenu),
Nombre varchar(60),
Controlador varchar(60),
Vista varchar(50),
Icono varchar(50),
Activo bit default 1,
FechaRegistro datetime default getdate()
);
GO
--()REGISTROS EN TABLA SUBMENU
INSERT INTO SUBMENU(IdMenu,Nombre,Controlador,Vista,Icono) VALUES

((SELECT TOP 1 IdMenu FROM MENU WHERE Nombre = 'Seguridad'),'Usuarios','Usuario','usuario','fas fa-address-card'),
((SELECT TOP 1 IdMenu FROM MENU WHERE Nombre = 'Seguridad'),'Perfil','Usuario','perfil','fas fa-address-card'),
((SELECT TOP 1 IdMenu FROM MENU WHERE Nombre = 'Seguridad'),'Administrar Rol','Rol','rol','fas fa-user-tag'),
((SELECT TOP 1 IdMenu FROM MENU WHERE Nombre = 'Seguridad'),'Asignar Permisos','Permisos','permisos','fas fa-user-lock'),
((SELECT TOP 1 IdMenu FROM MENU WHERE Nombre = 'Seguridad'),'Pistas Auditoria','Auditorias','auditorias','far fa-eye'),

((SELECT TOP 1 IdMenu FROM MENU WHERE Nombre = 'Mantenedor'),'Departamnetos','Departamentos','departamentos','fas fa-house-user'),

((SELECT TOP 1 IdMenu FROM MENU WHERE Nombre = 'Solicitante'),'Solicitante','Solicitantes','solicitantes','fas fa-user-edit'),

((SELECT TOP 1 IdMenu FROM MENU WHERE Nombre = 'Proveedores'), 'Registro de Proveedores',   'Proveedores',     'proveedores',     'fas fa-laptop'),

((SELECT TOP 1 IdMenu FROM MENU WHERE Nombre = 'Gestión de Activos'), 'Registro de Activos',   'Activos',     'activos',     'fas fa-laptop');

GO
--(3) TABLA ROL
if not exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'ROL')
create table ROL(
IdRol int primary key identity(1,1),
Descripcion varchar(60),
Activo bit default 1,
FechaRegistro datetime default getdate()
);
GO
--() REGISTROS EN TABLA ROL
INSERT INTO ROL(Descripcion) VALUES ('ADMINISTRADOR'),('USUARIOS'),('ABOGADOS'),('INGENERÍA')
GO
--(4) TABLA PERMISOS
if not exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'PERMISOS')
create table PERMISOS(
IdPermisos int primary key identity(1,1),
IdRol int references ROL(IdRol),
IdSubMenu int references SUBMENU(IdSubMenu),
Activo bit default 1,
FechaRegistro datetime default getdate()
);
GO
--REGISTROS EN TABLA PERMISOS
INSERT INTO PERMISOS(IdRol,IdSubMenu)
SELECT (select TOP 1 IdRol from ROL where Descripcion = 'ADMINISTRADOR'), IdSubMenu FROM SUBMENU
go
INSERT INTO PERMISOS(IdRol,IdSubMenu)
SELECT (select TOP 1 IdRol from ROL where Descripcion = 'USUARIOS'), IdSubMenu FROM SUBMENU
go
INSERT INTO PERMISOS(IdRol,IdSubMenu)
SELECT (select TOP 1 IdRol from ROL where Descripcion = 'ABOGADOS'), IdSubMenu FROM SUBMENU

go
INSERT INTO PERMISOS(IdRol,IdSubMenu)
SELECT (select TOP 1 IdRol from ROL where Descripcion = 'INGENERÍA'), IdSubMenu FROM SUBMENU

GO

--(5) TABLA INGRESOS
if not exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'INGRESO')
create table INGRESO(
IdUsuario int primary key identity(1,1),
TipoDocumento varchar(50), 
Cedula_Usuario varchar (250) NOT NULL UNIQUE,
Nom_Rol varchar(60),
Nom_Completo varchar(100),
Nom_User varchar(60),
Direccion VARCHAR(50) NULL,
Telefono1 INT NULL,
Telefono2 INT NULL,
Fax INT NULL,
Correo varchar(60),
Clave varchar(250),
IdRol int references ROL(IdRol),
Activo bit default 1,
Codigo_Recuperacion VARCHAR(6),
Fecha_Expiracion_Codigo DATETIME NULL,
FechaUltimoCambioClave DATETIME DEFAULT GETDATE(),
FechaRegistro datetime default getdate()
);
GO
--REGISTROS EN TABLA INGRESO
INSERT INTO INGRESO (Cedula_Usuario, Nom_Rol,Nom_Completo, Nom_User,Direccion,Telefono1, Correo, Clave, IdRol)
VALUES (206190030, 'ADMINISTRADOR','Ruth Esquivel Monge','resquivel','B jasus',88873170 ,'admin@gmail.com', '7932b2e116b076a54f452848eaabd5857f61bd957fe8a218faf216f24c9885bb', 1);

INSERT INTO dbo.INGRESO 
    (Cedula_Usuario, Nom_Rol, Nom_Completo, Nom_User, Direccion, Telefono1, Correo, Clave, IdRol)
VALUES 
    (118270165, 
     'ADMINISTRADOR',
     'Jafet Vásquez Sandoval',
     'JafetVS',
     'Rio Grande',
     63617248,
     'jafetcr01@gmail.com',
     LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256','Jafet123*'), 2)),
     1);

--(6) TABLA SOLICITANTES/PARTES
GO
if not exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SOLICITANTE')
create table SOLICITANTE(
ID_usuario INT PRIMARY KEY IDENTITY(1,1),
TipoDocumento varchar(50),
Cedula_Solicitante varchar (250) NOT NULL UNIQUE,
Nombre varchar(50),
Direccion varchar(50) NULL,
Telefono int NULL,
Telefono2 int NULL,
Fax INT NULL,
Correo VARCHAR(255) DEFAULT 'No Especificado',
Activo bit default 1,
FechaRegistro datetime default getdate()
);
GO
--REGISTROS EN TABLA SOLICITANTAS
INSERT INTO SOLICITANTE(TipoDocumento, Cedula_Solicitante,Nombre, Direccion, Telefono,Fax, Correo)
VALUES 
('C dula', 206190030,'Ruth Esquivel Monge','B.Jesus', 88888888,0,'esquivelruth321@gmail.com');
GO

INSERT INTO SOLICITANTE(TipoDocumento, Cedula_Solicitante,Nombre, Direccion, Telefono,Fax, Correo)
VALUES 
('C dula', 118270165,'Jafet Vásquez Sandoval','Rio Grande', 63617248,0,'jafetcr01@gmail.com');
GO
 --(10)  -************************************************************TABLA TIPOS DE DEPARTAMENTOS**********************************************************
if not exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'DEPARTAMENTOS')
create table DEPARTAMENTOS(
Id_Departamento int primary key identity(1,1),
Cod_Departamento int UNIQUE,
Descripcion varchar(100),
Activo bit default 1,
FechaRegistro datetime default getdate()
);
GO
INSERT INTO DEPARTAMENTOS (Cod_Departamento , Descripcion)
VALUES 
(1, 'TI'),
(2, 'Catastro'),
(3, 'Valoraciones'),
(4, 'Plataforma'),
(5, 'Tesorería'),
(6, 'Cajas'),
(7, 'Cementerio'),
(8, 'Mercado'),
(9, 'Estudio Ambiental'),
(10, 'Gestión Jurídica'),
(11, 'Alcaldía'),
(12, 'UTGV'),
(13, 'Presupuesto'),
(14, 'Contabilidad'),
(15, 'Archivo'),
(16, 'Recursos Humanos'),
(17, 'Cobros'),
(18, 'Gestión Social'),
(19, 'Ingeniería'),
(20, 'Bienes Inmuebles'),
(21, 'Planificación'),
(22, 'Patentes'),
(23, 'Comunicación'),
(24, 'Gestión Turística'),
(25, 'Proveeduría'),
(26, 'Salud Ocupacional'),
(27, 'Concejo'),
(28, 'Auditoría'),
(29, 'Control Fiscal y Urbano'),
(30, 'Parquímetros'),
(31, 'Policía Municipal'),
(32, 'Financiero');



GO

if not exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'TIEMPOLABORAL')
CREATE TABLE TIEMPOLABORAL(
    Cod_TiempoLaboral INT IDENTITY(1,1) PRIMARY KEY,
    Cedula_Usuario VARCHAR(250) references INGRESO(Cedula_Usuario),
    Fecha_TiempoLaboral DATE NOT NULL DEFAULT CONVERT(DATE, GETDATE()), 
    Hora_Ingreso TIME NULL,  -- Permitir valores NULL
    Hora_Cierre TIME NULL   -- Permitir valores NULL
);
GO
--()  -*****************************************TABLA REGISTROS DE PISTAS DE AUDITORIAS*********************************************
if not exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AUDITORIA')
CREATE TABLE AUDITORIA(
    Cod_Auditoria INT IDENTITY(1,1) PRIMARY KEY,
    Cedula_Usuario VARCHAR(250) references INGRESO(Cedula_Usuario),
	Nom_Rol varchar(60),
	Tabla varchar(250),
	Procedimiento varchar(250),
	ID_ColumAfectada INT NULL,
    Modificacion_JSON NVARCHAR(MAX) NULL,
    Fecha_Modificacion  datetime default getdate()
);


------------------------------------------------------------
-- CATEGORÍAS
------------------------------------------------------------
IF OBJECT_ID('dbo.ACTIVO_CATEGORIA','U') IS NULL
CREATE TABLE dbo.ACTIVO_CATEGORIA(
  IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
  Nombre VARCHAR(80) NOT NULL UNIQUE,
  Activo BIT NOT NULL DEFAULT 1,
  FechaRegistro DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Semilla inicial (solo si está vacío)
IF NOT EXISTS (SELECT 1 FROM dbo.ACTIVO_CATEGORIA)
INSERT INTO dbo.ACTIVO_CATEGORIA(Nombre) VALUES
('Servidor'),('CPU'),('Mini PC'),('Laptop'),('Monitor'),
('Teléfono'),('UPS'),('Teleféricos');
GO

------------------------------------------------------------
-- PROVEEDORES
------------------------------------------------------------
IF OBJECT_ID('dbo.PROVEEDOR','U') IS NULL
CREATE TABLE dbo.PROVEEDOR(
  IdProveedor INT IDENTITY(1,1) PRIMARY KEY,
  NombreEmpresa VARCHAR(150) NOT NULL,
  Servicio VARCHAR(150) NULL,
  NoContrato VARCHAR(50) NULL,
  NoProcedimiento VARCHAR(50) NULL,
  RenovacionContrato BIT NOT NULL DEFAULT 0,
  FechaVencimientoContrato DATE NULL,
  Contacto1_Nombre VARCHAR(100) NULL,
  Contacto1_Telefono VARCHAR(30) NULL,
  Contacto1_Correo VARCHAR(120) NULL,
  Contacto2_Nombre VARCHAR(100) NULL,
  Contacto2_Telefono VARCHAR(30) NULL,
  Contacto2_Correo VARCHAR(120) NULL,
  PaginaWeb VARCHAR(200) NULL,
  Activo BIT NOT NULL DEFAULT 1,

   -- Auditoría
  IdUsuarioCrea     INT NOT NULL
    FOREIGN KEY REFERENCES dbo.INGRESO(IdUsuario),
  FechaRegistro     DATETIME NOT NULL DEFAULT GETDATE()
);
GO

------------------------------------------------------------
-- (Opcional) Departamento en SOLICITANTE para ligarlo
------------------------------------------------------------
IF COL_LENGTH('dbo.SOLICITANTE','Id_Departamento') IS NULL
  ALTER TABLE dbo.SOLICITANTE ADD Id_Departamento INT NULL;
IF EXISTS (SELECT 1 FROM sys.objects WHERE name='DEPARTAMENTOS' AND type='U')
  IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name='FK_SOLICITANTE_DEPARTAMENTOS'
  )
  ALTER TABLE dbo.SOLICITANTE
  ADD CONSTRAINT FK_SOLICITANTE_DEPARTAMENTOS
  FOREIGN KEY (Id_Departamento) REFERENCES dbo.DEPARTAMENTOS(Id_Departamento);
GO

/* =========================================================
   CATÁLOGOS: TIPO y CLASE
========================================================= */
IF OBJECT_ID('dbo.ACTIVO_TIPO','U') IS NULL
CREATE TABLE dbo.ACTIVO_TIPO(
  IdTipo         INT IDENTITY(1,1) PRIMARY KEY,
  Nombre         VARCHAR(40) NOT NULL UNIQUE,
  Activo         BIT NOT NULL DEFAULT 1,
  FechaRegistro  DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ACTIVO_TIPO)
INSERT INTO dbo.ACTIVO_TIPO(Nombre) VALUES
('SinDefinir'), ('Hardware'), ('Software'), ('Datos'), ('Servicios');
GO

IF OBJECT_ID('dbo.ACTIVO_CLASE','U') IS NULL
CREATE TABLE dbo.ACTIVO_CLASE(
  IdClase        INT IDENTITY(1,1) PRIMARY KEY,
  Nombre         VARCHAR(80) NOT NULL UNIQUE,
  Activo         BIT NOT NULL DEFAULT 1,
  FechaRegistro  DATETIME NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ACTIVO_CLASE)
INSERT INTO dbo.ACTIVO_CLASE(Nombre) VALUES
('SinDefinir'),
('Activos Críticos'),
('Activos Esenciales'),
('Activos Relevantes'),
('Activos de Apoyo'),
('Activos Secundarios'),
('Activos Logísticos / Digitales');
GO


/* =========================================================
   ACTIVOS (Inventario de equipos)
   - Sin unicidad en Serie (permitidos duplicados)
========================================================= */
IF OBJECT_ID('dbo.ACTIVO','U') IS NULL
BEGIN
  CREATE TABLE dbo.ACTIVO(
    IdActivo INT IDENTITY(1,1) PRIMARY KEY,
    Codigo VARCHAR(30) NOT NULL,                    -- Identificador
    NombreTipo VARCHAR(100) NOT NULL,               -- Nombre/Tipo
    Descripcion VARCHAR(300) NULL,
    IdCategoria INT NOT NULL
      FOREIGN KEY REFERENCES dbo.ACTIVO_CATEGORIA(IdCategoria),
    Clasificacion VARCHAR(20) NOT NULL,             -- EnUso/EnReparacion/Desechado/SinDefinir
    Serie VARCHAR(80) NULL,                         -- <-- ahora puede repetirse
    Modelo VARCHAR(120) NULL,
    PlacaCodBarras VARCHAR(120) NULL,
    Procesador VARCHAR(120) NULL,
    RAM VARCHAR(120) NULL,
    SistemaOperativo VARCHAR(120) NULL,
    DiscoDuro VARCHAR(120) NULL,
    IP VARCHAR(50) NULL,
    Id_Departamento INT NULL
      FOREIGN KEY REFERENCES dbo.DEPARTAMENTOS(Id_Departamento),
    IdSolicitante INT NULL
      FOREIGN KEY REFERENCES dbo.SOLICITANTE(ID_usuario),  -- Responsable
    UsuarioLocal VARCHAR(120) NULL,
    OtraCaracteristica VARCHAR(300) NULL,
    FechaEntrega DATE NULL,

    -- Datos de compra:
    Costo DECIMAL(18,2) NULL,
    NoFactura VARCHAR(60) NULL,
    FechaCompra DATE NULL,
    IdProveedor INT NULL
      FOREIGN KEY REFERENCES dbo.PROVEEDOR(IdProveedor),

    -- NUEVOS CAMPOS:
    IdTipo INT NOT NULL
      FOREIGN KEY REFERENCES dbo.ACTIVO_TIPO(IdTipo),
    IdClase INT NOT NULL
      FOREIGN KEY REFERENCES dbo.ACTIVO_CLASE(IdClase),
    UbicacionFisica VARCHAR(150) NULL,
    UsoPrincipal VARCHAR(120) NULL,

    -- Auditoría base:
    Activo BIT NOT NULL DEFAULT 1,
    IdUsuarioCrea INT NOT NULL FOREIGN KEY REFERENCES dbo.INGRESO(IdUsuario),
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_ACTIVO_Codigo UNIQUE(Codigo),
    CONSTRAINT CK_ACTIVO_Clasificacion
      CHECK (Clasificacion IN ('EnUso','EnReparacion','Desechado','SinDefinir'))
  );

  -- Índices (OJO: Serie ahora NO es único)
  -- Antes: CREATE UNIQUE INDEX UQ_ACTIVO_Serie ...
  CREATE INDEX IX_ACTIVO_Serie 
    ON dbo.ACTIVO(Serie) 
    WHERE Serie IS NOT NULL;                          -- índice filtrado, no-único

  CREATE INDEX IX_ACTIVO_Filtros1 
    ON dbo.ACTIVO(IdCategoria, Id_Departamento, Clasificacion, IdProveedor);

  CREATE INDEX IX_ACTIVO_IdTipo  ON dbo.ACTIVO(IdTipo);
  CREATE INDEX IX_ACTIVO_IdClase ON dbo.ACTIVO(IdClase);
END
GO


------------------------------------------------------------
-- SOFTWARE instalado por activo
------------------------------------------------------------
IF OBJECT_ID('dbo.ACTIVO_SOFTWARE','U') IS NULL
CREATE TABLE dbo.ACTIVO_SOFTWARE(
  IdSoftware INT IDENTITY(1,1) PRIMARY KEY,
  IdActivo INT NOT NULL FOREIGN KEY REFERENCES dbo.ACTIVO(IdActivo),
  Nombre VARCHAR(150) NOT NULL,
  Editor VARCHAR(150) NULL,
  FechaInstalacion DATE NULL,
  Tamano VARCHAR(50) NULL,
  Version VARCHAR(50) NULL,
  IdUsuarioCrea INT NOT NULL FOREIGN KEY REFERENCES dbo.INGRESO(IdUsuario),
  FechaRegistro DATETIME NOT NULL DEFAULT GETDATE()
);
GO


------------------------------------------------------------
-- MOVIMIENTOS del activo (historial)
------------------------------------------------------------
IF OBJECT_ID('dbo.ACTIVO_MOVIMIENTO','U') IS NULL
CREATE TABLE dbo.ACTIVO_MOVIMIENTO(
  IdMovimiento INT IDENTITY(1,1) PRIMARY KEY,
  IdActivo INT NOT NULL FOREIGN KEY REFERENCES dbo.ACTIVO(IdActivo),
  IdSolicitante INT NOT NULL
    FOREIGN KEY REFERENCES dbo.SOLICITANTE(ID_usuario),  -- Responsable
  FechaMovimiento DATE NOT NULL DEFAULT GETDATE(),
  TipoMovimiento VARCHAR(20) NOT NULL,  -- ALTA/ASIGNACION/REUBICACION/REPARACION/BAJA
  Detalle VARCHAR(300) NULL,
  IdUsuario INT NOT NULL FOREIGN KEY REFERENCES dbo.INGRESO(IdUsuario)
);
GO

-- Índice para ordenar/consultar historial
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MOV_X_ActivoFecha' AND object_id=OBJECT_ID('dbo.ACTIVO_MOVIMIENTO'))
CREATE INDEX IX_MOV_X_ActivoFecha ON dbo.ACTIVO_MOVIMIENTO(IdActivo, FechaMovimiento DESC);
GO


------------------------------------------------------------
-- MANTENIMIENTOS del activo (historial técnico)
------------------------------------------------------------
IF OBJECT_ID('dbo.ACTIVO_MANTENIMIENTO','U') IS NULL
CREATE TABLE dbo.ACTIVO_MANTENIMIENTO(
  IdMantenimiento  INT IDENTITY(1,1) PRIMARY KEY,
  IdActivo         INT NOT NULL
    FOREIGN KEY REFERENCES dbo.ACTIVO(IdActivo),

  IdSolicitante    INT NOT NULL
    FOREIGN KEY REFERENCES dbo.SOLICITANTE(ID_usuario),   -- Responsable

  FechaMantenimiento DATE NOT NULL DEFAULT GETDATE(),      -- SOLO FECHA

  Tipo              VARCHAR(30) NOT NULL,                  -- p.ej. PREVENTIVO/CORRECTIVO/CALIBRACION/OTRO
  Detalle           VARCHAR(500) NULL,

  -- Auditoría
  IdUsuarioCrea     INT NOT NULL
    FOREIGN KEY REFERENCES dbo.INGRESO(IdUsuario),
  FechaRegistro     DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Índice para consultas por activo/fecha
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes
  WHERE name = 'IX_MANT_X_ActivoFecha'
    AND object_id = OBJECT_ID('dbo.ACTIVO_MANTENIMIENTO')
)
CREATE INDEX IX_MANT_X_ActivoFecha
ON dbo.ACTIVO_MANTENIMIENTO(IdActivo, FechaMantenimiento DESC);
GO

------------------------------------------------------------
-- ADJUNTOS por activo (binario en DB)
------------------------------------------------------------
IF OBJECT_ID('dbo.ACTIVO_ADJUNTO','U') IS NULL
CREATE TABLE dbo.ACTIVO_ADJUNTO(
  IdAdjunto      INT IDENTITY(1,1) PRIMARY KEY,
  IdActivo       INT NOT NULL
    FOREIGN KEY REFERENCES dbo.ACTIVO(IdActivo),

  NombreArchivo  VARCHAR(200) NOT NULL,     -- nombre visible
  Contenido      VARBINARY(MAX) NOT NULL,   -- archivo binario
  ContentType    VARCHAR(100) NULL,         -- ej. application/pdf (opcional)
  TamanoBytes    BIGINT       NULL,         -- tamaño (opcional)

  IdUsuarioCrea  INT NOT NULL
    FOREIGN KEY REFERENCES dbo.INGRESO(IdUsuario),
  FechaRegistro  DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Índice útil por activo/fecha
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes
  WHERE name='IX_ADJ_X_ActivoFecha' AND object_id=OBJECT_ID('dbo.ACTIVO_ADJUNTO')
)
CREATE INDEX IX_ADJ_X_ActivoFecha ON dbo.ACTIVO_ADJUNTO(IdActivo, FechaRegistro DESC);
GO
