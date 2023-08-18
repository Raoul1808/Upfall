dotnet restore -r linux-x64
dotnet publish -r linux-x64 -c Release
cp -r Brocco/deps/linux-x64/. Upfall/bin/Release/net7.0/linux-x64/publish/netcoredeps/
