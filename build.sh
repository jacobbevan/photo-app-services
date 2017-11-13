eval $(minikube docker-env)
docker build --rm -f Dockerfile -t photo-api:ist .
