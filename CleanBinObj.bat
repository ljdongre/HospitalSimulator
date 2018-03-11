@echo off
for /d /r . %%d in (bin,obj,packages) do @if exist "%%d" rd /s/q "%%d"


IF EXIST .\DeploymentFolder\Client rd /s /q .\DeploymentFolder\Client
IF EXIST .\DeploymentFolder\Server rd /s /q .\DeploymentFolder\Server