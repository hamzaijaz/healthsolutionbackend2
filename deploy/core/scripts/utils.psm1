#region Az Cli Helpers
function Invoke-AzCliCommand {
    param(
        # The command to execute
        [Parameter(Mandatory)]
        [scriptblock]
        $Command,
        $MaxRetries = 3,
        $RetryDelaySeconds = 5
    )

    for ($retryCount = 1; $retryCount -le $MaxRetries; $retryCount++) {

        $currentErrorActionPreference = $ErrorActionPreference
        $ErrorActionPreference = "Continue" 

        $result = . $Command

        $exitCode = $LastExitCode
    
        $ErrorActionPreference = $currentErrorActionPreference

        # If the command is successful continue
        if ($exitCode -eq 0) {
            break
        }
        # If we reach MaxRetries bail
        elseif ($retryCount -eq $MaxRetries) {
            throw
        }
        # Otherwise wait and try again
        else {
            Start-Sleep -Seconds $RetryDelaySeconds
        }

        $retryCount++
    }

    return $result
} 
#endregion

#region Exports
Export-ModuleMember -Function Invoke-AzCliCommand
#endregion