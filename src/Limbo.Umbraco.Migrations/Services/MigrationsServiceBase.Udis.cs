using System;
using System.Diagnostics.CodeAnalysis;
using Limbo.Umbraco.Migrations.Models.Udis;
using Umbraco.Cms.Core;

namespace Limbo.Umbraco.Migrations.Services {

    public partial class MigrationsServiceBase {

        public virtual GuidUdi ParseGuidUdi(string input) {

            if (TryParseUdi(input, out GuidUdi? udi)) {
                return udi!;
            }

            throw new Exception("Invalid GUID UDI: " + input);

        }

        public virtual bool TryParseUdi(string? value, [NotNullWhen(true)] out GuidUdi? result) {
            bool _ = UdiParser.TryParse(value ?? string.Empty, out Udi? udi);
            result = udi as GuidUdi;
            return result is not null;
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