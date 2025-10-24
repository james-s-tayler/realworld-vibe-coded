# Local Development Environment

This directory contains Docker configuration for local development with the Conduit application.

## Prerequisites

- Docker Desktop
- JetBrains Rider (or Visual Studio with Docker support)

## Running the Application

To start the local development environment:

```bash
# From the repository root
./build.sh run-local-server
```

This will start:
- SQL Server database container
- .NET backend API container with hot-reload enabled

The API will be available at `http://localhost:5000`

## Remote Debugging with JetBrains Rider

The development container includes vsdbg (Visual Studio Debugger) for remote debugging support.

### Attaching the Debugger

1. Start the local development environment:
   ```bash
   ./build.sh run-local-server
   ```

2. In Rider, go to **Run → Attach to Process...**

3. In the dialog:
   - Click the **Connect to remote debugger** button (connection icon)
   - Select **SSH** or **Docker** as the connection type
   - For Docker:
     - Connection type: `Docker`
     - Select the running `api` container from the list
   
4. After connecting:
   - Find the `dotnet` process in the process list
   - Click **Attach with .NET Core Debugger**

5. Set breakpoints in your code and debug as usual

### Alternative: Using Docker Connection String

You can also configure a persistent remote debugging configuration:

1. Go to **Run → Edit Configurations...**
2. Click **+** and select **Attach to Process**
3. Configure:
   - Name: `Attach to LocalDev Container`
   - Connection type: `Docker`
   - Container: Select the `api` container
   - Search for process by name: `dotnet`
4. Click **OK** to save

Now you can quickly attach by selecting this configuration from the run menu.

## Troubleshooting

### Container fails to start
- Ensure Docker Desktop is running
- Check that port 5000 is not already in use
- Verify SQL Server container is healthy: `docker ps`

### Cannot attach debugger
- Verify vsdbg is installed in the container:
  ```bash
  docker exec -it localdev-api-1 ls -la /vsdbg
  ```
- Ensure the container is running with the dotnet process
- Check Rider's Docker integration is properly configured

### Hot-reload not working
- Ensure source code is properly mounted as a volume in docker-compose.yml
- Check container logs: `docker logs localdev-api-1`

## Additional Resources

- [vsdbg GitHub Repository](https://github.com/microsoft/vsdbg)
- [Rider Docker Integration](https://www.jetbrains.com/help/rider/Docker.html)
- [Remote Debugging in Rider](https://www.jetbrains.com/help/rider/Remote_Debugging.html)
