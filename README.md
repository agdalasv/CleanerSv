<<<<<<< HEAD
# CleanerSv
Cleaner Sv es una herramienta de optimización para Windows 11 desarrollada en WPF/.NET 8 que permite limpiar archivos basura, optimizar RAM, reparar el registro, desfragmentar discos, monitorear CPU/RAM/disco/temperatura, gestionar aplicaciones y crear backups del sistema, todo con una interfaz Fluent Design con modo oscuro/claro.
=======
# Cleaner Sv

**Cleaner Sv** es una aplicación nativa para Windows 11 (64-bit) diseñada para optimizar, limpiar, reparar y monitorear el rendimiento del sistema operativo. Desarrollada con **WPF (.NET 8)** y una interfaz visual moderna inspirada en **Fluent Design**.

![Dashboard](Cleaner/1.png)

---

## Características Principales

### 🗑️ Limpieza de Archivos Basura
- Archivos temporales de Windows
- Caché del sistema y DNS
- Archivos de log y miniaturas
- Archivos de actualización antiguos
- Papelera de reciclaje
- Caché de navegadores y aplicaciones
- Archivos duplicados y descargas antiguas
- Vista previa antes de eliminar con nivel de riesgo

### 📋 Limpieza y Reparación del Registro
- Detección de entradas inválidas y claves rotas
- Eliminación de registros de aplicaciones desinstaladas
- Limpieza de rutas inexistentes
- Reparación de asociaciones de archivos
- Detección de DLL faltantes
- Backup automático y restauración del registro

### 💾 Optimizador de RAM
- Liberación inteligente de memoria
- Monitoreo en tiempo real con gráfico de consumo
- Modo Gaming para máximo rendimiento
- Limpieza automática programada
- Detección de las 10 aplicaciones más pesadas

### 💿 Desfragmentador de Disco
- Mapa gráfico animado de bloques del disco
- Optimización especial para HDD
- Detección automática de SSD (TRIM en lugar de desfragmentar)
- Estado de fragmentación y salud del disco

### 🔍 Chequeo y Reparación de Disco
- Escaneo de sectores dañados
- Diagnóstico SMART
- Verificación de integridad y reparación lógica
- Monitoreo de temperatura
- Estado de vida útil del SSD

### ⚡ Optimización del Sistema
- Gestión de programas de inicio
- Desactivación de servicios innecesarios
- Modo Turbo y optimización para gaming
- Optimización de red

### 📈 Monitor del Sistema en Tiempo Real
- Uso de CPU, RAM, GPU y Disco
- Temperatura del sistema
- Velocidad de red
- Lista de procesos activos con consumo
- Estado de batería (laptops)

### 📦 Gestor de Aplicaciones
- Desinstalador avanzado con eliminación de residuos
- Escaneo de aplicaciones instaladas
- Vista detallada con tamaño, versión y editor

### 🔐 Carpeta Segura
- Protege carpetas con contraseña (mínimo 6 caracteres)
- Hash SHA256 para almacenamiento seguro
- Carpeta oculta en Documentos con atributos del sistema
- Bloqueo y desbloqueo rápido

### 🛡️ Backup y Restauración
- Puntos de restauración automáticos
- Backup y restauración del registro
- Historial de cambios
- Protección ante errores críticos

---

## Tecnologías Utilizadas

| Tecnología | Propósito |
|---|---|
| **C# / .NET 8** | Lenguaje y framework principal |
| **WPF (Windows Presentation Foundation)** | Interfaz de usuario |
| **WinUI / Fluent Design** | Inspiración de diseño visual |
| **LiveChartsCore / SkiaSharp** | Gráficos interactivos en tiempo real |
| **Windows API / WMI** | Acceso a información del sistema |
| **WindowsAPICodePack** | Integración con shell de Windows |
| **WiX Toolset v4** | Instalador MSI profesional |

---

## Requisitos del Sistema

- **Sistema Operativo:** Windows 11 64-bit
- **Runtime:** .NET 8 Desktop Runtime
- **Memoria:** 4 GB RAM (mínimo)
- **Espacio:** 50 MB de espacio libre

---

## Instalación

1. Descarga el instalador desde [Releases](https://github.com/agdalasv/CleanerSv/releases)
2. Ejecuta `CleanerSv.msi`
3. Sigue las instrucciones del asistente de instalación
4. La aplicación se iniciará automáticamente al finalizar

También puedes descargar el ejecutable portable desde la sección de Releases.

---

## Capturas de Pantalla

| Dashboard | Limpieza | Registro |
|---|---|---|
| ![Dashboard](Cleaner/1.png) | ![Limpieza](Cleaner/2.png) | ![Registro](Cleaner/3.png) |

---

## Desarrollo

```bash
# Clonar el repositorio
git clone https://github.com/agdalasv/CleanerSv.git

# Restaurar dependencias
cd CleanerSv/Cleaner
dotnet restore

# Compilar
dotnet build -c Release

# Generar instalador
cd ../Cleaner.Installer
.\build.ps1
```

---

## Licencia

Este proyecto está bajo la licencia MIT. Consulta el archivo `LICENSE` para más detalles.

---

## Contacto

**Agdala**  
📧 agdala.sv@gmail.com  
🌐 [github.com/agdalasv](https://github.com/agdalasv)
>>>>>>> 2f7704c (Initial commit: Cleaner Sv full app + docs site)
