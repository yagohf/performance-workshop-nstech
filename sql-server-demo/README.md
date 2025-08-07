# SQL Server Performance Demo

Este projeto utiliza Docker para criar um ambiente de banco de dados SQL Server para demonstrações de performance. O container, ao ser iniciado, cria um banco de dados chamado `performance-demo-db`, define suas tabelas e as popula com uma carga de dados inicial.

## Pré-requisitos

- Docker instalado e em execução na sua máquina.

## Como utilizar

### 1. Construa a imagem Docker

Execute o comando abaixo no seu terminal, na raiz deste projeto, para construir a imagem Docker. A tag `-t performance-demo-db` nomeia a imagem para facilitar sua utilização nos próximos passos.

```bash
docker build -t performance-demo-db .
```

### 2. Execute o container

Após a construção da imagem, execute o seguinte comando para iniciar o container. Este comando também define a senha para o usuário `sa` e mapeia a porta do SQL Server para a sua máquina local.

```bash
docker run --name sql-server-demo -e "MSSQL_SA_PASSWORD=Demo@App@2025" -p 1433:1433 -d performance-demo-db
```

**Parâmetros do comando:**

- `--name sql-server-demo`: Nomeia o container para que possamos referenciá-lo facilmente.
- `-e "MSSQL_SA_PASSWORD=Demo@App@2025"`: Define a senha do usuário `sa` (System Administrator). **É fundamental manter as aspas duplas**.
- `-p 1433:1433`: Mapeia a porta 1433 do container (porta padrão do SQL Server) para a porta 1433 da sua máquina.
- `-d`: Executa o container em modo "detached" (em background).
- `performance-demo-db`: O nome da imagem que criamos no passo anterior.

### 3. Verifique a inicialização

O processo de criação e carga do banco de dados pode levar alguns minutos. Você pode acompanhar o progresso visualizando os logs do container.

```bash
docker logs -f sql-server-demo
```

Quando a mensagem "✅ Banco de dados [performance-demo-db] configurado com sucesso!" for exibida, o banco de dados estará pronto para ser acessado.

### 4. Acessando o banco de dados

Você pode se conectar ao banco de dados utilizando sua ferramenta de preferência (DBeaver, Azure Data Studio, SSMS, etc.) com as seguintes credenciais:

- **Servidor:** `localhost`
- **Porta:** `1433`
- **Usuário:** `sa`
- **Senha:** `Demo@App@2025`
- **Banco de dados (Database):** `performance-demo-db`

## Comandos úteis do Docker

- **Parar o container:**
  ```bash
  docker stop sql-server-demo
  ```

- **Iniciar o container novamente:**
  ```bash
  docker start sql-server-demo
  ```

- **Remover o container (após parado):**
  ```bash
  docker rm sql-server-demo
  ```

- **Listar containers em execução:**
  ```bash
  docker ps
  ```
