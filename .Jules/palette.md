## 2024-05-22 - [SwaggerWcf Runtime Requirement]
**Learning:** `SwaggerWcf` v0.2.15 requires `[SwaggerWcfServiceInfo]` on the service class. Without it, the application throws a runtime error, preventing documentation generation.
**Action:** Always verify `[SwaggerWcfServiceInfo]` exists when setting up SwaggerWcf in legacy WCF projects.
