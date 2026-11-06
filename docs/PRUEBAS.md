# Pruebas de socios y membresías

## Ejecución local

Inicia `Start-Local.cmd` y ejecuta `Test-Api.cmd` en otra terminal. El script comprueba Swagger, el documento OpenAPI y al menos 10 endpoints; después ejecuta la colección **Gimnasio.postman_collection.json**. El resultado esperado es 42 solicitudes y 59 aserciones sin fallos.

El reporte queda en artifacts/postman-local.json. Las pruebas usan variables socioId, otroSocioId y membresiaId para no suponer identificadores consecutivos. Se generan DNI y emails de prueba y se eliminan los registros creados al finalizar.

## Postman

1. Importa postman/Gimnasio.postman_collection.json.
2. Importa postman/Local.postman_environment.json para localhost o postman/Nube.postman_environment.json para la API publicada.
3. Selecciona el entorno y configura baseUrl sin barra final ni /api.
4. Ejecuta las 42 solicitudes en orden y con una iteración.
5. Activa Persist responses for a session si deseas conservar los detalles durante esa sesión y tomar capturas después.

La [matriz](MATRIZ-PRUEBAS.md) se genera desde los mismos casos de prueba.

## Códigos verificados por la colección

- 200: listas y detalles de socios y membresías, filtros por buscar y plan, ruta anidada y comprobación de cambios persistidos.
- 201: creación de socios y de una membresía enviando solo idSocio. Se comprueba Location.
- 204: actualización y eliminación de ambas entidades, con cuerpo de respuesta vacío.
- 400: nombre o plan obligatorio, longitudes, DNI/email inválidos, duración/precio fuera de rango, IDs discordantes y socio inexistente en POST y PUT de membresías.
- 404: consultas, actualizaciones y eliminaciones de IDs inexistentes, ruta anidada de socio inexistente y consultas posteriores a DELETE.
- 409: DNI/email duplicados en POST y PUT y eliminación de socio que tiene membresías.

Los casos de inexistencia usan 2147483647 en una base de laboratorio donde ese ID no esté registrado. La comparación de email ignora mayúsculas y el índice lower(Email) protege también las inserciones concurrentes.

## Ejecución pública

```powershell
.\Test-Api.cmd -Cloud -BaseUrl https://TU-API-GIMNASIO.onrender.com
```

Sustituye el dominio por el real. El reporte queda en artifacts/postman-nube.json. No uses localhost como prueba del despliegue público.

Para capturas e informe sigue [GUIA-CAPTURAS.md](GUIA-CAPTURAS.md). No vuelvas a enviar casos individuales después de los DELETE finales para intentar reconstruir respuestas: usa los detalles guardados o reinicia desde el caso 01.

## Verificación realizada en esta conversión

La colección del gimnasio se ejecutó contra la API local y PostgreSQL 17 real: **42 solicitudes y 59 aserciones aprobadas, sin fallos**. Se verificaron los 11 endpoints, la persistencia de cambios, las validaciones y las dos reglas 409. Los dos socios y las tres membresías iniciales se conservaron tras la limpieza de los registros de prueba.

Se comprobó también la vista MVC del gimnasio, el origen CORS permitido y el rechazo de un origen ajeno. Scaffold se ejecutó sobre GymnasioDb y generó las entidades del nuevo esquema. La compilación Release terminó sin advertencias ni errores. La publicación de esta versión y las pruebas sobre un dominio público se completan después de actualizar la conexión y redesplegar en la nube.
