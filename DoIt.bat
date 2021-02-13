sc create CheckSafeMode binpath= "C:\Users\Public\CheckSafeMode.exe" type= own start= auto DisplayName= "CheckSafeMode"
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SafeBoot\Minimal\CheckSafeMode"
reg add "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SafeBoot\Minimal\CheckSafeMode" /f /v "Service"
bcdedit /set {current} safeboot Minimal
shutdown /r /f /t 00