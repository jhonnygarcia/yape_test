# Challenge API

## Requisitos
Antes de iniciar, asegúrate de tener instalado:
- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
- [.NET SDK](https://dotnet.microsoft.com/en-us/download)

## Configuración e instalación

1. Levantar los contenedores de Kafka y PostgreSQL con Docker Compose:

   ```sh
   docker-compose up -d
   ```

2. Aplicar las migraciones de la base de datos ejecutando el siguiente comando dentro del proyecto:

   ```sh
   dotnet ef database update --project Application --startup-project WebApi
   ```

   > **Nota:** Este comando creará la base de datos e insertará dos cuentas iniciales con un saldo de 100,000 cada una.

## Endpoints disponibles
La aplicación expone tres endpoints según la especificación OpenAPI 3.0:

### **Account**
- **GET** `/api/v1/accounts`
  - Obtiene la lista de cuentas registradas en la base de datos.

### **Transaction**
- **GET** `/api/v1/transactions`
  - Obtiene la lista de transacciones registradas.
- **POST** `/api/v1/transactions`
  - Publica un mensaje en Kafka para registrar una nueva transacción.
  - Luego, otro proceso validará la transacción y actualizará su estado si cumple con las validaciones establecidas.

## Notas
- Es importante asegurarse de que el entorno de Kafka y PostgreSQL estén correctamente levantados antes de ejecutar la aplicación.
- La API está diseñada para procesar transacciones de manera asíncrona mediante Kafka, garantizando integridad y escalabilidad.

