FROM microsoft/aspnetcore-build:2.0.0 AS build

WORKDIR /code
COPY . .
RUN dotnet restore
RUN dotnet publish --output /output --configuration Release

FROM microsoft/aspnetcore:2.0.0
EXPOSE 5000
COPY --from=build /output /app
COPY Images /app/Images
WORKDIR /app
ENTRYPOINT [ "dotnet", "photo-api.dll"]