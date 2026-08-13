# Taskify

Aplicación ASP.NET Core 10 MVC con autenticación sobre Supabase: registro, inicio de sesión,
recuperación de contraseña y sesiones persistentes.

## Requisitos

- SDK de [.NET 10](https://dotnet.microsoft.com/download)
- Herramienta EF Core (una vez por máquina): `dotnet tool install --global dotnet-ef`
- Un proyecto en [Supabase](https://supabase.com/dashboard)

## Levantar el proyecto (desarrollo local)

Sigue estos pasos en orden desde la raíz del repositorio.

### Paso 1 — Entrar al proyecto y restaurar paquetes

```bash
cd Taskify-ASP
dotnet restore
dotnet build
```

### Paso 2 — Configurar user secrets

Las credenciales **no** van en el repo. El `UserSecretsId` ya está en `Taskify.csproj`; no hace falta
`dotnet user-secrets init`.

```bash
dotnet user-secrets set "Supabase:Url" "https://TU-PROYECTO.supabase.co"
dotnet user-secrets set "Supabase:AnonKey" "TU-ANON-KEY"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=TU-HOST.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.TU-PROJECT-REF;Password=TU-PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
```

| Secret | Dónde obtenerlo |
| --- | --- |
| `Supabase:Url` | Dashboard → **Project Settings > API** |
| `Supabase:AnonKey` | Misma pantalla (clave `anon`, no `service_role`) |
| `ConnectionStrings:DefaultConnection` | **Project Settings > Database** → Session pooler (puerto **5432**) |

Si la conexión directa a `db.*.supabase.co` falla por IPv6, usa el **session pooler**
(`*.pooler.supabase.com`, usuario `postgres.TU-PROJECT-REF`). Evita el transaction pooler (6543).

Verifica:

```bash
dotnet user-secrets list
```

### Paso 3 — Configurar Supabase (dashboard, una sola vez)

Sin esto los correos de registro y recuperación no funcionan con esta app.

**Authentication > URL Configuration**

- `Site URL`: `https://localhost:7221`
- `Redirect URLs`: `https://localhost:7221/**`

**Authentication > Email Templates > Confirm signup**

```html
<a href="{{ .SiteURL }}/auth/confirm?token_hash={{ .TokenHash }}&type=signup">Confirmar mi cuenta</a>
```

**Authentication > Email Templates > Reset Password**

```html
<a href="{{ .SiteURL }}/auth/confirm?token_hash={{ .TokenHash }}&type=recovery">Restablecer mi contraseña</a>
```

El template por defecto usa `{{ .ConfirmationURL }}` con el token en el fragmento de la URL (`#...`),
que el servidor nunca recibe. Con `{{ .TokenHash }}` el enlace llega como query string y la app lo
canjea en `/auth/confirm`.

### Paso 4 — Crear / actualizar tablas en PostgreSQL

Con la app **detenida** (cierra cualquier `dotnet run` previo):

```bash
dotnet ef database update
```

Crea o actualiza las tablas en Supabase según las migraciones en `Migrations/`. Esquema documentado
en [`docs/DATABASE.md`](docs/DATABASE.md).

### Paso 5 — Ejecutar la aplicación

```bash
dotnet run
```

Abre **https://localhost:7221** en el navegador.

En macOS/Linux, si HTTPS falla en desarrollo:

```bash
dotnet dev-certs https --trust
```

### Resumen rápido (comandos)

```bash
cd Taskify-ASP
dotnet restore && dotnet build
dotnet user-secrets set "Supabase:Url" "..."
dotnet user-secrets set "Supabase:AnonKey" "..."
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."
dotnet ef database update
dotnet run
```

*(Configura el dashboard de Supabase antes de probar registro por correo.)*

## Cómo funciona la autenticación

La sesión de Supabase se guarda en una única cookie `sb-session`, cifrada con ASP.NET Core Data
Protection y marcada como `HttpOnly`. La escribe el propio SDK a través de
`SupabaseCookieSessionPersistence`, que implementa la interfaz `IGotrueSessionPersistence` del
SDK: cuando Gotrue emite los eventos `SignedIn`, `UserUpdated` o `TokenRefreshed` guarda la
sesión, y cuando emite `SignedOut` la borra. La rotación de tokens se persiste sola.

En cada petición, `SupabaseAuthenticationHandler` carga esa cookie, comprueba localmente la
expiración del JWT y solo llama a Supabase cuando toca refrescar. Después construye el
`ClaimsPrincipal`, de modo que `[Authorize]`, `User.Identity` y `User.FindFirstValue(...)`
funcionan igual que con cualquier otro esquema de ASP.NET Core.

| Ruta | Descripción |
| --- | --- |
| `/Auth/Register` | Alta de usuario |
| `/Auth/Login` | Inicio de sesión |
| `/Auth/ForgotPassword` | Envía el correo de recuperación |
| `/auth/confirm` | Recibe los enlaces de correo y canjea el `token_hash` |
| `/Auth/ResetPassword` | Define la contraseña nueva |
| `/Auth/Profile` | Página protegida de ejemplo |

## Despliegue en un VPS con Dokploy

Dokploy corre sobre Docker Swarm con Traefik por delante. Hay dos ajustes que, si se omiten,
rompen la autenticación de forma silenciosa.

**Persistir el key ring de Data Protection.** Es lo que cifra la cookie de sesión. Por defecto
ASP.NET Core lo escribe en `$HOME/.aspnet/DataProtection-Keys`, que dentro de un contenedor vive
en la capa efímera: cada redespliegue generaría claves nuevas, las cookies existentes dejarían de
descifrarse y todos los usuarios aparecerían desconectados. En **Advanced > Volumes** del servicio
hay que montar un volumen en `/keys` y declarar `DataProtection__KeyRingPath=/keys`. Las imágenes
`aspnet:10.0` corren como usuario no root, así que ese volumen debe ser escribible por su UID.

**Cabeceras del proxy.** Traefik termina el TLS y habla HTTP con el contenedor. `Program.cs` ya
incluye `UseForwardedHeaders`, sin el cual `UseHttpsRedirection` entraría en un bucle de
redirecciones.

Variables en la pestaña **Environment** del servicio, en formato `KEY=value`:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_HTTP_PORTS=8080
Supabase__Url=https://TU-PROYECTO.supabase.co
Supabase__AnonKey=TU-ANON-KEY
ConnectionStrings__DefaultConnection=Host=TU-HOST.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.TU-PROJECT-REF;Password=TU-PASSWORD;SSL Mode=Require;Trust Server Certificate=true
DataProtection__KeyRingPath=/keys
```

El doble guion bajo es el separador de secciones que ASP.NET Core traduce a `Supabase:Url` o
`ConnectionStrings:DefaultConnection`.

Al desplegar con un dominio real hay que actualizar el `Site URL` del dashboard y añadirlo a
`Redirect URLs`, porque `{{ .SiteURL }}` es lo que construye los enlaces de los correos.
