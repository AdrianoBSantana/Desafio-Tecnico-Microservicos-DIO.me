FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# copy entire repository into container (docker-compose sets build context to repo root)
COPY . ./
# restore and publish the API project from its folder
RUN dotnet restore "src/Estoque.API/Estoque.API.csproj"
RUN dotnet publish "src/Estoque.API/Estoque.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Estoque.API.dll"]
