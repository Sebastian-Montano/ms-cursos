# ms-cursos

Microservicio REST para consultar y gestionar cursos académicos.

| Información | Detalle |
|---|---|
| Entidad gestionada | Cursos |
| Tecnología | ASP.NET Core Web API |
| Lenguaje | C# |
| Base de datos | MySQL |
| Acceso a datos | MySqlConnector (ADO.NET) |
| Arquitectura | Controller - Service - Repository |
| Puerto | Configurable en `appsettings.json` (por defecto, `30111`) |

## 1. Descripción

El microservicio `ms-cursos` administra cursos, incluidos su código, nombre, cantidad de créditos, cupo y semestre. Permite listar cursos —opcionalmente filtrados por semestre— y consultar, crear, actualizar y eliminar cursos.

La implementación está organizada en capas:

- **Controllers:** reciben las solicitudes HTTP y devuelven las respuestas.
- **Services:** aplican las reglas de negocio y manejan excepciones.
- **Repositories:** ejecutan consultas parametrizadas y operaciones de persistencia en MySQL.
- **Models:** representan la entidad `Curso` y los DTO de solicitud.
- **Infrastructure:** contiene la fábrica de conexiones a MySQL.

Las rutas del controlador están protegidas con JWT. La creación, actualización y eliminación también requieren uno de los roles configurados en el controlador, actualmente `administrador` o `admisiones`.

## 2. Requisitos previos

- .NET SDK 10.0.
- MySQL Server 8.0 o compatible.
- Git.
- Un token JWT válido emitido por el servicio de autenticación.

## 3. Instalación y ejecución

### 3.1. Obtener el proyecto

```bash
git clone URL_DEL_REPOSITORIO
cd ms-cursos
```

### 3.2. Restaurar dependencias

```bash
dotnet restore
```

Los paquetes usados por el proyecto incluyen:

```bash
dotnet add package MySqlConnector
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.0
```

La versión de `Microsoft.AspNetCore.Authentication.JwtBearer` debe ser compatible con el `TargetFramework` del proyecto.

### 3.3. Configurar la aplicación

En la raíz del proyecto, configura `appsettings.json` con el host, puerto, secreto JWT y cadena de conexión. Ejemplo:

```json
{
  "Host": "127.0.0.1",
  "Port": 3011,
  "Jwt": {
    "Secret": "REEMPLAZAR_POR_UN_SECRETO_SEGURO"
  },
  "ConnectionStrings": {
    "Cursos": "Server=localhost;Port=3306;Database=cursos_db;User ID=cursos_user;Password=TU_CLAVE;"
  }
}
```

El secreto JWT debe coincidir con el usado para firmar los tokens. No publiques secretos ni contraseñas reales en el repositorio. Para desarrollo local, puedes usar .NET User Secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "REEMPLAZAR_POR_UN_SECRETO_SEGURO"
dotnet user-secrets set "ConnectionStrings:Cursos" "Server=localhost;Port=3306;Database=cursos_db;User ID=cursos_user;Password=TU_CLAVE;"
```

Asegúrate de que `MySqlConnectionFactory` lea la cadena de conexión con el nombre `Cursos`. Si usa otro nombre, ajusta la configuración para que coincida.

### 3.4. Crear la base de datos

Crea la base de datos y el usuario:

```sql
CREATE DATABASE cursos_db
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

CREATE USER 'cursos_user'@'localhost'
IDENTIFIED BY 'TU_CLAVE';

GRANT SELECT, INSERT, UPDATE, DELETE
ON cursos_db.*
TO 'cursos_user'@'localhost';

FLUSH PRIVILEGES;
```

Crea la tabla `cursos`:

```sql
USE cursos_db;

