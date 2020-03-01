set fdir=%WINDIR%\Microsoft.NET\Framework
set csc=%fdir%\v4.0.30319\csc.exe
%csc% /t:exe /out:HardLinkCreator.exe HardLinkCreator.cs
HardLinkCreator HardLinkCreator.exe test_created_hardlink\hardlink.exe
pause