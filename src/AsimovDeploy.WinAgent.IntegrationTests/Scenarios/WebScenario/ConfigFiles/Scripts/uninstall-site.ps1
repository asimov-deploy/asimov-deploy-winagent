param (
    $SiteName
)

Import-module WebAdministration;
Remove-WebSite -name $SiteName
Remove-WebAppPool -Name "${SiteName}AppPool"
