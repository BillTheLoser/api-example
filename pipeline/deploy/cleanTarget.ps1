# arguments
# arg[0] = server name i.e. whsapidev
# arg[1] = rootPath i.e. api\htmlroot\it
# arg[2] = deployPath i.e. approvals

$serverName = "\\" + $args[0]
$rootPath = ${serverName} + "\" + $args[1] + "\"
$deployPath = ${rootPath} + $args[2] +"\"

Write-Host "serverName: $servername"
Write-Host "rootPath: $rootPath"
Write-Host "deployPath: $deployPath"

# Files are locked when someone is in the app.  Deleting some files first will force the release of the application.
# This approach is a work around until we have the start and shutdown of the app pool automated.

if (Test-Path -path ${rootPath}web.config) {
    Remove-Item -path ${rootPath}web.config;
}

if (Test-Path -path ${deployPath}web.config) {
    Remove-Item -path ${deployPath}web.config;
}

# pause allows locks to be released after web.config is deleted
Start-Sleep -s 5;

# Delete files in deploy target
# Remove-Item -path ${deployPath}*.*;
Get-ChildItem -Path ${deployPath} -Recurse| Foreach-object {Remove-item -Recurse -path $_.FullName }

# Adding sleep at the end to try and prevent error in steps that come after this
# Seems like files get deleted but locks exists for a short period of time.  Might be an async command.
Start-Sleep -s 10;
