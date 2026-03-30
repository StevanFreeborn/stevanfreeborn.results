param(
  [Parameter(Mandatory=$true)]
  [string]$Version,
  [string]$CsprojPath = "src/StevanFreeborn.Results/StevanFreeborn.Results.csproj"
)

$content = Get-Content $CsprojPath -Raw

if ($content -match "<Version>.*?</Version>") {
  $content = $content -replace "<Version>.*?</Version>", "<Version>$Version</Version>"
} elseif ($content -match "<PropertyGroup>") {
  $content = $content -replace "<PropertyGroup>", "<PropertyGroup>`n    <Version>$Version</Version>"
}

Set-Content -Path $CsprojPath -Value $content
