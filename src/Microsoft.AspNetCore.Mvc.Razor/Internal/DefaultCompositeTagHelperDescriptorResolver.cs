// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class DefaultCompositeTagHelperDescriptorResolver : CompositeTagHelperDescriptorResolver
    {
        public DefaultCompositeTagHelperDescriptorResolver(
            TagHelperDescriptorResolver resolver1,
            ViewComponentTagHelperDescriptorResolver resolver2)
            : base()
        {
            Resolvers.Add(resolver1);
            Resolvers.Add(resolver2);
        }
    }
}