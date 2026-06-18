#!/usr/bin/env bash
# =====================================================================
#  db-tt.sh - erzeugt ein linq2db-T4-Template (.tt) fuer eine SQLite-DB,
#  mit RELATIVEM Pfad (behebt den haeufigen PA-Fehler: absoluter C:\...-Pfad).
#  In Visual Studio: Rechtsklick auf das .tt -> "Run Custom Tool" -> Modell-Klassen.
#
#  Verwendung:
#    ./db-tt.sh <ziel-ordner> <pfad/zur.db> [Namespace] [tt-name]
#  Beispiel:
#    ./db-tt.sh ./Fotos/Fotos ./Fotos/Fotos/photoworld.db DataModels DataModel
# =====================================================================
set -uo pipefail

DIR="${1:-}"; DB="${2:-}"; NS="${3:-DataModels}"; TTNAME="${4:-DataModel}"
{ [ -n "$DIR" ] && [ -n "$DB" ]; } || { echo "Verwendung: ./db-tt.sh <ziel-ordner> <pfad/zur.db> [Namespace] [tt-name]"; exit 1; }
[ -f "$DB" ] || { echo "FEHLER: DB '$DB' nicht gefunden." >&2; exit 1; }
mkdir -p "$DIR"

DBNAME="$(basename "$DB")"
RELDIR="$(python3 -c 'import os,sys;print(os.path.relpath(os.path.dirname(os.path.abspath(sys.argv[1])), os.path.abspath(sys.argv[2])))' "$DB" "$DIR" 2>/dev/null || echo ".")"
[ -n "$RELDIR" ] || RELDIR="."

TTPATH="$DIR/$TTNAME.tt"
[ -e "$TTPATH" ] && { echo "FEHLER: '$TTPATH' existiert bereits." >&2; exit 1; }

# Quoted-Heredoc: $(LinqToDB...) bleibt LITERAL (kein Shell-Expand); Platzhalter danach ersetzen.
cat > "$TTPATH" <<'EOF'
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
EOF
sed -i.bak -e "s|__NS__|$NS|g" -e "s|__RELDIR__|$RELDIR|g" -e "s|__DBNAME__|$DBNAME|g" "$TTPATH" && rm -f "$TTPATH.bak"

echo "  + $TTNAME.tt  (DB relativ: $RELDIR/$DBNAME, namespace $NS)"
cat <<EOF

--> In die .csproj des Projekts (einmalig):
  <ItemGroup>
    <Compile Include="$TTNAME.tt">
      <Generator>TextTemplatingFileGenerator</Generator>
      <LastGenOutput>$TTNAME.generated.cs</LastGenOutput>
    </Compile>
    <Compile Update="$TTNAME.generated.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>$TTNAME.tt</DependentUpon>
    </Compile>
  </ItemGroup>
  <ItemGroup>
    <None Update="$DBNAME"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>
  </ItemGroup>

--> NuGet (falls noch nicht vorhanden):
  dotnet add package linq2db ; dotnet add package linq2db.SQLite ; dotnet add package Microsoft.Data.Sqlite

--> Ausfuehren:
  Visual Studio: Rechtsklick auf $TTNAME.tt -> "Run Custom Tool"  (=> $TTNAME.generated.cs)
  Ohne VS:       ./scaffold-db.sh "$DIR" "$DB" "$NS"   (nutzt linq2db.cli statt T4)
EOF
