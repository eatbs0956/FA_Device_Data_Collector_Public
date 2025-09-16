#!/usr/bin/env bash
set -euo pipefail

RABBITMQ_HOST=${RABBITMQ_HOST:-localhost}
RABBITMQ_USER=${RABBITMQ_USER:-guest}
RABBITMQ_PASS=${RABBITMQ_PASS:-guest}

# 需要安装 rabbitmqadmin 或使用 HTTP API
# 示例：创建 vhost/exchange/queue/bindings

curl -u "$RABBITMQ_USER:$RABBITMQ_PASS" -H 'content-type:application/json' \
  -XPUT http://$RABBITMQ_HOST:15672/api/vhosts/devdcp

curl -u "$RABBITMQ_USER:$RABBITMQ_PASS" -H 'content-type:application/json' \
  -XPOST http://$RABBITMQ_HOST:15672/api/exchanges/devdcp/ingestion \
  -d '{"type":"topic","durable":true,"auto_delete":false,"internal":false,"arguments":{}}'

curl -u "$RABBITMQ_USER:$RABBITMQ_PASS" -H 'content-type:application/json' \
  -XPOST http://$RABBITMQ_HOST:15672/api/queues/devdcp/ingestion.raw \
  -d '{"durable":true,"arguments":{"x-dead-letter-exchange":"ingestion.dlx"}}'

curl -u "$RABBITMQ_USER:$RABBITMQ_PASS" -H 'content-type:application/json' \
  -XPOST http://$RABBITMQ_HOST:15672/api/bindings/devdcp/e/ingestion/q/ingestion.raw \
  -d '{"routing_key":"ingestion.raw.*"}'
