using System;
using Limbo.Umbraco.Migrations.Models.Udis;
using Umbraco.Cms.Core;

namespace Limbo.Umbraco.Migrations.Services {

    public partial class MigrationsServiceBase {

        public virtual GuidUdi ParseGuidUdi(string input) {

            if (UdiParser.TryParse(input, out GuidUdi? udi)) {
                return udi!;
            }

            throw new Exception("Invalid GUID UDI: " + input);

        }

        /// <summary>
        /// Parses the specified <paramref name="input"/> string into a <see cref="GuidUdiList"/>. If
        /// <paramref name="input"/> is either null, empty or white space, <see langword="null"/> is returned instead.
        /// </summary>
        /// <param name="input">The input string to be parsed.</param>
        /// <returns>An instance of <see cref="GuidUdiList"/> if <paramref name="input"/> contains any UDIs; otherwise, <see langword="null"/>.</returns>
        public virtual GuidUdiList? ParseGuidUdiList(string? input) {

            if (string.IsNullOrWhiteSpace(input)) return null;

            GuidUdiList udis = new();

            foreach (string piece in input.Split(',')) {
                if (UdiParser.TryParse(piece, out GuidUdi? udi)) {
                    udis.Add(udi!);
                }
            }

            return udis;

        }

    }

}