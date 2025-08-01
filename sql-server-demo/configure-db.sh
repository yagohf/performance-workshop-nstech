#!/bin/bash

# Este script aguarda um tempo para o SQL Server iniciar e então
# executa os scripts de criação e carga do banco de dados.

# Aguarda 35 segundos. Este tempo é necessário para que o processo
# do SQL Server esteja totalmente no ar e pronto para aceitar conexões.
echo "⏳ Aguardando 35s para o SQL Server iniciar..."
sleep 35s

# Caminho para o sqlcmd na versão mais recente da imagem
SQLCMD_PATH="/opt/mssql-tools18/bin/sqlcmd"

# Parâmetros adicionais para lidar com a criptografia SSL/TLS
# -N Criptografa a conexão
# -C Confia no certificado do servidor (evita o erro "self-signed certificate")
SQL_CONN_OPTS="-N -C"

# Executa os scripts usando a ferramenta de linha de comando 'sqlcmd'
# A senha é passada pela variável de ambiente $MSSQL_SA_PASSWORD

echo "▶️  Executando script [criação_tabelas.sql]..."
$SQLCMD_PATH -S localhost -U sa -P "$MSSQL_SA_PASSWORD" $SQL_CONN_OPTS -d master -i ./criação_tabelas.sql

echo "▶️  Executando script [carga_dados.sql]. Isso pode levar vários minutos..."
# CORREÇÃO APLICADA AQUI: Variável "$MSSQL_SA_PASSWORD" com dois 'S'
$SQLCMD_PATH -S localhost -U sa -P "$MSSQL_SA_PASSWORD" $SQL_CONN_OPTS -d master -i ./carga_dados.sql

echo "✅ Banco de dados [performance-demo-db] configurado com sucesso!"