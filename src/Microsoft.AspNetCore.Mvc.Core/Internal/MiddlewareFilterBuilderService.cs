﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    /// <summary>
    /// Builds a middleware pipeline after receiving the pipeline from a pipeline provider
    /// </summary>
    public class MiddlewareFilterBuilderService
    {
        private readonly ConcurrentDictionary<Type, Lazy<RequestDelegate>> _pipelinesCache
            = new ConcurrentDictionary<Type, Lazy<RequestDelegate>>();
        private readonly MiddlewareFilterConfigurationProvider _configurationProvider;

        public IApplicationBuilder ApplicationBuilder { get; set; }

        public MiddlewareFilterBuilderService(MiddlewareFilterConfigurationProvider middlewareFilterConfigurationProvider)
        {
            _configurationProvider = middlewareFilterConfigurationProvider;
        }

        public RequestDelegate GetPipeline(Type middlewarePipelineProviderType)
        {
            // Build the pipeline only once. This is similar to how middlewares registered in Startup are constructed.

            var requestDelegate = _pipelinesCache.GetOrAdd(
                middlewarePipelineProviderType,
                key => new Lazy<RequestDelegate>(() => BuildPipeline(key)));

            return requestDelegate.Value;
        }

        private RequestDelegate BuildPipeline(Type middlewarePipelineProviderType)
        {
            var nestedAppBuilder = ApplicationBuilder.New();

            // Get the 'Configure' method from the user provided type.
            var configureDelegate = _configurationProvider.CreateConfigureDelegate(middlewarePipelineProviderType);
            configureDelegate(nestedAppBuilder);

            // The middleware resource filter, after receiving the request executes the user configured middleware
            // pipeline. Since we want execution of the request to continue to later MVC layers (resource filters
            // or model binding), add a middleware at the end of the user provided pipeline which make sure to continue
            // this flow.
            // Example:
            // middleware filter -> user-middleware1 -> user-middleware2 -> end-middleware -> resouce filters or model binding
            nestedAppBuilder.Run(async (httpContext) =>
            {
                var feature = httpContext.Features.Get<IMiddlewareFilterFeature>();
                var resourceExecutionDelegate = feature.ResourceExecutionDelegate;

                var resourceExecutedContext = await resourceExecutionDelegate();

                // Ideally we want the experience of a middleware pipeline to behave the same as if it was registered,
                // in Startup. In this scenario an exception thrown in a middelware later in the pipeline gets propagated
                // back to earlier middleware.
                // So check if a resource filter threw an exception and propagate back that exception to the
                // middleware pipeline.
                if (resourceExecutedContext.Exception != null)
                {
                    throw resourceExecutedContext.Exception;
                }
            });

            return nestedAppBuilder.Build();
        }
    }
}
