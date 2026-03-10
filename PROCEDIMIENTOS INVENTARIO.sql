USE [INVENTARIO]
/**************************************************PROCEDIMIENTOS PARA TABLAS ROLES*********************************************************************/

GO
--(1)PROCEDMIENTO PARA OBTENER ROLES
CREATE PROC [dbo].[usp_ObtenerRoles]
as
begin
 select IdRol, Descripcion,Activo from ROL
end
GO
--(2)PROCEDMIENTO PARA REGISTRAR ROLES
CREATE PROC [dbo].[usp_RegistrarRol](
@Descripcion varchar(50),
@Resultado bit output
)as
begin
	SET @Resultado = 1
	IF NOT EXISTS (SELECT * FROM ROL WHERE Descripcion = @Descripcion)
	begin
		declare @idrol int = 0
		insert into ROL(Descripcion) values (
		@Descripcion
		)
		set @idrol  = Scope_identity()

		insert into PERMISOS(IdRol,IdSubMenu,Activo)
		select @idrol, IdSubMenu,0 from SUBMENU
	end
	ELSE
		SET @Resultado = 0
end


GO
--(3)PROCEDMIENTO PARA MODIFICAR ROLES
CREATE procedure [dbo].[usp_ModificarRol](
@IdRol int,
@Descripcion varchar(60),
@Activo bit,
@Resultado bit output
)
as
begin
	SET @Resultado = 1
	IF NOT EXISTS (SELECT * FROM ROL WHERE Descripcion =@Descripcion and IdRol != @IdRol)
		update ROL set 
		Descripcion = @Descripcion,
		Activo = @Activo
		where IdRol = @IdRol
	ELSE
		SET @Resultado = 0
end
GO
--(4)PROCEDMIENTO PARA ELIMINAR ROLES
CREATE procedure [dbo].[usp_EliminarRol](
@IdRol int,
@Resultado bit output
)
as
begin
	SET @Resultado = 1

	--validamos que el rol no se encuentre asignado a algun usuario
	IF not EXISTS (select * from INGRESO u
	inner join ROL r on r.IdRol  = u.IdRol
	where r.IdRol = @IdRol)
	begin	
		delete from PERMISOS where IdRol = @IdRol
		delete from ROL where IdRol = @IdRol
	end
	ELSE
		SET @Resultado = 0
end
/**************************************************PROCEDIMIENTOS PARA ASIGNAR PERMISOS*********************************************************************/
GO
--(1)PROCEDMIENTO PARA OBTENER PERMISOS
CREATE PROCEDURE [dbo].[usp_ObtenerPermisos](
@IdRol int)
as
begin
select p.IdPermisos,m.Nombre[Menu],sm.Nombre[SubMenu],p.Activo from PERMISOS p
inner join SUBMENU sm on sm.IdSubMenu = p.IdSubMenu
inner join MENU m on m.IdMenu = sm.IdMenu
where p.IdRol = @IdRol
end

GO
--(2)PROCEDMIENTO PARA MODIFICAR PERMISOS
CREATE PROCEDURE [dbo].[usp_ModificarPermisos](
@Detalle xml,
@Resultado bit output
)
as
begin
begin try

	BEGIN TRANSACTION
	declare @permisos table(idpermisos int,activo bit)
	insert into @permisos(idpermisos,activo)
	select 
	idpermisos = Node.Data.value('(IdPermisos)[1]','int'),
	activo = Node.Data.value('(Activo)[1]','bit')
	FROM @Detalle.nodes('/DETALLE/PERMISO') Node(Data)
	select * from @permisos
	update p set p.Activo = pe.activo from PERMISOS p
	inner join @permisos pe on pe.idpermisos = p.IdPermisos
	COMMIT
	set @Resultado = 1
end try
begin catch
	ROLLBACK
	set @Resultado = 0
end catch
end

/**************************************************PROCEDIMIENTOS PARA ADMINISTRAR USUARIOS*********************************************************************/
GO
--(1)PROCEDMIENTO PARA OBTENER INGRESOS/USUARIOS
CREATE PROCEDURE [dbo].[usp_ObtenerUsuario] 
as
begin
 select u.IdUsuario,u.TipoDocumento,u.Cedula_Usuario,u.Nom_Rol,u.Nom_Completo,u.Nom_User,u.Direccion,u.Telefono1,u.Telefono2,u.Fax,u.Correo,u.Clave,u.IdRol,u.Activo,u.FechaRegistro,r.Descripcion[DescripcionRol],u.Activo from INGRESO u
 inner join ROL r on r.IdRol = u.IdRol
end

