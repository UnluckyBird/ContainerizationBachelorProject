#Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./KubernetesAPI.csproj" --disable-parallel
RUN dotnet publish "./KubernetesAPI.csproj" -c release -o /app --no-restore

#Serve
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app ./

EXPOSE 5000

ENTRYPOINT ["dotnet", "KubernetesAPI.dll"]