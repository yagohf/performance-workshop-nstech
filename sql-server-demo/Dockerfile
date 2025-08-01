# Usa a imagem oficial mais recente do SQL Server 2022 da Microsoft
FROM mcr.microsoft.com/mssql/server:2022-latest

# Define variáveis de ambiente. A senha do SA será passada no comando 'docker run'.
# Aceitamos o contrato de licença e usamos a edição Developer, que é gratuita.
ENV ACCEPT_EULA=Y
ENV MSSQL_PID=Developer

# Define o diretório de trabalho dentro do container
WORKDIR /usr/src/app/db-scripts

# Copia os scripts SQL e o script de configuração para dentro do container
COPY ./criação_tabelas.sql .
COPY ./carga_dados.sql .
COPY ./configure-db.sh .

# O comando que será executado quando o container iniciar.
# Ele executa nosso script de configuração em background (&) com o bash
# e inicia o processo do SQL Server em foreground, mantendo o container vivo.
CMD /bin/bash -c "/bin/bash ./configure-db.sh & /opt/mssql/bin/sqlservr"