#!/bin/bash
# DEPRECATED: 本仓库已改用 RabbitMQ。此脚本仅保留以供参考，后续将移除。
# 请改用 scripts/rabbitmq-init.sh 或 scripts/rabbitmq-init.ps1

# Simple script to create the data-points topic inside the kafka container.
KAFKA_CONTAINER=$(docker ps --filter "ancestor=bitnami/kafka:3.5" --format "{{.Names}}" | head -n 1)
if [ -z "$KAFKA_CONTAINER" ]; then
  echo "Kafka container not found. Ensure kafka from docker-compose is running."
  exit 1
fi
echo "Using kafka container: $KAFKA_CONTAINER"
docker exec -it $KAFKA_CONTAINER /opt/bitnami/kafka/bin/kafka-topics.sh --create --topic data-points --bootstrap-server localhost:9092 --replication-factor 1 --partitions 1
echo "Topic created (or already exists)."
