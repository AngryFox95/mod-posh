# netsh advfirewall firewall add rule name="RPC Dynamic Ports" dir=in action=allow enable=yes profile=domain localip=any remoteip=any localport=rpc remoteport=any protocol=tcp edge=no

$FwMgr = New-Object -ComObject HNetCfg.FwMgr
$FwProfile = $FwMgr.LocalPolicy.CurrentProfile

$FwPort = New-Object -ComObject HNetCfg.FwOpenPort
$FwPort.Name = "HTTP"
$FwPort.Protocol = 6
$FwPort.Port = 8080
$FwPort.Scope = 1
$FwPort.Enabled = $true

$FwProfile.GloballyOpenPorts.Add($FwPort)
