"# ContainerizationBachelorProject" 
#So kubernetes can access your private repositories
kubectl create secret docker-registry regcred --docker-server=https://index.docker.io/v1/ --docker-username=<your-name> --docker-password=<your-pword> --docker-email=<your-email>

minikube start --driver=docker
kubectl proxy --port=8080
kubectl create deployment mongo-depl --image=mongo
kubectl get pod

docker build --rm -t kube-api:1.4 -t kube-api:latest .
docker run --rm -p 5000:5000 -p 5001:5001 -e ASPNETCORE_HTTP_PORT=https://+:5001 -e ASPNETCORE_URLS=http://+:5000 kube-api -e "Docker:Username=unluckybird" -e "Docker:Namespace=unluckybird" -e "Docker:Repository=unluckybird_private_repository" 