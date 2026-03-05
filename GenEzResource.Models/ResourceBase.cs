using System;

namespace GenEzResource.Models;

/// <summary>
/// Represents the base class for resources in the application.
/// This class serves as the foundation for defining shared properties,
/// methods, or functionality that are common across multiple resource types.
/// Derive from this class to create specific resource implementations tailored
/// to the application's requirements.
/// </summary>
public class ResourceBase
{
    /// <summary>
    /// Gets or sets the unique identifier for an entity.
    /// This property is used to uniquely distinguish an instance of an entity.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the unique identifier of the owner associated with an entity.
    /// </summary>
    /// <remarks>
    /// The OwnerId property is typically used to associate the entity with a specific
    /// owner, such as a user, group, or organizational unit. It is generally represented
    /// as a GUID or other unique identifier within the system.
    /// </remarks>
    public string? OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the value associated with Key1.
    /// </summary>
    /// <remarks>
    /// This property is intended to store or retrieve data that corresponds to the logical identifier "Key1".
    /// Ensure the value assigned to this property aligns with the intended purpose or usage context.
    /// </remarks>
    public string? Key1 { get; set; }

    /// <summary>
    /// Gets or sets the value associated with Key2.
    /// This property is typically used to store or retrieve specific data
    /// identified by the Key2 identifier within the application.
    /// </summary>
    public string? Key2 { get; set; }

    /// <summary>
    /// Gets or sets the value associated with the property Key3.
    /// </summary>
    /// <remarks>
    /// This property is used to store and retrieve data corresponding to the Key3 designation
    /// within the application. Ensure the value assigned meets the expected type and format
    /// defined by the implementation.
    /// </remarks>
    public string? Key3 { get; set; }

    /// <summary>
    /// Gets or sets the version information of the object, typically represented as a string
    /// or numerical format. The version property is used to track changes, updates, or releases
    /// of the object or entity it is associated with.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the object was created.
    /// </summary>
    /// <remarks>
    /// This property typically holds the timestamp indicating when the instance
    /// of the object was instantiated or added to a database. The value is usually
    /// assigned during the creation process and is intended to remain unchanged
    /// throughout the lifespan of the object.
    /// </remarks>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated.
    /// </summary>
    /// <remarks>
    /// This property is typically used to track the most recent modification timestamp
    /// for an entity. It should be updated whenever changes are made to the entity's data.
    /// </remarks>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an entity is marked as deleted.
    /// </summary>
    /// <remarks>
    /// This property is typically used to implement soft delete functionality,
    /// where records are marked as deleted without being permanently removed
    /// from the database or collection.
    /// </remarks>
    public bool IsDeleted { get; set; }
}
