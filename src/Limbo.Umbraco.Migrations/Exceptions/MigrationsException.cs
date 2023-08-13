using System;

namespace Limbo.Umbraco.Migrations.Exceptions {

    public class MigrationsException : Exception {

        public MigrationsException(string message) : base(message) { }

        public MigrationsException(string message, Exception? innerException) : base(message, innerException) { }

    }

}