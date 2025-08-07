# Workshop de Performance de Aplicações - NS Tech

Este repositório contém o material prático utilizado no workshop de performance de aplicações realizado na [NS Tech](https://nstech.com.br/?scLang=pt-BR) em 07 de agosto de 2025.

O objetivo deste workshop foi demonstrar, de forma prática, como identificar e otimizar gargalos de performance em uma aplicação, utilizando ferramentas de mercado e boas práticas de desenvolvimento.

## Visão geral do repositório

Este repositório está dividido em quatro partes principais, cada uma em sua respectiva pasta:

1.  **`app-demo`**: Uma API .NET 8 que serve como objeto de estudo para as otimizações de performance.
2.  **`sql-server-demo`**: Um ambiente com Docker para subir um banco de dados SQL Server 2022 com uma carga de dados pré-definida.
3.  **`load-test-demo`**: Um script de teste de carga com K6 para estressar a API e medir os ganhos de performance.
4.  **`apm-demo`**: Uma stack de observabilidade com Jaeger, Prometheus e Grafana para monitoramento da aplicação.

## Como Utilizar este Repositório

Para reproduzir o ambiente do workshop, siga os passos abaixo. Cada pasta contém um `README.md` com instruções mais detalhadas.

### 1. Pré-requisitos

- [Docker](https://docs.docker.com/get-docker/) e [Docker Compose](https://docs.docker.com/compose/install/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [K6](https://k6.io/docs/getting-started/installation/)

### 2. Subindo o Banco de Dados

O primeiro passo é iniciar o banco de dados. Para isso, utilize o Docker.

```bash
cd sql-server-demo
docker build -t performance-demo-db .
docker run --name sql-server-demo -e "MSSQL_SA_PASSWORD=Demo@App@2025" -p 1433:1433 -d performance-demo-db
```

Aguarde alguns minutos para o banco de dados ser totalmente inicializado. Você pode acompanhar o progresso com o comando `docker logs -f sql-server-demo`.

### 3. Subindo a Stack de Observabilidade (APM)

Com o banco de dados no ar, o próximo passo é iniciar o ambiente de monitoramento.

```bash
cd apm-demo
docker-compose up -d
```

Os seguintes serviços estarão disponíveis:

- **Jaeger UI**: [http://localhost:16686](http://localhost:16686)
- **Prometheus UI**: [http://localhost:9090](http://localhost:9090)
- **Grafana UI**: [http://localhost:3000](http://localhost:3000) (login: `admin`, senha: `admin`)

### 4. Executando a Aplicação

Agora, vamos executar a API.

```bash
cd app-demo/PerformanceApi
dotnet run
```

A API estará disponível em `http://localhost:5227` (ou outra porta, verifique o console).

### 5. Executando o Teste de Carga

Com a API e o banco de dados em execução, utilize o K6 para simular o acesso de usuários e estressar a aplicação.

```bash
cd load-test-demo
k6 run load-test.js
```

## Otimizações de Performance

A pasta `app-demo` contém o arquivo `IMPROVEMENTS_STEP_BY_STEP.md`, que detalha o passo a passo das otimizações realizadas durante o workshop. Sinta-se à vontade para explorar e aplicar as melhorias no código da API.

## Conclusão

Este repositório é um material de apoio para estudos e experimentações em performance de aplicações. Explore o código, execute os testes e observe os resultados nas ferramentas de monitoramento.

Em caso de dúvidas, consulte os `README.md` de cada pasta.

Fiquem à vontade para contribuir, compartilhar e melhorar essa base de código e a apresentação!

Abraços, 
<br>Yago<br><yagoferreira21@gmail.com>
