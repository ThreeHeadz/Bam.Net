@echo on
SET CONFIG=%1
IF [%1]==[] SET CONFIG=Release
SET LIB=net462
SET VER=v4.6.2

MD Bam.Net.Analytics\lib\%LIB%
copy /Y .\BuildOutput\%CONFIG%\%VER%\Bam.Net.Analytics.dll Bam.Net.Analytics\lib\%LIB%\Bam.Net.Analytics.dll
copy /Y .\BuildOutput\%CONFIG%\%VER%\Bam.Net.Analytics.xml Bam.Net.Analytics\lib\%LIB%\Bam.Net.Analytics.xml