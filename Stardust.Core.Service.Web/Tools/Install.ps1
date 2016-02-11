param($installPath, $toolsPath, $package, $project)
Write-Host "Figuring out this shit"
Write-Host "Figuring out this shit!!"
$fileBinding=$project.ProjectItems.Item("App_Start").ProjectItems.Item("WebApplicationBindings.cs")

Write-Host $fileBinding

$files = $package.GetFiles() | Where-Object {$_.EffectivePath -match "tt$"}
 
  

