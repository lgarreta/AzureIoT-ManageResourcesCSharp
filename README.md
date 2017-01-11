# Manejo de recursos a través de programación (Visual Studio C#, PowerShell)
El repositorio contiene un archivo PDF con el reporte y tres directorios: 
- El primero con el script de autenticación en PowerShell, el cual al ejecutarse realiza el ingreso a su cuenta en Azure y con esta crea los IDs para después usarlos para autenticarse de forma segura y automática en aplicaciones.
- El segundo con el proyecto en VisualStudio C# con una aplicación donde usa los IDs anteriores para obtener un token de acceso y usarlo para crear de forma automática un grupo de recursos en Azure. El proyecto no incluye los paquetes de la solución y estos los debería instalar en su máquina a través del manejador de paquetes Nuget.
- El tercer con los archivos del template para crear el IoT Hub