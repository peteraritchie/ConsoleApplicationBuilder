# Repo project structure
2025-01-12

`/`
- `src`
- `.azuredevops`
- `docs`
- `scaffolding`

## Context

There are a number of common practices to structuring a repository. Each repository builds off of many practices to establish their unique structure.

## Rationale
- src
  - It's essential for one or more build pipelines to be triggered independently by changes to the source code. The source code is the most important part of the repository, so it should be at the root of the repository.
  - contains all the content that will be used to generate deployables
- .azuredevops
  - It's useful for one or more piplines to be triggered idenpendently by changes to Azure DevOps configuration. `.azuredevops` is a recognized location for storing Azure DevOps configuration files. \[[1][pr-templates]\]
- docs
  - chosen to be indepedent of other directories that may be used to trigger a pipeline
- scaffolding
  - chosen to be indepedent of other directories that may be used to trigger a pipeline

## References
 - 1: [Improve pull request descriptions using templates][pr-templates]

[pr-templates]: https://learn.microsoft.com/en-us/azure/devops/repos/git/pull-request-templates?view=azure-devops
