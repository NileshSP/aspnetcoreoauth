#.net core with sql file process
FROM microsoft/dotnet:2.2-sdk AS builder
WORKDIR /app

# Copy project files & build the project
COPY ./sampleaspnet/*.csproj ./
#RUN dotnet restore sampleaspnet.csproj
COPY ./sampleaspnet ./
RUN dotnet build sampleaspnet.csproj -c Release 
#--no-restore

# Copy test project & execute Unit tests 
COPY ./sampleaspnet.Tests ./sampleaspnet.Tests
RUN dotnet build ./sampleaspnet.Tests/sampleaspnet.Tests.csproj 
RUN dotnet test ./sampleaspnet.Tests/sampleaspnet.Tests.csproj 

RUN dotnet publish sampleaspnet.csproj -c Release -o out --no-restore

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app/out .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet sampleaspnet.dll
#ENTRYPOINT ["dotnet", "sampleaspnet.dll"]