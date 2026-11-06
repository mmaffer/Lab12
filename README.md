# Gestión de Gimnasio — Semana 12

Web API REST para administrar **socios y membresías de un gimnasio**, desarrollada para el laboratorio de Desarrollo de Aplicaciones Empresariales Avanzadas. Utiliza ASP.NET Core .NET 10, Entity Framework Core y PostgreSQL, con una relación **1:N** y acceso directo al `DbContext` mediante LINQ asíncrono.

La solución y el proyecto se llaman **Sem12**; la base de datos se llama **GymnasioDb**. Se conserva la estructura MVC del proyecto y se implementan los endpoints mediante controladores de API.

## Tecnologías y arquitectura

- ASP.NET Core .NET 10 con controladores y validación automática.
- Entity Framework Core y `Npgsql.EntityFrameworkCore.PostgreSQL` para PostgreSQL.
- Swagger/OpenAPI mediante `Swashbuckle.AspNetCore`.
- PostgreSQL 17 en Docker para desarrollo local y configuración compatible con Neon.
- Dockerfile y Blueprint para desplegar la API en Render.
- Postman y Newman para verificar los endpoints y sus respuestas.

Las solicitudes pasan del cliente al controlador, que aplica las reglas de negocio y utiliza `GymnasioContext` para consultar o guardar datos. Los controladores inyectan directamente el contexto y utilizan `ToListAsync`, `FindAsync`, `AnyAsync`, `Include` y `AsNoTracking`, sin Repository ni DTOs.

## Requisitos

- SDK de .NET 10.
- Docker Desktop iniciado si utilizarás PostgreSQL local.
- Node.js con npm para ejecutar `Test-Api.cmd`, o Postman para ejecutar la colección manualmente.
- Windows y PowerShell para los lanzadores `.cmd` incluidos.

## Ejecución local

Abre una terminal en la raíz del repositorio, donde se encuentra `Sem12.slnx`, y ejecuta:

```powershell
.\Start-Local.cmd
```

El comando realiza lo siguiente:

1. Crea `.env` desde `.env.example` si todavía no existe y carga sus variables.
2. Inicia PostgreSQL con Docker Compose cuando `LocalDatabase__Enabled=true`.
3. Espera a que la base esté disponible y ejecuta la API.

Al inicializar el volumen por primera vez, PostgreSQL ejecuta [scrip.sql](scrip.sql) y crea las tablas, **dos socios y tres membresías** de ejemplo. Los arranques posteriores conservan los datos y no vuelven a ejecutar el script.

Direcciones locales predeterminadas:

