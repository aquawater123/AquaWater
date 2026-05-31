# AquaWater 调洪演算程序 Web 版 — Windows Server 部署指南

## 1. 环境准备

在服务器上安装 Python 3.10+：
- 下载：https://www.python.org/downloads/
- 安装时勾选 "Add Python to PATH"

## 2. 获取代码

```powershell
git clone https://github.com/aquawater123/AquaWater.git
cd AquaWater\aquawater-web
```

## 3. 安装依赖

```powershell
pip install -r requirements.txt
```

## 4. 启动服务

```powershell
python app.py
```

服务运行在 http://0.0.0.0:8090

## 5. 配置防火墙

```powershell
# 以管理员身份运行
netsh advfirewall firewall add rule name="AquaWater" dir=in action=allow protocol=TCP localport=8090
```

## 6. 设为开机自启（推荐 NSSM）

下载 NSSM：https://nssm.cc/download
```powershell
nssm install AquaWater
# Application: C:\Python311\python.exe
# Arguments: app.py
# Start directory: C:\AquaWater\aquawater-web
nssm start AquaWater
```

## 7. 访问

内网访问：http://服务器IP:8090
公网访问：需要在路由器/防火墙做端口映射

## 安全提示

- 已关闭 debug 模式
- 已启用速率限制（防滥用）
- 上传大小限制 16MB
- 如需公网访问，建议前面加 nginx/Caddy 做 HTTPS
