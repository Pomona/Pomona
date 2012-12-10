@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION

echo Performing some hardcore file system nuking!
FOR /R . %%X IN (bin obj) DO (
	SET retard=%%X
	SET cretin=!retard:\Umbraco\bin=!
	IF !cretin! == %%X (
		RD /S /Q "%%X" 2> NUL
	)
)