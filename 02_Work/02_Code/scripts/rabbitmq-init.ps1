Param(
  [string]$HostName = "localhost",
  [string]$User = "guest",
  [string]$Pass = "guest"
)

$pair = "$User:$Pass"
$hdr = @{ 'content-type' = 'application/json' }

Invoke-RestMethod -Method Put -Headers $hdr -Uri "http://$HostName:15672/api/vhosts/devdcp" -Authentication Basic -Credential (New-Object System.Management.Automation.PSCredential($User,(ConvertTo-SecureString $Pass -AsPlainText -Force)))

$bodyExchange = '{"type":"topic","durable":true,"auto_delete":false,"internal":false,"arguments":{}}'
Invoke-RestMethod -Method Post -Headers $hdr -Uri "http://$HostName:15672/api/exchanges/devdcp/ingestion" -Body $bodyExchange -Authentication Basic -Credential (New-Object System.Management.Automation.PSCredential($User,(ConvertTo-SecureString $Pass -AsPlainText -Force)))

$bodyQueue = '{"durable":true,"arguments":{"x-dead-letter-exchange":"ingestion.dlx"}}'
Invoke-RestMethod -Method Post -Headers $hdr -Uri "http://$HostName:15672/api/queues/devdcp/ingestion.raw" -Body $bodyQueue -Authentication Basic -Credential (New-Object System.Management.Automation.PSCredential($User,(ConvertTo-SecureString $Pass -AsPlainText -Force)))

$bodyBind = '{"routing_key":"ingestion.raw.*"}'
Invoke-RestMethod -Method Post -Headers $hdr -Uri "http://$HostName:15672/api/bindings/devdcp/e/ingestion/q/ingestion.raw" -Body $bodyBind -Authentication Basic -Credential (New-Object System.Management.Automation.PSCredential($User,(ConvertTo-SecureString $Pass -AsPlainText -Force)))
