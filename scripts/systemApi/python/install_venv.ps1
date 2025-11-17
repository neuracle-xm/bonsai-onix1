[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# 1. 设置虚拟环境目录
$venvDir = "venv"

# 2. 检查是否已存在虚拟环境
if (Test-Path $venvDir) {
    Write-Host "虚拟环境已存在，跳过创建..."
}
else {
    Write-Host "创建虚拟环境..."
    python -m venv $venvDir
}

# 3. 激活虚拟环境
Write-Host "激活虚拟环境..."
$activateScript = "$venvDir\Scripts\Activate.ps1"
. $activateScript

# 4. 安装dependencies
Write-Host "安装中..."
pip install "python-osc>=1.9.3,<2.0.0"
