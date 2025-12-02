# Documentación del Sistema E-Commerce

## 1. Visión General del Sistema

Este sistema es una plataforma de comercio electrónico desarrollada con **ASP.NET Core MVC**. La aplicación sigue una arquitectura Modelo-Vista-Controlador (MVC) y gestiona tres roles principales de usuarios: **Administrador**, **Vendedor** y **Cliente**.

### Tecnologías Clave

- **Framework**: ASP.NET Core MVC
- **Lenguaje**: C#
- **Base de Datos**: Entity Framework Core (implícito por el uso de `IUserService` y modelos)
- **Autenticación**: Cookie Authentication con roles (Claims)
- **Seguridad**: BCrypt para hashing de contraseñas, protección CSRF.

---

## 2. Roles y Funcionalidades

### 2.1 Administrador (Admin)

El administrador tiene control total sobre la plataforma.

- **Dashboard**: Vista general con métricas clave (Total de productos, órdenes, ingresos, usuarios).
- **Gestión de Productos**: Listar, filtrar y eliminar productos de cualquier vendedor.
- **Gestión de Órdenes**: Ver todas las órdenes, filtrar por estado o búsqueda.
- **Gestión de Usuarios**:
  - Listar todos los usuarios.
  - Crear nuevos usuarios.
  - Cambiar roles de usuarios.
  - Eliminar usuarios.
- **Gestión de Categorías**: Crear, editar y eliminar categorías de productos.
- **Reportes**: Generar reportes de ventas por rango de fechas.

### 2.2 Vendedor (Vendor)

Los vendedores gestionan su propio inventario y ventas.

- **Dashboard**: Métricas específicas del vendedor (Stock total, valor de inventario, productos con bajo stock).
- **Gestión de Productos**:
  - Crear nuevos productos.
  - Editar productos existentes (precio, stock, descripción, etc.).
  - Eliminar productos propios.
  - Aplicar descuentos.
- **Gestión de Órdenes**: Ver órdenes que contienen sus productos y actualizar el estado de las mismas (ej. de "Pending" a "Shipped").
- **Reseñas**: Ver reseñas dejadas por clientes en sus productos.
- **Reportes de Ventas**: Ver desempeño de ventas de sus propios productos.

### 2.3 Cliente (Customer)

Los usuarios finales que compran en la plataforma.

- **Navegación**:
  - Página de inicio con productos destacados.
  - Catálogo de productos con filtros (precio, categoría, metal, búsqueda).
  - Detalles de producto con reseñas y productos relacionados.
- **Carrito de Compras**: Agregar/quitar productos, actualizar cantidades.
- **Checkout**: Proceso de compra con selección de dirección de envío y método de pago.
- **Historial de Órdenes**: Ver estado de órdenes pasadas y detalles.
- **Cancelación**: Posibilidad de cancelar órdenes pendientes (restaurando stock automáticamente).

---

## 3. Estructura de Vistas (Views)

La interfaz de usuario está organizada en carpetas correspondientes a los controladores:

### 3.1 Vistas Públicas / Auth (`/Views/Auth`)

- `Login`: Formulario de inicio de sesión.
- `Register`: Formulario de registro de nuevos usuarios (Cliente o Vendedor).
- `AccessDenied`: Página de error de permisos.

### 3.2 Vistas de Administrador (`/Views/Admin`)

- `Dashboard`: Panel principal.
- `Products`: Tabla de gestión de productos.
- `Orders`: Tabla de gestión de órdenes.
- `Users`: Tabla de gestión de usuarios.
- `Categories`: Gestión de categorías.
- `Reports`: Visualización de reportes de ventas.

### 3.3 Vistas de Vendedor (`/Views/Vendor`)

- `Dashboard`: Panel principal del vendedor.
- `MyProducts`: Listado de inventario propio.
- `CreateProduct` / `EditProduct`: Formularios de gestión de productos.
- `MyOrders`: Listado de órdenes relacionadas al vendedor.
- `Reviews`: Listado de reseñas de sus productos.
- `Reports`: Reportes de ventas del vendedor.

### 3.4 Vistas de Cliente (`/Views/Customer`)

- `Home`: Página principal.
- `Products`: Catálogo con filtros.
- `ProductDetails`: Vista detallada de un producto.
- `Cart`: Vista del carrito de compras.
- `Checkout`: Proceso de pago y envío.
- `MyOrders`: Historial de compras del usuario.
- `OrderDetails`: Detalle de una orden específica.

---

## 4. Flujos Principales

### 4.1 Flujo de Compra

1. Cliente navega y agrega productos al **Carrito**.
2. Procede al **Checkout**.
3. Selecciona **Dirección de Envío**.
4. Confirma pago.
5. Se crea la **Orden** (Estado: Pending) y se descuenta el **Stock**.

### 4.2 Flujo de Gestión de Orden (Vendedor)

1. Vendedor recibe notificación (visual) en **MyOrders**.
2. Revisa los detalles de la orden.
3. Prepara el paquete y actualiza el estado a **Shipped**.
4. Finalmente actualiza a **Delivered**.

### 4.3 Flujo de Cancelación

1. Cliente cancela una orden en estado **Pending**.
2. El sistema cambia el estado a **Cancelled**.
3. El sistema **restaura automáticamente el stock** de los productos involucrados.
