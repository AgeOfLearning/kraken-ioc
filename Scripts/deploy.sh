# KrakenIoc
cd ./KrakenIoc/KrakenIoc/
uget build -p ./KrakenIoc.csproj --debug
uget create -p ./KrakenIoc.csproj --debug
uget pack -p ./KrakenIoc.csproj --debug
uget push -p ./KrakenIoc.csproj --debug

# KrakenIoc.Editor
cd ../KrakenIoc.Editor/
uget build -p ./KrakenIoc.Editor.csproj --debug
uget create -p ./KrakenIoc.Editor.csproj --debug
uget pack -p ./KrakenIoc.Editor.csproj --debug
uget push -p ./KrakenIoc.Editor.csproj --debug

# KrakenIoc.Unity
cd ../KrakenIoc.Unity/
uget build -p ./KrakenIoc.Unity.csproj --debug
uget create -p ./KrakenIoc.Unity.csproj --debug
uget pack -p ./KrakenIoc.Unity.csproj --debug
uget push -p ./KrakenIoc.Unity.csproj --debug