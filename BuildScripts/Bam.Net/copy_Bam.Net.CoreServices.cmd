@echo off

SET LIB=net40
SET VER=v4.0
SET NEXT=NEXT
GOTO COPY

:NEXT
SET LIB=net45
SET VER=v4.5
SET NEXT=END
GOTO COPY

:COPY
MD Bam.Net.CoreServices\lib\%LIB%
copy /Y .\BuildOutput\Release\%VER%\Bam.Net.CoreServices.dll Bam.Net.CoreServices\lib\%LIB%\Bam.Net.CoreServices.dll
copy /Y .\BuildOutput\Release\%VER%\Bam.Net.CoreServices.xml Bam.Net.CoreServices\lib\%LIB%\Bam.Net.CoreServices.xml
GOTO %NEXT%

:END


