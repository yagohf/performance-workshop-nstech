import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Trend } from 'k6/metrics';

// --- Configuração do Teste ---
const BASE_URL = 'http://localhost:5227';

// --- Métricas Customizadas ---
const statementResponseTrend = new Trend('statement_req_duration');

// --- Perfil da Carga (Opções do Teste) ---
export const options = {
  stages: [
    { duration: '5s', target: 50 }, // 1. Rampa de subida: de 0 a 50 usuários em 5 segundos
    { duration: '30s', target: 50 },  // 2. Carga sustentada: mantém 50 usuários por 30 segundos
  ],
  thresholds: {
    'http_req_failed': ['rate<0.01'],
    'http_req_duration': ['p(95)<1500'],
    'statement_req_duration': ['p(95)<1500'],
  },
};

// --- O Cenário do Teste (código que cada usuário virtual executa) ---
export default function () {
  // Helper para formatar data como YYYY-MM-DD
  function formatDate(date) {
    return date.toISOString().split('T')[0];
  }

  // Em vez de um ID fixo, geramos um número aleatório entre 1 e 100 a cada
  // vez que o código do usuário virtual é executado (para simular tráfego 
  // realista, de vários usuários ao mesmo tempo).
  const accountId = Math.floor(Math.random() * 100) + 1;

  const endDate = new Date();
  const startDate = new Date();
  startDate.setMonth(startDate.getMonth() - 3);

  const url = `${BASE_URL}/api/statement/${accountId}?startDate=${formatDate(startDate)}&endDate=${formatDate(endDate)}`;

  group('API de Extrato', function () {
    const res = http.get(url, {
      tags: { name: 'GetStatement' },
    });

    statementResponseTrend.add(res.timings.duration);

    check(res, {
      'status é 200': (r) => r.status === 200,
      'corpo da resposta não está vazio': (r) => r.body.length > 0,
      'resposta contém transacoes': (r) => r.body.includes('transactions')
    });
  });

  //sleep(1); // Para o teste de estresse, não queremos intervalo entre chamadas do VU.
}