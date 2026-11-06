# Ejecutar GymnasioDb localmente y en la nube

## Arranque local

Requisitos: SDK .NET 10 y Docker Desktop. Para las pruebas automáticas necesitas Node.js/npm; puedes usar Postman como alternativa.

Desde la carpeta abierta del proyecto:

```powershell
cd C:\Users\Jefferson\source\repos\Sem12
.\Start-Local.cmd
```

El script carga `.env` y, si `LocalDatabase__Enabled=true`, inicia PostgreSQL 17 con Docker Compose. La base **GymnasioDb** escucha en `localhost:55432`. El servicio y el volumen del gimnasio tienen nombres nuevos para inicializar su propio esquema. Los datos se conservan entre ejecuciones.

Abre **http://localhost:5001/** y prueba:

- `GET /api/socios`: dos socios iniciales, Diego Ramirez y Valeria Lopez.
- `GET /api/membresias`: tres membresías iniciales.
- `GET /api/membresias?plan=Mensual`: filtro por plan.
- `GET /api/socios/1/membresias`: membresías vinculadas al socio 1.

La vista MVC está en **http://localhost:5001/Home/Index**. Swagger ocupa la raíz. Para detener la API usa Ctrl+C. Para detener solo PostgreSQL sin borrar datos:

```powershell
docker compose stop postgres
```

No vuelvas a ejecutar el SQL sobre tablas existentes. El script de inicialización de Docker se ejecuta solo al crear el volumen, no en cada arranque.

## Configuración de .env

`.env.example` contiene el ejemplo completo. Los valores relevantes para desarrollo son:

```dotenv
POSTGRES_DB=GymnasioDb
POSTGRES_USER=gymnasio
POSTGRES_PASSWORD=gymnasio_solo_desarrollo
POSTGRES_PORT=55432
LocalDatabase__Enabled=true
ConnectionStrings__DefaultConnection=Host=localhost;Port=55432;Database=GymnasioDb;Username=gymnasio;Password=gymnasio_solo_desarrollo;SSL Mode=Disable
Cors__AllowedOrigins__0=http://localhost:5001
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5001
```

La contraseña anterior es un ejemplo de desarrollo local. Si eliges otra, usa el mismo valor en POSTGRES_PASSWORD y Password de la conexión. Cambiar el archivo después de inicializar PostgreSQL no modifica los usuarios guardados dentro del volumen.

El archivo .env se carga mediante Start-Local.cmd; Visual Studio y dotnet run por sí solos no lo leen. Los lanzadores cmd utilizan PowerShell para cargarlo sin cambiar permanentemente su política de ejecución.

## Pruebas

Con la API encendida, abre otra terminal:

```powershell
.\Test-Api.cmd
```

El resultado esperado es 42 solicitudes y 59 aserciones sin fallos. Se crean registros propios de prueba y se eliminan al finalizar; los datos iniciales permanecen. El reporte queda en `artifacts/postman-local.json`.

Para Postman importa `postman/Gimnasio.postman_collection.json` y `postman/Local.postman_environment.json`, selecciona el entorno Gimnasio Local y ejecuta la colección completa en orden.

## Scaffold

Con PostgreSQL activo:

```powershell
.\Scaffold.cmd
```

Se genera GymnasioScaffoldContext y sus entidades en una carpeta nueva dentro de ScaffoldPreview. La conexión se obtiene por nombre desde configuración y no se incluye en el código generado. Las entidades finales conservan sus Data Annotations y navegación ajustada.

## Usar Neon

1. Crea una base nueva llamada **GymnasioDb** en la consola de Neon. Si utilizas un nombre con distinta capitalización, copia exactamente el enlace de Connect de esa base.
2. Selecciona esa base en SQL Editor y ejecuta `scrip.sql` una sola vez.
3. Detén la API local antes de editar su configuración.
4. En .env cambia `LocalDatabase__Enabled=false` y reemplaza la conexión local por el enlace de la base nueva:

   ```dotenv
   LocalDatabase__Enabled=false
   ConnectionStrings__DefaultConnection=postgresql://USUARIO:CONTRASEÑA@HOST/GymnasioDb?sslmode=require&channel_binding=require
   ```

5. Ejecuta `Start-Local.cmd`. La API permanece en localhost, pero consulta Neon. Docker ya no se inicia para esa conexión.
6. Ejecuta `Test-Api.cmd` para validar el nuevo esquema desde la API local.

También se admite formato ADO.NET con `Database=GymnasioDb;SSL Mode=VerifyFull;Channel Binding=Require`. Conserva la contraseña real en .env o en los secretos del proveedor, nunca en los ejemplos ni en capturas.

## Publicar la API del gimnasio

1. Publica esta versión de Sem12 en tu repositorio GitHub.
2. Despliega en Render usando el Dockerfile o el Blueprint `render.yaml`.
3. Configura ConnectionStrings__DefaultConnection con la base del gimnasio en Neon, ASPNETCORE_ENVIRONMENT=Production, PORT=8080 y ASPNETCORE_HTTP_PORTS=8080.
4. Configura Cors__AllowedOrigins__0 con un origen concreto sin barra final.
5. Comprueba Swagger y GET /api/socios en la URL pública.
6. Ejecuta:

   ```powershell
   .\Test-Api.cmd -Cloud -BaseUrl https://TU-API-GIMNASIO.onrender.com
   ```

7. Usa Postman para obtener capturas sobre esa URL. La publicación previa de otra versión no cambia hasta que subas y redespliegues este código.

La estructura y el nombre técnico de la solución Sem12 se conservan. El dominio, las tablas, clases, controladores, variables de prueba y configuración corresponden ahora al gimnasio.
