@echo off
REM cleanup.bat - Script to remove obsolete files from the refactoring

echo.
echo ====================================================================
echo   Cleanup Script - Removing Obsolete CQRS Files
echo ====================================================================
echo.

REM Function to safely delete a file
set "deleted_count=0"
set "not_found_count=0"

echo [1/4] Removing custom query...
if exist "Domain\Queries\GetBooksByClientQuery.cs" (
    echo    [OK] Deleting: Domain\Queries\GetBooksByClientQuery.cs
    del "Domain\Queries\GetBooksByClientQuery.cs"
    set /a deleted_count+=1
) else (
    echo    [SKIP] Not found: Domain\Queries\GetBooksByClientQuery.cs
    set /a not_found_count+=1
)
echo.

echo [2/4] Removing custom handler...
if exist "Domain\Handlers\GetBooksByClientHandler.cs" (
    echo    [OK] Deleting: Domain\Handlers\GetBooksByClientHandler.cs
    del "Domain\Handlers\GetBooksByClientHandler.cs"
    set /a deleted_count+=1
) else (
    echo    [SKIP] Not found: Domain\Handlers\GetBooksByClientHandler.cs
    set /a not_found_count+=1
)
echo.

echo [3/4] Checking BookDto usage...
findstr /S /I /C:"BookDto" Domain\*.cs API\*.cs > temp_bookdto_check.txt 2>nul
findstr /V /I /C:"BookDto.cs" temp_bookdto_check.txt | findstr /V /I /C:"BookMappingProfile.cs" > temp_bookdto_usage.txt 2>nul
for /f %%A in ("temp_bookdto_usage.txt") do set size=%%~zA
if %size% gtr 0 (
    echo    [SKIP] BookDto is used elsewhere - keeping file
) else (
    if exist "Domain\DTOs\BookDto.cs" (
        echo    [OK] Deleting: Domain\DTOs\BookDto.cs
        del "Domain\DTOs\BookDto.cs"
        set /a deleted_count+=1
    ) else (
        echo    [SKIP] Not found: Domain\DTOs\BookDto.cs
        set /a not_found_count+=1
    )
)
del temp_bookdto_check.txt 2>nul
del temp_bookdto_usage.txt 2>nul
echo.

echo [4/4] Checking BookMappingProfile usage...
findstr /S /I /C:"BookMappingProfile" Domain\*.cs API\*.cs > temp_mapping_check.txt 2>nul
findstr /V /I /C:"BookMappingProfile.cs" temp_mapping_check.txt > temp_mapping_usage.txt 2>nul
for /f %%A in ("temp_mapping_usage.txt") do set size=%%~zA
if %size% gtr 0 (
    echo    [SKIP] BookMappingProfile is referenced elsewhere - keeping file
) else (
    if exist "Domain\Mappings\BookMappingProfile.cs" (
        echo    [OK] Deleting: Domain\Mappings\BookMappingProfile.cs
        del "Domain\Mappings\BookMappingProfile.cs"
        set /a deleted_count+=1
    ) else (
        echo    [SKIP] Not found: Domain\Mappings\BookMappingProfile.cs
        set /a not_found_count+=1
    )
)
del temp_mapping_check.txt 2>nul
del temp_mapping_usage.txt 2>nul
echo.

echo ====================================================================
echo   Cleanup Summary
echo ====================================================================
echo   Files deleted: %deleted_count%
echo   Files not found: %not_found_count%
echo ====================================================================
echo.
echo Next steps:
echo   1. Copy refactored controllers to API\Controllers\
echo   2. Run: dotnet clean
echo   3. Run: dotnet build
echo   4. Test the new endpoints in Swagger
echo.
echo Press any key to exit...
pause >nul
