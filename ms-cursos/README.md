# ms-cursos · Contrato de la API (código de referencia, ficha 3)

Microservicio de cursos **relacionado con ms-programas**. Puerto: **3011**.
Guarda el atributo `programa_id`, lo valida contra ms-programas, aplica la regla de negocio de la ficha 3 y responde cada curso con su objeto `programa`.

## Cómo ejecutarlo

Requisitos: Node.js 22.9 o superior, MySQL 8, ms-usuarios (para obtener tokens) y ms-programas en el puerto 3012.

1. Como root en MySQL (si la base de datos y el usuario aún no existen):

```sql
CREATE DATABASE cursos_db;
CREATE USER 'cursos_user'@'localhost' IDENTIFIED BY 'una-clave-segura';
GRANT ALL PRIVILEGES ON cursos_db.* TO 'cursos_user'@'localhost';
```

2. `mysql -u cursos_user -p cursos_db` y luego `SOURCE db/schema.sql;` (el script borra y vuelve a crear la tabla `cursos`).
3. `npm install`
4. Copiar `.env.example` como `.env`. `JWT_SECRET` debe ser **la misma de ms-usuarios**, y `PROGRAMAS_URL` la dirección de ms-programas.
5. `npm run dev`

## Representación

```json
{
  "id": 1,
  "codigo": "MAT-101",
  "nombre": "Cálculo I",
  "creditos": 4,
  "cupo": 40,
  "semestre": "2026-2",
  "programa_id": 1,
  "programa": { "id": 1, "nombre_programa": "Ingeniería Multimedia", "facultad": "Ingeniería" }
}
```

Si ms-programas no responde, las consultas siguen respondiendo 200 con `"programa": null` y un campo `"aviso"`.

## Atributos

| Atributo | Tipo | ¿Obligatorio al crear? | Reglas |
|---|---|---|---|
| id | entero | No: lo asigna el servicio | Positivo y único |
| codigo | texto | Sí | No vacío; único (si se repite: 409) |
| nombre | texto | Sí | No vacío |
| creditos | entero | Sí | Entre 1 y 6, y **no mayor que `creditos_max_curso` del programa** (400) |
| cupo | entero | Sí | Mayor que 0 |
| semestre | texto | Sí | Formato AAAA-1 o AAAA-2 |
| programa_id | entero | Sí | Positivo; el programa debe existir en ms-programas (400) |

Cuerpo de ejemplo para crear:

```json
{"codigo":"MAT-101-1","nombre":"Cálculo I","creditos":4,"cupo":30,"semestre":"2026-2","programa_id":1}
```

## Endpoints

Todas las peticiones llevan `Authorization: Bearer <token>`. GET para cualquier rol; POST, PUT y DELETE solo para administrador.

| Operación | Método | URI | Respuesta exitosa | Errores |
|---|---|---|---|---|
| Listar | GET | /cursos | 200 arreglo, con `programa` | 401 |
| Filtrar | GET | /cursos?semestre=2026-2 | 200 arreglo | 401 |
| Obtener | GET | /cursos/:id | 200 objeto, con `programa` | 400 · 401 · 404 |
| Crear | POST | /cursos | 201 objeto + cabecera Location | 400 · 401 · 403 · 409 · 503 |
| Actualizar | PUT | /cursos/:id | 200 objeto | 400 · 401 · 403 · 404 · 409 · 503 |
| Eliminar | DELETE | /cursos/:id | 204 | 400 · 401 · 403 · 404 |

## Formato de los errores

```json
{ "error": "Datos inválidos", "detalles": ["cupo debe ser un entero mayor que 0"] }
```

`detalles` aparece solo en los errores de validación.

## Relación con ms-programas

| Le consulto a | Petición | Para qué | Si no responde |
|---|---|---|---|
| ms-programas | `GET /programas/:id` | Validar `programa_id` y aplicar la regla (crear y actualizar) | 503 |
| ms-programas | `GET /programas?ids=…` | Componer el objeto `programa`: **una sola petición** para todo el listado | 200 con `programa: null` y `aviso` |

El token de la petición original se reenvía a ms-programas. El código de la relación está en `src/clients/programas.client.js`; la regla y la composición, en `src/services/cursos.service.js`.

## Datos iniciales

| id | codigo | nombre | creditos | cupo | semestre | programa_id |
|---|---|---|---|---|---|---|
| 1 | MAT-101 | Cálculo I | 4 | 40 | 2026-2 | 1 |
| 2 | FIS-201 | Física II | 5 | 35 | 2026-2 | 2 |
| 3 | COM-110 | Taller de Escritura | 2 | 25 | 2026-1 | 3 |

## Pruebas

`docs/ms-cursos.postman_collection.json`: el login y los 8 casos de la ficha 3, más los demás códigos. Ajuste las variables `usuarios_url`, `cursos_url`, `email` y `password` de la colección (para probar por el host virtual: `http://<host>/api/usuarios` y `http://<host>/api/cursos`).
