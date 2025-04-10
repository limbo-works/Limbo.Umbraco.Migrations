using Limbo.Umbraco.MigrationsClient.Models.Users;
using Umbraco.Cms.Core.Models.Membership;

namespace Limbo.Umbraco.Migrations.Models.Users;

public class ImportUserResult {

    public LegacyUser LegacyUser { get; }

    public IUser UmbracoUser { get; }

    public ImportUserStatus Status { get; }

    public ImportUserResult(LegacyUser legacyUser, IUser umbracoUser, ImportUserStatus status) {
        LegacyUser = legacyUser;
        UmbracoUser = umbracoUser;
        Status = status;
    }

}