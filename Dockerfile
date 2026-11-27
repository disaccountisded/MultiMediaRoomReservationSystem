FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["MultiMediaRoomReservationSystem.csproj", "./"]
RUN dotnet restore "MultiMediaRoomReservationSystem.csproj"

COPY . .
RUN dotnet publish "MultiMediaRoomReservationSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080
ENTRYPOINT ["dotnet", "MultiMediaRoomReservationSystem.dll"]
