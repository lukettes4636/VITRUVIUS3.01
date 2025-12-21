# Auditoría Sensorial Ultra-Profunda: Audición y Percepción - VITRUVIUS 3.01

Tras un análisis línea por línea de `EnemySenses.cs`, `EnemyBrain.cs` y los emisores de ruido, se han detectado fallos en la arquitectura de "escucha" que explican por qué el enemigo se siente inconsistente.

---

## 1. El Fallo de la "Atenuación Exponencial" (Paredes)

### Problema Técnico:
En `CalculateAudioStrength`, el ruido se atenúa usando:
`attenuatedRadius = rawRadius * Mathf.Pow(soundAttenuationPerWall, walls);`

### Por qué falla:
*   **Raycast All:** `CountSoundBlockers` usa un solo rayo desde el centro del enemigo al centro del objetivo.
*   **El Efecto "Búnker":** Si hay una esquina pequeña o un objeto decorativo en la trayectoria, el rayo lo cuenta como una "pared" completa, reduciendo el ruido drásticamente (ej. al 70%). Esto hace que el enemigo se vuelva "sordo" injustamente por obstáculos visuales que no deberían bloquear tanto el sonido.
*   **Falta de Difracción:** El sonido no "rodea" las esquinas; si no hay línea directa de raycast, el cálculo de muros es impreciso.

---

## 2. El Sesgo de "Ruido Activo" (Filtro Binario)

### Problema Técnico:
En `ProcessAudioDetection`, existe este filtro:
`bool isEmittingActiveNoise = noiseRadius > idleNoiseRadius + 0.1f;`

### Por qué falla:
*   **Detección de Sigilo Imposible:** Si el jugador se mueve muy agachado o en idle, el enemigo lo ignora **incluso si está a 0.1 metros**, porque el código retorna inmediatamente si el ruido no supera el umbral de "activo".
*   **Consecuencia:** El enemigo parece no tener "presencia". Puedes estar respirándole en la nuca y, si no te mueves, eres invisible para sus oídos.

---

## 3. Inconsistencia Vertical (El Problema de los Pisos)

### Problema Técnico:
La IA usa distancias Euclidianas (`Vector3.Distance`) pero el NavMesh es 2.5D.

### Qué sucede:
*   Si el jugador corre en el piso de arriba, el enemigo detecta un "ruido fuerte" justo encima de él. 
*   Como la distancia es corta (ej. 3 metros hacia arriba), la IA intenta "atacar" el techo o caminar contra la pared de abajo.
*   **Fallo de Lógica:** No hay una validación de `Y-offset`. El sonido debería viajar por las escaleras o atenuarse masivamente a través de suelos de hormigón. Aquí atraviesa todo por igual.

---

## 4. Auditoría de Objetos (ObjectNoiseEmitter)

### Problema Técnico:
`ObjectNoiseDetection` busca objetos en la escena, pero los descarta si la IA ya tiene un jugador en el punto de mira.

### Qué sucede:
*   Si lanzas una botella para distraer al enemigo mientras te persigue, la IA **ignora la botella totalmente** porque prioriza al `CurrentPlayer`.
*   Esto elimina la posibilidad de usar distracciones tácticas durante una persecución, haciendo que el enemigo se sienta "programado para seguirte" en lugar de un ser reactivo a estímulos del mundo.

---

## 5. Matriz de Soluciones Sugeridas (Nivel Experto)

| Sistema | Fallo Detectado | Solución Recomendada |
| :--- | :--- | :--- |
| **Audición** | Atenuación por Raycast único | Usar un promedio de 3 rayos (pies, centro, cabeza) para contar muros. |
| **Sigilo** | Filtro de "Ruido Activo" | Eliminar el filtro binario; usar un gradiente donde el `idleNoise` también detecte si está muy cerca. |
| **Verticalidad** | Sonido traspasa suelos | Añadir un `floorAttenuation` extra si `abs(deltaY) > 2.0f`. |
| **Prioridad** | Jugador > Objetos siempre | Implementar un sistema de "Interrupción": Si el ruido del objeto es > que el del jugador * x2, distraerse. |

---

### Conclusión del Auditor
La audición actual es **geométricamente ingenua**. Confía demasiado en operaciones de física simples que no representan cómo se propaga el sonido en un entorno complejo de terror. La IA necesita un sistema de **"Prioridad Dinámica"** y una **"Conciencia de Entorno"** que incluya la altura y la proximidad física absoluta.
