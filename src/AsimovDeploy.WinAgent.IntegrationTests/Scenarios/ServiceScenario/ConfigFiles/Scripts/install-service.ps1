param (
    $ServiceExecutable,
    $DisplayName
)

& $ServiceExecutable install -displayname \"$DisplayName\"