class ConnectorTypeGet {
  String type = '';
  String repository = '';
  int? maxReplicas; 
  List<int> exposedPorts = List.empty(growable: true); 
  List<String> images = List.empty(growable: true);

  ConnectorTypeGet({
    required this.type,
    required this.repository,
    required this.maxReplicas,
    required this.exposedPorts,
    required this.images
  });

  factory ConnectorTypeGet.fromJson(Map<String, dynamic> json) => ConnectorTypeGet(
    type: json['type'] ?? '',
    repository: json['repository'] ?? '',
    maxReplicas: json['maxReplicas'],
    exposedPorts: List<int>.from(json['exposedPorts'].map((x) => x)),
    images: List<String>.from(json['images'].map((x) => x))
  );

  clear() {
    type = '';
    repository = '';
    maxReplicas = null; 
    exposedPorts = List.empty(growable: true); 
    images = List.empty(growable: true);
  }
}