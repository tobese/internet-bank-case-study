# Kubernetes (k8s) Miniguide

## Common Commands

- **Show all resources in current namespace:**
  kubectl get all

- **Show all pods in all namespaces:**
  kubectl get pods --all-namespaces

- **Describe a resource (e.g., pod):**
  kubectl describe pod <pod-name>

- **View logs for a pod:**
  kubectl logs <pod-name>

- **View logs for a container in a pod:**
  kubectl logs <pod-name> -c <container-name>

- **Execute a shell in a pod:**
  kubectl exec -it <pod-name> -- sh

- **Apply a manifest file:**
  kubectl apply -f <file.yaml>

- **Delete a resource by file:**
  kubectl delete -f <file.yaml>

- **Get cluster info:**
  kubectl cluster-info

- **Get current context:**
  kubectl config current-context

- **Switch context:**
  kubectl config use-context <context-name>

- **Port forward a pod to localhost:**
  kubectl port-forward <pod-name> <local-port>:<pod-port>

---

For more, see: https://kubernetes.io/docs/reference/kubectl/
