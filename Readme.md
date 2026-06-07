# OpenTK Render Engine

Este repositorio contiene un motor gráfico básico y funcional desarrollado desde cero en C# utilizando OpenGL. El proyecto nace como un Trabajo de Fin de Grado con el objetivo de comprender e implementar los fundamentos de la programación gráfica y la simulación física de bajo nivel, sin depender de la abstracción o sistemas preestablecidos de motores comerciales como Unity o Unreal Engine.

## 🚀 Características Principales

El motor cuenta con un pipeline de renderizado y simulación física completo, gestionando íntegramente el ciclo de vida de una escena tridimensional.

### Renderizado e Iluminación
* **Modelo de Iluminación Avanzado:** Implementación del modelo Blinn-Phong para lograr brillos especulares suaves y realistas.
* **Múltiples Fuentes de Luz:** Soporte para calcular en tiempo real luces direccionales, puntuales (con atenuación) y focales (spotlights).
* **Mapas y Texturas:** Carga de texturas base, mapas especulares para gestionar el nivel de reflejo y mapas de normales para simular microrrelieve sin añadir carga geométrica.
* **Shadow Mapping:** Sistema dinámico de dibujado de sombras direccionales, puntuales (utilizando *cubemaps*) y focales. Incluye correcciones mediante *Shadow Bias* para evitar el *Shadow Acne* y filtros PCF para suavizar los bordes.
* **Corrección Gamma:** Integrada en el *Fragment Shader* para lograr gradientes y sombras físicamente más precisas en los monitores.
* **Modelos 3D:** Carga e importación de modelos complejos (.obj, .fbx) junto a sus mapeados UV.

### Física Integrada
* **Simulación de Cuerpos Rígidos:** Aplicación de velocidad, rozamiento, y movimiento bajo fuerzas gravitatorias.
* **Detección de Colisiones Complexas:** Soporte para colisiones de cajas orientadas (OBB), primitivas (esferas, cápsulas) y colisionadores de malla (Mesh Colliders) tanto convexos como cóncavos.
* **Dinámica Rotacional:** Simulación realista de inercia y momento angular derivado de impactos físicos.
* **Materiales Físicos:** Sistema modular para asignar propiedades de fricción (rozamiento) y restitución (rebote) de forma independiente a cada objeto.

### Arquitectura y Utilidades
* **Entity Component System (ECS):** Arquitectura de software modular basada en entidades (GameObjects) y componentes independientes (Transform, Renderer, Rigidbody, Collider, Light, etc.).
* **Serialización de Escenas:** Sistema de guardado y carga del estado del entorno en archivos `.json` para persistencia de datos y transiciones fluidas.
* **Generación de Primitivas y Patrón Builder:** Capacidad para instanciar formas 3D nativas fácilmente, utilizando llamadas encadenadas para un código más legible.
* **Gestión de Memoria:** Prevención automática de fugas de memoria limpiando punteros de recursos en C++ directamente en la API gráfica.

---

## 🛠️ Tecnologías y Librerías Utilizadas

Para el desarrollo del proyecto se han empleado herramientas estandarizadas y de código abierto:

* **Lenguaje Principal:** C#
* **API Gráfica:** OpenGL mediante el wrapper OpenTK (que incorpora GLFW para ventanas/input y GLAD).
* **Físicas:** Bullet Physics SDK.
* **Carga de Modelos:** AssimpNET.
* **Manejo de Imágenes/Texturas:** StbImageSharp.
* **Serialización de Datos:** Newtonsoft.Json.
* **Entorno de Desarrollo:** Visual Studio Community 2022.

---

## 🎮 Controles y Uso en Tiempo de Ejecución

El motor cuenta con atajos preconfigurados para facilitar la depuración, edición y navegación durante la ejecución (Runtime):

* **Cámara Libre:** Navegación utilizando input periférico de ratón y teclado.
* **Control del Tiempo:**
  * Pulsa `P` para pausar la simulación y liberar el ratón.
  * Usa `Alt + →` y `Alt + ←` para acelerar o ralentizar la velocidad del tiempo global.
* **Gestión de Escenas:**
  * Existen 9 ranuras (slots) seleccionables desde el panel numérico.
  * `Ctrl + S`: Guarda la escena actual en el slot activo.
  * `Ctrl + L`: Carga la escena del slot activo.
  * `Ctrl + R`: Recarga el estado inicial de la escena actual.
* **Visualización de Variables:** `G` y `H` para conmutar ciertos parámetros del renderizado para inspección.

---

## 👨‍💻 Autor y Contexto Académico

* **Autor:** Álvaro Morales Cruz
* **Institución:** Florida Universitària / Universitat Politècnica de València (UPV)
* **Proyecto:** Trabajo de Fin de Grado en Diseño y Desarrollo de Videojuegos y Experiencias Interactivas (Junio 2026)
* **Tutor:** Roberto Sanz Requena
