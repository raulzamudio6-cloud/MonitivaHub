FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY Hub.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish "Hub.csproj" -r linux-x64 -p:PublishSingleFile=true -c Release -o /app/publish --self-contained true
FROM mcr.microsoft.com/dotnet/runtime-deps:5.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN chmod +x ./Hub
EXPOSE 5002
RUN addgroup --system dotnetgroup && adduser --system --ingroup dotnetgroup dotnetuser
USER dotnetuser
ENTRYPOINT ["./Hub"]
