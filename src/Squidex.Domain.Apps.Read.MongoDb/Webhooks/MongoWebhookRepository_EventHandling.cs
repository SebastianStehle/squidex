﻿// ==========================================================================
//  MongoSchemaWebhookRepository_EventHandling.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Domain.Apps.Events.Webhooks;
using Squidex.Domain.Apps.Read.MongoDb.Utils;

namespace Squidex.Domain.Apps.Read.MongoDb.Webhooks
{
    public partial class MongoWebhookRepository
    {
        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "(^webhook-)|(^schema-)"; }
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        protected async Task On(WebhookCreated @event, EnvelopeHeaders headers)
        {
            await EnsureWebooksLoadedAsync();

            await Collection.CreateAsync(@event, headers, w =>
            {
                SimpleMapper.Map(@event, w);

                w.SchemaIds = w.Schemas.Select(x => x.SchemaId).ToList();

                inMemoryWebhooks.GetOrAddNew(w.AppId).RemoveAll(x => x.Id == w.Id);
                inMemoryWebhooks.GetOrAddNew(w.AppId).Add(w);
            });
        }

        protected async Task On(WebhookUpdated @event, EnvelopeHeaders headers)
        {
            await EnsureWebooksLoadedAsync();

            await Collection.UpdateAsync(@event, headers, w =>
            {
                SimpleMapper.Map(@event, w);

                w.SchemaIds = w.Schemas.Select(x => x.SchemaId).ToList();

                inMemoryWebhooks.GetOrAddNew(w.AppId).RemoveAll(x => x.Id == w.Id);
                inMemoryWebhooks.GetOrAddNew(w.AppId).Add(w);
            });
        }

        protected async Task On(SchemaDeleted @event, EnvelopeHeaders headers)
        {
            await EnsureWebooksLoadedAsync();

            await Collection.UpdateAsync(@event, headers, w =>
            {
                w.Schemas.RemoveAll(s => s.SchemaId == @event.SchemaId.Id);

                w.SchemaIds = w.Schemas.Select(x => x.SchemaId).ToList();

                inMemoryWebhooks.GetOrAddNew(w.AppId).RemoveAll(x => x.Id == w.Id);
                inMemoryWebhooks.GetOrAddNew(w.AppId).Add(w);
            });
        }

        protected async Task On(WebhookDeleted @event, EnvelopeHeaders headers)
        {
            await EnsureWebooksLoadedAsync();

            inMemoryWebhooks.GetOrAddNew(@event.AppId.Id).RemoveAll(x => x.Id == @event.WebhookId);

            await Collection.DeleteManyAsync(x => x.Id == @event.WebhookId);
        }
    }
}
