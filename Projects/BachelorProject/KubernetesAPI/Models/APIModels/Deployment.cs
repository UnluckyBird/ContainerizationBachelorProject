namespace KubernetesAPI.Models.APIModels
{
    public class Deployments
    {
        public List<Deployment> items { get; set; }
    }

    public class Deployment
    {
        public Metadata metadata { get; set; }
        public Spec spec { get; set; }
        public Status status { get; set; }
    }

    public class Metadata
    {
        public string name { get; set; }
        public string nameSpace { get; set; }
        public string uid { get; set; }
        public string resourceVersion { get; set; }
        public int generation { get; set; }
        public DateTime? creationTimestamp { get; set; }
        public Labels labels { get; set; }
    }

    public class Labels
    {
        public string app { get; set; }
        public string k8sapp { get; set; }
    }

    public class Spec
    {
        public int replicas { get; set; }
        public Selector selector { get; set; }
        public Template template { get; set; }
        public Strategy strategy { get; set; }
        public int revisionHistoryLimit { get; set; }
        public int progressDeadlineSeconds { get; set; }
    }

    public class Selector
    {
        public Matchlabels matchLabels { get; set; }
    }

    public class Matchlabels
    {
        public string app { get; set; }
        public string k8sapp { get; set; }
    }

    public class Template
    {
        public Metadata metadata { get; set; }
        public Spec1 spec { get; set; }
    }

    public class Spec1
    {
        public Container[] containers { get; set; }
        public string restartPolicy { get; set; }
        public int terminationGracePeriodSeconds { get; set; }
        public string dnsPolicy { get; set; }
        public string schedulerName { get; set; }
        public Volume[] volumes { get; set; }
        public Nodeselector nodeSelector { get; set; }
        public string serviceAccountName { get; set; }
        public string serviceAccount { get; set; }
        public Affinity affinity { get; set; }
        public Toleration[] tolerations { get; set; }
        public string priorityClassName { get; set; }
    }

    public class Nodeselector
    {
        public string kubernetesioos { get; set; }
    }

    public class Affinity
    {
        public Podantiaffinity podAntiAffinity { get; set; }
    }

    public class Podantiaffinity
    {
        public Preferredduringschedulingignoredduringexecution[] preferredDuringSchedulingIgnoredDuringExecution { get; set; }
    }

    public class Preferredduringschedulingignoredduringexecution
    {
        public int weight { get; set; }
        public Podaffinityterm podAffinityTerm { get; set; }
    }

    public class Podaffinityterm
    {
        public Labelselector labelSelector { get; set; }
        public string topologyKey { get; set; }
    }

    public class Labelselector
    {
        public Matchexpression[] matchExpressions { get; set; }
    }

    public class Matchexpression
    {
        public string key { get; set; }
        public string _operator { get; set; }
        public string[] values { get; set; }
    }

    public class Container
    {
        public string name { get; set; }
        public string image { get; set; }
        public Resources resources { get; set; }
        public string terminationMessagePath { get; set; }
        public string terminationMessagePolicy { get; set; }
        public string imagePullPolicy { get; set; }
        public string[] args { get; set; }
        public Port[] ports { get; set; }
        public Volumemount[] volumeMounts { get; set; }
        public Livenessprobe livenessProbe { get; set; }
        public Readinessprobe readinessProbe { get; set; }
        public List<EnvVar> env { get; set; }
    }

    public class EnvVar
    {
        public string name { get; set; }
        public string value { get; set; }
        public EnvVarSource valueFrom { get; set; }
    }

    public class EnvVarSource
    {
        public ConfigMapKeySelector? configMapKeyRef { get; set; }
        public SecretKeySelector? secretKeyRef { get; set; }
    }

    public class ConfigMapKeySelector
    {
        public string? name { get; set; }
    }

    public class SecretKeySelector
    {
        public string? key { get; set; }
    }

    public class Resources
    {
        public Limits limits { get; set; }
        public Requests requests { get; set; }
    }

    public class Limits
    {
        public string memory { get; set; }
    }

    public class Requests
    {
        public string cpu { get; set; }
        public string memory { get; set; }
    }

    public class Livenessprobe
    {
        public Httpget httpGet { get; set; }
        public int initialDelaySeconds { get; set; }
        public int timeoutSeconds { get; set; }
        public int periodSeconds { get; set; }
        public int successThreshold { get; set; }
        public int failureThreshold { get; set; }
    }

    public class Httpget
    {
        public string path { get; set; }
        public int port { get; set; }
        public string scheme { get; set; }
    }

    public class Readinessprobe
    {
        public Httpget1 httpGet { get; set; }
        public int timeoutSeconds { get; set; }
        public int periodSeconds { get; set; }
        public int successThreshold { get; set; }
        public int failureThreshold { get; set; }
    }

    public class Httpget1
    {
        public string path { get; set; }
        public int port { get; set; }
        public string scheme { get; set; }
    }

    public class Capabilities
    {
        public string[] add { get; set; }
        public string[] drop { get; set; }
    }

    public class Port
    {
        public string name { get; set; }
        public int containerPort { get; set; }
        public string protocol { get; set; }
    }

    public class Volumemount
    {
        public string name { get; set; }
        public bool readOnly { get; set; }
        public string mountPath { get; set; }
    }

    public class Volume
    {
        public string name { get; set; }
        public Configmap configMap { get; set; }
    }

    public class Configmap
    {
        public string name { get; set; }
        public Item1[] items { get; set; }
        public int defaultMode { get; set; }
    }

    public class Item1
    {
        public string key { get; set; }
        public string path { get; set; }
    }

    public class Toleration
    {
        public string key { get; set; }
        public string _operator { get; set; }
        public string effect { get; set; }
    }

    public class Strategy
    {
        public string type { get; set; }
        public Rollingupdate rollingUpdate { get; set; }
    }

    public class Rollingupdate
    {
        public object maxUnavailable { get; set; }
        public string maxSurge { get; set; }
    }

    public class Status
    {
        public int observedGeneration { get; set; }
        public int replicas { get; set; }
        public int updatedReplicas { get; set; }
        public int readyReplicas { get; set; }
        public int availableReplicas { get; set; }
        public Condition[] conditions { get; set; }
    }

    public class Condition
    {
        public string type { get; set; }
        public string status { get; set; }
        public DateTime lastUpdateTime { get; set; }
        public DateTime lastTransitionTime { get; set; }
        public string reason { get; set; }
        public string message { get; set; }
    }

}
