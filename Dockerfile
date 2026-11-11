FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["JobProcessing.csproj", "./"]
RUN dotnet restore "JobProcessing.csproj"

COPY . .
RUN dotnet publish "JobProcessing.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app 

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "JobProcessing.dll"]