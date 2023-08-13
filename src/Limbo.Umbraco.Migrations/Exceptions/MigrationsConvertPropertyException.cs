using System;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Exceptions {

    public class MigrationsConvertPropertyException : Exception {

        public ILegacyElement Owner { get; }

        public ILegacyProperty Property { get; }

        public MigrationsConvertPropertyException(ILegacyElement owner, ILegacyProperty property, string message) : base(message) {
            Owner = owner;
            Property = property;
        }

        public MigrationsConvertPropertyException(ILegacyElement owner, ILegacyProperty property, string message, Exception? innerException) : base(message, innerException) {
            Owner = owner;
            Property = property;
        }

    }

}