eval $(minikube docker-env)
kubectl config use-context minikube
docker build --rm -f Dockerfile -t photo-app-api:ist .
