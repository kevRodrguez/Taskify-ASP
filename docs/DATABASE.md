# Taskify — Esquema de base de datos

> ERD aprobado. Implementado con EF Core Code First + PostgreSQL (Supabase).

## Tablas

| Tabla | Descripción |
| --- | --- |
| `Profiles` | Perfiles enlazados a `auth.users` vía FK en `UserId` |
| `Teams` | Equipos de trabajo |
| `TeamMembers` | M:N perfil ↔ equipo (roles: Owner, Admin, Member) |
| `Projects` | Proyectos por equipo |
| `TaskItems` | Tareas Kanban por proyecto |
| `Notifications` | Notificaciones por perfil |

## Migraciones

Generar migración (con la app detenida):

```powershell
dotnet ef migrations add InitialCreate
```

Aplicar a Supabase Postgres:

```powershell
dotnet ef database update
```

Revertir última migración:

```powershell
dotnet ef database update NombreMigracionAnterior
dotnet ef migrations remove
```

## Convenciones

- Tablas y columnas en **PascalCase** (convención por defecto de EF Core)
- **Nombres en inglés** para tablas, columnas y valores de enum (estándar de BD)
- Comentarios en código pueden estar en español; identificadores de BD siempre en inglés
- Entidades C# en `Models/Entities/`
- Enums en `Models/Enums/`
- Configuración Fluent API en `Data/Configurations/` (relaciones, longitudes, índices)

## Enums

| Enum | Valores |
| --- | --- |
| `TeamMemberRole` | `Owner`, `Admin`, `Member` |
| `ProjectStatus` | `Active`, `Completed`, `Archived` |
| `TaskItemStatus` | `Todo`, `InProgress`, `Done` |
