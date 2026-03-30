param(
  [string]$Branch = "main"
)

$tag = git describe --tags --abbrev=0 2>$null
$base = if ($tag) { $tag } else { $Branch }

$commits = git log "$base..HEAD" --format="%s"
$commitList = $commits -split "`n"
$major = 0
$minor = 0
$patch = 0

foreach ($message in $commitList) {
  if ($message -match " BREAKING CHANGE|!:" -and $message.Trim().Length -gt 0) {
    $major = 1
  } elseif ($message -match "feat(\(.*\))?:" -and $message.Trim().Length -gt 0) {
    $minor = 1
  } elseif ($message -match "fix(\(.*\))?:" -and $message.Trim().Length -gt 0) {
    $patch = 1
  }
}

if ($major -eq 1) {
  $minor = 0
  $patch = 0
} elseif ($minor -eq 1) {
  $patch = 0
} elseif ($patch -eq 0 -and $commitList.Count -gt 0 -and $commitList[0]) {
  $patch = 1
}

$currentVersion = "0.0.0"
$hasExistingTag = $false

if ($tag) {
  $hasExistingTag = $true
  $currentVersion = $tag -replace "^v", ""
}

$parts = $currentVersion.Split(".")
$majorCurrent = [int]$parts[0]
$minorCurrent = [int]$parts[1]
$patchCurrent = [int]$parts[2]

$majorNew = $majorCurrent + $major
$minorNew = if ($major -eq 1) { 0 } else { $minorCurrent + $minor }
$patchNew = if ($major -eq 1 -or $minor -eq 1) { 0 } else { $patchCurrent + $patch }

if ($majorNew -eq $majorCurrent -and $minorNew -eq $minorCurrent -and $patchNew -eq $patchCurrent) {
  if (-not $hasExistingTag) {
    $version = "0.0.0"
    Write-Output "VERSION=$version"
    Write-Host "First release: publishing version 0.0.0"
  } else {
    Write-Host "No version bump needed, skipping release"
    exit 0
  }
} else {
  $version = "$majorNew.$minorNew.$patchNew"
  Write-Output "VERSION=$version"
}
