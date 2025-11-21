编译指南 (Build Instructions)
================================

如果你在运行 build.bat 时遇到 "'dotnet' 不是内部或外部命令" 的错误：

原因：
你的电脑上没有安装 .NET SDK，这是编译 C# 代码所必须的工具。

解决方法：

1. 下载 .NET SDK
   访问微软官网：https://dotnet.microsoft.com/download/dotnet
   下载并安装最新的 .NET SDK (推荐 .NET 6.0 或 .NET 8.0)。
   选择 Windows x64 版本。

2. 安装
   运行下载的安装程序进行安装。

3. 重启
   安装完成后，**必须重启 VS Code** (完全关闭再打开)，否则终端无法识别新安装的命令。

4. 重新编译
   重启后，再次运行 build.bat 即可。
