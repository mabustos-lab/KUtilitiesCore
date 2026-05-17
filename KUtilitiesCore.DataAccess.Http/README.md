# KUtilitiesCore.DataAccess.Http

Implementación de la capa de acceso a datos orientada a servicios web y APIs REST, permitiendo que la aplicación consuma datos de forma remota manteniendo la misma abstracción de repositorio.

## Características

- **ApiRepository**: Repositorios especializados en la realización de peticiones HTTP y la gestión de respuestas JSON.
- **ApiUnitOfWork**: Implementación de la unidad de trabajo para coordinar múltiples llamadas a servicios API.
- **Deserialización Automática**: Integración con la infraestructura de datos para convertir respuestas HTTP en entidades de negocio.
- **Flexibilidad de Endpoint**: Configuración sencilla de URLs base y rutas de API para diferentes entornos.

## Instalación

Instale la librería a través de NuGet:
`dotnet add package KUtilitiesCore.DataAccess.Http`
