# Taskify — Guía de branding

> Referencia visual derivada de la muestra de diseño (agencia creativa / estética premium cálida).
> Los tokens canónicos viven en [`wwwroot/css/brand-tokens.css`](../wwwroot/css/brand-tokens.css).

---

## 1. Identidad visual

| Atributo | Valor |
| --- | --- |
| **Nombre** | Taskify |
| **Descriptor** | Gestor de tareas y proyectos colaborativo |
| **Tono** | Profesional, cálido, directo, con sensación premium |
| **Estilo** | Minimalista, mucho espacio en blanco, tipografía bold en titulares, acentos naranja vibrantes |

---

## 2. Paleta de colores

### Colores principales

| Token CSS | Hex | Uso |
| --- | --- | --- |
| `--color-bg-primary` | `#F5F2EE` | Fondo general de la aplicación (crema cálido) |
| `--color-accent` | `#EF661F` | Botones primarios, etiquetas, números destacados, links activos |
| `--color-accent-hover` | `#D95515` | Hover de botones y links de acento |
| `--color-accent-muted` | `#FDF0E8` | Fondos suaves de acento (badges, highlights) |
| `--color-text-primary` | `#1A1A1A` | Titulares, texto principal, navbar |
| `--color-text-secondary` | `#707070` | Texto de apoyo, metadatos, footer |
| `--color-border` | `#E2DCD5` | Bordes, divisores, inputs inactivos |
| `--color-surface` | `#FFFFFF` | Tarjetas, formularios, dropdowns |
| `--color-white` | `#FFFFFF` | Texto sobre fondos de acento |

### Colores semánticos

| Token CSS | Hex | Uso |
| --- | --- | --- |
| `--color-danger` | `#C0392B` | Errores de validación, alertas destructivas |
| `--color-success` | `#2D6A4F` | Confirmaciones, estados completados |
| `--color-info` | `#3D5A80` | Mensajes informativos (TempData, avisos) |

### Proporciones recomendadas

- **60 %** fondo crema (`--color-bg-primary`)
- **30 %** texto oscuro y superficies blancas
- **10 %** acento naranja (CTA, labels, métricas)

---

## 3. Tipografía

| Rol | Familia | Peso | Tamaño (desktop) | Notas |
| --- | --- | --- | --- | --- |
| **Display / H1** | Inter | 700 | 2.75–3.5 rem | Titulares hero, tracking tight (`-0.02em`) |
| **H2–H4** | Inter | 600–700 | 1.25–2 rem | Secciones y títulos de tarjeta |
| **Body** | Inter | 400 | 1 rem (16 px) | Line-height 1.6 |
| **Label / Eyebrow** | Inter | 600 | 0.75 rem | ALL CAPS, letter-spacing `0.08em`, color acento |
| **Small / Caption** | Inter | 400 | 0.875 rem | Color secundario |

**Fuente:** [Inter](https://fonts.google.com/specimen/Inter) vía Google Fonts.

---

## 4. Componentes UI

### Botón primario

- Forma: **pill** (`border-radius: 9999px`)
- Fondo: `--color-accent`
- Texto: blanco, peso 600
- Hover: `--color-accent-hover`, ligera elevación
- Padding: `0.75rem 1.75rem`

### Botón secundario

- Forma: pill
- Fondo: transparente
- Borde: `1px solid var(--color-border)`
- Texto: `--color-text-primary`
- Hover: fondo `--color-accent-muted`

### Tarjetas (auth, perfil)

- Fondo: `--color-surface`
- Borde: `1px solid var(--color-border)`
- Radio: `16px` (`--radius-card`)
- Sombra: `--shadow-md` (sutil, cálida)
- Padding interno: `2rem`

### Inputs

- Fondo: blanco
- Borde: `1px solid var(--color-border)`
- Radio: `12px` (`--radius-input`)
- Focus: borde acento + ring suave naranja (`--color-accent-ring`)

### Navbar

- Fondo: `--color-bg-primary` (sin sombra pesada)
- Borde inferior: `1px solid var(--color-border)`
- Logo: bold, `--color-text-primary`
- Links: secundarios; hover → acento

### Etiqueta eyebrow

```html
<p class="brand-eyebrow">GESTIÓN DE PROYECTOS</p>
```

- Uppercase, color acento, letter-spacing amplio, tamaño pequeño

---

## 5. Espaciado y layout

| Token | Valor | Uso |
| --- | --- | --- |
| `--space-xs` | `0.25rem` | Gaps mínimos |
| `--space-sm` | `0.5rem` | Entre label e input |
| `--space-md` | `1rem` | Padding de secciones |
| `--space-lg` | `1.5rem` | Entre bloques de formulario |
| `--space-xl` | `2.5rem` | Separación hero / contenido |
| `--space-2xl` | `4rem` | Padding vertical de hero |

**Grid:** contenido principal max-width `1140px`, formularios auth centrados en `480px`.

---

## 6. Sombras y profundidad

| Token | Valor |
| --- | --- |
| `--shadow-sm` | `0 2px 8px rgba(26, 26, 26, 0.06)` |
| `--shadow-md` | `0 8px 24px rgba(26, 26, 26, 0.08)` |
| `--shadow-lg` | `0 16px 48px rgba(26, 26, 26, 0.10)` |

Evitar sombras frías azuladas; mantener tono cálido basado en `#1A1A1A`.

---

## 7. Iconografía y decoración

- Flechas de acción: dirección noreste (↗) en CTAs principales
- Logos de terceros / partners: monocromo gris (`--color-text-secondary`)
- Líneas divisoras: `1px solid var(--color-border)`, no grises fríos
- Estadísticas: número en acento, descripción en secundario debajo

---

## 8. Clases CSS del sistema

| Clase | Propósito |
| --- | --- |
| `.brand-eyebrow` | Etiqueta superior en mayúsculas |
| `.brand-hero` | Sección hero de landing |
| `.brand-hero__title` | Titular display |
| `.brand-hero__lead` | Párrafo introductorio |
| `.auth-page` | Contenedor centrado de formularios |
| `.auth-card` | Tarjeta de autenticación |
| `.btn-brand-primary` | Botón CTA principal |
| `.btn-brand-secondary` | Botón outline |
| `.brand-stat` | Bloque de métrica (número + label) |
| `.brand-link` | Enlace con color acento |

---

## 9. Accesibilidad

- Contraste texto principal sobre fondo crema: **≥ 12:1** ✓
- Contraste acento `#EF661F` sobre blanco para texto pequeño: usar solo en elementos ≥ 14 px bold o ≥ 18 px regular
- Botones primarios: texto blanco sobre naranja cumple WCAG AA para UI components
- Focus visible: ring naranja de 3 px, nunca eliminar outline sin reemplazo

---

## 10. Implementación en el repositorio

```
wwwroot/css/
  brand-tokens.css   ← constantes (:root)
  site.css           ← componentes y overrides Bootstrap

Views/Shared/_Layout.cshtml   ← carga fuentes + tokens
Views/Auth/*.cshtml           ← formularios con auth-card
Views/Home/Index.cshtml       ← hero de landing
```

Al añadir nuevas pantallas, importar siempre `brand-tokens.css` antes de `site.css` y reutilizar las clases documentadas arriba en lugar de colores hardcodeados.
