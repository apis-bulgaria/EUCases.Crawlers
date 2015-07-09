@echo off

setlocal
for /F "tokens=1*" %%i in ("%USERNAME%") do set SPACE=%%j
if defined SPACE (
  echo *** ERROR ***
  echo The Windows username must not contain spaces. Setting the environment variable USERNAME to a name without spaces is not sufficient to run Hadoop correclty.
  exit /B 1
)
endlocal

pushd %~dp0
cd ..
rem necessary for Nutch to locate winutils.exe
set HADOOP_HOME=%cd%\hadoop-2.4.0-win64
rem necessary for winutils.exe to find its dlls
set PATH=%PATH%;%cd%\hadoop-2.4.0-win64\bin
popd 
