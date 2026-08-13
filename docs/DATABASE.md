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

## Borrado y ciclo de vida

### Resumen por entidad

| Entidad | Estrategia | Campo(s) | Cómo se aplica |
| --- | --- | --- | --- |
| `Profiles` | Hard delete | — | Cascade desde `auth.users` |
| `TeamMembers` | Hard delete | — | Borrar fila al salir del equipo |
| `Notifications` | Hard delete | — | Borrar fila |
| `Teams` | **Soft delete** | `DeletedAt` | `null` = activo; timestamp = borrado |
| `TaskItems` | **Soft delete** | `DeletedAt` | `null` = activo; timestamp = borrado |
| `Projects` | **Estado (enum)** | `Status` | Ver tabla abajo — **no** usa `DeletedAt` |

### Soft delete (`Teams`, `TaskItems`)

- **Borrar:** `entity.DeletedAt = DateTimeOffset.UtcNow`
- **Restaurar:** `entity.DeletedAt = null`
- **Consultas:** EF omite borrados con `HasQueryFilter(e => e.DeletedAt == null)`
- **Ver borrados:** `.IgnoreQueryFilters()` (admin, restauración)

### Proyectos — sin `DeletedAt` ni columna `Archived`

Los proyectos **no tienen** soft delete. El archivado va en la columna **`Status`** (`int4`):

| Valor | Enum | Uso |
| --- | --- | --- |
| `0` | `Active` | Proyecto en curso (listados por defecto) |
| `1` | `Completed` | Proyecto terminado |
| `2` | `Archived` | Proyecto archivado (equivalente a “dado de baja” sin borrar la fila) |

```csharp
// Archivar un proyecto (NO es soft delete)
project.Status = ProjectStatus.Archived;

// Listar solo activos
.Where(p => p.Status == ProjectStatus.Active)
```

**Por qué enum y no `DeletedAt`:** permite distinguir *completado* (`Completed`) de *archivado* (`Archived`) sin dos mecanismos distintos.

## Enums

| Enum | Valores |
| --- | --- |
| `TeamMemberRole` | `Owner`, `Admin`, `Member` |
| `ProjectStatus` | `Active`, `Completed`, `Archived` |
| `TaskItemStatus` | `Todo`, `InProgress`, `Done` |
