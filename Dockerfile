FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Banking_IVR.csproj ./
RUN dotnet restore Banking_IVR.csproj

COPY . ./
RUN dotnet publish Banking_IVR.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "Banking_IVR.dll"]
