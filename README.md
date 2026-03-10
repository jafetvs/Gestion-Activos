# Sistema de Gestión de Activos Tecnológicos

Aplicación web desarrollada para la gestión y control de activos tecnológicos dentro de una organización.  
Permite registrar equipos, administrar inventario y gestionar información relacionada con los activos del departamento de TI.

Este sistema fue desarrollado como parte de una práctica profesional en la Municipalidad de Atenas, con el objetivo de facilitar el control y administración de equipos tecnológicos.

## Tecnologías utilizadas

- ASP.NET MVC
- C#
- SQL Server
- JavaScript
- HTML
- CSS

## Funcionalidades principales

- Registro de activos tecnológicos
- Gestión de inventario de equipos
- Administración de información de activos
- Interfaz web para consulta y control de datos
- Integración con base de datos relacional

## Configuración del proyecto

Para ejecutar el sistema localmente es necesario seguir los siguientes pasos.

### 1. Configurar la base de datos

Ejecutar el script SQL incluido en el repositorio:

BD INVENTARIO.sql

Esto creará la base de datos y las tablas necesarias para el funcionamiento del sistema.

### 2. Configurar la conexión a la base de datos

Abrir el archivo:

Web.config

Modificar la cadena de conexión con los datos de su servidor SQL.

Ejemplo:

<connectionStrings>
    <add name="miconexion"
         connectionString="Data Source=SERVIDOR_SQL;Initial Catalog=InventarioTI;Integrated Security=True" />
</connectionStrings>

### 3. Configuración de correo (opcional)

Si se desea habilitar el envío de correos desde el sistema, configurar los siguientes valores en el archivo Web.config:

<appSettings>
  <add key="FromEmail" value="correo@ejemplo.com" />
  <add key="FromPassword" value="PASSWORD_AQUI" />
  <add key="SMTPHost" value="smtp.gmail.com" />
  <add key="SMTPPort" value="587" />
</appSettings>

### 4. Ejecutar la aplicación

1. Abrir el proyecto en Visual Studio
2. Ejecutar con IIS Express
3. La aplicación iniciará automáticamente en el navegador.

## Estructura del proyecto

El proyecto está organizado en diferentes capas para separar responsabilidades:

- CapaModelo → Modelos de datos
- CapaDatos → Acceso a base de datos
- Control_Gestion_V3 → Controladores y vistas del sistema

## Autor

Jafet Vásquez Sandoval  
Ingeniero en Sistemas  
Costa Rica