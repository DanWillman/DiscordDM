FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

COPY ./src/*.csproj ./
RUN dotnet restore 

COPY . ./
RUN dotnet publish -c Release -o publishdir

WORKDIR /app/ytdl
RUN curl -L https://yt-dl.org/downloads/latest/youtube-dl -o /usr/local/bin/youtube-dl
RUN chmod a+rx /usr/local/bin/youtube-dl

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app/publishdir .
ENTRYPOINT ["dotnet", "DiscordDM.dll"]