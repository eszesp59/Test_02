## 2024-05-22 - SwaggerWcf Runtime Requirements
**Learning:** `SwaggerWcf` v0.2.15 requires `[SwaggerWcfServiceInfo]` on the service class to prevent runtime errors, and `Newtonsoft.Json` often requires an assembly binding redirect in `Web.config` due to dependency mismatches.
**Action:** Always check for `[SwaggerWcfServiceInfo]` and binding redirects when working with `SwaggerWcf` on legacy .NET Framework services to ensure stability and proper documentation generation.
