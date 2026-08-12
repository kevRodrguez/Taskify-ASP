# Taskify

Aplicación ASP.NET Core 10 MVC con autenticación sobre Supabase: registro, inicio de sesión,
recuperación de contraseña y sesiones persistentes.

## Requisitos

- SDK de .NET 10
- Un proyecto en [Supabase](https://supabase.com/dashboard)

## Puesta en marcha

### 1. Credenciales

Las credenciales nunca se guardan en el repositorio. En desarrollo se usan los
[user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) del SDK, que
se almacenan fuera del proyecto:

```bash
dotnet user-secrets set "Supabase:Url" "https://TU-PROYECTO.supabase.co"
dotnet user-secrets set "Supabase:AnonKey" "TU-ANON-KEY"
```

Los dos valores están en el dashboard, en **Project Settings > API**. La clave es la pública
(`anon` o `sb_publishable_...`), no la `service_role`. Funciona igual en Windows, macOS y Linux;
solo cambia la ruta del archivo donde el SDK los guarda.

El `UserSecretsId` ya está en `Taskify.csproj`, así que al clonar el repositorio no hace falta
ejecutar `dotnet user-secrets init`.

### 2. Configuración del dashboard de Supabase

Estos tres pasos son obligatorios: sin ellos los correos llegan con enlaces que la aplicación
no puede procesar.

**Authentication > URL Configuration**

- `Site URL`: `https://localhost:7221`
- En `Redirect URLs`, añadir `https://localhost:7221/**`

**Authentication > Email Templates > Confirm signup**

Reemplazar el enlace del template por:

```html
<a href="{{ .SiteURL }}/auth/confirm?token_hash={{ .TokenHash }}&type=signup">Confirmar mi cuenta</a>
```

**Authentication > Email Templates > Reset Password**

```html
<a href="{{ .SiteURL }}/auth/confirm?token_hash={{ .TokenHash }}&type=recovery">Restablecer mi contraseña</a>
```

Por qué hay que cambiarlos: el template por defecto usa `{{ .ConfirmationURL }}`, que devuelve
la sesión en el fragmento de la URL (`#access_token=...`). El navegador nunca envía el fragmento
al servidor, así que una aplicación renderizada en servidor no puede leerlo. Con `{{ .TokenHash }}`
el token viaja como query string y se canjea en el servidor con `Auth.VerifyTokenHash`, que es el
enfoque que [Supabase recomienda](https://supabase.com/docs/guides/auth/passwords) para apps
server-side.

### 3. Ejecutar

```bash
dotnet run
```

La aplicación queda en `https://localhost:7221`. En macOS y Linux puede hacer falta confiar el
certificado de desarrollo con `dotnet dev-certs https --trust`.

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
DataProtection__KeyRingPath=/keys
```

El doble guion bajo es el separador de secciones que ASP.NET Core traduce a `Supabase:Url`.

Al desplegar con un dominio real hay que actualizar el `Site URL` del dashboard y añadirlo a
`Redirect URLs`, porque `{{ .SiteURL }}` es lo que construye los enlaces de los correos.
