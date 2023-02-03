class ConnectorGet {

  String deploymentName = '';
  DateTime? deploymentTime = DateTime.now();
  String type = '';
  String image = '';
  int replicas = 0; 
  int availableReplicas = 0; 
  List<EnvVar> envVars = List.generate(3, ((index) {
    return EnvVar(name: 'name $index', value: 'value $index');
  }));

  ConnectorGet({
    required this.deploymentName,
    this.deploymentTime,
    required this.type,
    required this.image,
    required this.replicas,
    required this.availableReplicas,
    required this.envVars,
  });

  factory ConnectorGet.fromJson(Map<String, dynamic> json) => ConnectorGet(
    deploymentName: json['deploymentName'] ?? '',
    deploymentTime: json['deploymentTime'] == null ? null : DateTime.parse(json['deploymentTime']),
    type: json['type'] ?? '',
    image: json['image'] ?? '',
    replicas: json['replicas'] ?? 0,
    availableReplicas: json['availableReplicas'] ?? 0,
    envVars: List<EnvVar>.from(json['env'].map((x) => EnvVar.fromJson(x)))
  );

  clear() {
    deploymentName = '';
    deploymentTime = DateTime.now();
    type = '';
    image = '';
    replicas = 0; 
    availableReplicas = 0; 
  }
}

class EnvVar {
  String name = '';
  String value = '';

  EnvVar({
    required this.name,
    required this.value,
  });

  factory EnvVar.fromJson(Map<String, dynamic> json) => EnvVar(
    name: json['name'] ?? '',
    value: json['value'] ?? '',
  );
}