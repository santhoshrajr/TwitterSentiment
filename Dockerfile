FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["TwitterSentiment.csproj", ""]
RUN dotnet restore "./TwitterSentiment.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TwitterSentiment.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TwitterSentiment.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TwitterSentiment.dll"]