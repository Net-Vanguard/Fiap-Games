@echo off
setlocal enabledelayedexpansion

REM Lê o .env linha por linha e seta como variável
for /f "usebackq tokens=1,2 delims==" %%a in (".env") do (
  set "%%a=%%b"
)

REM Debug opcional: veja se a variável foi carregada
echo SONAR_TOKEN = [%SONAR_TOKEN%]

REM Checa se a variável foi de fato carregada
if "%SONAR_TOKEN%"=="" (
  echo ❌ ERRO: Variável SONAR_TOKEN não encontrada ou vazia no arquivo .env
  pause
  exit /b 1
)

REM Executa análise com token carregado corretamente
dotnet sonarscanner begin ^
  /k:"fiap-back" ^
  /d:sonar.host.url="http://localhost:9001" ^
  /d:sonar.login=%SONAR_TOKEN%

dotnet build

dotnet sonarscanner end /d:sonar.login=%SONAR_TOKEN%
pause
