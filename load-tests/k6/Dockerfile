FROM grafana/k6:0.49.0
WORKDIR /tests
COPY scripts ./scripts
ENTRYPOINT ["k6", "run"]
CMD ["/tests/scripts/smoke.js"]
