using Limbo.Umbraco.Migrations.Models.Users;
using Limbo.Umbraco.MigrationsClient.Models.Users;
using Umbraco.Cms.Core.Models.Membership;

namespace Limbo.Umbraco.Migrations.Services;

public partial class MigrationsServiceBase {

    /// <summary>
    /// Imports a legacy user into Umbraco.
    /// </summary>
    /// <param name="user">The legacy user.</param>
    /// <returns>An instance of <see cref="ImportUserResult"/> representing the result of the import.</returns>
    public virtual ImportUserResult ImportUser(LegacyUser user) {

        IUser? umbracoUser = Dependencies.UserService.GetByEmail(user.Email);

        // TODO: handle user avatars...

        if (umbracoUser is null) {

            umbracoUser = Dependencies.UserService.CreateUserWithIdentity(user.Email, user.Email);
            umbracoUser.Name = user.Name;
            umbracoUser.Language = user.Language;
            umbracoUser.CreateDate = user.CreateDate.DateTime;

            Dependencies.UserService.Save(umbracoUser);

            return new ImportUserResult(user, umbracoUser, ImportUserStatus.Created);

        }

        if (umbracoUser.Name != user.Name || umbracoUser.Language != user.Language) {

            umbracoUser.Name = user.Name;
            umbracoUser.Language = user.Language;

            Dependencies.UserService.Save(umbracoUser);

            return new ImportUserResult(user, umbracoUser, ImportUserStatus.Updated);

        }

        return new ImportUserResult(user, umbracoUser, ImportUserStatus.NotModified);

    }

}