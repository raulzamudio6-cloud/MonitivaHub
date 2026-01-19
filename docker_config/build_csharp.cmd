REM #####################################################################

echo namespace eu.advapay.core.hub { public static class CiInfo { public const string BuildTag = ("%BUILD_TAG%" == "" ? @"untagged" : "%BUILD_TAG%") + " / %DATE%-%TIME%"; } } > "..\CiInfo.cs"

ECHO ************************* Hub ************************
dotnet publish ..\Hub.csproj -r linux-x64 -p:PublishSingleFile=true -c Release --nologo --output bin
if not %ErrorLevel% equ 0 (echo BuildError:%ErrorLevel% && exit /B %ErrorLevel% )

ECHO /application/Hub > .\entrypoint.sh


