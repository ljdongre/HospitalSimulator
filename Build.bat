
cmd /C "nuget restore"
"C:\Program Files (x86)\MSBuild\12.0\Bin\amd64\MSBuild.exe" HospitalSimulator.sln /p:Configuration=Release /p:Platform="Any CPU"
pushd ResourceManagerTests\bin\Release
cmd /c "C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\MSTest.exe" /testcontainer:ResourceManagerTests.dll
popd

