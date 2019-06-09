#.net core with sql file process
FROM microsoft/dotnet:2.2-sdk AS builder
WORKDIR /app

# Copy project files & build the project
COPY ./aspnetcoreoauth ./aspnetcoreoauth
RUN dotnet build ./aspnetcoreoauth/aspnetcoreoauth.csproj -c Release 

# Copy test project & execute Unit tests 
COPY ./aspnetcoreoauth.Tests ./aspnetcoreoauth.Tests
RUN dotnet build ./aspnetcoreoauth.Tests/aspnetcoreoauth.Tests.csproj 
RUN dotnet test ./aspnetcoreoauth.Tests/aspnetcoreoauth.Tests.csproj 

RUN dotnet publish ./aspnetcoreoauth/aspnetcoreoauth.csproj -c Release -o out --no-restore

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app/aspnetcoreoauth/out .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet aspnetcoreoauth.dll
#ENTRYPOINT ["dotnet", "aspnetcoreoauth.dll"]