--(2)OBTENER DETALLES DEL USUARIO COMO/TIPO DE USUARIO/PERMISOS/ACTIVO ETC
GO
CREATE PROCEDURE [dbo].[usp_ObDetalleUsuario]
(
    @IdUsuario INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Obtener el rol del usuario
    DECLARE @IdRol INT;

    SELECT TOP 1 @IdRol = IdRol
    FROM INGRESO
    WHERE IdUsuario = @IdUsuario;

    -- XML de salida con detalles del usuario, incluyendo nuevas columnas
    SELECT 
        up.IdUsuario,
		up.TipoDocumento, 
        up.Cedula_Usuario,
        up.Nom_Rol,
        up.Nom_Completo,
        up.Nom_User,
        up.Direccion,
        up.Telefono1,
        up.Telefono2,
        up.Fax,
        up.Correo,
        up.Clave,
        up.IdRol,
        up.Activo,
        up.Codigo_Recuperacion,
        up.Fecha_Expiracion_Codigo,
        up.FechaRegistro,

        -- Detalle del Rol
        (
            SELECT * 
            FROM ROL r
            WHERE r.IdRol = @IdRol
            FOR XML PATH(''), TYPE
        ) AS 'DetalleRol',

        -- Detalle del Men� y SubMen�
        (
            SELECT 
                m.Nombre AS NombreMenu, 
                m.Icono,
                (
                    SELECT 
                        sm.Nombre AS NombreSubMenu,
                        sm.Controlador,
                        sm.Vista,
                        sm.Icono,
                        p.Activo
                    FROM PERMISOS p
                    INNER JOIN ROL r ON r.IdRol = p.IdRol
                    INNER JOIN SUBMENU sm ON sm.IdSubMenu = p.IdSubMenu
                    INNER JOIN MENU m2 ON m2.IdMenu = sm.IdMenu
                    WHERE sm.IdMenu = m.IdMenu
                      AND p.IdRol = @IdRol
                    FOR XML PATH('SubMenu'), TYPE
                ) AS 'DetalleSubMenu'
            FROM MENU m
            WHERE EXISTS (
                SELECT 1
                FROM PERMISOS p
                INNER JOIN SUBMENU sm ON sm.IdSubMenu = p.IdSubMenu
                WHERE sm.IdMenu = m.IdMenu
                  AND p.IdRol = @IdRol
                  AND p.Activo = 1
            )
            FOR XML PATH('Menu'), TYPE
        ) AS 'DetalleMenu'

    FROM INGRESO up
    WHERE up.IdUsuario = @IdUsuario
    FOR XML PATH(''), ROOT('Usuario');
END
GO
--(3)PROCEDIMEINTO PARA REGISTRAR USUARIO
CREATE PROCEDURE [dbo].[usp_RegistrarUsuario](
    @Cedula_Usuario varchar (250), 
    @Nom_Rol VARCHAR(60),
    @Nom_Completo VARCHAR(100),
    @Nom_User VARCHAR(60),
    @Direccion VARCHAR(50) = NULL,
    @Telefono1 INT = NULL,
    @Telefono2 INT = NULL,
    @Fax INT = NULL,
    @Correo VARCHAR(60),
    @Clave VARCHAR(100),
    @IdRol INT,
    @Activo BIT = 1,
    @Resultado BIT OUTPUT
)
AS
BEGIN
    -- Se asume que el resultado ser� exitoso
    SET @Resultado = 1;

    -- Verificar si ya existe otro usuario con la misma c�dula
    IF EXISTS (SELECT 1 FROM INGRESO WHERE Cedula_Usuario = @Cedula_Usuario)
    BEGIN
        -- Ya existe un usuario con esa c�dula, no se permite registrar
        SET @Resultado = 0;
        RETURN;
    END

    -- Si no existe, se registra el usuario
    INSERT INTO INGRESO (
        Cedula_Usuario, Nom_Rol, Nom_Completo, Nom_User, Direccion, 
        Telefono1, Telefono2, Fax, Correo, Clave, IdRol, Activo
    )
    VALUES (
        @Cedula_Usuario, @Nom_Rol, @Nom_Completo, @Nom_User, @Direccion, 
        @Telefono1, @Telefono2, @Fax, @Correo, @Clave, @IdRol, @Activo
    );
END
GO
--(4)MODIFICAR USUARIO 
CREATE PROCEDURE [dbo].[usp_ModificarUsuario]
(
    @IdUsuario INT,
    @Cedula_Usuario VARCHAR(250),
    @Nom_Rol VARCHAR(60),
    @Nom_Completo VARCHAR(100),
    @Nom_User VARCHAR(60),
    @Direccion VARCHAR(50),
    @Telefono1 INT = NULL,
    @Telefono2 INT = NULL,
    @Fax INT = NULL,
    @Correo VARCHAR(60),
    @Clave VARCHAR(100) = NULL,  -- Aseg�rate que sea opcional
    @IdRol INT,
    @Activo BIT,
    @Resultado BIT OUTPUT
)
AS
BEGIN
    SET @Resultado = 1;

    -- Verifica si ya existe otra persona con la misma c�dula pero distinto IdUsuario
    IF NOT EXISTS (
        SELECT 1 FROM INGRESO 
        WHERE Cedula_Usuario = @Cedula_Usuario 
        AND IdUsuario != @IdUsuario
    )
    BEGIN
        -- Obtener la clave actual si no se pasa una nueva
        IF (@Clave IS NULL OR LTRIM(RTRIM(@Clave)) = '')
        BEGIN
            SELECT @Clave = Clave FROM INGRESO WHERE IdUsuario = @IdUsuario;
        END

        UPDATE INGRESO
        SET
            Cedula_Usuario = @Cedula_Usuario,
            Nom_Rol = @Nom_Rol,
            Nom_Completo = @Nom_Completo,
            Nom_User = @Nom_User,
            Direccion = @Direccion,
            Telefono1 = @Telefono1,
            Telefono2 = @Telefono2,
            Fax = @Fax,
            Correo = @Correo,
            Clave = @Clave,
            IdRol = @IdRol,
            Activo = @Activo
        WHERE IdUsuario = @IdUsuario;
    END
    ELSE
    BEGIN
        SET @Resultado = 0;
    END
END;


--(5)PROCEDIMIENTO PARA ELIMINAR USUARIO 
GO
CREATE PROCEDURE [dbo].[usp_EliminarUsuario]
(
    @Cedula_Usuario INT,
    @Resultado BIT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Eliminar el usuario por su c�dula
    DELETE FROM INGRESO
    WHERE Cedula_Usuario = @Cedula_Usuario;

    -- Verificar si se elimin� alg�n registro
    IF @@ROWCOUNT > 0
    BEGIN
        SET @Resultado = 1;
    END
    ELSE
    BEGIN
        SET @Resultado = 0;
    END
END;
GO
CREATE PROCEDURE [dbo].[usp_NewCodRecuperacion]
    @Codigo NVARCHAR(6),
    @FechaExpiracion DATETIME,
    @Correo VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE INGRESO
    SET 
        Codigo_Recuperacion = @Codigo, 
        Fecha_Expiracion_Codigo = @FechaExpiracion
    WHERE Correo = @Correo;
END;





/**************************************************PROCEDIMIENTOS PARA ADMINISTRAR SOLICITANTES/PARTES*********************************************************************/
GO
--(1)PROCEDMIENTO PARA OBTENER SOLICITANTES
CREATE PROCEDURE [dbo].[usp_ObtenerSolicitante]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  ID_usuario, TipoDocumento, Cedula_Solicitante, Nombre, Direccion,
       Telefono, Telefono2, Fax, Correo, Activo, FechaRegistro
    FROM SOLICITANTE;
END;
GO
--(2)PROCEDMIENTO PARA OBTENER UN SOLICITANTES
CREATE PROCEDURE [dbo].[usp_ObtenerUnSolicitante]
    @Cedula_Solicitante VARCHAR(50)  -- Par�metro obligatorio
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ID_usuario, TipoDocumento, Cedula_Solicitante, Nombre, Direccion, Telefono, Telefono2,Fax, Correo, Activo, FechaRegistro
    FROM SOLICITANTE
    WHERE Cedula_Solicitante = @Cedula_Solicitante;
END;
--(3)PROCEDMIENTO PARA REGISTRAR SOLICITANTES
GO
CREATE PROCEDURE [dbo].[usp_RegistrarSolicitante]
    @TipoDocumento VARCHAR(50)=NULL,
    @Cedula_Solicitante VARCHAR(250), 
    @Nombre VARCHAR(50)=null,
    @Direccion VARCHAR(100), 
    @Telefono INT=NULL, 
	@Telefono2 INT=NULL, 
    @Fax INT = NULL, 
    @Correo VARCHAR(255),
    @Activo BIT = 1,
    @Resultado BIT OUTPUT -- Par�metro de salida
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Inicializar el resultado en 1 (�xito por defecto)
        SET @Resultado = 1;

        -- Verificar si ya existe un registro con la misma c�dula
        IF NOT EXISTS (SELECT 1 FROM SOLICITANTE WHERE Cedula_Solicitante = @Cedula_Solicitante)
        BEGIN
            -- Insertar un nuevo registro en la tabla
            INSERT INTO SOLICITANTE (TipoDocumento, Cedula_Solicitante, Nombre, Direccion, Telefono,Telefono2, Fax, Correo, Activo)
            VALUES (@TipoDocumento, @Cedula_Solicitante, @Nombre, @Direccion, @Telefono,@Telefono2, @Fax, @Correo, @Activo);
        END
        ELSE
        BEGIN
            -- Indicar que el registro ya existe
            SET @Resultado = 0;
        END
    END TRY
    BEGIN CATCH
        -- Manejar errores y retornar un resultado de falla
        SET @Resultado = 0;
    END CATCH
END;
--(4)PROCEDMIENTO PARA MODIFICAR SOLICITANTES
GO
CREATE PROCEDURE [dbo].[usp_ModificarSolicitante] (
    @TipoDocumento VARCHAR(50),
    @Cedula_Solicitante INT,
    @Nombre VARCHAR(50),
    @Direccion VARCHAR(50) = NULL,
    @Telefono INT = NULL,
    @Telefono2 INT = NULL,
    @Fax INT = NULL,
    @Correo VARCHAR(255),
    @Activo BIT,
    @Resultado BIT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 1;
    -- Verificar si el usuario con esa C�dula existe
    IF NOT EXISTS (SELECT 1 FROM SOLICITANTE WHERE Cedula_Solicitante = @Cedula_Solicitante)
    BEGIN
        SET @Resultado = 0;
        RETURN;
    END
    -- Realizar la actualizaci�n usando la c�dula como clave
    UPDATE SOLICITANTE
    SET 
        TipoDocumento = @TipoDocumento,
        Nombre = @Nombre,
        Direccion = ISNULL(@Direccion, Direccion),
        Telefono = @Telefono,
        Telefono2 = @Telefono2,
        Fax = @Fax,
        Correo = @Correo,
        Activo = @Activo
    WHERE Cedula_Solicitante = @Cedula_Solicitante;
END;



GO

--(5)PROCEDIMIENTO PARA ELIMINAR SOLICITANTE
CREATE PROCEDURE [dbo].[usp_EliminarSolicitante]
(
    @Cedula_Solicitante VARCHAR(250),
    @Resultado BIT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    -- Verificar si el usuario est� relacionado en la tabla CASOS
    IF EXISTS (
        SELECT 1
        FROM CASOS
        WHERE Cedula_Solicitante  = @Cedula_Solicitante 
    )
    BEGIN
        -- El usuario est� relacionado, no se puede eliminar
        SET @Resultado = 0;
    END
    ELSE
    BEGIN
        -- Eliminar el usuario por su c�dula
        DELETE FROM SOLICITANTE
        WHERE Cedula_Solicitante = @Cedula_Solicitante;

        -- Verificar si se elimin� alg�n registro
        IF @@ROWCOUNT > 0
        BEGIN
            SET @Resultado = 1;
        END
        ELSE
        BEGIN
            SET @Resultado = 0;
        END
    END
END;















/*****************************************************PROCEDIMIENTOS PARA TABLA DEPARTAMENTOS***********************************************************************/
--(1)PROCEDMIENTO PARA OBTENER DEPARTAMENTO
GO
CREATE PROCEDURE [dbo].[usp_ObtenerDepartamentos]
@Activo BIT = NULL -- Par�metro opcional para filtrar (activo/inactivo)
AS
BEGIN
    SET NOCOUNT ON;
    -- Consultar los registros de la tabla DEPARTAMENTOS
    SELECT 
        Id_Departamento, Cod_Departamento, Descripcion,  Activo,FechaRegistro
    FROM 
        DEPARTAMENTOS
	  WHERE 
        (@Activo IS NULL OR Activo = @Activo) -- Filtrar por estado si se proporciona
END;


--(2)PROCEDMIENTO PARA REGISTAR DEPARTAMENTO
GO
CREATE PROCEDURE [dbo].[usp_RegistrarDepartamento](
    @Descripcion varchar(100),
    @Activo bit,
    @Resultado bit OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    -- Inicializar el resultado como exitoso
    SET @Resultado = 1;

    -- Verificar si ya existe un registro con la misma descripci�n o c�digo
    IF NOT EXISTS (SELECT 1 FROM DEPARTAMENTOS WHERE Descripcion = @Descripcion)
    BEGIN
        -- Insertar en la tabla DEPARTAMENTOS
        INSERT INTO DEPARTAMENTOS (Descripcion, Activo)
        VALUES (@Descripcion, @Activo);
		 -- Obt�n el Id_Procedencia reci�n generado
        DECLARE @Id_Departamento INT = SCOPE_IDENTITY();
		-- Construye el Cod_Procedencia
        DECLARE @Cod_Departamento int = @Id_Departamento;
		 -- Actualiza el registro con el n�mero de expediente
        UPDATE DEPARTAMENTOS
        SET Cod_Departamento = @Cod_Departamento
        WHERE Id_Departamento = @Id_Departamento;
    END
    ELSE
    BEGIN
        -- Si ya existe, establecer el resultado como fallido
        SET @Resultado = 0;
    END
END;

--(3)PROCEDMIENTO PARA MODIFICAR DEPARTAMENTO 
GO
CREATE PROCEDURE [dbo].[usp_ModificarDepartamento](
    @Id_Departamento int,
    @Descripcion varchar(100),
    @Activo bit,
    @Resultado bit OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Inicializar el resultado como exitoso
    SET @Resultado = 1;

    -- Actualizar solo los campos Descripcion y Activo
    BEGIN TRY
        UPDATE DEPARTAMENTOS
        SET 
            Descripcion = @Descripcion,
            Activo = @Activo
        WHERE Id_Departamento = @Id_Departamento;

        -- Verificar si se realiz� la actualizaci�n
        IF @@ROWCOUNT = 0
        BEGIN
            SET @Resultado = 0; -- No se actualiz� ning�n registro
        END
    END TRY
    BEGIN CATCH
        -- Manejar errores
        SET @Resultado = 0;
    END CATCH
END;
--(4)PROCEDMIENTO PARA ELIMINAR DEPARTAMENTO 
GO
CREATE PROCEDURE [dbo].[usp_EliminarDepartamentos](
    @Cod_Departamento INT,
    @Resultado BIT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN
        DELETE FROM DEPARTAMENTOS
        WHERE Cod_Departamento = @Cod_Departamento;

        -- Verificar si la eliminaci�n fue exitosa
        IF @@ROWCOUNT > 0
        BEGIN
            SET @Resultado = 1;
        END
        ELSE
        BEGIN
            SET @Resultado = 0;
        END
    END
END;






/* =====================================================================
   MÓDULO: GESTIÓN DE ACTIVOS (Inventario)
   Descripción: CRUD de categorías, proveedores, activos, software,
                adjuntos y movimientos. Incluye validaciones de uso.
   ===================================================================== */


/* ===============================================================
   1) CATEGORÍAS DE ACTIVOS
   =============================================================== */

-- (1.1) LISTAR CATEGORÍAS
GO
CREATE OR ALTER PROC dbo.usp_Categoria_Listar
AS
BEGIN
  SET NOCOUNT ON;
  SELECT IdCategoria, Nombre, Activo, FechaRegistro
  FROM dbo.ACTIVO_CATEGORIA
  ORDER BY Nombre;
END
GO

-- (1.2) REGISTRAR CATEGORÍA
CREATE OR ALTER PROC dbo.usp_Categoria_Registrar
  @Nombre     VARCHAR(80),
  @Activo     BIT = 1,
  @IdGenerado INT  OUTPUT,
  @Resultado  BIT  OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET @Resultado = 1; SET @IdGenerado = NULL;

  IF EXISTS (SELECT 1 FROM dbo.ACTIVO_CATEGORIA WHERE Nombre = @Nombre)
  BEGIN
    SET @Resultado = 0;                -- ya existe
    RETURN;
  END

  INSERT INTO dbo.ACTIVO_CATEGORIA (Nombre, Activo)
  VALUES (@Nombre, @Activo);

  SET @IdGenerado = SCOPE_IDENTITY();
END
GO

-- (1.3) MODIFICAR CATEGORÍA
CREATE OR ALTER PROC dbo.usp_Categoria_Modificar
  @IdCategoria INT,
  @Nombre      VARCHAR(80),
  @Activo      BIT,
  @Resultado   BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET @Resultado = 1;

  -- evitar duplicados por nombre
  IF EXISTS (SELECT 1 FROM dbo.ACTIVO_CATEGORIA 
             WHERE Nombre = @Nombre AND IdCategoria <> @IdCategoria)
  BEGIN
    SET @Resultado = 0;
    RETURN;
  END

  UPDATE dbo.ACTIVO_CATEGORIA
    SET Nombre = @Nombre,
        Activo = @Activo
  WHERE IdCategoria = @IdCategoria;

  IF @@ROWCOUNT = 0 SET @Resultado = 0;
END
GO

-- (1.4) ELIMINAR CATEGORÍA (bloqueo si está en uso)
CREATE OR ALTER PROC dbo.usp_Categoria_Eliminar
  @IdCategoria INT,
  @Resultado   BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET @Resultado = 1;

  -- BLOQUEO: no permitir eliminar si hay activos con esa categoría
  IF EXISTS (SELECT 1 FROM dbo.ACTIVO WHERE IdCategoria = @IdCategoria)
  BEGIN
    SET @Resultado = 0;                -- en uso
    RETURN;
  END

  DELETE FROM dbo.ACTIVO_CATEGORIA WHERE IdCategoria = @IdCategoria;
  IF @@ROWCOUNT = 0 SET @Resultado = 0;
END
GO



/* ===============================================================
   2) PROVEEDORES
   =============================================================== */

-- (2.1) LISTAR PROVEEDORES (con filtros)
CREATE OR ALTER PROC dbo.usp_Proveedor_Listar
  @Texto                NVARCHAR(200) = NULL,
  @Servicio             VARCHAR(150)  = NULL,   -- <--- FALTABA
  @NoContrato           VARCHAR(50)   = NULL,
  @NoProcedimiento      VARCHAR(50)   = NULL,
  @RenovacionContrato   BIT           = NULL,
  @Activo               BIT           = NULL,
  @VenceDesde           DATE          = NULL,
  @VenceHasta           DATE          = NULL,
  @IdProveedor          INT           = NULL
AS
BEGIN
  SET NOCOUNT ON;

  SET @Texto = NULLIF(LTRIM(RTRIM(@Texto)), N'');
  SET @Servicio = NULLIF(LTRIM(RTRIM(@Servicio)), '');

  SELECT
      p.IdProveedor,
      p.NombreEmpresa,
      p.Servicio,
      p.NoContrato,
      p.NoProcedimiento,
      p.RenovacionContrato,
      p.FechaVencimientoContrato,
      p.Contacto1_Nombre,
      p.Contacto1_Telefono,
      p.Contacto1_Correo,
      p.Contacto2_Nombre,
      p.Contacto2_Telefono,
      p.Contacto2_Correo,
      p.PaginaWeb,
      p.Activo,
      p.IdUsuarioCrea,
      p.FechaRegistro
  FROM dbo.PROVEEDOR p
  WHERE
        (@IdProveedor          IS NULL OR p.IdProveedor = @IdProveedor)
    AND (@Activo               IS NULL OR p.Activo = @Activo)
    AND (@RenovacionContrato   IS NULL OR p.RenovacionContrato = @RenovacionContrato)
    AND (@NoContrato           IS NULL OR p.NoContrato = @NoContrato)
    AND (@NoProcedimiento      IS NULL OR p.NoProcedimiento = @NoProcedimiento)
    AND (@VenceDesde           IS NULL OR (p.FechaVencimientoContrato IS NOT NULL AND p.FechaVencimientoContrato >= @VenceDesde))
    AND (@VenceHasta           IS NULL OR (p.FechaVencimientoContrato IS NOT NULL AND p.FechaVencimientoContrato <= @VenceHasta))
    AND (
          @Servicio IS NULL
          OR ISNULL(p.Servicio, '') COLLATE Latin1_General_CI_AI LIKE N'%'+@Servicio+N'%'
        )
    AND (
          @Texto IS NULL
          OR p.NombreEmpresa                               COLLATE Latin1_General_CI_AI LIKE N'%'+@Texto+N'%'
          OR ISNULL(p.Servicio,              N'')          COLLATE Latin1_General_CI_AI LIKE N'%'+@Texto+N'%'
          OR ISNULL(p.NoContrato,            N'')          LIKE N'%'+@Texto+N'%'
          OR ISNULL(p.NoProcedimiento,       N'')          LIKE N'%'+@Texto+N'%'
          OR ISNULL(p.Contacto1_Nombre,      N'')          COLLATE Latin1_General_CI_AI LIKE N'%'+@Texto+N'%'
          OR ISNULL(p.Contacto1_Telefono,    N'')          LIKE N'%'+@Texto+N'%'
          OR ISNULL(p.Contacto1_Correo,      N'')          LIKE N'%'+@Texto+N'%'
          OR ISNULL(p.Contacto2_Nombre,      N'')          COLLATE Latin1_General_CI_AI LIKE N'%'+@Texto+N'%'
          OR ISNULL(p.Contacto2_Telefono,    N'')          LIKE N'%'+@Texto+N'%'
          OR ISNULL(p.Contacto2_Correo,      N'')          LIKE N'%'+@Texto+N'%'
          OR ISNULL(p.PaginaWeb,             N'')          LIKE N'%'+@Texto+N'%'
        )
  ORDER BY p.NombreEmpresa, p.IdProveedor;
END
GO



-- (2.2) REGISTRAR PROVEEDOR (con auditoría)
CREATE OR ALTER PROC dbo.usp_Proveedor_Registrar
  @NombreEmpresa            VARCHAR(150),
  @Servicio                 VARCHAR(150) = NULL,
  @NoContrato               VARCHAR(50)  = NULL,
  @NoProcedimiento          VARCHAR(50)  = NULL,
  @RenovacionContrato       BIT          = 0,
  @FechaVencimientoContrato DATE         = NULL,
  @Contacto1_Nombre         VARCHAR(100) = NULL,
  @Contacto1_Telefono       VARCHAR(30)  = NULL,
  @Contacto1_Correo         VARCHAR(120) = NULL,
  @Contacto2_Nombre         VARCHAR(100) = NULL,
  @Contacto2_Telefono       VARCHAR(30)  = NULL,
  @Contacto2_Correo         VARCHAR(120) = NULL,
  @PaginaWeb                VARCHAR(200) = NULL,
  @Activo                   BIT          = 1,
  @IdUsuarioCrea            INT,
  @IdGenerado               INT          OUTPUT,
  @Resultado                BIT          OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado  = 0;
  SET @IdGenerado = NULL;

  IF @NombreEmpresa IS NULL OR LTRIM(RTRIM(@NombreEmpresa)) = '' OR @IdUsuarioCrea IS NULL
  BEGIN
    RETURN;
  END

  BEGIN TRY
    INSERT INTO dbo.PROVEEDOR
    (NombreEmpresa, Servicio, NoContrato, NoProcedimiento, RenovacionContrato, FechaVencimientoContrato,
     Contacto1_Nombre, Contacto1_Telefono, Contacto1_Correo,
     Contacto2_Nombre, Contacto2_Telefono, Contacto2_Correo,
     PaginaWeb, Activo, IdUsuarioCrea, FechaRegistro)
    VALUES
    (@NombreEmpresa, @Servicio, @NoContrato, @NoProcedimiento, @RenovacionContrato, @FechaVencimientoContrato,
     @Contacto1_Nombre, @Contacto1_Telefono, @Contacto1_Correo,
     @Contacto2_Nombre, @Contacto2_Telefono, @Contacto2_Correo,
     @PaginaWeb, @Activo, @IdUsuarioCrea, GETDATE());

    SET @IdGenerado = SCOPE_IDENTITY();
    SET @Resultado  = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado  = 0;
    SET @IdGenerado = NULL;
  END CATCH
END
GO


-- (2.3) MODIFICAR PROVEEDOR (no altera auditoría)
CREATE OR ALTER PROC dbo.usp_Proveedor_Modificar
  @IdProveedor              INT,
  @NombreEmpresa            VARCHAR(150),
  @Servicio                 VARCHAR(150) = NULL,
  @NoContrato               VARCHAR(50)  = NULL,
  @NoProcedimiento          VARCHAR(50)  = NULL,
  @RenovacionContrato       BIT          = 0,
  @FechaVencimientoContrato DATE         = NULL,
  @Contacto1_Nombre         VARCHAR(100) = NULL,
  @Contacto1_Telefono       VARCHAR(30)  = NULL,
  @Contacto1_Correo         VARCHAR(120) = NULL,
  @Contacto2_Nombre         VARCHAR(100) = NULL,
  @Contacto2_Telefono       VARCHAR(30)  = NULL,
  @Contacto2_Correo         VARCHAR(120) = NULL,
  @PaginaWeb                VARCHAR(200) = NULL,
  @Activo                   BIT          = 1,
  @Resultado                BIT          OUTPUT
AS
BEGIN
  SET NOCOUNT ON; 
  SET @Resultado = 0;

  IF @IdProveedor IS NULL OR @IdProveedor <= 0 OR @NombreEmpresa IS NULL OR LTRIM(RTRIM(@NombreEmpresa)) = ''
  BEGIN
    RETURN;
  END

  BEGIN TRY
    UPDATE dbo.PROVEEDOR
       SET NombreEmpresa            = @NombreEmpresa,
           Servicio                 = @Servicio,
           NoContrato               = @NoContrato,
           NoProcedimiento          = @NoProcedimiento,
           RenovacionContrato       = @RenovacionContrato,
           FechaVencimientoContrato = @FechaVencimientoContrato,
           Contacto1_Nombre         = @Contacto1_Nombre,
           Contacto1_Telefono       = @Contacto1_Telefono,
           Contacto1_Correo         = @Contacto1_Correo,
           Contacto2_Nombre         = @Contacto2_Nombre,
           Contacto2_Telefono       = @Contacto2_Telefono,
           Contacto2_Correo         = @Contacto2_Correo,
           PaginaWeb                = @PaginaWeb,
           Activo                   = @Activo
     WHERE IdProveedor = @IdProveedor;

    IF @@ROWCOUNT > 0 SET @Resultado = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado = 0;
  END CATCH
END
GO


-- (2.4) ELIMINAR PROVEEDOR (bloqueo si está en uso)
CREATE OR ALTER PROC dbo.usp_Proveedor_Eliminar
  @IdProveedor INT,
  @Resultado   BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON; 
  SET @Resultado = 0;

  IF @IdProveedor IS NULL OR @IdProveedor <= 0
  BEGIN
    RETURN;
  END

  -- Bloqueo: hay activos asociados
  IF EXISTS (SELECT 1 FROM dbo.ACTIVO WHERE IdProveedor = @IdProveedor)
  BEGIN
    -- en uso
    RETURN;
  END

  BEGIN TRY
    DELETE FROM dbo.PROVEEDOR WHERE IdProveedor = @IdProveedor;
    IF @@ROWCOUNT > 0 SET @Resultado = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado = 0;
  END CATCH
END
GO


/* ===============================================================
   2) CATÁLOGO: TIPO DE ACTIVO (ACTIVO_TIPO)
   =============================================================== */

-- (2.1) LISTAR TIPOS
GO
CREATE OR ALTER PROC dbo.usp_Tipo_Listar
AS
BEGIN
  SET NOCOUNT ON;
  SELECT IdTipo, Nombre, Activo, FechaRegistro
  FROM dbo.ACTIVO_TIPO
  ORDER BY Nombre;
END
GO

-- (2.2) REGISTRAR TIPO
CREATE OR ALTER PROC dbo.usp_Tipo_Registrar
  @Nombre     VARCHAR(40),
  @Activo     BIT = 1,
  @IdGenerado INT  OUTPUT,
  @Resultado  BIT  OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET @Resultado = 1; SET @IdGenerado = NULL;

  IF EXISTS (SELECT 1 FROM dbo.ACTIVO_TIPO WHERE Nombre = @Nombre)
  BEGIN
    SET @Resultado = 0;                -- ya existe
    RETURN;
  END

  INSERT INTO dbo.ACTIVO_TIPO (Nombre, Activo)
  VALUES (@Nombre, @Activo);

  SET @IdGenerado = SCOPE_IDENTITY();
END
GO

-- (2.3) MODIFICAR TIPO
CREATE OR ALTER PROC dbo.usp_Tipo_Modificar
  @IdTipo   INT,
  @Nombre   VARCHAR(40),
  @Activo   BIT,
  @Resultado BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET @Resultado = 1;

  -- evitar duplicados por nombre
  IF EXISTS (SELECT 1 FROM dbo.ACTIVO_TIPO
             WHERE Nombre = @Nombre AND IdTipo <> @IdTipo)
  BEGIN
    SET @Resultado = 0;
    RETURN;
  END

  UPDATE dbo.ACTIVO_TIPO
     SET Nombre = @Nombre,
         Activo = @Activo
   WHERE IdTipo = @IdTipo;

  IF @@ROWCOUNT = 0 SET @Resultado = 0;
END
GO

-- (2.4) ELIMINAR TIPO (bloqueo si está en uso por ACTIVO)
CREATE OR ALTER PROC dbo.usp_Tipo_Eliminar
  @IdTipo    INT,
  @Resultado BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET @Resultado = 1;

  IF EXISTS (SELECT 1 FROM dbo.ACTIVO WHERE IdTipo = @IdTipo)
  BEGIN
    SET @Resultado = 0;                -- en uso
    RETURN;
  END

  DELETE FROM dbo.ACTIVO_TIPO WHERE IdTipo = @IdTipo;
  IF @@ROWCOUNT = 0 SET @Resultado = 0;
END
GO


/* ===============================================================
   3) CATÁLOGO: CLASE DE ACTIVO (ACTIVO_CLASE)
   =============================================================== */

-- (3.1) LISTAR CLASES
CREATE OR ALTER PROC dbo.usp_Clase_Listar
AS
BEGIN
  SET NOCOUNT ON;
  SELECT IdClase, Nombre, Activo, FechaRegistro
  FROM dbo.ACTIVO_CLASE
  ORDER BY Nombre;
END
GO

-- (3.2) REGISTRAR CLASE
CREATE OR ALTER PROC dbo.usp_Clase_Registrar
  @Nombre     VARCHAR(80),
  @Activo     BIT = 1,
  @IdGenerado INT  OUTPUT,
  @Resultado  BIT  OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET @Resultado = 1; SET @IdGenerado = NULL;

  IF EXISTS (SELECT 1 FROM dbo.ACTIVO_CLASE WHERE Nombre = @Nombre)
  BEGIN
    SET @Resultado = 0;                -- ya existe
    RETURN;
  END

  INSERT INTO dbo.ACTIVO_CLASE (Nombre, Activo)
  VALUES (@Nombre, @Activo);

  SET @IdGenerado = SCOPE_IDENTITY();
END
GO

-- (3.3) MODIFICAR CLASE
CREATE OR ALTER PROC dbo.usp_Clase_Modificar
  @IdClase   INT,
  @Nombre    VARCHAR(80),
  @Activo    BIT,
  @Resultado BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET @Resultado = 1;

  -- evitar duplicados por nombre
  IF EXISTS (SELECT 1 FROM dbo.ACTIVO_CLASE
             WHERE Nombre = @Nombre AND IdClase <> @IdClase)
  BEGIN
    SET @Resultado = 0;
    RETURN;
  END

  UPDATE dbo.ACTIVO_CLASE
     SET Nombre = @Nombre,
         Activo = @Activo
   WHERE IdClase = @IdClase;

  IF @@ROWCOUNT = 0 SET @Resultado = 0;
END
GO

-- (3.4) ELIMINAR CLASE (bloqueo si está en uso por ACTIVO)
CREATE OR ALTER PROC dbo.usp_Clase_Eliminar
  @IdClase   INT,
  @Resultado BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON; SET @Resultado = 1;

  IF EXISTS (SELECT 1 FROM dbo.ACTIVO WHERE IdClase = @IdClase)
  BEGIN
    SET @Resultado = 0;                -- en uso
    RETURN;
  END

  DELETE FROM dbo.ACTIVO_CLASE WHERE IdClase = @IdClase;
  IF @@ROWCOUNT = 0 SET @Resultado = 0;
END
GO



/* ===============================================================
   3) ACTIVOS (INVENTARIO)
   =============================================================== */

-- (3.1) LISTAR ACTIVOS (con filtros, incluye Tipo/Clase)
CREATE OR ALTER PROC dbo.usp_Activo_Listar
  @Texto         NVARCHAR(100) = NULL,  -- NVARCHAR para búsquedas con acentos
  @IdCategoria   INT           = NULL,
  @IdDepto       INT           = NULL,
  @Clasificacion NVARCHAR(20)  = NULL,  -- EnUso / EnReparacion / Desechado / SinDefinir
  /*@IdProveedor   INT           = NULL,*/
  @IdTipo        INT           = NULL,  -- NUEVO filtro
  @IdClase       INT           = NULL   -- NUEVO filtro
AS
BEGIN
  SET NOCOUNT ON;

  -- Normaliza texto
  SET @Texto = NULLIF(LTRIM(RTRIM(@Texto)), N'');

  SELECT
      a.IdActivo,
      a.Codigo,
      a.NombreTipo,
      a.Descripcion,
      a.IdCategoria,
      c.Nombre AS Categoria,
      a.Clasificacion,
      a.Serie,
      a.Modelo,
      a.PlacaCodBarras,
      a.Procesador,
      a.RAM,
      a.SistemaOperativo,
      a.DiscoDuro,
      a.IP,
      a.Id_Departamento,
      d.Descripcion AS Departamento,
      a.IdSolicitante,
      s.Nombre AS Responsable,
      a.UsuarioLocal,
      a.OtraCaracteristica,
      a.FechaEntrega,
      a.Costo,
      a.NoFactura,
      a.FechaCompra,
      a.IdProveedor,
      p.NombreEmpresa AS Proveedor,

      -- NUEVOS CAMPOS
      a.IdTipo,
      t.Nombre AS TipoNombre,
      a.IdClase,
      cl.Nombre AS ClaseNombre,
      a.UbicacionFisica,
      a.UsoPrincipal,

      a.Activo,
      a.IdUsuarioCrea,
      a.FechaRegistro
  FROM dbo.ACTIVO a
  INNER JOIN dbo.ACTIVO_CATEGORIA c
          ON c.IdCategoria = a.IdCategoria
  LEFT  JOIN dbo.DEPARTAMENTOS d
          ON d.Id_Departamento = a.Id_Departamento
  LEFT  JOIN dbo.SOLICITANTE s
          ON s.ID_usuario = a.IdSolicitante
  LEFT  JOIN dbo.PROVEEDOR p
          ON p.IdProveedor = a.IdProveedor
  LEFT  JOIN dbo.ACTIVO_TIPO t
          ON t.IdTipo = a.IdTipo
  LEFT  JOIN dbo.ACTIVO_CLASE cl
          ON cl.IdClase = a.IdClase
  WHERE
      (@IdCategoria   IS NULL OR a.IdCategoria     = @IdCategoria)
  AND (@IdDepto       IS NULL OR a.Id_Departamento = @IdDepto)
  AND (@Clasificacion IS NULL OR a.Clasificacion   = @Clasificacion)
  /*AND (@IdProveedor   IS NULL OR a.IdProveedor     = @IdProveedor)*/
  AND (@IdTipo        IS NULL OR a.IdTipo          = @IdTipo)         -- filtro NUEVO
  AND (@IdClase       IS NULL OR a.IdClase         = @IdClase)        -- filtro NUEVO
  AND (
        @Texto IS NULL
        OR a.Codigo                                           LIKE N'%'+@Texto+N'%'
        OR a.NombreTipo            COLLATE Latin1_General_CI_AI LIKE N'%'+@Texto+N'%'
        OR ISNULL(a.Serie,           N'') COLLATE Latin1_General_CI_AI LIKE N'%'+@Texto+N'%'
        OR ISNULL(a.PlacaCodBarras,  N'') COLLATE Latin1_General_CI_AI LIKE N'%'+@Texto+N'%'
        OR ISNULL(s.Nombre,          N'') COLLATE Latin1_General_CI_AI LIKE N'%'+@Texto+N'%'
        OR ISNULL(s.Cedula_Solicitante, N'')                 LIKE N'%'+@Texto+N'%'
        OR ISNULL(a.UbicacionFisica, N'') COLLATE Latin1_General_CI_AI LIKE N'%'+@Texto+N'%'  -- búsqueda NUEVA
        OR ISNULL(a.UsoPrincipal,    N'') COLLATE Latin1_General_CI_AI LIKE N'%'+@Texto+N'%'  -- búsqueda NUEVA
      )
  ORDER BY a.FechaRegistro DESC, a.Codigo;
END
GO


-- (3.2) REGISTRAR ACTIVO (permite Serie duplicada)
CREATE OR ALTER PROC dbo.usp_Activo_Registrar
  @Codigo             VARCHAR(30),
  @NombreTipo         VARCHAR(100),
  @Descripcion        VARCHAR(300) = NULL,
  @IdCategoria        INT,
  @Clasificacion      VARCHAR(20),
  @Serie              VARCHAR(80)  = NULL,
  @Modelo             VARCHAR(120) = NULL,
  @PlacaCodBarras     VARCHAR(120) = NULL,
  @Procesador         VARCHAR(120) = NULL,
  @RAM                VARCHAR(120) = NULL,
  @SistemaOperativo   VARCHAR(120) = NULL,
  @DiscoDuro          VARCHAR(120) = NULL,
  @IP                 VARCHAR(50)  = NULL,
  @Id_Departamento    INT           = NULL,
  @IdSolicitante      INT           = NULL,
  @UsuarioLocal       VARCHAR(120)  = NULL,
  @OtraCaracteristica VARCHAR(300)  = NULL,
  @FechaEntrega       DATE          = NULL,
  @Costo              DECIMAL(18,2) = NULL,
  @NoFactura          VARCHAR(60)   = NULL,
  @FechaCompra        DATE          = NULL,
  @IdProveedor        INT           = NULL,
  @IdTipo             INT,                -- NUEVO
  @IdClase            INT,                -- NUEVO
  @UbicacionFisica    VARCHAR(150) = NULL,-- NUEVO
  @UsoPrincipal       VARCHAR(120) = NULL,-- NUEVO
  @IdUsuarioCrea      INT,
  @IdGenerado         INT OUTPUT,
  @Resultado          BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON; 
  SET @Resultado = 1; 
  SET @IdGenerado = NULL;

  -- Normaliza
  SET @Codigo = LTRIM(RTRIM(@Codigo));
  SET @Serie  = NULLIF(LTRIM(RTRIM(@Serie)), '');

  -- Código único
  IF EXISTS (SELECT 1 FROM dbo.ACTIVO WHERE Codigo = @Codigo)
  BEGIN
    SET @Resultado = 0; RETURN;
  END

  -- SIN validación de unicidad en Serie

  BEGIN TRY
    INSERT INTO dbo.ACTIVO(
      Codigo, NombreTipo, Descripcion, IdCategoria, Clasificacion,
      Serie, Modelo, PlacaCodBarras, Procesador, RAM, SistemaOperativo, DiscoDuro, IP,
      Id_Departamento, IdSolicitante, UsuarioLocal, OtraCaracteristica, FechaEntrega,
      Costo, NoFactura, FechaCompra, IdProveedor,
      IdTipo, IdClase, UbicacionFisica, UsoPrincipal,   -- NUEVOS
      Activo, IdUsuarioCrea
    )
    VALUES(
      @Codigo, @NombreTipo, @Descripcion, @IdCategoria, @Clasificacion,
      @Serie, @Modelo, @PlacaCodBarras, @Procesador, @RAM, @SistemaOperativo, @DiscoDuro, @IP,
      @Id_Departamento, @IdSolicitante, @UsuarioLocal, @OtraCaracteristica, @FechaEntrega,
      @Costo, @NoFactura, @FechaCompra, @IdProveedor,
      @IdTipo, @IdClase, @UbicacionFisica, @UsoPrincipal,  -- NUEVOS
      1, @IdUsuarioCrea
    );

    SET @IdGenerado = SCOPE_IDENTITY();
    SET @Resultado  = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado = 0;
  END CATCH
END
GO


-- (3.3) MODIFICAR ACTIVO (permite Serie duplicada)
CREATE OR ALTER PROC dbo.usp_Activo_Modificar
  @IdActivo           INT,
  @Codigo             VARCHAR(30),
  @NombreTipo         VARCHAR(100),
  @Descripcion        VARCHAR(300),
  @IdCategoria        INT,
  @Clasificacion      VARCHAR(20),
  @Serie              VARCHAR(80)  = NULL,
  @Modelo             VARCHAR(120) = NULL,
  @PlacaCodBarras     VARCHAR(120) = NULL,
  @Procesador         VARCHAR(120) = NULL,
  @RAM                VARCHAR(120) = NULL,
  @SistemaOperativo   VARCHAR(120) = NULL,
  @DiscoDuro          VARCHAR(120) = NULL,
  @IP                 VARCHAR(50)  = NULL,
  @Id_Departamento    INT,
  @IdSolicitante      INT          = NULL,
  @UsuarioLocal       VARCHAR(120) = NULL,
  @OtraCaracteristica VARCHAR(300) = NULL,
  @FechaEntrega       DATE         = NULL,
  @Costo              DECIMAL(18,2) = NULL,
  @NoFactura          VARCHAR(60)   = NULL,
  @FechaCompra        DATE          = NULL,
  @IdProveedor        INT           = NULL,
  @IdTipo             INT,                -- NUEVO
  @IdClase            INT,                -- NUEVO
  @UbicacionFisica    VARCHAR(150) = NULL,-- NUEVO
  @UsoPrincipal       VARCHAR(120) = NULL,-- NUEVO
  @Activo             BIT          = 1,
  @Resultado          BIT          OUTPUT
AS
BEGIN
  SET NOCOUNT ON; 
  SET @Resultado = 1;

  -- Normaliza
  SET @Codigo = LTRIM(RTRIM(@Codigo));
  SET @Serie  = NULLIF(LTRIM(RTRIM(@Serie)), '');

  -- Existe el activo
  IF NOT EXISTS (SELECT 1 FROM dbo.ACTIVO WHERE IdActivo = @IdActivo)
  BEGIN
    SET @Resultado = 0; RETURN;
  END

  -- Código único (excluye el propio)
  IF EXISTS (SELECT 1 FROM dbo.ACTIVO WHERE Codigo = @Codigo AND IdActivo <> @IdActivo)
  BEGIN
    SET @Resultado = 0; RETURN;
  END

  -- SIN validación de unicidad en Serie

  UPDATE dbo.ACTIVO
     SET Codigo             = @Codigo,
         NombreTipo         = @NombreTipo,
         Descripcion        = @Descripcion,
         IdCategoria        = @IdCategoria,
         Clasificacion      = @Clasificacion,
         Serie              = @Serie,
         Modelo             = @Modelo,
         PlacaCodBarras     = @PlacaCodBarras,
         Procesador         = @Procesador,
         RAM                = @RAM,
         SistemaOperativo   = @SistemaOperativo,
         DiscoDuro          = @DiscoDuro,
         IP                 = @IP,
         Id_Departamento    = @Id_Departamento,
         IdSolicitante      = @IdSolicitante,
         UsuarioLocal       = @UsuarioLocal,
         OtraCaracteristica = @OtraCaracteristica,
         FechaEntrega       = @FechaEntrega,
         Costo              = @Costo,
         NoFactura          = @NoFactura,
         FechaCompra        = @FechaCompra,
         IdProveedor        = @IdProveedor,

         -- NUEVOS CAMPOS
         IdTipo             = @IdTipo,
         IdClase            = @IdClase,
         UbicacionFisica    = @UbicacionFisica,
         UsoPrincipal       = @UsoPrincipal,

         Activo             = @Activo
   WHERE IdActivo = @IdActivo;

  IF @@ROWCOUNT = 0 SET @Resultado = 0;
END
GO


-- (3.4) ELIMINAR ACTIVO (soft-delete)
CREATE OR ALTER PROC dbo.usp_Activo_Eliminar
  @IdActivo  INT,
  @Resultado BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON; 
  SET @Resultado = 1;

  UPDATE dbo.ACTIVO
     SET Clasificacion = 'Desechado'
   WHERE IdActivo = @IdActivo;

  IF @@ROWCOUNT = 0 
     SET @Resultado = 0;
END
GO







/* ===============================================================
   4) SOFTWARE POR ACTIVO (LISTA / CRUD con filtros)
   =============================================================== */

-- (4.1) LISTAR / FILTRAR SOFTWARE POR ACTIVO
-- Filtros: rango de fechas (DATE, sobre FechaInstalacion) y texto (Nombre o Editor)
GO
CREATE OR ALTER PROC dbo.usp_Software_ListarPorActivo
  @IdActivo INT,
  @Desde    DATE          = NULL,
  @Hasta    DATE          = NULL,
  @Texto    NVARCHAR(150) = NULL
AS
BEGIN
  SET NOCOUNT ON;

  -- Normaliza texto
  SET @Texto = NULLIF(LTRIM(RTRIM(@Texto)), N'');

  SELECT
      s.IdSoftware,
      s.IdActivo,
      s.Nombre,
      s.Editor,
      s.FechaInstalacion,         -- DATE
      s.Tamano,                   -- Mostrarás en KB en el front; aquí va tal cual está almacenado
      s.Version,
      s.IdUsuarioCrea,            -- << quién creó el registro
      s.FechaRegistro             -- << cuándo se creó
  FROM dbo.ACTIVO_SOFTWARE s
  WHERE s.IdActivo = @IdActivo
    AND (@Desde IS NULL OR s.FechaInstalacion >= @Desde)
    AND (@Hasta IS NULL OR s.FechaInstalacion <= @Hasta)
    AND (
         @Texto IS NULL
         OR ISNULL(s.Nombre, N'') LIKE N'%'+@Texto+N'%'
         OR ISNULL(s.Editor, N'') LIKE N'%'+@Texto+N'%'
        )
  -- Fechas de instalación más recientes primero; si es NULL, cae al final
  ORDER BY
      CASE WHEN s.FechaInstalacion IS NULL THEN 1 ELSE 0 END ASC,
      s.FechaInstalacion DESC,
      s.Nombre ASC,
      s.IdSoftware DESC;
END
GO


-- (4.2) AGREGAR SOFTWARE
-- Obligatorios: IdActivo, Nombre, IdUsuarioCrea
-- Opcionales: Editor, FechaInstalacion (DATE), Tamano (varchar, p.ej. "12345" = KB), Version
GO
CREATE OR ALTER PROC dbo.usp_Software_Agregar
  @IdActivo         INT,
  @Nombre           VARCHAR(150),
  @Editor           VARCHAR(150) = NULL,
  @FechaInstalacion DATE         = NULL,
  @Tamano           VARCHAR(50)  = NULL,
  @Version          VARCHAR(50)  = NULL,
  @IdUsuarioCrea    INT,
  @IdGenerado       INT          OUTPUT,
  @Resultado        BIT          OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado  = 0;
  SET @IdGenerado = NULL;

  BEGIN TRY
    INSERT INTO dbo.ACTIVO_SOFTWARE
      (IdActivo, Nombre, Editor, FechaInstalacion, Tamano, Version, IdUsuarioCrea, FechaRegistro)
    VALUES
      (@IdActivo, @Nombre, @Editor, @FechaInstalacion, @Tamano, @Version, @IdUsuarioCrea, GETDATE());

    SET @IdGenerado = SCOPE_IDENTITY();
    SET @Resultado  = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado  = 0;
    SET @IdGenerado = NULL;
  END CATCH
END
GO


-- (4.3) EDITAR SOFTWARE
-- Permite cambiar Nombre, Editor, FechaInstalacion, Tamano, Version.
-- No se modifica IdUsuarioCrea ni FechaRegistro.
GO
CREATE OR ALTER PROC dbo.usp_Software_Editar
  @IdSoftware       INT,
  @IdActivo         INT,
  @Nombre           VARCHAR(150),
  @Editor           VARCHAR(150) = NULL,
  @FechaInstalacion DATE         = NULL,
  @Tamano           VARCHAR(50)  = NULL,
  @Version          VARCHAR(50)  = NULL,
  @Resultado        BIT          OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado = 0;

  BEGIN TRY
    UPDATE dbo.ACTIVO_SOFTWARE
       SET Nombre           = @Nombre,
           Editor           = @Editor,
           FechaInstalacion = @FechaInstalacion,
           Tamano           = @Tamano,
           Version          = @Version
     WHERE IdSoftware = @IdSoftware
       AND IdActivo   = @IdActivo;

    IF @@ROWCOUNT > 0 SET @Resultado = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado = 0;
  END CATCH
END
GO


-- (4.4) ELIMINAR SOFTWARE (borrado físico)
-- Incluye @IdActivo por seguridad, igual que en Movimientos.
GO
CREATE OR ALTER PROC dbo.usp_Software_Eliminar
  @IdSoftware INT,
  @IdActivo   INT,
  @Resultado  BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado = 0;

  BEGIN TRY
    DELETE FROM dbo.ACTIVO_SOFTWARE
     WHERE IdSoftware = @IdSoftware
       AND IdActivo   = @IdActivo;

    IF @@ROWCOUNT > 0 SET @Resultado = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado = 0;
  END CATCH
END
GO


/* ==============================================================
   5) MOVIMIENTOS DE ACTIVO (HISTORIAL) — SOLO FECHA / IdSolicitante
   =============================================================== */

-- (6.1) LISTAR / FILTRAR MOVIMIENTOS POR ACTIVO
-- Filtros: rango de fechas (DATE) y texto (Detalle o Responsable Nombre/Cédula)
GO
CREATE OR ALTER PROC dbo.usp_Movimiento_ListarPorActivo
  @IdActivo INT,
  @Desde    DATE          = NULL,
  @Hasta    DATE          = NULL,
  @Texto    NVARCHAR(100) = NULL
AS
BEGIN
  SET NOCOUNT ON;

  SET @Texto = NULLIF(LTRIM(RTRIM(@Texto)), N'');

  SELECT
      m.IdMovimiento,
      m.IdActivo,
      m.FechaMovimiento,                 -- DATE
      m.TipoMovimiento,
      m.Detalle,
      m.IdUsuario,
      m.IdSolicitante,                   -- Responsable (FK a SOLICITANTE)
      s.ID_usuario         AS ResponsableId,
      s.Nombre             AS ResponsableNombre,
      s.Cedula_Solicitante AS ResponsableCedula
  FROM dbo.ACTIVO_MOVIMIENTO m
  LEFT JOIN dbo.SOLICITANTE s
         ON s.ID_usuario = m.IdSolicitante
  WHERE m.IdActivo = @IdActivo
    AND (@Desde IS NULL OR m.FechaMovimiento >= @Desde)
    AND (@Hasta IS NULL OR m.FechaMovimiento <= @Hasta)
    AND (
         @Texto IS NULL
         OR ISNULL(m.Detalle, N'')             LIKE N'%'+@Texto+N'%'
         OR ISNULL(s.Nombre, N'')              LIKE N'%'+@Texto+N'%'
         OR ISNULL(s.Cedula_Solicitante, N'')  LIKE N'%'+@Texto+N'%'
        )
  ORDER BY m.FechaMovimiento DESC, m.IdMovimiento DESC;
END
GO


-- (6.2) REGISTRAR MOVIMIENTO
-- Obligatorios: IdActivo, IdSolicitante, TipoMovimiento, IdUsuario
-- Opcionales: Detalle, FechaMovimiento (DATE)
GO
CREATE OR ALTER PROC dbo.usp_Movimiento_Registrar
  @IdActivo        INT,
  @IdSolicitante   INT,
  @TipoMovimiento  VARCHAR(20),
  @Detalle         VARCHAR(300) = NULL,
  @IdUsuario       INT,
  @FechaMovimiento DATE = NULL,       -- SOLO FECHA
  @IdGenerado      INT OUTPUT,
  @Resultado       BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado  = 0;
  SET @IdGenerado = NULL;

  BEGIN TRY
    INSERT INTO dbo.ACTIVO_MOVIMIENTO
      (IdActivo, FechaMovimiento, TipoMovimiento, Detalle, IdUsuario, IdSolicitante)
    VALUES
      (@IdActivo, ISNULL(@FechaMovimiento, CAST(GETDATE() AS DATE)),
       @TipoMovimiento, @Detalle, @IdUsuario, @IdSolicitante);

    SET @IdGenerado = SCOPE_IDENTITY();
    SET @Resultado  = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado  = 0;
    SET @IdGenerado = NULL;
  END CATCH
END
GO


-- (6.3) EDITAR MOVIMIENTO
-- Permite cambiar tipo, detalle, responsable y (si viene) la fecha (DATE)
GO
CREATE OR ALTER PROC dbo.usp_Movimiento_Editar
  @IdMovimiento     INT,
  @IdActivo         INT,
  @IdSolicitante    INT,
  @TipoMovimiento   VARCHAR(20),
  @Detalle          VARCHAR(300) = NULL,
  @FechaMovimiento  DATE = NULL,      -- SOLO FECHA
  @Resultado        BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado = 0;

  BEGIN TRY
    UPDATE dbo.ACTIVO_MOVIMIENTO
       SET TipoMovimiento  = @TipoMovimiento,
           Detalle         = @Detalle,
           FechaMovimiento = ISNULL(@FechaMovimiento, FechaMovimiento),
           IdSolicitante   = @IdSolicitante
     WHERE IdMovimiento = @IdMovimiento
       AND IdActivo     = @IdActivo;

    IF @@ROWCOUNT > 0 SET @Resultado = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado = 0;
  END CATCH
END
GO


-- (6.4) ELIMINAR MOVIMIENTO (borrado físico)
GO
CREATE OR ALTER PROC dbo.usp_Movimiento_Eliminar
  @IdMovimiento INT,
  @IdActivo     INT,
  @Resultado    BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado = 0;

  BEGIN TRY
    DELETE FROM dbo.ACTIVO_MOVIMIENTO
     WHERE IdMovimiento = @IdMovimiento
       AND IdActivo     = @IdActivo;

    IF @@ROWCOUNT > 0 SET @Resultado = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado = 0;
  END CATCH
END
GO


/* ===============================================================
   Procedimientos de Mantenimientos (LISTA / CRUD con filtros)
   =============================================================== */

-- (M.1) LISTAR / FILTRAR MANTENIMIENTOS POR ACTIVO
-- Filtros: rango de fechas (DATE, sobre FechaMantenimiento)
--          texto (Detalle o Responsable: Nombre/Cédula)
--          tipo (coincidencia parcial)
CREATE OR ALTER PROC dbo.usp_Mantenimiento_ListarPorActivo
  @IdActivo INT,
  @Desde    DATE          = NULL,
  @Hasta    DATE          = NULL,
  @Texto    NVARCHAR(150) = NULL,
  @Tipo     NVARCHAR(50)  = NULL
AS
BEGIN
  SET NOCOUNT ON;
  SET @Texto = NULLIF(LTRIM(RTRIM(@Texto)), N'');
  SET @Tipo  = NULLIF(LTRIM(RTRIM(@Tipo)),  N'');

  SELECT
      m.IdMantenimiento,
      m.IdActivo,
      m.IdSolicitante,
      m.FechaMantenimiento,
      m.Tipo,
      m.Detalle,
      m.IdUsuarioCrea,
      m.FechaRegistro,

      -- Datos del responsable (para mostrar en la lista)
      s.ID_usuario         AS ResponsableId,
      s.Nombre             AS ResponsableNombre,
      s.Cedula_Solicitante AS ResponsableCedula
  FROM dbo.ACTIVO_MANTENIMIENTO m
  LEFT JOIN dbo.SOLICITANTE s
         ON s.ID_usuario = m.IdSolicitante
  WHERE m.IdActivo = @IdActivo
    AND (@Desde IS NULL OR m.FechaMantenimiento >= @Desde)
    AND (@Hasta IS NULL OR m.FechaMantenimiento <= @Hasta)
    AND (@Tipo  IS NULL OR m.Tipo LIKE N'%'+@Tipo+N'%')
    AND (
         @Texto IS NULL
         OR ISNULL(m.Detalle, N'')            LIKE N'%'+@Texto+N'%'
         OR ISNULL(s.Nombre, N'')             LIKE N'%'+@Texto+N'%'
         OR ISNULL(s.Cedula_Solicitante, N'') LIKE N'%'+@Texto+N'%'
        )
  ORDER BY m.FechaMantenimiento DESC, m.IdMantenimiento DESC;
END
GO


-- (M.2) AGREGAR MANTENIMIENTO
-- Obligatorios: IdActivo, IdSolicitante, Tipo, IdUsuarioCrea
-- Opcionales: Detalle, FechaMantenimiento (si NULL usa la de hoy)
CREATE OR ALTER PROC dbo.usp_Mantenimiento_Registrar
  @IdActivo           INT,
  @IdSolicitante      INT,
  @Tipo               VARCHAR(50),
  @Detalle            VARCHAR(300) = NULL,
  @FechaMantenimiento DATE         = NULL,
  @IdUsuarioCrea      INT,
  @IdGenerado         INT          OUTPUT,
  @Resultado          BIT          OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado  = 0;
  SET @IdGenerado = NULL;

  BEGIN TRY
    INSERT INTO dbo.ACTIVO_MANTENIMIENTO
      (IdActivo, IdSolicitante, FechaMantenimiento, Tipo, Detalle, IdUsuarioCrea, FechaRegistro)
    VALUES
      (@IdActivo, @IdSolicitante, ISNULL(@FechaMantenimiento, CAST(GETDATE() AS DATE)),
       @Tipo, @Detalle, @IdUsuarioCrea, GETDATE());

    SET @IdGenerado = SCOPE_IDENTITY();
    SET @Resultado  = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado  = 0;
    SET @IdGenerado = NULL;
  END CATCH
END
GO


-- (M.3) EDITAR MANTENIMIENTO
-- Permite cambiar responsable, tipo, detalle y fecha (si viene).
-- No cambia IdUsuarioCrea ni FechaRegistro.
CREATE OR ALTER PROC dbo.usp_Mantenimiento_Editar
  @IdMantenimiento    INT,
  @IdActivo           INT,
  @IdSolicitante      INT,
  @Tipo               VARCHAR(50),
  @Detalle            VARCHAR(300) = NULL,
  @FechaMantenimiento DATE         = NULL,
  @Resultado          BIT          OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado = 0;

  BEGIN TRY
    UPDATE dbo.ACTIVO_MANTENIMIENTO
       SET IdSolicitante      = @IdSolicitante,
           Tipo               = @Tipo,
           Detalle            = @Detalle,
           FechaMantenimiento = ISNULL(@FechaMantenimiento, FechaMantenimiento)
     WHERE IdMantenimiento = @IdMantenimiento
       AND IdActivo        = @IdActivo;

    IF @@ROWCOUNT > 0 SET @Resultado = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado = 0;
  END CATCH
END
GO


-- (M.4) ELIMINAR MANTENIMIENTO (borrado físico)
-- Incluye IdActivo por seguridad (igual que en movimientos y software).
CREATE OR ALTER PROC dbo.usp_Mantenimiento_Eliminar
  @IdMantenimiento INT,
  @IdActivo        INT,
  @Resultado       BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado = 0;

  BEGIN TRY
    DELETE FROM dbo.ACTIVO_MANTENIMIENTO
     WHERE IdMantenimiento = @IdMantenimiento
       AND IdActivo        = @IdActivo;

    IF @@ROWCOUNT > 0 SET @Resultado = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado = 0;
  END CATCH
END
GO


/* ===============================================================
   ADJUNTOS por ACTIVO (BINARIO en DB) — LISTA / CRUD
   =============================================================== */

-- (A.1) LISTAR ADJUNTOS POR ACTIVO (solo metadatos, SIN Contenido)
-- Filtro opcional: @Texto (busca en NombreArchivo)
GO
CREATE OR ALTER PROC dbo.usp_Adjunto_ListarPorActivo
  @IdActivo INT,
  @Texto    NVARCHAR(200) = NULL
AS
BEGIN
  SET NOCOUNT ON;

  SET @Texto = NULLIF(LTRIM(RTRIM(@Texto)), N'');

  SELECT
      a.IdAdjunto,
      a.IdActivo,
      a.NombreArchivo,
      a.ContentType,
      a.TamanoBytes,
      a.IdUsuarioCrea,
      a.FechaRegistro
  FROM dbo.ACTIVO_ADJUNTO a
  WHERE a.IdActivo = @IdActivo
    AND (
         @Texto IS NULL
         OR ISNULL(a.NombreArchivo, N'') LIKE N'%'+@Texto+N'%'
        )
  ORDER BY a.FechaRegistro DESC, a.IdAdjunto DESC;
END
GO


-- (A.2) SUBIR / AGREGAR ADJUNTO (incluye binario)
-- Obligatorios: IdActivo, NombreArchivo, Contenido, IdUsuarioCrea
-- Opcionales:   ContentType, TamanoBytes
GO
CREATE OR ALTER PROC dbo.usp_Adjunto_Subir
  @IdActivo      INT,
  @NombreArchivo VARCHAR(200),
  @Contenido     VARBINARY(MAX),
  @ContentType   VARCHAR(100) = NULL,
  @TamanoBytes   BIGINT       = NULL,
  @IdUsuarioCrea INT,
  @IdGenerado    INT OUTPUT,
  @Resultado     BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado  = 0;
  SET @IdGenerado = NULL;

  IF @IdActivo <= 0 OR @NombreArchivo IS NULL OR LTRIM(RTRIM(@NombreArchivo)) = '' OR @Contenido IS NULL
  BEGIN
    RETURN;
  END

  BEGIN TRY
    INSERT INTO dbo.ACTIVO_ADJUNTO
      (IdActivo, NombreArchivo, Contenido, ContentType, TamanoBytes, IdUsuarioCrea, FechaRegistro)
    VALUES
      (@IdActivo, @NombreArchivo, @Contenido, @ContentType, @TamanoBytes, @IdUsuarioCrea, GETDATE());

    SET @IdGenerado = SCOPE_IDENTITY();
    SET @Resultado  = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado  = 0;
    SET @IdGenerado = NULL;
  END CATCH
END
GO


-- (A.3) DESCARGAR ADJUNTO (devuelve el binario y metadatos)
-- Usar esta SP para servir el archivo al usuario final.
GO
CREATE OR ALTER PROC dbo.usp_Adjunto_Descargar
  @IdAdjunto INT
AS
BEGIN
  SET NOCOUNT ON;

  SELECT
      a.IdAdjunto,
      a.IdActivo,
      a.NombreArchivo,
      a.Contenido,       -- VARBINARY(MAX)
      a.ContentType,
      a.TamanoBytes,
      a.IdUsuarioCrea,
      a.FechaRegistro
  FROM dbo.ACTIVO_ADJUNTO a
  WHERE a.IdAdjunto = @IdAdjunto;
END
GO


-- (A.4) ELIMINAR ADJUNTO (borrado físico)
-- Incluye @IdActivo por seguridad, igual que en software/movs/mnts.
GO
CREATE OR ALTER PROC dbo.usp_Adjunto_Eliminar
  @IdAdjunto INT,
  @IdActivo  INT,
  @Resultado BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado = 0;

  BEGIN TRY
    DELETE FROM dbo.ACTIVO_ADJUNTO
     WHERE IdAdjunto = @IdAdjunto
       AND IdActivo  = @IdActivo;

    IF @@ROWCOUNT > 0 SET @Resultado = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado = 0;
  END CATCH
END
GO


-- (A.5) (OPCIONAL) RENOMBRAR ADJUNTO (no toca el binario ni auditoría)
GO
CREATE OR ALTER PROC dbo.usp_Adjunto_Renombrar
  @IdAdjunto     INT,
  @IdActivo      INT,
  @NombreArchivo VARCHAR(200),
  @Resultado     BIT OUTPUT
AS
BEGIN
  SET NOCOUNT ON;
  SET @Resultado = 0;

  IF @NombreArchivo IS NULL OR LTRIM(RTRIM(@NombreArchivo)) = ''
  BEGIN
    RETURN;
  END

  BEGIN TRY
    UPDATE dbo.ACTIVO_ADJUNTO
       SET NombreArchivo = @NombreArchivo
     WHERE IdAdjunto = @IdAdjunto
       AND IdActivo  = @IdActivo;

    IF @@ROWCOUNT > 0 SET @Resultado = 1;
  END TRY
  BEGIN CATCH
    SET @Resultado = 0;
  END CATCH
END
GO
