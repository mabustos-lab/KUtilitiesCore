# KUtilitiesCore.DataAccess.EfCore

Implementación de la capa de acceso a datos utilizando Entity Framework Core, integrando las abstracciones de KUtilitiesCore.DataAccess con la potencia de EF Core.

## Características

- **EfRepository**: Implementaciones concretas de repositorios optimizados para el uso de DbSet y consultas de Entity Framework.
- **Specification Evaluator**: Traductor de especificaciones de KUtilitiesCore a expresiones compatibles con IQueryable de EF Core.
- **EfUnitOfWork**: Implementación del patrón Unit of Work basada en el DbContext de Entity Framework.
- **Integración Nativa**: Aprovecha las capacidades de seguimiento y carga de EF Core mientras mantiene la abstracción de la capa de datos.

## Instalación

Instale la librería a través de NuGet:
`dotnet add package KUtilitiesCore.DataAccess.EfCore`
