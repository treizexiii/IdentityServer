param(
    $app,
    $tag,
    [switch]$arm64,
    [switch]$help
)

if ($help)
{
    Write-Host "Builds the docker image for the specified composent"
    Write-Host "Parameters:"
    Write-Host "-app: The composent to build (server, command)"
    Write-Host "-tag: The tag to use for the image, default: dev"
    Write-Host "-arm64: Build for arm64"
    exit 0
}

if ($null -eq $tag)
{
    $tag = "dev"
}

$platform = "linux/amd64"
if ($arm64)
{
    Write-Output "Building for arm64"
    $tag = "$tag-arm64"
    $platform = "linux/arm64"
}

$image_name = "identity." + $app + ":$tag"

Write-Host "Building $image_name" -ForegroundColor Green

$path_to_dockerfile = "src//Identity.$app//Dockerfile"

docker buildx build --platform $platform -f $path_to_dockerfile -t $image_name .

if ($LASTEXITCODE -ne 0)
{
    Write-Error "Docker build for $app failed"
    exit 1
}

Write-Host "Build completed" -ForegroundColor Green

exit 0

