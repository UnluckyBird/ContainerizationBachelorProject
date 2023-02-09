# Containerization Bachelor Project

## How to get up and running
In order to run the software you will need to have downloaded and gotten minikube up and running.
Then using the command line use the following command, replace the necessary information.

    kubectl create secret docker-registry regcred 
    --docker-server=https://index.docker.io/v1/ 
    --docker-username={{docker username}} 
    --docker-password={{docker password}} 
    --docker-email={{docker email}}

Then use this second command to initaliaze the deployments and services to run, it might be necessary to make a few changes in the template file.

    kubectl apply -f "{{Your path to this git repository}}\Bachelor Project\YAML Template\InitializeCluster.yaml" 

## Project was created by Martin de Fries Justinussen