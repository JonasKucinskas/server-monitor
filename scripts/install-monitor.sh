#!/bin/sh

# Function to ensure the proxy URL ends with a /
ensure_trailing_slash() {
  if [ -n "$1" ]; then
    case "$1" in
    */) echo "$1" ;;
    *) echo "$1/" ;;
    esac
  else
    echo "$1"
  fi
}

# Define default values
PORT=13245
UNINSTALL=false
GITHUB_PROXY_URL=""
KEY=""

# Check for help flag
case "$1" in
-h | --help)
  printf "monitor-agent installation script\n\n"
  printf "Usage: ./install.sh [options]\n\n"
  printf "Options: \n"
  printf "  -k                    : SSH key (required, or interactive if not provided)\n"
  printf "  -p                    : Port (default: $PORT)\n"
  printf "  -u                    : Uninstall monitor agent\n"
  exit 0
  ;;
esac

# Build sudo args by properly quoting everything
build_sudo_args() {
  QUOTED_ARGS=""
  while [ $# -gt 0 ]; do
    if [ -n "$QUOTED_ARGS" ]; then
      QUOTED_ARGS="$QUOTED_ARGS "
    fi
    QUOTED_ARGS="$QUOTED_ARGS'$(echo "$1" | sed "s/'/'\\\\''/g")'"
    shift
  done
  echo "$QUOTED_ARGS"
}

# Check if running as root and re-execute with sudo if needed
if [ "$(id -u)" != "0" ]; then
  if command -v sudo >/dev/null 2>&1; then
    SUDO_ARGS=$(build_sudo_args "$@")
    eval "exec sudo $0 $SUDO_ARGS"
  else
    echo "This script must be run as root. Please either:"
    echo "1. Run this script as root (su root)"
    echo "2. Install sudo and run with sudo"
    exit 1
  fi
fi

# Parse arguments
while [ $# -gt 0 ]; do
  case "$1" in
  -k)
    shift
    KEY="$1"
    ;;
  -p)
    shift
    PORT="$1"
    ;;
  -u)
    UNINSTALL=true
    ;;
  *)
    echo "Invalid option: $1" >&2
    exit 1
    ;;
  esac
  shift
done

# Uninstall process
if [ "$UNINSTALL" = true ]; then
  
  echo "Stopping and disabling the agent service..."
  systemctl stop monitor-agent.service
  systemctl disable monitor-agent.service
  
  echo "Removing the systemd service file..."
  rm /etc/systemd/system/monitor-agent.service
  
  # Remove the update timer and service if they exist
  echo "Removing the daily update service and timer..."
  systemctl stop monitor-agent-update.timer 2>/dev/null
  systemctl disable monitor-agent-update.timer 2>/dev/null
  rm -f /etc/systemd/system/monitor-agent-update.service
  rm -f /etc/systemd/system/monitor-agent-update.timer
  
  systemctl daemon-reload

  echo "Removing the monitor agent directory..."
  rm -rf /opt/monitor-agent

  echo "Removing the dedicated user for the agent service..."
  killall monitor-agent 2>/dev/null
  userdel monitor 2>/dev/null


  echo "monitor-agent has been uninstalled successfully!"
  exit 0
fi

# Function to check if a package is installed
package_installed() {
  command -v "$1" >/dev/null 2>&1
}

# If no SSH key is provided, ask for the SSH key interactively
if [ -z "$KEY" ]; then
  printf "Enter your SSH key: "
  read KEY
fi

# Verify checksum
if command -v sha256sum >/dev/null; then
  CHECK_CMD="sha256sum"
elif command -v md5 >/dev/null; then
  CHECK_CMD="md5 -q"
else
  echo "No MD5 checksum utility found"
  exit 1
fi

# Create a dedicated user for the service if it doesn't exist

if ! id -u monitor >/dev/null 2>&1; then
    echo "Creating a dedicated user for the monitor-agent service..."
    useradd -M -s /bin/false monitor
fi

# Create the directory for the Beszel Agent
if [ ! -d "/opt/monitor-agent" ]; then
  echo "Creating the directory for the Beszel Agent..."
  mkdir -p /opt/monitor-agent
  chown monitor:monitor /opt/monitor-agent
  chmod 755 /opt/monitor-agent
fi

cp -r ../monitor/out/. /opt/monitor-agent/
chown monitor:monitor /opt/monitor-agent
chmod 755 /opt/monitor-agent

# Cleanup
#rm -rf "$TEMP_DIR"

# Original systemd service installation code
echo "Creating the systemd service for the agent..."
cat >/etc/systemd/system/monitor-agent.service <<EOF
[Unit]
Description=monitor-agent
Wants=network-online.target
After=network-online.target

[Service]
Environment="PORT=$PORT"
Environment="KEY=$KEY"
ExecStart=/opt/monitor-agent/monitor
User=monitor
Restart=on-failure
RestartSec=5
StateDirectory=monitor-agent

[Install]
WantedBy=multi-user.target
EOF

# Load and start the service
printf "\nLoading and starting the agent service...\n"
systemctl daemon-reload
systemctl enable monitor-agent.service
systemctl start monitor-agent.service

# Wait for the service to start or fail
if [ "$(systemctl is-active monitor-agent.service)" != "active" ]; then
  echo "Error: The monitor-agent service is not running."
  echo "$(systemctl status monitor-agent.service)"
  exit 1
fi


printf "\n\033[32mmonitorgent has been installed successfully! It is now running on port $PORT.\033[0m\n"