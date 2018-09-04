https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?tabs=aspnetcore2x&view=aspnetcore-2.1

https://www.microsoft.com/net/download/linux-package-manager/ubuntu16-04/sdk-2.1.4

#install SDK x64:
wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1.4

#arm:
https://www.microsoft.com/net/download/thank-you/dotnet-sdk-2.1.302-linux-arm32-binaries
wget https://download.microsoft.com/download/4/0/9/40920432-3302-47a8-b13c-bbc4848ad114/dotnet-sdk-2.1.302-linux-arm.tar.gz
mkdir -p $HOME/dotnet && tar zxf dotnet-sdk-2.1.302-linux-arm.tar.gz -C $HOME/dotnet
export DOTNET_ROOT=$PATH:$HOME/dotnet 
export PATH=$PATH:$HOME/dotnet

#proxy
apt-get install nginx
## alleen nodig na eerste setup
sudo service nginx start

http://<server_IP_address>/index.nginx-debian.html

modify /etc/nginx/sites-available/default

server {
    listen        8080;
    server_name   localhost;
    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}

#firewall
sudo ufw allow 8080

create server file:
sudo nano /etc/systemd/system/kestrel-pi-gpio.service

[Unit]
Description=Example .NET Web API App running on Ubuntu

[Service]
WorkingDirectory=/home/opvolger/runapi
ExecStart=/home/opvolger/dotnet/dotnet /home/opvolger/runapi/api.rpi.gpio.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
SyslogIdentifier=dotnet-rpi-gpio
User=opvolger
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target

##
systemctl enable kestrel-pi-gpio.service
systemctl start kestrel-pi-gpio.service
systemctl status kestrel-pi-gpio.service

sudo journalctl -fu kestrel-pi-gpio.service

sudo journalctl -fu kestrel-pi-gpio.service --since "2016-10-18" --until "2016-10-18 04:00"