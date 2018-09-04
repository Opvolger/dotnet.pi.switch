param ([string]$ip, [string]$destination = "/home/opvolger/runapi/", [string]$username = "opvolger")

dotnet clean .
dotnet restore .
dotnet build .
dotnet publish . -r linux-arm

& plink.exe -v -ssh ${username}@${ip} systemctl stop kestrel-pi-gpio.service
#& pscp.exe -r .\bin\Debug\netcoreapp2.1\linux-arm\publish\api.rpi.gpio* ${username}@${ip}:${destination}
& pscp.exe -r .\bin\Debug\netcoreapp2.1\linux-arm\publish\* ${username}@${ip}:${destination}
#& plink.exe -v -ssh ${username}@${ip} chmod u+x,o+x ${destination}/api.rpi.gpio.dll
& plink.exe -v -ssh ${username}@${ip} systemctl start kestrel-pi-gpio.service
