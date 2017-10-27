param (
    $ServiceExecutable,
	$ServiceName
)

write-host $ServiceExecutable, $ServiceName

& "$ServiceExecutable" uninstall -servicename:"$ServiceName"