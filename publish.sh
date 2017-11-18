eval $(minikube docker-env)
docker tag photo-app-api:ist jacobbevan/photo-app-api:test
docker push jacobbevan/photo-app-api:test