#!/bin/sh
set -e

echo "Generating self-signed certificate for HTTPS testing..."

# Create certificate directory
mkdir -p /https

# Generate self-signed certificate
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout /https/aspnetapp.key \
  -out /https/aspnetapp.crt \
  -subj "/CN=localhost/O=Development/C=US"

# Convert to PFX format (required by ASP.NET Core)
openssl pkcs12 -export -out /https/aspnetapp.pfx \
  -inkey /https/aspnetapp.key \
  -in /https/aspnetapp.crt \
  -passout pass:

echo "Certificate generated successfully at /https/aspnetapp.pfx"
