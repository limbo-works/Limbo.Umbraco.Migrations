@echo off
dotnet build src/Limbo.Umbraco.Migrations --configuration Release /t:rebuild /t:pack -p:PackageOutputPath=../../releases/nuget