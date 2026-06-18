# =====================================================================
#  db-tt.ps1 - erzeugt ein linq2db-T4-Template (.tt) fuer eine SQLite-DB
#  (wird von db-tt.bat aufgerufen). Relativer Pfad statt absolutem C:\...
# =====================================================================
param([string]$Dir = "", [string]$Db = "", [string]$Ns = "DataModels", [string]$TtName = "DataModel")

if (-not $Dir -or -not $Db) { Write-Host "Verwendung: db-tt.bat <ziel-ordner> <pfad\zur.db> [Namespace] [tt-name]"; exit 1 }
if (-not (Test-Path $Db)) { Write-Host "FEHLER: DB '$Db' nicht gefunden."; exit 1 }
New-Item -ItemType Directory -Force -Path $Dir | Out-Null

# Relativen Ordner berechnen (funktioniert auch in Windows PowerShell 5.1 via Uri)
function Get-RelDir($from, $to) {
    $fromFull = (Resolve-Path $from).Path
    if (-not $fromFull.EndsWith([IO.Path]::DirectorySeparatorChar)) { $fromFull += [IO.Path]::DirectorySeparatorChar }
    $toFull = (Resolve-Path $to).Path
    if (-not $toFull.EndsWith([IO.Path]::DirectorySeparatorChar)) { $toFull += [IO.Path]::DirectorySeparatorChar }
    $rel = (New-Object Uri $fromFull).MakeRelativeUri((New-Object Uri $toFull)).ToString()
    $rel = [Uri]::UnescapeDataString($rel).TrimEnd('/').Replace('/', [IO.Path]::DirectorySeparatorChar)
    if (-not $rel) { $rel = "." }
    return $rel
}

$dbName = Split-Path $Db -Leaf
$dbDir = Split-Path (Resolve-Path $Db).Path -Parent
$relDir = Get-RelDir $Dir $dbDir
$ttPath = Join-Path $Dir "$TtName.tt"
if (Test-Path $ttPath) { Write-Host "FEHLER: '$ttPath' existiert bereits."; exit 1 }

# Single-Quote-Here-String: $(LinqToDB...) bleibt literal; Platzhalter danach ersetzen.
$tt = @'
<#@ template language="C#" debug="True" hostSpecific="True"                              #>
<#@ CleanupBehavior processor="T4VSHost" CleanupAfterProcessingtemplate="true"           #>
<#@ output extension=".generated.cs"                                                     #>
<#@ include file="$(LinqToDBT4SQLiteTemplatesPath)LinqToDB.SQLite.ttinclude" once="true" #>
<#@ include file="$(LinqToDBT4TemplatesPath)PluralizationService.ttinclude"  once="true" #>
<#
    NamespaceName        = "__NS__";
    GenerateSchemaAsType = true;

    // Relativer Pfad: (Ordner der DB, Dateiname) - KEIN absoluter C:\...-Pfad!
    LoadSQLiteMetadata(@"__RELDIR__", "__DBNAME__");

    GenerateModel();
#>
'@
$tt = $tt.Replace("__NS__", $Ns).Replace("__RELDIR__", $relDir).Replace("__DBNAME__", $dbName)
Set-Content -Path $ttPath -Value $tt

Write-Host "  + $TtName.tt  (DB relativ: $relDir\$dbName, namespace $Ns)"
Write-Host ""
Write-Host "--> In die .csproj des Projekts (einmalig):"
Write-Output @"
  <ItemGroup>
    <Compile Include="$TtName.tt">
      <Generator>TextTemplatingFileGenerator</Generator>
      <LastGenOutput>$TtName.generated.cs</LastGenOutput>
    </Compile>
    <Compile Update="$TtName.generated.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>$TtName.tt</DependentUpon>
    </Compile>
  </ItemGroup>
  <ItemGroup>
    <None Update="$dbName"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>
  </ItemGroup>
"@
Write-Host ""
Write-Host "--> NuGet: dotnet add package linq2db ; dotnet add package linq2db.SQLite ; dotnet add package Microsoft.Data.Sqlite"
Write-Host "--> Ausfuehren: Visual Studio -> Rechtsklick auf $TtName.tt -> 'Run Custom Tool'"
