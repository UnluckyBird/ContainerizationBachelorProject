"# ContainerizationBachelorProject" 

minikube start --driver=docker
kubectl proxy --port=8080
kubectl create deployment mongo-depl --image=mongo
kubectl get pod
