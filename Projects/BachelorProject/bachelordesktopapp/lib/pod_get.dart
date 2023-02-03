class PodGet {
  String name = '';

  PodGet({
    required this.name,
  });

  factory PodGet.fromJson(Map<String, dynamic> json) => PodGet(
    name: json["name"] ?? '',
  );
}