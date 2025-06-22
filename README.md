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
