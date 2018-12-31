
krakeniocRE="kraken-ioc-([0-9\.]+)"
krakenioceditorRE="kraken-ioc-editor-([0-9\.]+)"
krakeniocunityRE="kraken-ioc-unity-([0-9\.]+)"

cd ./KrakenIoc

# KrakenIoc
if [[ $TRAVIS_TAG =~ $krakeniocRE ]]; then
    cd ./KrakenIoc/
    uget build -p ./KrakenIoc.csproj --debug
    uget create -p ./KrakenIoc.csproj --debug
    uget pack -p ./KrakenIoc.csproj --debug
    uget push -p ./KrakenIoc.csproj --debug
    cd ../
fi

# KrakenIoc.Editor
if [[ $TRAVIS_TAG =~ $krakenioceditorRE ]]; then
    cd ./KrakenIoc.Editor/
    uget build -p ./KrakenIoc.Editor.csproj --debug
    uget create -p ./KrakenIoc.Editor.csproj --debug
    uget pack -p ./KrakenIoc.Editor.csproj --debug
    uget push -p ./KrakenIoc.Editor.csproj --debug
    cd ../
fi

# KrakenIoc.Unity
if [[ $TRAVIS_TAG =~ $krakeniocunityRE ]]; then
    cd ./KrakenIoc.Unity/
    uget build -p ./KrakenIoc.Unity.csproj --debug
    uget create -p ./KrakenIoc.Unity.csproj --debug
    uget pack -p ./KrakenIoc.Unity.csproj --debug
    uget push -p ./KrakenIoc.Unity.csproj --debug
    cd ../
fi