# Análisis Exhaustivo de Mejora de Inteligencia Artificial - VITRUVIUS 3.01

Este documento detalla el análisis del estado actual de la IA de los enemigos y propone una serie de mejoras arquitectónicas, sensoriales y tácticas para elevar la experiencia de juego a un nivel premium y competitivo.

---

## 1. Análisis del Estado Actual

Tras revisar los sistemas actuales (`EnemyMonsterAI`, `NemesisAI_Enhanced` y el sistema modular `EnemyBrain`), se han identificado los siguientes puntos:

### Fortalezas:
*   **Modularidad Inicial:** Existe un esfuerzo por separar los sentidos (`EnemySenses`), el movimiento (`EnemyMotor`) y el cerebro (`EnemyBrain`).
*   **Interacción Ambiental:** Capacidad básica de destruir paredes y reaccionar a ruidos de objetos.
*   **Estados de Transición:** Animaciones de despertar y rugir integradas en el flujo lógico.

### Debilidades:
*   **Predecibilidad:** Los enemigos siguen rutas lineales y reaccionan de forma binaria (te veo/no te veo).
*   **Falta de Táctica:** No hay flanqueo, emboscadas ni uso inteligente del entorno (como apagar luces o cerrar puertas).
*   **Sistemas Rígidos:** El uso de Maquinas de Estados Finitas (FSM) extensas dificulta la adición de comportamientos complejos sin romper la lógica existente.
*   **Sentidos Limitados:** La IA "ciega" es interesante, pero carece de matices como la persistencia de olor o la reacción a la luz (linternas).

---

## 2. Propuesta de Arquitectura: Behavior Trees (Árboles de Comportamiento)

Se recomienda migrar de Maquinas de Estados Finitas (FSM) a **Behavior Trees**.

> [!NOTE]
> Los Árboles de Comportamiento permiten ramificar la lógica de forma jerárquica, facilitando comportamientos como "Investigar -> Si no encuentro nada -> Buscar sistemáticamente en la zona" sin crear un código espagueti.

### Ventajas:
*   **Priorización Dinámica:** Puede abortar una búsqueda si escucha un ruido más fuerte o detecta una amenaza mayor.
*   **Reutilización:** Nodos de "Huir", "Flanquear" o "Acechar" se pueden compartir entre diferentes tipos de enemigos.

---

## 3. Mejora del Sistema Sensorial (Percepción Progresiva)

En lugar de una detección instantánea, se propone un sistema de **Medidores de Alerta**.

### A. Visión Dinámica
*   **Detección de Luz:** El enemigo debería detectar el haz de la linterna del jugador incluso si no ve al jugador directamente.
*   **Visibilidad por Sombras:** El tiempo de detección debe variar según si el jugador está en la luz o en la penumbra.

### B. Audio Avanzado
*   **Tipos de Estímulos:** Diferenciar entre sonidos accidentales (tirar un objeto), sonidos rítmicos (pasos) y sonidos de amenaza (disparos).
*   **Persistencia Acústica:** El enemigo debe "recordar" la dirección de donde vino el sonido y proyectar una zona de búsqueda, no solo un punto exacto.

### C. Rastro y Olfato (Hunting Mode)
*   **Scent Trails:** Si el jugador está herido, deja un rastro que el enemigo puede seguir incluso sin verlo ni oírlo.

---

## 4. Tácticas y Combate Inteligente

Para que el enemigo se sienta "inteligente", debe dejar de caminar en línea recta hacia el jugador.

### A. Movimiento de Intercepción y Flanqueo
*   **Predicción de Posición:** Calcular hacia dónde va el jugador y "cortarle el paso" en lugar de seguir su rastro.
*   **Flanqueo:** Si hay dos enemigos, uno debe atacar de frente mientras el otro busca una ruta lateral para rodear al jugador.

### B. Uso del Entorno
*   **Interacción con Puertas:** El enemigo no solo debe romper paredes, sino poder abrir puertas (o derribarlas violentamente si están cerradas con llave).
*   **Hiding Spots:** El enemigo podría "fingir" que se ha ido, escondiéndose en una habitación cercana para emboscar al jugador cuando este salga de su escondite.

---

## 5. IA de Grupo (Coordinación Social)

Si hay múltiples enemigos, deben actuar como una jauría, no como individuos aislados.

*   **Llamada de Alerta:** Si un enemigo encuentra al jugador, emite un rugido que alerta a los demás en un radio amplio.
*   **Compartición de Información:** La "Last Known Position" (LKP) se sincroniza entre los enemigos cercanos.
*   **Roles de Caza:** Un enemigo puede actuar como "Ojeador" (manteniendo distancia y observando) mientras otro actúa como "Ataque" (presionando cuerpo a cuerpo).

---

## 6. Lógica de "Caza Sistemática" (Search Logic)

Cuando el jugador se esconde, la IA actual se rinde muy rápido. Se propone un algoritmo de **Búsqueda por Rejilla**:

1.  **Zona de Interés:** El enemigo define un radio alrededor de la última posición vista del jugador.
2.  **Puntos de Escondite:** La IA escanea objetos `Hideable` o esquinas oscuras dentro de esa zona.
3.  **Investigación Aleatoria Dinámica:** Camina hacia esos puntos con animaciones de "inspección" (mirar debajo de camas, tras armarios, etc.).

---

## 7. Plan de Implementación Sugerido

| Fase | Tarea | Impacto |
| :--- | :--- | :--- |
| **Fase 1** | Implementar Medidores de Alerta (Visual/Audio) | Inmediato (Mejor sigilo) |
| **Fase 2** | Integrar Behavior Trees para la lógica de búsqueda | Alta complejidad (Mejor flujo de combate) |
| **Fase 3** | Añadir comportamientos de flanqueo e intercepción | Experiencia Premium (Combate desafiante) |
| **Fase 4** | Coordinación de grupo y roles de IA | Finalización (Inmersión total) |

---

### Conclusión
Mejorar la inteligencia enemiga no se trata solo de hacer que el enemigo sea más rápido o fuerte, sino de hacerlo **impredecible**. Al implementar sistemas de percepción basados en medidores y tácticas de movimiento no lineales, *VITRUVIUS 3.01* alcanzará un nivel de inmersión y terror psicológico superior.
