# Comandos úteis

## 1. Comando para construir a imagem Docker
docker build -t performance-demo-db .

## 2. Comando para executar o container
docker run --name sql-server-demo -e "MSSQL_SA_PASSWORD=Demo@App@2025" -p 1433:1433 -d performance-demo-db

## 3. Comando para verificar os logs dos scripts sendo executados durante a subida do container
docker logs -f sql-server-demo

## 4. Parar o container
docker stop sql-server-demo

## 5. Deletar o container
docker rm sql-server-demo