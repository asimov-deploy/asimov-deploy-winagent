param (
    $SiteName,
    $SiteUrl
)

write-host $ServiceExecutable, $ServiceName
Import-module WebAdministration;
New-WebAppPool -Name "${SiteName}AppPool"
New-WebSite -Name $SiteName -ApplicationPool "${SiteName}AppPool" -PhysicalPath "$(pwd)" -Port ([System.Uri]$SiteUrl).Port
        