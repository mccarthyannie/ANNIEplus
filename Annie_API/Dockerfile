# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Annie_API/Annie_API.csproj", "Annie_API/"]
COPY ["Annie_Shared/Annie_Shared.csproj", "Annie_Shared/"]

RUN dotnet restore "Annie_API/Annie_API.csproj"

COPY . . 
WORKDIR "/src/Annie_API"
RUN dotnet publish -c Release -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update \
	&& apt-get install -y libkrb5-3 \
	&& rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Annie_API.dll"]