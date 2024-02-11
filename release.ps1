$DistDir = "$PSScriptRoot\dist"
$BuildDir = "$DistDir\build"
$ReleaseDir = "$DistDir\release"
$xml = [xml](Get-Content $PSScriptRoot\src\QualityCompany.csproj)
$Version = $xml.Project.PropertyGroup.Version
$ZipFile = "$ReleaseDir\QualityCompany_$Version.zip"
$ReleaseAssetBundleArtifact = "$PSScriptRoot\src\Assets\modnetworkhandlerbundle"
$ReleaseDllArtifact = "$PSScriptRoot\src\bin\Release\netstandard2.1\QualityCompany.dll"
$ReleaseXmlDocsArtifact = "$PSScriptRoot\src\bin\Release\netstandard2.1\QualityCompany.xml"

Write-Output "Creating build for QualityCompany v$Version"

# cleanup build directory
Remove-Item $BuildDir -Recurse -Force -ErrorAction SilentlyContinue
# cleanup existing Zip file
Remove-Item $ZipFile -Force -ErrorAction SilentlyContinue

# create directories if necessary
If (-not (Test-Path $BuildDir)) {
    New-Item -Type dir $BuildDir | Out-Null
}
If (-not (Test-Path $ReleaseDir)) {
    New-Item -Type dir $ReleaseDir | Out-Null
}

# build the mod if needed, this will occur on a Release build in IDE
# dotnet build --configuration Release

# update manifest.json version
$ModManifestFile = "$PSScriptRoot\manifest.json"
$ModManifestTmpFile = "$PSScriptRoot\manifest_tmp.json"
Get-Content $ModManifestFile | jq --arg version "$Version" '.version_number = "$version"' > $ModManifestTmpFile
Copy-Item $ModManifestTmpFile $ModManifestFile
Remove-Item $ModManifestTmpFile

function CopyItemToBuild {
    $src = $args[0]
    $fileName = [System.IO.Path]::GetFileName("$src")
    Copy-Item $src $BuildDir\$fileName
}

# copy required mod files
CopyItemToBuild $PSScriptRoot\manifest.json
CopyItemToBuild $PSScriptRoot\README.md
CopyItemToBuild $PSScriptRoot\CHANGELOG.md
CopyItemToBuild $ReleaseAssetBundleArtifact
CopyItemToBuild $ReleaseDllArtifact
CopyItemToBuild $ReleaseXmlDocsArtifact

# create the zip in ReleaseDir
Get-ChildItem -Path $BuildDir | Compress-Archive -DestinationPath $ZipFile

Write-Output "Created $ZipFile"