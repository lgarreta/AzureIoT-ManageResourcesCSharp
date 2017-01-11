
$login = Login-AzureRmAccount
$tenantId = $login.Context.Tenant.TenantId
$suscriptionId = $login.Context.Tenant.TenantId
$nombreAplicacion = "progiothubapp"
$clave = "iothub2017A"

$app = New-AzureRmADApplication -DisplayName $nombreAplicacion -HomePage "http://$nombreAplicacion/home" -IdentifierUris "http://$nombreAplicacion" -Password $clave
$serv = New-AzureRmADServicePrincipal -ApplicationId $app.ApplicationId
$role = New-AzureRmRoleAssignment -RoleDefinitionName Owner -ServicePrincipalName $app.ApplicationId

echo "TenantId: $tenantId"
echo "SubscriptionId: $suscriptionId"
echo "ApplicationId: $app.ApplicationId"
echo "Password: $clave"