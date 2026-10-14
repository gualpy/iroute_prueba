# iRoute – Carga y procesamiento de comercios

Aplicación web para cargar archivos `commerce_DDMMYYYY.csv`, procesarlos por fecha y revisar los registros que no pasan las validaciones.

- **Front End:** Angular 21
- **Back End:** .NET 8 (API REST)
- **Base de datos:** SQL Server 2022

## Nota sobre Docker

Desarrollé la práctica en **Linux**, donde SQL Server no se instala de forma nativa como en Windows. Por eso corro SQL Server en un contenedor **Docker** (`docker-compose.yml`), que se comporta como una instancia local en `localhost,1433`. La API usa autenticación SQL (`sa`), no autenticación de Windows, así que funciona igual en cualquier sistema operativo.

Si ya tienes una instancia local de SQL Server (por ejemplo en Windows), no necesitas Docker: ejecuta `database/01_schema.sql` en ella y cambia el connection string en `backend/IRoute.Api/appsettings.json`.

## Estructura

```
backend/IRoute.Api/   API REST (.NET 8)
frontend/             Aplicación Angular
database/             Script SQL (tablas, tipo tabla y stored procedures)
samples/              CSV de ejemplo
docker-compose.yml    SQL Server en contenedor
```

## Requisitos

- Docker (con Docker Compose)
- .NET SDK 8.0
- Node.js 20+ y npm
- Angular CLI (`npm i -g @angular/cli`), o usar `npx ng`

## Ejecución

Abre tres terminales, desde la raíz del repositorio.

### 1. Base de datos

```bash
docker compose up -d
```

Espera unos segundos a que SQL Server arranque y carga el esquema (solo la primera vez):

```bash
docker cp database/01_schema.sql iroute-sql:/tmp/
docker exec iroute-sql /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'IRoute_Pass123!' -b -i /tmp/01_schema.sql
```

Esto crea la base `iroute`, las tablas `commerce` y `commerce_quarantine`, y los stored procedures `sp_create_commerce` y `sp_process_commerce`.

### 2. API

```bash
cd backend/IRoute.Api
dotnet run --launch-profile http
```

La API queda en `http://localhost:5009` y Swagger en `http://localhost:5009/swagger`.

### 3. Front End

```bash
cd frontend
npm install
npx ng serve
```

Abre `http://localhost:4200`.

## Cómo probar

1. **Cargar:** selecciona `samples/commerce_13102026.csv`, revisa la previsualización y pulsa *Enviar al servidor*. Debe cargar 7 registros.
2. **Procesar:** elige la fecha `13/10/2026` y pulsa *Procesar*. Debe enviar 4 registros a cuarentena.
3. **Errores:** lista los 4 registros rechazados con su motivo.

Para repetir la prueba desde cero:

```bash
docker exec iroute-sql /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'IRoute_Pass123!' -d iroute -Q "TRUNCATE TABLE commerce; TRUNCATE TABLE commerce_quarantine"
```

## Formato del CSV

El nombre debe ser `commerce_DDMMYYYY.csv`. Encabezados y formato de fecha `yyyy-MM-dd`:

```
pc_numdoc,pc_tipodoc,pc_nomcomred,pc_razonsocial,pc_direccion,pc_telefono,pc_email,pc_processdate
```

El enunciado no incluía el CSV, por lo que definí estas columnas básicas.

## API REST

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/commerce/upload` | Recibe el `.csv` (form-data, campo `file`). Retorna 400 si está vacío o el nombre es inválido. Inserta con `sp_create_commerce`. Respuesta: `{ "inserted": n }` |
| POST | `/api/commerce/process` | Body `{ "processDate": "yyyy-MM-dd" }`. Ejecuta `sp_process_commerce`. Respuesta: `{ "quarantined": n }` |
| GET | `/api/commerce/quarantine` | Lista los registros de `commerce_quarantine` con su `motivo` |

## Validaciones (`sp_process_commerce`)

Para los registros de `commerce` con la fecha indicada:

- `pc_nomcomred` no debe estar vacío.
- `pc_numdoc` no debe estar vacío, ni contener letras ni caracteres especiales.

Los registros que no cumplen se eliminan de `commerce` y se registran en `commerce_quarantine` con el motivo, por ejemplo:

- `El nombre del comercio (nomcomred) se encuentra vacio.`
- `El número (numdoc) contiene letras.`
- `El número (numdoc) contiene caracteres especiales.`

## Decisiones de diseño

- **Carga masiva:** `sp_create_commerce` recibe todas las filas en un parámetro de tipo tabla (`commerce_type`). Es una sola llamada a la base por archivo, en vez de una por fila.
- **Proceso transaccional:** mover a cuarentena y borrar de `commerce` ocurre en una sola transacción.
- **Capas en la API:** controlador → parser CSV (CsvHelper) → repositorio (ADO.NET con stored procedures).
- **Previsualización en el cliente:** el front lee el CSV en el navegador y valida nombre y contenido antes de enviarlo.

## Configuración

Connection string en `backend/IRoute.Api/appsettings.json`. Las credenciales (`sa` / `IRoute_Pass123!`) son solo para desarrollo local.

## Pendiente

- Pantalla de inicio de sesión (opcional en el enunciado).
