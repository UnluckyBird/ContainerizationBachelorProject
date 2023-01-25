"# ContainerizationBachelorProject" 
#So kubernetes can access your private repositories
kubectl create secret docker-registry regcred --docker-server=https://index.docker.io/v1/ --docker-username=unluckybird --docker-password={{docker password}} --docker-email=martin.justinussen@gmail.com

minikube start --driver=docker
kubectl proxy --port=8080
kubectl create deployment mongo-depl --image=mongo
kubectl get pod

docker build --rm -t unluckybird/bachelor-project:kube-api-1.0 -t unluckybird/bachelor-project:kube-api-latest .
docker run --rm -p 5000:5000 -p 5001:5001 -e ASPNETCORE_HTTP_PORT=https://+:5001 -e ASPNETCORE_URLS=http://+:5000 unluckybird/bachelor-project:kube-api-latest -e "Docker:Username=unluckybird" -e "Docker:Namespace=unluckybird" -e "Docker:Repository=unluckybird_private_repository" 