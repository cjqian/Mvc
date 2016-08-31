using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Host;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.ViewComponentTagHelpers
{
    public class ViewComponentTagHelperDescriptorFactory 
    {
        private IViewComponentDescriptorProvider _viewComponentDescriptorProvider;
        private GeneratedViewComponentTagHelperContext _context;

        public ViewComponentTagHelperDescriptorFactory(IViewComponentDescriptorProvider viewComponentDescriptorProvider)
        {
            _viewComponentDescriptorProvider = viewComponentDescriptorProvider;
            _context = new GeneratedViewComponentTagHelperContext();
        }

        public static IViewComponentDescriptorProvider CreateDescriptorProvider(string assemblyName)
        {
            try
            {
                var assembly = Assembly.Load(new AssemblyName(assemblyName));

                var partManager = new ApplicationPartManager();
                partManager.ApplicationParts.Add(new AssemblyPart(assembly));
                partManager.FeatureProviders.Add(new ViewComponentFeatureProvider());

                // TODO: Allow customization by user. 
                var viewComponentDescriptorProvider = new DefaultViewComponentDescriptorProvider(partManager);
                return viewComponentDescriptorProvider;
            }
            catch
            {
                throw new Exception("This assembly is null!");
            }
        }

        public IEnumerable<TagHelperDescriptor> CreateDescriptors()
        {
            var viewComponentDescriptors = _viewComponentDescriptorProvider.GetViewComponents();
            return ResolveDescriptors(viewComponentDescriptors);
        }

        public IEnumerable<TagHelperDescriptor> CreateDescriptors(string assemblyName)
        {
            var viewComponentDescriptors = ResolveViewComponentsInAssembly(assemblyName);
            return ResolveDescriptors(viewComponentDescriptors);
        }

        public IEnumerable<TagHelperDescriptor> ResolveDescriptors(IEnumerable<ViewComponentDescriptor> viewComponentDescriptors)
        {
            var tagHelperDescriptors = new List<TagHelperDescriptor>();
            foreach (var viewComponentDescriptor in viewComponentDescriptors)
            {
                var tagHelperDescriptor = CreateTagHelperDescriptor(viewComponentDescriptor);
                tagHelperDescriptors.Add(tagHelperDescriptor);
            }

            return tagHelperDescriptors;
        }

        private IEnumerable<ViewComponentDescriptor> ResolveViewComponentsInAssembly(string assemblyName)
        {
            // We choose only view components in the given assembly.
            var viewComponents = new List<ViewComponentDescriptor>();
            var providedViewComponents = _viewComponentDescriptorProvider.GetViewComponents();
            foreach (var viewComponent in providedViewComponents)
            {
                var currentAssemblyName = viewComponent.TypeInfo.Assembly.GetName().Name;
                if (currentAssemblyName.Equals(assemblyName))
                {
                    viewComponents.Add(viewComponent);
                }
            }

            return viewComponents;
        }

        private TagHelperDescriptor CreateTagHelperDescriptor(ViewComponentDescriptor viewComponentDescriptor)
        {
            // Set attributes.
            IEnumerable<TagHelperAttributeDescriptor> attributeDescriptors;
            IEnumerable<TagHelperRequiredAttributeDescriptor> requiredAttributeDescriptors;

            if (!TryGetAttributeDescriptors(viewComponentDescriptor,
                out attributeDescriptors,
                out requiredAttributeDescriptors))
            {
                throw new Exception("Something went wrong.");
            }

            var typeName = FormatTypeName(viewComponentDescriptor);

            var tagHelperDescriptor = new TagHelperDescriptor
            {
                TagName = FormatTagName(viewComponentDescriptor),
                TypeName = typeName,
                AssemblyName = viewComponentDescriptor.TypeInfo.Assembly.GetName().Name,
                Attributes = attributeDescriptors,
                RequiredAttributes = requiredAttributeDescriptors,
                TagStructure = TagStructure.NormalOrSelfClosing,
            };

            tagHelperDescriptor.PropertyBag[ViewComponentTagHelperDescriptorConventions.ViewComponentProperty] = viewComponentDescriptor.ShortName;
            tagHelperDescriptor.PropertyBag[ViewComponentTagHelperDescriptorConventions.ViewComponentTagHelperProperty] = typeName;
            
            return tagHelperDescriptor;
        }

        private string FormatTagName(ViewComponentDescriptor viewComponentDescriptor) =>
            $"vc:{TagHelperDescriptorFactory.ToHtmlCase(viewComponentDescriptor.ShortName)}";

        private string FormatTypeName(ViewComponentDescriptor viewComponentDescriptor) =>
            $"__Generated__{viewComponentDescriptor.ShortName}ViewComponentTagHelper";

        // TODO: Add support to HtmlTargetElement, HtmlAttributeName (vc: asdfadf)
        // TODO: Add validation of view component; valid attribute names?
        private bool TryGetAttributeDescriptors(
            ViewComponentDescriptor viewComponentDescriptor,
            out IEnumerable<TagHelperAttributeDescriptor> attributeDescriptors,
            out IEnumerable<TagHelperRequiredAttributeDescriptor> requiredAttributeDescriptors
            )
        {
            var methodParameters = viewComponentDescriptor.MethodInfo.GetParameters();
            var descriptors = new List<TagHelperAttributeDescriptor>();
            var requiredDescriptors = new List<TagHelperRequiredAttributeDescriptor>();
            var requiredValues = new Dictionary<string, object>();

            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameter = methodParameters[i];
                var lowerKebabName = TagHelperDescriptorFactory.ToHtmlCase(parameter.Name);
                var tagHelperAttributeDescriptor = new TagHelperAttributeDescriptor
                {
                    Name = lowerKebabName,
                    PropertyName = parameter.Name,
                    TypeName = parameter.ParameterType.FullName
                };

                var tagHelperType = Type.GetType(tagHelperAttributeDescriptor.TypeName);
                if (tagHelperType.Equals(typeof(string)))
                {
                    tagHelperAttributeDescriptor.IsStringProperty = true;
                }

                descriptors.Add(tagHelperAttributeDescriptor);

                if (!parameter.HasDefaultValue)
                {
                    var requiredAttributeDescriptor = new TagHelperRequiredAttributeDescriptor
                    {
                        Name = lowerKebabName
                    };

                    requiredDescriptors.Add(requiredAttributeDescriptor);
                }
            }

            attributeDescriptors = descriptors;
            requiredAttributeDescriptors = requiredDescriptors;

            return true;
        }
    }
}