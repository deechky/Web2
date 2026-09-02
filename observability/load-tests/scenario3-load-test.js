// Scenario 3 (docs/observability-scenarios.md) - k6 load test protiv Gateway-a. Pokretanje:
//   k6 run observability/load-tests/scenario3-load-test.js
// Preduslov: sva 4 servisa deployovana i zdrava (curl http://localhost:8141/health/ready),
// seed nalozi marko@primer.com / ana@primer.com dostupni (README.md, sekcija 7).
// Rezultate uporediti sa Prometheus histogramom (p95 iz http_server_request_duration_seconds
// treba da se poklopi sa k6-ovim http_req_duration p95, u okviru merne greske).
import http from 'k6/http';
import { check, sleep } from 'k6';

// Ramp od 1 do 30 virtuelnih korisnika kroz Gateway, realan tok: login pa GET /api/trips,
// isto sto i normalan korisnik radi.
export const options = {
  stages: [
    { duration: '15s', target: 5 },
    { duration: '20s', target: 30 },
    { duration: '15s', target: 30 },
    { duration: '10s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<3000'],
  },
};

const BASE = 'http://localhost:8141';
const USERS = [
  { email: 'marko@primer.com', lozinka: 'marko123' },
  { email: 'ana@primer.com', lozinka: 'ana123' },
];

export default function () {
  const user = USERS[Math.floor(Math.random() * USERS.length)];

  const loginRes = http.post(`${BASE}/api/auth/login`, JSON.stringify({
    email: user.email,
    lozinka: user.lozinka,
  }), { headers: { 'Content-Type': 'application/json' } });

  check(loginRes, { 'login 200': (r) => r.status === 200 });

  if (loginRes.status === 200) {
    const token = loginRes.json('token');
    const tripsRes = http.get(`${BASE}/api/trips`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    check(tripsRes, { 'trips 200': (r) => r.status === 200 });
  }

  sleep(1);
}
