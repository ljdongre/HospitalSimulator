
cmd /C "nuget restore"
"C:\Program Files (x86)\MSBuild\12.0\Bin\amd64\MSBuild.exe" HospitalSimulator.sln /p:Configuration=Release /p:Platform="Any CPU"