CREATE TABLE cursos (
    id INT NOT NULL AUTO_INCREMENT,
    codigo VARCHAR(50) NOT NULL,
    nombre VARCHAR(255) NOT NULL,
    creditos INT NOT NULL,
    cupo INT NOT NULL,
    semestre VARCHAR(6) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_cursos_codigo (codigo),
    CONSTRAINT chk_cursos_creditos CHECK (creditos BETWEEN 1 AND 6),
    CONSTRAINT chk_cursos_cupo CHECK (cupo > 0),
    CONSTRAINT chk_cursos_semestre
        CHECK (semestre REGEXP '^[0-9]{4}-[1-2]$')
);
```

La restricción `UNIQUE` en `codigo` protege la unicidad también ante solicitudes simultáneas. El servicio verifica previamente si el código existe y responde `409 Conflict` cuando detecta un duplicado.

### 3.5. Ejecutar el microservicio

```bash
dotnet run
```

Con la configuración de ejemplo, el servicio estará disponible en:

```text
http://localhost:3000
```

El endpoint de salud no requiere autenticación:

```bash
curl http://localhost:3000/health
```

Respuesta esperada:

```json
{
  "servicio": "ms-cursos",
  "estado": "ok"
}
```

## 4. Representación y validaciones

### 4.1. Representación JSON

```json
{
  "id": 1,
  "codigo": "INF-101",
  "nombre": "Introducción a la programación",
  "creditos": 3,
  "cupo": 30,
  "semestre": "2026-1"
}
```

### 4.2. Atributos

| Atributo | Tipo | Obligatorio | Reglas |
|---|---|---:|---|
| `id` | entero | Sí, generado automáticamente | Clave primaria autoincremental. |
| `codigo` | texto | Sí | Debe ser único. |
| `nombre` | texto | Sí | No puede estar vacío. |
| `creditos` | entero | Sí | Entre 1 y 6, inclusive. |
| `cupo` | entero | Sí | Mayor que 0. |
| `semestre` | texto | Sí | Formato `AAAA-1` o `AAAA-2`, por ejemplo `2026-1`. |

El JSON usa nombres de propiedad en `snake_case`. Al crear un curso se deben enviar todos los campos excepto `id`. En la actualización, los campos son opcionales: los campos omitidos conservan su valor actual.

## 5. Endpoints

Ruta base:

```text
http://localhost:3011/cursos
```

Excepto `/health`, los endpoints requieren un token JWT enviado en la cabecera:

```text
Authorization: Bearer <token>
```

| Método | Endpoint | Parámetros | Descripción | Respuestas principales |
|---|---|---|---|---|
| GET | `/cursos` | Ninguno | Lista todos los cursos. | `200 OK` |
| GET | `/cursos?semestre=2026-1` | `semestre` (query, opcional) | Lista los cursos del semestre indicado. | `200 OK`, `400 Bad Request` |
| GET | `/cursos/{id}` | `id` (path) | Consulta un curso por ID. | `200 OK`, `400 Bad Request`, `404 Not Found` |
| POST | `/cursos` | JSON | Crea un curso. Requiere rol autorizado. | `201 Created`, `400 Bad Request`, `409 Conflict` |
| PUT | `/cursos/{id}` | `id` (path) y JSON | Actualiza los campos incluidos. Requiere rol autorizado. | `200 OK`, `400 Bad Request`, `404 Not Found`, `409 Conflict` |
| DELETE | `/cursos/{id}` | `id` (path) | Elimina un curso. Requiere rol autorizado. | `204 No Content`, `400 Bad Request`, `404 Not Found` |

Los roles permitidos para POST, PUT y DELETE están definidos en `CursosController`:

```csharp
[Authorize(Roles = "administrador, admisiones")]
```

El token debe incluir una claim `rol` cuyo valor coincida con uno de esos roles. Para cambiar los roles permitidos, modifica los atributos `[Authorize(Roles = "...")]`.

### 5.1. Listar cursos

```bash
curl http://localhost:3011/cursos \
  -H "Authorization: Bearer <token>"
```

Para filtrar por semestre:

```bash
curl "http://localhost:3011/cursos?semestre=2026-1" \
  -H "Authorization: Bearer <token>"
```

### 5.2. Consultar un curso

```bash
curl http://localhost:3011/cursos/1 \
  -H "Authorization: Bearer <token>"
```

### 5.3. Crear un curso

```bash
curl -X POST http://localhost:3011/cursos \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "codigo": "INF-101",
    "nombre": "Introducción a la programación",
    "creditos": 3,
    "cupo": 30,
    "semestre": "2026-1"
  }'
```

La respuesta es `201 Created` e incluye el curso creado y su ID asignado por MySQL.

### 5.4. Actualizar un curso

Los campos enviados se actualizan; los omitidos conservan su valor actual.

```bash
curl -X PUT http://localhost:3011/cursos/1 \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "cupo": 35,
    "semestre": "2026-2"
  }'
```

### 5.5. Eliminar un curso

```bash
curl -X DELETE http://localhost:3011/cursos/1 \
  -H "Authorization: Bearer <token>"
```

Si la eliminación es exitosa, devuelve `204 No Content`.

## 6. Respuestas HTTP y errores

| Código HTTP | Descripción |
|---|---|
| `200 OK` | Consulta o actualización completada. |
| `201 Created` | Curso creado correctamente. |
| `204 No Content` | Curso eliminado correctamente. |
| `400 Bad Request` | ID, semestre o datos de entrada inválidos. |
| `401 Unauthorized` | Falta el token o no es válido. |
| `403 Forbidden` | El usuario autenticado no tiene el rol requerido. |
| `404 Not Found` | No existe el curso solicitado o la ruta no existe. |
| `409 Conflict` | Ya existe un curso con el mismo código. |
| `500 Internal Server Error` | Error interno no controlado. |

Los errores se devuelven como JSON. Por ejemplo:

```json
{
  "error": "Ya existe un curso con el código INF-101"
}
```

## 7. Estructura del proyecto

```text
ms-cursos/
├── Controllers/
│   └── CursosController.cs
├── Models/
│   └── Curso.cs
├── Services/
│   ├── ICursosService.cs
│   └── CursosService.cs
├── Repositories/
│   ├── ICursosRepository.cs
│   └── CursosRepository.cs
├── Infrastructure/
│   └── Database/
│       ├── IMySqlConnectionFactory.cs
│       └── MySqlConnectionFactory.cs
├── Exceptions/
│   └── ApiException.cs
├── appsettings.json
├── Program.cs
└── ms-cursos.csproj
```

La estructura puede variar según la organización definitiva del proyecto.

## 8. Persistencia de datos

`CursosRepository` realiza las operaciones de acceso a datos mediante consultas SQL parametrizadas:

- `FindAllAsync(semestre)`: obtiene todos los cursos o filtra por semestre.
- `FindByIdsAsync(ids)`: busca cursos por una colección de IDs.
- `FindByIdAsync(id)`: busca un curso por su ID.
- `FindByCodigoAsync(codigo)`: busca un curso por código.
- `FindBySemestreAsync(semestre)`: busca cursos por semestre.
- `CreateAsync(curso)`: inserta un curso.
- `UpdateAsync(id, curso)`: actualiza un curso.
- `RemoveAsync(id)`: elimina un curso.

La fábrica `IMySqlConnectionFactory` centraliza la creación de conexiones a MySQL.