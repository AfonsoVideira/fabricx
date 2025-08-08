#!/bin/bash

build_image() {

    local context=$1
    local dockerfile=$2
    local image_name=$3

    echo "Building Docker image: $image_name from context: $context with Dockerfile: $dockerfile"

    docker build -t "$image_name" -f "$dockerfile" "$context"

}

build_cluster(){
    echo "Building Kubernetes cluster with kind"
    $(PWD)/infra/setup.sh
    if [[ $? -ne 0 ]]; then
        echo "Failed to set up Kubernetes cluster"
        exit 1
    fi
    echo "Kubernetes cluster setup completed successfully" 
    kubectl get nodes
}

load_image() {

    local image_name=$1
    local cluster_name=$2

    echo "Loading Docker image: $image_name into the Kubernetes cluster"

    kind load docker-image "$image_name" --name "$cluster_name"
}

build_and_load_image() {

    local context=$1
    local image_name=$2

    dockerfile="$context/Dockerfile"

    build_image "$context" "$dockerfile" "$image_name"
    load_image "$image_name" "five9-cluster"
}

initialize_cluster_and_dependencies() {

    echo "Initializing Kubernetes cluster and dependencies"
    build_cluster

    echo "Building and loading Docker images into the cluster"
    build_and_load_image "./AuthService" "fabricx/auth:1.0"
    build_and_load_image "./AgentStateApi" "fabricx/agent-state-api:1.0"
    build_and_load_image "./InteractionApi" "fabricx/interaction-api:1.0"

    echo "All images built and loaded into the cluster successfully"

    echo "Setting up DBs for each service"
    docker compose up -d

    echo "Databases for each service are up and running"
}

tear_down_cluster_and_dependencies() {

    echo "Tearing down Kubernetes cluster"
    kind delete cluster --name five9-cluster

    echo "Stopping docker services"
    docker compose down

    echo "Kubernetes cluster and dependencies have been torn down successfully"
}

helm_install_upgrade() {

    local release_name=$1
    local chart_path=$2
    local namespace=$3

    echo "Installing Helm chart: $chart_path with release name: $release_name in namespace: $namespace"

    # Lint Chart
    helm lint "$chart_path"

    if [[ $? -ne 0 ]]; then
        echo "Helm linting failed for chart: $chart_path"
        exit 1
    fi

    command="helm upgrade --install $release_name $chart_path -f $chart_path/values.yaml --namespace $namespace --create-namespace"
    # Dry Run
    echo "$command --dry-run" | sh

    if [[ $? -ne 0 ]]; then
        echo "Helm dry run failed for chart: $chart_path"
        exit 1
    fi
    # Actual Install
    echo "$command" | sh

    if [[ $? -ne 0 ]]; then
        echo "Helm install failed for chart: $chart_path"
        exit 1
    fi

}

helm_upgrade(){
    local release_name=$1
    local chart_path=$2
    local namespace=$3
    local set_values=$4

    echo "Upgrading Helm release: $release_name with chart: $chart_path in namespace: $namespace with values: $set_values"

    command="helm upgrade $release_name $chart_path -f $chart_path/values.yaml $set_values --namespace $namespace"

    echo "$command" | sh

    if [[ $? -ne 0 ]]; then
        echo "Helm upgrade failed for release: $release_name"
        exit 1
    fi
}

helm_test_suite() {

    local release_name=$1
    local namespace=$2

    echo "Testing Helm release: $release_name in namespace: $namespace"

    helm test "$release_name" --namespace "$namespace" --logs

}