# Sistema de Inventario en C#

Este es un sistema de inventario básico desarrollado en C# utilizando .NET Framework y Windows Forms. El sistema permite gestionar usuarios, categorías de productos, productos, ventas y compras, así como generar reportes de ventas y compras.

## Características

- **Registro de Usuarios**: Permite agregar, editar y eliminar usuarios.
- **Categorías de Productos**: Gestión de categorías de productos.
- **Productos**: Registro y gestión de productos.
- **Registro de Ventas y Compras**: Permite registrar ventas y compras.
- **Detalle de Compras y Ventas**: Visualización de detalles de cada transacción.
- **Reportes**: Generación de reportes de ventas y compras.

## Tecnologías Utilizadas

- **Lenguaje**: C#
- **Framework**: .NET Framework
- **Interfaz de Usuario**: Windows Forms
- **Base de Datos**: SQL Server

## Estructura del Proyecto

El proyecto sigue una estructura de una sola capa, con toda la lógica de negocio y la interfaz de usuario en la capa `CapaPresentacion`.

## Instalación

1. Clona el repositorio:
    ```bash
    git clone https://github.com/AdanHernandez2/inventory.git
    ```

2. Abre el proyecto en Visual Studio.

3. Configura la cadena de conexión a la base de datos directamente en el código. Por ejemplo:
    ```csharp
    string connectionString = "Data Source=tu-servidor;Initial Catalog=tu-base-de-datos;Integrated Security=True";
    ```

4. Restaura las dependencias y construye el proyecto.

## Uso

1. Ejecuta la aplicación desde Visual Studio.
2. Utiliza las diferentes secciones del sistema para gestionar usuarios, productos, categorías, ventas y compras.
3. Genera reportes desde la sección de reportes.

## Contribuciones

Las contribuciones son bienvenidas. Si deseas contribuir, por favor sigue estos pasos:

1. Haz un fork del repositorio.
2. Crea una nueva rama (`git checkout -b feature/nueva-caracteristica`).
3. Realiza tus cambios y haz commit (`git commit -am 'Añadir nueva característica'`).
4. Sube tus cambios (`git push origin feature/nueva-caracteristica`).
5. Abre un Pull Request.

## Licencia

Este proyecto está bajo la Licencia MIT. Consulta el archivo LICENSE para más detalles.

#

---

¡Gracias por pasar a ver este repositorio, estoy en modo aprendizaje con c#! Espero que te sea de utilidad.
