#!/bin/bash

if [[ ! $(command -v kind) ]] 
then
    echo "Command kind not found"
    exit 1
elif [[ $(kind get clusters | grep "five9-cluster") ]] 
then
    echo "Cluster already exists"
else
    kind create cluster --config $PWD/infra/kind_cluster.yml
    kubectl apply -f https://kind.sigs.k8s.io/examples/ingress/deploy-ingress-nginx.yaml
fi