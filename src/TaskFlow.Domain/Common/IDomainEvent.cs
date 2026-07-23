using MediatR;

namespace TaskFlow.Domain.Common;

/// <summary>
/// Marker interface for domain events. Extends MediatR's INotification so that later,
/// an EF Core SaveChanges interceptor (Infrastructure layer) can dispatch these events
/// through the same MediatR pipeline that handles commands/queries - one dispatch mechanism,
/// not two. This is a deliberate, well-known pragmatic exception to strict layer purity
/// (see Jason Taylor's Clean Architecture template, which uses the same approach).
/// </summary>
public interface IDomainEvent : INotification
{
}
