using System;
using Limbo.Umbraco.Migrations.Models.Users;
using Limbo.Umbraco.MigrationsClient.Models.Users;
using Umbraco.Cms.Core.Models.Membership;

namespace Limbo.Umbraco.Migrations.Services;

public partial class MigrationsServiceBase {

    /// <summary>
    /// Imports a legacy user into Umbraco.
    /// </summary>
    /// <param name="user">The legacy user.</param>
    /// <returns>An instance of <see cref="UserImportResult"/> representing the result of the import.</returns>
    public virtual UserImportResult ImportUser(LegacyUser user) {

        try {

            IUser? umbracoUser = Dependencies.UserService.GetByEmail(user.Email);

            // TODO: handle user avatars...

            if (umbracoUser is null) {

                umbracoUser = Dependencies.UserService.CreateUserWithIdentity(user.Email, user.Email);
                umbracoUser.Name = user.Name;
                umbracoUser.Language = user.Language;
                umbracoUser.CreateDate = user.CreateDate.DateTime;

                Dependencies.UserService.Save(umbracoUser);

                return new UserImportResult(user, umbracoUser, UserImportStatus.Created);

            }

            if (umbracoUser.Name != user.Name || umbracoUser.Language != user.Language) {

                umbracoUser.Name = user.Name;
                umbracoUser.Language = user.Language;

                Dependencies.UserService.Save(umbracoUser);

                return new UserImportResult(user, umbracoUser, UserImportStatus.Updated);

            }

            return new UserImportResult(user, umbracoUser, UserImportStatus.NotModified);

        } catch (Exception ex) {

            return new UserImportResult(user, ex);


        }

    }

}