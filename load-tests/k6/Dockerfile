FROM grafana/k6:0.49.0
WORKDIR /tests
COPY scripts ./scripts
ENV K6_TARGET_URL=""
ENV K6_VUS="10"
ENV K6_DURATION="30s"
ENTRYPOINT ["k6", "run", "/tests/scripts/smoke.js"]
