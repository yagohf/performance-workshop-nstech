# APM Demo - Ambiente de Observabilidade Local

Este projeto contém a configuração para executar um ambiente de observabilidade local utilizando Docker Compose. A stack inclui:

*   **Jaeger**: Para tracing distribuído (APM).
*   **Prometheus**: Para coleta de métricas.
*   **Grafana**: Para visualização de métricas e traces.

## Pré-requisitos

*   [Docker](https://docs.docker.com/get-docker/)
*   [Docker Compose](https://docs.docker.com/compose/install/)

## Como Executar

1.  **Navegue até a pasta `apm-demo`:**

    ```bash
    cd apm-demo
    ```

2.  **Inicie os contêineres Docker:**

    ```bash
    docker-compose up -d
    ```

    Este comando irá baixar as imagens necessárias e iniciar os serviços em background.

## Serviços Disponíveis

Após a execução, os seguintes serviços estarão disponíveis nos seus respectivos ports:

*   **Jaeger UI**: [http://localhost:16686](http://localhost:16686)
*   **Prometheus UI**: [http://localhost:9090](http://localhost:9090)
*   **Grafana UI**: [http://localhost:3000](http://localhost:3000)
    *   **Login**: `admin`
    *   **Senha**: `admin`

## Configuração

*   **Prometheus (`prometheus.yml`)**: Configurado para fazer o scrape de métricas de aplicações. Adicione novos `targets` em `scrape_configs` para monitorar outras aplicações.
*   **Grafana**:
    *   **Datasources (`datasources.yml`)**: O Prometheus já vem pré-configurado como um datasource.
    *   **Dashboards (`dashboards.yml` e `api-performance.json`)**: Um dashboard de exemplo para performance de API já está incluído. Para adicionar novos dashboards, adicione o arquivo `.json` na pasta `grafana/dashboards` e reinicie o Grafana, ou crie diretamente pela UI.

## Como Parar o Ambiente

Para parar e remover os contêineres, execute:

```bash
docker-compose down
```
