function Get-SolutionDir {
    if($dte.Solution -and $dte.Solution.IsOpen) {
        return Split-Path $dte.Solution.Properties.Item("Path").Value
    }
    else {
        throw "Solution not avaliable"
    }
}

function Resolve-ProjectName {
    param(
        [parameter(ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    
    if($ProjectName) {
        $projects = Get-Project $ProjectName
    }
    else {
        # All projects by default
        $projects = Get-Project -All
    }
    
    $projects
}

function Get-MSBuildProject {
    param(
        [parameter(ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    Process {
        (Resolve-ProjectName $ProjectName) | % {
            $path = $_.FullName
            @([Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($path))[0]
        }
    }
}

function Set-MSBuildProperty {
    param(
        [parameter(Position = 0, Mandatory = $true)]
        $PropertyName,
        [parameter(Position = 1, Mandatory = $true)]
        $PropertyValue,
        [parameter(Position = 2, ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName
    )
    Process {
        (Resolve-ProjectName $ProjectName) | %{
            $buildProject = $_ | Get-MSBuildProject
            $buildProject.SetProperty($PropertyName, $PropertyValue) | Out-Null
            $_.Save()
        }
    }
}

function Get-MSBuildProperty {
    param(
        [parameter(Position = 0, Mandatory = $true)]
        $PropertyName,
        [parameter(Position = 2, ValueFromPipelineByPropertyName = $true)]
        [string]$ProjectName
    )
    
    $buildProject = Get-MSBuildProject $ProjectName
    $buildProject.GetProperty($PropertyName)
}

function Install-stardust {
    param(
        [parameter(ValueFromPipelineByPropertyName = $true)]
        [string[]]$ProjectName,
    	[switch]$EnableIntelliSense,
        [string]$TemplatePath
    )
    
    Process {
    
        $projects = (Resolve-ProjectName $ProjectName)
        
        if(!$projects) {
            Write-Error "Unable to locate project. Make sure it isn't unloaded."
            return
        }
		
		$profileDirectory = Split-Path $profile -parent
		$profileModulesDirectory = (Join-Path $profileDirectory "Modules")
		$moduleDir = (Join-Path $profileModulesDirectory "stardust")
		
        if($EnableIntelliSense){
            Enable-NuSpecIntelliSense            
        }
        
    }
}

function Enable-stardustIntelliSense {
    Process {		
		$profileDirectory = Split-Path $profile -parent
		$profileModulesDirectory = (Join-Path $profileDirectory "Modules")
		$moduleDir = (Join-Path $profileModulesDirectory "Stardust")

        $solutionDir = Get-SolutionDir
        $solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])
        
        # Set up solution folder "Solution Items"
        $solutionItemsProject = $dte.Solution.Projects | Where-Object { $_.ProjectName -eq "Solution Items" }
        if(!($solutionItemsProject)) {
            $solutionItemsProject = $solution.AddSolutionFolder("Solution Items")
        }        
        
        # Copy the XSD in the solution directory
        try {
            $xsdInstallPath = Join-Path $solutionDir 'stardust.xsd'
            $xsdToolsPath = Join-Path $moduleDir 'stardust.xsd'
                
            if(!(Test-Path $xsdInstallPath)) {
                Copy-Item $xsdToolsPath $xsdInstallPath
            }
                
            $alreadyAdded = $solutionItemsProject.ProjectItems | Where-Object { $_.Name -eq 'stardust.xsd' }
            if(!($alreadyAdded)) {
                $solutionItemsProject.ProjectItems.AddFromFile($xsdInstallPath) | Out-Null
            }
        }
        catch {
            Write-Warning "Failed to install stardust.xsd into 'Solution Items'"
        }
        $solution.SaveAs($solution.FullName)
    }
}

# Statement completion for project names
'Install-stardust', 'Enable-stardustIntelliSense' | %{ 
    Register-TabExpansion $_ @{
        ProjectName = { Get-Project -All | Select -ExpandProperty Name }
    }
}

Export-ModuleMember Install-stardust, Enable-stardustIntelliSense