# Multi-stage build for the EMS API (net8.0)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore layers cache-friendly: solution + csproj files first
COPY EmployeeManagementSys.API.sln ./
COPY EmployeeManagementSys.API/EmployeeManagementSys.API.csproj EmployeeManagementSys.API/
COPY EmployeeManagementSys.BL/EmployeeManagementSys.BL.csproj EmployeeManagementSys.BL/
COPY EmployeeManagementSys.DL/EmployeeManagementSys.DL.csproj EmployeeManagementSys.DL/
RUN dotnet restore EmployeeManagementSys.API.sln

COPY . .
RUN dotnet publish EmployeeManagementSys.API/EmployeeManagementSys.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EmployeeManagementSys.API.dll"]
