## 2024-05-22 - SwaggerWcf Runtime Requirements
**Learning:** In this WCF environment with SwaggerWcf v0.2.15, the `[SwaggerWcfServiceInfo]` attribute is NOT optional. Omitting it doesn't just result in empty docs; it causes runtime errors. Also, `Newtonsoft.Json` version mismatches are common and require explicit binding redirects in `Web.config`.
**Action:** Always verify `Web.config` binding redirects and `[SwaggerWcfServiceInfo]` presence when setting up SwaggerWcf in legacy .NET Framework projects.
