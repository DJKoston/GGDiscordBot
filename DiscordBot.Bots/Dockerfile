#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DiscordBot.Bots/DiscordBot.Bots.csproj", "DiscordBot.Bots/"]
COPY ["DiscordBot.Core/DiscordBot.Core.csproj", "DiscordBot.Core/"]
COPY ["DiscordBot.DAL/DiscordBot.DAL.csproj", "DiscordBot.DAL/"]
COPY ["DiscordBot.DAL.Migrations/DiscordBot.DAL.Migrations.csproj", "DiscordBot.DAL.Migrations/"]
RUN dotnet restore "DiscordBot.Bots/DiscordBot.Bots.csproj"
COPY . .
WORKDIR "/src/DiscordBot.Bots"
RUN dotnet build "DiscordBot.Bots.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiscordBot.Bots.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiscordBot.dll"]