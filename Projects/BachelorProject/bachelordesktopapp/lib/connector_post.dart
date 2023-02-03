class ConnectorPost {

  String deploymentName = '';
  String type = '';
  String image = '';
  int replicas = 1; 
  bool createService = false;
  int externalPortNumber = 0;

  send() {
    clear();
  }

  clear() {
    deploymentName = '';
    type = '';
    image = '';
    replicas = 1; 
    createService = false;
    externalPortNumber = 0;
  }

  Map<String, dynamic> toJson() => 
  {
    'deploymentName': deploymentName,
    'type': type,
    'image': image,
    'replicas': replicas,
    'createService': createService,
    'externalPortNumber': externalPortNumber
  };
}