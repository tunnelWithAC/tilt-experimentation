# tilt-experimentation

## Working with services across multiple repos

```
include('./frontend/Tiltfile')
include('./backend/Tiltfile')
```
[Source: Tilt Multiple Repos](https://docs.tilt.dev/multiple_repos.html)

## ctlptl

`ctlptl` (pronounced "cattle patrol") is a CLI for declaratively setting up local Kubernetes clusters.

[Source: ctlptl repo](https://github.com/tilt-dev/ctlptl)

## Kustomize

```
yaml = kustomize('./wordpress')
k8s_yaml(yaml)
```

[Source: Kustomize Tilt Example](https://github.com/tilt-dev/kustomize-example)

## Using Tilt with Argo CD (GitOps CI/CD)

**Tilt** is a local development tool for Kubernetes, providing fast feedback loops for developers by automating builds, deploys, and live updates as you code. **Argo CD** is a GitOps continuous delivery tool for Kubernetes, designed to keep your clusters in sync with your Git repositories.

### Typical Workflow
- **Develop Locally with Tilt:** Use Tilt for rapid local development. Tilt watches your code, builds images, and applies manifests to your local (or remote) Kubernetes cluster.
- **Push to Git:** When you’re ready, push your changes (code, manifests, Helm charts, Kustomize overlays, etc.) to your Git repository.
- **Argo CD Syncs to Cluster:** Argo CD watches your Git repository. When it detects changes, it automatically applies them to your production (or staging) Kubernetes cluster.

### How They Integrate
- **Tilt is for local dev:** It’s not meant for production deployment, but for developer feedback.
- **Argo CD is for GitOps deployment:** It’s meant for staging/production, and is triggered by Git changes.

**You do not run Tilt in CI/CD or production.** Instead, you use Tilt to iterate locally, then commit/push, and Argo CD takes over for cluster delivery.

### Example Flow
1. **Local Dev:**
   - Run `tilt up` to develop and test changes locally.
   - Tilt builds images, applies manifests, and gives you live feedback.
2. **Commit & Push:**
   - When ready, commit your changes (including Dockerfiles, manifests, etc.) and push to your Git repo.
3. **Argo CD Watches Git:**
   - Argo CD detects the change and syncs your cluster to the new state.

### Advanced: Using Tilt with a Remote Cluster Managed by Argo CD
- You can point Tilt at a remote cluster (e.g., a dev namespace in a shared cluster).
- You can use the same manifests/Helm charts/Kustomize overlays in both Tilt and Argo CD, ensuring consistency.
- **But:** Tilt is still for dev, Argo CD is for GitOps delivery.

### Argo Workflows for CI/CD
- If you use **Argo Workflows** for CI/CD, you can have a pipeline that builds, tests, and pushes images, then updates manifests in Git.
- Argo CD then deploys those changes.

### Summary Table

| Tool      | Purpose                | Typical Use                |
|-----------|------------------------|----------------------------|
| Tilt      | Local dev, fast feedback| `tilt up` on dev machine   |
| Argo CD   | GitOps deployment      | Watches Git, syncs cluster |
| Argo Workflows | CI/CD pipelines   | Build/test/push/update Git |

**References:**
- [Tilt docs: How Tilt fits into CI/CD](https://docs.tilt.dev/ci.html)
- [Argo CD docs](https://argo-cd.readthedocs.io/en/stable/)
- [GitOps with Argo CD](https://argo-cd.readthedocs.io/en/stable/user-guide/auto_sync/)
