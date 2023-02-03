class ConnectorTypePut {

  String type = '';
  String repository = '';
  int maxReplicas = 1; 
  List<int> exposedPorts = List.empty(growable: true); 

  send() {
    clear();
  }

  clear() {
    type = '';
    repository = '';
    maxReplicas = 1; 
    exposedPorts = List.empty(growable: true); 
  }

  Map<String, dynamic> toJson() => 
  {
    'type': type,
    'repository': repository,
    'maxReplicas': maxReplicas,
    'exposedPorts': exposedPorts
  };
}