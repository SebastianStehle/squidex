﻿// ==========================================================================
//  AppContributorRemoved.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Apps
{
    [TypeName("AppContributorRemovedEvent")]
    public class AppContributorRemoved : IEvent
    {
        public string ContributorId { get; set; }
    }
}