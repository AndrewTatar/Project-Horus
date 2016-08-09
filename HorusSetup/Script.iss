
; # The skeleton Inno Setup script was generated by Visual & Installer for MS Visual Studio
; # Visit http://www.visual-installer.com/ for more details.  
; 

; See the Inno Setup documentation at http://www.jrsoftware.org/ for details on creating script files!  
 

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName ReadIni(SourcePath + "\ProjectMetaData.txt", "application", "name", "Project Horus")
#define FeedbackUrl ReadIni(SourcePath + "\ProjectMetaData.txt", "uninstall", "feedback_url", "unknown")
#define MyAppExeName ReadIni(SourcePath + "\ProjectMetaData.txt", "application", "startup_filename", "unknown")
#define AppCPU ReadIni(SourcePath + "\ProjectMetaData.txt", "platform", "cpu", "unknown")
#define MyAppIconPath ReadIni(SourcePath + "\ProjectMetaData.txt", "application", "icon_path", "")
#define MyAppIconName ReadIni(SourcePath + "\ProjectMetaData.txt", "application", "icon_name", "")
#define ShortcutName ReadIni(SourcePath + "\ProjectMetaData.txt", "application", "shortcut_name", "Project Horus")

[Setup]
AppId=28404119-5266-486e-a10a-8e0c119ac49f
AppName=HorusSetup
AppVersion=1.0
AppPublisher=TeamHorus
AppPublisherURL=https://github.com/AndrewTatar/Project-Horus
AppSupportURL=https://github.com/AndrewTatar/Project-Horus
AppUpdatesURL=https://github.com/AndrewTatar/Project-Horus
DefaultDirName={code:GetPFPath|{#AppCPU}}\Team Horus\Project Horus
DefaultGroupName=Team Horus\Project Horus
InfoBeforeFile=C:\Users\Duane\Source\Repos\Project-Horus\HorusSetup\Resources\readme.txt
OutputDir=C:\Users\Duane\Source\Repos\Project-Horus\HorusSetup\Deployable
OutputBaseFilename=HorusSetup
SetupIconFile=C:\Users\Duane\Source\Repos\Project-Horus\HorusSetup\Resources\Icons\48X48-HORUS-Icon.ico
Compression=lzma
SolidCompression=yes
;#section Association
ChangesAssociations=yes
;#end_section Association

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#MyAppIconPath}{#MyAppIconName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Duane\Source\Repos\Project-Horus\Horus\bin\Debug\Horus.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Duane\Source\Repos\Project-Horus\Horus\*"; DestDir: "{app}\Horus"; Flags: recursesubdirs createallsubdirs
Source: "C:\Users\Duane\Source\Repos\Project-Horus\Horus-Config\*"; DestDir: "{app}\Horus-Config"; Flags: recursesubdirs createallsubdirs
Source: "C:\Users\Duane\Source\Repos\Project-Horus\packages\*"; DestDir: "{app}\packages"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#ShortcutName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppIconName}"
Name: "{commondesktop}\{#ShortcutName}"; Filename: "{app}\Horus\bin\Debug\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\{#MyAppIconName}"
Name: "{commonprograms}\HorusSetup"; Filename: "{app}\Horus.exe"

[Run]
Filename: "{app}\Horus\bin\Debug\{#MyAppExeName}"; Description: "{cm:LaunchProgram, {#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{userappdata}\AppPublisher\AppName\*.*"
Type: dirifempty; Name: "{userappdata}\AppPublisher\AppName"
Type: dirifempty; Name: "userappdata\AppPublisher"

#include "it_download.iss";     

[Code]
var
  Page: TInputQueryWizardPage;

procedure InitializeWizard();
begin
  itd_init;
  itd_downloadafter(wpReady);
  Page := CreateInputQueryPage(wpWelcome,
  'Personal Information (Required)', 'Who are you?',
  'Please specify your name and your email address, then click Next.');
  // Add items (False means it's not a password edit)
  Page.Add('Name:', False);
  Page.Add('Email:', False);

  // Set initial values (optional)
  Page.Values[1] := ExpandConstant('{sysuserinfoname}');

end;

function GetUserName(): string;
begin
result := Page.Values[0];
end;

function GetUserEmail(): string;
begin
result := Page.Values[1];
end;


function GetPFPath(appCpu : string): string;
begin   
  if (appCpu = 'AnyCPU') then
    begin
      if (IsWin64) then
        begin
          Result := ExpandConstant('{pf64}');
        end
      else
        begin
          Result := ExpandConstant('{pf32}');
        end
    end
  else
    if (appCpu = '64bit') then
      begin
        Result := ExpandConstant('{pf64}');
      end
    else
      begin
        Result := ExpandConstant('{pf32}');
      end;
end;


function IsDotNetDetected_da7caa4c_5c42_47d9_88e0_a8c0ed5e1b76(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//    'v4.5.1'        .NET Framework 4.5.1
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var        
    key: string;
    install, release, serviceCount: cardinal;
    check45, check451, success: boolean;
begin
    version := 'v' + version;
    // .NET 4.5 installs as update to .NET 4.0 Full
    if version = 'v4.5' then begin
        version := 'v4\Full';
        check45 := true;
    end else
        check45 := false;

	// .NET 4.5.1 installs as update to .NET 4.0 Full
    if version = 'v4.5.1' then begin
        version := 'v4\Full';
        check451 := true;
    end else
        check451 := false;
		
    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0/4.5/4.5.1 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 uses additional value Release
    if check45 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= 378389);
    end;
	
	// .NET 4.5.1 uses additional value Release
    if check451 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= 378675);
    end;

    result := success and (install = 1) and (serviceCount >= service);

end;

//#end_section PrerequisiteScripts

function InitializeSetup(): Boolean;
var        
    ErrCode: integer;
    FinalResult: boolean;
	

begin
  FinalResult := true;


if (not IsDotNetDetected_da7caa4c_5c42_47d9_88e0_a8c0ed5e1b76('4.5.1', 0)) then begin
          
    itd_addfile('https://go.microsoft.com/fwlink/?LinkId=225702', expandconstant('{tmp}\dotnet.exe'));
    
end;

//#end_section PrerequisiteInit
  result := FinalResult;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
    ErrCode: integer;
	UserName, UserEmail: String;
begin
 if CurStep=ssInstall then 
   begin 

WizardForm.StatusLabel.Caption := '';
WizardForm.StatusLabel.Caption := 'Please wait while setup install prerequisite: .net framework...';
ShellExec('open', ExpandConstant('{tmp}\dotnet.exe'), '', '', SW_SHOW, ewWaitUntilTerminated, ErrCode);

if (not IsDotNetDetected_da7caa4c_5c42_47d9_88e0_a8c0ed5e1b76('4.5.1', 0)) then begin
    MsgBox('Automatic installation of .net framework failed. Please try manual installation.', mbInformation, MB_OK);
    Abort();
end;
WizardForm.StatusLabel.Caption := '';
//#end_section PrerequisiteInstall
   end;
	
	UserName := GetUserName;

	UserEmail := GetUserEmail;

    RegWriteStringValue(HKEY_CURRENT_USER, 'SOFTWARE\TeamHorus\Project Horus',
      'User Name', UserName);

	RegWriteStringValue(HKEY_CURRENT_USER, 'SOFTWARE\TeamHorus\Project Horus',
      'User Email', UserEmail);

end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
    ErrCode: integer;
    Url: string;
begin
    Url := '{#FeedbackUrl}';
    if (CurUninstallStep=usDone) then
    begin
        if (Url <> '') then
          ShellExec('open', Url, '', '', SW_SHOW, ewNoWait, ErrCode);
    end;
end;