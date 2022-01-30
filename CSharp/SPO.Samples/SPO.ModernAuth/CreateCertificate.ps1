Param(
    [string]$FriendlyName = "Bipul_Test123",
    [string]$DnsName = "Bipul_Test123",
    [string]$Subject = "CN=Bipul_Test123",
    [string]$ValidFrom = "2022-01-01",
    [string]$ValidUntill = "2023-01-01",
    [string]$CerFilePath = "D:\\Bipul_Test123.cer",
    [string]$PfxFilePath = "D:\\\Bipul_Test123.pfx",
    [SecureString]$SecurePassword
)

# Read password from console
$SecurePassword = Read-Host "Enter Password for Certificate" -AsSecureString
# Create Certificate
$Cert = New-SelfSignedCertificate -FriendlyName $FriendlyName -DnsName $DnsName -Subject $Subject -NotBefore $ValidFrom -NotAfter $ValidUntill -KeyExportPolicy Exportable -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" -CertStoreLocation "Cert:\LocalMachine\My"

#Generate .pfx
Export-PfxCertificate -Cert $Cert -FilePath $PfxFilePath -Password $SecurePassword

#Export to .cer format
Export-Certificate -Type CERT -Cert $Cert -FilePath $CerFilePath

# Add Certificate to trusted root
$cert = (Get-ChildItem -Path $CerFilePath)
$cert | Import-Certificate -CertStoreLocation cert:\CurrentUser\Root

Write-Host Task Completed ! -ForegroundColor Green 