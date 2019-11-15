# this will create a stage directory if it doesn't exist and also create a subdirectory for the api artifacts
# arg[0] = server name i.e. apidev
# arg[1] = root path 
# arg[2] = stage path i.e. stage
# arg[3] = api subdirectory under the stage directory

$serverName = $args[0]
$stagePath = "\\" + ${serverName} + "\" + $args[1] + "\" + $args[2]
$stageApiPath = $stagePath + "\" + $args[3]

If(-not (Test-Path $stagePath))
{
      New-Item -ItemType Directory -Force -Path ${stagePath}
}
else {
      Remove-Item -path ${stagePath}\* -recurse;
}

# Short pause
Start-Sleep -s 5;

New-Item -ItemType Directory -Force -Path $stageApiPath
