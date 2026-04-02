# Diseño: Refactorización y Mejora de GitHubUpdater

**Fecha**: 2026-04-02
**Estado**: En Revisión
**Tema**: github-updater-refactor

## 1. Introducción
El módulo `KUtilitiesCore.GitHubUpdater` requiere una evolución para ser más robusto, seguir los principios SOLID y facilitar la integración del proceso de descarga con reporte de progreso. Se busca que el usuario pueda configurar patrones de búsqueda para los archivos (assets) y que el sistema maneje diferentes canales (Dev, QA, Test) de forma automática.

## 2. Objetivos
- Desacoplar la lógica de obtención de datos de la lógica de comparación y descarga.
- Implementar un selector de assets basado en patrones (Wildcards).
- Mejorar el análisis de versiones (SemVer compatible).
- Integrar el reporte de progreso de descarga en el flujo principal.

## 3. Arquitectura (SOLID)

### 3.1. Nuevas Interfaces
| Interfaz | Responsabilidad |
| :--- | :--- |
| `IVersionParser` | Extraer y comparar versiones de los Tags de GitHub. |
| `IAssetSelector` | Seleccionar el archivo correcto de la lista de assets de una release. |
| `IGitHubUpdateManager` | Orquestar el flujo completo: Buscar -> Comparar -> Descargar. |

### 3.2. Implementaciones Propuestas
- **`DefaultVersionParser`**: Usará Regex `(\d+\.\d+\.\d+)` para extraer versiones.
- **`WildcardAssetSelector`**: Permitirá filtrar assets usando patrones como `*Siomax*.exe`.
- **`GitHubUpdateManager`**: Clase de alto nivel que consume `GitHubUpdateService` y `GitHubAssetDownloader`.

## 4. Flujo de Trabajo (UpdateManager)
1. **CheckForUpdatesAsync()**: 
   - Obtiene la última release desde `GitHubUpdateService`.
   - Filtra por el canal configurado (`UpdateChannel`).
   - Compara la versión encontrada con la local (`AppVersion`).
2. **DownloadUpdateAsync(path, progress)**:
   - Selecciona el asset usando `IAssetSelector` y el `AssetPattern`.
   - Inicia la descarga usando `GitHubAssetDownloader`.
   - Reporta el progreso a través de `IProgress<DownloadProgressEventArgs>`.

## 5. Cambios en el Modelo de Datos
- **`IAppUpdateInfo`**: 
  - Añadir `string AssetPattern { get; set; }`.
  - Añadir `bool IsUpdateAvailable(string remoteVersion)`.

## 6. Estrategia de Pruebas
- Crear Mocks para las interfaces `IVersionParser` y `IAssetSelector`.
- Pruebas unitarias para validar que el `WildcardAssetSelector` elige el archivo correcto en diferentes escenarios.
- Pruebas de integración (existentes) actualizadas para usar el nuevo flujo.

## 7. Documentación
- Se utilizará XML Documentation en todos los nuevos métodos y clases.
- Se mantendrá la compatibilidad con .NET 4.8 y .NET 8.
