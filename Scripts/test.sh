msbuild /p:Configuration=Debug ./KrakenIoc/KrakenIoc.sln 
dotnet vstest ./KrakenIoc/KrakenIoc.Tests/bin/Debug/KrakenIoc.UnitTests.dll /InIsolation /logger:trx