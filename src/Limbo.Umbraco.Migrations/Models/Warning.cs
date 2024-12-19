using System;

namespace Limbo.Umbraco.Migrations.Models;

public class Warning {

    public string Message { get; }

    public Exception? Exception { get; }

    public Warning(string message) {
        Message = message;
    }

    public Warning(string message, Exception? exception) {
        Message = message;
        Exception = exception;
    }

}