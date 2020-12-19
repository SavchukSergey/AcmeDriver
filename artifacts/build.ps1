$runtimes = @("linux-x64", "linux-arm", "win10-x64", "win10-x86")
$project = "../AcmeDriver/AcmeDriver.csproj"

foreach ($runtime in $runtimes) {
    dotnet publish $project --runtime $runtime -p:PublishSingleFile=true --self-contained false --output "build/$runtime"
}

dotnet clean $project