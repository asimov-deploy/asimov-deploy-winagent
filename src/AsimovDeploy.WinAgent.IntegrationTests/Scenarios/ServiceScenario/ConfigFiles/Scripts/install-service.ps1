param (
    $ServiceExecutable,
	$ServiceName
)

write-host $ServiceExecutable, $ServiceName

& "$ServiceExecutable" install -servicename:"$ServiceName"