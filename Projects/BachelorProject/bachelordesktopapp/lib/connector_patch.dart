class ConnectorPatch {

  String deploymentName = '';
  String type = '';
  String image = '';
  int replicas = 1; 

  send() {
    clear();
  }

  clear() {
    deploymentName = '';
    type = '';
    image = '';
    replicas = 1;
  }

  Map<String, dynamic> toJson() => 
  {
    'deploymentName': deploymentName,
    'type': type,
    'image': image,
    'replicas': replicas,
  };
}