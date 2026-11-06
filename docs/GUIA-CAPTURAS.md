# Capturas y entrega del laboratorio de gimnasio

## Preparación

Publica esta versión del gimnasio en Render, con la base GymnasioDb en Neon. Confirma que GET /api/socios responde sobre el dominio público y que Swagger muestra Socios y Membresias.

En Postman importa Gimnasio.postman_collection.json y el entorno Nube. Sustituye baseUrl por tu URL HTTPS real, sin /api ni barra final. No uses el enlace de conexión de Neon como URL de las solicitudes.

## Secuencia

1. Captura Swagger con el dominio público visible.
2. Ejecuta las 42 solicitudes en orden, una iteración y respuestas persistidas durante la sesión.
3. Captura el resumen del Runner con el resultado real. Se esperan 59 aserciones aprobadas.
4. Abre los detalles de cada caso y guarda su captura siguiendo [MATRIZ-PRUEBAS.md](MATRIZ-PRUEBAS.md).

En cada imagen muestra método, URL pública, código HTTP y JSON de respuesta. Para POST y PUT incluye el cuerpo enviado; utiliza dos imágenes si no cabe. Para 201 muestra Location y para 204 muestra el cuerpo vacío.

## Casos destacados

- 01 y 02: crear socios, 201.
- 03 a 06: consultas, filtro y socio sin membresías, 200.
- 07 a 11: DNI/email duplicados, 409.
- 12 y 13: actualizar socio y comprobar su nuevo nombre, 204 y 200.
- 14 y 15: validación de socio y discordancia de ID, 400.
- 16: crear membresía solo con idSocio, 201.
- 17 a 20: consultas de membresías, filtro por plan y ruta anidada, 200.
- 21: impedir eliminar un socio con membresías, 409.
- 22 y 23: actualizar membresía y comprobar su precio, 204 y 200.
- 24 a 29: FK inexistente, duración/precio inválidos, campos y IDs incorrectos, 400.
- 30 a 36: recursos inexistentes, 404.
- 37: filtro sin resultados, 200 con lista vacía.
- 38 a 42: eliminación de los registros de prueba y consultas posteriores, 204 y 404.

Usa nombres como 01-post-socio-201.png y 21-delete-socio-409.png. Guarda también capturas de GitHub con el código y el SQL, Render con el servicio y Neon con las tablas. Nunca muestres la contraseña ni la cadena de conexión privada.

## Informe

Usa [INFORME.md](INFORME.md) y completa el tema, entidades 1:N, reglas, repositorio, URL de Swagger, proveedores y conclusiones. Debajo de cada captura describe el objetivo, resultado esperado y resultado observado.

Ejemplo: “Se intentó eliminar un socio con membresías vinculadas. Se esperaba HTTP 409 Conflict y se obtuvo ese código. Resultado: aprobado”. Escribe aprobado solo cuando la respuesta real coincida.

Las capturas HTTP de entrega deben mostrar la API pública. La prueba local conectada a Neon demuestra la integración con la base, pero todavía requiere publicar la API para cumplir el despliegue completo.
