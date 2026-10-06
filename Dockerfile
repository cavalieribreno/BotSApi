FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /source
COPY BotSaaS.Api.csproj ./
RUN dotnet restore
COPY . .
EXPOSE 5069
CMD ["dotnet", "run"]
