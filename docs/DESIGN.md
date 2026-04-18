```markdown
# Design System Documentation: The Technical Curator

## 1. Overview & Creative North Star
This design system is a specialized, dark-only environment engineered for deep focus, precision, and high-density information architecture. Unlike standard consumer interfaces that rely on friendly roundness and bright splashes of color, this system adopts the **"Technical Curator"** North Star. 

The aesthetic is inspired by high-end developer tools and editorial code journals. It prioritizes data over decoration, using a sophisticated palette of deep slates and navy tones to create an "infinite canvas" feel. We break the "template" look through intentional asymmetry, where large-scale typography meets dense, utility-focused components. It is a system built for those who build.

---

## 2. Colors & Surface Logic
The palette is rooted in the deep shadows of `#10141a`. This is not a flat black; it is a layered, atmospheric navy-slate that provides a sense of immense digital depth.

### The "No-Line" Rule
To achieve a premium, custom feel, designers are **prohibited from using 1px solid borders for sectioning layout.** Large areas of the UI should be defined by shifts in background tokens:
- Use `surface` for the main canvas.
- Use `surface_container_low` for sidebar or secondary navigation zones.
- Use `surface_container_highest` for floating panels or modal surfaces.
*The eye should perceive a change in depth through color, not a "drawn" line.*

### Surface Hierarchy & Nesting
Treat the UI as a series of physical layers. 
1. **Base Layer:** `surface` (#10141a).
2. **Sunken Layer:** `surface_container_lowest` (#0a0e14) for "well" components like code blocks or terminal outputs.
3. **Elevated Layer:** `surface_container_high` (#262a31) for cards or interactive modules.

### The "Glass & Gradient" Rule
For primary call-to-actions and hero sections, we move beyond flat hex codes. 
- **CTAs:** Utilize a subtle linear gradient from `primary` (#acc7ff) to `primary_container` (#498fff) at a 135-degree angle. This creates a "metallic" tech sheen.
- **Floating Elements:** Apply a `surface_variant` with 60% opacity and a `20px` backdrop-blur to create a "frosted glass" tech aesthetic that maintains context with the layers beneath.

---

## 3. Typography
We utilize **Inter** exclusively. Its tall x-height and geometric clarity are essential for the high-contrast, technical look we require.

- **The Power of Scale:** We use a high-contrast ratio between `display-lg` (3.5rem) and `body-sm` (0.75rem). This creates an editorial rhythm—large, authoritative headers followed by precise, dense data.
- **Labeling as UI:** Small labels (`label-sm`) should be used frequently for metadata, mimicking the information density of an IDE. 
- **Weight Strategy:** Use `SemiBold` for titles to command attention against the dark background, but keep body text at `Regular` weight with a slightly increased letter-spacing (+0.01em) to ensure legibility on high-resolution displays.

---

## 4. Elevation & Depth
In this system, depth is a function of **Tonal Layering** rather than drop shadows.

- **The Layering Principle:** Place a `surface_container_lowest` element inside a `surface_container` section to create an "inset" look. This is the primary way to group content.
- **Ambient Shadows:** Standard drop shadows are forbidden. If an element must float (e.g., a dropdown menu), use a massive blur (40px+) with an extremely low-opacity version of `on_surface` (4-6%). It should feel like a soft glow of light being blocked, not a black smudge.
- **The "Ghost Border":** For internal component logic (like input fields), use the `outline_variant` token at 20% opacity. This creates a "barely-there" edge that guides the eye without cluttering the visual field.
- **Corner Radii:** We use a "Sharp-Tech" scale. Most components use `sm` (0.125rem) or `md` (0.375rem). Avoid `full` or `xl` except for specific status indicators.

---

## 5. Components

### Buttons
- **Primary:** Gradient-filled (Primary to Primary Container) with `on_primary_container` text. Sharp corners (`sm`).
- **Secondary:** `surface_container_high` background with a "Ghost Border" of `primary` at 20% opacity.
- **Tertiary:** No background. `primary` text with an underline that only appears on hover.

### Inputs & Text Fields
- **Base:** `surface_container_lowest` background. 
- **Border:** `outline_variant` at 30%. On focus, the border shifts to 100% `primary` with a 2px outer glow (using a 10% opacity primary shadow).
- **Style:** Tight padding, monospace-adjacent feel (high tracking on labels).

### Chips & Tags
- **Selection:** Use `primary_container` with `on_primary_container` text. 
- **Status:** Forbid standard "traffic light" colors where possible. Use `secondary_container` for neutral and `tertiary` (#ffb68b) for warnings to maintain the sophisticated slate palette.

### Cards & Data Lists
- **Rule:** No dividers. Separate list items by `1px` of vertical margin, allowing the `surface_container_low` background of the parent to show through, or use alternating tonal shifts between `surface_container_low` and `surface_container`.

---

## 6. Do's and Don'ts

### Do:
- **Embrace Negative Space:** Use generous "breathing room" around large typography to make the dense data sections feel intentional, not cluttered.
- **Use "Primary" Sparingly:** The `acc7ff` blue is a high-energy highlight. Use it for critical actions and active states only.
- **Prioritize Alignment:** In a borderless layout, strict grid alignment is the only thing holding the UI together. Use a 4px/8px baseline grid religiously.

### Don't:
- **Don't use pure black (#000):** It kills the "navy-slate" depth and creates harsh visual vibration against white text.
- **Don't use standard Dividers:** If you feel the need for a line, try a background color shift first.
- **Don't use Rounded Corners (>8px):** Roundness creates a "consumer-friendly" vibe that contradicts the "Technical Curator" aesthetic. Keep it sharp and professional.
- **Don't use Light Mode:** This system is architected exclusively for low-light, high-performance environments. Transitioning to light mode would break the tonal hierarchy.