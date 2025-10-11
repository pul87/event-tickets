If you have permission problems on your system related to the script execution you can try to allow the execution for the current Powershell Session
```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force
```