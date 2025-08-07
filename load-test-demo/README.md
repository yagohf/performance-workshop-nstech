# Teste de Carga para API de Extrato

Este projeto contém um script de teste de carga desenvolvido com [k6](https://k6.io/) para avaliar o desempenho e a confiabilidade de um endpoint de API de extrato financeiro.

O teste simula múltiplos usuários solicitando seus extratos de conta durante um período de três meses.

## Índice

- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Executando o Teste](#executando-o-teste)
- [Entendendo o Script de Teste (`load-test.js`)](#entendendo-o-script-de-teste-load-testjs)
  - [Configuração](#configuração)
  - [Perfil de Carga](#perfil-de-carga)
  - [Cenário de Teste](#cenário-de-teste)
  - [Checagens e Limites (Thresholds)](#checagens-e-limites-thresholds)
- [Interpretando os Resultados](#interpretando-os-resultados)

## Pré-requisitos

1.  **k6:** Você precisa ter o binário do k6 instalado no seu sistema.
2.  **API em Execução:** A aplicação da API alvo deve estar em execução e acessível a partir da máquina onde você executará o teste. Por padrão, o script aponta para `http://localhost:5227`.

## Instalação

Para instalar o k6, siga as instruções oficiais para o seu sistema operacional.

**macOS**
```bash
brew install k6
```

**Linux (Debian/Ubuntu)**
```bash
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

**Windows**
```powershell
winget install k6.k6
# ou usando Chocolatey
choco install k6
```

Para outros sistemas, por favor, consulte o [Guia Oficial de Instalação do k6](https://grafana.com/docs/k6/latest/set-up/install-k6/).

## Executando o Teste

1.  Navegue até o diretório do projeto no seu terminal:
    ```bash
    cd load-test-demo
    ```
2.  Garanta que sua API alvo esteja em execução. Se não estiver em `http://localhost:5227`, atualize a constante `BASE_URL` em `load-test.js`.

3.  Execute o teste usando o seguinte comando:
    ```bash
    k6 run load-test.js
    ```

## Entendendo o Script de Teste (`load-test.js`)

### Configuração

-   `BASE_URL`: A URL base da API sob teste.

### Perfil de Carga

A carga do teste é definida no objeto `options.stages`:
1.  **Rampa de Subida (Ramp-up):** O teste começa com 0 usuários virtuais (VUs) e aumenta gradualmente para 50 VUs ao longo de 5 segundos. Isso simula a chegada de usuários ao serviço.
2.  **Carga Sustentada:** O teste mantém uma carga constante de 50 VUs por 30 segundos. Esta é a fase principal de estresse para observar como o sistema se comporta sob pressão.

### Cenário de Teste

Cada usuário virtual executa a função `default` repetidamente. Em cada iteração, o VU:
1.  Gera um `accountId` aleatório entre 1 e 100 para simular requisições de diferentes usuários.
2.  Calcula um intervalo de datas dos últimos três meses.
3.  Constrói a URL completa para o endpoint da API de extrato.
4.  Envia uma requisição HTTP GET para o endpoint.
5.  Realiza checagens na resposta.

### Checagens e Limites (Thresholds)

-   **Checagens:** Após cada requisição, o script verifica três condições:
    -   O código de status HTTP é `200 OK`.
    -   O corpo da resposta não está vazio.
    -   O corpo da resposta contém a palavra "transactions".

-   **Limites (Thresholds):** O teste será considerado "aprovado" ou "reprovado" com base nestes critérios:
    -   `http_req_failed`: A taxa de requisições com falha deve ser inferior a 1%.
    -   `http_req_duration`: 95% de todas as requisições HTTP devem ser concluídas em menos de 1500ms (1.5s).
    -   `statement_req_duration`: Uma métrica personalizada para rastrear especificamente o endpoint de extrato. 95% dessas requisições também devem ser concluídas em menos de 1500ms.

## Interpretando os Resultados

Após a conclusão do teste, o k6 imprimirá uma tabela de resumo no console. Preste muita atenção a:

-   **`checks`**: Deve ter um visto verde (✓) e mostrar 100% de sucesso. Se for menor, significa que sua API retornou respostas inesperadas.
-   **`http_req_duration`**: Olhe para o valor `p(95)`. Este é o tempo de resposta do 95º percentil. O resumo mostrará um visto verde se estiver abaixo do limite de 1500ms que você definiu.
-   **`http_req_failed`**: Deve ser 0.00%. Se for maior, indica erros de rede ou que o servidor está descartando conexões.
-   **Resumo dos Limites (Thresholds)**: No final, o k6 mostra um resumo de quais limites passaram ou falharam com um ✓ verde ou ✗ vermelho ao lado de cada um.

Este resumo oferece uma visão geral rápida e clara do desempenho da sua API sob a carga testada.
