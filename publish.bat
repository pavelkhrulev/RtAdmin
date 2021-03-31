dotnet publish -c Release  -r win-x86 --self-contained -o publish/win-x86
dotnet publish -c Release  -r osx-x64 --self-contained -o publish/osx-x64
dotnet publish -c Release  -r linux-x64 --self-contained -o publish/linux-x64
dotnet publish -c Release  -r linux-arm64 --self-contained -o publish/linux-arm64
dotnet publish -c Release  -r linux-arm --self-contained -o publish/linux-armv32
