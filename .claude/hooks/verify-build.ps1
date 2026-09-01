# Hook de Stop: verifica o build .NET ao fim de cada turno.
# Guardado: só age quando já existe uma solution REAL (ignora a pasta interna .vs do
# Visual Studio). Enquanto o projeto for greenfield (sem .sln/.slnx), sai limpo e não faz nada.
# Quando houver build quebrado, bloqueia o Stop e devolve o erro ao modelo para corrigir.
$ErrorActionPreference = 'SilentlyContinue'

$sln = Get-ChildItem -Recurse -Depth 4 -Include *.sln, *.slnx -File |
    Where-Object { $_.FullName -notmatch '\\\.vs\\' } |
    Select-Object -First 1

if (-not $sln) { exit 0 }  # ainda não há o que compilar

$output = dotnet build $sln.FullName --nologo 2>&1 | Out-String
if ($LASTEXITCODE -ne 0) {
    $reason = "Build .NET falhou (TreatWarningsAsErrors). Corrija antes de finalizar:`n$output"
    @{ decision = 'block'; reason = $reason } | ConvertTo-Json -Compress
}
exit 0
