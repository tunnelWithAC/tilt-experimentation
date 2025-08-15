# Ephemeral Kafka Topics with ArgoCD

This document describes how to achieve the following workflow on Kubernetes using ArgoCD:

1. **Presync**: Create a topic with a unique name using a Kubernetes job
2. **Sync**: Deploy a backend service
3. **Postsync**: Run automation tests using the created topic
4. **Cleanup**: Delete the topic regardless of test job success/failure

## Implementation

### 1. Presync Hook - Create Topic

```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: create-weather-updates-topic-job
  annotations:
    argocd.argoproj.io/hook: PreSync
    argocd.argoproj.io/hook-delete-policy: BeforeHookCreation
spec:
  backoffLimit: 3
  template:
    spec:
      restartPolicy: OnFailure
      containers:
        - name: create-topic
          image: confluentinc/cp-kafka:7.4.1
          command: ["/bin/sh", "-c"]
          args:
            - |
              # Generate unique topic name using timestamp
              TOPIC_NAME="weather_updates_$(date +%s)_${RANDOM}"
              echo "Creating topic: $TOPIC_NAME"
              
              # Wait for Kafka to be ready
              cub kafka-ready -b kafka-broker:29092 1 20 && \
              kafka-topics --create --if-not-exists \
                --bootstrap-server kafka-broker:29092 \
                --topic "$TOPIC_NAME" \
                --replication-factor 1 \
                --partitions 1
              
              # Store topic name for other jobs to use
              echo "$TOPIC_NAME" > /tmp/topic_name.txt
```

### 2. Main Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: hellotilt
  annotations:
    argocd.argoproj.io/sync-wave: "1"
spec:
  # ... your deployment spec
```

### 3. Postsync Hook - Run Tests

```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: run-automation-tests-job
  annotations:
    argocd.argoproj.io/hook: PostSync
    argocd.argoproj.io/hook-delete-policy: BeforeHookCreation
    argocd.argoproj.io/sync-wave: "2"
spec:
  backoffLimit: 3
  template:
    spec:
      restartPolicy: OnFailure
      containers:
        - name: run-tests
          image: your-test-image:latest
          env:
            - name: KAFKA_TOPIC
              value: "weather_updates_$(date +%s)_${RANDOM}"
            - name: KAFKA_BROKER
              value: "kafka-broker:29092"
          command: ["dotnet", "test"]
          # ... your test configuration
```

### 4. Final Hook - Cleanup Topic

```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: cleanup-kafka-topic-job
  annotations:
    argocd.argoproj.io/hook: PostSync
    argocd.argoproj.io/hook-delete-policy: BeforeHookCreation
    argocd.argoproj.io/sync-wave: "3"
spec:
  backoffLimit: 1
  template:
    spec:
      restartPolicy: OnFailure
      containers:
        - name: cleanup-topic
          image: confluentinc/cp-kafka:7.4.1
          command: ["/bin/sh", "-c"]
          args:
            - |
              # Get topic name from the create job
              TOPIC_NAME=$(kubectl logs job/create-weather-updates-topic-job --tail=1 | grep -o 'weather_updates_[0-9]*_[0-9]*' || echo "")
              
              if [ -n "$TOPIC_NAME" ]; then
                echo "Deleting topic: $TOPIC_NAME"
                kafka-topics --delete \
                  --bootstrap-server kafka-broker:29092 \
                  --topic "$TOPIC_NAME"
              else
                echo "No topic found to delete"
              fi
```

### 5. Kustomization

```yaml
bases:
  - ../../base

resources:
  - service.kafka-broker.yaml
  - job.create-kafka-topics.yaml
  - job.run-tests.yaml
  - job.cleanup-topic.yaml
  - deployment.yaml
```

## Alternative: Use ConfigMap for Topic Name

For better coordination between jobs, use a ConfigMap:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: topic-config
  annotations:
    argocd.argoproj.io/hook: PreSync
    argocd.argoproj.io/hook-delete-policy: BeforeHookCreation
data:
  topic_name: "weather_updates_$(date +%s)_${RANDOM}"
```

Then reference it in your jobs:

```yaml
env:
  - name: KAFKA_TOPIC
    valueFrom:
      configMapKeyRef:
        name: topic-config
        key: topic_name
```

## Key ArgoCD Concepts

- **`PreSync`**: Runs before main sync
- **`PostSync`**: Runs after main sync
- **`sync-wave`**: Controls execution order (lower numbers run first)
- **`BeforeHookCreation`**: Deletes old hook resources before creating new ones

## Workflow Order

1. **PreSync**: Create topic with unique name
2. **Wave 1**: Deploy backend service
3. **Wave 2**: Run tests using the topic
4. **Wave 3**: Clean up topic regardless of test results

This ensures proper sequencing and cleanup even if tests fail!

## Benefits

- **Declarative**: Topic creation is part of your infrastructure
- **Idempotent**: Safe to run multiple times
- **Configurable**: Easy to modify topic settings
- **Operational**: Clear separation of concerns
- **Reliable**: Cleanup happens regardless of test outcomes
