# Quick & Dirty EKS 1.33 Upgrade

1. Upgrade control plane:
   aws eks update-cluster-version --name internet-bank --kubernetes-version 1.33

2. Upgrade each managed node group:
   aws eks update-nodegroup-version --cluster-name internet-bank --nodegroup-name standard-workers --kubernetes-version 1.33

3. Validate:
   kubectl get nodes
   kubectl get pods --all-namespaces

If everything is Running, you're done.
