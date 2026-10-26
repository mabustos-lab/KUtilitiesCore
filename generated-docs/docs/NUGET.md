# Empaquetado y Publicación NuGet

## 📦 Generación del Paquete
El proyecto está configurado para generar paquetes `.nupkg` durante el build en modo Release.

```bash
dotnet pack KUtilitiesCore.sln -c Release
```

## 🚀 Publicación
1. **Obtener API Key**: Consigue tu clave desde [nuget.org](https://www.nuget.org/).
2. **Subir Paquete**:
```bash
dotnet nuget push bin/Release/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## ⚠️ Consideraciones de Versionado
Sigue el estándar de **Semantic Versioning (SemVer)**:
- **Major**: Cambios disruptivos (Breaking changes).
- **Minor**: Nuevas funcionalidades compatibles.
- **Patch**: Correcciones de errores.
