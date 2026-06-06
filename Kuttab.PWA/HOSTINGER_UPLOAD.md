# Hostinger Upload Instructions

## Server Details
- **Server:** root@145.223.103.109
- **Port:** 22
- **Path:** /opt/ai-solutions/kuttab
- **URL:** https://ai-solutions.al-manar.de/kuttab/

## Deploy Steps

### 1. Build
```bash
dotnet publish Kuttab.PWA/Kuttab.PWA.csproj -c Release -o Kuttab.PWA/publish
```

### 2. Upload
```bash
scp -r -P 22 Kuttab.PWA/publish/wwwroot/* root@145.223.103.109:/opt/ai-solutions/kuttab/
```

### 3. Restart Docker (on server)
```bash
ssh -p 22 root@145.223.103.109 "docker-compose down && docker-compose up -d"
```

## Troubleshooting

If the browser shows old content after deploy:
1. Hard refresh: `Ctrl + Shift + R`
2. If still old: Open DevTools (`F12`) > Application > Service Workers > Unregister, then reload
