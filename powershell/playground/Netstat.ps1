$netstat = netstat -a -n -o | where-object { $_ -match "(UDP|TCP)" }
[regex]$regexTCP = '(?<Protocol>\S+)\s+((?<LAddress>(2[0-4]\d|25[0-5]|[01]?\d\d?)\.(2[0-4]\d|25[0-5]|[01]?\d\d?)\.(2[0-4]\d|25[0-5]|[01]?\d\d?)\.(2[0-4]\d|25[0-5]|[01]?\d\d?))|(?<LAddress>\[?[0-9a-fA-f]{0,4}(\:([0-9a-fA-f]{0,4})){1,7}\%?\d?\]))\:(?<Lport>\d+)\s+((?<Raddress>(2[0-4]\d|25[0-5]|[01]?\d\d?)\.(2[0-4]\d|25[0-5]|[01]?\d\d?)\.(2[0-4]\d|25[0-5]|[01]?\d\d?)\.(2[0-4]\d|25[0-5]|[01]?\d\d?))|(?<RAddress>\[?[0-9a-fA-f]{0,4}(\:([0-9a-fA-f]{0,4})){1,7}\%?\d?\]))\:(?<RPort>\d+)\s+(?<State>\w+)\s+(?<PID>\d+$)'

[regex]$regexUDP = '(?<Protocol>\S+)\s+((?<LAddress>(2[0-4]\d|25[0-5]|[01]?\d\d?)\.(2[0-4]\d|25[0-5]|[01]?\d\d?)\.(2[0-4]\d|25[0-5]|[01]?\d\d?)\.(2[0-4]\d|25[0-5]|[01]?\d\d?))|(?<LAddress>\[?[0-9a-fA-f]{0,4}(\:([0-9a-fA-f]{0,4})){1,7}\%?\d?\]))\:(?<Lport>\d+)\s+(?<RAddress>\*)\:(?<RPort>\*)\s+(?<PID>\d+)'
$Report = @()

foreach ($Line in $Netstat)
{
    $LineItem = New-Object -TypeName PSobject
    switch -regex ($Line.Trim())
    {
        $RegexTCP
        {
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name Protocol -Value $Matches.Protocol
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name LocalAddress -Value $Matches.LAddress
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name LocalPort -Value $Matches.LPort
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name RemoteAddress -Value $Matches.Raddress
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name RemotePort -Value $Matches.RPort
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name State -Value $Matches.State
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name PID -Value $Matches.PID
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name ProcessName -Value (Get-Process -Id $Matches.PID -ErrorAction SilentlyContinue).ProcessName
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name ProcessPath -Value (Get-Process -Id $Matches.PID -ErrorAction SilentlyContinue).Path
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name User -Value (Get-WmiObject -Class Win32_Process -Filter ("ProcessId = "+$Matches.PID)).GetOwner().User
        }
        $RegexUDP
        {
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name Protocol -Value $Matches.Protocol
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name LocalAddress -Value $Matches.LAddress
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name LocalPort -Value $Matches.LPort
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name RemoteAddress -Value $Matches.Raddress
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name RemotePort -Value $Matches.RPort
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name State -Value $Matches.State
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name PID -Value $Matches.PID
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name ProcessName -Value (Get-Process -Id $Matches.PID -ErrorAction SilentlyContinue).ProcessName
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name ProcessPath -Value (Get-Process -Id $Matches.PID -ErrorAction SilentlyContinue).Path
            Add-Member -InputObject $LineItem -MemberType NoteProperty -Name User -Value (Get-WmiObject -Class Win32_Process -Filter ("ProcessId = "+$Matches.PID)).GetOwner().User
        }
    }
    $Report += $LineItem
}