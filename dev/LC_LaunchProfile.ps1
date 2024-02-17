<#
    .SYNOPSIS
    PowerShell script to launch Lethal Company profiles easily.

    .DESCRIPTION
    This script facilitates the launching of Lethal Company with custom configurations.
    It allows the user to select a game profile, define the number of concurrent game windows, 
    and set the size of these windows. It reads and saves configurations to a JSON file.

    .NOTES
    File Name      : LC_LaunchProfile.ps1
    Author         : Ricky Davis
    Copyright      : � 2024 Ricky Davis. All rights reserved.
    Version        : 1.0.0

    .DISCLAIMER
    This script is provided "as is", without warranty of any kind, express or implied.
    In no event shall the author be liable for any claim, damages, or other liability
    arising from, out of, or in connection with the software or the use or other dealings in the software.

    .EXAMPLE
    .\LC_LaunchProfile.ps1
    Executes the script to start Lethal Company based on the user's configurations.
#>

Add-Type -AssemblyName System.Windows.Forms


#### Util Initializations ###
#### Util Initializations ###
#### Util Initializations ###

<#
.SYNOPSIS
Loads the application configuration from a file, or creates a default configuration if the file does not exist.

.DESCRIPTION
This function checks for the existence of a configuration file. If found, it loads and returns the configuration.
Otherwise, it creates a default configuration, saves it to a file, and then returns it.

.OUTPUTS
Hashtable. Returns a hashtable of configuration settings.
#>
function LoadConfig() {
    # Check if the configuration file exists
    if (Test-Path $configFilePath) {
        # Read and return the configuration from the existing file as a JSON object
        return Get-Content $configFilePath | ConvertFrom-Json
    } else {
        # Define the default configuration settings
        $defaultConfig = @{
            "exePath" = 'C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company.exe'
            "profileDirectory" = "$($env:APPDATA)\r2modmanPlus-local\LethalCompany\profiles"
            "monitor" = 0  # Default monitor index
            "selectedProfile" = ""  # Default to no selected profile
            "windowCount" = 1  # Default number of game windows to launch
            "windowSize" = 1  # Default game window size (full size)
        }
        # Save the default configuration to a file and return it
        $defaultConfig | ConvertTo-Json | Set-Content $configFilePath
        return $defaultConfig
    }
}

<#
.SYNOPSIS
Saves the application configuration to a file.

.DESCRIPTION
This function takes a configuration object as input and saves it to a file in JSON format. 
The JSON is formatted for readability.

.PARAMETER config
The configuration object to be saved.

.INPUTS
Hashtable. The function expects a hashtable object containing configuration settings.

.OUTPUTS
None. The function saves the configuration to a file and does not return any output.
#>
function SaveConfig($config) {
    # Convert the configuration object to a JSON string and save it to the config file
    $config | ConvertTo-Json | Set-Content $configFilePath
}

<#
.SYNOPSIS
Gets a validated user selection from the console.

.DESCRIPTION
Prompts the user for input and validates the input against a specified range.

.PARAMETER Prompt
The prompt message to display to the user.

.PARAMETER DefaultIndex
The default index to use if the user does not provide an input.

.PARAMETER MaxValue
The maximum valid value for the user's selection.

.OUTPUTS
Int. Returns the validated user selection.
#>
function GetValidatedSelection($Prompt, $DefaultIndex, $MaxValue) {
    do {
        Write-Host -ForegroundColor Yellow -NoNewline $Prompt
        $selection = Read-Host

        if ([string]::IsNullOrWhiteSpace($selection)) {
            return $DefaultIndex
        }

        if ($selection -match '^\d+$' -and $selection -ge 1 -and $selection -le $MaxValue) {
            return $selection
        }

        Write-Host -ForegroundColor Red "Invalid selection. Please try again."
    } while ($true)
}

<#
.SYNOPSIS
Prepares the arguments for launching the game.

.DESCRIPTION
This function creates a list of arguments for the game launch command based on the given window dimensions and configuration settings.

.PARAMETER Width
The width of the game window.

.PARAMETER Height
The height of the game window.

.PARAMETER Config
The configuration object containing various settings like profile directory, selected profile, etc.

