dotnet restore -r osx-x64
dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:UseAppHost=true -property:Configuration=Release
cp -r Brocco/deps/osx/. Upfall/bin/Release/net7.0/osx-x64/publish/Upfall.app/Contents/MacOS/
