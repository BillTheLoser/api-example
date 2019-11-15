#arguments
#
# arg[0] = server name i.e. apidev
# arg[1] = root path i.e. api\htmlroot\it
# arg[2] = stage path i.e. approvals\stage

$serverName = "\\" + $args[0]
$stagePath = ${serverName} + "\" + $args[1] + "\" + $args[2]

Remove-Item -path ${stagePath}\* -recurse;