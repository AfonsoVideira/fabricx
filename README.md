# FabricX - Call Center Management System #
A microservices-based call center management system built with .NET 8, featuring agent state management, authentication, and interaction orchestration.

# Helm-Based Deployment Architecture #
This project uses Helm for Kubernetes deployments. Each microservice (AuthService, AgentStateApi, InteractionApi) has its own Helm chart and deployment files, allowing independent upgrades and configuration.

## Umbrella Chart: fabricx ##
 - The umbrella chart (fabricx) wraps all microservices, enabling unified deployment and management of the entire system.
 - You can deploy all services together or manage them individually.

## How to Deploy & Upgrade ##
 - See the setup.sh script in the root directory for step-by-step instructions to deploy, upgrade and test each microservice or the umbrella chart.
 - The script automates Helm commands for install, upgrade, and test.

## Helm Test Pods ##
 - Helm charts include test pods (using hooks) that check connectivity between services (e.g., AuthService to AgentStateApi).
