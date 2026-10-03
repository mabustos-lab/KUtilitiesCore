# Tasks

> Norma aplicable: proyecto tipo librería aislada (suite de DLLs .NET) — sin base de datos, sin API HTTP y sin frontend. Según la Nota de Validación de `docs/openspec-tasks-mandatory-steps.md`, los pasos de verificación de BD, pruebas curl (N+2) y Playwright E2E (N+3) **se omiten por completo**. La verificación de cada tarea es el build (`dotnet build KUtilitiesCore.sln --no-incremental`) y la suite de tests (`dotnet test --solution KUtilitiesCore.sln`), comandos exigidos por `openspec/config.yaml`.

## 1. Configuración: Crear rama de la característica (OBLIGATORIO - PRIMER PASO)

- [x] 1.1 Crear la rama `feature/eliminate-build-warnings` a partir de la rama base y cambiarse a ella; verificar con `git status` que la rama actual es la correcta
- [x] 1.2 Capturar la línea base de warnings: ejecutar `dotnet build KUtilitiesCore.sln --no-incremental -v:n` y guardar el inventario por proyecto/código (454 en librerías esperados) para medir avance

## 2. Habilitar Nullable en Dal y Data (cuantificar emergentes)

- [x] 2.1 Cambiar `<Nullable>disable</Nullable>` a `<Nullable>enable</Nullable>` en `KUtilitiesCore.Dal/KUtilitiesCore.Dal.csproj` y `KUtilitiesCore.Data/KUtilitiesCore.Data.csproj`; verificar con `dotnet build` que ambos compilan (se permitirá el commit solo junto al grupo 13/11, aquí solo se cuantifica)
- [x] 2.2 Re-ejecutar el build completo y registrar el inventario emergente de CS86xx en `Dal` y `Data` (línea base actualizada); si el volumen crece desproporcionadamente respecto a los ~454, detenerse y re-plantear alcance con el PO antes de continuar (riesgo R1 de design.md)

## 3. Correcciones puntuales globales

- [x] 3.1 Eliminar el constructor de serialización obsoleto (SYSLIB0051) en `KUtilitiesCore.Data/DataExporter/EmptyDataSourceException.cs` y cualquier otro resto de serialización binaria en librerías; verificar que el build no emite SYSLIB0051
- [x] 3.2 Corregir todas las docs XML malformadas (CS1570, CS1572, CS1573, CS1574, CS1711, CS1587 — ~36 en librerías; p. ej. `SpecificationEvaluator.cs:17`, `IPagedResult.cs:20`, `Delegates.cs:14`, `FactoryEncryptionService.cs:14`, `BulkOperationsService.cs:18`): etiquetas desalineadas, `cref` no resueltos, `param`/`typeparam` huérfanos y comentarios fuera de elemento válido; verificar que el build no emite ninguno de esos códigos en librerías

## 4. Librería KUtilitiesCore.Encryption (7 warnings)

- [x] 4.1 Corregir los warnings de nulabilidad (si los hay) y documentar los miembros públicos sin docs (CS1591) en español según criterio D3 de design.md; verificar que `dotnet build` no emite warnings para `KUtiitiesCore.Encryption/KUtilitiesCore.Encryption.csproj` (ruta con triple 'i')
- [x] 4.2 Ejecutar `dotnet test KUtilitiesCoreTests` y confirmar sin regresiones atribuibles a Encryption

## 5. Librería KUtilitiesCore.MVVM.Messaging (7 warnings)

- [x] 5.1 Documentar miembros públicos (CS1591) y corregir nulabilidad/cref en `KUtilitiesCore.MVVM.Messaging`; verificar 0 warnings del proyecto en el build
- [x] 5.2 Ejecutar `dotnet test KUtilitiesCore.MVVMTests` y confirmar sin regresiones

## 6. Librería KUtilitiesCore.GitHubUpdater (10 warnings)

