@echo off
dotnet build src/Limbo.Umbraco.Migrations --configuration Debug /t:rebuild /t:pack -p:PackageOutputPath=c:/nuget