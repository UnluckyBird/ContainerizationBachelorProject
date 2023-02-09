class ConnectorTypePut {

  String type = '';
  String repository = '';
  int? maxReplicas; 
  List<int> exposedPorts = List.empty(growable: true); 

  send() {
    clear();
  }

  clear() {
    type = '';
    repository = '';
    maxReplicas = null; 
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