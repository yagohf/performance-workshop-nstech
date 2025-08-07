# Configurações da Máquina Local

Os testes de carga foram executados em uma máquina local com as seguintes configurações:

- **S.O:** Windows 11 Pro version 10.0.26100
- **Máquina:** Notebook Dell Vostro 15 5510
- **Processador:** Intel Core i7-11390H 3.40GHz, 2918 Mhz, 4 núcleos, 8 processadores lógicos
- **RAM:** 32GB

# Comparativo de Métricas de Teste de Carga

| Métrica                          | V0: Sem otimizações | V1: Sem lazy loading, com AsNoTracking() e Include() | V2: Banco indexado | V3: Dapper | V4: Paginação |
| -------------------------------- | ------------------- | ---------------------------------------------------- | ------------------ | ---------- | ------------- |
| **HEAP**                         | 1GB                 | 630MB                                                | 670MB              | 502MB      | 110MB         |
| **iterations**                   | 228                 | 471                                                  | 602                | 988        | 23668         |
| **iterations (taxa)**            | 6.19/s              | 12.60/s                                              | 16.70/s            | 26.91/s    | 675.15/s      |
| **duração média (avg)**          | 7343.14ms           | 3568.71ms                                            | 2746.06ms          | 1710.75ms  | 65.73ms       |
| **duração mínima (min)**         | 335.93ms            | 359.82ms                                             | 102.12ms           | 59.74ms    | 0.50ms        |
| **duração mediana (med)**        | 7771.30ms           | 3495.00ms                                            | 1748.58ms          | 1601.77ms  | 59.56ms       |
| **duração máxima (max)**         | 10254.44ms          | 8486.57ms                                            | 14179.32ms         | 7285.10ms  | 911.08ms      |
| **duração p(90)**                | 9246.82ms           | 5430.67ms                                            | 7429.59ms          | 3000.50ms  | 92.79ms       |
| **duração p(95)**                | 9379.91ms           | 6073.82ms                                            | 10493.29ms         | 3420.48ms  | 113.56ms      |