.OUTPUTS
Array. Returns an array of strings representing the game launch arguments.
#>
function PrepareGameLaunchArguments {
    param(
        [int]$Width,
        [int]$Height,
        [PSCustomObject]$Config
    )

    # Prepare the screen dimensions arguments
    $heightArg = '-screen-height ' + $Height
    $widthArg = '-screen-width ' + $Width

    # Prepare other necessary arguments
    $doorstopEnable = '--doorstop-enable true'
    $fullPath = [IO.Path]::Combine($Config.profileDirectory, $Config.selectedProfile, "BepInEx\core\BepInEx.Preloader.dll")
    $doorstopTarget = "--doorstop-target `"$fullPath`""
    $monitorArg = '-monitor ' + $Config.monitor
    $fullscreenArg = '--screen-fullscreen 0'

    # Return the array of arguments
    return $doorstopEnable, $doorstopTarget, $monitorArg, $fullscreenArg, $heightArg, $widthArg
}



#### Main Function Initializations ###
#### Main Function Initializations ###
#### Main Function Initializations ###


<#
.SYNOPSIS
Prompts the user to select a game profile from the available directories.

.DESCRIPTION
This function lists all child directories in the profile directory and allows the user to select one.
If no selection is made, the default profile is used.

.OUTPUTS
String. Returns the name of the selected profile.
#>
function SelectProfile() {
    # Retrieve all child directories in the profile directory
    $childDirs = Get-ChildItem -Path $config.profileDirectory -Directory

    # Handle case when no profiles are available
    if ($childDirs.Count -eq 0) {
        Write-Host -ForegroundColor Red "No profiles found."
        return $null
    }

    # Default to the first profile if no profile is currently selected
    if ([string]::IsNullOrWhiteSpace($config.selectedProfile)) {
        $config.selectedProfile = $childDirs[0].Name
    }

    # Find the index of the default profile
    $defaultProfileIndex = $childDirs.Name.IndexOf($config.selectedProfile) + 1

    # Display the list of available profiles to the user
    for ($i = 0; $i -lt $childDirs.Count; $i++) {
        Write-Host "$($i+1): $($childDirs[$i].Name)"
    }

    # Get user selection with validation
    $selection = GetValidatedSelection -Prompt "Select a profile ($($config.selectedProfile)): " -DefaultIndex $defaultProfileIndex -MaxValue $childDirs.Count

    # Return the selected profile name
    return $childDirs[$selection - 1].Name
}

<#
.SYNOPSIS
Prompts the user to select the number of concurrent game windows to launch.

.DESCRIPTION
This function asks the user to specify how many copies of the game they want to launch. 
It ensures the input is within a valid range (1 to 4).

.OUTPUTS
Int. Returns the number of game windows to launch.
#>
function SelectConcurrentWindows() {
    # Prompt for user input with validation
    $promptMessage = "How many copies of the game do you want to launch? ($($config.windowCount)): "
    $selection = GetValidatedSelection -Prompt $promptMessage -DefaultIndex $config.windowCount -MaxValue 8

    # Return the validated selection
    return $selection
}

<#
.SYNOPSIS
Prompts the user to select the size of the game window.

.DESCRIPTION
This function displays a list of possible window sizes as fractions of the full screen size.
The user is asked to choose one of these sizes.

.OUTPUTS
Double. Returns the fraction representing the selected window size.
#>
function SelectWindowSize() {
    # Define an array of possible window sizes as fractions of full size
    $sizeSelections = @(1.0, 0.75, 0.5, 0.25)

    # Find the index of the default window size in the size selections
    $defaultSizeIndex = $sizeSelections.IndexOf([double]$config.windowSize) + 1

    # Display the window size options to the user
    for ($i = 0; $i -lt $sizeSelections.Count; $i++) {
        Write-Host "$($i+1): $([int]($sizeSelections[$i]*100))%"
    }

    # Prompt for user input with validation
    $promptMessage = "How big do you want the game windows compared to your screen ($([int]($config.windowSize*100))%): "
    $selection = GetValidatedSelection -Prompt $promptMessage -DefaultIndex $defaultSizeIndex -MaxValue $sizeSelections.Count

    # Return the selected size from the size selections
    return $sizeSelections[$selection - 1]
}



#### Main script execution block ###
#### Main script execution block ###
#### Main script execution block ###

# Load existing config or create a default one
$configFilePath = Join-Path $PSScriptRoot "LCLaunchConfig.json"
$config = LoadConfig

# User Configuration Section
# ---------------------------
# Select profile, number of windows, and window size based on user input or default values
$config.selectedProfile = SelectProfile
$config.windowCount = SelectConcurrentWindows
$config.windowSize = SelectWindowSize

# Display the final selections to the user
Write-Host -ForegroundColor Green "Selected Profile: $($config.selectedProfile)"
Write-Host -ForegroundColor Green "Number of Windows: $($config.windowCount)"
Write-Host -ForegroundColor Green "Window Size: $($config.windowSize*100)%"

# Save the current configuration
SaveConfig $config

# Game Launch Preparation Section
# -------------------------------
# Calculate screen resolution and window size
$ScreenWidth = [System.Windows.Forms.Screen]::AllScreens[0].Bounds.Width
$ScreenHeight = [System.Windows.Forms.Screen]::AllScreens[0].Bounds.Height
$WindowWidth = [Math]::Floor($ScreenWidth * $config.windowSize)
$WindowHeight = [Math]::Floor($ScreenHeight * $config.windowSize)

# Prepare arguments for game launch
$arguments = PrepareGameLaunchArguments -Width $WindowWidth -Height $WindowHeight -Config $config

# Launch the Game
# ----------------
foreach ($i in 1..$config.windowCount){
    Write-Host "Starting game: $i"
    Start-Process $config.exePath -ArgumentList $arguments
    Start-Sleep -Milliseconds 2000  # Brief pause between launches
}

# Wait for user input before exiting the script
Read-Host
