using System;
using Limbo.Umbraco.MigrationsClient.Models;

namespace Limbo.Umbraco.Migrations.Exceptions {

    public class MigrationsConvertPropertyException : Exception {

        public LegacyEntity Owner { get; }

        public LegacyProperty Property { get; }

        public MigrationsConvertPropertyException(LegacyEntity owner, LegacyProperty property, string message) : base(message) {
            Owner = owner;
            Property = property;
        }

        public MigrationsConvertPropertyException(LegacyEntity owner, LegacyProperty property, string message, Exception? innerException) : base(message, innerException) {
            Owner = owner;
            Property = property;
        }

    }

}