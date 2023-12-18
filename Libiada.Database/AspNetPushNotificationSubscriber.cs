using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;


namespace Libiada.Database;

/// <summary>
/// Table for storing data about devices that are subscribers to push notifications.
/// </summary>
public partial class AspNetPushNotificationSubscriber  
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Endpoint { get; set; } = null!;

    public string P256dh { get; set; } = null!;

    public string Auth { get; set; } = null!;

    public virtual AspNetUser AspNetUser { get; set; } = null!;
}
