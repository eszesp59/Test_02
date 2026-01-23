rem @echo off
rem SonarQube analizis
rem lokalis valtozokat korrketul allitani!
rem SLN == cmd-tol relativ
rem
rem Sonar Scanner letoltheto:
rem   https://docs.sonarqube.org/latest/analysis/scan/sonarscanner-for-msbuild/
rem
cls
set RUNNER_EXE="c:\Program Files\SonarScanner\SonarScanner.MSBuild.exe" 
set MS_BUILD_EXE="c:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
set JAVA_HOME=c:\Program Files\Java\jdk-17
;
set SONARQUBE_PROJECT_KEY="UMS:PTB-Main-Dev"
set SONARQUBE_PROJECT_NAME="UMS"
set SONARQUBE_PROJECT_VERSION="1.0"
;
set SONAR_HOST=/d:sonar.host.url=http://tadsonar.tad.mavinformatika.hu:9000
set SONAR_UID=/d:sonar.login=eszesp
set SONAR_PWD=/d:sonar.password=XXXX
set SONAR_EXCLUDE=/d:sonar.exclusions="**/Scripts/*.js, **/Scripts/esm/*.js, **/Scripts/esm/**.js, **/Scripts/umd/*.js, **/Scripts/jqwidgets/**.js"
;
set SONAR_PROPERTIES=%SONAR_HOST% %SONAR_UID% %SONAR_PWD% %SONAR_EXCLUDE%  /d:sonar.verbose=true /d:sonar.lang.patterns.plsqlopen=.off /d:sonar.sql.dialect=tsql
;
set SLN="UMS.sln"
;
%RUNNER_EXE% begin /k:%SONARQUBE_PROJECT_KEY% /n:%SONARQUBE_PROJECT_NAME% /v:%SONARQUBE_PROJECT_VERSION%  %SONAR_PROPERTIES%
%MS_BUILD_EXE% %SLN%  /t:Rebuild -m
%RUNNER_EXE% end %SONAR_UID% %SONAR_PWD%
