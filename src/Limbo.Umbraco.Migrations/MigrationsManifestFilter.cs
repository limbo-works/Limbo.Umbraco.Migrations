using System.Collections.Generic;
using Umbraco.Cms.Core.Manifest;

namespace Limbo.Umbraco.Migrations;

//// <inheritdoc />
public class MigrationsManifestFilter : IManifestFilter {

    /// <inheritdoc />
    public void Filter(List<PackageManifest> manifests) {
        manifests.Add(new PackageManifest {
            AllowPackageTelemetry = true,
            PackageId = MigrationsPackage.Alias,
            PackageName = MigrationsPackage.Name,
            Version = MigrationsPackage.InformationalVersion
        });
    }

}