- [x] 6.1 Corregir eventos no-null no inicializados (CS8618 en `DownloadProgress`) con el patrón adecuado (`EventHandler<T>?` nulo o `field` keyword) y documentar miembros públicos; verificar 0 warnings del proyecto en el build
- [x] 6.2 Ejecutar `dotnet test KUtilitiesCore.GitHubUpdaterTests` y confirmar sin regresiones (el fallo 401 de `GitHubUpdateServiceTest` sin `GITHUB_TOKEN` es preexistente y se excluye)

## 7. Librería KUtilitiesCore.MVVM (13 warnings)

- [x] 7.1 Corregir nulabilidad (CS8618/CS8625/CS8600) en `KUtilitiesCore.MVVM` (RelayCommands, helpers de ViewModel) con fixes de flujo, no supresiones; verificar 0 warnings del proyecto en el build
- [x] 7.2 Documentar miembros públicos (CS1591) en español; verificar 0 warnings y ejecutar `dotnet test KUtilitiesCore.MVVMTests` + `dotnet test KUtilitiesCore.MVVMTests.EventCommandBinder`

## 8. Librería KUtilitiesCore.DataAccess (16 warnings — solo docs)

- [x] 8.1 Documentar los 15 miembros públicos sin docs (CS1591) de la capa de abstracciones en español, orientados a propósito (p. ej. contrato de UoW/Specification/Paging) y validar el `cref` de `IPagedResult.cs:20`; verificar 0 warnings del proyecto en el build (mantiene `Nullable disable`, sin cambio de setting)
- [x] 8.2 Ejecutar `dotnet test KUtilitiesCore.DataAccessTests` y confirmar sin regresiones en las abstracciones

## 9. Librería KUtilitiesCore.Logger (18 warnings)

- [x] 9.1 Alinear contratos de nulabilidad CS8767/CS8667 en `ILoggerService`/`NullLoggerService` y corregir el resto de CS86xx en `KUtilities.Logger`; verificar 0 warnings del proyecto en el build
- [x] 9.2 Documentar miembros públicos (CS1591) en español; verificar 0 warnings y ejecutar `dotnet test KUtilitiesCore.LoggerTests`

## 10. Librería KUtilitiesCore.DataAccess.EfCore (13 warnings)

- [x] 10.1 Alinear contratos de nulabilidad CS8766/CS8613 en `EfRepositoryReadOnly` con la interfaz y corregir el resto de CS86xx; verificar 0 warnings del proyecto en el build
- [x] 10.2 Documentar miembros públicos (CS1591) en español; verificar 0 warnings y ejecutar `dotnet test KUtilitiesCore.DataAccessTests`

## 11. Librería KUtilitiesCore.Data (27 warnings + emergentes de Nullable)

- [x] 11.1 Corregir el inventario completo de CS86xx (base + emergentes del grupo 2) en `KUtilitiesCore.Data` con fixes de flujo reales; verificar 0 warnings del proyecto en el build
- [x] 11.2 Documentar los 24 miembros públicos sin docs (CS1591) en español; verificar 0 warnings del proyecto en el build
- [x] 11.3 Ejecutar `dotnet test KUtilitiesCore.DataTests` y confirmar sin regresiones en importación/exportación CSV/Excel

## 12. Librería KUtilitiesCore.Data.Win (26 warnings)

- [x] 12.1 Corregir nulabilidad (CS8618/CS8600/CS8602/CS8625) en los componentes WinForms; verificar 0 warnings del proyecto en el build
- [x] 12.2 Documentar los 18 miembros públicos sin docs (CS1591) en español; verificar 0 warnings y ejecutar `dotnet test KUtilitiesCore.Data.WinTests` (target net10.0-windows)

## 13. Librería KUtilitiesCore.Dal (~175 warnings + emergentes de Nullable)

