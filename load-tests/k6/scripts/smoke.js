import http from 'k6/http';
import { check, sleep } from 'k6';

const targetUrl = __ENV.K6_TARGET_URL || 'http://host.docker.internal:8080/health/ready';

export const options = {
  vus: Number(__ENV.K6_VUS || 10),
  duration: __ENV.K6_DURATION || '30s',
};

export default function () {
  const response = http.get(targetUrl);
  check(response, {
    'status is 200': (r) => r.status === 200,
    'request completed under 500ms': (r) => r.timings.duration < 500,
  });
  sleep(1);
}
