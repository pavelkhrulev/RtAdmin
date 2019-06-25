dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true -o publish\win64 ./ /p:Runtime=win-x64 /p:NativeLibraryName=rtpkcs11ecp.dll
dotnet publish -c Release -r win-x86 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true -o publish\win32 ./ /p:Runtime=win-x86 /p:NativeLibraryName=rtpkcs11ecp.dll
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true -o publish\linux64 ./ /p:Runtime=linux-x64 /p:NativeLibraryName=librtpkcs11ecp.so
dotnet publish -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true -o publish\osx64 ./ /p:Runtime=osx-x64 /p:NativeLibraryName=librtpkcs11ecp.dylib