- **Swagger:** [http://localhost:5001/](http://localhost:5001/).
- **Vista MVC del gimnasio:** [http://localhost:5001/Home/Index](http://localhost:5001/Home/Index).
- **PostgreSQL:** `localhost:55432`, base `GymnasioDb`.

Para detener la API utiliza `Ctrl+C`. Para detener PostgreSQL sin eliminar sus datos:

```powershell
docker compose stop postgres
```

Si el puerto 5001 está ocupado, detén la instancia anterior antes de ejecutar otra. Si cambias la contraseña local después de crear el volumen, el usuario de PostgreSQL conserva la contraseña con la que se inicializó.

## Configuración de entorno

El archivo [.env.example](.env.example) contiene la configuración de desarrollo local. Las variables principales son:

```dotenv
POSTGRES_DB=GymnasioDb
POSTGRES_PORT=55432
LocalDatabase__Enabled=true
ConnectionStrings__DefaultConnection=Host=localhost;Port=55432;Database=GymnasioDb;Username=gymnasio;Password=TU_CLAVE_LOCAL;SSL Mode=Disable
Cors__AllowedOrigins__0=http://localhost:5001
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5001
```

El ejemplo completo también define `POSTGRES_USER` y `POSTGRES_PASSWORD`. La contraseña debe coincidir con la de la cadena de conexión. `TU_CLAVE_LOCAL` es un marcador; el arranque inicial ya puede utilizar los valores de laboratorio incluidos en `.env.example`.

`Start-Local.cmd` carga `.env`; `dotnet run` y Visual Studio por sí solos no leen ese archivo. Si prefieres ejecutar desde Visual Studio, consulta la configuración de secretos de usuario en [GUIA.md](docs/GUIA.md).

### Conexión con Neon

Crea la base `GymnasioDb` en Neon y ejecuta `scrip.sql` sobre esa base vacía. Después detén la API y cambia estos valores de `.env`:

```dotenv
LocalDatabase__Enabled=false
ConnectionStrings__DefaultConnection=postgresql://USUARIO:CONTRASEÑA@HOST/GymnasioDb?sslmode=require&channel_binding=require
```

Utiliza el enlace real de **Connect** de la base del gimnasio, sin el comando `psql`. La aplicación convierte la URI al formato de Npgsql y configura TLS. También admite una cadena ADO.NET con `SSL Mode=VerifyFull` y `Channel Binding=Require`.

Vuelve a ejecutar `Start-Local.cmd`: la API seguirá escuchando localmente, pero consultará Neon. En ese modo no se inicia PostgreSQL con Docker. Consulta [config/neon.env.example](config/neon.env.example).

`.env` está excluido de Git. En la nube, configura la conexión mediante variables de entorno del proveedor.

## Entidades y reglas de negocio

**Socio** contiene `IdSocio`, `Nombre`, `Dni`, `Telefono` y `Email`. Su colección `Membresias` utiliza `[JsonIgnore]` para evitar ciclos de serialización.

**Membresia** contiene `IdMembresia`, `Plan`, `TipoAcceso`, `Turno`, `DuracionMeses`, `Precio` e `IdSocio`. La navegación `Socio?` permite enviar únicamente la clave foránea en el JSON.

Un socio puede tener varias membresías y cada membresía pertenece a un socio existente. Se aplican estas validaciones y reglas:

- DNI de ocho dígitos y email con formato válido.
- Campos de texto obligatorios y longitudes máximas mediante Data Annotations.
- Duración de la membresía entre **1 y 36 meses** y precio entre **0.01 y 99999.99**.
- **409 Conflict:** DNI o email duplicado al crear o actualizar socios.
- **409 Conflict:** eliminación de un socio con membresías vinculadas.
- **400 Bad Request:** socio inexistente al crear o actualizar una membresía.

PostgreSQL respalda las reglas con índices únicos y una clave foránea con `ON DELETE RESTRICT`. El email también dispone de un índice único sobre `lower("Email")`.

## Endpoints

La API contiene **11 endpoints**. Los filtros se aplican sobre las rutas de listado.

**Socios:**

- `GET /api/socios` — listar; admite `?buscar=Diego` para filtrar por nombre, DNI o email.
- `GET /api/socios/{id}` — consultar por ID.
- `GET /api/socios/{id}/membresias` — consultar las membresías del socio.
- `POST /api/socios` — registrar.
- `PUT /api/socios/{id}` — actualizar.
- `DELETE /api/socios/{id}` — eliminar si no tiene membresías.

**Membresías:**

- `GET /api/membresias` — listar con su socio; admite `?plan=Mensual`.
- `GET /api/membresias/{id}` — consultar por ID con su socio.
- `POST /api/membresias` — registrar con una FK de socio existente.
- `PUT /api/membresias/{id}` — actualizar.
- `DELETE /api/membresias/{id}` — eliminar.

Se utilizan **200** para consultas exitosas, **201** con `Location` para creaciones, **204** para actualizaciones y eliminaciones, **400** para datos inválidos, **404** para recursos inexistentes y **409** para las reglas de conflicto.

### Ejemplos de POST

`POST /api/socios`:

```json
{
  "nombre": "Luis Torres",
  "dni": "45678912",
  "telefono": "987654320",
  "email": "luis.torres@gym.example"
}
```

`POST /api/membresias`, utilizando un socio existente:

```json
{
  "plan": "Mensual",
  "tipoAcceso": "Completo",
  "turno": "Mañana",
  "duracionMeses": 1,
  "precio": 120.00,
  "idSocio": 1
}
```

En POST se omite el ID de la entidad, que genera la base. En PUT se envían los campos completos y el identificador correspondiente (`idSocio` o `idMembresia`), coincidente con la ruta.

## Pruebas

Con la API iniciada, ejecuta en otra terminal:

```powershell
.\Test-Api.cmd
```

El script comprueba Swagger y el documento OpenAPI y ejecuta **42 solicitudes con 59 aserciones**. La colección crea sus propios registros y los elimina al finalizar, conservando los datos iniciales. El reporte se guarda en `artifacts/postman-local.json`.

Para Postman, importa [la colección del gimnasio](postman/Gimnasio.postman_collection.json) y el entorno [Local](postman/Local.postman_environment.json) o [Nube](postman/Nube.postman_environment.json). Ejecuta los casos en orden y configura `baseUrl` con el destino elegido, sin barra final ni `/api`.

Las pruebas locales de esta conversión aprobaron las **42 solicitudes y 59 aserciones**. También se verificaron CORS, Scaffold, la vista MVC y el contenedor en modo Production. Las evidencias del laboratorio deben obtenerse adicionalmente sobre la URL pública.

## Database First y Scaffold

El esquema se define con SQL y el modelo se obtiene mediante ingeniería inversa. Para repetir Scaffold con la conexión configurada:

```powershell
.\Scaffold.cmd
```

El resultado se guarda en una carpeta nueva de `ScaffoldPreview/`, fuera del proyecto compilado, sin sobrescribir las entidades finales con validaciones ni escribir las credenciales en el código generado. La herramienta se restaura desde `dotnet-tools.json`.

El índice de expresión `lower("Email")` puede producir una advertencia al generar el modelo; su definición permanece en el SQL. La aplicación no utiliza `EnsureCreated` ni `Migrate` al arrancar.

## Despliegue

El [Dockerfile](Dockerfile) compila y publica la aplicación .NET 10. El archivo [render.yaml](render.yaml) define el servicio `sem12-gimnasio`, el puerto 8080 y el health check `/api/socios`.

Para Render configura:

- `ConnectionStrings__DefaultConnection`: conexión privada a la base del gimnasio en Neon.
- `ASPNETCORE_ENVIRONMENT=Production`.
- `ASPNETCORE_HTTP_PORTS=8080`.
- `PORT=8080`.
- `Cors__AllowedOrigins__0`: origen concreto permitido, sin ruta ni barra final.

Swagger permanece habilitado en la raíz también en producción. Para comprobar el despliegue, sustituye el dominio de ejemplo por tu URL real:

```powershell
.\Test-Api.cmd -Cloud -BaseUrl https://TU-API-GIMNASIO.onrender.com
```

El reporte público se guarda en `artifacts/postman-nube.json`. Completa los enlaces reales de GitHub y Swagger en [INFORME.md](docs/INFORME.md) después de publicar y verificar esta versión.

## Estructura del repositorio

```text
Sem12.slnx
Sem12/
  Controllers/
    SociosController.cs
    MembresiasController.cs
    HomeController.cs
  Data/
    GymnasioContext.cs
    DatabaseConnection.cs
  Models/
    Socio.cs
    Membresia.cs
    ErrorViewModel.cs
  Views/
  Properties/launchSettings.json
  Program.cs
  appsettings.json
  Gimnasio.http
  Sem12.csproj
scripts/                         # Arranque, Scaffold y pruebas
postman/                         # Colección y entornos
config/neon.env.example          # Configuración de referencia para Neon
docs/                           # Guías, matriz e informe
scrip.sql
.env.example
compose.yaml
Dockerfile
render.yaml
dotnet-tools.json
Start-Local.cmd
Scaffold.cmd
Test-Api.cmd
```

## Documentación del laboratorio

- [Inicio local y conexión con Neon](docs/INICIO.md).
- [Configuración, Scaffold y despliegue](docs/GUIA.md).
- [Plan de pruebas](docs/PRUEBAS.md).
- [Matriz de solicitudes](docs/MATRIZ-PRUEBAS.md).
- [Guía de capturas](docs/GUIA-CAPTURAS.md).
- [Informe de entrega](docs/INFORME.md).
