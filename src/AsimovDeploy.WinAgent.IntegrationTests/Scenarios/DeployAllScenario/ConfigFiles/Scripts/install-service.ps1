param (
    $ServiceExecutable,
	$ServiceName,
	$DisplayName
)

write-host $ServiceExecutable, $ServiceName, $DisplayName

& "$ServiceExecutable" install -servicename:"$ServiceName" -displayname:"$DisplayName"