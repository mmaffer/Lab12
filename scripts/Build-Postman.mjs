// Genera una colección reproducible. Ejecutar desde la raíz: node scripts/Build-Postman.mjs
import fs from 'node:fs';
fs.mkdirSync('postman', { recursive: true });
const items = [];
function add(name, method, path, code, body, extra = []) {
  const tests = [`pm.test('HTTP ${code}', () => pm.response.to.have.status(${code}));`, ...extra];
  if (code === 204) tests.push("pm.test('Sin cuerpo', () => pm.expect(pm.response.text()).to.eql(''));");
  if (code === 201) tests.push("pm.test('Location del recurso', () => pm.expect(pm.response.headers.get('Location')).to.match(/api\\//i));");
  items.push({ name: `${String(items.length + 1).padStart(2, '0')} ${name}`, request: {
    method, header: body ? [{ key: 'Content-Type', value: 'application/json' }] : [],
    url: '{{baseUrl}}' + path,
    ...(body ? { body: { mode: 'raw', raw: JSON.stringify(body, null, 2), options: { raw: { language: 'json' } } } } : {})
  }, event: [{ listen: 'test', script: { type: 'text/javascript', exec: tests } }] });
}
const socio = {nombre:'Socio Postman', dni:'{{dni}}', telefono:'987654321', email:'gym-{{runId}}@example.com'};
const otroSocio = {...socio, dni:'{{otroDni}}', email:'otro-gym-{{runId}}@example.com'};
const membresia = {plan:'Mensual Postman', tipoAcceso:'Completo', turno:'Mañana', duracionMeses:1, precio:120.00, idSocio:'{{socioId}}'};
add('Crear socio', 'POST', '/api/socios', 201, socio, ["pm.collectionVariables.set('socioId', pm.response.json().idSocio);"]);
add('Crear segundo socio', 'POST', '/api/socios', 201, otroSocio, ["pm.collectionVariables.set('otroSocioId', pm.response.json().idSocio);"]);
add('Listar socios', 'GET', '/api/socios', 200, null, ["pm.test('Incluye socio creado', () => pm.expect(pm.response.json().some(x => x.idSocio === Number(pm.collectionVariables.get('socioId')))).to.be.true);"]);
add('Filtro socios', 'GET', '/api/socios?buscar={{dni}}', 200, null, ["pm.test('Filtro DNI', () => pm.expect(pm.response.json().some(x => x.dni === pm.collectionVariables.get('dni'))).to.be.true);"]);
add('Obtener socio', 'GET', '/api/socios/{{socioId}}', 200);
add('Membresias vacías de socio existente', 'GET', '/api/socios/{{socioId}}/membresias', 200, null, ["pm.test('Colección vacía', () => pm.expect(pm.response.json()).to.eql([]));"]);
add('DNI duplicado POST', 'POST', '/api/socios', 409, {...socio, email:'distinto-{{runId}}@example.com'});
add('Email duplicado POST', 'POST', '/api/socios', 409, {...socio, dni:'12345678'});
add('Email mayúsculas duplicado', 'POST', '/api/socios', 409, {...socio, dni:'12345678',email:'GYM-{{runId}}@EXAMPLE.COM'});
add('DNI duplicado PUT', 'PUT', '/api/socios/{{otroSocioId}}', 409, {...otroSocio,idSocio:'{{otroSocioId}}',dni:'{{dni}}'});
add('Email duplicado PUT', 'PUT', '/api/socios/{{otroSocioId}}', 409, {...otroSocio,idSocio:'{{otroSocioId}}',email:socio.email});
add('Actualizar socio', 'PUT', '/api/socios/{{socioId}}', 204, {...socio,idSocio:'{{socioId}}',nombre:'Socio actualizado'});
add('Comprobar actualización socio', 'GET', '/api/socios/{{socioId}}', 200, null, ["pm.test('Nombre persistido', () => pm.expect(pm.response.json().nombre).to.eql('Socio actualizado'));"]);
add('Socio inválido', 'POST', '/api/socios', 400, {...socio,nombre:'',dni:'abc',email:'invalido'});
add('ID socio discordante', 'PUT', '/api/socios/{{socioId}}', 400, {...socio,idSocio:0});
add('Crear membresia solo FK', 'POST', '/api/membresias', 201, membresia, ["pm.collectionVariables.set('membresiaId', pm.response.json().idMembresia);"]);
add('Listar membresias con socio', 'GET', '/api/membresias', 200, null, ["pm.test('Include sin ciclos', () => { const m=pm.response.json().find(x=>x.idMembresia===Number(pm.collectionVariables.get('membresiaId'))); pm.expect(m.socio.idSocio).to.eql(Number(pm.collectionVariables.get('socioId'))); pm.expect(m.socio).not.to.have.property('membresias'); });"]);
add('Filtro por plan', 'GET', '/api/membresias?plan=mEnSuAl', 200, null, ["pm.test('Filtro sin distinción de mayúsculas', () => { pm.expect(pm.response.json().length).to.be.above(0); pm.response.json().forEach(m => pm.expect(m.plan.toLowerCase()).to.include('mensual')); });"]);
add('Obtener membresia', 'GET', '/api/membresias/{{membresiaId}}', 200);
add('Ruta anidada', 'GET', '/api/socios/{{socioId}}/membresias', 200, null, ["pm.test('Membresia vinculada', () => pm.expect(pm.response.json().some(x=>x.idMembresia===Number(pm.collectionVariables.get('membresiaId')))).to.be.true);"]);
add('Eliminar socio con membresias', 'DELETE', '/api/socios/{{socioId}}', 409);
add('Actualizar membresia', 'PUT', '/api/membresias/{{membresiaId}}', 204, {...membresia,idMembresia:'{{membresiaId}}',plan:'Mensual actualizado',precio:159.90});
add('Comprobar actualización membresia', 'GET', '/api/membresias/{{membresiaId}}', 200, null, ["pm.test('Precio persistido', () => pm.expect(pm.response.json().precio).to.eql(159.90));"]);
add('FK inexistente POST', 'POST', '/api/membresias', 400, {...membresia,idSocio:2147483647});
add('FK inexistente PUT', 'PUT', '/api/membresias/{{membresiaId}}', 400, {...membresia,idMembresia:'{{membresiaId}}',idSocio:2147483647});
add('Duracion y precio invalidos', 'POST', '/api/membresias', 400, {...membresia,duracionMeses:0,precio:100000});
add('Plan de membresia obligatorio', 'POST', '/api/membresias', 400, {...membresia,plan:''});
add('Nombre socio demasiado largo', 'POST', '/api/socios', 400, {...socio,nombre:'x'.repeat(101)});
add('ID membresia discordante', 'PUT', '/api/membresias/{{membresiaId}}', 400, {...membresia,idMembresia:0});
for (const resource of ['socios','membresias']) {
  add(`${resource} GET inexistente`, 'GET', `/api/${resource}/2147483647`, 404);
  add(`${resource} PUT inexistente`, 'PUT', `/api/${resource}/2147483647`, 404,
    resource==='socios'?{...socio,idSocio:2147483647}:{...membresia,idMembresia:2147483647});
  add(`${resource} DELETE inexistente`, 'DELETE', `/api/${resource}/2147483647`, 404);
}
add('Ruta anidada socio inexistente', 'GET', '/api/socios/2147483647/membresias', 404);
add('Filtro sin resultados', 'GET', '/api/membresias?plan=SinCoincidencias{{runId}}', 200, null, ["pm.test('Lista vacía', () => pm.expect(pm.response.json()).to.eql([]));"]);
add('Eliminar membresia', 'DELETE', '/api/membresias/{{membresiaId}}', 204);
add('Membresia eliminada', 'GET', '/api/membresias/{{membresiaId}}', 404);
add('Eliminar socio liberado', 'DELETE', '/api/socios/{{socioId}}', 204);
add('Socio eliminado', 'GET', '/api/socios/{{socioId}}', 404);
add('Eliminar segundo socio', 'DELETE', '/api/socios/{{otroSocioId}}', 204);
const collection = {info:{name:'Semana 12 Gestión de Gimnasio',schema:'https://schema.getpostman.com/json/collection/v2.1.0/collection.json',description:'Ejecutar completa en orden sobre datos de laboratorio. Crea y elimina sus propios registros. No modifica los datos iniciales.'},
  variable:[{key:'baseUrl',value:'http://localhost:5001'}],
  event:[{listen:'prerequest',script:{type:'text/javascript',exec:[
    "if (pm.info.requestName.startsWith('01 ')) {",
    "const id=Date.now().toString()+Math.floor(Math.random()*100000);",
    "const dni=String(10000000+Math.floor(Math.random()*80000000));",
    "pm.collectionVariables.set('runId',id); pm.collectionVariables.set('dni',dni);",
    "pm.collectionVariables.set('otroDni',String(Number(dni)+1));",
    "}"
  ]}}],item:items};
fs.writeFileSync('postman/Gimnasio.postman_collection.json',JSON.stringify(collection,null,2)+'\n');
// Matriz legible para el plan y las capturas, derivada de las mismas solicitudes.
fs.mkdirSync('docs',{recursive:true});
fs.writeFileSync('docs/MATRIZ-PRUEBAS.md','# Matriz de solicitudes Postman\n\nCada caso indica método, ruta y código esperado. Ejecutar en orden con Collection Runner.\n\n'+items.map(i=>{
 const code=i.event[0].script.exec[0].match(/HTTP (\d+)/)[1];
 return `- **${i.name}** — \`${i.request.method} ${i.request.url.replace('{{baseUrl}}','')}\` → **${code}**.`;
}).join('\n')+'\n');
console.log(`${items.length} solicitudes generadas.`);
