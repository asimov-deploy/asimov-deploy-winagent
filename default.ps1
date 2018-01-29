properties {
	$base_dir  = resolve-path .
	$build_dir = "$base_dir\build_artifacts"
	$lib_dir = "$base_dir\libs"
	$sln_file = "$base_dir\AsimovDeploy.sln"
	$tools_dir = "$base_dir\tools"
	$configuration = "Debug"
	$drop_folder = "$base_dir\build_artifacts\drop"
	$version = "0.7"
	$commit = "1234567"
	$branch = "master"
	$build = "10"
	$asimov_watch_folder = "D:\Path\ToWinAgentPackages"
	$asimov_install_dir = "D:\Path\ToWinAgentInstallDir"
	$asimov_web_port = '21233'
}

include .\teamcity.ps1

task default -depends Release

task Clean {
	Remove-Item -force -recurse $build_dir -ErrorAction SilentlyContinue
	Remove-Item -force -Recurse "src\*\build_artifacts"
}

task Init -depends Clean {
	$script:version = "$version.$build"
	$script:commit = $commit.substring(0,7)

	TeamCity-SetBuildNumber $script:version

	exec { git update-index --assume-unchanged "$base_dir\src\SharedAssemblyInfo.cs" }
	(Get-Content "$base_dir\src\SharedAssemblyInfo.cs") |
		Foreach-Object { $_ -replace "{version}", $script:version } |
		Set-Content "$base_dir\src\SharedAssemblyInfo.cs" -Encoding UTF8

	New-Item $build_dir -itemType directory -ErrorAction SilentlyContinue | Out-Null
}

function CreateZipFile([string] $name, [string] $folder) {
	$zipFile = "$base_dir\build_artifacts\packages\$name-v$script:version-[$branch]-[$script:commit].zip"
	$folderToZip = "$base_dir\build_artifacts\packages\$name\*"

	exec {
		& $tools_dir\7zip\7z.exe a -tzip `
			$zipFile `
			$folderToZip
	}
}

task CopyAsimovDeployWinAgent {
	Copy-Item "$base_dir\src\AsimovDeploy.WinAgent\build_artifacts\*" "$build_dir\packages\AsimovDeploy.WinAgent\" -Recurse -Force

	CreateZipFile("AsimovDeploy.WinAgent")
}

task CopyAsimovDeployWinAgentUpdater {
	Copy-Item "$base_dir\src\AsimovDeploy.WinAgentUpdater\build_artifacts\*" "$build_dir\packages\AsimovDeploy.WinAgentUpdater\" -Recurse -Force
	CreateZipFile("AsimovDeploy.WinAgentUpdater")
}

task CreateDistributionPackage {
	New-Item $build_dir\packages\AsimovDeploy -Type directory -ErrorAction SilentlyContinue | Out-Null
	Copy-Item "$build_dir\packages\*.zip" "$build_dir\packages\AsimovDeploy" -Force -ErrorAction SilentlyContinue

	$licenseFiles = @('LICENSE', "NOTICE", "library-licenses")
	Copy-Item "$base_dir\*" "$build_dir\packages\AsimovDeploy" -Recurse -Force -include $licenseFiles

	CreateZipFile("AsimovDeploy")
}
task UpdateAppConfig {
	$appConfig = "$base_dir\src\AsimovDeploy.WinAgentUpdater\App.config"
	$doc = (Get-Content $appConfig) -as [XML]
	$obj = $doc.configuration.appsettings.add | where {$_.Key -eq 'Asimov.WatchFolder'}
	$obj.value = $asimov_watch_folder
	$obj = $doc.configuration.appsettings.add | where {$_.Key -eq 'Asimov.InstallDir'}
	$obj.value = $asimov_install_dir
	$obj = $doc.configuration.appsettings.add | where {$_.Key -eq 'Asimov.WebPort'}
	$obj.value = $asimov_web_port
	$doc.Save($appConfig)
}
task Compile -depends Init,UpdateAppConfig {

	$v4_net_version = (ls "$env:windir\Microsoft.NET\Framework\v4.0*").Name

	try {
		Write-Host "Compiling with '$configuration' configuration"
		exec { dotnet restore }
		exec { dotnet build "$sln_file" -o "build_artifacts" -c $configuration }

	} catch {
		Throw
	} finally {
		exec { git checkout "$base_dir\src\SharedAssemblyInfo.cs" }
	}
}

task Test -depends Compile {

}

task Release -depends DoRelease

task CreateOutputDirectories {
	New-Item $build_dir\packages -Type directory -ErrorAction SilentlyContinue | Out-Null
	New-Item $build_dir\packages\AsimovDeploy.WinAgent -Type directory | Out-Null
	New-Item $build_dir\packages\AsimovDeploy.WinAgentUpdater -Type directory | Out-Null
	New-Item $build_dir\drop -Type directory | Out-Null
}

task PublishArtifact {
	Write-Host "Publish artifacts"

	Get-Childitem "$build_dir\packages\AsimovDeploy.WinAgentUpdater-*.zip" | Foreach-Object {
		TeamCity-PublishArtifact $_.FullName
	}
	Get-Childitem "$build_dir\packages\AsimovDeploy.WinAgent-*.zip" | Foreach-Object {
		TeamCity-PublishArtifact $_.FullName
	}
}

task CopyToDropFolder {
	Write-Host "Copying to drop folder $drop_folder"

	New-Item "$drop_folder\$script:version" -Type directory | Out-Null

	Copy-Item "$build_dir\packages\*.zip" "$drop_folder\$script:version" -Force -ErrorAction SilentlyContinue
}

task CopyToDropFolderProd {
	Write-Host "Copying to drop folder $drop_folder"

	if (-not(Test-Path "$drop_folder")) {
		New-Item "$drop_folder" -Type directory | Out-Null
	}

	Copy-Item "$build_dir\packages\*.zip" "$drop_folder" -Force -ErrorAction SilentlyContinue
}

task DoRelease -depends Compile, `
	CreateOutputDirectories, `
	CopyAsimovDeployWinAgentUpdater, `
	CopyAsimovDeployWinAgent, `
	CreateDistributionPackage, `
	PublishArtifact {
	Write-Host "Done building AsimovDeploy"
}
