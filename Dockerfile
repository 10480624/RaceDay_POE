# Build and publish the ASP.NET Core MVC application.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RaceDay.Web/RaceDay.Web.csproj", "RaceDay.Web/"]
RUN dotnet restore "RaceDay.Web/RaceDay.Web.csproj"

COPY . .
WORKDIR /src/RaceDay.Web
RUN dotnet publish "RaceDay.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Run the published application using the smaller ASP.NET runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]
