dotnet build -p:Configuration=Release
rm "/home/eva/.local/share/Steam/steamapps/common/VA Proxy Demo/instance1/BepInEx/plugins/vapsr-client.dll"
cp ./bin/Release/net48/vapsr-client.dll "/home/eva/.local/share/Steam/steamapps/common/VA Proxy Demo/instance1/BepInEx/plugins/"
rm "/home/eva/.local/share/Steam/steamapps/common/VA Proxy Demo/instance2/BepInEx/plugins/vapsr-client.dll"
cp ./bin/Release/net48/vapsr-client.dll "/home/eva/.local/share/Steam/steamapps/common/VA Proxy Demo/instance2/BepInEx/plugins/"