- [x] 13.1 Corregir nulabilidad de flujos de datos en `DaoContext`, `DataReaderConverter` y estrategias de mapeo (CS8600/02/03/04, CS8625: ~91 base + emergentes) con guardas y anotaciones, sin `!` injustificado; verificar 0 warnings del proyecto en el build
- [x] 13.2 Corregir nulabilidad de `BulkOperationsService` y resto de tipos; verificar 0 warnings del proyecto en el build
- [x] 13.3 Documentar los 75 miembros públicos sin docs (CS1591) en español con `<example>` en el fluent builder (`DataReaderConverter.Create().WithResult<T>()`) y el patrón strategy; verificar 0 warnings del proyecto en el build
- [x] 13.4 Ejecutar `dotnet test KUtilitiesCore.DataAccessTests` y confirmar sin regresiones (respetar los gotchas de mocking de `DbDataReader` de AGENTS.md: `CallBase = true`, `Read()`/`NextResult()` finitos)

## 14. Librería KUtilitiesCore.Core (142 warnings)

- [x] 14.1 Corregir nulabilidad en validación, extensiones LINQ y telemetría (CS8618/CS8625/CS8600/CS8604/CS8767); verificar 0 warnings del proyecto en el build
- [x] 14.2 Documentar los 101 miembros públicos sin docs (CS1591) en español, priorizando helpers de uso no obvio; verificar 0 warnings del proyecto en el build
- [x] 14.3 Ejecutar `dotnet test KUtilitiesCoreTests` y confirmar sin regresiones

## 15. Revisar y actualizar las pruebas unitarias existentes (OBLIGATORIO)

- [x] 15.1 Revisar los proyectos de test en busca de usos de firmas cuyo contrato de nulabilidad cambió (abstracciones de `DataAccess`/`Logger` y sus implementaciones); actualizar solo lo que el cambio exige, sin drive-by refactors; verificar que la suite compila sin errores
- [x] 15.2 Si algún fix de flujo de NULL en librerías carece de cobertura (p. ej. guardas nuevas que lanzan `ArgumentNullException`), añadir el test que lo demuestra antes de darlo por cerrado; verificar con el test correspondiente en verde

## 16. Ejecutar pruebas unitarias y generar reporte (OBLIGATORIO - EL AGENTE DEBE EJECUTARLAS)

- [x] 16.1 Ejecutar la suite completa: `dotnet test --solution KUtilitiesCore.sln`; registrar conteo total, fallos, tiempo de ejecución y tests inestables
- [x] 16.2 Crear el reporte `specs/eliminate-build-warnings/reports/AAAA-MM-DD-paso-N+1-verificacion-de-pruebas-unitarias.md` (bajo la carpeta del cambio) con los comandos ejecutados, los resultados resumidos y el resultado global; el fallo 401 preexistente de `GitHubUpdateServiceTest` sin `GITHUB_TOKEN` se documenta como excepción conocida
- [x] 16.3 Marcar este paso como completado solo después de que la suite pase y el reporte exista

## 17. Actualizar la documentación técnica (OBLIGATORIO)

- [x] 17.1 Actualizar `AGENTS.md`: la nota de convenciones que dice que `KUtilitiesCore.Dal` y `KUtilitiesCore.Data` usan `Nullable disable` debe reflejar que ahora usan `Nullable enable`; verificar leyendo la sección actualizada
- [x] 17.2 Actualizar el `context` de `openspec/config.yaml` (línea sobre Nullable inconsistente) para reflejar el estado post-cambio; verificar con una lectura del archivo
- [x] 17.3 Verificar que ningún consumidor de las librerías dentro de la solución compila con warnings nuevos causados por las anotaciones (los proyectos de test conservan sus 28 preexistentes, ni más ni menos)

## 18. Verificación final de integración

- [x] 18.1 Ejecutar `dotnet build KUtilitiesCore.sln --no-incremental` y confirmar: 0 errores y 0 warnings atribuibles a los 12 proyectos de librería (los 28 de test se mantienen, fuera de alcance)
- [x] 18.2 Confirmar que ningún `.csproj` de librería incorporó `NoWarn` nuevo ni se eliminó `GenerateDocumentationFile`; verificar con una búsqueda en los csproj
- [x] 18.3 Ejecutar `openspec validate --change eliminate-build-warnings` y confirmar que el cambio valida correctamente
