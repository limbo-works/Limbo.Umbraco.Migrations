using Limbo.Umbraco.MigrationsClient.Models.Users;
using Umbraco.Cms.Core.Models.Membership;

namespace Limbo.Umbraco.Migrations.Models.Users;

public class UserImportResult {

    public LegacyUser LegacyUser { get; }

    public IUser UmbracoUser { get; }

    public UserImportStatus Status { get; }

    public UserImportResult(LegacyUser legacyUser, IUser umbracoUser, UserImportStatus status) {
        LegacyUser = legacyUser;
        UmbracoUser = umbracoUser;
        Status = status;
    }

}