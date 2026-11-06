# Configuración y despliegue del gimnasio

## Proyecto existente

La solución `Sem12.slnx` ya contiene el proyecto .NET 10. No lo recrees para ejecutar esta entrega. Inicia con los pasos de [INICIO.md](INICIO.md).

Paquetes instalados:

```powershell
dotnet add Sem12 package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.3
dotnet add Sem12 package Microsoft.EntityFrameworkCore.Design --version 10.0.14
dotnet add Sem12 package Swashbuckle.AspNetCore --version 10.3.1
dotnet tool restore
```

Npgsql es el proveedor de PostgreSQL. Swashbuckle.AspNetCore incluye Swagger, SwaggerGen y SwaggerUI. La herramienta dotnet-ef se restaura con la versión del manifiesto dotnet-tools.json.

Para reproducir una API equivalente en otra carpeta, la plantilla es `dotnet new webapi -n Sem12 --framework net10.0 --use-controllers --no-openapi`. En esta solución se conserva además la estructura MVC existente con HomeController y Views.

## Esquema y validaciones

La base GymnasioDb contiene Socios y Membresias. El flujo es Database First: ejecutar scrip.sql, obtener el modelo con Scaffold y ajustar validaciones y navegación.

- Socio: nombre, DNI de ocho dígitos, teléfono y email. DNI y email únicos.
- Membresia: plan, tipo de acceso, turno, duración de 1 a 36 meses, precio de 0.01 a 99999.99 y FK positiva.
- Socio.Membresias usa JsonIgnore y Membresia.Socio es anulable para aceptar solo idSocio en POST.
- GymnasioContext define la relación 1:N con DeleteBehavior.Restrict y Precio con precisión 7,2.

La aplicación no ejecuta EnsureCreated ni Migrate al iniciar. En local Docker ejecuta el SQL al inicializar el volumen; en Neon se ejecuta en SQL Editor.

## Obtener el modelo con Scaffold

```powershell
.\Scaffold.cmd
```

El lanzador carga .env y ejecuta dotnet ef dbcontext scaffold con el proveedor Npgsql, `Name=ConnectionStrings:DefaultConnection`, `--data-annotations` y `--no-onconfiguring`. Se genera un directorio nuevo en ScaffoldPreview, fuera del proyecto compilado y excluido de Git. Consulta el comando completo en scripts/Scaffold.ps1.

El generador puede advertir que no representa el índice SQL `lower("Email")`; ese índice se conserva en PostgreSQL y su definición está en scrip.sql. Las validaciones de negocio y nombres de navegación se ajustan en las entidades finales.

## Visual Studio y HTTPS

Start-Local.cmd es el arranque preparado para .env. Si prefieres F5, puedes usar secretos de usuario:

```powershell
dotnet user-secrets init --project Sem12
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' 'TU_CONEXION_PRIVADA' --project Sem12
```

Sustituye la conexión antes de ejecutar y no la publiques. Selecciona el perfil http o https del proyecto. Si utilizas https local, confía el certificado con `dotnet dev-certs https --trust`.

## Base de datos en Neon

En Neon crea la base **GymnasioDb**, selecciónala en SQL Editor y ejecuta scrip.sql sobre una base vacía. Comprueba Socios y Membresias con SELECT. Copia el enlace de Connect de esa base a ConnectionStrings__DefaultConnection y desactiva LocalDatabase__Enabled. El ejemplo está en config/neon.env.example.

El SQL adapta identity, VARCHAR, NUMERIC y la selección de base a PostgreSQL. No contiene GO ni USE. La FK usa ON DELETE RESTRICT y los índices aseguran la unicidad.

## GitHub

La carpeta de trabajo es `C:\Users\Jefferson\source\repos\Sem12`. Configura tu repositorio real antes de publicar:

```powershell
git init
git remote add origin URL_REAL_DE_TU_REPOSITORIO
git add .
git commit -m "Implementar gestion de gimnasio con PostgreSQL"
git push -u origin HEAD
```

Si ya tienes Git y origin configurados, omite su creación. Revisa los archivos antes de agregar cambios. .env, ScaffoldPreview y artifacts se excluyen del control de versiones. La conversión no crea commits ni publica cambios automáticamente.

## API en Render

Opción manual: New Web Service, repositorio de esta solución, runtime Docker, plan Free y Root Directory vacío. Dockerfile Path debe ser ./Dockerfile y Docker Build Context debe ser el directorio raíz del repositorio. No necesitas un Start Command adicional.

Variables del servicio:

- ConnectionStrings__DefaultConnection: conexión privada a GymnasioDb en Neon.
- ASPNETCORE_ENVIRONMENT: Production.
- ASPNETCORE_HTTP_PORTS: 8080.
- PORT: 8080.
- Cors__AllowedOrigins__0: origen exacto sin barra final, por ejemplo https://TU-API-GIMNASIO.onrender.com.

Health Check Path: `/api/socios`. Swagger aparece en `/` incluso en Production. Render termina HTTPS en su proxy y el contenedor escucha HTTP internamente.

Como alternativa a la configuración manual, New Blueprint utiliza render.yaml, que define el servicio sem12-gimnasio y solicita las variables privadas sin almacenarlas en el repositorio. Elige una sola modalidad para no crear servicios duplicados.

La base y la API deben estar ambas en la nube para completar el laboratorio. La colección contra localhost no sustituye las pruebas públicas. Sigue [PRUEBAS.md](PRUEBAS.md) y [GUIA-CAPTURAS.md](GUIA-CAPTURAS.md).

## Proveedores y justificación

Se propone Render para alojar el contenedor ASP.NET Core y Neon para PostgreSQL administrado, seleccionando sus planes gratuitos disponibles en la cuenta. Revisa las cuotas y condiciones actuales antes de crear recursos.

Como alternativas se pueden investigar Koyeb para contenedores y Supabase para PostgreSQL. Se conserva Render por el Dockerfile y Blueprint preparados, y Neon por su compatibilidad con Npgsql y la conexión directa a PostgreSQL.

Fuentes oficiales:

- [Render Free](https://render.com/docs/free).
- [Docker en Render](https://render.com/docs/docker).
- [Blueprints](https://render.com/docs/blueprint-spec).
- [Neon](https://neon.com/pricing).
- [Koyeb](https://www.koyeb.com/docs/reference/instances).
- [Supabase](https://supabase.com/pricing).

## Problemas frecuentes

- Puerto 5001 ocupado: detén la instancia anterior antes de iniciar otra.
- Tabla Socios inexistente: ejecuta scrip.sql en la base exacta de la conexión.
- Base GymnasioDb inexistente en Neon: créala primero desde su consola; el script crea tablas, no bases remotas.
- Contraseña local modificada después de inicializar: el usuario de PostgreSQL conserva la contraseña del volumen original.
- Conexión de Neon: pega la URI sin el comando psql o usa formato ADO.NET. La aplicación normaliza la URI sin mostrar credenciales.
- CORS: revisa protocolo, host y puerto; configura orígenes concretos, sin rutas ni barra final.
