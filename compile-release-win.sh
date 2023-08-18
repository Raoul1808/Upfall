dotnet restore -r win-x64
dotnet publish -r win-x64 -c Release
cp -r Brocco/deps/win-x64/. Upfall/bin/Release/net7.0/win-x64/publish/
