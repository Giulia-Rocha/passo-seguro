$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
Set-Location -LiteralPath $projectRoot
if (-not (Test-Path -LiteralPath '.env')) {
    $databasePassword = [guid]::NewGuid().ToString('N')
    $tokenKey = [guid]::NewGuid().ToString('N') + [guid]::NewGuid().ToString('N')
    $demoPassword = [guid]::NewGuid().ToString('N') + 'Aa1!'
    @("POSTGRES_DB=passoseguro", "POSTGRES_USER=passoseguro", "POSTGRES_PASSWORD=$databasePassword", "JWT_KEY=$tokenKey", "DEMO_PASSWORD=$demoPassword") | Set-Content -LiteralPath '.env' -Encoding ascii
    Write-Host 'Configuração local criada em .env (não versionada). Consulte DEMO_PASSWORD para entrar com as contas fictícias.'
}
docker compose up -d db --wait
if ($LASTEXITCODE -ne 0) { throw 'Falha ao iniciar PostgreSQL.' }
docker compose build api
if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar API.' }
docker compose run --rm api --migrate
if ($LASTEXITCODE -ne 0) { throw 'Falha ao aplicar migrações/seed.' }
docker compose up -d api
if ($LASTEXITCODE -ne 0) { throw 'Falha ao iniciar API.' }
Write-Host 'API: http://localhost:5080/swagger'
