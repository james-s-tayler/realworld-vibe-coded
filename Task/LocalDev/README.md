# Local Development Environment

This directory contains Docker configuration for local development with the Conduit application.

## Prerequisites

- Docker Desktop
- JetBrains Rider (or Visual Studio with Docker support)

## Running the Application

There are two ways to run the local development environment:

### Option 1: Hot-Reload Development (Recommended for Active Development)

```bash
# From the repository root
./build.sh RunLocalServer
```

This will start:
- SQL Server database container
- .NET backend API container with hot-reload enabled

The API will be available at `http://localhost:5000`

### Option 2: Published Artifact (Production-like Environment)

```bash
# From the repository root
./build.sh RunLocalPublish
```

This will:
- Build and publish the application in Release configuration
- Start SQL Server database container
- Run the published artifact in a production-like container

The API will be available at:
- `http://localhost:5000` (HTTP)
- `https://localhost:5001` (HTTPS)

**Note:** This mode is useful for testing the published artifact and debugging production-like scenarios.

## Remote Debugging with JetBrains Rider

Both development modes (hot-reload and published artifact) include vsdbg (Visual Studio Debugger) for remote debugging support.

### Debugging with Hot-Reload (RunLocalServer)

1. Start the local development environment:
   ```bash
   ./build.sh RunLocalServer
   ```

2. In Rider, go to **Run → Attach to Process...**

3. In the dialog:
   - Click the **Connect to remote debugger** button (connection icon)
   - Select **Docker** as the connection type
   - Select the running `api` container from the list
   
4. After connecting:
   - Find the `dotnet` process in the process list
   - Click **Attach with .NET Core Debugger**

5. Set breakpoints in your code and debug as usual

### Debugging with Published Artifact (RunLocalPublish)

1. Start the published artifact environment:
   ```bash
   ./build.sh RunLocalPublish
   ```

2. In Rider, go to **Run → Attach to Process...**

3. In the dialog:
   - Click the **Connect to remote debugger** button (connection icon)
   - Select **Docker** as the connection type
   - Select the running `api` container from the list
   
4. After connecting:
   - Find the `Server.Web` process in the process list (the process name may appear as the executable name)
   - Click **Attach with .NET Core Debugger**

5. Set breakpoints in your code and debug as usual

**Note:** When debugging the published artifact, you're debugging optimized Release code. This is useful for troubleshooting production-like issues but may have different behavior than Debug builds.

### Alternative: Using Docker Connection String

You can also configure a persistent remote debugging configuration:

1. Go to **Run → Edit Configurations...**
2. Click **+** and select **Attach to Process**
3. Configure:
   - Name: `Attach to LocalDev Container` (or `Attach to Published Container`)
   - Connection type: `Docker`
   - Container: Select the `api` container
   - Search for process by name: `dotnet` (for hot-reload) or `Server.Web` (for published)
4. Click **OK** to save

Now you can quickly attach by selecting this configuration from the run menu.

## Troubleshooting

### Container fails to start
- Ensure Docker Desktop is running
- Check that ports 5000 and 5001 are not already in use
- Verify SQL Server container is healthy: `docker ps`
- For RunLocalPublish, ensure the publish directory was created: check `publish/` directory exists

### Cannot attach debugger
- Verify vsdbg is installed in the container:
  ```bash
  # For hot-reload container
  docker exec -it localdev-api-1 ls -la /vsdbg
  ```
- Ensure the container is running with the dotnet process:
  ```bash
  docker ps
  docker exec -it localdev-api-1 ps aux | grep -E 'dotnet|Server.Web'
  ```
- Check Rider's Docker integration is properly configured
- For published artifact debugging, the process name is `Server.Web` (not `dotnet`)

### Hot-reload not working
- Ensure source code is properly mounted as a volume in docker-compose.yml
- Check container logs: `docker logs localdev-api-1`
- Hot-reload only works with RunLocalServer (not RunLocalPublish)

### Published artifact issues
- Ensure the build succeeded: check `publish/` directory for Server.Web executable
- Check container logs: `docker logs localdev-api-1`
- Verify the HTTPS certificate is accessible in the container:
  ```bash
  docker exec -it localdev-api-1 ls -la /https/aspnetapp.pfx
  ```

## Additional Resources

- [vsdbg GitHub Repository](https://github.com/microsoft/vsdbg)
- [Rider Docker Integration](https://www.jetbrains.com/help/rider/Docker.html)
- [Remote Debugging in Rider](https://www.jetbrains.com/help/rider/Remote_Debugging.html)
