#!/bin/bash

while getopts ":uh" opt; do
  case $opt in
  u) UNINSTALL="true" ;;
  h)
    printf "API project installation\n\n"
    printf "Options: \n"
    printf "  -u  : Uninstall the API service\n"
    exit 0
    ;;
  \?)
    exit 1
    ;;
  esac
done

if [ "$UNINSTALL" = "true" ]; then

  echo "Stopping the API service"
  systemctl stop api.service
  systemctl disable api.service

  echo "Removing systemd file"
  rm /etc/systemd/system/api.service

  systemctl daemon-reload

  echo "Removing the API data"
  rm -rf /opt/api

  echo "Removing user"
  userdel api

  echo "The API has been uninstalled"
  exit 0
fi

if ! id -u api >/dev/null 2>&1; then
  echo "Creating user"
  useradd -M -s /bin/false api
fi

mkdir -p /opt/api/api
cp ./api/bin/Release/net9.0/linux-x64 /opt/api/api
chown -R api:api /opt/api

printf "Creating systemd service\n\n"
tee /etc/systemd/system/api.service <<EOF
[Unit]
Description=API Service
After=network.target

[Service]
ExecStart=/opt/api/api
WorkingDirectory=/opt/api
User=api
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
EOF

printf "\nStarting API service\n"
systemctl daemon-reload
systemctl enable api.service
systemctl start api.service

sleep 2

if [ "$(systemctl is-active api.service)" != "active" ]; then
  echo "service is not running."
exit 0