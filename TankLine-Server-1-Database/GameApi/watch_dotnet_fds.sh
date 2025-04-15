#!/bin/bash

# Max number of open files per process (adjust if changed in /etc/security/limits.conf)
MAX_LIMIT=$(ulimit -n)
THRESHOLD=$((MAX_LIMIT - 100))

echo "Maximum number of open files: $MAX_LIMIT"
echo "Alert threshold: $THRESHOLD open files"

while true; do
  clear
  echo "=== .NET Process Monitoring ($(date +%T)) ==="
  echo -e "PID\tFDs\tName\t\tALERT"
  
  for pid in $(pgrep dotnet); do
    open_fds=$(ls /proc/$pid/fd 2>/dev/null | wc -l)
    name=$(ps -p $pid -o comm=)
    
    if [ "$open_fds" -ge "$THRESHOLD" ]; then
      echo -e "$pid\t$open_fds\t$name\t⚠️  Near limit"
    else
      echo -e "$pid\t$open_fds\t$name\t"
    fi
  done
  
  sleep 2
done
