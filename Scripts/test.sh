msbuild /p:Configuration=Debug ./KrakenIoc/KrakenIoc.sln 
dotnet vstest ./KrakenIoc/KrakenIoc.Testing/bin/Debug/KrakenIoc.Testing.dll /InIsolation /logger:trx