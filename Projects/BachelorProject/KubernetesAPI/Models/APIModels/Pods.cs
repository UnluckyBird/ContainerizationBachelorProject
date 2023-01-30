namespace KubernetesAPI.Models.APIModels
{
    public class Pods
    {
        public List<Pod> items { get; set; }
    }

    public class Pod
    {
        public Metadata metadata { get; set; }
    }
}
