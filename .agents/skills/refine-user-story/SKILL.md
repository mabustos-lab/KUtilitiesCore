---
name: refine-user-story
description: Analiza y mejora historias de usuario usando el contexto técnico del proyecto (arquitectura, objetivos, dependencias y documentación). Úsala cuando el usuario comparta una historia de usuario y pida refinarla, validarla, mejorarla o evaluar si está lista para desarrollo — incluso si no lo pide con esas palabras exactas (p. ej. "revisa este ticket", "¿esta historia está completa?", "prepara este US para desarrollo", "dale una pasada a esta HU").
author: Siomax
version: 1.0.0
---
# refine-user-story Skill

Actúa como un experto de producto con conocimiento técnico profundo del proyecto. Esta skill evalúa historias de usuario contra el contexto del proyecto y un checklist de calidad, y produce una versión mejorada cuando hace falta.

## Instrucciones

Historia de usuario recibida: $ARGUMENTS

Sigue estos pasos:

1. **Reunir el contexto del proyecto.** Antes de evaluar, revisa `@documentation` y cualquier material de arquitectura, objetivos de negocio y dependencias disponible en el proyecto. Si el contexto disponible es insuficiente para juzgar viabilidad técnica o valor de negocio, dilo explícitamente en la salida en lugar de asumir.
2. **Entender el problema.** Identifica qué problema busca resolver la historia y para quién.
3. **Evaluar críticamente** la historia según estos seis criterios:
   - **Claridad del Problema** — ¿está claramente definido el problema que resuelve?
   - **Criterios de Aceptación** — ¿son SMART (específicos, medibles, alcanzables, relevantes, con límite de tiempo) y suficientemente detallados para guiar la implementación?
   - **Valor para el Usuario/Negocio** — ¿es claro el valor que aporta al usuario final o al negocio?
   - **Independencia** — ¿tiene dependencias significativas con otras historias que puedan bloquear su implementación?
   - **Viabilidad Técnica** — ¿es factible con la arquitectura y tecnologías actuales del proyecto?
   - **Nivel de Detalle** — ¿hay requisitos funcionales/no funcionales, ejemplos y casos de uso suficientes para una implementación autónoma?
4. **Decidir si la historia está lista.** Si cumple los seis criterios con suficiencia, decláralo lista para desarrollo y detente ahí — no fuerces una versión "Enhanced" innecesaria.
5. **Si falta detalle**, produce la salida completa con las secciones del formato de abajo.

## Formato de salida

- `## Original` — la historia tal como se recibió.
- `## Evaluación` — checklist de los seis criterios (✅ / ⚠️ / ❌) con una línea de justificación cada uno.
- `## Preguntas Clave` — preguntas puntuales para el Product Owner o el equipo, solo si hay ambigüedades reales.
- `## Riesgos` — riesgos técnicos o de negocio detectados, si los hay.
- `## Enhanced` — versión mejorada de la historia, alineada al contexto técnico del proyecto (solo si el paso 4 determinó que falta detalle).

## Notas

- No inventes detalles de arquitectura o negocio que no estén respaldados por `@documentation` o el contexto compartido en el chat; en su lugar, pregunta.
- Si la historia ya cumple los seis criterios, no generes la sección `## Enhanced` — solo confirma que está lista.
- Esta versión no usa Jira: el ticket siempre se recibe como texto directo en el prompt/chat.
