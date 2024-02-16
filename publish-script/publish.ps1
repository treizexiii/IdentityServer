param
(
    $app,
    $tag,
    $registry,
    [switch] $help
)

if ($help)
{
    write-host "Publishes the specified composent to docker registry"
    write-host "Parameters:"
    write-host "-app: The composent to publish (server, command)"
    write-host "-tag: The tag to use for the image"
    write-host "-registry: The registry to publish to (could be set as .env file)"
    exit 0
}

if ($null -eq $tag)
{
    $tag = "dev"
}

if (Test-Path 'publish/.env')
{
    $envFile = Get-Content -Path 'publish/.env'
    foreach ($line in $envFile)
    {
        Write-Output $line
        $parts = $line -split '=', 2
        if ($parts[0] -eq "REGISTRY")
        {
            $registry = $parts[1]
        }
    }
}

if ($null -eq $registry)
{
    Write-Error "Registry is not provided"
    exit 1
}

$image_to_push = "identity.$app" + ":$tag"
$repository = $registry + $image_to_push

Write-Host "Publishing to $repository" -ForegroundColor Green

docker image tag $image_to_push $repository
docker push $repository
docker rmi $repository

if ($LASTEXITCODE -ne 0)
{
    Write-Error "Docker push for $app failed"
    exit 1
}

Write-Host "Publish completed" -ForegroundColor Green

# delete env variables
Remove-Item Env:\REGISTRY

exit 0
