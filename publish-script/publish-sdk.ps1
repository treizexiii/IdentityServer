param
(
    [switch] $help,
    $nuggetRegistry,
    $nuggetApiKey
)

if ($help)
{
    Write-Host "Publishes the SDK to the specified nuget registry"
    Write-Host "Parameters:"
    Write-Host "-nuggetRegistry: The nuget registry to publish to"
    Write-Host "-nuggetApiKey: The api key to use for the registry"
    exit 0
}

if (Test-Path 'publish/.env')
{
    $envFile = Get-Content -Path 'publish/.env'
    foreach ($line in $envFile)
    {
        $parts = $line -split '=', 2
        if ($parts[0] -eq "NUGGET_REGISTRY")
        {
            $nuggetRegistry = $parts[1]
        }
        if ($parts[0] -eq "NUGGET_API_KEY")
        {
            $nuggetApiKey = $parts[1]
        }
    }
}

if ($null -eq $nuggetRegistry)
{
    Write-Error "Nugget registry is not provided"
    exit 1
}

if ($null -eq $nuggetApiKey)
{
    Write-Error "Nugget api key is not provided"
    exit 1
}

Write-Host "Publishing SDK" -ForegroundColor Green
Write-Output "Retore project"
dotnet restore
Write-Output "Build project"
dotnet build --no-restore -c Release
Write-Output "Pack project"
dotnet pack --no-restore --no-build -c Release -o ./artifacts  src/Identity.Sdk/Identity.Sdk.csproj
dotnet pack --no-restore --no-build -c Release -o ./artifacts  src/Tools/Tools.TransactionsManager/Tools.TransactionsManager.csproj

Write-Output "Publish project to $nuggetRegistry"
dotnet nuget push ./artifacts/Identity.Sdk.0.9.0.nupkg --source $nuggetRegistry --api-key $nuggetApiKey

