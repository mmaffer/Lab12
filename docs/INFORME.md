# Informe Semana 12 Gestión de Gimnasio

## Explicación del tema

El tema elegido es Gestión de Gimnasio. La API permite registrar socios y administrar las membresías contratadas, centralizando los datos de contacto y las condiciones de acceso, duración y precio de cada membresía.

Las entidades son Socio y Membresia. Socio contiene identificador, nombre, DNI, teléfono y email. Membresia contiene identificador, plan, tipo de acceso, turno, duración en meses, precio y clave foránea del socio. La relación es 1:N: un socio puede tener varias membresías y cada membresía pertenece a un socio existente.

Las reglas principales devuelven HTTP 409 Conflict: impedir DNI o email duplicados al crear o actualizar socios, e impedir la eliminación de un socio con membresías vinculadas. Los datos inválidos, identificadores discordantes y claves foráneas inexistentes devuelven HTTP 400.

## Implementación y repositorio

La solución conserva la estructura del proyecto Sem12 y utiliza ASP.NET Core .NET 10, EF Core y PostgreSQL. Models contiene Socio y Membresia; Data contiene GymnasioContext y la normalización de conexión; Controllers contiene SociosController y MembresiasController. Se conserva HomeController y Views para la estructura MVC original.

Los controladores de la API heredan de ControllerBase, usan ApiController y rutas api/[controller]. Todo el CRUD utiliza directamente el contexto mediante LINQ asíncrono, sin Repository ni DTOs. Se aplican ToListAsync, FindAsync, AnyAsync, Include y AsNoTracking. Los 11 endpoints incluyen filtros por nombre/DNI/email de socio y por plan de membresía, además de la ruta anidada de membresías de un socio.

scrip.sql crea el esquema de GymnasioDb con datos iniciales. Las validaciones usan Required, StringLength, Range y EmailAddress. La navegación Socio? permite enviar solo idSocio y JsonIgnore evita ciclos al serializar. Swagger está habilitado en la raíz y también en producción. CORS utiliza orígenes concretos configurables.

Repositorio: **[COMPLETAR CON LA URL REAL DE GITHUB]**.

## Despliegue

Swagger público: **[COMPLETAR CON LA URL REAL DE LA API DEL GIMNASIO]**.

Proveedores propuestos: Render para la API Docker y Neon para PostgreSQL, seleccionando sus planes gratuitos. El Dockerfile y render.yaml están preparados para esta versión. La cadena privada se configura mediante ConnectionStrings__DefaultConnection y no se publica en el repositorio.

Se cambió el proveedor a Npgsql y UseNpgsql, y se adaptó el script a PostgreSQL. La base se crea desde la consola del proveedor y el script define tablas, índices, claves y datos. El modelo se obtiene con Scaffold y se ajusta con anotaciones y navegación requeridas por la API.

## Pruebas

Se incluye una colección Postman de 42 solicitudes y 59 aserciones para validar los códigos 200, 201, 204, 400, 404 y 409, los 11 endpoints, los filtros, la ruta anidada y ambas reglas de negocio. Las pruebas crean sus propios registros y los eliminan al finalizar.

**Completar con la ejecución real:** indicar el resultado sobre la URL pública y adjuntar las capturas de cada caso. Las verificaciones locales se documentan por separado y no se presentan como evidencias de una API publicada.

## Observaciones

1. El esquema utiliza restricciones únicas y una clave foránea para respaldar las reglas del gimnasio.
2. Swagger facilita la exploración de los endpoints y Postman comprueba los resultados con aserciones automáticas.
3. Los servicios gratuitos pueden aplicar cuotas o suspenderse por inactividad; se debe verificar su disponibilidad antes de presentar el laboratorio.

## Conclusiones

1. EF Core y LINQ asíncrono simplifican las consultas y modificaciones de socios y membresías.
2. Las validaciones y restricciones protegen la integridad al impedir duplicados, referencias inexistentes y eliminaciones incompatibles con la relación 1:N.
3. La configuración permite ejecutar el mismo proyecto localmente y publicarlo con PostgreSQL administrado, conservando las credenciales fuera del código fuente.
