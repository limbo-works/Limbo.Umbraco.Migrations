using System;
using Umbraco.Cms.Core.Models;

namespace Limbo.Umbraco.Migrations.Models;

public class ContentImportResult {

    public ContentImportStatus Status { get; }

    public IContent? Content { get; }

    public Exception? Exception { get; }

    public ContentImportResult(ContentImportStatus status, IContent content) {
        Status = status;
        Content = content;
    }

    public ContentImportResult(Exception exception) {
        Status = ContentImportStatus.Failed;
        Exception = exception;
    }

}

public enum ContentImportStatus {
    Created,
    Updated,
    NotModified,
    Failed
}