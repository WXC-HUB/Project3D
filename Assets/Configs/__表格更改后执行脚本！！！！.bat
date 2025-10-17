@echo off
REM 获取当前 BAT 文件目录
set BASE_DIR=%~dp0

REM 输入目录就是当前目录（Assets/Configs）
set INPUT_DIR=%BASE_DIR%

REM 输出目录相对于项目根目录：Assets/Resources/Configs
set OUTPUT_DIR=%BASE_DIR%..\Resources\Configs

REM 创建输出目录（如果不存在）
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

REM 调用 exe
REM 这里用双引号包裹路径，防止有空格导致参数解析错误
if "%INPUT_DIR:~-1%"=="\" set INPUT_DIR=%INPUT_DIR:~0,-1%
if "%OUTPUT_DIR:~-1%"=="\" set OUTPUT_DIR=%OUTPUT_DIR:~0,-1%
REM echo INPUT_DIR=%INPUT_DIR%
REM echo OUTPUT_DIR=%OUTPUT_DIR%
"%BASE_DIR%export_csv.exe" -i "%INPUT_DIR%" -o "%OUTPUT_DIR%"
REM echo "%BASE_DIR%export_csv.exe" -i "%INPUT_DIR%" -o "%OUTPUT_DIR%"

echo.
echo ===== 导出完成 =====
pause